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
        sb.AppendLine("=== РЕЗУЛЬТАТЫ БЕНЧМАРКОВ ===");
        sb.AppendLine();

        AppendTwoColumns(
            sb,
            cpu,
            gpu);

        return sb.ToString();
    }

    private static void AppendTwoColumns(
        StringBuilder sb,
        BenchmarkResult cpu,
        BenchmarkResult gpu)
    {
        const int labelWidth = 22;
        const int columnWidth = 32;

        string Header(string text)
        {
            return text.PadRight(columnWidth);
        }

        string Row(
            string label,
            string left,
            string right)
        {
            return
                label.PadRight(labelWidth) +
                left.PadRight(columnWidth) +
                right;
        }

        sb.AppendLine(
            "".PadRight(labelWidth) +
            Header("CPU-ТЕСТ") +
            "GPU-ТЕСТ");

        sb.AppendLine(
            new string('-', labelWidth + columnWidth * 2));

        sb.AppendLine(
            Row(
                "Средний FPS",
                $"{cpu.AverageFps:F0}",
                $"{gpu.AverageFps:F0}"));

        sb.AppendLine(
            Row(
                "Минимальный FPS",
                $"{cpu.MinimumFps:F0}",
                $"{gpu.MinimumFps:F0}"));

        sb.AppendLine(
            Row(
                "Максимальный FPS",
                $"{cpu.MaximumFps:F0}",
                $"{gpu.MaximumFps:F0}"));

        sb.AppendLine(
            Row(
                "FPS95",
                $"{cpu.Fps95:F0}",
                $"{gpu.Fps95:F0}"));

        sb.AppendLine(
            Row(
                "CPUAvg",
                $"{cpu.CpuUsage:F0}%",
                $"{gpu.CpuUsage:F0}%"));

        sb.AppendLine(
            Row(
                "GPUAvg",
                $"{cpu.GpuUsage:F0}%",
                $"{gpu.GpuUsage:F0}%"));

        sb.AppendLine(
            Row(
                "Видеопамять, ГБ",
                $"{Math.Round(cpu.VideoMemory, 1)}",
                $"{Math.Round(gpu.VideoMemory, 1)}"));

        sb.AppendLine(
            Row(
                "Время кадра CPU",
                $"{Math.Round(cpu.CpuFrameTime, 1)} мс",
                $"{Math.Round(gpu.CpuFrameTime, 1)} мс"));

        sb.AppendLine(
            Row(
                "Время кадра GPU",
                $"{Math.Round(cpu.GpuFrameTime, 1)} мс",
                $"{Math.Round(gpu.GpuFrameTime, 1)} мс"));

        sb.AppendLine(
            Row(
                "Ограничивает",
                cpu.Bottleneck,
                gpu.Bottleneck));

        sb.AppendLine();
        sb.AppendLine(
            "".PadRight(labelWidth) +
            Header("CPU-ТЕСТ") +
            "GPU-ТЕСТ");

        sb.AppendLine(
            new string('-', labelWidth + columnWidth * 2));

        AppendSettingRow(
            sb,
            "QualityLevel",
            cpu.Settings.QualityLevel,
            gpu.Settings.QualityLevel,
            labelWidth,
            columnWidth);

        AppendSettingRow(
            sb,
            "ImageQuality",
            cpu.Settings.ImageQuality,
            gpu.Settings.ImageQuality,
            labelWidth,
            columnWidth);

        AppendSettingRow(
            sb,
            "ViewDistance",
            cpu.Settings.ViewDistance,
            gpu.Settings.ViewDistance,
            labelWidth,
            columnWidth);

        AppendSettingRow(
            sb,
            "AntiAliasing",
            cpu.Settings.AntiAliasing,
            gpu.Settings.AntiAliasing,
            labelWidth,
            columnWidth);

        AppendSettingRow(
            sb,
            "PostProcessing",
            cpu.Settings.PostProcessing,
            gpu.Settings.PostProcessing,
            labelWidth,
            columnWidth);

        AppendSettingRow(
            sb,
            "ShadowQuality",
            cpu.Settings.ShadowQuality,
            gpu.Settings.ShadowQuality,
            labelWidth,
            columnWidth);

        AppendSettingRow(
            sb,
            "TextureQuality",
            cpu.Settings.TextureQuality,
            gpu.Settings.TextureQuality,
            labelWidth,
            columnWidth);

        AppendSettingRow(
            sb,
            "MaterialQuality",
            cpu.Settings.MaterialQuality,
            gpu.Settings.MaterialQuality,
            labelWidth,
            columnWidth);

        AppendSettingRow(
            sb,
            "VegetationQuality",
            cpu.Settings.VegetationQuality,
            gpu.Settings.VegetationQuality,
            labelWidth,
            columnWidth);

        AppendSettingRow(
            sb,
            "MotionBlur",
            cpu.Settings.MotionBlur,
            gpu.Settings.MotionBlur,
            labelWidth,
            columnWidth);

        AppendSettingRow(
            sb,
            "Rtx",
            cpu.Settings.Rtx,
            gpu.Settings.Rtx,
            labelWidth,
            columnWidth);

        AppendSettingRow(
            sb,
            "Dlss",
            cpu.Settings.Dlss,
            gpu.Settings.Dlss,
            labelWidth,
            columnWidth);

        AppendSettingRow(
            sb,
            "InsertFrame",
            cpu.Settings.InsertFrame,
            gpu.Settings.InsertFrame,
            labelWidth,
            columnWidth);

        AppendSettingRow(
            sb,
            "Dx12",
            cpu.Settings.Dx12,
            gpu.Settings.Dx12,
            labelWidth,
            columnWidth);
    }

    private static void AppendSettingRow(
        StringBuilder sb,
        string name,
        int cpuValue,
        int gpuValue,
        int labelWidth,
        int columnWidth)
    {
        sb.AppendLine(
            name.PadRight(labelWidth) +
            cpuValue.ToString().PadRight(columnWidth) +
            gpuValue);
    }
}