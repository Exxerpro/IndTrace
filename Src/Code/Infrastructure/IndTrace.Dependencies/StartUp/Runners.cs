// <copyright file="Runners.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Security.Principal;
using IndTrace.Dependencies.Helpers;

namespace IndTrace.Dependencies.Startup;

/// <summary>
/// Provides utility methods for process management, privilege elevation, and singleton enforcement.
/// </summary>
/// <remarks>
/// All methods are static and intended for use during application startup or process control scenarios.
/// </remarks>
public static class Runners
{
    /// <summary>
    /// Determines whether the current process is running with administrator privileges.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the current process is running as administrator; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method is Windows-specific and relies on <see cref="WindowsIdentity"/> and <see cref="WindowsPrincipal"/>.
    /// </remarks>
    public static bool IsRunningAsAdministrator()
    {
        if (!OperatingSystem.IsWindows())
            return false;

        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    /// <summary>
    /// Relaunches the current application with administrator privileges and exits the current instance.
    /// </summary>
    /// <remarks>
    /// If the user cancels the UAC prompt or an error occurs, the current process will still exit.
    /// </remarks>
    public static void RelaunchAsAdministrator()
    {
        var processInfo = new ProcessStartInfo
        {
            UseShellExecute = true,
            WorkingDirectory = Environment.CurrentDirectory,
            FileName = Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty,
            Verb = "runas",
        };

        try
        {
            Process.Start(processInfo);
        }
        catch (Exception ex)
        {
            Console.WriteLine("This application must be run as an administrator. \n\n" + ex.Message);
        }

        // Exit the current instance to prevent multiple instances from running
        Environment.Exit(0);
    }

    /// <summary>
    /// Ensures that only a single instance of the program is running, sets the console title, and attempts to activate any existing instance.
    /// </summary>
    /// <param name="logger">The logger to use for logging information and diagnostics.</param>
    /// <returns>The process name of the current application.</returns>
    /// <remarks>
    /// This method enforces singleton behavior and may terminate the process if another instance is detected.
    /// </remarks>
    public static string EnsureProgramIsSingleton(ILogger logger)
    {
        string processName = Process.GetCurrentProcess().ProcessName;

        Console.Title = processName;

        logger.LogInformation("Setting console title to: {Title}", processName);

        // Attempt to bring an already running instance to front (if exists)
        InstanceActivator.TryActivateWindow(processName);

        SingleInstanceEnforcer.EnsureSingleInstance(processName, logger);

        VerifyIfApplicationIsNotRunning(processName);

        return processName;
    }

    /// <summary>
    /// Sets the current process priority to <see cref="ProcessPriorityClass.High"/>.
    /// </summary>
    /// <param name="logger">The logger to use for logging information.</param>
    /// <remarks>
    /// This method may throw <see cref="System.ComponentModel.Win32Exception"/> if the priority cannot be set.
    /// </remarks>
    public static void EnsureProgramIsHighPriority(ILogger logger)
    {
        Process currentProcess = Process.GetCurrentProcess();

        currentProcess.PriorityClass = ProcessPriorityClass.High;
        logger.LogInformation("Setting process priority to High.");
    }

    /// <summary>
    /// Verifies that only one instance of the application is running; exits if another instance is found.
    /// </summary>
    /// <param name="name">The process name to check for running instances.</param>
    /// <remarks>
    /// If more than one process with the specified name is found, the current process will terminate.
    /// </remarks>
    public static void VerifyIfApplicationIsNotRunning(string name)
    {
        try
        {
            // Get the list of all processes by the name
            var runningProcesses = Process.GetProcessesByName(name);

            // If more than one instance is running, that means this one is not the first
            if (runningProcesses.Length <= 1) return;

            Console.WriteLine("An instance of the application is already running.");
            Console.WriteLine("This program will terminate now.");

            if (Debugger.IsAttached)
            {
                Console.WriteLine("Press [Enter] to exit...");
                Console.ReadLine();
                Environment.Exit(0);
            }
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking running processes: {ex.Message}");

            // Only block for interactive acknowledgement when a real console is attached; under a
            // process supervisor (input redirected) blocking here would hang the service forever.
            if (!Console.IsInputRedirected)
            {
                Console.WriteLine("Press [Enter] to exit...");
                Console.ReadLine();
            }

            // Non-zero: an error occurred, so supervisors must NOT treat this exit as a healthy shutdown.
            Environment.Exit(1);
        }
    }
}