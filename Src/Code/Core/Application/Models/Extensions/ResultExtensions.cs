// <copyright file="ResultExtensions.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndQuestResults;

namespace IndTrace.Application.Models.Extensions;

/// <summary>
/// Extension methods for Result&lt;T&gt; to implement Railway-Oriented Programming patterns.
/// Enables fluent chaining of operations with automatic error propagation.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Chains two async operations where the second depends on the result of the first.
    /// If the first operation fails, the error is propagated without executing the second operation.
    /// </summary>
    /// <typeparam name="T">Type of the input result value</typeparam>
    /// <typeparam name="U">Type of the output result value</typeparam>
    /// <param name="result">The input result to bind from</param>
    /// <param name="func">Function to execute if the input result is successful</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Result of the chained operation or propagated error</returns>
    public static async Task<Result<U>> BindAsync<T, U>(
        this Task<Result<T>> result,
        Func<T, CancellationToken, Task<Result<U>>> func,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<U>.WithFailure("Operation was canceled.");
        }

        var awaitedResult = await result.ConfigureAwait(false);
        if (awaitedResult.IsFailure || awaitedResult.Value is null)
        {
            return Result<U>.WithFailure(awaitedResult.Errors);
        }

        return await func(awaitedResult.Value, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Chains two sync operations where the second depends on the result of the first.
    /// If the first operation fails, the error is propagated without executing the second operation.
    /// </summary>
    /// <typeparam name="T">Type of the input result value</typeparam>
    /// <typeparam name="U">Type of the output result value</typeparam>
    /// <param name="result">The input result to bind from</param>
    /// <param name="func">Function to execute if the input result is successful</param>
    /// <returns>Result of the chained operation or propagated error</returns>
    public static Result<U> Bind<T, U>(
        this Result<T> result,
        Func<T, Result<U>> func)
    {
        if (result.IsFailure || result.Value is null)
        {
            return Result<U>.WithFailure(result.Errors);
        }

        return func(result.Value);
    }

    /// <summary>
    /// Transforms the value of a successful result using an async function.
    /// If the input result is a failure, the error is propagated.
    /// </summary>
    /// <typeparam name="T">Type of the input result value</typeparam>
    /// <typeparam name="U">Type of the output result value</typeparam>
    /// <param name="result">The input result to map from</param>
    /// <param name="func">Function to transform the value</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Result with transformed value or propagated error</returns>
    public static async Task<Result<U>> MapAsync<T, U>(
        this Task<Result<T>> result,
        Func<T, Task<U>> func,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<U>.WithFailure("Operation was canceled.");
        }

        var awaitedResult = await result.ConfigureAwait(false);
        if (awaitedResult.IsFailure || awaitedResult.Value is null)
        {
            return Result<U>.WithFailure(awaitedResult.Errors);
        }

        try
        {
            var transformedValue = await func(awaitedResult.Value).ConfigureAwait(false);
            return Result<U>.Success(transformedValue);
        }
        catch (Exception ex)
        {
            return Result<U>.WithFailure($"Map operation failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Transforms the value of a successful result using a sync function.
    /// If the input result is a failure, the error is propagated.
    /// </summary>
    /// <typeparam name="T">Type of the input result value</typeparam>
    /// <typeparam name="U">Type of the output result value</typeparam>
    /// <param name="result">The input result to map from</param>
    /// <param name="func">Function to transform the value</param>
    /// <returns>Result with transformed value or propagated error</returns>
    public static Result<U> Map<T, U>(
        this Result<T> result,
        Func<T, U> func)
    {
        if (result.IsFailure || result.Value is null)
        {
            return Result<U>.WithFailure(result.Errors);
        }

        try
        {
            var transformedValue = func(result.Value);
            return Result<U>.Success(transformedValue);
        }
        catch (Exception ex)
        {
            return Result<U>.WithFailure($"Map operation failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes a side effect (like logging) if the result is successful.
    /// The original result is returned unchanged.
    /// </summary>
    /// <typeparam name="T">Type of the result value</typeparam>
    /// <param name="result">The input result</param>
    /// <param name="action">Action to execute if successful</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>The original result unchanged</returns>
    public static async Task<Result<T>> TapAsync<T>(
        this Task<Result<T>> result,
        Func<T, CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<T>.WithFailure("Operation was canceled.");
        }

        var awaitedResult = await result.ConfigureAwait(false);
        if (awaitedResult.IsSuccess && awaitedResult.Value is not null)
        {
            try
            {
                await action(awaitedResult.Value, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Log the tap error but don't affect the result
                // Could add logging here if needed
                _ = ex; // Suppress unused variable warning
            }
        }

        return awaitedResult;
    }

    /// <summary>
    /// Executes a side effect (like logging) if the result is successful.
    /// The original result is returned unchanged.
    /// </summary>
    /// <typeparam name="T">Type of the result value</typeparam>
    /// <param name="result">The input result</param>
    /// <param name="action">Action to execute if successful</param>
    /// <returns>The original result unchanged</returns>
    public static Result<T> Tap<T>(
        this Result<T> result,
        Action<T> action)
    {
        if (result.IsSuccess && result.Value is not null)
        {
            try
            {
                action(result.Value);
            }
            catch (Exception ex)
            {
                // Log the tap error but don't affect the result
                _ = ex; // Suppress unused variable warning
            }
        }

        return result;
    }

    /// <summary>
    /// Executes a side effect if the result is a failure.
    /// The original result is returned unchanged.
    /// </summary>
    /// <typeparam name="T">Type of the result value</typeparam>
    /// <param name="result">The input result</param>
    /// <param name="action">Action to execute on failure</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>The original result unchanged</returns>
    public static async Task<Result<T>> OnFailureAsync<T>(
        this Task<Result<T>> result,
        Func<IEnumerable<string>, CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<T>.WithFailure("Operation was canceled.");
        }

        var awaitedResult = await result.ConfigureAwait(false);
        if (awaitedResult.IsFailure)
        {
            try
            {
                await action(awaitedResult.Errors, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Log the failure handler error but don't affect the result
                _ = ex; // Suppress unused variable warning
            }
        }

        return awaitedResult;
    }

    /// <summary>
    /// Executes a side effect if the result is a failure.
    /// The original result is returned unchanged.
    /// </summary>
    /// <typeparam name="T">Type of the result value</typeparam>
    /// <param name="result">The input result</param>
    /// <param name="action">Action to execute on failure</param>
    /// <returns>The original result unchanged</returns>
    public static Result<T> OnFailure<T>(
        this Result<T> result,
        Action<IEnumerable<string>> action)
    {
        if (result.IsFailure)
        {
            try
            {
                action(result.Errors);
            }
            catch (Exception ex)
            {
                // Log the failure handler error but don't affect the result
                _ = ex; // Suppress unused variable warning
            }
        }

        return result;
    }

    /// <summary>
    /// Chains operations for Result&lt;T&gt; to Result (no value).
    /// Useful when the final operation doesn't return a value.
    /// </summary>
    /// <typeparam name="T">Type of the input result value</typeparam>
    /// <param name="result">The input result to bind from</param>
    /// <param name="func">Function to execute if the input result is successful</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Result of the chained operation or propagated error</returns>
    public static async Task<Result> BindAsync<T>(
        this Task<Result<T>> result,
        Func<T, CancellationToken, Task<Result>> func,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result.WithFailure("Operation was canceled.");
        }

        var awaitedResult = await result.ConfigureAwait(false);
        if (awaitedResult.IsFailure || awaitedResult.Value is null)
        {
            return Result.WithFailure(awaitedResult.Errors);
        }

        return await func(awaitedResult.Value, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Bridges a repository result that may carry a nullable value (<see cref="Result{T}"/> of <c>T?</c>)
    /// into a non-nullable <see cref="Result{T}"/>. Propagates existing failures, and converts a
    /// successful-but-null value into a failure carrying <paramref name="errorMessage"/>.
    /// </summary>
    /// <typeparam name="T">Reference type of the result value.</typeparam>
    /// <param name="task">The asynchronous repository result with a nullable value.</param>
    /// <param name="errorMessage">Failure message used when the value is null.</param>
    /// <returns>A non-nullable result, or a failure when the source failed or the value was null.</returns>
    public static async Task<Result<T>> RequireValue<T>(this Task<Result<T?>> task, string errorMessage)
        where T : class
    {
        var result = await task.ConfigureAwait(false);
        return result.IsFailure
            ? Result<T>.WithFailure(result.Errors)
            : result.Value is null
                ? Result<T>.WithFailure(errorMessage)
                : Result<T>.Success(result.Value);
    }
}