// <copyright file="CreateVariableCommandTest.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.Variables.Commands;
/// <summary>
/// Represents the CreateVariableCommandTest.
/// </summary>

public class CreateVariableCommandTest : DependenciesFactory
{
    //[Fix]
    //CLAUDE
    //Date: 09/09/2025
    //Reason: [Constructor Pattern] - Updated to use single-argument DependenciesFactory constructor
    public CreateVariableCommandTest(ITestOutputHelper outputHelper) : base(outputHelper)
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

        // Use real dispatcher from DependenciesFactory
        var dispatcher = DpMonitorRequestDispatcher;

        // Set deterministic time
        DpDateTimeMachine.SetDateTimeNow(new DateTimeOffset(2020, 06, 06, 06, 06, 06, 6, TimeSpan.Zero));

        var request = new CreateVariableCommand()
        {
            MachineId = 200,
            Name = "TorqueStatus",
            Address = "DB20.DBX20.0",
            Type = "Bool",
            Length = 1,
            Event = 0,
            Direction = 0,
            VariableGroupId = 1
        };

        // Act
        var result = await dispatcher.ProcessAsync(request, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<VariableCreatedEvent>();
        result.Value.MachineId.ShouldBe(request.MachineId);
        result.Value.Name.ShouldBe(request.Name);
        result.Value.Address.ShouldBe(request.Address);
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

        // Use real repository from DependenciesFactory
        var repository = DpVariablesRepository;
        var logger = XUnitLogger.CreateLogger<CreateVariableCommandHandler>();

        // Set deterministic time
        DpDateTimeMachine.SetDateTimeNow(new DateTimeOffset(2020, 06, 06, 06, 06, 06, 6, TimeSpan.Zero));

        var request = new CreateVariableCommand()
        {
            MachineId = 200,
            Name = "TorqueStatus",
            Address = "DB20.DBX20.0",
            Type = "Bool",
            Length = 1,
            Event = 0,
            Direction = 0,
            VariableGroupId = 1
        };

        var sut = new CreateVariableCommandHandler(repository, DpVariablesGroupRepository, logger);

        // Act
        var result = await sut.ProcessAsync(request, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<VariableCreatedEvent>();
        result.Value.MachineId.ShouldBe(request.MachineId);
        result.Value.Name.ShouldBe(request.Name);
        result.Value.Address.ShouldBe(request.Address);
        result.Value.Type.ShouldBe(request.Type);
        result.Value.Length.ShouldBe(request.Length);
        result.Value.Event.ShouldBe(request.Event);
        result.Value.Direction.ShouldBe(request.Direction);
        result.Value.VariableGroupId.ShouldBe(request.VariableGroupId);
    }
}