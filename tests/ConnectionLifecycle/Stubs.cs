using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace TransparentTwitchChatWPF
{
    internal static class App
    {
        public static TestSettings Settings { get; } = new();
    }
    internal sealed class TestSettings
    {
        public TestGeneralSettings GeneralSettings { get; } = new();
    }
    internal sealed class TestGeneralSettings
    {
        public string ChannelID { get; set; } = "";
        public string OAuthToken { get; set; } = "";
        public bool RedemptionsEnabled { get; set; }
    }
}
namespace TransparentTwitchChatWPF.Helpers
{
    internal static class ImageHelpers
    {
        public static BitmapImage LoadFromUrl(string url) => null;
    }
}
namespace TransparentTwitchChatWPF.View.Settings
{
    public partial class ConnectionSettingsPage
    {
        private readonly Button btConnect = new();
        private readonly Label lblTwitch = new();
        private readonly Label lblTwitchStatus = new();
        private readonly Image imgTwitch = new();
        private void InitializeComponent() { }
    }
}
