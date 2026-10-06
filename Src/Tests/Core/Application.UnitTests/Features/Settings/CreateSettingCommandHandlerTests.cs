// <copyright file="CreateSettingCommandHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Application.Settings.Commands.Create;

namespace Application.UnitTests.Features.Settings;

/// <summary>
/// Unit tests for CreateSettingCommandHandler.
/// #95 Phase 2 Slice C: Setting is a member of the Machine aggregate — the handler loads the Machine root,
/// stages the new Setting on it, and persists through IAggregateRepository&lt;Machine&gt;.SaveAsync (one
/// explicit transaction). The mocks mirror that contract.
/// </summary>
public class CreateSettingCommandHandlerTests
{
    private readonly IAggregateRepository<Machine> _machineAggregateRepository = null!;
    private readonly IRepository<Machine> _machineRepository = null!;
    private readonly ILogger<CreateSettingCommandHandler> _logger = null!;
    private readonly CreateSettingCommandHandler _handler = null!;

    // The Machine root the aggregate repository "loads"; staged sets are inspected on this instance.
    private readonly Machine _machineRoot = new() { MachineId = new MachineId(10000) };
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public CreateSettingCommandHandlerTests()
    {
        _machineAggregateRepository = Substitute.For<IAggregateRepository<Machine>>();
        _machineRepository = Substitute.For<IRepository<Machine>>();
        // #97: by default the parent Machine exists so the fail-closed FK probe passes; the dangling-parent
        // test overrides this with Success(0).
        _machineRepository.CountAsync(Arg.Any<ISpecification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(1));
        // #95 Slice C: by default the root loads and the aggregate save succeeds; failure tests override.
        _machineAggregateRepository.LoadAsync(Arg.Any<int>(), Arg.Any<AggregateLoadOptions>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine>.Success(_machineRoot));
        _machineAggregateRepository.SaveAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        _logger = XUnitLogger.CreateLogger<CreateSettingCommandHandler>();
        _handler = new CreateSettingCommandHandler(_machineAggregateRepository, _machineRepository, _logger);
    }
    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var handler = new CreateSettingCommandHandler(_machineAggregateRepository, _machineRepository, _logger);

        // Assert
        handler.ShouldNotBeNull();
    }

    /// <summary>
    /// #97 red-green: a positive-but-dangling MachineId (no parent Machine row -> CountAsync Success(0)) must be
    /// refused gracefully; the handler must NOT reach the aggregate load/save. On pre-fix code this proceeded to
    /// persist and threw SqlException 547 on real SQL.
    /// </summary>
    /// <returns>The result of Process_WithDanglingMachineId_ShouldFailAndNotPersist.</returns>
    [Fact]
    public async Task Process_WithDanglingMachineId_ShouldFailAndNotPersist()
    {
        // Arrange
        var command = CreateValidCommand();
        _machineRepository.CountAsync(Arg.Any<ISpecification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(0));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain($"Machine with id {command.MachineId} does not exist.");
        await _machineAggregateRepository.DidNotReceive().LoadAsync(Arg.Any<int>(), Arg.Any<AggregateLoadOptions>(), Arg.Any<CancellationToken>());
        await _machineAggregateRepository.DidNotReceive().SaveAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// #97: when the parent Machine existence probe cannot be verified (CountAsync failure), the handler must
    /// fail closed (propagate the failure) rather than proceed to persist.
    /// </summary>
    /// <returns>The result of Process_WhenMachineProbeFails_ShouldFailClosed.</returns>
    [Fact]
    public async Task Process_WhenMachineProbeFails_ShouldFailClosed()
    {
        // Arrange
        var command = CreateValidCommand();
        _machineRepository.CountAsync(Arg.Any<ISpecification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.WithFailure("Machine lookup timed out"));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Machine lookup timed out");
        await _machineAggregateRepository.DidNotReceive().SaveAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>());
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

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.SettingId.ShouldBe(command.SettingId);
        result.Value.MachineId.ShouldBe(command.MachineId);
        result.Value.Setting.ShouldBe(command.Setting);

        await _machineAggregateRepository.Received(1).LoadAsync(command.MachineId, Arg.Any<AggregateLoadOptions>(), Arg.Any<CancellationToken>());
        await _machineAggregateRepository.Received(1).SaveAsync(_machineRoot, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// #95 Slice C: SettingId is a store-generated identity, so StageSettingAppend refuses an entity that
    /// already carries a persisted identity — a preset SettingId must fail before any save.
    /// </summary>
    /// <returns>The result of Process_WithPresetSettingId_ShouldRefuseStaging.</returns>
    [Fact]
    public async Task Process_WithPresetSettingId_ShouldRefuseStaging()
    {
        // Arrange
        var command = new CreateSettingCommand
        {
            SettingId = 42,
            MachineId = 10000,
            Setting = "Test Configuration",
        };

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("persisted identity", StringComparison.Ordinal));
        await _machineAggregateRepository.DidNotReceive().SaveAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_Return_Failure_When_Add_Fails operation (now: the aggregate LOAD fails — the first
    /// write-path step, mirroring the former AddAsync failure point).
    /// </summary>
    /// <returns>The result of Should_Return_Failure_When_Add_Fails.</returns>

    [Fact]
    public async Task Should_Return_Failure_When_Add_Fails()
    {
        // Arrange
        var command = CreateValidCommand();

        _machineAggregateRepository.LoadAsync(Arg.Any<int>(), Arg.Any<AggregateLoadOptions>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine>.WithFailure("Database connection failed"));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Database connection failed");
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

        _machineAggregateRepository.SaveAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Result.WithFailure("Transaction commit failed"));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Transaction commit failed");
    }

    /// <summary>
    /// Executes Process_ShouldCallRepositoryWithCorrectEntity operation (now: the entity is STAGED on the
    /// loaded Machine root before the aggregate save).
    /// </summary>
    /// <returns>The result of Process_ShouldCallRepositoryWithCorrectEntity.</returns>

    [Fact]
    public async Task Process_ShouldCallRepositoryWithCorrectEntity()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert — the mock SaveAsync does not consume the staged set, so the staged entity is inspectable.
        _machineRoot.PendingSettingAppends.Count.ShouldBe(1);
        var staged = _machineRoot.PendingSettingAppends[0];
        staged.SettingId.ShouldBe(command.SettingId);
        staged.MachineId.Value.ShouldBe(command.MachineId);
        staged.Config.ShouldBe(command.Setting);
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
        var command = CreateValidCommand();
        var cancellationToken = new CancellationToken();

        // Act
        await _handler.ProcessAsync(command, cancellationToken);

        // Assert
        await _machineAggregateRepository.Received(1).LoadAsync(command.MachineId, Arg.Any<AggregateLoadOptions>(), cancellationToken);
        await _machineAggregateRepository.Received(1).SaveAsync(_machineRoot, cancellationToken);
    }
    /// <summary>
    /// Executes Process_WithNullOrEmptyConfig_ShouldHandleGracefully operation.
    /// </summary>
    /// <param name="config">The config.</param>
    /// <returns>The result of Process_WithNullOrEmptyConfig_ShouldHandleGracefully.</returns>

    [Theory]
    [InlineData("TestConfig")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Process_WithNullOrEmptyConfig_ShouldHandleGracefully(string config)
    {
        // Arrange — SettingId stays 0: the identity is store-generated (#95 Slice C staging invariant).
        var command = new CreateSettingCommand
        {
            SettingId = 0,
            MachineId = 10000,
            Setting = config
        };

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Setting.ShouldBe(config);
    }
    /// <summary>
    /// Executes Process_ShouldReturnErrorWhenAddFails operation (now: the aggregate LOAD fails).
    /// </summary>
    /// <returns>The result of Process_ShouldReturnErrorWhenAddFails.</returns>

    [Fact]
    public async Task Process_ShouldReturnErrorWhenAddFails()
    {
        // Arrange
        var command = CreateValidCommand();

        _machineAggregateRepository.LoadAsync(Arg.Any<int>(), Arg.Any<AggregateLoadOptions>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine>.WithFailure(["Repository connection timeout"]));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Repository connection timeout");
    }
    /// <summary>
    /// Executes Process_ShouldReturnErrorWhenCommitFails operation (now: the atomic aggregate SAVE fails).
    /// </summary>
    /// <returns>The result of Process_ShouldReturnErrorWhenCommitFails.</returns>

    [Fact]
    public async Task Process_ShouldReturnErrorWhenCommitFails()
    {
        // Arrange
        var command = CreateValidCommand();

        _machineAggregateRepository.SaveAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Result.WithFailure(["Commit transaction failed"]));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Commit transaction failed");
    }

    private static CreateSettingCommand CreateValidCommand()
    {
        // SettingId = 0: the identity column is store-generated; #95 Slice C StageSettingAppend refuses a
        // preset identity (the old handler silently carried it and would have failed identity-insert on real SQL).
        return new CreateSettingCommand
        {
            SettingId = 0,
            MachineId = 10000,
            Setting = "Test Configuration"
        };
    }
}
