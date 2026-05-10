using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhoebeEditor.Models;
using PhoebeEditor.Services;
using System.IO;

namespace PhoebeEditor.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly GamePathDetector _pathDetector = new();
    private readonly GameLauncherService _launcher = new();
    private readonly IniFileService _iniService = new();
    private readonly PresetService _presetService = new();

    [ObservableProperty] private string _gamePath = string.Empty;
    [ObservableProperty] private bool _isGameFound = false;
    [ObservableProperty] private string _statusMessage = "Searching Game Installation...";
    [ObservableProperty] private string _statusColor = "#8B949E";
    [ObservableProperty] private string _selectedDxApi = "-dx12";
    [ObservableProperty] private string _customFlagsInput = string.Empty;
    [ObservableProperty] private List<Preset> _presets = [];
    [ObservableProperty] private Preset? _selectedPreset;
    [ObservableProperty] private TweaksViewModel _tweaks;
    [ObservableProperty] private LauncherType _launcherType = LauncherType.Steam;

    public List<string> DxApiOptions { get; } = ["-dx12", "-dx11", "-vulkan"];

    public MainViewModel()
    {
        _tweaks = new TweaksViewModel(_iniService);
        AutoDetectGame();
        LoadPresets();
    }

    [RelayCommand]
    private void AutoDetectGame()
    {
        var (path, detectedLauncher) = _pathDetector.Detect();
        if (path != null)
        {
            GamePath = path;
            IsGameFound = true;
            LauncherType = detectedLauncher;
            StatusMessage = $"✓ Game found ({LauncherName(detectedLauncher)})";
            StatusColor = "#3FB950";
            Tweaks.LoadFromGame(GamePath);
        }
        else
        {
            IsGameFound = false;
            StatusMessage = "Game not found. Please select path manual...";
            StatusColor = "#F85149";
        }
    }

    [RelayCommand]
    private void BrowseGamePath()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Select Wuthering Waves executable",
            Filter = "Executable|Client-Win64-Shipping.exe",
            FileName = "Client-Win64-Shipping.exe"
        };

        if (dialog.ShowDialog() != true) return;

        var exeDir = Path.GetDirectoryName(dialog.FileName)!;

        var root3 = Path.GetFullPath(Path.Combine(exeDir, @"..\..\..\"));
        var root4 = Path.GetFullPath(Path.Combine(exeDir, @"..\..\..\..\"));

        GamePath = _pathDetector.IsValid(root3) ? root3
                 : _pathDetector.IsValid(root4) ? root4
                 : root3;

        IsGameFound = true;
        StatusMessage = $"✓ Path is set manually ({LauncherName(LauncherType)})";
        StatusColor = "#3FB950";
        Tweaks.LoadFromGame(GamePath);
    }

    partial void OnLauncherTypeChanged(LauncherType value)
    {
        if (IsGameFound)
        {
            _pathDetector.SetLauncherType(GamePath, value, out var validatedPath);
            if (validatedPath != null) GamePath = validatedPath;
            StatusMessage = $"✓ Game found ({LauncherName(value)})";
            StatusColor = "#3FB950";
        }
    }

    [RelayCommand]
    private void LaunchGame()
    {
        if (!IsGameFound) return;

        var extraFlags = CustomFlagsInput
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Concat(string.IsNullOrEmpty(SelectedDxApi) ? [] : new[] { SelectedDxApi })
            .Distinct();

        Tweaks.ApplyToGame(GamePath);
        _launcher.Launch(GamePath, LauncherType, extraFlags);
        StatusMessage = $"Game launched! ({LauncherName(LauncherType)})";
    }

    [RelayCommand]
    private void SavePreset(string presetName)
    {
        var preset = new Preset
        {
            Name = presetName,
            EngineIniSettings = new Dictionary<string, string>(Tweaks.EngineSettings),
            LaunchFlags = [SelectedDxApi, .. CustomFlagsInput.Split(' ', StringSplitOptions.RemoveEmptyEntries)],
            LauncherType = LauncherType
        };

        _presetService.Save(preset);
        LoadPresets();
    }

    [RelayCommand]
    private void LoadPreset(Preset preset)
    {
        SelectedPreset = preset;
        Tweaks.LoadFromPreset(preset);

        var dx = preset.LaunchFlags.FirstOrDefault(f => f.StartsWith("-dx") || f == "-vulkan");
        if (dx != null) SelectedDxApi = dx;

        LauncherType = preset.LauncherType;
    }

    [RelayCommand]
    private void DeletePreset(Preset preset)
    {
        _presetService.Delete(preset);
        LoadPresets();
    }

    private void LoadPresets() => Presets = _presetService.LoadAll();

    private static string LauncherName(LauncherType t) =>
        t == LauncherType.Steam ? "Steam" : "Kuro Launcher";
}
