// <copyright file="ResultFailureFactory.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Models.RequestHandler;

/// <summary>
/// Builds a failed <see cref="Result"/> or <see cref="Result{T}"/> for an <b>open-generic</b>
/// pipeline response type.
/// </summary>
/// <remarks>
/// <para>
/// The CQRS dispatchers and the validation/pipeline behaviors are open generic over the response
/// type, so a given <c>TResponse</c> is only ever <see cref="Result"/> or <see cref="Result{T}"/>,
/// but the concrete <c>T</c> is not known at compile time. This factory constructs the correct
/// failed value <b>without</b> the fragile <c>(TResponse)(object)Result.WithFailure(...)</c> cast —
/// that cast throws <see cref="InvalidCastException"/> whenever <c>TResponse</c> is a
/// <see cref="Result{T}"/>, replacing the real failure with a crash.
/// </para>
/// <para>
/// For <see cref="Result{T}"/> the failed value is built through a <c>nameof</c>-bound, exact-overload
/// reflective lookup (no magic strings, no null-forgiving). Any lookup miss fails <b>closed</b> by
/// throwing, so a mistake surfaces as a diagnostic failure rather than silently proceeding.
/// </para>
/// </remarks>
internal static class ResultFailureFactory
{
    /// <summary>
    /// Builds a failed response of type <typeparamref name="TResponse"/> carrying a single error.
    /// </summary>
    /// <typeparam name="TResponse">The pipeline response type (<see cref="Result"/> or <see cref="Result{T}"/>).</typeparam>
    /// <param name="error">The error message.</param>
    /// <returns>A failed <typeparamref name="TResponse"/>.</returns>
    public static TResponse Create<TResponse>(string error)
        => Create<TResponse>(string.IsNullOrEmpty(error) ? Array.Empty<string>() : new[] { error });

    /// <summary>
    /// Builds a failed response of type <typeparamref name="TResponse"/> carrying the supplied errors.
    /// </summary>
    /// <typeparam name="TResponse">The pipeline response type (<see cref="Result"/> or <see cref="Result{T}"/>).</typeparam>
    /// <param name="errors">The error messages.</param>
    /// <returns>A failed <typeparamref name="TResponse"/>.</returns>
    public static TResponse Create<TResponse>(IEnumerable<string> errors)
    {
        var messages = errors as string[] ?? errors?.ToArray() ?? Array.Empty<string>();
        var responseType = typeof(TResponse);

        // Non-generic Result: direct, compile-checked construction.
        if (responseType == typeof(Result))
        {
            return (TResponse)(object)Result.WithFailure(messages);
        }

        // Result<T>: the concrete inner type is only known at runtime. Build the failed value via an
        // exact-overload, nameof-bound lookup. A miss fails closed (throws) rather than fail-open.
        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var innerType = responseType.GetGenericArguments()[0];
            var withFailure = responseType.GetMethod(
                nameof(Result.WithFailure),
                BindingFlags.Public | BindingFlags.Static,
                binder: null,
                types: new[] { typeof(string[]), innerType, typeof(Exception) },
                modifiers: null);

            if (withFailure is not null)
            {
                // Array.CreateInstance yields default(innerType) boxed (null for reference types,
                // zero-initialized for value types) — a valid "no value" for the failed Result<T>.
                var defaultValue = Array.CreateInstance(innerType, 1).GetValue(0);
                var failed = withFailure.Invoke(null, new object?[] { messages, defaultValue, null });
                if (failed is TResponse typedFailure)
                {
                    return typedFailure;
                }
            }

            throw new InvalidOperationException(
                $"Could not construct a failed Result<{innerType.Name}> from the supplied errors.");
        }

        throw new InvalidOperationException(
            $"ResultFailureFactory cannot build a failure for non-Result response type {responseType.Name}.");
    }
}
