namespace WukongBench.model;

public class BenchmarkRunResult
{
    public BenchmarkResult Result { get; set; } = new();
    public SystemInfo System { get; set; } = new();
}