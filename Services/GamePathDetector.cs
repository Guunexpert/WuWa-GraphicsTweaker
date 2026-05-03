using Microsoft.Win32;
using System.IO;
namespace PhoebeEditor.Services;

public class GamePathDetector
{
    // Steam App ID untuk Wuthering Waves
    private const string SteamAppId = "3513350";

    private static readonly string[] CommonSteamPaths =
    [
        @"C:\Program Files (x86)\Steam\steamapps\common\Wuthering Waves",
        @"C:\Program Files\Steam\steamapps\common\Wuthering Waves",
        @"D:\Steam\steamapps\common\Wuthering Waves",
        @"D:\SteamLibrary\steamapps\common\Wuthering Waves",
        @"E:\Steam\steamapps\common\Wuthering Waves",
        @"E:\SteamLibrary\steamapps\common\Wuthering Waves",
    ];

    public string? Detect()
    {
        // 1. Coba cari via Steam registry
        var fromRegistry = TryFromRegistry();
        if (fromRegistry != null) return fromRegistry;

        // 2. Coba cari via Steam library folders
        var fromLibrary = TryFromSteamLibrary();
        if (fromLibrary != null) return fromLibrary;

        // 3. Fallback ke common paths
        return TryFromCommonPaths();
    }

    private string? TryFromRegistry()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(
                $@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App {SteamAppId}");

            var installLocation = key?.GetValue("InstallLocation") as string;
            if (!string.IsNullOrEmpty(installLocation) && IsValidGamePath(installLocation))
                return installLocation;
        }
        catch { /* ignored */ }

        return null;
    }

    private string? TryFromSteamLibrary()
    {
        try
        {
            // Cari instalasi Steam dari registry
            using var steamKey = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
            var steamPath = steamKey?.GetValue("SteamPath") as string;
            if (string.IsNullOrEmpty(steamPath)) return null;

            var vdfPath = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
            if (!File.Exists(vdfPath)) return null;

            // Parse libraryfolders.vdf untuk dapet semua Steam library paths
            var lines = File.ReadAllLines(vdfPath);
            foreach (var line in lines)
            {
                if (!line.Contains("\"path\"")) continue;

                var parts = line.Trim().Split('"');
                if (parts.Length < 4) continue;

                var libraryPath = parts[3].Replace(@"\\", @"\");
                var gamePath = Path.Combine(libraryPath, "steamapps", "common", "Wuthering Waves");

                if (IsValidGamePath(gamePath))
                    return gamePath;
            }
        }
        catch { /* ignored */ }

        return null;
    }

    private string? TryFromCommonPaths()
    {
        return CommonSteamPaths.FirstOrDefault(IsValidGamePath);
    }

    private bool IsValidGamePath(string path)
    {
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) return false;

        var exePath1 = Path.Combine(path, "Client", "Binaries", "Win64", "Client-Win64-Shipping.exe");
        var exePath2 = Path.Combine(path, "Wuthering Waves Game", "Client", "Binaries", "Win64", "Client-Win64-Shipping.exe");

        return File.Exists(exePath1) || File.Exists(exePath2);
    }
}
