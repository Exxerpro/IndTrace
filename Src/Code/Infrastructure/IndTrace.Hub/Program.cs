// <copyright file="Program.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Hub.Server
{
    using Serilog;

    using IndTrace.HubConnection.Extensions;
    using IndTrace.HubConnection.Dashboard;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Represents the Program.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Executes Main operation.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns>The result of Main.</returns>
        public static async Task Main(string[] args)
        {

            // Load externalized, centralized config
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var logger = Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddSerilog(); // Use the already configured Serilog instance
            });
            Microsoft.Extensions.Logging.ILogger msLogger = loggerFactory.CreateLogger("Runners");

            var windowTitle = Runners.EnsureProgramIsSingleton(msLogger);

            var builder = WebApplication.CreateBuilder(args);

            // Set console title here
            Console.Title = windowTitle;

            // Create Serilog logger
            var logFilePath = configuration["Serilog:WriteTo:1:Args:path"]; // Adjust index if needed
            Log.Information("Serilog is logging to file: {LogFilePath}", logFilePath);

            // Set up logging
            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(logger, dispose: true);

            // Also add Console and Debug providers to see output in development
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            // Use Kestrel server options from configuration
            builder.WebHost.UseKestrel((context, options) =>
            {
                // Load Kestrel configuration from appsettings.json
                options.Configure(context.Configuration.GetSection("Kestrel"));
            });

            builder.Services.Configure<WorkerHubServerOptions>(configuration.GetSection("WorkerHubServer"));
            builder.Services.AddSingleton<IndTrace.Domain.Interfaces.IDateTimeMachine, IndTrace.Domain.Models.DateTimeMachine>();
            builder.Services.AddHostedService<WorkerHubServer>();
            //[Fix]
            //CLAUDE
            //Date: 23/06/2026
            //Reason: [Hub payload size] Default SignalR caps received messages at 32 KB, which rejects
            //        larger gateway/diagnostic payloads. A 1 MB application payload exceeds a 1 MB cap
            //        once SignalR JSON framing is added, so allow 2 MB of headroom (covers the 1 MB
            //        large-message integration test and real payload needs).
            // [Fix]
            // CLAUDE
            // Date: 23/07/2026
            // Reason: [#188] Gateway broadcast payloads (TaskGatewayRequest/TaskGatewayResponseDto) carry domain
            //         types default System.Text.Json cannot round-trip: the write-once Register (#39) and
            //         BarCodeLabel fail server-side argument binding with NotSupportedException, and EnumModel
            //         smart enums silently corrupt to Invalid(-1). Register the shared domain converter set on
            //         the payload serializer; HubConnectionFactory applies the SAME configuration client-side.
            builder.Services.AddSignalR(options =>
            {
                options.MaximumReceiveMessageSize = 2 * 1024 * 1024;
            })
            .AddJsonProtocol(o => IndTrace.HubConnection.Protocols.IndTraceHubJsonProtocol.Configure(o.PayloadSerializerOptions));
            // Register hub connection abstractions for client to upstream hub
            builder.Services.AddHubConnectionAbstractions(configuration);

            logger.Information("Starting to build services");

            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(logger, dispose: true);

            var app = builder.Build();
            app.MapHub<EventMonitorHub>("/EventMonitor");

            // Map dashboard endpoints for ADR metrics exposure
            app.MapHubMetricsEndpoints();

            // Lightweight health endpoints for hub and metrics
            app.MapGet("/health/hub", (IndTrace.Domain.Interfaces.IDateTimeMachine dateTimeMachine) => Results.Ok(new { Status = "Healthy", Timestamp = dateTimeMachine.GetUtcNow() }))
               .WithName("HubHealth");
            // TODO: Implement IHubMetricsDashboard interface
            // app.MapGet("/health/metrics", async (IHubMetricsDashboard dashboard, CancellationToken ct) =>
            // {
            //     var health = await dashboard.GetHealthStatusAsync(ct).ConfigureAwait(false);
            //     return Results.Ok(health);
            // }).WithName("HubMetricsHealth");

            app.UseDeveloperExceptionPage();

            app.UseRouting();

            Log.Information("Starting web host");
            await app.RunAsync();
        }
    }
}
