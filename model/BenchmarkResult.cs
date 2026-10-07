namespace WukongBench.model;

public class BenchmarkResult
{
    public string TestName { get; set; } = "";

    public double AverageFps { get; set; }
    public double MinimumFps { get; set; }
    public double MaximumFps { get; set; }
    public double Fps95 { get; set; }

    public double CpuUsage { get; set; }
    public double GpuUsage { get; set; }

    public double VideoMemory { get; set; }

    public double CpuFrameTime { get; set; }
    public double GpuFrameTime { get; set; }

    public string Bottleneck =>
        CpuFrameTime > GpuFrameTime
            ? "процессор"
            : "видеокарта";

    public BenchmarkSettings Settings { get; set; } = new();
}