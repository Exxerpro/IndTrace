// <copyright file="UpdatePLCCommandTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.PLCs.Commands;
/// <summary>
/// Represents the UpdatePLCCommandTests.
/// </summary>

public class UpdatePLCCommandTests : DependenciesFactory
{
    public UpdatePLCCommandTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    /// <summary>
    /// Executes ShouldSendRequestAsync operation.
    /// </summary>
    /// <returns>The result of ShouldSendRequestAsync.</returns>

    [Fact]
    public async Task ShouldSendRequestAsync()
    {
        await Initialization;

        // Arrange - NO MOCKING: Use real DpMonitorRequestDispatcher for UI operations

        DpDateTimeMachine.SetDateTimeNow(new DateTimeOffset(2020, 06, 06, 06, 06, 06, 6, TimeSpan.Zero));

        var request = new UpdatePlcCommand()
        {
            PlcId = 100
        };

        // Act - Use real dispatcher (no mocking)
        var result = await DpMonitorRequestDispatcher.ProcessAsync(request, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<Result<PlcDto>>();
    }

    /// <summary>
    /// Executes ShouldExecuteRequestHandler operation.
    /// </summary>
    /// <returns>The result of ShouldExecuteRequestHandler.</returns>

    [Fact]
    public async Task ShouldExecuteRequestHandler()
    {
        await Initialization;

        // Arrange

        var logger = XUnitLogger.CreateLogger<UpdatePlcCommandHandler>();

        var existingPlc = Plc.CreateFixture(
            plcId: 100,
            machineId: 0,
            enabled: ActiveStatus.None,
            name: "ExistingPLC",
            ipAddress: "192.168.1.100",
            plcType: "1200",
            plcBrand: "Siemens",
            options: string.Empty,
            commLibrary: "S7Link",
            brandOwner: "ExistingBrand");

        var request = new UpdatePlcCommand()
        {
            PlcId = 100,
            Name = "UpdatedPLC",
            BrandOwner = "UpdatedBrand",
            IpAddress = "192.168.1.101",
            PlcType = "1500",
            PlcBrand = "Siemens",
            CommLibrary = "S7CommPlus"
        };

        // Setup repository mock responses

        var sut = new UpdatePlcCommandHandler(DpPlcRepository, logger);

        // Act
        var result = await sut.ProcessAsync(request, cancellationToken: TestContext.Current.CancellationToken);
        result.Value.ShouldNotBeNull();

        // Assert

        result.Value.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeOfType<PlcDto>();
        result.Value.PlcId.ShouldBeEquivalentTo(request.PlcId);
        result.Value.Name.ShouldBe(request.Name);
        result.Value.BrandOwner.ShouldBe(request.BrandOwner);
        result.Value.IpAddress.ShouldBe(request.IpAddress);
        result.Value.PlcType.ShouldBe(request.PlcType);
        result.Value.PlcBrand.ShouldBe(request.PlcBrand);
        result.Value.CommLibrary.ShouldBe(request.CommLibrary);

        // Verify repository interactions
    }
}