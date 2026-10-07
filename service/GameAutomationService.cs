using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WukongBench.service;

public class GameAutomationService
{
    private const double MenuX = 0.1115;
    private const double MenuY = 0.4458;

    private const double ConfirmX = 0.3898;
    private const double ConfirmY = 0.5885;

    private const int InitialDelaySeconds = 40;
    private const int ActionDelaySeconds = 1;

    private const uint INPUT_MOUSE = 0;
    private const uint INPUT_KEYBOARD = 1;

    private const uint KEYEVENTF_KEYUP = 0x0002;

    private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const uint MOUSEEVENTF_LEFTUP = 0x0004;

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(
        IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(
        int x,
        int y);

    [DllImport("user32.dll")]
    private static extern bool GetClientRect(
        IntPtr hWnd,
        out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern bool ClientToScreen(
        IntPtr hWnd,
        ref POINT lpPoint);

    [DllImport("user32.dll")]
    private static extern uint SendInput(
        uint nInputs,
        INPUT[] pInputs,
        int cbSize);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public InputUnion union;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)]
        public KEYBDINPUT keyboard;

        [FieldOffset(0)]
        public MOUSEINPUT mouse;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    public async Task StartBenchmarkAsync(
        CancellationToken cancellationToken)
    {
        Process gameProcess =
            await WaitForGameProcessAsync(
                cancellationToken);

        IntPtr windowHandle =
            gameProcess.MainWindowHandle;

        if (windowHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException(
                "Не найдено окно benchmark.");
        }

        Console.WriteLine(
            $"Найдено окно игры. HWND={windowHandle}");

        SetForegroundWindow(windowHandle);

        Console.WriteLine(
            $"Ждём {InitialDelaySeconds} секунд...");

        await Task.Delay(
            TimeSpan.FromSeconds(
                InitialDelaySeconds),
            cancellationToken);

        // 1. Нажимаем любую клавишу
        Console.WriteLine(
            "1. Нажимаем Space");

        PressKey(0x20);

        await Task.Delay(
            TimeSpan.FromSeconds(
                ActionDelaySeconds),
            cancellationToken);

        // 2. Тест быстродействия
        Console.WriteLine(
            "2. Кликаем «Тест быстродействия»");

        ClickRelative(
            windowHandle,
            MenuX,
            MenuY);

        await Task.Delay(
            TimeSpan.FromSeconds(
                ActionDelaySeconds),
            cancellationToken);

        // 3. Подтвердить
        Console.WriteLine(
            "3. Кликаем «Подтвердить»");

        ClickRelative(
            windowHandle,
            ConfirmX,
            ConfirmY);

        await Task.Delay(
            TimeSpan.FromSeconds(
                ActionDelaySeconds),
            cancellationToken);

        Console.WriteLine(
            "Автоматический запуск теста завершён.");

        gameProcess.Dispose();
    }

    private static async Task<Process> WaitForGameProcessAsync(
        CancellationToken cancellationToken)
    {
        Stopwatch timer =
            Stopwatch.StartNew();

        while (timer.Elapsed < TimeSpan.FromSeconds(30))
        {
            cancellationToken.ThrowIfCancellationRequested();

            Process[] processes =
                Process.GetProcessesByName(
                    "b1-Win64-Shipping");

            foreach (Process process in processes)
            {
                try
                {
                    if (process.MainWindowHandle != IntPtr.Zero)
                    {
                        return process;
                    }
                }
                catch
                {
                    process.Dispose();
                }
            }

            await Task.Delay(
                500,
                cancellationToken);
        }

        throw new TimeoutException(
            "Не удалось найти окно b1-Win64-Shipping.");
    }

    private static void ClickRelative(
        IntPtr windowHandle,
        double relativeX,
        double relativeY)
    {
        if (!GetClientRect(
                windowHandle,
                out RECT rect))
        {
            throw new InvalidOperationException(
                "Не удалось получить размер окна игры.");
        }

        int width =
            rect.Right - rect.Left;

        int height =
            rect.Bottom - rect.Top;

        int x =
            (int)(width * relativeX);

        int y =
            (int)(height * relativeY);

        POINT point = new()
        {
            X = x,
            Y = y
        };

        if (!ClientToScreen(
                windowHandle,
                ref point))
        {
            throw new InvalidOperationException(
                "Не удалось получить экранные координаты.");
        }

        Console.WriteLine(
            $"Клик: {relativeX:P1}, {relativeY:P1} " +
            $"→ X={point.X}, Y={point.Y}");

        SetCursorPos(
            point.X,
            point.Y);

        Thread.Sleep(100);

        MouseClick();
    }

    private static void MouseClick()
    {
        INPUT[] inputs =
        {
            new INPUT
            {
                type = INPUT_MOUSE,
                union = new InputUnion
                {
                    mouse = new MOUSEINPUT
                    {
                        dwFlags =
                            MOUSEEVENTF_LEFTDOWN
                    }
                }
            },

            new INPUT
            {
                type = INPUT_MOUSE,
                union = new InputUnion
                {
                    mouse = new MOUSEINPUT
                    {
                        dwFlags =
                            MOUSEEVENTF_LEFTUP
                    }
                }
            }
        };

        SendInput(
            (uint)inputs.Length,
            inputs,
            Marshal.SizeOf<INPUT>());
    }

    private static void PressKey(
        ushort virtualKey)
    {
        INPUT[] inputs =
        {
            new INPUT
            {
                type = INPUT_KEYBOARD,
                union = new InputUnion
                {
                    keyboard = new KEYBDINPUT
                    {
                        wVk = virtualKey
                    }
                }
            },

            new INPUT
            {
                type = INPUT_KEYBOARD,
                union = new InputUnion
                {
                    keyboard = new KEYBDINPUT
                    {
                        wVk = virtualKey,
                        dwFlags = KEYEVENTF_KEYUP
                    }
                }
            }
        };

        SendInput(
            (uint)inputs.Length,
            inputs,
            Marshal.SizeOf<INPUT>());
    }
}