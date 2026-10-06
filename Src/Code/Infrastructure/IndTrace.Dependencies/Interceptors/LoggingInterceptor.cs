// <copyright file="LoggingInterceptor.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Dependencies.Interceptors;

/// <summary>
/// Intercepts method calls and logs before and after invocation for debugging purposes.
/// </summary>
public class LoggingInterceptor : IInterceptor
{
    /// <summary>
    /// Intercepts a method invocation, logging before and after the call.
    /// </summary>
    /// <param name="invocation">The method invocation information.</param>
    /// <summary>
    /// Executes Intercept operation.
    /// </summary>
    /// <param name="invocation">The invocation.</param>
    public void Intercept(IInvocation invocation)
    {
        Console.WriteLine($"[Before] Calling: {invocation.Method.Name}");

        invocation.Proceed(); // Proceed with the actual method

        Console.WriteLine($"[After] Done: {invocation.Method.Name}");
    }
}