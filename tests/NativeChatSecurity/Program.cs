using Microsoft.Web.WebView2.Core;
using TransparentTwitchChatWPF.ChatProviders;
using TransparentTwitchChatWPF.Helpers;

try
{
    void Check(bool passed, string message) { if (!passed) throw new Exception(message); }
    foreach (var source in new[] { "https://nativechat.overlay/v2/index.html", "https://nativechat.overlay:443/v2/index.html?v=123" })
    {
        var webview = new CoreWebView2 { Source = source };
        await new NativeChatProvider().ConfigureAsync(webview);
        Check(webview.Messages.Count == 2 && webview.Messages[1].Contains("synthetic-test-token"), "Trusted overlay did not receive credentials");
    }
    foreach (var source in new[] { "https://external.example/", "https://nativechat.overlay.external.example/v2/index.html",
        "http://nativechat.overlay/v2/index.html", "https://nativechat.overlay:444/v2/index.html",
        "https://nativechat.overlay/index.html", "https://user@nativechat.overlay/v2/index.html", "about:blank", "file:///v2/index.html", "", null })
    {
        var webview = new CoreWebView2 { Source = source };
        await new NativeChatProvider().ConfigureAsync(webview);
        Check(webview.Messages.Count == 0, $"Credentials exposed to {source}");
    }
    Check(NativeChatBridgeSecurity.IsRepairSource("about:blank", true), "Repair page denied");
    Check(!NativeChatBridgeSecurity.IsRepairSource("about:blank", false), "Unexpected blank page trusted");
    Check(!NativeChatBridgeSecurity.IsRepairSource("https://external.example/", true), "External repair source trusted");
    Console.WriteLine("NativeChat security regression checks passed.");
    return 0;
}
catch (Exception e) { Console.Error.WriteLine(e.Message); return 1; }
