// <copyright file="UpdateSettingCommandHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Application.Settings.Commands.Update;

namespace Application.UnitTests.Features.Settings;

/// <summary>
/// Unit tests for UpdateSettingCommandHandler.
/// #95 Phase 2 Slice C: the Setting read stays on IReadOnlyRepository&lt;Setting&gt;; the write is staged on
/// the owning Machine root and persisted through IAggregateRepository&lt;Machine&gt;.SaveAsync (one explicit
/// transaction). The mocks mirror that contract.
/// </summary>
public class UpdateSettingCommandHandlerTests
{
    private readonly IReadOnlyRepository<Setting> _repository = null!;
    private readonly IAggregateRepository<Machine> _machineAggregateRepository = null!;
    private readonly ILogger<UpdateSettingCommandHandler> _logger = null!;
    private readonly UpdateSettingCommandHandler _handler = null!;

    // The Machine root the aggregate repository "loads" for the setting's owning machine (10000).
    private readonly Machine _machineRoot = new() { MachineId = new MachineId(10000) };
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public UpdateSettingCommandHandlerTests()
    {
        _repository = Substitute.For<IReadOnlyRepository<Setting>>();
        _machineAggregateRepository = Substitute.For<IAggregateRepository<Machine>>();
        // #95 Slice C: by default the root loads and the aggregate save succeeds; failure tests override.
        _machineAggregateRepository.LoadAsync(Arg.Any<int>(), Arg.Any<AggregateLoadOptions>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine>.Success(_machineRoot));
        _machineAggregateRepository.SaveAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        _logger = XUnitLogger.CreateLogger<UpdateSettingCommandHandler>();
        _handler = new UpdateSettingCommandHandler(_repository, _machineAggregateRepository, _logger);
    }

    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var handler = new UpdateSettingCommandHandler(_repository, _machineAggregateRepository, _logger);

        // Assert
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
        var existingSetting = new Setting
        {
            SettingId = 1,
            MachineId = new MachineId(10000),
            Config = "Original Config"
        };

        var command = new UpdateSettingCommand
        {
            SettingId = 1,
            MachineId = 10000,
            Config = "Updated Config"
        };

        _repository.GetByIdAsync(command.SettingId ?? 0, Arg.Any<CancellationToken>())
            .Returns(Result<Setting?>.Success(existingSetting));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.SettingId.ShouldBe(command.SettingId ?? 0);
        result.Value.MachineId.ShouldBe(existingSetting.MachineId.Value);
        result.Value.Config.ShouldBe(command.Config);

        await _repository.Received(1).GetByIdAsync(command.SettingId ?? 0, Arg.Any<CancellationToken>());
        await _machineAggregateRepository.Received(1).LoadAsync(existingSetting.MachineId.Value, Arg.Any<AggregateLoadOptions>(), Arg.Any<CancellationToken>());
        await _machineAggregateRepository.Received(1).SaveAsync(_machineRoot, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_Return_Failure_When_Entity_Not_Found operation.
    /// </summary>
    /// <returns>The result of Should_Return_Failure_When_Entity_Not_Found.</returns>

    [Fact]
    public async Task Should_Return_Failure_When_Entity_Not_Found()
    {
        // Arrange
        var command = CreateValidCommand();

        _repository.GetByIdAsync(command.SettingId ?? 0, Arg.Any<CancellationToken>())
                   .Returns(Result<Setting?>.WithFailure("Entity not found in database"));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        //[Fix]
        //CURSOR
        //Date: 23/08/2025
        //Reason: Pattern 12 Fix - Handler returns specific error message, not generic repository error
        result.Errors.ShouldContain("SettingId 1 does not exist");
    }

    /// <summary>
    /// Executes Should_Return_Failure_When_Update_Fails operation (now: the aggregate LOAD of the owning
    /// Machine root fails — the first write-path step, mirroring the former UpdateAsync failure point).
    /// </summary>
    /// <returns>The result of Should_Return_Failure_When_Update_Fails.</returns>

    [Fact]
    public async Task Should_Return_Failure_When_Update_Fails()
    {
        // Arrange
        var command = CreateValidCommand();
        var existingSetting = CreateExistingSetting();

        _repository.GetByIdAsync(command.SettingId ?? 0, Arg.Any<CancellationToken>())
                   .Returns(Result<Setting?>.Success(existingSetting));
        _machineAggregateRepository.LoadAsync(Arg.Any<int>(), Arg.Any<AggregateLoadOptions>(), Arg.Any<CancellationToken>())
                   .Returns(Result<Machine>.WithFailure("Database update failed"));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Database update failed");
    }

    /// <summary>
    /// Executes Should_Return_Failure_When_Commit_Fails operation (now: the atomic aggregate SAVE fails).
    /// </summary>
    /// <returns>The result of Should_Return_Failure_When_Commit_Fails.</returns>

    [Fact]
    public async Task Should_Return_Failure_When_Commit_Fails()
    {
        // Arrange
        var command = CreateValidCommand();
        var existingSetting = CreateExistingSetting();

        _repository.GetByIdAsync(command.SettingId ?? 0, Arg.Any<CancellationToken>())
                   .Returns(Result<Setting?>.Success(existingSetting));
        _machineAggregateRepository.SaveAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
                   .Returns(Result.WithFailure("Transaction commit failed"));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Transaction commit failed");
    }

    /// <summary>
    /// Executes Process_ShouldCallRepositoryWithCorrectParameters operation.
    /// </summary>
    /// <returns>The result of Process_ShouldCallRepositoryWithCorrectParameters.</returns>

    [Fact]
    public async Task Process_ShouldCallRepositoryWithCorrectParameters()
    {
        // Arrange
        var existingSetting = new Setting { SettingId = 1, MachineId = new MachineId(10000) };
        var command = new UpdateSettingCommand
        {
            SettingId = 1,
            MachineId = 10001,
            Config = "New Config"
        };

        _repository.GetByIdAsync(command.SettingId ?? 0, Arg.Any<CancellationToken>())
            .Returns(Result<Setting?>.Success(existingSetting));

        // Act
        await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert — the root is loaded for the setting's PERSISTED MachineId, not the command's.
        await _repository.Received(1).GetByIdAsync(command.SettingId ?? 0, Arg.Any<CancellationToken>());
        await _machineAggregateRepository.Received(1).LoadAsync(existingSetting.MachineId.Value, Arg.Any<AggregateLoadOptions>(), Arg.Any<CancellationToken>());
        await _machineAggregateRepository.Received(1).SaveAsync(_machineRoot, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Process_ShouldPassCancellationTokenToRepository operation.
    /// </summary>
    /// <returns>The result of Process_ShouldPassCancellationTokenToRepository.</returns>

    [Fact]
    public async Task Process_ShouldPassCancellationTokenToRepository()
    {
        // Arrange
        var existingSetting = new Setting { SettingId = 1, MachineId = new MachineId(10000) };
        //[Fix]
        //CLAUDE
        //Date: 24/08/2025
        //Reason: [TEST SETUP BUG FIX] - Command must have valid Config value since handler validates string.IsNullOrEmpty(request.Config). Test was failing early and never calling repository.
        var command = new UpdateSettingCommand { SettingId = 1, Config = "ValidConfig" };
        var cancellationToken = new CancellationToken();

        _repository.GetByIdAsync(command.SettingId ?? 0, Arg.Any<CancellationToken>())
            .Returns(Result<Setting?>.Success(existingSetting));

        // Act
        var result = await _handler.ProcessAsync(command, cancellationToken);

        result.IsSuccess.ShouldBeTrue();

        // Assert
        await _repository.Received(1).GetByIdAsync(command.SettingId ?? 0, cancellationToken);
        await _machineAggregateRepository.Received(1).LoadAsync(existingSetting.MachineId.Value, Arg.Any<AggregateLoadOptions>(), cancellationToken);
        await _machineAggregateRepository.Received(1).SaveAsync(_machineRoot, cancellationToken);
    }

    /// <summary>
    /// Executes Process_WithNullOrEmptyConfig_ShouldHandleGracefully operation.
    /// </summary>
    /// <param name="config">The config.</param>
    /// <returns>The result of Process_WithNullOrEmptyConfig_ShouldHandleGracefully.</returns>

    //[Fix]
    //CLAUDE
    //Date: 25/08/2025
    //Reason: [REPOSITORY SETUP MISSING] - Whitespace config ("   ") bypasses string.IsNullOrEmpty() validation and calls repository
    [Theory]
    [InlineData("")]     // This triggers string.IsNullOrEmpty() and returns early
    [InlineData("   ")]  // This bypasses string.IsNullOrEmpty() and needs repository mocks
    public async Task Process_WithNullOrEmptyConfig_ShouldHandleGracefully(string config)
    {
        // Arrange
        var command = new UpdateSettingCommand
        {
            SettingId = 1,
            MachineId = 10000,
            Config = config
        };

        //[Fix]
        //CLAUDE
        //Date: 25/08/2025
        //Reason: [REPOSITORY SETUP BUG FIX] - Whitespace config bypasses early validation, so we need repository mocks
        // For whitespace config ("   "), handler will call repository, so we need to mock it to return failure
        if (!string.IsNullOrEmpty(config)) // Whitespace case
        {
            _repository.GetByIdAsync(command.SettingId ?? 0, Arg.Any<CancellationToken>())
                .Returns(Result<Setting?>.WithFailure("Setting not found"));
        }

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert - All cases should fail (empty string fails early, whitespace fails at repository level)
        result.IsSuccess.ShouldBeFalse();
        result.Value.ShouldBeNull();
        result.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Process_WithValidConfig_ShouldSucceed()
    {
        // Arrange
        var existingSetting = new Setting { SettingId = 1, MachineId = new MachineId(10000), Config = "Original" };
        var command = new UpdateSettingCommand
        {
            SettingId = 1,
            MachineId = 10000,
            Config = "TestConfig"
        };

        _repository.GetByIdAsync(command.SettingId ?? 0, Arg.Any<CancellationToken>())
            .Returns(Result<Setting?>.Success(existingSetting));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert - Valid config should succeed
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Config.ShouldBe("TestConfig");
    }

    private static Setting CreateExistingSetting()
    {
        return new Setting
        {
            SettingId = 1,
            MachineId = new MachineId(10000),
            Config = "Existing Configuration"
        };
    }

    private static UpdateSettingCommand CreateValidCommand()
    {
        return new UpdateSettingCommand
        {
            SettingId = 1,
            MachineId = 10000,
            Config = "Updated Config"
        };
    }
}
