using System.Windows;
using TransparentTwitchChatWPF.Twitch;
using TransparentTwitchChatWPF.View.Settings;

internal static class Program
{
    [STAThread]
    private static int Main()
    {
        try
        {
            var auth = new FakeAuthService();
            var page = new ConnectionSettingsPage(auth);
            Check(auth.SubscriberCount == 0, "An unloaded page should not subscribe.");
            for (int visit = 0; visit < 3; visit++)
            {
                page.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
                Check(auth.SubscriberCount == 1, "Returning to Connections must subscribe.");
                page.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
                Check(auth.SubscriberCount == 1, "Repeated Loaded events must not duplicate callbacks.");
                page.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
                Check(auth.SubscriberCount == 0, "Leaving Connections must unsubscribe.");
            }
            Console.WriteLine("Connection lifecycle regression checks passed.");
            return 0;
        }
        catch (Exception e) { Console.Error.WriteLine(e.Message); return 1; }
    }

    private static void Check(bool passed, string message)
    {
        if (!passed) throw new Exception(message);
    }
}

internal sealed class FakeAuthService : ITwitchAuthService
{
    private EventHandler<string> handlers;
    public int SubscriberCount => handlers?.GetInvocationList().Length ?? 0;
    public event EventHandler<string> AccessTokenReceived
    {
        add => handlers += value;
        remove => handlers -= value;
    }
    public Task ConnectAsync() => Task.CompletedTask;
}
