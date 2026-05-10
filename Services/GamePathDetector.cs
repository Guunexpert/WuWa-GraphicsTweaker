using Microsoft.Win32;
using System.IO;
using PhoebeEditor.Models;

namespace PhoebeEditor.Services;

public class GamePathDetector
{
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

    private static readonly string[] CommonKuroPaths =
    [
        @"C:\Wuthering Waves",
        @"C:\Program Files\Wuthering Waves",
        @"D:\Wuthering Waves",
        @"E:\Wuthering Waves",
        @"F:\Wuthering Waves",
    ];

    public (string? path, LauncherType launcher) Detect()
    {
        var steamPath = TryDetectSteam();
        if (steamPath != null) return (steamPath, LauncherType.Steam);

        var kuroPath = TryDetectKuro();
        if (kuroPath != null) return (kuroPath, LauncherType.Kuro);

        return (null, LauncherType.Steam);
    }

    public void SetLauncherType(string gamePath, LauncherType launcher, out string? validatedPath)
    {
        if (launcher == LauncherType.Steam)
        {
            validatedPath = TryDetectSteam() ?? TryFromSteamLibrary() ?? TryFromCommonPaths(CommonSteamPaths);
        }
        else
        {
            validatedPath = TryDetectKuro() ?? TryFromCommonPaths(CommonKuroPaths);
        }

        if (validatedPath == null && IsValidGamePath(gamePath, launcher))
        {
            validatedPath = gamePath;
        }
    }

    private string? TryDetectSteam()
    {
        var fromRegistry = TryFromRegistry();
        if (fromRegistry != null) return fromRegistry;
        return TryFromSteamLibrary();
    }

    private string? TryDetectKuro()
    {
        return TryFromCommonPaths(CommonKuroPaths);
    }

    private string? TryFromRegistry()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(
                $@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App {SteamAppId}");

            var installLocation = key?.GetValue("InstallLocation") as string;
            if (!string.IsNullOrEmpty(installLocation) && IsValidGamePath(installLocation, LauncherType.Steam))
                return installLocation;
        }
        catch { /* ignored */ }

        return null;
    }

    private string? TryFromSteamLibrary()
    {
        try
        {
            using var steamKey = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
            var steamPath = steamKey?.GetValue("SteamPath") as string;
            if (string.IsNullOrEmpty(steamPath)) return null;

            var vdfPath = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
            if (!File.Exists(vdfPath)) return null;

            var lines = File.ReadAllLines(vdfPath);
            foreach (var line in lines)
            {
                if (!line.Contains("\"path\"")) continue;

                var parts = line.Trim().Split('"');
                if (parts.Length < 4) continue;

                var libraryPath = parts[3].Replace(@"\\", @"\");
                var gamePath = Path.Combine(libraryPath, "steamapps", "common", "Wuthering Waves");

                if (IsValidGamePath(gamePath, LauncherType.Steam))
                    return gamePath;
            }
        }
        catch { /* ignored */ }

        return null;
    }

    private string? TryFromCommonPaths(string[] paths)
    {
        return paths.FirstOrDefault(p => IsValidGamePath(p, LauncherType.Steam))
            ?? paths.FirstOrDefault(p => IsValidGamePath(p, LauncherType.Kuro));
    }

    private bool IsValidGamePath(string path, LauncherType launcher)
    {
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) return false;

        var gameFolder = launcher == LauncherType.Steam
            ? Path.Combine(path, "Wuthering Waves Game", "Client", "Binaries", "Win64", "Client-Win64-Shipping.exe")
            : Path.Combine(path, "Wuthering Waves Game", "Client", "Binaries", "Win64", "Client-Win64-Shipping.exe");

        if (File.Exists(gameFolder)) return true;

        var altPath = Path.Combine(path, "Client", "Binaries", "Win64", "Client-Win64-Shipping.exe");
        return File.Exists(altPath);
    }
}
