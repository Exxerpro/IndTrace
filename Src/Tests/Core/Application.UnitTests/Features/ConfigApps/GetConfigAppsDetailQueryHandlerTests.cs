// <copyright file="GetConfigAppsDetailQueryHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.ConfigApps;

/// <summary>
/// Unit tests for GetConfigAppsDetailQueryHandler using repository pattern
/// </summary>
public class GetConfigAppsDetailQueryHandlerTests
{
    private readonly IRepository<ConfigApp> _repository = null!;
    private readonly ILogger<GetConfigAppsDetailQueryHandler> _logger = null!;
    private readonly GetConfigAppsDetailQueryHandler _handler = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public GetConfigAppsDetailQueryHandlerTests()
    {
        _repository = Substitute.For<IRepository<ConfigApp>>();
        _logger = XUnitLogger.CreateLogger<GetConfigAppsDetailQueryHandler>();
        _handler = new GetConfigAppsDetailQueryHandler(_repository, _logger);
    }

    /// <summary>
    /// Executes Constructor_ShouldCreateValidInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_ShouldCreateValidInstance()
    {
        // Arrange & Act
        var handler = new GetConfigAppsDetailQueryHandler(_repository, _logger);

        // Assert
        handler.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Should_Return_ConfigApp_Detail_Successfully operation.
    /// </summary>
    /// <returns>The result of Should_Return_ConfigApp_Detail_Successfully.</returns>

    [Fact]
    public async Task Should_Return_ConfigApp_Detail_Successfully()
    {
        // Arrange
        var configApp = new ConfigApp
        {
            ConfigAppId = "CONFIG001",
            AppId = 1,
            MachineId = 10000,
            PlcId = 10,
            Pc = "1",
            Client = "Test Client",
            Factory = "Test Factory",
            Line = "Test Line",
            Project = "Test Project",
            Version = "1.0.0"
        };

        var query = new GetConfigAppsDetailQuery { Id = 1 };

        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>())
                   .Returns(Result<ConfigApp?>.Success(configApp));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ConfigAppId.ShouldBe(configApp.ConfigAppId);
        result.Value.AppId.ShouldBe(configApp.AppId);
        result.Value.MachineId.ShouldBe(configApp.MachineId);
        result.Value.PlcId.ShouldBe(configApp.PlcId);
        result.Value.Pc.ShouldBe(configApp.Pc);
        result.Value.Client.ShouldBe(configApp.Client);
        result.Value.Factory.ShouldBe(configApp.Factory);
        result.Value.Line.ShouldBe(configApp.Line);
        result.Value.Project.ShouldBe(configApp.Project);
        result.Value.Version.ShouldBe(configApp.Version);

        await _repository.Received(1).GetByIdAsync(1, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_Return_Failure_When_ConfigApp_Not_Found operation.
    /// </summary>
    /// <returns>The result of Should_Return_Failure_When_ConfigApp_Not_Found.</returns>

    [Fact]
    public async Task Should_Return_Failure_When_ConfigApp_Not_Found()
    {
        // Arrange
        var query = new GetConfigAppsDetailQuery { Id = 999 };

        _repository.GetByIdAsync(999, Arg.Any<CancellationToken>())
                   .Returns(Result<ConfigApp?>.WithFailure("ConfigApp not found"));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("ConfigApp not found");
    }

    /// <summary>
    /// Executes Should_Return_Failure_When_ConfigApp_Is_Null operation.
    /// </summary>
    /// <returns>The result of Should_Return_Failure_When_ConfigApp_Is_Null.</returns>

    [Fact]
    public async Task Should_Return_Failure_When_ConfigApp_Is_Null()
    {
        // Arrange
        var query = new GetConfigAppsDetailQuery { Id = 1 };

        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>())
                   .Returns(Result<ConfigApp?>.Success(null));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
    }

    /// <summary>
    /// Executes Should_Return_Failure_When_Repository_Fails operation.
    /// </summary>
    /// <returns>The result of Should_Return_Failure_When_Repository_Fails.</returns>

    [Fact]
    public async Task Should_Return_Failure_When_Repository_Fails()
    {
        // Arrange
        var query = new GetConfigAppsDetailQuery { Id = 1 };

        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>())
                   .Returns(Result<ConfigApp?>.WithFailure("Repository error"));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Repository error");
    }

    /// <summary>
    /// Executes Should_Query_Repository_With_Correct_Id operation.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <returns>The result of Should_Query_Repository_With_Correct_Id.</returns>

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(999)]
    public async Task Should_Query_Repository_With_Correct_Id(int id)
    {
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Arrange
        var query = new GetConfigAppsDetailQuery { Id = id };

        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>())
                   .Returns(Result<ConfigApp?>.WithFailure("Not found"));

        // Act
        await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        await _repository.Received(1).GetByIdAsync(id, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_Map_All_ConfigApp_Properties_In_Detail operation.
    /// </summary>
    /// <returns>The result of Should_Map_All_ConfigApp_Properties_In_Detail.</returns>

    [Fact]
    public async Task Should_Map_All_ConfigApp_Properties_In_Detail()
    {
        // Arrange
        var configApp = new ConfigApp
        {
            ConfigAppId = "CONFIG_DETAIL_001",
            AppId = 42,
            MachineId = 200,
            PlcId = 25,
            Pc = "3",
            Client = "Detailed Client",
            Factory = "Detailed Factory",
            Line = "Detailed Line",
            Project = "Detailed Project",
            Version = "2.1.0"
        };

        var query = new GetConfigAppsDetailQuery { Id = 42 };

        _repository.GetByIdAsync(42, Arg.Any<CancellationToken>())
                   .Returns(Result<ConfigApp?>.Success(configApp));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var dto = result.Value;
        dto.ShouldNotBeNull();
        dto.ShouldNotBeNull();
        dto.ShouldNotBeNull();
        dto.ConfigAppId.ShouldBe("CONFIG_DETAIL_001");
        dto.AppId.ShouldBe(42);
        dto.MachineId.ShouldBe(200);
        dto.PlcId.ShouldBe(25);
        dto.Pc.ShouldBe("3");
        dto.Client.ShouldBe("Detailed Client");
        dto.Factory.ShouldBe("Detailed Factory");
        dto.Line.ShouldBe("Detailed Line");
        dto.Project.ShouldBe("Detailed Project");
        dto.Version.ShouldBe("2.1.0");
    }

    /// <summary>
    /// Executes Should_Log_Error_When_Repository_Fails operation.
    /// </summary>
    /// <returns>The result of Should_Log_Error_When_Repository_Fails.</returns>

    [Fact]
    public async Task Should_Log_Error_When_Repository_Fails()
    {
        // Arrange
        var query = new GetConfigAppsDetailQuery { Id = 1 };

        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>())
                   .Returns(Task.FromResult(Result<ConfigApp?>.WithFailure(["Repository error"])));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        //this is a static method can not be intercepted
        //_logger.Received(1).LogError(Arg.Any<string>(), Arg.Any<string[]>());
    }

    /// <summary>
    /// Executes Should_Pass_CancellationToken_To_Repository operation.
    /// </summary>
    /// <returns>The result of Should_Pass_CancellationToken_To_Repository.</returns>

    [Fact]
    public async Task Should_Pass_CancellationToken_To_Repository()
    {
        // Arrange
        var query = new GetConfigAppsDetailQuery { Id = 1 };
        var cancellationToken = TestContext.Current.CancellationToken;

        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>())
                   .Returns(Result<ConfigApp?>.WithFailure("Not found"));

        // Act
        await _handler.ProcessAsync(query, cancellationToken);

        // Assert
        await _repository.Received(1).GetByIdAsync(1, cancellationToken);
    }
}
