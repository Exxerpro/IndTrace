// <copyright file="CreateConfigAppCommandHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.ConfigApps;

/// <summary>
/// Unit tests for CreateConfigAppCommandHandler using repository pattern
/// </summary>
public class CreateConfigAppCommandHandlerTests
{
    private readonly IRepository<ConfigApp> _repository = null!;
    private readonly ILogger<CreateConfigAppCommandHandler> _logger = null!;
    private readonly CreateConfigAppCommandHandler _handler = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public CreateConfigAppCommandHandlerTests()
    {
        _repository = Substitute.For<IRepository<ConfigApp>>();
        _logger = XUnitLogger.CreateLogger<CreateConfigAppCommandHandler>();
        _handler = new CreateConfigAppCommandHandler(_repository, _logger);
    }

    /// <summary>
    /// Executes Constructor_ShouldCreateValidInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_ShouldCreateValidInstance()
    {
        // Arrange & Act
        var handler = new CreateConfigAppCommandHandler(_repository, _logger);

        // Assert
        handler.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Should_Create_ConfigApp_Successfully operation.
    /// </summary>
    /// <returns>The result of Should_Create_ConfigApp_Successfully.</returns>

    [Fact]
    public async Task Should_Create_ConfigApp_Successfully()
    {
        // Arrange
        var command = CreateValidCommand();

        _repository.AddAsync(Arg.Any<ConfigApp>(), Arg.Any<CancellationToken>())
                   .Returns(Result<int>.Success(1));
        _repository.CommitAsync(Arg.Any<CancellationToken>())
                   .Returns(Result.Success());

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.MachineId.ShouldBe(command.MachineId);
        result.Value.Client.ShouldBe(command.Client);
        result.Value.Factory.ShouldBe(command.Factorie);
        result.Value.Line.ShouldBe(command.Line);
        result.Value.Project.ShouldBe(command.Project);
        result.Value.Version.ShouldBe(command.Version);

        await _repository.Received(1).AddAsync(Arg.Any<ConfigApp>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_Return_Failure_When_Add_Fails operation.
    /// </summary>
    /// <returns>The result of Should_Return_Failure_When_Add_Fails.</returns>

    [Fact]
    public async Task Should_Return_Failure_When_Add_Fails()
    {
        // Arrange
        var command = CreateValidCommand();

        _repository.AddAsync(Arg.Any<ConfigApp>(), Arg.Any<CancellationToken>())
                   .Returns(Result<int>.WithFailure("Database connection failed"));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Database connection failed");
    }

    /// <summary>
    /// Executes Should_Return_Failure_When_Commit_Fails operation.
    /// </summary>
    /// <returns>The result of Should_Return_Failure_When_Commit_Fails.</returns>

    [Fact]
    public async Task Should_Return_Failure_When_Commit_Fails()
    {
        // Arrange
        var command = CreateValidCommand();

        _repository.AddAsync(Arg.Any<ConfigApp>(), Arg.Any<CancellationToken>())
                   .Returns(Result<int>.Success(1));
        _repository.CommitAsync(Arg.Any<CancellationToken>())
                   .Returns(Result.WithFailure("Transaction commit failed"));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Transaction commit failed");
    }

    /// <summary>
    /// Executes Should_Handle_Empty_Properties_Gracefully operation.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <param name="factory">The factory.</param>
    /// <param name="line">The line.</param>
    /// <param name="project">The project.</param>
    /// <param name="version">The version.</param>
    /// <returns>The result of Should_Handle_Empty_Properties_Gracefully.</returns>

    [Theory]
    [InlineData("", "Factory", "Line", "Project", "Version")]
    [InlineData("Client", "", "Line", "Project", "Version")]
    [InlineData("Client", "Factory", "", "Project", "Version")]
    [InlineData("Client", "Factory", "Line", "", "Version")]
    [InlineData("Client", "Factory", "Line", "Project", "")]
    public async Task Should_Handle_Empty_Properties_Gracefully(string client, string factory, string line, string project, string version)
    {
        // Using parameters: client, factory, line, project, version
        _ = client; // xUnit1026 fix
        _ = factory; // xUnit1026 fix
        _ = line; // xUnit1026 fix
        _ = project; // xUnit1026 fix
        _ = version; // xUnit1026 fix
        // Using parameters: client, factory, line, project, version
        _ = client; // xUnit1026 fix
        _ = factory; // xUnit1026 fix
        _ = line; // xUnit1026 fix
        _ = project; // xUnit1026 fix
        _ = version; // xUnit1026 fix
        // Using parameters: client, factory, line, project, version
        _ = client; // xUnit1026 fix
        _ = factory; // xUnit1026 fix
        _ = line; // xUnit1026 fix
        _ = project; // xUnit1026 fix
        _ = version; // xUnit1026 fix
        // Using parameters: client, factory, line, project, version
        _ = client; // xUnit1026 fix
        _ = factory; // xUnit1026 fix
        _ = line; // xUnit1026 fix
        _ = project; // xUnit1026 fix
        _ = version; // xUnit1026 fix
        // Using parameters: client, factory, line, project, version
        _ = client; // xUnit1026 fix
        _ = factory; // xUnit1026 fix
        _ = line; // xUnit1026 fix
        _ = project; // xUnit1026 fix
        _ = version; // xUnit1026 fix
        // Arrange
        var command = new CreateConfigAppCommand
        {
            MachineId = 100,
            Client = client,
            Factorie = factory,
            Line = line,
            Project = project,
            Version = version
        };

        _repository.AddAsync(Arg.Any<ConfigApp>(), Arg.Any<CancellationToken>())
                   .Returns(Result<int>.Success(1));
        _repository.CommitAsync(Arg.Any<CancellationToken>())
                   .Returns(Result.Success());

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Should_Return_Error_When_Add_Fails operation.
    /// </summary>
    /// <returns>The result of Should_Return_Error_When_Add_Fails.</returns>

    [Fact]
    public async Task Should_Return_Error_When_Add_Fails()
    {
        // Arrange
        var command = CreateValidCommand();

        _repository.AddAsync(Arg.Any<ConfigApp>(), Arg.Any<CancellationToken>())
                   .Returns(Result<int>.WithFailure(["Repository connection timeout"]));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Repository connection timeout");
    }

    /// <summary>
    /// Executes Should_Return_Error_When_Commit_Fails operation.
    /// </summary>
    /// <returns>The result of Should_Return_Error_When_Commit_Fails.</returns>

    [Fact]
    public async Task Should_Return_Error_When_Commit_Fails()
    {
        // Arrange
        var command = CreateValidCommand();

        _repository.AddAsync(Arg.Any<ConfigApp>(), Arg.Any<CancellationToken>())
                   .Returns(Result<int>.Success(1));
        _repository.CommitAsync(Arg.Any<CancellationToken>())
                   .Returns(Result.WithFailure(["Commit transaction failed"]));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Commit transaction failed");
    }

    /// <summary>
    /// Executes Should_Pass_CancellationToken_To_Repository operation.
    /// </summary>
    /// <returns>The result of Should_Pass_CancellationToken_To_Repository.</returns>

    [Fact]
    public async Task Should_Pass_CancellationToken_To_Repository()
    {
        // Arrange
        var command = CreateValidCommand();
        var cancellationToken = TestContext.Current.CancellationToken;

        _repository.AddAsync(Arg.Any<ConfigApp>(), Arg.Any<CancellationToken>())
                   .Returns(Result<int>.Success(1));
        _repository.CommitAsync(Arg.Any<CancellationToken>())
                   .Returns(Result.Success());

        // Act
        await _handler.ProcessAsync(command, cancellationToken);

        // Assert
        await _repository.Received(1).AddAsync(Arg.Any<ConfigApp>(), cancellationToken);
        await _repository.Received(1).CommitAsync(cancellationToken);
    }

    private static CreateConfigAppCommand CreateValidCommand()
    {
        return new CreateConfigAppCommand
        {
            MachineId = 100,
            Client = "Test Client",
            Factorie = "Test Factory",
            Line = "Test Line",
            Project = "Test Project",
            Version = "1.0.0"
        };
    }
}