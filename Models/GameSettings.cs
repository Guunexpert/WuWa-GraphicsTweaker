namespace PhoebeEditor.Models;

using System.IO;

public class GameSettings
{
    public string GameRootPath { get; set; } = string.Empty;

    private bool UsesNewStructure => File.Exists(
        Path.Combine(GameRootPath, "Client", "Binaries", "Win64", "Client-Win64-Shipping.exe"));

    private string ClientBasePath => UsesNewStructure
        ? Path.Combine(GameRootPath, "Client")
        : Path.Combine(GameRootPath, "Wuthering Waves Game", "Client");

    public string EngineIniPath => Path.Combine(ClientBasePath, "Binaries", "Win64", "Engine.ini");

    public string DeviceProfilesPath => Path.Combine(ClientBasePath, "Saved", "Config", "WindowsNoEditor", "DeviceProfiles.ini");

    public string GameExePath => Path.Combine(ClientBasePath, "Binaries", "Win64", "Client-Win64-Shipping.exe");

    public bool IsValid => !string.IsNullOrEmpty(GameRootPath) && File.Exists(GameExePath);
}