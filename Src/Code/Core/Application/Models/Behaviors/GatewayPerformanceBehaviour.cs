// <copyright file="GatewayPerformanceBehaviour.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Models.Behaviors;

/// <summary>
/// Pipeline behavior for measuring performance of gateway commands and recording execution time on the response.
/// </summary>
/// <typeparam name="TRequest">The type of the incoming gateway request.</typeparam>
public class GatewayPerformanceBehaviour<TRequest, TResponse>(ILogger<TRequest> logger) : IPipelineBehavior<TRequest, Result<TaskGatewayResponseDto>>
    where TRequest : IGatewayRequest
{
    /// <summary>
    /// Logger instance for logging performance information.
    /// </summary>
    private readonly ILogger<TRequest> logger = logger;

    /// <summary>
    /// Threshold for considering a request as long-running.
    /// </summary>
    private readonly TimeSpan longExecutionTime = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// Handles the request, measures execution time, and records it on the response.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <param name="next">Delegate for the next action in the pipeline.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response of the action, with execution time recorded.</returns>
    public async Task<Result<TaskGatewayResponseDto>> HandleAsync(
        TRequest request,
        RequestFunctionalHandlerDelegate<Result<TaskGatewayResponseDto>> next,
        CancellationToken cancellationToken)
    {
        var startTime = Stopwatch.GetTimestamp();

        var response = await next().ConfigureAwait(false);

        var elapsedTime = Stopwatch.GetElapsedTime(startTime);

        if (response.Value is not null)
        {
            // #32 C2: immutable wire record — evolve with `with` before rebuilding the Result.
            var timed = response.Value with { ExecutionTime = elapsedTime };
            response = response.IsSuccess
                ? Result<TaskGatewayResponseDto>.Success(timed)
                : Result<TaskGatewayResponseDto>.WithFailure(response.Errors, timed);
        }
        else
        {
            // var result = new TaskGatewayResponseDto().MapFrom(request);
            response = Result<TaskGatewayResponseDto>.WithFailure("No response value found", null);
        }

        if (elapsedTime <= this.longExecutionTime)
        {
            this.logger.LogInformation(
                "IndTrace Performance Request: {Name} ({ElapsedMilliseconds} ms) {@Request}",
                typeof(TRequest).Name, elapsedTime, request);
        }
        else
        {
            this.logger.LogWarning(
                "IndTrace Performance Long Running Request: {Name} ({ElapsedMilliseconds} ms) {@Request}",
                typeof(TRequest).Name, elapsedTime, request);
        }

        return response;
    }
}