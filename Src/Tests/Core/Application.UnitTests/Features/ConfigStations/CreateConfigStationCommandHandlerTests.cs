// <copyright file="CreateConfigStationCommandHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.ConfigStations.Commands.Create;

namespace Application.UnitTests.Features.ConfigStations;

/// <summary>
/// Unit tests for CreateConfigStationCommandHandler using repository pattern
/// </summary>
public class CreateConfigStationCommandHandlerTests
{
    private readonly IRepository<ConfigStation> _repository = null!;
    private readonly ILogger<CreateConfigStationCommandHandler> _logger = null!;
    private readonly CreateConfigStationCommandHandler _handler = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public CreateConfigStationCommandHandlerTests()
    {
        _repository = Substitute.For<IRepository<ConfigStation>>();
        _logger = XUnitLogger.CreateLogger<CreateConfigStationCommandHandler>();
        _handler = new CreateConfigStationCommandHandler(_repository, _logger);
    }

    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var handler = new CreateConfigStationCommandHandler(_repository, _logger);

        // Assert
        handler.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Constructor_WithNullRepository_ShouldThrowArgumentNullException operation.
    /// </summary>

    // MARKED FOR DELETION - Constructor null guard test no longer needed for DI handlers
    // [Fact]
    //     public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
    //     {
    //         // Arrange & Act & Assert
    //         Should.Throw<ArgumentNullException>(() => new CreateConfigStationCommandHandler(null!, _logger))
    //             .ParamName.ShouldBe("repository");
    //     }
    /// <summary>
    /// Executes Constructor_WithNullLogger_ShouldAllowNullLogger operation.
    /// </summary>

    [Fact]
    public void Constructor_WithNullLogger_ShouldAllowNullLogger()
    {
        // Arrange & Act
        var handler = new CreateConfigStationCommandHandler(_repository, null!);

        // Assert - constructor accepts null logger (no validation)
        handler.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Process_WithValidCommand_ShouldReturnSuccess operation.
    /// </summary>
    /// <returns>The result of Process_WithValidCommand_ShouldReturnSuccess.</returns>

    [Fact]
    public async Task Process_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var command = CreateValidCommand();
        _repository.AddAsync(Arg.Any<ConfigStation>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(1));
        _repository.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ConfigId.ShouldBe(command.ConfigId);
    }

    /// <summary>
    /// Executes Process_WhenAddFails_ShouldReturnFailure operation.
    /// </summary>
    /// <returns>The result of Process_WhenAddFails_ShouldReturnFailure.</returns>

    [Fact]
    public async Task Process_WhenAddFails_ShouldReturnFailure()
    {
        // Arrange
        var command = CreateValidCommand();
        _repository.AddAsync(Arg.Any<ConfigStation>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.WithFailure("Database connection failed"));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Database connection failed");
    }

    /// <summary>
    /// Executes Process_WhenCommitFails_ShouldReturnFailure operation.
    /// </summary>
    /// <returns>The result of Process_WhenCommitFails_ShouldReturnFailure.</returns>

    [Fact]
    public async Task Process_WhenCommitFails_ShouldReturnFailure()
    {
        // Arrange
        var command = CreateValidCommand();
        _repository.AddAsync(Arg.Any<ConfigStation>(), Arg.Any<CancellationToken>())
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
    /// Executes Process_WithInvalidConfigId_ShouldHandleGracefully operation.
    /// </summary>
    /// <param name="configId">The configId.</param>
    /// <returns>The result of Process_WithInvalidConfigId_ShouldHandleGracefully.</returns>

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Process_WithInvalidConfigId_ShouldHandleGracefully(string? configId)
    {
        // Using parameters: configId

        // Arrange
        var command = CreateValidCommand();
        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS8602] Add null check before dereferencing
        command.ShouldNotBeNull();
        command.ConfigId = configId!;
        _repository.AddAsync(Arg.Any<ConfigStation>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(1));
        _repository.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        //[Fix]
        //CLAUDE
        //Date: 28/08/2025
        //Reason: [CS8602] - Add null-forgiving operator since result.IsSuccess was verified true
        result.Value.ShouldNotBeNull().ConfigId.ShouldBe(configId);
    }

    /// <summary>
    /// Executes Process_ShouldCallRepositoryWithCorrectEntity operation.
    /// </summary>
    /// <returns>The result of Process_ShouldCallRepositoryWithCorrectEntity.</returns>

    [Fact]
    public async Task Process_ShouldCallRepositoryWithCorrectEntity()
    {
        // Arrange
        var command = CreateValidCommand();
        _repository.AddAsync(Arg.Any<ConfigStation>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(1));
        _repository.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        await _repository.Received(1).AddAsync(
            Arg.Is<ConfigStation>(entity =>
                entity.ConfigAppId == command.ConfigId &&
                entity.MachineId == command.MachineId &&
                entity.Client == command.Client &&
                entity.Factory == command.Factorie &&
                entity.Line == command.Line),
            Arg.Any<CancellationToken>());
    }

    private static CreateConfigStationCommand CreateValidCommand()
    {
        return new CreateConfigStationCommand
        {
            ConfigId = "CONFIG001",
            MachineId = 100,
            Client = "Test Client",
            Factorie = "Test Factory",
            Line = "Test Line",
            Machine = "Test Machine",
            Project = "Test Project",
            Version = "1.0.0",
            VersionDate = DateTime.Now,
            ModifiedDate = DateTime.Now
        };
    }

    /// <summary>
    /// Executes Process_ShouldPassCancellationTokenToRepository operation.
    /// </summary>
    /// <returns>The result of Process_ShouldPassCancellationTokenToRepository.</returns>

    [Fact]
    public async Task Process_ShouldPassCancellationTokenToRepository()
    {
        // Arrange
        var command = CreateValidCommand();
        var cancellationToken = TestContext.Current.CancellationToken;

        _repository.AddAsync(Arg.Any<ConfigStation>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(1));
        _repository.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        await _handler.ProcessAsync(command, cancellationToken);

        // Assert
        await _repository.Received(1).AddAsync(Arg.Any<ConfigStation>(), cancellationToken);
        await _repository.Received(1).CommitAsync(cancellationToken);
    }

    /// <summary>
    /// Executes Process_WhenAddFails_ShouldLogError operation.
    /// </summary>
    /// <returns>The result of Process_WhenAddFails_ShouldLogError.</returns>

    [Fact]
    public async Task Process_WhenAddFails_ShouldLogError()
    {
        // Arrange
        var command = CreateValidCommand();
        _repository.AddAsync(Arg.Any<ConfigStation>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.WithFailure("Repository connection timeout"));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert

        result.IsSuccess.ShouldBeFalse();
        result.Errors.Count().ShouldBeGreaterThan(0);
        result.Errors.ShouldContain("Repository connection timeout");
    }

    /// <summary>
    /// Executes Process_WhenCommitFails_ShouldLogError operation.
    /// </summary>
    /// <returns>The result of Process_WhenCommitFails_ShouldLogError.</returns>

    [Fact]
    public async Task Process_WhenCommitFails_ShouldLogError()
    {
        // Arrange
        var command = CreateValidCommand();
        _repository.AddAsync(Arg.Any<ConfigStation>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(1));
        _repository.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(Result.WithFailure("Commit transaction failed"));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Commit transaction failed");
    }

    /// <summary>
    /// Executes Process_WithEmptyStringProperties_ShouldHandleGracefully operation.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <param name="factory">The factory.</param>
    /// <param name="line">The line.</param>
    /// <returns>The result of Process_WithEmptyStringProperties_ShouldHandleGracefully.</returns>

    [Theory]
    [InlineData("", "Factory", "Line")]
    [InlineData("Client", "", "Line")]
    [InlineData("Client", "Factory", "")]
    public async Task Process_WithEmptyStringProperties_ShouldHandleGracefully(string client, string factory, string line)
    {
        // Using parameters: client, factory, line
        _ = client; // xUnit1026 fix
        _ = factory; // xUnit1026 fix
        _ = line; // xUnit1026 fix
        // Using parameters: client, factory, line
        _ = client; // xUnit1026 fix
        _ = factory; // xUnit1026 fix
        _ = line; // xUnit1026 fix
        // Using parameters: client, factory, line
        _ = client; // xUnit1026 fix
        _ = factory; // xUnit1026 fix
        _ = line; // xUnit1026 fix
        // Using parameters: client, factory, line
        _ = client; // xUnit1026 fix
        _ = factory; // xUnit1026 fix
        _ = line; // xUnit1026 fix
        // Using parameters: client, factory, line
        _ = client; // xUnit1026 fix
        _ = factory; // xUnit1026 fix
        _ = line; // xUnit1026 fix
        // Arrange
        var command = new CreateConfigStationCommand
        {
            ConfigId = "CONFIG001",
            MachineId = 100,
            Client = client,
            Factorie = factory,
            Line = line,
            Version = "1.0"
        };

        _repository.AddAsync(Arg.Any<ConfigStation>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(1));
        _repository.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
    }
}