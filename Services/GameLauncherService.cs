using System.Diagnostics;
using System.IO;
namespace PhoebeEditor.Services;

public class GameLauncherService
{
    private const string SteamAppId = "3513350";

    public void Launch(string _, IEnumerable<string> extraFlags)
    {
        var flags = BuildFlags(extraFlags);
        LaunchViaSteam(flags);
    }

    private void LaunchViaSteam(string flags)
    {
        // steam://run/{appid}//{launch options}
        var uri = $"steam://run/{SteamAppId}//{flags}/";
        Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
    }

    private string BuildFlags(IEnumerable<string> extraFlags)
    {
        var flags = new List<string>
        {
            // arguments for wuthering waves steam
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
