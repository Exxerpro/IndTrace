// <copyright file="CommonPipelineConfiguration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Serilog;
using ILogger = Serilog.ILogger;

namespace IndTrace.Dependencies.Middleware;

/// <summary>
/// Provides extension methods for configuring a common middleware pipeline for web applications.
/// </summary>
public static class CommonPipelineConfiguration
{
    /// <summary>
    /// Configures the application's middleware pipeline with common defaults such as HTTPS redirection, static files, authentication, and CORS.
    /// </summary>
    /// <param name="app">The web application instance.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="logger">The logger for logging configuration steps.</param>
    /// <returns>The configured <see cref="WebApplication"/> instance.</returns>
    public static WebApplication UseCommonPipeline(this WebApplication app, IConfiguration configuration, ILogger logger)
    {
        logger.Information("Starting to configure common the pipeline");

        app.UseHttpsRedirection();

        // SECURITY: the developer exception page leaks stack traces and request state. Restrict it to
        // Development; production keeps the framework default (no detailed error page is emitted).
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseStaticFiles();  // Ensure this is called only once

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseAntiforgery();

        app.UseCors("AllowAllOrigins");

        // SECURITY: the migrations endpoint exposes an EF schema-management surface. Restrict it to
        // Development so it is never reachable in production.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }


        logger.Information("common the pipeline configured");
        return app;
    }
}
