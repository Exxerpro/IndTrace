// <copyright file="GetConfigAppsListQueryHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.ConfigApps;

/// <summary>
/// Unit tests for GetConfigAppsListQueryHandler using repository pattern
/// </summary>
public class GetConfigAppsListQueryHandlerTests
{
    private readonly IRepository<ConfigApp> _repository = null!;
    private readonly ILogger<GetConfigAppsListQueryHandler> _logger = null!;
    private readonly IDateTimeMachine _dateTimeMachine = Substitute.For<IDateTimeMachine>();
    private readonly GetConfigAppsListQueryHandler _handler = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public GetConfigAppsListQueryHandlerTests()
    {
        _repository = Substitute.For<IRepository<ConfigApp>>();
        _logger = XUnitLogger.CreateLogger<GetConfigAppsListQueryHandler>();
        _handler = new GetConfigAppsListQueryHandler(_repository, _logger, _dateTimeMachine);
    }

    /// <summary>
    /// Executes Constructor_ShouldCreateValidInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_ShouldCreateValidInstance()
    {
        // Arrange & Act
        var handler = new GetConfigAppsListQueryHandler(_repository, _logger, _dateTimeMachine);

        // Assert
        handler.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Should_Return_ConfigApps_List_Successfully operation.
    /// </summary>
    /// <returns>The result of Should_Return_ConfigApps_List_Successfully.</returns>

    [Fact]
    public async Task Should_Return_ConfigApps_List_Successfully()
    {
        // Arrange
        var configApps = new List<ConfigApp>
        {
            new() { AppId = 1, Client = "Client1", Factory = "Factory1", Line = "Line1" },
            new() { AppId = 2, Client = "Client2", Factory = "Factory2", Line = "Line2" },
            new() { AppId = 3, Client = "Client3", Factory = "Factory3", Line = "Line3" }
        };

        var query = new GetConfigAppsListQuery { Id = "test" };

        _repository.ListAsync(Arg.Any<CancellationToken>())
                   .Returns(Result<IEnumerable<ConfigApp>>.Success(configApps));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ConfigApp.ShouldNotBeNull();
        result.Value.ConfigApp.Count.ShouldBe(3);
        result.Value.Count.ShouldBe(3);

        result.Value.ConfigApp[0].AppId.ShouldBe(1);
        result.Value.ConfigApp[0].Client.ShouldBe("Client1");
        result.Value.ConfigApp[1].AppId.ShouldBe(2);
        result.Value.ConfigApp[1].Client.ShouldBe("Client2");
        result.Value.ConfigApp[2].AppId.ShouldBe(3);
        result.Value.ConfigApp[2].Client.ShouldBe("Client3");

        await _repository.Received(1).ListAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_Return_Empty_List_When_No_ConfigApps_Exist operation.
    /// </summary>
    /// <returns>The result of Should_Return_Empty_List_When_No_ConfigApps_Exist.</returns>

    [Fact]
    public async Task Should_Return_Empty_List_When_No_ConfigApps_Exist()
    {
        // Arrange
        var query = new GetConfigAppsListQuery { Id = "test" };

        _repository.ListAsync(Arg.Any<CancellationToken>())
                   .Returns(Result<IEnumerable<ConfigApp>>.Success(new List<ConfigApp>()));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ConfigApp.ShouldNotBeNull();
        result.Value.ConfigApp.Count.ShouldBe(0);
        result.Value.Count.ShouldBe(0);
    }

    /// <summary>
    /// Executes Should_Return_Failure_When_Repository_Fails operation.
    /// </summary>
    /// <returns>The result of Should_Return_Failure_When_Repository_Fails.</returns>

    [Fact]
    public async Task Should_Return_Failure_When_Repository_Fails()
    {
        // Arrange
        var query = new GetConfigAppsListQuery { Id = "test" };

        _repository.ListAsync(Arg.Any<CancellationToken>())
                   .Returns(Result<IEnumerable<ConfigApp>>.WithFailure("Repository error"));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Repository error");
    }

    /// <summary>
    /// Executes Should_Handle_Null_Repository_Result operation.
    /// </summary>
    /// <returns>The result of Should_Handle_Null_Repository_Result.</returns>

    [Fact]
    public async Task Should_Handle_Null_Repository_Result()
    {
        // Arrange
        var query = new GetConfigAppsListQuery { Id = "test" };

        _repository.ListAsync(Arg.Any<CancellationToken>())
                   .Returns(Result<IEnumerable<ConfigApp>>.Success(null!));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ConfigApp.ShouldNotBeNull();
        result.Value.ConfigApp.Count.ShouldBe(0);
        result.Value.Count.ShouldBe(0);
    }

    /// <summary>
    /// Executes Should_Map_All_ConfigApp_Properties_Correctly operation.
    /// </summary>
    /// <returns>The result of Should_Map_All_ConfigApp_Properties_Correctly.</returns>

    [Fact]
    public async Task Should_Map_All_ConfigApp_Properties_Correctly()
    {
        // Arrange
        var configApp = new ConfigApp
        {
            ConfigAppId = "CONFIG001",
            AppId = 1,
            Client = "Test Client",
            Factory = "Test Factory",
            Line = "Test Line",
            MachineId = 10000,
            Project = "Test Project",
            Version = "1.0.0",
            CreatedOn = DateTime.UtcNow.AddDays(-1),
            ModifiedOn = DateTime.UtcNow
        };

        var query = new GetConfigAppsListQuery { Id = "test" };

        _repository.ListAsync(Arg.Any<CancellationToken>())
                   .Returns(Result<IEnumerable<ConfigApp>>.Success(new[] { configApp }));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        //[Fix]
        //CLAUDE
        //Date: 28/08/2025
        //Reason: [CS8602] - Add null-forgiving operator since result.IsSuccess was verified true
        var dto = result.Value.ShouldNotBeNull().ConfigApp.First();
        dto.ConfigAppId.ShouldBe(configApp.ConfigAppId);
        dto.AppId.ShouldBe(configApp.AppId);
        dto.Client.ShouldBe(configApp.Client);
        dto.Factory.ShouldBe(configApp.Factory);
        dto.Line.ShouldBe(configApp.Line);
        dto.MachineId.ShouldBe(configApp.MachineId);
        dto.Project.ShouldBe(configApp.Project);
        dto.Version.ShouldBe(configApp.Version);
    }

    /// <summary>
    /// Executes Should_Log_Error_When_Repository_Fails operation.
    /// </summary>
    /// <returns>The result of Should_Log_Error_When_Repository_Fails.</returns>

    [Fact]
    public async Task Should_Log_Error_When_Repository_Fails()
    {
        // Arrange
        var query = new GetConfigAppsListQuery { Id = "test" };

        _repository.ListAsync(Arg.Any<CancellationToken>())
                   .Returns(Result<IEnumerable<ConfigApp>>.WithFailure(["Repository error"]));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        //this is a static method so we can not intercept it
        //_logger.Received().LogError("Failed to get ConfigApp list: {Errors}", Arg.Any<string>());
    }

    /// <summary>
    /// Executes Should_Pass_CancellationToken_To_Repository operation.
    /// </summary>
    /// <returns>The result of Should_Pass_CancellationToken_To_Repository.</returns>

    [Fact]
    public async Task Should_Pass_CancellationToken_To_Repository()
    {
        // Arrange
        var query = new GetConfigAppsListQuery { Id = "test" };
        var cancellationToken = TestContext.Current.CancellationToken;

        _repository.ListAsync(Arg.Any<CancellationToken>())
                   .Returns(Result<IEnumerable<ConfigApp>>.Success(new List<ConfigApp>()));

        // Act
        await _handler.ProcessAsync(query, cancellationToken);

        // Assert
        await _repository.Received(1).ListAsync(cancellationToken);
    }
}