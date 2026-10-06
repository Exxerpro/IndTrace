// <copyright file="EventLoggerBehavior.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Models.Behaviors;

/// <summary>
/// Pipeline behavior for logging requests and responses in the pipeline.
/// </summary>
/// <typeparam name="TRequest">Type of the request being processed.</typeparam>
/// <typeparam name="TResponse">Type of the response produced.</typeparam>
public class EventLoggerBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<EventLoggerBehavior<TRequest, TResponse>> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventLoggerBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public EventLoggerBehavior(ILogger<EventLoggerBehavior<TRequest, TResponse>> logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// Processes the incoming request and logs the request and response events.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <param name="next">Delegate for the next action in the pipeline.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response of the action.</returns>
    public async Task<TResponse> HandleAsync(TRequest request, RequestFunctionalHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {

        // Log the handling event.
        this.logger.LogInformation("Handling {RequestType}", typeof(TRequest).Name);

        // Await the next action in the pipeline.
        var response = await next().ConfigureAwait(false);

        // Log the handled event.
        this.logger.LogInformation("Handled {ResponseType}", typeof(TResponse).Name);

        return response;
    }
}