// <copyright file="HubPublishTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace HubConnection.Tests.Unit;

using HubConnection.Tests.TestDoubles;
using IndTrace.Domain.Entities;
using IndTrace.Domain.Models;
using IndTrace.HubConnection.Abstractions;
using IndTrace.HubConnection.Extensions;
using IndTrace.HubConnection.Contracts;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;

public class HubPublishTests
{
    private readonly TestLogger<object> logger = new();
    private readonly DateTimeMachine dateTimeMachine = new();

    [Fact]
    public async Task PublishCommandToHubAsync_Should_Publish_Valid_Request()
    {
        // Arrange
        var factory = new TestHubConnectionFactory();
        var connection = (TestHubConnection)await factory.CreateAsync(TestContext.Current.CancellationToken);
        await connection.StartAsync(TestContext.Current.CancellationToken);

        var request = new TaskGatewayRequest
        {
            MachineId = 1001,
            PartNumber = "REQ-001"
        };

        // Act
        await connection.PublishCommandToHubAsync(request, factory, dateTimeMachine, logger, TestContext.Current.CancellationToken);

        // Assert
        request.TimeStamp.ShouldBeGreaterThan(DateTime.MinValue);
        var sent = connection.GetAllSentMessages();
        var msg = sent.FirstOrDefault(m => m.method == HubMethods.BroadcastTaskGatewayRequest);
        msg.method.ShouldBe(HubMethods.BroadcastTaskGatewayRequest);
        msg.args.Length.ShouldBe(2);
        ((int)msg.args[0]!).ShouldBe(request.MachineId);
        msg.args[1].ShouldBeOfType<TaskGatewayRequest>();
    }

    [Fact]
    public async Task PublishResultsToHubAsync_Should_Publish_Successful_Result()
    {
        // Arrange
        var factory = new TestHubConnectionFactory();
        var connection = (TestHubConnection)await factory.CreateAsync(TestContext.Current.CancellationToken);
        await connection.StartAsync(TestContext.Current.CancellationToken);

        var request = new TaskGatewayRequest { MachineId = 2002, PartNumber = "REQ-2002" };
        var response = new TaskGatewayResponseDto { MachineId = request.MachineId, PartNumber = request.PartNumber };
        var result = Result<TaskGatewayResponseDto>.Success(response);

        // Act
        await connection.PublishResultsToHubAsync(request, result, factory, dateTimeMachine, logger, TestContext.Current.CancellationToken);

        // Assert
        var sent = connection.GetAllSentMessages();
        var resp = sent.FirstOrDefault(m => m.method == HubMethods.BroadcastTaskGatewayResponse);
        resp.method.ShouldBe(HubMethods.BroadcastTaskGatewayResponse);
        resp.args.Length.ShouldBe(2);
        ((int)resp.args[0]!).ShouldBe(request.MachineId);
        resp.args[1].ShouldBeOfType<TaskGatewayResponseDto>();
        // No error broadcasts expected
        sent.Count(m => m.method == HubMethods.BroadcastMessageToClients).ShouldBe(0);
    }

    [Fact]
    public async Task PublishResultsToHubAsync_Should_Publish_Failed_Result_With_Errors()
    {
        // Arrange
        var factory = new TestHubConnectionFactory();
        var connection = (TestHubConnection)await factory.CreateAsync(TestContext.Current.CancellationToken);
        await connection.StartAsync(TestContext.Current.CancellationToken);

        var request = new TaskGatewayRequest { MachineId = 3003, PartNumber = "REQ-3003" };
        var response = new TaskGatewayResponseDto { MachineId = request.MachineId, PartNumber = request.PartNumber };
        var result = Result<TaskGatewayResponseDto>.WithFailure(new[] { "E1", "E2" }, response);

        // Act
        await connection.PublishResultsToHubAsync(request, result, factory, dateTimeMachine, logger, TestContext.Current.CancellationToken);

        // Assert
        var sent = connection.GetAllSentMessages();
        var resp = sent.FirstOrDefault(m => m.method == HubMethods.BroadcastTaskGatewayResponse);
        resp.method.ShouldBe(HubMethods.BroadcastTaskGatewayResponse);
        resp.args.Length.ShouldBe(2);
        ((int)resp.args[0]!).ShouldBe(request.MachineId);
        resp.args[1].ShouldBeOfType<TaskGatewayResponseDto>();

        // Error messages also broadcasted
        sent.Count(m => m.method == HubMethods.BroadcastMessageToClients).ShouldBeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task LogAndSendMessageFromControllerAsync_Should_Log_And_Send_Message()
    {
        // Arrange
        var factory = new TestHubConnectionFactory();
        var connection = (TestHubConnection)await factory.CreateAsync(TestContext.Current.CancellationToken);
        await connection.StartAsync(TestContext.Current.CancellationToken);

        const string msg = "Controller says hello";

        // Act
        await connection.LogAndSendMessageFromControllerAsync(msg, logger, factory, TestContext.Current.CancellationToken);

        // Assert
        logger.HasMessage(msg).ShouldBeTrue();
        var sent = connection.GetAllSentMessages();
        var bcast = sent.FirstOrDefault(m => m.method == HubMethods.BroadcastMessageToClients);
        bcast.method.ShouldBe(HubMethods.BroadcastMessageToClients);
        bcast.args.Length.ShouldBe(2);
        ((string)bcast.args[0]!).ShouldBe("Gateway");
        ((string)bcast.args[1]!).ShouldContain("Controller says hello");
    }
}
