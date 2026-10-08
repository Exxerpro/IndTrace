// <copyright file="Program.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Communications;

using Serilog;

/// <summary>
/// Represents the Program.
/// </summary>
public partial class Program
{
    /// <summary>
    /// Registers the edition's device drivers. Implemented in <c>Program.Enterprise.cs</c> by the enterprise
    /// edition; in the community edition the call compiles away and the community defaults apply.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The host configuration.</param>
    static partial void AddEditionDevices(IServiceCollection services, IConfiguration configuration);

    /// <summary>
    /// Registers the edition's device background workers, after the gateway worker manager so the host start
    /// order is unchanged. Implemented in <c>Program.Enterprise.cs</c> by the enterprise edition.
    /// </summary>
    /// <param name="services">The service collection.</param>
    static partial void AddEditionHostedServices(IServiceCollection services);

    /// <summary>
    /// Executes Main operation.
    /// </summary>
    /// <param name="args">The args.</param>
    /// <returns>The result of Main.</returns>
    public static async Task Main(string[] args)
    {
        // Load externalized, centralized config
        var configuration = ConfigLoader.Load();

        var logger = Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSerilog(); // Use the already configured Serilog instance
        });

        Microsoft.Extensions.Logging.ILogger msLogger = loggerFactory.CreateLogger("Runners");

        var windowTitle = Runners.EnsureProgramIsSingleton(msLogger);

        Runners.EnsureProgramIsHighPriority(msLogger);

        var builder = Host.CreateApplicationBuilder(args);

        //[Fix]
        //CLAUDE
        //Date: 22/06/2026
        //Reason: [DI scope validation - finding D5] Enable boot-time container validation on this production
        //host. ValidateOnBuild forces every registered service to be resolvable at Build() (so a missing
        //gateway-handler dependency fails fast at startup instead of on the first PLC dispatch), and
        //ValidateScopes detects captive (singleton-captures-scoped) dependencies. HostApplicationBuilder has no
        //UseDefaultServiceProvider, so configure the DefaultServiceProviderFactory directly.
        ((IHostApplicationBuilder)builder).ConfigureContainer(
            new DefaultServiceProviderFactory(new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true,
            }));

        builder.Configuration.AddConfiguration(configuration);

        // Set console title here
        Console.Title = windowTitle;

        var logFilePath = configuration["Serilog:WriteTo:2:Args:path"]; // Adjust index if needed
        Log.Information("Serilog is logging to file: {LogFilePath}", logFilePath);

        Log.Information("Logging configured");

        try
        {
            Log.Information("Starting web host");

            // Add services to the container
            // Add Serilog to the DI container
            // Ensure the logger is available throughout the app
            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(logger);

            // Also add Console and Debug providers to see output in development
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            // Configure DbContext
            var connectionStringApp = builder.Configuration.GetConnectionString("IndTraceDbContext");
            if (string.IsNullOrEmpty(connectionStringApp))
            {
                Log.Information("Could not find the 'IndTraceDbContext' connection string.");
                throw new InvalidOperationException("Could not find the 'IndTraceDbContext' connection string.");
            }

            // The whole gateway composition lives in AddGatewayHostServices so a test can build it under the same
            // ValidateOnBuild/ValidateScopes options as this host. The edition hooks are passed in as callbacks.
            builder.Services.AddGatewayHostServices(
                builder.Configuration,
                connectionStringApp,
                (services, configuration) => AddEditionDevices(services, configuration),
                services => AddEditionHostedServices(services));

            var host = builder.Build();
            await host.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
            Console.ReadLine();
        }
        finally
        {
            await Log.CloseAndFlushAsync();
            Console.ReadLine();
        }
    }
}
