using System.Diagnostics;
using System.IO;
using PhoebeEditor.Models;

namespace PhoebeEditor.Services;

public class GameLauncherService
{
    private const string SteamAppId = "3513350";

    public void Launch(string gamePath, LauncherType launcher, IEnumerable<string> extraFlags)
    {
        var flags = BuildFlags(extraFlags);

        if (launcher == LauncherType.Steam)
            LaunchViaSteam(flags);
        else
            LaunchViaKuro(gamePath, flags);
    }

    private void LaunchViaSteam(string flags)
    {
        var uri = $"steam://run/{SteamAppId}//{flags}/";
        Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
    }

    private void LaunchViaKuro(string gamePath, string flags)
    {
        var exePath = ResolveExePath(gamePath);
        if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
            throw new FileNotFoundException("Game executable not found. Please verify the game path.", exePath ?? "Unknown");

        Process.Start(new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = flags,
            WorkingDirectory = Path.GetDirectoryName(exePath),
            UseShellExecute = false
        });
    }

    private static string? ResolveExePath(string gamePath)
    {
        var candidates = new[]
        {
            Path.Combine(gamePath, "Wuthering Waves Game", "Client", "Binaries", "Win64", "Client-Win64-Shipping.exe"),
            Path.Combine(gamePath, "Client", "Binaries", "Win64", "Client-Win64-Shipping.exe")
        };
        return candidates.FirstOrDefault(File.Exists);
    }

    private string BuildFlags(IEnumerable<string> extraFlags)
    {
        var flags = new List<string>
        {
            "-EngineIni=Engine.ini"
        };

        foreach (var flag in extraFlags)
        {
            var trimmed = flag.Trim();
            if (!string.IsNullOrEmpty(trimmed) && !flags.Contains(trimmed))
                flags.Add(trimmed);
        }

        return string.Join(" ", flags);
    }
}