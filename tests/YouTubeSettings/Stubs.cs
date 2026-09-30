namespace TransparentTwitchChatWPF
{
    public static class App { public static TestSettings Settings { get; } = new(); }
    public class TestSettings
    {
        public TestGeneralSettings GeneralSettings { get; } = new();
        public TestChatSettings jChatSettings { get; } = new();
        public void SyncJChatSettings() { }
    }
    public class TestGeneralSettings { public string Username { get; set; } = "testchannel"; public string OAuthToken { get; set; } = "synthetic-test-token"; }
    public class TestChatSettings { public string Channel { get; set; } public string Yt { get; set; } }
}
namespace TransparentTwitchChatWPF.Helpers
{
    public static class OverlayPathHelper { public static string GetNativeChatHostname() => "nativechat.overlay"; }
}
namespace TransparentTwitchChatWPF.Properties { internal class NamespaceMarker { } }
namespace Microsoft.Web.WebView2.Core
{
    public class CoreWebView2
    {
        public string Source { get; set; }
        public List<string> Messages { get; } = new();
        public void PostWebMessageAsJson(string message) => Messages.Add(message);
    }
}
