using System.Diagnostics;

namespace WukongBench.util;

public static class ProcessUtils
{
    public static void KillGameProcesses()
    {
        KillProcess("b1_benchmark");
        KillProcess("b1-Win64-Shipping");
    }

    private static void KillProcess(string processName)
    {
        Process[] processes =
            Process.GetProcessesByName(processName);

        foreach (Process process in processes)
        {
            try
            {
                Console.WriteLine(
                    $"Найден процесс: {process.ProcessName}, PID={process.Id}");

                if (!process.HasExited)
                {
                    process.Kill(true);

                    Console.WriteLine(
                        $"Kill вызван: PID={process.Id}");

                    process.WaitForExit(5000);

                    Console.WriteLine(
                        $"Процесс завершён: PID={process.Id}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Ошибка завершения {processName}: {ex.Message}");
            }
            finally
            {
                process.Dispose();
            }
        }
    }
}