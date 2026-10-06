// <copyright file="CreateConfigAppCommandTest.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.ConfigApplication.Commands.Create;

namespace IndTrace.Aggregation.BoundedTests.ConfigApp.Commands;
/// <summary>
/// Represents the CreateConfigAppCommandTest.
/// </summary>

public class CreateConfigAppCommandTest : DependenciesFactory
{
    //[Fix]
    //CLAUDE
    //Date: 09/09/2025
    //Reason: [Constructor Pattern] - Added ITestContextAccessor parameter to match DependenciesFactory signature
    public CreateConfigAppCommandTest(ITestOutputHelper outputHelper) : base(outputHelper)
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

        var dispatcher = DpMonitorRequestDispatcher;

        // Set deterministic time for test
        DpDateTimeMachine.SetDateTimeNow(new DateTimeOffset(2020, 06, 06, 06, 06, 06, 6, TimeSpan.Zero));

        var request = new CreateConfigAppCommand()
        {
            MachineId = 100, // Fixed: Use existing machine ID from test data
            Client = "TestClient",
            Factorie = "TestPlant",
            Line = "TestLine",
            Project = "TestProject",
            Version = "1.0",
        };

        // Act
        var result = await dispatcher.ProcessAsync(request, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<ConfigAppCreated>();
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

        var repository = DpConfigAppRepository;
        var logger = XUnitLogger.CreateLogger<CreateConfigAppCommandHandler>();

        // Set deterministic time for test
        DpDateTimeMachine.SetDateTimeNow(new DateTimeOffset(2020, 06, 06, 06, 06, 06, 6, TimeSpan.Zero));

        var request = new CreateConfigAppCommand()
        {
            MachineId = 100, // Fixed: Use existing machine ID from test data
            Client = "Acme",
            Factorie = "1",
            Line = "Nissan",
            Project = "Nissan",
            Version = "1",
        };

        var sut = new CreateConfigAppCommandHandler(repository, logger);

        var result = await sut.ProcessAsync(request, cancellationToken: TestContext.Current.CancellationToken);

        //[Fix]
        //CLAUDE
        //Date: 09/09/2025
        //Reason: [Result Pattern] - Check IsSuccess before accessing Value to avoid NullReference
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue("Command processing should succeed");
        result.Value.ShouldNotBeNull();

        // Assert
        result.Value.ShouldBeOfType<ConfigAppCreated>();
        result.Value.MachineId.ShouldBe(request.MachineId);
        result.Value.Client.ShouldBe(request.Client);
        result.Value.Factory.ShouldBe(request.Factorie);
        result.Value.Line.ShouldBe(request.Line);
        result.Value.Project.ShouldBe(request.Project);
        result.Value.Version.ShouldBe(request.Version);
    }
}