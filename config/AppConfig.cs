using System.IO;

namespace WukongBench.config;

public class AppConfig
{
    public string GameDirectory { get; set; } = "";
    public string SteamExecutable { get; set; } = "";
    public int SteamAppId { get; set; }

    public string IniPath =>
        Path.Combine(
            GameDirectory,
            @"b1\Saved\Config\Windows\GameUserSettings.ini");

    public string HistoryDirectory =>
        Path.Combine(
            Path.GetTempPath(),
            @"b1\BenchMarkHistory\Tool");

    public string BenchmarkExePath =>
        Path.Combine(
            GameDirectory,
            "b1_benchmark.exe");
}