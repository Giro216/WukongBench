using System.Text;
using WukongBench.model;

namespace WukongBench.service;

public class ReportService
{
    public string BuildReport(
        SystemInfo system,
        BenchmarkResult cpu,
        BenchmarkResult gpu)
    {
        var sb = new StringBuilder();

        sb.AppendLine("=== КОНФИГУРАЦИЯ СИСТЕМЫ ===");
        sb.AppendLine(
            $"Процессор:          {system.CPUModel}");
        sb.AppendLine(
            $"Видеокарта:         {system.GPUModel}");
        sb.AppendLine(
            $"Драйвер:            {system.GpuDriverVer}");
        sb.AppendLine(
            $"Видеопамять:        {system.VideoMemSize}");
        sb.AppendLine(
            $"Оперативная память: {system.SysMem}");
        sb.AppendLine(
            $"ОС:                 {system.SysVer}");
        sb.AppendLine(
            $"Версия игры:        {system.GameVer}");
        sb.AppendLine();

        AppendTest(sb, cpu);
        AppendTest(sb, gpu);

        return sb.ToString();
    }

    private static void AppendTest(
        StringBuilder sb,
        BenchmarkResult result)
    {
        sb.AppendLine(
            $"=== {result.TestName} ===");

        sb.AppendLine(
            $"Средний FPS:      {result.AverageFps}");
        sb.AppendLine(
            $"Минимальный FPS:  {result.MinimumFps}");
        sb.AppendLine(
            $"Максимальный FPS: {result.MaximumFps}");
        sb.AppendLine(
            $"FPS95:            {result.Fps95}");

        sb.AppendLine(
            $"CPUAvg (benchmark): {result.CpuUsage}");
        sb.AppendLine(
            $"GPUAvg:             {result.GpuUsage}");

        sb.AppendLine(
            $"Видеопамять, ГБ:  {Math.Round(result.VideoMemory, 1)}");

        sb.AppendLine(
            $"Время кадра CPU:  {Math.Round(result.CpuFrameTime, 1)} мс");

        sb.AppendLine(
            $"Время кадра GPU:  {Math.Round(result.GpuFrameTime, 1)} мс");

        sb.AppendLine(
            $"Ограничивает:     {result.Bottleneck}");

        sb.AppendLine();
        sb.AppendLine("Настройки теста:");

        AppendSettings(sb, result.Settings);

        sb.AppendLine();
    }

    private static void AppendSettings(
        StringBuilder sb,
        BenchmarkSettings settings)
    {
        sb.AppendLine(
            $"  QualityLevel = {settings.QualityLevel}");

        sb.AppendLine(
            $"  ImageQuality = {settings.ImageQuality}");

        sb.AppendLine(
            $"  ViewDistance = {settings.ViewDistance}");

        sb.AppendLine(
            $"  AntiAliasing = {settings.AntiAliasing}");

        sb.AppendLine(
            $"  PostProcessing = {settings.PostProcessing}");

        sb.AppendLine(
            $"  ShadowQuality = {settings.ShadowQuality}");

        sb.AppendLine(
            $"  TextureQuality = {settings.TextureQuality}");

        sb.AppendLine(
            $"  MaterialQuality = {settings.MaterialQuality}");

        sb.AppendLine(
            $"  VegetationQuality = {settings.VegetationQuality}");

        sb.AppendLine(
            $"  MotionBlur = {settings.MotionBlur}");

        sb.AppendLine(
            $"  Rtx = {settings.Rtx}");

        sb.AppendLine(
            $"  Dlss = {settings.Dlss}");

        sb.AppendLine(
            $"  InsertFrame = {settings.InsertFrame}");

        sb.AppendLine(
            $"  Dx12 = {settings.Dx12}");
    }
}