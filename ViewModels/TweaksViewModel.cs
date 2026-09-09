using System.Globalization;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhoebeEditor.Models;
using PhoebeEditor.Services;

namespace PhoebeEditor.ViewModels;

public partial class TweaksViewModel : ObservableObject
{
    private readonly IniFileService _iniService;
    private string _currentGamePath = string.Empty;

    // Streaming
    [ObservableProperty] private bool _useNewKuroStreaming = true;

    // Resolution & Upscaling
    [ObservableProperty] private int _screenPercentage = 100;
    [ObservableProperty] private bool _useFsrSecondaryUpscale;
    [ObservableProperty] private double _fsrSharpness = 1.0;

    // Shadows
    [ObservableProperty] private int _shadowQuality = 3;
    [ObservableProperty] private int _shadowMaxCsmResolution = 1024;
    [ObservableProperty] private int _shadowPerObjectResolution = 512;
    [ObservableProperty] private double _shadowRadiusThreshold = 0.02;
    [ObservableProperty] private bool _enableCapsuleShadows = true;
    [ObservableProperty] private bool _enableContactShadows = true;

    // Textures
    [ObservableProperty] private int _textureStreamingPoolSize = 512;
    [ObservableProperty] private int _viewTextureMipBias;
    [ObservableProperty] private int _maxAnisotropy = 8;
    [ObservableProperty] private int _streamingMipBias;

    // LOD
    [ObservableProperty] private double _viewDistanceScale = 1.0;
    [ObservableProperty] private double _foliageLodDistanceScale = 1.0;
    [ObservableProperty] private double _staticMeshLodDistanceScale = 1.0;
    [ObservableProperty] private int _skeletalMeshLodBias;

    // Post Processing
    [ObservableProperty] private bool _enableBloom = true;
    [ObservableProperty] private bool _enableMotionBlur = true;
    [ObservableProperty] private bool _enableAmbientOcclusion = true;
    [ObservableProperty] private bool _enableDepthOfField = true;
    [ObservableProperty] private bool _enableLensFlare = true;
    [ObservableProperty] private bool _enableChromaticAberration = true;
    [ObservableProperty] private bool _enableEyeAdaptation = true;
    [ObservableProperty] private int _tonemapperQuality = 1;

    // Anti-Aliasing
    [ObservableProperty] private int _temporalAaSamples = 8;
    [ObservableProperty] private double _temporalAaCurrentFrameWeight = 0.04;
    [ObservableProperty] private double _temporalAaFilterSize = 1.0;
    [ObservableProperty] private double _temporalAaSharpen;

    // Lighting & Reflections
    [ObservableProperty] private bool _enableSsr = true;
    [ObservableProperty] private int _ssrQuality = 3;
    [ObservableProperty] private bool _enableDistanceFieldAo = true;
    [ObservableProperty] private int _reflectionCaptureResolution = 128;
    [ObservableProperty] private int _lightFunctionQuality = 1;

    // Performance
    [ObservableProperty] private bool _enableVSync;
    [ObservableProperty] private int _frameRateLimit;

    [ObservableProperty] private string _applyStatus = string.Empty;

    public Dictionary<string, string> EngineSettings => BuildEngineSettings();

    public TweaksViewModel(IniFileService iniService)
    {
        _iniService = iniService ?? throw new ArgumentNullException(nameof(iniService));
    }

    private static bool TryGetValue<T>(IReadOnlyDictionary<string, string> settings, string key, out T value)
    {
        if (settings.TryGetValue(key, out var raw))
        {
            if (typeof(T) == typeof(int) && int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intValue))
            {
                value = (T)(object)intValue;
                return true;
            }
            if (typeof(T) == typeof(double) && double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var doubleValue))
            {
                value = (T)(object)doubleValue;
                return true;
            }
            if (typeof(T) == typeof(bool))
            {
                value = (T)(object)(raw != "0");
                return true;
            }
        }
        value = default;
        return false;
    }

    private static string Format(double value, string format = "0.###")
    {
        return value.ToString(format, CultureInfo.InvariantCulture);
    }

    // load it
    public void LoadFromGame(string gamePath)
    {
        if (string.IsNullOrWhiteSpace(gamePath))
            return;

        _currentGamePath = gamePath;
        var gameSettings = new GameSettings { GameRootPath = gamePath };

        if (!File.Exists(gameSettings.EngineIniPath))
        {
            ApplyStatus = "⚠ Engine.ini not found";
            return;
        }

        try
        {
            var settings = _iniService.ReadSection(gameSettings.EngineIniPath, "SystemSettings");
            LoadSettings(settings);
            ApplyStatus = "✓ Engine.ini loaded";
        }
        catch (Exception ex)
        {
            ApplyStatus = $"✕ Failed to load Engine.ini: {ex.Message}";
        }
    }

    public void LoadFromPreset(Preset preset)
    {
        if (preset?.EngineIniSettings == null)
        {
            ApplyStatus = "⚠ Preset contains no settings";
            return;
        }

        LoadSettings(preset.EngineIniSettings);
        ApplyStatus = "✓ Preset loaded";
    }

    private void LoadSettings(IReadOnlyDictionary<string, string> settings)
    {
        UseNewKuroStreaming = TryGetValue(settings, "r.Streaming.UsingNewKuroStreaming", out bool useNewKuro) ? useNewKuro : UseNewKuroStreaming;
        ScreenPercentage = TryGetValue(settings, "r.SecondaryScreenPercentage.GameViewport", out int screen) ? screen : ScreenPercentage;
        UseFsrSecondaryUpscale = TryGetValue(settings, "r.FidelityFX.FSR.SecondaryUpscale", out bool useFsr) ? useFsr : UseFsrSecondaryUpscale;
        FsrSharpness = TryGetValue(settings, "r.FidelityFX.FSR.RCAS.Sharpness", out double sharpness) ? sharpness : FsrSharpness;

        ShadowQuality = TryGetValue(settings, "r.ShadowQuality", out int shadowQuality) ? shadowQuality : ShadowQuality;
        ShadowMaxCsmResolution = TryGetValue(settings, "r.Shadow.MaxCSMResolution", out int maxCsm) ? maxCsm : ShadowMaxCsmResolution;
        ShadowPerObjectResolution = TryGetValue(settings, "r.Shadow.PerObjectResolutionMax", out int perObject) ? perObject : ShadowPerObjectResolution;
        ShadowRadiusThreshold = TryGetValue(settings, "r.Shadow.RadiusThreshold", out double radius) ? radius : ShadowRadiusThreshold;
        EnableCapsuleShadows = TryGetValue(settings, "r.CapsuleShadows", out bool enableCapsule) ? enableCapsule : EnableCapsuleShadows;
        EnableContactShadows = TryGetValue(settings, "r.ContactShadows", out bool enableContact) ? enableContact : EnableContactShadows;

        TextureStreamingPoolSize = TryGetValue(settings, "r.Streaming.PoolSize", out int pool) ? pool : TextureStreamingPoolSize;
        ViewTextureMipBias = TryGetValue(settings, "r.ViewTextureMipBias.Offset", out int mipBias) ? mipBias : ViewTextureMipBias;
        MaxAnisotropy = TryGetValue(settings, "r.MaxAnisotropy", out int anisotropy) ? anisotropy : MaxAnisotropy;
        StreamingMipBias = TryGetValue(settings, "r.Streaming.MipBias", out int streamingMipBias) ? streamingMipBias : StreamingMipBias;

        ViewDistanceScale = TryGetValue(settings, "r.ViewDistanceScale", out double viewDistance) ? viewDistance : ViewDistanceScale;
        FoliageLodDistanceScale = TryGetValue(settings, "foliage.LODDistanceScale", out double foliageLod) ? foliageLod : FoliageLodDistanceScale;
        StaticMeshLodDistanceScale = TryGetValue(settings, "r.StaticMeshLODDistanceScale", out double staticMeshLod) ? staticMeshLod : StaticMeshLodDistanceScale;
        SkeletalMeshLodBias = TryGetValue(settings, "r.SkeletalMeshLODBias", out int skeletalLod) ? skeletalLod : SkeletalMeshLodBias;

        EnableBloom = TryGetValue(settings, "r.BloomQuality", out bool enableBloom) ? enableBloom : EnableBloom;
        EnableMotionBlur = TryGetValue(settings, "r.MotionBlurQuality", out bool enableMotionBlur) ? enableMotionBlur : EnableMotionBlur;
        EnableAmbientOcclusion = TryGetValue(settings, "r.AmbientOcclusionLevels", out bool enableAmbientOcclusion) ? enableAmbientOcclusion : EnableAmbientOcclusion;
        EnableDepthOfField = TryGetValue(settings, "r.DepthOfFieldQuality", out bool enableDepthOfField) ? enableDepthOfField : EnableDepthOfField;
        EnableLensFlare = TryGetValue(settings, "r.LensFlareQuality", out bool enableLensFlare) ? enableLensFlare : EnableLensFlare;
        EnableChromaticAberration = TryGetValue(settings, "r.SceneColorFringeQuality", out bool enableChromaticAberration) ? enableChromaticAberration : EnableChromaticAberration;
        EnableEyeAdaptation = TryGetValue(settings, "r.EyeAdaptationQuality", out bool enableEyeAdaptation) ? enableEyeAdaptation : EnableEyeAdaptation;
        TonemapperQuality = TryGetValue(settings, "r.TonemapperQuality", out int tonemapper) ? tonemapper : TonemapperQuality;

        TemporalAaSamples = TryGetValue(settings, "r.TemporalAASamples", out int taaSamples) ? taaSamples : TemporalAaSamples;
        TemporalAaCurrentFrameWeight = TryGetValue(settings, "r.TemporalAACurrentFrameWeight", out double taaWeight) ? taaWeight : TemporalAaCurrentFrameWeight;
        TemporalAaFilterSize = TryGetValue(settings, "r.TemporalAAFilterSize", out double taaFilter) ? taaFilter : TemporalAaFilterSize;
        TemporalAaSharpen = TryGetValue(settings, "r.TemporalAASharpen", out double taaSharpen) ? taaSharpen : TemporalAaSharpen;

        if (TryGetValue(settings, "r.SSR.Quality", out int ssrQuality))
        {
            SsrQuality = ssrQuality;
            EnableSsr = ssrQuality > 0;
        }

        EnableDistanceFieldAo = TryGetValue(settings, "r.DistanceFieldAO", out bool enableDistanceFieldAo) ? enableDistanceFieldAo : EnableDistanceFieldAo;
        ReflectionCaptureResolution = TryGetValue(settings, "r.ReflectionCaptureResolution", out int reflection) ? reflection : ReflectionCaptureResolution;
        LightFunctionQuality = TryGetValue(settings, "r.LightFunctionQuality", out int lightFunction) ? lightFunction : LightFunctionQuality;

        EnableVSync = TryGetValue(settings, "r.VSync", out bool vsync) ? vsync : EnableVSync;
        FrameRateLimit = TryGetValue(settings, "t.MaxFPS", out int maxFps) ? maxFps : FrameRateLimit;
    }
    
    // apply it
    [RelayCommand]
    private void ApplySettings()
    {
        if (string.IsNullOrWhiteSpace(_currentGamePath))
        {
            ApplyStatus = "⚠ Game path not set";
            return;
        }

        try
        {
            ApplyToGame(_currentGamePath);
            ApplyStatus = "✓ Settings applied successfully";
        }
        catch (Exception ex)
        {
            ApplyStatus = $"✕ Failed to apply settings: {ex.Message}";
        }
    }

    public void ApplyToGame(string gamePath)
    {
        if (string.IsNullOrWhiteSpace(gamePath))
            throw new ArgumentException("Game path cannot be empty.", nameof(gamePath));

        _currentGamePath = gamePath;
        var gameSettings = new GameSettings { GameRootPath = gamePath };

        if (!File.Exists(gameSettings.EngineIniPath))
            throw new FileNotFoundException("Engine.ini was not found.", gameSettings.EngineIniPath);

        _iniService.CreateBackup(gameSettings.EngineIniPath);
        _iniService.UpdateValues(gameSettings.EngineIniPath, "SystemSettings", BuildEngineSettings());
    }

    private Dictionary<string, string> BuildEngineSettings()
    {
        var settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["r.Streaming.UsingNewKuroStreaming"] = UseNewKuroStreaming ? "1" : "0",
            ["r.SecondaryScreenPercentage.GameViewport"] = ScreenPercentage.ToString(CultureInfo.InvariantCulture),
            ["r.ShadowQuality"] = ShadowQuality.ToString(CultureInfo.InvariantCulture),
            ["r.Shadow.MaxCSMResolution"] = ShadowMaxCsmResolution.ToString(CultureInfo.InvariantCulture),
            ["r.Shadow.PerObjectResolutionMax"] = ShadowPerObjectResolution.ToString(CultureInfo.InvariantCulture),
            ["r.Shadow.PerObjectResolutionMin"] = ShadowPerObjectResolution.ToString(CultureInfo.InvariantCulture),
            ["r.Shadow.RadiusThreshold"] = Format(ShadowRadiusThreshold, "F3"),
            ["r.CapsuleShadows"] = EnableCapsuleShadows ? "1" : "0",
            ["r.ContactShadows"] = EnableContactShadows ? "1" : "0",
            ["r.Streaming.PoolSize"] = TextureStreamingPoolSize.ToString(CultureInfo.InvariantCulture),
            ["r.ViewTextureMipBias.Offset"] = ViewTextureMipBias.ToString(CultureInfo.InvariantCulture),
            ["r.MaxAnisotropy"] = MaxAnisotropy.ToString(CultureInfo.InvariantCulture),
            ["r.Streaming.MipBias"] = StreamingMipBias.ToString(CultureInfo.InvariantCulture),
            ["r.ViewDistanceScale"] = Format(ViewDistanceScale, "F1"),
            ["foliage.LODDistanceScale"] = Format(FoliageLodDistanceScale, "F1"),
            ["r.StaticMeshLODDistanceScale"] = Format(StaticMeshLodDistanceScale, "F1"),
            ["r.SkeletalMeshLODBias"] = SkeletalMeshLodBias.ToString(CultureInfo.InvariantCulture),
            ["r.BloomQuality"] = EnableBloom ? "5" : "0",
            ["r.MotionBlurQuality"] = EnableMotionBlur ? "4" : "0",
            ["r.AmbientOcclusionLevels"] = EnableAmbientOcclusion ? "3" : "0",
            ["r.DepthOfFieldQuality"] = EnableDepthOfField ? "2" : "0",
            ["r.LensFlareQuality"] = EnableLensFlare ? "2" : "0",
            ["r.SceneColorFringeQuality"] = EnableChromaticAberration ? "1" : "0",
            ["r.EyeAdaptationQuality"] = EnableEyeAdaptation ? "2" : "0",
            ["r.TonemapperQuality"] = TonemapperQuality.ToString(CultureInfo.InvariantCulture),
            ["r.TemporalAASamples"] = TemporalAaSamples.ToString(CultureInfo.InvariantCulture),
            ["r.TemporalAACurrentFrameWeight"] = Format(TemporalAaCurrentFrameWeight, "F2"),
            ["r.TemporalAAFilterSize"] = Format(TemporalAaFilterSize, "F1"),
            ["r.TemporalAASharpen"] = Format(TemporalAaSharpen, "F1"),
            ["r.SSR.Quality"] = EnableSsr ? SsrQuality.ToString(CultureInfo.InvariantCulture) : "0",
            ["r.DistanceFieldAO"] = EnableDistanceFieldAo ? "1" : "0",
            ["r.ReflectionCaptureResolution"] = ReflectionCaptureResolution.ToString(CultureInfo.InvariantCulture),
            ["r.LightFunctionQuality"] = LightFunctionQuality.ToString(CultureInfo.InvariantCulture),
            ["r.VSync"] = EnableVSync ? "1" : "0",
            ["t.MaxFPS"] = FrameRateLimit.ToString(CultureInfo.InvariantCulture),
            ["r.FidelityFX.FSR.SecondaryUpscale"] = UseFsrSecondaryUpscale ? "1" : "0"
        };

        if (UseFsrSecondaryUpscale)
        {
            settings["r.FidelityFX.FSR.MipBias.Method"] = "2";
            settings["r.FidelityFX.FSR.MipBias.Offset"] = "-2";
            settings["r.FidelityFX.FSR.RCAS.Sharpness"] = Format(FsrSharpness, "F1");
        }

        return settings;
    }
}
