// <copyright file="LoggingBehavior.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Models.Behaviors;

/// <summary>
/// Pipeline behavior for logging requests and responses, including detailed information for specific response types.
/// </summary>
/// <typeparam name="TRequest">Type of the request being processed.</typeparam>
/// <typeparam name="TResponse">Type of the response produced.</typeparam>
public class LoggingBehavior<TRequest, TResponse>(ILogger<TRequest> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger logger = logger;

    /// <summary>
    /// Handles the request, logs request and response details, and calls the next handler in the pipeline.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <param name="next">Delegate for the next action in the pipeline.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response of the action.</returns>
    public async Task<TResponse> HandleAsync(TRequest request, RequestFunctionalHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        // Get the type name, handling generics appropriately
        var responseType = typeof(TResponse);
        var responseName = ResponseName(responseType);

        this.logger.LogInformation("IndTrace UI Request: {RequestName} of {ResponseName}", requestName, responseName);
        this.logger.LogInformation("IndTrace UI Request: {@Request}", request);

        // Call the next handler in the pipeline
        // Proceed with the next handler in the pipeline
        TResponse response = await next().ConfigureAwait(false);

        this.logger.LogInformation("IndTrace UI Request: {RequestName} of {ResponseName}", requestName, responseName);

        if (response is Result<ApplicationConfiguration>)
        {
            this.logger.LogDebug("IndTrace UI Response: {@Response}", response);
        }
        else if (response is Result<TaskGatewayResponseDto> { Value: not null } result)
        {
            this.logger.LogInformation("IndTrace UI Response:");
            this.logger.LogInformation("{@Response}", result.Value.ToString());
        }
        else if (response is Result<BarCodesListVm> { Value: not null } report)
        {
            this.logger.LogInformation("IndTrace UI Response: Result<BarCodesListVm>");
            this.logger.LogInformation(
                "IndTrace UI First Item: {@Response}",
                report.Value?.BarCodes?.FirstOrDefault()?.ToString() ?? string.Empty);
            this.logger.LogTrace("{@Response}", response);
        }

        return response;
    }

    private static string ResponseName(Type responseType)
    {
        string responseName;

        if (responseType.IsGenericType)
        {
            // ProcessAsync generic types like Result<T>
            var genericTypeDefinition = responseType.GetGenericTypeDefinition();
            var genericArguments = responseType.GetGenericArguments();
            var genericArgumentNames = string.Join(", ", genericArguments.Select(arg => arg.Name));

            // Format as Result<T>
            responseName = $"{genericTypeDefinition.Name.Split('`')[0]}<{genericArgumentNames}>";
        }
        else
        {
            // Non-generic types
            responseName = responseType.Name;
        }

        return responseName;
    }
}

