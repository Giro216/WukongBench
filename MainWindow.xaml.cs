using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;

namespace WukongBench
{
    public partial class MainWindow : Window
    {
        // ---- пути (поправьте, если игра установлена в другом месте) ----
        static readonly string GameDir = @"D:\SteamLibrary\steamapps\common\Black Myth Wukong Benchmark Tool";
        static readonly string ExePath = Path.Combine(GameDir, "b1_benchmark.exe");
        static readonly string IniPath = Path.Combine(GameDir, @"b1\Saved\Config\Windows\GameUserSettings.ini");
        static readonly string HistoryDir = Path.Combine(Path.GetTempPath(), @"b1\BenchMarkHistory\Tool");

        // настройки, которые покажем в отчёте (имена из файла результата)
        static readonly string[] SettingNames =
        {
            "QualityLevel", "ScreenResolution", "ImageQuality", "ViewDistance", "AntiAliasing",
            "PostProcessing", "ShadowQuality", "TextureQuality", "MaterialQuality", "VegetationQuality",
            "MotionBlur", "Rtx", "Dlss", "InsertFrame", "Dx12"
        };

        // ключ в UISettingData -> ключ в [ScalabilityGroups] (там значение на 1 меньше)
        static readonly string[][] SgPairs =
        {
            new[] { "ViewDistance", "sg.ViewDistanceQuality" },
            new[] { "AntiAliasing", "sg.AntiAliasingQuality" },
            new[] { "PostProcessing", "sg.PostProcessQuality" },
            new[] { "ShadowQuality", "sg.ShadowQuality" },
            new[] { "TextureQuality", "sg.TextureQuality" },
            new[] { "FxQuality", "sg.EffectsQuality" },
            new[] { "MaterialQuality", "sg.ShadingQuality" },
            new[] { "VegetationQuality", "sg.FoliageQuality" },
            new[] { "GlobalIllumination", "sg.GlobalIlluminationQuality" },
            new[] { "ReflectionQuality", "sg.ReflectionQuality" },
        };

        CancellationTokenSource cts;

        public MainWindow()
        {
            InitializeComponent();
        }

        // ---------- кнопки ----------

        async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = false;
            CancelButton.IsEnabled = true;
            OutputBox.Text = "";
            cts = new CancellationTokenSource();

            string originalIni = null;
            try
            {
                if (!File.Exists(ExePath)) throw new Exception("Не найден " + ExePath);
                if (!File.Exists(IniPath)) throw new Exception("Не найден " + IniPath + "\nСначала один раз запустите бенчмарк вручную.");

                originalIni = File.ReadAllText(IniPath);
                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "GameUserSettings.ini.backup"), originalIni);

                KillGame();

                string cpuJson = await RunTest("Тест 1/2: CPU", CpuProfile(), originalIni);
                await Task.Delay(3000);
                string gpuJson = await RunTest("Тест 2/2: GPU", GpuProfile(), originalIni);

                string report = BuildReport(cpuJson, gpuJson);
                OutputBox.Text = report;
                StatusText.Text = "Готово";

                string file = Path.Combine(AppContext.BaseDirectory, "report_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt");
                File.WriteAllText(file, report);
                OutputBox.Text += "\r\nОтчёт сохранён: " + file;
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
                // закрываем игру и возвращаем настройки пользователя
                KillGame();
                await Task.Delay(1500);
                if (originalIni != null) File.WriteAllText(IniPath, originalIni);

                StartButton.IsEnabled = true;
                CancelButton.IsEnabled = false;
            }
        }

        void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (cts != null) cts.Cancel();
        }

        // ---------- профили настроек (уровни: 1 = низко ... 5 = максимум) ----------

        // CPU: GPU разгружаем (низкое разрешение рендера, текстуры, сглаживание и т.д.), CPU нагружаем (дальность, тени, растительность, эффекты)
        static Dictionary<string, int> CpuProfile()
        {
            var p = new Dictionary<string, int>();

            // GPU минимум
            p["QualityLevel"] = 6;
            p["ImageQuality"] = 25;
            p["Rtx"] = 0;
            p["MotionBlur"] = 0;

            p["TextureQuality"] = 1;
            p["AntiAliasing"] = 1;
            p["PostProcessing"] = 1;
            p["GlobalIllumination"] = 1;
            p["ReflectionQuality"] = 1;

            // CPU максимум
            p["ViewDistance"] = 5;
            p["ShadowQuality"] = 5;
            p["VegetationQuality"] = 5;
            p["FxQuality"] = 5;
            p["MaterialQuality"] = 5;

            p["Dlss"] = 0;
            p["InsertFrame"] = 0;

            return p;
        }

        // GPU: всё на максимум
        static Dictionary<string, int> GpuProfile()
        {
            var p = new Dictionary<string, int>();
            p["QualityLevel"] = 6;
            p["ImageQuality"] = 100;
            p["Rtx"] = 1;
            p["MotionBlur"] = 0;
            p["ViewDistance"] = 5;
            p["ShadowQuality"] = 5;
            p["VegetationQuality"] = 5;
            p["FxQuality"] = 5;
            p["MaterialQuality"] = 5;
            p["TextureQuality"] = 5;
            p["AntiAliasing"] = 5;
            p["PostProcessing"] = 5;
            p["GlobalIllumination"] = 5;
            p["ReflectionQuality"] = 5;

            p["Dlss"] = 0;
            p["InsertFrame"] = 0;
            return p;
        }

        // ---------- запуск одного теста ----------

        async Task<string> RunTest(string title, Dictionary<string, int> profile, string originalIni)
        {
            Directory.CreateDirectory(HistoryDir);
            var oldFiles = new HashSet<string>(Directory.GetFiles(HistoryDir));

            StatusText.Text = title + ": применяю настройки";
            File.WriteAllText(
                IniPath,
                ApplyProfile(originalIni, profile, title.Contains("CPU"))
            );

            Process.Start(new ProcessStartInfo(ExePath) { WorkingDirectory = GameDir, UseShellExecute = true });

            var timer = Stopwatch.StartNew();
            while (true)
            {
                cts.Token.ThrowIfCancellationRequested();
                await Task.Delay(1500);

                if (timer.Elapsed.TotalMinutes > 20)
                    throw new Exception(title + ": не дождался результата за 20 минут");
                StatusText.Text = title + ": идёт тест... " + timer.Elapsed.ToString(@"mm\:ss");

                foreach (string file in Directory.GetFiles(HistoryDir))
                {
                    if (oldFiles.Contains(file)) continue;
                    try
                    {
                        string json = File.ReadAllText(file);
                        JsonDocument.Parse(json).RootElement.GetProperty("Records"); // если файл недописан — будет исключение
                        StatusText.Text = title + ": готово";
                        await Task.Delay(2000);
                        KillGame();
                        await Task.Delay(2500);
                        return json;
                    }
                    catch
                    {
                        // файл ещё пишется, попробуем на следующем круге
                    }
                }
            }
        }

        // подставляет значения профиля в текст ini-файла
        static string ApplyProfile(
            string ini,
            Dictionary<string, int> profile,
            bool cpuTest)
        {
            foreach (var item in profile)
            {
                ini = Regex.Replace(
                    ini,
                    "\\(\"" + item.Key + "\",\\s*\"[^\"]*\"\\)",
                    "(\"" + item.Key + "\", \"" + item.Value + "\")");
            }

            foreach (var pair in SgPairs)
            {
                if (profile.ContainsKey(pair[0]))
                    ini = SetLine(
                        ini,
                        pair[1],
                        (profile[pair[0]] - 1).ToString());
            }

            ini = SetLine(
                ini,
                "sg.ResolutionQuality",
                profile["ImageQuality"].ToString());

            // CPU-тест: максимально разгружаем GPU
            if (cpuTest)
            {
                ini = SetLine(ini, "ResolutionSizeX", "1280");
                ini = SetLine(ini, "ResolutionSizeY", "720");

                ini = SetLine(
                    ini,
                    "LastUserConfirmedResolutionSizeX",
                    "1280");

                ini = SetLine(
                    ini,
                    "LastUserConfirmedResolutionSizeY",
                    "720");

                ini = SetLine(
                    ini,
                    "LastUserConfirmedDesiredScreenWidth",
                    "1280");

                ini = SetLine(
                    ini,
                    "LastUserConfirmedDesiredScreenHeight",
                    "720");

                ini = SetLine(
                    ini,
                    "DesiredScreenWidth",
                    "1280");

                ini = SetLine(
                    ini,
                    "DesiredScreenHeight",
                    "720");
            }

            ini = SetLine(ini, "bNeverShowStartupUI", "True");
            ini = SetLine(ini, "bUseVSync", "False");
            ini = SetLine(ini, "FrameRateLimit", "0.000000");

            return ini;
        }

        static string SetLine(string ini, string key, string value)
        {
            return Regex.Replace(ini, "(?m)^" + Regex.Escape(key) + "=[^\\r\\n]*", key + "=" + value);
        }

        // закрывает все процессы из папки бенчмарка
        static void KillGame()
        {
            foreach (Process p in Process.GetProcesses())
            {
                try
                {
                    string path = p.MainModule.FileName;
                    if (path.StartsWith(GameDir, StringComparison.OrdinalIgnoreCase)) p.Kill(true);
                }
                catch
                {
                    // нет доступа к процессу — пропускаем
                }
            }
        }

        // ---------- отчёт ----------

        static string BuildReport(string cpuJson, string gpuJson)
        {
            var sb = new StringBuilder();
            JsonElement gpu = JsonDocument.Parse(gpuJson).RootElement;

            sb.AppendLine("=== КОНФИГУРАЦИЯ СИСТЕМЫ ===");
            sb.AppendLine("Процессор:        " + Text(gpu, "CPUModel") + " (потоков: " + Environment.ProcessorCount + ")");
            sb.AppendLine("Видеокарта:       " + Text(gpu, "GPUModel"));
            sb.AppendLine("Драйвер:          " + Text(gpu, "GpuDriverVer"));
            sb.AppendLine("Видеопамять:      " + Text(gpu, "VideoMemSize"));
            sb.AppendLine("Оперативная память: " + Text(gpu, "SysMem"));
            sb.AppendLine("ОС:               " + Text(gpu, "SysVer"));
            sb.AppendLine("Версия игры:      " + Text(gpu, "GameVer"));
            sb.AppendLine();

            AddTest(sb, "CPU-ТЕСТ", cpuJson);
            AddTest(sb, "GPU-ТЕСТ", gpuJson);
            return sb.ToString();
        }

        static void AddTest(StringBuilder sb, string title, string json)
        {
            JsonElement r = JsonDocument.Parse(json).RootElement;

            // среднее время кадра CPU и GPU по всем записям
            double cpuTime = 0, gpuTime = 0;
            int count = 0;
            foreach (JsonElement rec in r.GetProperty("Records").EnumerateArray())
            {
                cpuTime += rec.GetProperty("CPUFrameTime").GetDouble();
                gpuTime += rec.GetProperty("GPUFrameTime").GetDouble();
                count++;
            }
            if (count > 0) { cpuTime /= count; gpuTime /= count; }

            sb.AppendLine("=== " + title + " ===");
            sb.AppendLine("Средний FPS:      " + Text(r, "FPSAvg"));
            sb.AppendLine("Минимальный FPS:  " + Text(r, "FPSMin"));
            sb.AppendLine("Максимальный FPS: " + Text(r, "FPSMax"));
            sb.AppendLine("FPS95:            " + Text(r, "FPS95"));
            sb.AppendLine("Загрузка CPU, %:  " + Text(r, "CPUAvg"));
            sb.AppendLine("Загрузка GPU, %:  " + Text(r, "GPUAvg"));
            sb.AppendLine("Видеопамять, ГБ:  " + Math.Round(r.GetProperty("VideoMem").GetDouble(), 1));
            sb.AppendLine("Время кадра CPU:  " + Math.Round(cpuTime, 1) + " мс");
            sb.AppendLine("Время кадра GPU:  " + Math.Round(gpuTime, 1) + " мс");
            sb.AppendLine("Ограничивает:     " + (cpuTime > gpuTime ? "процессор" : "видеокарта"));
            sb.AppendLine();
            sb.AppendLine("Настройки теста:");
            foreach (string name in SettingNames)
            {
                if (r.TryGetProperty(name, out JsonElement value))
                    sb.AppendLine("  " + name + " = " + value);
            }
            sb.AppendLine();
        }

        static string Text(JsonElement e, string name)
        {
            return e.TryGetProperty(name, out JsonElement v) ? v.ToString() : "?";
        }
    }
}
