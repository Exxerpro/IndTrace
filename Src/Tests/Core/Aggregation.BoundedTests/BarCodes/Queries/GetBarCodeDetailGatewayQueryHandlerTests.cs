// <copyright file="GetBarCodeDetailGatewayQueryHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.BarCodes.Queries;
/// <summary>
/// Represents the GetBarCodeDetailGatewayQueryHandlerTests.
/// </summary>

public class GetBarCodeDetailGatewayQueryHandlerTests : DependenciesFactory
{
    public GetBarCodeDetailGatewayQueryHandlerTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    /// <summary>
    /// Executes GetBarCodeDetailGatewayQueryHandler_ShouldProcessCommand_AndReturnTaskGatewayResponse operation.
    /// </summary>
    /// <returns>The result of GetBarCodeDetailGatewayQueryHandler_ShouldProcessCommand_AndReturnTaskGatewayResponse.</returns>

    [Fact]
    public async Task GetBarCodeDetailGatewayQueryHandler_ShouldProcessCommand_AndReturnTaskGatewayResponse()
    {
        await Initialization;

        // ArrangeU||
        var logger = XUnitLogger.CreateLogger<GetBarCodeDetailGatewayQueryHandlerTests>();
        logger.LogInformation("Starting test: GetBarCodeDetailGatewayQueryHandler_ShouldProcessCommand_AndReturnTaskGatewayResponse");

        var mockMediator = DpMonitorRequestDispatcher;

        var sut = new GetBarCodeDetailGatewayQueryHandler(DpBarCodeDetailsLoader);

        var machineId = 100;
        var barCode = "L1AL100003232372501";
        var command = new ReadBarCodeQuery();
        command.WithData(TaskGatewayRequest.CreateWithLabel(machineId, barCode));

        // Act
        var result = await sut.ProcessAsync(command, cancellationToken: TestContext.Current.CancellationToken);
        result.Value.ShouldNotBeNull();

        // Assert
        result.Value.ShouldBeOfType<TaskGatewayResponseDto>();
    }
}