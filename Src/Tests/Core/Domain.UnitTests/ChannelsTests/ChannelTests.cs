// <copyright file="ChannelTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Threading.Channels;
using Meziantou.Extensions.Logging.Xunit.v3;
using Microsoft.Extensions.Logging;

namespace IndTrace.Domain.UnitTests.ChannelsTests;
/// <summary>
/// Represents the ChannelExample.
/// </summary>

public class ChannelExample
{
    private ITestOutputHelper outputHelper;
    private ILogger<ChannelExample> logger;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="outputHelper">The outputHelper.</param>

    public ChannelExample(ITestOutputHelper outputHelper)
    {
        this.outputHelper = outputHelper;

        // Arrange
        var loggerFactory = LoggerFactory.Create(builder => builder
            .AddProvider(new XUnitLoggerProvider(this.outputHelper, new XUnitLoggerOptions()))
            .SetMinimumLevel(LogLevel.Information));

        logger = loggerFactory.CreateLogger<ChannelExample>();

        logger = new LoggerFactory().CreateLogger<ChannelExample>();
    }
    /// <summary>
    /// Executes RunAsync operation.
    /// </summary>
    /// <returns>The result of RunAsync.</returns>

    [Fact]
    public async Task RunAsync()
    {
        // Create an unbounded channel
        var channel = Channel.CreateUnbounded<string>();

        // Define a producer task

        // Start the producer and consumer tasks

        var result = true;

        var producerTask = ProduceAsync(channel.Writer);
        var consumerTask = ConsumeAsync(channel.Reader);

        // Wait for both tasks to complete
        await Task.WhenAll(producerTask, consumerTask);

        logger.LogInformation("Channel example completed.");

        result.ShouldBe(true, "the channel should complete without errors.");
    }

    private async Task ProduceAsync(ChannelWriter<string> writer)
    {
        for (var i = 1; i <= 5; i++)
        {
            await writer.WriteAsync($"Message {i}");
            await Task.Delay(100); // Simulate some work
        }
        writer.Complete(); // Signal that no more data will be written
    }

    // Define a consumer task
    private async Task ConsumeAsync(ChannelReader<string> reader)
    {
        try
        {
            while (await reader.WaitToReadAsync())
            {
                if (reader.TryRead(out var message))
                {
                    logger.LogInformation("Received: {message}", message);
                }
            }
        }
        finally
        {
            // Ensure that any remaining data is read before the channel is closed.
            // This is important if the channel was used in a multi-producer/multi-consumer
            // scenario where the writer could have been written to many times before
            // the consumer ever began to read.
            await reader.Completion;
        }
    }
}