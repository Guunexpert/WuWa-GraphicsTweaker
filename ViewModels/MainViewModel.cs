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
        var detected = _pathDetector.Detect();
        if (detected != null)
        {
            GamePath = detected;
            IsGameFound = true;
            StatusMessage = "✓ Game found";
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
        var root = Path.GetFullPath(Path.Combine(exeDir, @"..\..\..\"));

        GamePath = root;
        IsGameFound = true;
        StatusMessage = "✓ Path is set manually";
        StatusColor = "#3FB950";
        Tweaks.LoadFromGame(GamePath);
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
        _launcher.Launch(GamePath, extraFlags);
        StatusMessage = "Game launched!";
    }

    [RelayCommand]
    private void SavePreset(string presetName)
    {
        var preset = new Preset
        {
            Name = presetName,
            EngineIniSettings = new Dictionary<string, string>(Tweaks.EngineSettings),
            LaunchFlags = [SelectedDxApi, .. CustomFlagsInput.Split(' ', StringSplitOptions.RemoveEmptyEntries)]
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
    }

    [RelayCommand]
    private void DeletePreset(Preset preset)
    {
        _presetService.Delete(preset);
        LoadPresets();
    }

    private void LoadPresets() => Presets = _presetService.LoadAll();
}