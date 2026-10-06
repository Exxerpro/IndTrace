// <copyright file="InstanceActivator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Dependencies.Helpers
{
    /// <summary>
    /// Provides methods to activate or bring an existing process window to the foreground.
    /// </summary>
    public static class InstanceActivator
    {
        // SW_SHOWNORMAL: restores and activates the target window.
        private const int SwShowNormal = 1;

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        /// <summary>
        /// Attempts to bring the window of an existing process with the specified name to the foreground.
        /// </summary>
        /// <param name="processName">The name of the process whose window should be activated.</param>
        /// <remarks>
        /// Platform selection is performed at RUNTIME via <see cref="OperatingSystem"/> — the previous
        /// <c>#if WINDOWS</c>/<c>#elif LINUX</c> blocks were never compiled (neither symbol is ever
        /// defined), so the method was a silent no-op and its bodies referenced members that do not exist.
        /// </remarks>
        public static void TryActivateWindow(string processName)
        {
            if (string.IsNullOrWhiteSpace(processName))
            {
                return;
            }

            try
            {
                if (OperatingSystem.IsWindows())
                {
                    ActivateWindowsWindow(processName);
                }
                else if (OperatingSystem.IsLinux())
                {
                    ActivateLinuxWindow(processName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WindowActivator] Could not bring window to front: {ex.Message}");
            }
        }

        private static void ActivateWindowsWindow(string processName)
        {
            var current = Process.GetCurrentProcess();
            foreach (var process in Process.GetProcessesByName(processName))
            {
                if (process.Id != current.Id && process.MainWindowHandle != IntPtr.Zero)
                {
                    ShowWindow(process.MainWindowHandle, SwShowNormal);
                    SetForegroundWindow(process.MainWindowHandle);
                    return;
                }
            }
        }

        private static void ActivateLinuxWindow(string processName)
        {
            using var _ = Process.Start("xdotool", $"search --name \"{processName}\" windowactivate");
        }
    }
}
