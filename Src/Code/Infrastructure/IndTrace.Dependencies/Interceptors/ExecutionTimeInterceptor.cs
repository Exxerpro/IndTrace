// <copyright file="ExecutionTimeInterceptor.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Dependencies.Interceptors;
/// <summary>
/// Represents the ExecutionTimeInterceptor.
/// </summary>

public class ExecutionTimeInterceptor : IInterceptor
{
    private readonly ILogger<ExecutionTimeInterceptor> logger;
    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionTimeInterceptor"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="logger">The logger.</param>

    public ExecutionTimeInterceptor(ILogger<ExecutionTimeInterceptor> logger)
    {
        this.logger = logger;
    }
    /// <summary>
    /// Executes Intercept operation.
    /// </summary>
    /// <param name="invocation">The invocation.</param>

    public void Intercept(IInvocation invocation)
    {
        var method = invocation.MethodInvocationTarget ?? invocation.Method;
        var attr = method.GetCustomAttribute<MeasureExecutionTimeAttribute>();

        if (attr != null)
        {
            var label = attr.Label ?? $"{method.DeclaringType?.Name}.{method.Name}";
            var sw = Stopwatch.StartNew();

            try
            {
                invocation.Proceed();
            }
            finally
            {
                sw.Stop();
                this.logger.LogInformation("Interceptor Executed {Label} in {ElapsedMs}ms", label, sw.Elapsed.TotalMilliseconds);
            }
        }
        else
        {
            invocation.Proceed(); // bypass if not marked
        }
    }
}