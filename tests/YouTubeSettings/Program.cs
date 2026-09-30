using System.Text.Json;
using Microsoft.Web.WebView2.Core;
using TransparentTwitchChatWPF;
using TransparentTwitchChatWPF.ChatProviders;

try
{
    App.Settings.jChatSettings.Channel = "twitch-channel";
    App.Settings.jChatSettings.Yt = "legacy-youtube-channel";
    var webview = new CoreWebView2 { Source = "https://nativechat.overlay/v2/index.html" };
    await new NativeChatProvider().ConfigureAsync(webview);
    using var config = JsonDocument.Parse(webview.Messages[0]);
    var payload = config.RootElement.GetProperty("payload");
    if (payload.GetProperty("yt").GetString() != "" || App.Settings.jChatSettings.Yt != "")
        throw new Exception("Legacy YouTube values should be cleared before configuring the overlay.");
    if (payload.GetProperty("channel").GetString() != "twitch-channel")
        throw new Exception("The Twitch channel changed when clearing unsupported YouTube settings.");
    Console.WriteLine("YouTube configuration regression checks passed.");
    return 0;
}
catch (Exception e) { Console.Error.WriteLine(e.Message); return 1; }
