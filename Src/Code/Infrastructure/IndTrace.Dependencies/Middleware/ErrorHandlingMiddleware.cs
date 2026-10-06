// <copyright file="ErrorHandlingMiddleware.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Dependencies.Middleware;

using System.Net;
using System.Text.Json;
using IndTrace.Application.Models.Exceptions;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Middleware for handling exceptions and returning appropriate error responses.
/// </summary>
/// <remarks>
/// Issue #88: relocated out of <c>IndTrace.Application</c> (Core) into the infrastructure/composition layer so
/// ASP.NET types no longer bleed into Core. The logger is now a proper per-instance DI dependency (was a
/// process-wide <c>static</c> field), and the response-shaping boolean logic uses short-circuit <c>&amp;&amp;</c>.
/// </remarks>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<ErrorHandlingMiddleware> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorHandlingMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger for error logging.</param>
    public ErrorHandlingMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    /// <summary>
    /// Invokes the middleware to handle the HTTP context.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await this.next(context);
        }
        catch (Exception ex)
        {
            await this.HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        int statusCode;
        object? errors = null;

        if (exception is RestException re)
        {
            statusCode = (int)re.Code;

            if (re.Message is string message)
            {
                errors = new[] { message };
            }
        }
        else
        {
            statusCode = (int)HttpStatusCode.InternalServerError;
            errors = "An internal server error has occured.";
        }

        this.logger.LogError(exception, "Error occurred - Errors: {Errors} - Source: {Source} - TargetSite: {TargetSite}",
            errors, exception.Source, exception.TargetSite?.Name);

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(new { errors }));
    }
}
