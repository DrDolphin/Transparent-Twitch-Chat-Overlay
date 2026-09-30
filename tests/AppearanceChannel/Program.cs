using TransparentTwitchChatWPF;

try
{
    var settings = new AppSettings();
    settings.GeneralSettings.Username = "redemption-account";
    settings.jChatSettings.Channel = "new-chat-channel";
    settings.UpdateJChatConfig("{\"channel\":\"stale-channel\",\"font\":\"2\",\"size\":3}", preserveChannel: true);
    Check(settings.jChatSettings.Channel == "new-chat-channel", "Appearance overwrote the newly selected chat channel.");
    Check(settings.jChatSettings.Font == "2" && settings.jChatSettings.Size == 3, "Appearance settings were not applied.");
    Check(settings.GeneralSettings.Username == "redemption-account", "The connected redemption account changed.");
    settings.UpdateJChatConfig("{\"channel\":\"intentional-channel\"}");
    Check(settings.jChatSettings.Channel == "intentional-channel", "Explicit full configuration updates must still change the channel.");
    settings.UpdateJChatConfig("{broken", preserveChannel: true);
    Check(settings.jChatSettings.Channel == "intentional-channel", "Malformed configuration changed the channel.");
    Console.WriteLine("Appearance channel regression checks passed.");
    return 0;
}
catch (Exception e) { Console.Error.WriteLine(e.Message); return 1; }

static void Check(bool passed, string message)
{
    if (!passed) throw new Exception(message);
}
