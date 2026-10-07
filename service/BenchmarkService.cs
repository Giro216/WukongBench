using System.Diagnostics;
using System.IO;
using WukongBench.model;
using WukongBench.util;
using WukongBench.config;

namespace WukongBench.service;

public class BenchmarkService
{
    private readonly AppConfig _config;
    private readonly IniService _iniService;
    private readonly BenchmarkParser _parser;

    private readonly GameAutomationService _gameAutomationService;

    public BenchmarkService(AppConfig config)
    {
        _config = config;

        _iniService = new IniService();
        _parser = new BenchmarkParser();
        _gameAutomationService = new GameAutomationService();
    }

    public async Task<BenchmarkRunResult> RunAsync(
        string testName,
        BenchmarkSettings settings,
        string originalIni,
        CancellationToken cancellationToken,
        Action<string>? statusCallback = null)
    {
        Directory.CreateDirectory(_config.HistoryDirectory);

        var oldFiles = new HashSet<string>(
            Directory.GetFiles(_config.HistoryDirectory));

        statusCallback?.Invoke(
            $"{testName}: применяю настройки");

        string configuredIni =
            _iniService.ApplyProfile(
                originalIni,
                settings);

        File.WriteAllText(
            _config.IniPath,
            configuredIni);

        Process.Start(new ProcessStartInfo
        {
            FileName = _config.SteamExecutable,
            Arguments = $"-applaunch {_config.SteamAppId}",
            UseShellExecute = true
        });

        await _gameAutomationService.StartBenchmarkAsync(cancellationToken);

        Stopwatch timer = Stopwatch.StartNew();

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await Task.Delay(
                TimeSpan.FromSeconds(1.5),
                cancellationToken);

            if (timer.Elapsed > TimeSpan.FromMinutes(20))
            {
                throw new TimeoutException(
                    $"{testName}: не дождался результата за 20 минут");
            }

            statusCallback?.Invoke(
                $"{testName}: идёт тест... " +
                timer.Elapsed.ToString(@"mm\:ss"));

            string? resultFile =
                FindNewResultFile(oldFiles);

            if (resultFile == null)
            {
                continue;
            }


            try
            {
                string json = File.ReadAllText(resultFile);

                var result =
                    _parser.Parse(json, testName);

                var system =
                    _parser.ParseSystemInfo(json);


                statusCallback?.Invoke(
                    $"{testName}: готово");

                await Task.Delay(
                    TimeSpan.FromSeconds(2),
                    cancellationToken);

                ProcessUtils.KillGameProcesses();

                await Task.Delay(
                    TimeSpan.FromSeconds(2.5),
                    cancellationToken);

                return new BenchmarkRunResult
                {
                    Result = result,
                    System = system
                };
            }
            catch (System.Text.Json.JsonException ex)
            {
                Debug.WriteLine(
                    $"[JSON] JSON ещё не готов: {ex.Message}");
            }
            catch (IOException ex)
            {
                Debug.WriteLine(
                    $"[IO] Файл ещё занят: {ex.Message}");
            }
        }
    }

    private string? FindNewResultFile(
        HashSet<string> oldFiles)
    {
        foreach (string file in
                 Directory.GetFiles(_config.HistoryDirectory))
        {
            if (!oldFiles.Contains(file))
                return file;
        }

        return null;
    }
}