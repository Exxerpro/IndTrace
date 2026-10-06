// <copyright file="CreateMachineCommandTest.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.Machines.Commands;

/// <summary>
/// Represents the CreateMachineMonitorRequestTest.
/// TODO: TECH DEBT - File name (CreateMachineCommandTest.cs) doesn't match class name (CreateMachineMonitorRequestTest)
/// TODO: Consider renaming for consistency during next cleanup phase
/// TODO: Currently kept for EF Core diagnostic information from failing tests
/// </summary>

public class CreateMachineMonitorRequestTest : DependenciesFactory
{
    private readonly ILogger<CreateMachineMonitorRequestHandler> logger;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="base(output">The base(output.</param>

    public CreateMachineMonitorRequestTest(ITestOutputHelper output) : base(output)
    {
        logger = XUnitLogger.CreateLogger<CreateMachineMonitorRequestHandler>(output);
    }

    /// <summary>
    /// Executes ShouldThrowExceptionWhenMachineExists operation.
    /// </summary>
    /// <returns>The result of ShouldThrowExceptionWhenMachineExists.</returns>

    [Fact]
    public async Task ShouldThrowExceptionWhenMachineExists()
    {
        await Initialization;

        // NO MOCKING: Use real DpMonitorRequestDispatcher for UI operations
        DpDateTimeMachine.SetDateTimeNow(new DateTimeOffset(2020, 06, 06, 06, 06, 06, 6, TimeSpan.Zero));

        var request = new CreateMachineMonitorRequest
        {
            MachineId = 100,
            Name = "Station 60",
            Location = "Trunk",
            MachineType = 2,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        var sut = new CreateMachineMonitorRequestHandler(DpMachineRepository, logger);
        var result = await sut.ProcessAsync(request, cancellationToken: TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue("Because this machine already exist");

        result.Errors.ShouldContain("A machine with the same ID or name already exists.");
    }

    /// <summary>
    /// Executes ShouldThrowExceptionWhenMachineIdExists operation.
    /// </summary>
    /// <returns>The result of ShouldThrowExceptionWhenMachineIdExists.</returns>

    [Fact]
    public async Task ShouldThrowExceptionWhenMachineIdExists()
    {
        await Initialization;

        var request = new CreateMachineMonitorRequest
        {
            MachineId = 100,
            Name = "Station 61",
            Location = "Trunk",
            MachineType = 2,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        var sut = new CreateMachineMonitorRequestHandler(DpMachineRepository, logger);
        var result = await sut.ProcessAsync(request, cancellationToken: TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue("Because this machine already exist");
        result.Errors.ShouldContain("A machine with the same ID or name already exists.");
    }

    /// <summary>
    /// Executes ShouldThrowExceptionWhenMachineNameExists operation.
    /// </summary>
    /// <returns>The result of ShouldThrowExceptionWhenMachineNameExists.</returns>

    [Fact]
    public async Task ShouldThrowExceptionWhenMachineNameExists()
    {
        await Initialization;

        var request = new CreateMachineMonitorRequest
        {
            MachineId = 100,
            Name = "WS100",
            Location = "Trunk",
            MachineType = 2,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0
        };

        var sut = new CreateMachineMonitorRequestHandler(DpMachineRepository, logger);
        var result = await sut.ProcessAsync(request, cancellationToken: TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue("Because this machine already exist");
        result.Errors.ShouldContain("A machine with the same ID or name already exists.");
    }
}