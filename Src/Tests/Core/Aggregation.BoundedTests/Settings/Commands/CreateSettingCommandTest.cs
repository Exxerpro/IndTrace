// <copyright file="CreateSettingCommandTest.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.Settings.Commands;
/// <summary>
/// Represents the CreateSettingCommandTest.
/// </summary>

public class CreateSettingCommandTest : DependenciesFactory
{
    //[Fix]
    //CLAUDE
    //Date: 09/09/2025
    //Reason: [Constructor Pattern] - Added ITestContextAccessor parameter to match DependenciesFactory signature
    public CreateSettingCommandTest(ITestOutputHelper outputHelper) : base(outputHelper)
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

        var request = new CreateSettingCommand()
        {
            MachineId = 100,
            Setting = "Ubicacion",
        };

        // Act
        var result = await dispatcher.ProcessAsync(request, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<SettingCreatedEvent>();
        result.Value.MachineId.ShouldBe(request.MachineId);
        result.Value.Setting.ShouldBe(request.Setting);
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

        // #95 Slice C: the Setting write now goes through the Machine aggregate repository.
        var logger = XUnitLogger.CreateLogger<CreateSettingCommandHandler>();

        // Set deterministic time
        DpDateTimeMachine.SetDateTimeNow(new DateTimeOffset(2020, 06, 06, 06, 06, 06, 6, TimeSpan.Zero));

        var request = new CreateSettingCommand()
        {
            MachineId = 200,
            Setting = "Ubicacion",
        };

        var sut = new CreateSettingCommandHandler(DpMachineAggregateRepository, DpMachineRepository, logger);

        // Act
        var result = await sut.ProcessAsync(request, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<SettingCreatedEvent>();
        result.Value.MachineId.ShouldBe(request.MachineId);
        result.Value.Setting.ShouldBe(request.Setting);
    }

    /// <summary>
    /// Diagnostic test to capture actual failure details
    /// </summary>
    [Fact]
    public async Task DiagnosticTest_CaptureFailureDetails()
    {
        await Initialization;

        // Arrange

        var dispatcher = DpMonitorRequestDispatcher;
        DpDateTimeMachine.SetDateTimeNow(new DateTimeOffset(2020, 06, 06, 06, 06, 06, 6, TimeSpan.Zero));

        var request = new CreateSettingCommand()
        {
            SettingId = 0, // Explicitly set to test identity behavior
            MachineId = 100,
            Setting = "DiagnosticTest",
        };

        // Act
        var result = await dispatcher.ProcessAsync(request, cancellationToken: TestContext.Current.CancellationToken);

        // Detailed logging for diagnosis
        DpLogger.LogInformation($"Result.IsSuccess: {result.IsSuccess}");
        if (result.Errors != null)
        {
            DpLogger.LogInformation($"Errors Count: {result.Errors.Count()}");
            foreach (var error in result.Errors)
            {
                DpLogger.LogInformation($"Error: {error}");
            }
        }
        DpLogger.LogInformation($"Result.Value is null: {result.Value == null}");

        // This test is for diagnostic purposes - we want to see what actually happens
        if (!result.IsSuccess)
        {
            DpLogger.LogInformation("Test failed as expected - this is for diagnostic purposes");
        }
    }
}