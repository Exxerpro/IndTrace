// <copyright file="ValidationBehavior.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Models.Behaviors;

using ValidationException = FluentValidation.ValidationException;

/// <summary>
/// Pipeline behavior for validating incoming requests using registered validators.
/// </summary>
/// <typeparam name="TRequest">Type of the request being processed.</typeparam>
/// <typeparam name="TResponse">Type of the response produced.</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="validators">List of Fluent Validators for the TRequest type.</param>
    /// <param name="logger">Logger for diagnostic information.</param>
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators, ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        this.validators = validators;
        this.logger = logger;
    }

    /// <summary>
    /// Validates the request before passing it to the next delegate.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <param name="next">Delegate for the next action in the pipeline.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response of the action.</returns>
    public async Task<TResponse> HandleAsync(TRequest request, RequestFunctionalHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // If no validators are registered, proceed to next handler
        if (!this.validators.Any())
        {
            return await next().ConfigureAwait(false);
        }

        // Create validation context for the incoming request
        var context = new ValidationContext<TRequest>(request);

        // Run all validators asynchronously
        var validationResults = await Task.WhenAll(this.validators
            .Select(v => v.ValidateAsync(context, cancellationToken))).ConfigureAwait(false);

        // Collect all validation failures
        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null).ToList();

        // If there are validation errors, short-circuit the pipeline. The handler must NEVER run on
        // invalid input (no fail-open).
        if (failures.Count != 0)
        {
            var responseType = typeof(TResponse);
            var errorMessages = failures.Select(f => f.ErrorMessage).ToArray();

            // Result / Result<T>: surface the failure as a railway result (typed, no reflection cast
            // crash, no null-forgiving). The inner value type of Result<T> is resolved at runtime by
            // ResultFailureFactory.
            if (responseType == typeof(Result)
                || (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>)))
            {
                this.logger.LogWarning(
                    "ValidationBehavior short-circuited {RequestType}: {ErrorCount} validation error(s). Handler not invoked.",
                    typeof(TRequest).Name,
                    errorMessages.Length);

                return ResultFailureFactory.Create<TResponse>(errorMessages);
            }

            // Genuinely non-Result response: there is no railway channel to carry a failure, so fail
            // CLOSED. The handler is still not invoked; the ValidationException is converted to a
            // Result failure by the dispatcher's top-level catch.
            this.logger.LogWarning(
                "ValidationBehavior blocked non-Result response type {ResponseType} on validation failure. Handler not invoked.",
                responseType.Name);

            throw new ValidationException(failures);
        }

        // If no validation errors, proceed to next handler
        return await next().ConfigureAwait(false);
    }
}

