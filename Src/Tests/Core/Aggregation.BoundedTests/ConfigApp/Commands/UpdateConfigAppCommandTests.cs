// <copyright file="UpdateConfigAppCommandTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.ConfigApplication.Commands.Update;

namespace IndTrace.Aggregation.BoundedTests.ConfigApp.Commands;
/// <summary>
/// Represents the UpdateConfigAppCommandTests.
/// </summary>

public class UpdateConfigAppCommandTests : DependenciesFactory
{
    // Removed: DpLogger - using Meziantou logging instead
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="base(outputHelper">The base(outputHelper.</param>

    //[Fix]
    //CLAUDE
    //Date: 09/09/2025
    //Reason: [Constructor Pattern] - Added ITestContextAccessor parameter to match DependenciesFactory signature
    public UpdateConfigAppCommandTests(ITestOutputHelper outputHelper) : base(outputHelper)
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

        // Arrange - NO MOCKING: Use real DependenciesFactory for UI operations

        DpDateTimeMachine.SetDateTimeNow(new DateTimeOffset(2020, 06, 06, 06, 06, 06, 6, TimeSpan.Zero));

        var response = new ConfigAppDto();

        var request = new UpdateConfigAppCommand()
        {
            AppId = 1,
            Client = "Consulta",
            Factory = "Acme",
            Line = "Nissan",
            Project = "Nissan",
            Version = "1"
        };

        // REMOVED: NSubstitute setup - using real dispatcher

        // Act
        var result = await DpMonitorRequestDispatcher.ProcessAsync(request, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeOfType<Result<ConfigAppDto>>();
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

        var logger = XUnitLogger.CreateLogger<UpdateConfigAppCommandHandler>();

        var existingConfigApp = new IndTrace.Domain.Entities.ConfigApp
        {
            AppId = 1,
            Client = "ExistingClient",
            Factory = "ExistingFactory",
            Line = "ExistingLine",
            Project = "ExistingProject",
            Version = "1.0"
        };

        var request = new UpdateConfigAppCommand()
        {
            AppId = 1,
            Client = "Consulta",
            Factory = "Acme",
            Line = "Nissan",
            Project = "Nissan",
            Version = "1"
        };

        var sut = new UpdateConfigAppCommandHandler(DpConfigAppRepository, logger);

        // Act
        var result = await sut.ProcessAsync(request, cancellationToken: TestContext.Current.CancellationToken);
        result.Value.ShouldNotBeNull();

        // Assert

        result.Value.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeOfType<ConfigAppDto>();
        result.Value.AppId.ShouldBeEquivalentTo(request.AppId);
        result.Value.Client.ShouldBe(request.Client);
        result.Value.Factory.ShouldBe(request.Factory);
        result.Value.Line.ShouldBe(request.Line);
        result.Value.Project.ShouldBe(request.Project);
        result.Value.Version.ShouldBe(request.Version);
    }
}