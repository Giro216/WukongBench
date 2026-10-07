using System.Diagnostics;

namespace WukongBench.util;

public static class ProcessUtils
{
    public static void KillGameProcesses(
        string gameDirectory)
    {
        foreach (Process process in Process.GetProcesses())
        {
            try
            {
                string? path =
                    process.MainModule?.FileName;

                if (path != null &&
                    path.StartsWith(
                        gameDirectory,
                        StringComparison.OrdinalIgnoreCase))
                {
                    process.Kill(true);
                }
            }
            catch
            {
                // Процесс мог уже завершиться или не иметь доступа к MainModule
            }
        }
    }
}