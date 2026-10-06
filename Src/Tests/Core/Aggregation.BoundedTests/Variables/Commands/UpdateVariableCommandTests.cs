// <copyright file="UpdateVariableCommandTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.Variables.Commands;
/// <summary>
/// Represents the UpdateVariableCommandTests.
/// </summary>

public class UpdateVariableCommandTests : DependenciesFactory
{
    public UpdateVariableCommandTests(ITestOutputHelper outputHelper) : base(outputHelper)
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

        // Arrange

        // NO MOCKING: Use real DpMonitorRequestDispatcher for UI operations

        DpDateTimeMachine.SetDateTimeNow(new DateTimeOffset(2020, 06, 06, 06, 06, 06, 6, TimeSpan.Zero));
        var dateTime = DpDateTimeMachine;

        var response = new VariableDetailVm();

        //[Fix]
        //CLAUDE
        //Date: 09/09/2025
        //Reason: [Test Data Mismatch] - Updated to use existing VariableId and MachineId from test data
        var request = new UpdateVariableCommand()
        {
            VariableId = 1, // Use existing VariableId from Variables.json
            MachineId = 100, // Use existing MachineId from Variables.json
            Name = "UpdatedPartStatusPlc",
            Address = "DB257.W1",
            Type = "System.Int16"
        };

        // Act
        var result = await DpMonitorRequestDispatcher.ProcessAsync(request, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeOfType<Result<VariableDetailVm>>();
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

        // DELETED: Mock repository declaration
        var logger = XUnitLogger.CreateLogger<UpdateVariableCommandHandler>();

        DpDateTimeMachine.SetDateTimeNow(new DateTimeOffset(2020, 06, 06, 06, 06, 06, 6, TimeSpan.Zero));
        DpDateTimeMachine.SetDateTimeNow(DateTime.Now);
        var dateTime = DpDateTimeMachine;

        var existingVariable = new Variable
        {
            VariableId = 1,
            MachineId = 400,
            Name = "ExistingVariable",
            Address = "DB1.DBX0.0",
            NetType = "BOOL"
        };

        var request = new UpdateVariableCommand()
        {
            VariableId = 1,
            MachineId = 400,
            Name = "UpdatedVariable",
            Address = "DB1.DBX0.1",
            Type = "BOOL"
        };

        // Setup repository mock responses

        var sut = new UpdateVariableCommandHandler(DpVariablesRepository, DpVariablesGroupRepository, logger);

        // Act
        var result = await sut.ProcessAsync(request, cancellationToken: TestContext.Current.CancellationToken);
        result.Value.ShouldNotBeNull();

        // Assert

        result.Value.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeOfType<VariableDetailVm>();
        result.Value.VariableId.ShouldBeEquivalentTo(request.VariableId);
        result.Value.MachineId.ShouldBeEquivalentTo(request.MachineId);
        result.Value.Name.ShouldBe(request.Name);
        result.Value.Address.ShouldBe(request.Address);
        result.Value.NetType.ShouldBe(request.Type);

        // Verify repository interactions
    }
}