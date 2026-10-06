// <copyright file="SingleInstanceEnforcer.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Dependencies.Helpers;

internal static class SingleInstanceEnforcer
{
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

    private const int SwRestore = 9;

    // The acquired single-instance mutex is retained for the lifetime of the process. Disposing it
    // (as the previous `using var` did) released the guard microseconds after acquisition, defeating
    // the single-instance protection entirely. Storing it in a static field keeps the handle — and
    // therefore the Global\ mutex ownership — alive until the process exits.
    private static Mutex? heldMutex;

    public static bool EnsureSingleInstance(string processName, ILogger logger)
    {
        string mutexId = $"Global\\{processName}";

        var mutex = new Mutex(true, mutexId, out var createdNew);

        if (createdNew)
        {
            // Retain ownership for the process lifetime; do NOT dispose.
            heldMutex = mutex;
            return true;
        }

        // We did not create it, so we do not own the guard: release our handle.
        mutex.Dispose();

        var currentProcess = Process.GetCurrentProcess();
        foreach (var process in Process.GetProcessesByName(processName))
        {
            if (process.Id != currentProcess.Id)
            {
                IntPtr hWnd = process.MainWindowHandle;
                if (hWnd != IntPtr.Zero)
                {
                    ShowWindowAsync(hWnd, SwRestore);
                    SetForegroundWindow(hWnd);
                    Console.WriteLine("Activated existing instance.");
                }
                break;
            }
        }

        return false;
    }
}