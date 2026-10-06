// <copyright file="MonitorRequestDispatcherTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Serilog;

namespace IndTrace.Aggregation.BoundedTests.Mediatr;
/// <summary>
/// Represents the MonitorRequestDispatcherTests.
/// </summary>

public class MonitorRequestDispatcherTests
{
    /// <summary>
    /// Represents the HomeItem.
    /// </summary>
    public class HomeItem : IItem
    {
    }

    public interface IItem
    {
    }
    /// <summary>
    /// Represents the CreateItemCommand.
    /// </summary>

    public class CreateItemCommand<T> : IMonitorRequest<T> where T : IItem, new()
    {
    }
    /// <summary>
    /// Represents the CreateItemCommandHandler.
    /// </summary>

    public class CreateItemCommandHandler<T> : IMonitorRequestHandler<CreateItemCommand<T>, T> where T : IItem, new()
    {
        /// <summary>
        /// Executes ProcessAsync operation.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellationToken.</param>
        /// <returns>The result of ProcessAsync.</returns>
        public Task<Result<T>> ProcessAsync(CreateItemCommand<T> request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<T>.Success(new T()));
        }
    }

    private readonly Microsoft.Extensions.Logging.ILogger _logger;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public MonitorRequestDispatcherTests()
    {
        // Use console output instead of TestOutput sink for xUnit v3
        var seriLogger = new LoggerConfiguration()
            .MinimumLevel.Verbose()

            .CreateLogger()
            .ForContext<MonitorRequestDispatcherTests>();

        _logger = new LoggerFactory()

            .CreateLogger<MonitorRequestDispatcherTests>();
    }
    /// <summary>
    /// Executes ExampleUsage operation.
    /// </summary>

    [Fact]
    public void ExampleUsage()
    {
        // Use ILogger as you normally would. These messages will show up in the console
        _logger.LogInformation("Test output to Serilog!");

        Action sketchy = () => throw new Exception("I threw up.");
        var exception = Record.Exception(sketchy);

        _logger.LogError(exception, "Here is an error.");
        exception.ShouldNotBeNull();
    }
}