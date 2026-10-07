namespace WukongBench.model;

public class BenchmarkSettings
{
    public int QualityLevel { get; set; }
    public int ImageQuality { get; set; }
    public int FxQuality { get; set; }
    public int GlobalIllumination { get; set; }
    public int ReflectionQuality { get; set; }
    public int ViewDistance { get; set; }
    public int AntiAliasing { get; set; }
    public int PostProcessing { get; set; }
    public int ShadowQuality { get; set; }
    public int TextureQuality { get; set; }
    public int MaterialQuality { get; set; }
    public int VegetationQuality { get; set; }

    public int MotionBlur { get; set; }

    public int Rtx { get; set; }
    public int Dlss { get; set; }
    public int InsertFrame { get; set; }
    public int Dx12 { get; set; }

    public int ResolutionWidth { get; set; }
    public int ResolutionHeight { get; set; }
}
