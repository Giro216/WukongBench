using System.IO;
using System.Text.Json;
using WukongBench.config;
using System.Windows;
using WukongBench.model;
using WukongBench.profile;
using WukongBench.service;
using WukongBench.util;

namespace WukongBench;

public partial class MainWindow : Window
{
    private readonly BenchmarkService _benchmarkService;
    private readonly ReportService _reportService;
    private readonly AppConfig _config;

    private CancellationTokenSource? _cts;

    public MainWindow()
    {
        InitializeComponent();

        string configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

        if (!File.Exists(configPath))
        {
            MessageBox.Show(
                $"Не найден файл конфигурации:\n{configPath}");

            Close();
            return;
        }

        _config =
            JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(configPath))
            ?? throw new InvalidOperationException(
                "Не удалось загрузить appsettings.json.");

        _benchmarkService = new BenchmarkService(
            _config.GameDirectory,
            _config.IniPath,
            _config.HistoryDirectory);

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

            originalIni = File.ReadAllText(_config.IniPath);

            SaveBackup(originalIni);

            ProcessUtils.KillGameProcesses();

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
            ProcessUtils.KillGameProcesses();

            if (originalIni != null)
            {
                File.WriteAllText(
                    _config.IniPath,
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

    private void ValidateFiles()
    {
        if (!File.Exists(_config.BenchmarkExePath))
            throw new FileNotFoundException(
                "Не найден benchmark.",
                _config.BenchmarkExePath);

        if (!File.Exists(_config.IniPath))
            throw new FileNotFoundException(
                "Не найден GameUserSettings.ini.",
                _config.IniPath);
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