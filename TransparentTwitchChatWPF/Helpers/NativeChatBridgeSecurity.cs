namespace TransparentTwitchChatWPF.Helpers;

internal static class NativeChatBridgeSecurity
{
    public static bool IsTrustedOverlaySource(string source)
    {
        return Uri.TryCreate(source, UriKind.Absolute, out var uri)
            && uri.Scheme == Uri.UriSchemeHttps
            && string.Equals(uri.Host, OverlayPathHelper.GetNativeChatHostname(), StringComparison.OrdinalIgnoreCase)
            && uri.IsDefaultPort
            && string.IsNullOrEmpty(uri.UserInfo)
            && uri.AbsolutePath == "/v2/index.html";
    }

    public static bool IsRepairSource(string source, bool isShowingRepairPrompt)
    {
        return isShowingRepairPrompt && source == "about:blank";
    }
}
