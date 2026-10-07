using System.IO;
using System.Windows;
using WukongBench.model;
using WukongBench.profile;
using WukongBench.service;
using WukongBench.util;

namespace WukongBench;

public partial class MainWindow : Window
{
    private static readonly string GameDirectory =
        @"D:\SteamLibrary\steamapps\common\Black Myth Wukong Benchmark Tool";

    private static readonly string IniPath =
        Path.Combine(
            GameDirectory,
            @"b1\Saved\Config\Windows\GameUserSettings.ini");

    private static readonly string HistoryDirectory =
        Path.Combine(
            Path.GetTempPath(),
            @"b1\BenchMarkHistory\Tool");

    private readonly BenchmarkService _benchmarkService;
    private readonly ReportService _reportService;

    private CancellationTokenSource? _cts;

    public MainWindow()
    {
        InitializeComponent();

        _benchmarkService = new BenchmarkService(
            GameDirectory,
            IniPath,
            HistoryDirectory);

        _reportService = new ReportService();
    }

    private async void StartButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        StartButton.IsEnabled = false;
        CancelButton.IsEnabled = true;

        OutputBox.Text = "";
        _cts = new CancellationTokenSource();

        string? originalIni = null;

        try
        {
            ValidateFiles();

            originalIni = File.ReadAllText(IniPath);

            SaveBackup(originalIni);

            ProcessUtils.KillGameProcesses(GameDirectory);

            BenchmarkRunResult cpuRun =
                await _benchmarkService.RunAsync(
                    "CPU-ТЕСТ",
                    BenchmarkProfiles.Cpu,
                    originalIni,
                    _cts.Token,
                    UpdateStatus);

            await Task.Delay(3000, _cts.Token);

            BenchmarkRunResult gpuRun =
                await _benchmarkService.RunAsync(
                    "GPU-ТЕСТ",
                    BenchmarkProfiles.Gpu,
                    originalIni,
                    _cts.Token,
                    UpdateStatus);

            string report =
                _reportService.BuildReport(
                    cpuRun.System,
                    cpuRun.Result,
                    gpuRun.Result);

            OutputBox.Text = report;

            SaveReport(report);

            StatusText.Text = "Готово";
        }
        catch (OperationCanceledException)
        {
            StatusText.Text = "Отменено";
        }
        catch (Exception ex)
        {
            StatusText.Text = "Ошибка";
            MessageBox.Show(ex.Message);
        }
        finally
        {
            ProcessUtils.KillGameProcesses(GameDirectory);

            if (originalIni != null)
            {
                File.WriteAllText(
                    IniPath,
                    originalIni);
            }

            _cts?.Dispose();
            _cts = null;

            StartButton.IsEnabled = true;
            CancelButton.IsEnabled = false;
        }
    }

    private void CancelButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        _cts?.Cancel();
    }

    private void UpdateStatus(string message)
    {
        StatusText.Text = message;
    }

    private static void ValidateFiles()
    {
        string exePath =
            Path.Combine(
                GameDirectory,
                "b1_benchmark.exe");

        if (!File.Exists(exePath))
            throw new FileNotFoundException(
                "Не найден benchmark.",
                exePath);

        if (!File.Exists(IniPath))
            throw new FileNotFoundException(
                "Не найден GameUserSettings.ini.",
                IniPath);
    }

    private static void SaveBackup(string ini)
    {
        File.WriteAllText(
            Path.Combine(
                AppContext.BaseDirectory,
                "GameUserSettings.ini.backup"),
            ini);
    }

    private void SaveReport(string report)
    {
        string file =
            Path.Combine(
                AppContext.BaseDirectory,
                $"report_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

        File.WriteAllText(file, report);

        OutputBox.Text +=
            $"\r\nОтчёт сохранён: {file}";
    }
}