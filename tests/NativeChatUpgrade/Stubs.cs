namespace TransparentTwitchChatWPF
{
    internal static class App
    {
        public static TestSettings Settings { get; } = new();
    }

    internal sealed class TestSettings
    {
        public TestGeneralSettings GeneralSettings { get; } = new();
        public int PersistCount { get; private set; }
        public void Persist() => PersistCount++;
    }

    internal sealed class TestGeneralSettings
    {
        public string NativeChatVersion { get; set; } = "1.0.4";
    }
}

namespace TransparentTwitchChatWPF.Helpers
{
    internal static class AppInfo
    {
        public const string Version = "1.1.0";
    }

    internal static class OverlayPathHelper
    {
        public static string TestPath { get; set; } = "";
        public static string GetNativeChatPath() => TestPath;
    }
}
