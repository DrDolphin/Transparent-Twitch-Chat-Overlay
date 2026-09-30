using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging.Abstractions;
using TransparentTwitchChatWPF;
using TransparentTwitchChatWPF.Helpers;

internal static class Program
{
    private const string ManifestName = "nativechat-manifest.json";
    private const string ResourceName = "TransparentTwitchChatWPF.Resources.native-chat.zip";
    private const string OldScript = "// legacy NativeChat 1.0.4 asset\n";

    private static int Main()
    {
        // All extraction, backup and staging paths are confined to this unique test directory.
        string root = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "nativechat-upgrade-" + Guid.NewGuid().ToString("N")));
        try
        {
            Directory.CreateDirectory(root);
            Run(root);
            Console.WriteLine("NativeChat upgrade regression passed: valid 1.0.4 assets replaced, backup retained, repeat startup unchanged.");
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error);
            return 1;
        }
        finally
        {
            string allowed = Path.GetFullPath(AppContext.BaseDirectory)
                .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (root.StartsWith(allowed, StringComparison.OrdinalIgnoreCase) && Directory.Exists(root))
            {
                try { Directory.Delete(root, recursive: true); }
                catch (Exception error) { Console.Error.WriteLine("Fixture cleanup failed: " + error.Message); }
            }
        }
    }

    private static void Run(string root)
    {
        Dictionary<string, byte[]> embedded = ReadEmbeddedFiles();
        JsonObject manifest = JsonNode.Parse(embedded[ManifestName])!.AsObject();
        string version = manifest["version"]!.GetValue<string>();
        Check(Version.Parse(version) > new Version(1, 0, 4),
            "Embedded asset version must be newer than the existing 1.0.4 installation.");
        string currentScript = manifest["files"]!.AsArray()
            .Select(file => file!["path"]!.GetValue<string>())
            .Single(path => path.StartsWith("v2.", StringComparison.Ordinal) && path.EndsWith(".js"));
        Check(Encoding.UTF8.GetString(embedded[currentScript]) != OldScript,
            "The embedded bundle must contain the updated client.");

        var legacy = new Dictionary<string, byte[]>(embedded, StringComparer.Ordinal);
        const string legacyScript = "legacy-v2.js";
        legacy.Remove(currentScript);
        legacy[legacyScript] = Encoding.UTF8.GetBytes(OldScript);
        legacy["v2/index.html"] = Encoding.UTF8.GetBytes(
            Encoding.UTF8.GetString(legacy["v2/index.html"]).Replace(currentScript, legacyScript));
        legacy["version.json"] = Encoding.UTF8.GetBytes("{\"version\":\"1.0.4\"}");
        manifest["version"] = "1.0.4";
        // Recompute hashes so the legacy fixture is valid, rather than forcing a corruption repair.
        foreach (JsonNode? fileNode in manifest["files"]!.AsArray())
        {
            JsonObject file = fileNode!.AsObject();
            if (file["path"]!.GetValue<string>() == currentScript) file["path"] = legacyScript;
            byte[] bytes = legacy[file["path"]!.GetValue<string>()];
            file["size"] = bytes.LongLength;
            file["sha256"] = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        }
        legacy[ManifestName] = Encoding.UTF8.GetBytes(manifest.ToJsonString());

        string legacyZip = Path.Combine(root, "legacy.zip");
        using (ZipArchive zip = ZipFile.Open(legacyZip, ZipArchiveMode.Create))
        {
            foreach ((string name, byte[] bytes) in legacy)
            {
                using Stream output = zip.CreateEntry(name).Open();
                output.Write(bytes);
            }
        }

        string overlay = Path.Combine(root, "overlay");
        OverlayPathHelper.TestPath = overlay;
        var manager = new NativeChatFileManager(NullLogger<NativeChatFileManager>.Instance);
        manager.ExtractFromZipFile(legacyZip, overlay);
        Check(File.ReadAllText(Path.Combine(overlay, legacyScript)) == OldScript,
            "The fixture must start with the legacy script.");
        Check(manager.EnsureFilesAreUpToDate(), "Startup must install the newer embedded assets.");
        Check(App.Settings.GeneralSettings.NativeChatVersion == version && App.Settings.PersistCount == 1,
            "Startup must persist the new installed version once.");

        foreach ((string name, byte[] bytes) in embedded)
        {
            string installed = Path.Combine(overlay, name.Replace('/', Path.DirectorySeparatorChar));
            Check(File.Exists(installed) && File.ReadAllBytes(installed).AsSpan().SequenceEqual(bytes),
                "Updated asset was not installed: " + name);
        }
        Check(!File.Exists(Path.Combine(overlay, legacyScript)),
            "The old script must not remain in the activated bundle.");
        Check(File.ReadAllText(Path.Combine(overlay + ".backup", legacyScript)) == OldScript,
            "A valid older installation must be retained as the upgrade backup.");
        Check(!manager.EnsureFilesAreUpToDate() && App.Settings.PersistCount == 1,
            "A second startup must leave the current bundle and settings unchanged.");
    }

    private static Dictionary<string, byte[]> ReadEmbeddedFiles()
    {
        using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException("Build the WPF application before running this regression.");
        using var zip = new ZipArchive(stream, ZipArchiveMode.Read);
        var files = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        foreach (ZipArchiveEntry entry in zip.Entries)
        {
            string name = entry.FullName.Replace('\\', '/');
            if (name.EndsWith('/')) continue;
            using Stream input = entry.Open();
            using var bytes = new MemoryStream();
            input.CopyTo(bytes);
            files.Add(name, bytes.ToArray());
        }
        return files;
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
