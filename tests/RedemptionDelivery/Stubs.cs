using Jot;

namespace TransparentTwitchChatWPF
{
    internal static class App
    {
        public static AppSettings Settings { get; } = new();
    }
    internal static class AppInfo { public static bool IsPortable => true; }
    internal static class KapChat { public static List<string> Themes { get; } = new() { "dark" }; }
}
namespace TransparentTwitchChatWPF.Helpers
{
    internal static class SettingsMigrator { public static GeneralSettings AttemptMigration(Tracker tracker) => null; }
    internal static class OverlayPathHelper { public static string GetNativeChatHostname() => "nativechat.overlay"; }
}
namespace TransparentTwitchChatWPF.Properties { internal class NamespaceMarker { } }
namespace Microsoft.Web.WebView2.Core
{
    public class CoreWebView2
    {
        public string Source { get; set; }
        public List<string> Messages { get; } = new();
        public List<string> Scripts { get; } = new();
        public void PostWebMessageAsJson(string message) => Messages.Add(message);
        public Task<string> ExecuteScriptAsync(string script) { Scripts.Add(script); return Task.FromResult("null"); }
    }
}
