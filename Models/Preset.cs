using PhoebeEditor.Models;

namespace PhoebeEditor.Models;

public class Preset
{
    public string Name { get; set; } = "New Preset";
    public Dictionary<string, string> EngineIniSettings { get; set; } = new();
    public Dictionary<string, string> DeviceProfilesSettings { get; set; } = new();
    public List<string> LaunchFlags { get; set; } = new();
    public LauncherType LauncherType { get; set; } = LauncherType.Steam;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}