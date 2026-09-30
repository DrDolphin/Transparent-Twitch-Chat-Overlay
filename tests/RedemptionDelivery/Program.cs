using System.Text.Json;
using Microsoft.Web.WebView2.Core;
using TransparentTwitchChatWPF.ChatProviders;

try
{
    const string message = "Reward \"quoted\"\n<script> \\ ";
    var webview = new CoreWebView2 { Source = "https://nativechat.overlay/v2/index.html?v=1" };
    IChatProvider provider = new NativeChatProvider();
    await provider.PushChatMessageAsync(webview, message, "Viewer", "#a1b3c4");
    using var payload = JsonDocument.Parse(webview.Messages.Single());
    Check(payload.RootElement.GetProperty("type").GetString() == "chatMessage", "Wrong host message type.");
    Check(payload.RootElement.GetProperty("payload").GetProperty("message").GetString() == message, "Redemption content changed during serialization.");
    webview.Source = "https://external.example/";
    await provider.PushChatMessageAsync(webview, message, "Viewer", "");
    Check(webview.Messages.Count == 1, "Redemption was sent to an unrelated page.");
    provider = new KapChatProvider();
    webview.Source = "https://nightdev.com/hosted/obschat/?channel=test-channel";
    await provider.PushChatMessageAsync(webview, message, "Viewer", "#a1b3c4");
    Check(webview.Scripts.Count == 1 && webview.Scripts[0].StartsWith("Chat.insert("), "KapChat did not receive the redemption.");
    Check(webview.Scripts[0].Contains(JsonSerializer.Serialize(message)), "KapChat content was not JSON escaped.");
    webview.Source = "https://external.example/";
    await provider.PushChatMessageAsync(webview, message, "Viewer", "");
    Check(webview.Scripts.Count == 1, "KapChat delivery executed on an unrelated page.");
    Console.WriteLine("Redemption provider regression checks passed.");
    return 0;
}
catch (Exception e) { Console.Error.WriteLine(e.Message); return 1; }

static void Check(bool passed, string message) { if (!passed) throw new Exception(message); }
