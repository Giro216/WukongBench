using System.Text.Json;
using WukongBench.model;

namespace WukongBench.service;

public class BenchmarkParser
{
    public BenchmarkResult Parse(
        string json,
        string testName)
    {
        using JsonDocument document = JsonDocument.Parse(json);

        JsonElement root = document.RootElement;

        return new BenchmarkResult
        {
            TestName = testName,

            AverageFps = GetDouble(root, "FPSAvg"),
            MinimumFps = GetDouble(root, "FPSMin"),
            MaximumFps = GetDouble(root, "FPSMax"),
            Fps95 = GetDouble(root, "FPS95"),

            CpuUsage = GetDouble(root, "CPUAvg"),
            GpuUsage = GetDouble(root, "GPUAvg"),

            VideoMemory = GetDouble(root, "VideoMem"),

            CpuFrameTime = GetAverageFrameTime(
                root,
                "CPUFrameTime"),

            GpuFrameTime = GetAverageFrameTime(
                root,
                "GPUFrameTime"),

            Settings = ParseSettings(root)
        };
    }

    public SystemInfo ParseSystemInfo(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);

        JsonElement root = document.RootElement;

        return new SystemInfo
        {
            CPUModel = GetString(root, "CPUModel"),
            GPUModel = GetString(root, "GPUModel"),
            GpuDriverVer = GetString(root, "GpuDriverVer"),
            VideoMemSize = GetString(root, "VideoMemSize"),
            SysMem = GetString(root, "SysMem"),
            SysVer = GetString(root, "SysVer"),
            GameVer = GetString(root, "GameVer")
        };
    }

    private static double GetAverageFrameTime(
        JsonElement root,
        string property)
    {
        double total = 0;
        int count = 0;

        foreach (JsonElement record in
                 root.GetProperty("Records").EnumerateArray())
        {
            total += record.GetProperty(property).GetDouble();
            count++;
        }

        return count == 0 ? 0 : total / count;
    }

    private static BenchmarkSettings ParseSettings(
        JsonElement root)
    {
        return new BenchmarkSettings
        {
            QualityLevel = GetInt(root, "QualityLevel"),
            ImageQuality = GetInt(root, "ImageQuality"),

            ViewDistance = GetInt(root, "ViewDistance"),
            AntiAliasing = GetInt(root, "AntiAliasing"),
            PostProcessing = GetInt(root, "PostProcessing"),
            ShadowQuality = GetInt(root, "ShadowQuality"),
            TextureQuality = GetInt(root, "TextureQuality"),
            MaterialQuality = GetInt(root, "MaterialQuality"),
            VegetationQuality = GetInt(root, "VegetationQuality"),

            MotionBlur = GetInt(root, "MotionBlur"),

            Rtx = GetInt(root, "Rtx"),
            Dlss = GetInt(root, "Dlss"),
            InsertFrame = GetInt(root, "InsertFrame"),
            Dx12 = GetInt(root, "Dx12"),

            ResolutionWidth = 0,
            ResolutionHeight = 0
        };
    }

    private static int GetInt(
        JsonElement root,
        string name)
    {
        return root.TryGetProperty(name, out JsonElement value)
            ? value.GetInt32()
            : 0;
    }

    private static double GetDouble(
        JsonElement root,
        string name)
    {
        return root.TryGetProperty(name, out JsonElement value)
            ? value.GetDouble()
            : 0;
    }

    private static string GetString(
        JsonElement root,
        string name)
    {
        return root.TryGetProperty(name, out JsonElement value)
            ? value.ToString()
            : "?";
    }
}