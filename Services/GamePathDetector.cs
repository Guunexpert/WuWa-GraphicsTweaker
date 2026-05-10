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
        @"F:\Steam\steamapps\common\Wuthering Waves",
        @"F:\SteamLibrary\steamapps\common\Wuthering Waves",
    ];

    private static readonly string[] CommonKuroPaths =
    [
        @"C:\Wuthering Waves",
        @"C:\Program Files\Wuthering Waves",
        @"C:\Program Files (x86)\Wuthering Waves",
        @"C:\Games\Wuthering Waves",
        @"D:\Wuthering Waves",
        @"D:\Games\Wuthering Waves",
        @"E:\Wuthering Waves",
        @"E:\Games\Wuthering Waves",
        @"F:\Wuthering Waves",
        @"F:\Games\Wuthering Waves",
        @"G:\Wuthering Waves",
        @"G:\Games\Wuthering Waves",
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
        validatedPath = launcher == LauncherType.Steam
            ? TryDetectSteam() ?? TryFromSteamLibrary() ?? TryFromCommonPaths(CommonSteamPaths)
            : TryDetectKuro() ?? TryFromCommonPaths(CommonKuroPaths);

        if (validatedPath == null && IsValid(gamePath))
            validatedPath = gamePath;
    }

    public bool IsValid(string path) => IsValidGamePath(path);

    private string? TryDetectSteam()
    {
        return TryFromRegistry() ?? TryFromSteamLibrary();
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
        catch { }
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

            foreach (var line in File.ReadAllLines(vdfPath))
            {
                if (!line.Contains("\"path\"")) continue;
                var parts = line.Trim().Split('"');
                if (parts.Length < 4) continue;

                var libraryPath = parts[3].Replace(@"\\", @"\");
                var gamePath = Path.Combine(libraryPath, "steamapps", "common", "Wuthering Waves");
                if (IsValidGamePath(gamePath)) return gamePath;
            }
        }
        catch { }
        return null;
    }

    private string? TryDetectKuro()
    {
        return TryFromKuroRegistry() ?? TryFromCommonPaths(CommonKuroPaths);
    }

    private string? TryFromKuroRegistry()
    {
        try
        {
            string[] kuroKeys =
            [
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Wuthering Waves",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Wuthering Waves",
                @"SOFTWARE\Kuro Games\Wuthering Waves",
                @"SOFTWARE\WOW6432Node\Kuro Games\Wuthering Waves",
            ];

            foreach (var keyPath in kuroKeys)
            {
                using var key = Registry.LocalMachine.OpenSubKey(keyPath);
                if (key == null) continue;

                var location = key.GetValue("InstallPath") as string
                            ?? key.GetValue("InstallLocation") as string
                            ?? key.GetValue("DisplayIcon") as string
                            ?? key.GetValue("UninstallString") as string;

                if (string.IsNullOrEmpty(location)) continue;

                location = location.Trim('"');

                if (location.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    location = Path.GetDirectoryName(location) ?? location;

                if (IsValidGamePath(location)) return location;

                var parent = Path.GetDirectoryName(location);
                if (parent != null && IsValidGamePath(parent)) return parent;
            }
        }
        catch { }
        return null;
    }


    private string? TryFromCommonPaths(string[] paths)
    {
        return paths.FirstOrDefault(IsValidGamePath);
    }

    private bool IsValidGamePath(string path)
    {
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) return false;

        return File.Exists(Path.Combine(path, "Wuthering Waves Game", "Client", "Binaries", "Win64", "Client-Win64-Shipping.exe"))
            || File.Exists(Path.Combine(path, "Client", "Binaries", "Win64", "Client-Win64-Shipping.exe"));
    }

    private bool IsValidGamePath(string path, LauncherType _) => IsValidGamePath(path);
}
