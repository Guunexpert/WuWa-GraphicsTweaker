using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhoebeEditor.Models;
using PhoebeEditor.Services;
using System.IO;

namespace PhoebeEditor.ViewModels;

public partial class TweaksViewModel : ObservableObject
{
    private readonly IniFileService _iniService;
    private string _currentGamePath = string.Empty;

    // ─── Streaming (new 3.3 feature) ────────────────────────────
    [ObservableProperty] private bool _useNewKuroStreaming = true;

    // ─── Resolution & Upscaling ───────────────────────────
    [ObservableProperty] private int _screenPercentage = 100;
    [ObservableProperty] private bool _useFsrSecondaryUpscale = false;
    [ObservableProperty] private double _fsrSharpness = 1.0;

    // ─── Shadows ──────────────────────────────────────────
    [ObservableProperty] private int _shadowQuality = 3;
    [ObservableProperty] private int _shadowMaxCsmResolution = 1024;
    [ObservableProperty] private int _shadowPerObjectResolution = 512;
    [ObservableProperty] private double _shadowRadiusThreshold = 0.02;
    [ObservableProperty] private bool _enableCapsuleShadows = true;
    [ObservableProperty] private bool _enableContactShadows = true;

    // ─── Textures & Streaming ─────────────────────────────
    [ObservableProperty] private int _textureStreamingPoolSize = 512;
    [ObservableProperty] private int _viewTextureMipBias = 0;
    [ObservableProperty] private int _maxAnisotropy = 8;
    [ObservableProperty] private int _streamingMipBias = 0;

    // ─── LOD & Distance ───────────────────────────────────
    [ObservableProperty] private double _viewDistanceScale = 1.0;
    [ObservableProperty] private double _foliageLodDistanceScale = 1.0;
    [ObservableProperty] private double _staticMeshLodDistanceScale = 1.0;
    [ObservableProperty] private int _skeletalMeshLodBias = 0;

    // ─── Post Processing ──────────────────────────────────
    [ObservableProperty] private bool _enableBloom = true;
    [ObservableProperty] private bool _enableMotionBlur = true;
    [ObservableProperty] private bool _enableAmbientOcclusion = true;
    [ObservableProperty] private bool _enableDepthOfField = true;
    [ObservableProperty] private bool _enableLensFlare = true;
    [ObservableProperty] private bool _enableChromaticAberration = true;
    [ObservableProperty] private bool _enableEyeAdaptation = true;
    [ObservableProperty] private int _tonemapperQuality = 1;

    // ─── Anti-Aliasing ────────────────────────────────────
    [ObservableProperty] private int _temporalAaSamples = 8;
    [ObservableProperty] private double _temporalAaCurrentFrameWeight = 0.04;
    [ObservableProperty] private double _temporalAaFilterSize = 1.0;
    [ObservableProperty] private double _temporalAaSharpen = 0.0;

    // ─── Lighting & Reflections ───────────────────────────
    [ObservableProperty] private bool _enableSsr = true;
    [ObservableProperty] private int _ssrQuality = 3;
    [ObservableProperty] private bool _enableDistanceFieldAo = true;
    [ObservableProperty] private int _reflectionCaptureResolution = 128;
    [ObservableProperty] private int _lightFunctionQuality = 1;

    // ─── Performance ──────────────────────────────────────
    [ObservableProperty] private bool _enableVSync = false;
    [ObservableProperty] private int _frameRateLimit = 0;

    // ─── Status ───────────────────────────────────────────
    [ObservableProperty] private string _applyStatus = string.Empty;

    public Dictionary<string, string> EngineSettings => BuildEngineSettings();

    public TweaksViewModel(IniFileService iniService)
    {
        _iniService = iniService;
    }

    public void LoadFromGame(string gamePath)
    {
        _currentGamePath = gamePath;
        var gs = new GameSettings { GameRootPath = gamePath };
        if (!File.Exists(gs.EngineIniPath)) return;

        var s = _iniService.ReadSection(gs.EngineIniPath, "SystemSettings");

        // Streaming
        if (s.TryGetValue("r.Streaming.UsingNewKuroStreaming", out var nks)) UseNewKuroStreaming = nks == "1";

        // Resolution
        if (s.TryGetValue("r.SecondaryScreenPercentage.GameViewport", out var sp) && int.TryParse(sp, out var spv)) ScreenPercentage = spv;
        if (s.TryGetValue("r.FidelityFX.FSR.SecondaryUpscale", out var fsr)) UseFsrSecondaryUpscale = fsr == "1";
        if (s.TryGetValue("r.FidelityFX.FSR.RCAS.Sharpness", out var fsrs) && double.TryParse(fsrs, out var fsrsv)) FsrSharpness = fsrsv;

        // Shadows
        if (s.TryGetValue("r.ShadowQuality", out var sq) && int.TryParse(sq, out var sqv)) ShadowQuality = sqv;
        if (s.TryGetValue("r.Shadow.MaxCSMResolution", out var smr) && int.TryParse(smr, out var smrv)) ShadowMaxCsmResolution = smrv;
        if (s.TryGetValue("r.Shadow.PerObjectResolutionMax", out var spo) && int.TryParse(spo, out var spov)) ShadowPerObjectResolution = spov;
        if (s.TryGetValue("r.CapsuleShadows", out var cs)) EnableCapsuleShadows = cs != "0";
        if (s.TryGetValue("r.ContactShadows", out var cos)) EnableContactShadows = cos != "0";

        // Textures
        if (s.TryGetValue("r.Streaming.PoolSize", out var pool) && int.TryParse(pool, out var poolv)) TextureStreamingPoolSize = poolv;
        if (s.TryGetValue("r.MaxAnisotropy", out var ani) && int.TryParse(ani, out var aniv)) MaxAnisotropy = aniv;
        if (s.TryGetValue("r.Streaming.MipBias", out var mb) && int.TryParse(mb, out var mbv)) StreamingMipBias = mbv;

        // LOD
        if (s.TryGetValue("r.ViewDistanceScale", out var vds) && double.TryParse(vds, out var vdsv)) ViewDistanceScale = vdsv;
        if (s.TryGetValue("foliage.LODDistanceScale", out var fol) && double.TryParse(fol, out var folv)) FoliageLodDistanceScale = folv;
        if (s.TryGetValue("r.StaticMeshLODDistanceScale", out var sml) && double.TryParse(sml, out var smlv)) StaticMeshLodDistanceScale = smlv;
        if (s.TryGetValue("r.SkeletalMeshLODBias", out var skl) && int.TryParse(skl, out var sklv)) SkeletalMeshLodBias = sklv;

        // Post Processing
        if (s.TryGetValue("r.BloomQuality", out var bloom)) EnableBloom = bloom != "0";
        if (s.TryGetValue("r.MotionBlurQuality", out var motblur)) EnableMotionBlur = motblur != "0";
        if (s.TryGetValue("r.AmbientOcclusionLevels", out var ao)) EnableAmbientOcclusion = ao != "0";
        if (s.TryGetValue("r.DepthOfFieldQuality", out var dof)) EnableDepthOfField = dof != "0";
        if (s.TryGetValue("r.LensFlareQuality", out var lf)) EnableLensFlare = lf != "0";
        if (s.TryGetValue("r.SceneColorFringeQuality", out var ca)) EnableChromaticAberration = ca != "0";
        if (s.TryGetValue("r.EyeAdaptationQuality", out var ea)) EnableEyeAdaptation = ea != "0";

        // AA
        if (s.TryGetValue("r.TemporalAASamples", out var taas) && int.TryParse(taas, out var taasv)) TemporalAaSamples = taasv;
        if (s.TryGetValue("r.TemporalAACurrentFrameWeight", out var taacfw) && double.TryParse(taacfw, out var taacfwv)) TemporalAaCurrentFrameWeight = taacfwv;
        if (s.TryGetValue("r.TemporalAASharpen", out var taash) && double.TryParse(taash, out var taashv)) TemporalAaSharpen = taashv;

        // Lighting
        if (s.TryGetValue("r.SSR.Quality", out var ssr) && int.TryParse(ssr, out var ssrv)) { EnableSsr = ssrv > 0; SsrQuality = ssrv; }
        if (s.TryGetValue("r.DistanceFieldAO", out var dfao)) EnableDistanceFieldAo = dfao != "0";
        if (s.TryGetValue("r.ReflectionCaptureResolution", out var rcr) && int.TryParse(rcr, out var rcrv)) ReflectionCaptureResolution = rcrv;

        // Performance
        if (s.TryGetValue("r.VSync", out var vs)) EnableVSync = vs == "1";
        if (s.TryGetValue("t.MaxFPS", out var fps) && int.TryParse(fps, out var fpsv)) FrameRateLimit = fpsv;
    }

    public void LoadFromPreset(Preset preset)
    {
        var s = preset.EngineIniSettings;
        if (s.TryGetValue("r.ScreenPercentage", out var sp) && int.TryParse(sp, out var spv)) ScreenPercentage = spv;
        if (s.TryGetValue("r.Shadow.MaxCSMResolution", out var smr) && int.TryParse(smr, out var smrv)) ShadowMaxCsmResolution = smrv;
        if (s.TryGetValue("r.Streaming.PoolSize", out var pool) && int.TryParse(pool, out var poolv)) TextureStreamingPoolSize = poolv;
        if (s.TryGetValue("r.ViewDistanceScale", out var vds) && double.TryParse(vds, out var vdsv)) ViewDistanceScale = vdsv;
        if (s.TryGetValue("r.BloomQuality", out var bloom)) EnableBloom = bloom != "0";
        if (s.TryGetValue("r.MotionBlurQuality", out var mb)) EnableMotionBlur = mb != "0";
        if (s.TryGetValue("r.VSync", out var vs)) EnableVSync = vs == "1";
        if (s.TryGetValue("t.MaxFPS", out var fps) && int.TryParse(fps, out var fpsv)) FrameRateLimit = fpsv;
    }

    [RelayCommand]
    private void ApplySettings()
    {
        if (string.IsNullOrEmpty(_currentGamePath)) { ApplyStatus = "⚠ Game path not set"; return; }
        ApplyToGame(_currentGamePath);
        ApplyStatus = "✓ Settings applied successfully";
    }

    public void ApplyToGame(string gamePath)
    {
        _currentGamePath = gamePath;
        var gs = new GameSettings { GameRootPath = gamePath };
        _iniService.WriteSection(gs.EngineIniPath, "SystemSettings", BuildEngineSettings());
    }

    private Dictionary<string, string> BuildEngineSettings()
    {
        var s = new Dictionary<string, string>
        {
            ["r.Streaming.UsingNewKuroStreaming"] = UseNewKuroStreaming ? "1" : "0",

            // Resolution
            ["r.SecondaryScreenPercentage.GameViewport"] = ScreenPercentage.ToString(),

            // Shadows
            ["r.ShadowQuality"] = ShadowQuality.ToString(),
            ["r.Shadow.MaxCSMResolution"] = ShadowMaxCsmResolution.ToString(),
            ["r.Shadow.PerObjectResolutionMax"] = ShadowPerObjectResolution.ToString(),
            ["r.Shadow.PerObjectResolutionMin"] = ShadowPerObjectResolution.ToString(),
            ["r.Shadow.RadiusThreshold"] = ShadowRadiusThreshold.ToString("F3"),
            ["r.CapsuleShadows"] = EnableCapsuleShadows ? "1" : "0",
            ["r.ContactShadows"] = EnableContactShadows ? "1" : "0",

            // Textures
            ["r.Streaming.PoolSize"] = TextureStreamingPoolSize.ToString(),
            ["r.ViewTextureMipBias.Offset"] = ViewTextureMipBias.ToString(),
            ["r.MaxAnisotropy"] = MaxAnisotropy.ToString(),
            ["r.Streaming.MipBias"] = StreamingMipBias.ToString(),

            // LOD
            ["r.ViewDistanceScale"] = ViewDistanceScale.ToString("F1"),
            ["foliage.LODDistanceScale"] = FoliageLodDistanceScale.ToString("F1"),
            ["r.StaticMeshLODDistanceScale"] = StaticMeshLodDistanceScale.ToString("F1"),
            ["r.SkeletalMeshLODBias"] = SkeletalMeshLodBias.ToString(),

            // Post Processing
            ["r.BloomQuality"] = EnableBloom ? "5" : "0",
            ["r.MotionBlurQuality"] = EnableMotionBlur ? "4" : "0",
            ["r.AmbientOcclusionLevels"] = EnableAmbientOcclusion ? "3" : "0",
            ["r.DepthOfFieldQuality"] = EnableDepthOfField ? "2" : "0",
            ["r.LensFlareQuality"] = EnableLensFlare ? "2" : "0",
            ["r.SceneColorFringeQuality"] = EnableChromaticAberration ? "1" : "0",
            ["r.EyeAdaptationQuality"] = EnableEyeAdaptation ? "2" : "0",
            ["r.TonemapperQuality"] = TonemapperQuality.ToString(),

            // Anti-Aliasing
            ["r.TemporalAASamples"] = TemporalAaSamples.ToString(),
            ["r.TemporalAACurrentFrameWeight"] = TemporalAaCurrentFrameWeight.ToString("F2"),
            ["r.TemporalAAFilterSize"] = TemporalAaFilterSize.ToString("F1"),
            ["r.TemporalAASharpen"] = TemporalAaSharpen.ToString("F1"),

            // Lighting
            ["r.SSR.Quality"] = EnableSsr ? SsrQuality.ToString() : "0",
            ["r.DistanceFieldAO"] = EnableDistanceFieldAo ? "1" : "0",
            ["r.ReflectionCaptureResolution"] = ReflectionCaptureResolution.ToString(),
            ["r.LightFunctionQuality"] = LightFunctionQuality.ToString(),

            // Performance
            ["r.VSync"] = EnableVSync ? "1" : "0",
            ["t.MaxFPS"] = FrameRateLimit.ToString(),
        };

        // FSR Secondary Upscale
        if (UseFsrSecondaryUpscale)
        {
            
            s["r.FidelityFX.FSR.SecondaryUpscale"] = "1";
            s["r.FidelityFX.FSR.MipBias.Method"] = "2";
            s["r.FidelityFX.FSR.MipBias.Offset"] = "-2";
            s["r.FidelityFX.FSR.RCAS.Sharpness"] = FsrSharpness.ToString("F1");
        }

        return s;
    }
}
