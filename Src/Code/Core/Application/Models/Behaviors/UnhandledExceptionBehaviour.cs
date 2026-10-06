// <copyright file="UnhandledExceptionBehaviour.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Models.Behaviors;

/// <summary>
/// Pipeline behavior for logging unhandled exceptions during request processing.
/// </summary>
/// <typeparam name="TRequest">Type of the request being processed.</typeparam>
/// <typeparam name="TResponse">Type of the response produced.</typeparam>
public class UnhandledExceptionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<TRequest> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnhandledExceptionBehaviour{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="logger">Logger to log unhandled exceptions.</param>
    public UnhandledExceptionBehaviour(ILogger<TRequest> logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// Processes unhandled exceptions and logs them.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <param name="next">Delegate for the next action in the pipeline.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response of the action.</returns>
    public async Task<TResponse> HandleAsync(TRequest request, RequestFunctionalHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            var requestName = typeof(TRequest).Name;
            logger.LogError(ex, "IndTrace App Request: Unhandled Exception for Request {Name} {@Request}", requestName, request);

            var responseType = typeof(TResponse);

            // Handle Result<T> types
            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var result = CreateGenericResultFailure(responseType, $"Unhandled exception: {ex.Message}");
                return (TResponse)result;
            }

            // Handle non-generic Result type
            if (responseType == typeof(Result))
            {
                var result = Result.WithFailure($"Unhandled exception: {ex.Message}");
                return (TResponse)(object)result;
            }

            // Handle value types - return default value
            if (responseType.IsValueType)
            {
                var instance = Activator.CreateInstance(responseType)
                    ?? throw new InvalidOperationException($"Could not create a default value for value type '{responseType.Name}'.");
                return (TResponse)instance;
            }

            // Every response flowing through this CQRS pipeline is Result / Result<T> (handled above) or a value
            // type (handled above). A non-Result reference type has no valid degraded response, so fail fast with a
            // descriptive diagnostic rather than returning a null-forgiving `default(TResponse)!` that would only
            // surface as a NullReferenceException downstream. (No response type in the codebase reaches this path.)
            throw new InvalidOperationException($"Cannot create error result for response type: {responseType.Name}");
        }
    }

    private static readonly MethodInfo BuildFailureMethod =
        typeof(UnhandledExceptionBehaviour<TRequest, TResponse>)
            .GetMethod(nameof(BuildFailure), BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException($"Private static '{nameof(BuildFailure)}' method not found on UnhandledExceptionBehaviour.");

    //[Fix]
    //CLAUDE
    //Date: 20/06/2026
    //Reason: [Result<T> -> IndQuestResults migration] - the prior code located a WithFailure overload by exact
    //        arity (a 1-parameter (string) or 2-parameter (string, T) signature). After the migration WithFailure
    //        carries optional trailing parameters (string, T value = default, Exception = default), so it matched
    //        NEITHER check and this method threw at line 122 — turning the global exception middleware itself into
    //        a throwing path instead of degrading to a Result failure. Bind T reflectively, then call the SAME
    //        strongly-typed Result<T>.WithFailure(string) the rest of the codebase uses, so the binding can never
    //        drift from the package's overload set again.
    private static object CreateGenericResultFailure(Type resultType, string message)
    {
        var genericArg = resultType.GetGenericArguments()[0];
        var builder = BuildFailureMethod.MakeGenericMethod(genericArg);
        return builder.Invoke(null, new object[] { message })
            ?? throw new InvalidOperationException($"BuildFailure invocation for '{genericArg.Name}' returned null.");
    }

    private static Result<T> BuildFailure<T>(string message) => Result<T>.WithFailure(message);
}

