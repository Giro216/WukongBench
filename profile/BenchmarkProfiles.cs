using WukongBench.model;

namespace WukongBench.profile;

public static class BenchmarkProfiles
{
    public static BenchmarkSettings Cpu => new()
    {
        QualityLevel = 6,

        ResolutionWidth = 1280,
        ResolutionHeight = 720,

        ImageQuality = 25,

        Rtx = 0,
        Dlss = 0,
        InsertFrame = 0,
        MotionBlur = 0,

        TextureQuality = 1,
        AntiAliasing = 1,
        PostProcessing = 1,
        GlobalIllumination = 1,
        ReflectionQuality = 1,

        ViewDistance = 5,
        ShadowQuality = 5,
        VegetationQuality = 5,
        FxQuality = 5,
        MaterialQuality = 5,

        Dx12 = 1
    };

    public static BenchmarkSettings Gpu => new()
    {
        QualityLevel = 5,

        ResolutionWidth = 1920,
        ResolutionHeight = 1080,

        ImageQuality = 100,

        Rtx = 1,
        Dlss = 0,
        InsertFrame = 0,
        MotionBlur = 0,

        ViewDistance = 5,
        ShadowQuality = 5,
        VegetationQuality = 5,
        FxQuality = 5,
        MaterialQuality = 5,

        TextureQuality = 5,
        AntiAliasing = 5,
        PostProcessing = 5,
        GlobalIllumination = 5,
        ReflectionQuality = 5,

        Dx12 = 1
    };
}