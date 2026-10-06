// <copyright file="LocalDatabaseServiceStarter.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace IndTrace.Persistence.Services;
/// <summary>
/// Represents the LocalDatabaseServiceStarter.
/// </summary>

public class LocalDatabaseServiceStarter
{
    private readonly ILogger<LocalDatabaseServiceStarter> logger;
    private readonly string? connectionString;
    private readonly IConfiguration configuration;
    /// <summary>
    /// Initializes a new instance of the <see cref="LocalDatabaseServiceStarter"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="_configuration">The _configuration.</param>

    public LocalDatabaseServiceStarter(ILogger<LocalDatabaseServiceStarter> logger, IConfiguration _configuration)
    {
        this.logger = logger;
        this.configuration = _configuration;
        this.connectionString = this.configuration.GetConnectionString("DefaultConnection");
    }
    /// <summary>
    /// Executes StartIfLocal operation.
    /// </summary>

    public void StartIfLocal()
    {
        string localDbServiceName = "MSSQLSERVER";
        if (this.IsDatabaseLocal() && !this.IsServiceRunning(localDbServiceName))
        {
            var service = this.StartService(localDbServiceName);
        }
        else if (this.IsDatabaseLocal() && this.IsServiceRunning(localDbServiceName))
        {
            this.logger.LogInformation("Local database service {serviceName} is already running.", localDbServiceName);
        }
        else
        {
            this.logger.LogInformation("Database is not local or service is not applicable.");
        }
    }
    /// <summary>
    /// Executes IsDatabaseLocal operation.
    /// </summary>
    /// <returns>The result of IsDatabaseLocal.</returns>

    public bool IsDatabaseLocal()
    {
        var builder = new SqlConnectionStringBuilder(this.connectionString);
        string dataSource = builder.DataSource.ToLower();

        string machineName = Environment.MachineName.ToLower();

        var isLocalDataSource = dataSource == "localhost" || dataSource == "127.0.0.1" || dataSource.Contains("(local)") || dataSource == machineName;
        // Additionally, check if the data source contains the machine name
        if (dataSource.Contains("\\"))
        {
            var serverName = dataSource.Split('\\')[0];
            isLocalDataSource = isLocalDataSource || serverName == machineName;
        }

        return isLocalDataSource;
    }
    /// <summary>
    /// Executes IsServiceRunning operation.
    /// </summary>
    /// <param name="serviceName">The serviceName.</param>
    /// <returns>The result of IsServiceRunning.</returns>

    public bool IsServiceRunning(string serviceName)
    {
        this.logger.LogInformation("Checking if {ServiceName} service is running", serviceName);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return this.IsWindowsServiceRunning(serviceName);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return this.IsLinuxServiceRunning(serviceName);
        }
        else
        {
            this.logger.LogWarning("Unsupported operating system.");
            return false;
        }
    }
    /// <summary>
    /// Executes IsWindowsServiceRunning operation.
    /// </summary>
    /// <param name="serviceName">The serviceName.</param>
    /// <returns>The result of IsWindowsServiceRunning.</returns>

    public bool IsWindowsServiceRunning(string serviceName)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "sc",
                Arguments = $"query {serviceName}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = Process.Start(startInfo);
            if (process is null)
            {
                this.logger.LogInformation("Failed to start process for checking {ServiceName} service.", serviceName);
                return false;
            }
            process.WaitForExit();
            var output = process.StandardOutput.ReadToEnd();
            this.logger.LogInformation(output);

            return output.Contains("RUNNING");
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Exception occurred while checking {ServiceName} service on Windows", serviceName);
            return false;
        }
    }
    /// <summary>
    /// Executes IsLinuxServiceRunning operation.
    /// </summary>
    /// <param name="serviceName">The serviceName.</param>
    /// <returns>The result of IsLinuxServiceRunning.</returns>

    public bool IsLinuxServiceRunning(string serviceName)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "systemctl",
                Arguments = $"is-active {serviceName}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = Process.Start(startInfo);
            if (process is null)
            {
                this.logger.LogInformation("Failed to start process for checking {ServiceName} service.", serviceName);
                return false;
            }
            process.WaitForExit();
            var output = process.StandardOutput.ReadToEnd().Trim();
            this.logger.LogInformation(output);

            return output == "active";
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Exception occurred while checking {ServiceName} service on Linux", serviceName);
            return false;
        }
    }
    /// <summary>
    /// Executes StartService operation.
    /// </summary>
    /// <param name="serviceName">The serviceName.</param>
    /// <returns>The result of StartService.</returns>

    public async Task StartService(string serviceName)
    {
        this.logger.LogInformation("Starting local database service: {serviceName}", serviceName);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            await this.StartWindowsServiceAsync(serviceName);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            await this.StartLinuxServiceAsync(serviceName);
        }
        else
        {
            this.logger.LogWarning("Unsupported operating system.");
        }
    }
    /// <summary>
    /// Executes StartWindowsServiceAsync operation.
    /// </summary>
    /// <param name="serviceName">The serviceName.</param>
    /// <returns>The result of StartWindowsServiceAsync.</returns>

    public async Task StartWindowsServiceAsync(string serviceName)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "net",
                Arguments = $"start {serviceName}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            await this.ProcessStartAsync(serviceName, startInfo);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Exception occurred while starting {ServiceName} service on Windows", serviceName);
        }
    }
    /// <summary>
    /// Executes StartLinuxServiceAsync operation.
    /// </summary>
    /// <param name="serviceName">The serviceName.</param>
    /// <returns>The result of StartLinuxServiceAsync.</returns>

    public async Task StartLinuxServiceAsync(string serviceName)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "systemctl",
                Arguments = $"start {serviceName}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            await this.ProcessStartAsync(serviceName, startInfo);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Exception occurred while starting {ServiceName} service on Linux", serviceName);
        }
    }

    private async Task ProcessStartAsync(string serviceName, ProcessStartInfo startInfo)
    {
        using var process = Process.Start(startInfo);

        await Task.Delay(1000); // Wait for a second to ensure the process starts properly

        if (process is null)
        {
            this.logger.LogError("Failed to start process for starting {ServiceName} service.", serviceName);
            return;
        }
        await process.WaitForExitAsync();

        if (process.ExitCode == 0)
        {
            this.logger.LogInformation("{serviceName} service finished successfully.", serviceName);
        }
        else
        {
            var errorMsg = await process.StandardError.ReadToEndAsync();
            this.logger.LogError("Serviced {serviceName} failed with Exit code: {ExitCode}", serviceName, process.ExitCode);

            this.logger.LogError("error was {error}", errorMsg);
        }
    }
}