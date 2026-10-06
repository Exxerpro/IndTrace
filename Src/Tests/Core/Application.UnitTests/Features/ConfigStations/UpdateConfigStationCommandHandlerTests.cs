// <copyright file="UpdateConfigStationCommandHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.ConfigStations.Commands.Update;

namespace Application.UnitTests.Features.ConfigStations;

/// <summary>
/// Unit tests for UpdateConfigStationCommandHandler using repository pattern
/// </summary>
public class UpdateConfigStationCommandHandlerTests
{
    private readonly IRepository<ConfigStation> _repository = null!;
    private readonly ILogger<UpdateConfigStationCommandHandler> _logger = null!;
    private readonly UpdateConfigStationCommandHandler _handler = null!;
    // MARKED FOR REMOVAL - Constructor null guard test no longer needed with Result<T> patterns
    // /// <summary>
    // /// Initializes a new instance of the class.
    // /// </summary>

    public UpdateConfigStationCommandHandlerTests()
    {
        _repository = Substitute.For<IRepository<ConfigStation>>();
        _logger = XUnitLogger.CreateLogger<UpdateConfigStationCommandHandler>();
        _handler = new UpdateConfigStationCommandHandler(_repository, _logger);
    }

    // /// <summary>
    // /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    // /// </summary>

    // [Fact]
    // public void Constructor_WithValidParameters_ShouldCreateInstance()
    // {
    //     // Arrange & Act
    //     var handler = new UpdateConfigStationCommandHandler(_repository, _logger);

    //     // Assert
    //     handler.ShouldNotBeNull();
    // }

    // /// <summary>
    // /// Executes Constructor_WithNullRepository_ShouldThrowArgumentNullException operation.
    // /// </summary>

    // [Fact]
    // public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
    // {
    //     // Arrange & Act & Assert
    //     // Act
    //     var result = new UpdateConfigStationCommandHandler(null!, _logger);

    //     // Assert
    //     //result.IsFailure.ShouldBeTrue();
    //     //result.Errors.ShouldNotBeNull();
    //     //result.Errors.ShouldContain("repository");
    // }

    // MARKED FOR REMOVAL - Constructor null guard test no longer needed with Result<T> patterns
    // /// <summary>
    // /// Executes Constructor_WithNullLogger_ShouldThrowArgumentNullException operation.
    // /// </summary>

    // [Fact]
    // public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    // {
    //     // Arrange & Act & Assert
    //     // Act
    //     var result = new UpdateConfigStationCommandHandler(_repository, null!);

    //     // Assert
    //     //result.IsFailure.ShouldBeTrue();
    //     //result.Errors.ShouldNotBeNull();
    //     //result.Errors.ShouldContain("logger");
    // }

    /// <summary>
    /// Executes Process_WithValidCommand_ShouldReturnSuccess operation.
    /// </summary>
    /// <returns>The result of Process_WithValidCommand_ShouldReturnSuccess.</returns>

    [Fact]
    public async Task Process_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        // This test nee to set a Substutite for repository at least

        var command = CreateValidCommand();
        var existingEntity = CreateExistingEntity();

        _repository.GetByIdAsync(command.MachineId, Arg.Any<CancellationToken>())
            .Returns(Result<ConfigStation?>.Success(existingEntity));
        _repository.UpdateAsync(Arg.Any<ConfigStation>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        _repository.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.ShouldNotBeNull();
        result.Value.ShouldNotBeNull();
        result.Value.ConfigStationId.ShouldBe(existingEntity.AppId);
    }

    /// <summary>
    /// Executes Process_WhenEntityNotFound_ShouldReturnFailure operation.
    /// </summary>
    /// <returns>The result of Process_WhenEntityNotFound_ShouldReturnFailure.</returns>

    [Fact]
    public async Task Process_WhenEntityNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = CreateValidCommand();
        _repository.GetByIdAsync(command.MachineId, Arg.Any<CancellationToken>())
            .Returns(Result<ConfigStation?>.Success(null));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("ConfigStation not found");
    }

    /// <summary>
    /// Executes Process_WhenGetByIdFails_ShouldReturnFailure operation.
    /// </summary>
    /// <returns>The result of Process_WhenGetByIdFails_ShouldReturnFailure.</returns>

    [Fact]
    public async Task Process_WhenGetByIdFails_ShouldReturnFailure()
    {
        // Arrange
        var command = CreateValidCommand();
        _repository.GetByIdAsync(command.MachineId, Arg.Any<CancellationToken>())
            .Returns(Result<ConfigStation?>.WithFailure("Entity not found in database"));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
    }

    /// <summary>
    /// Executes Process_WhenUpdateFails_ShouldReturnFailure operation.
    /// </summary>
    /// <returns>The result of Process_WhenUpdateFails_ShouldReturnFailure.</returns>

    [Fact]
    public async Task Process_WhenUpdateFails_ShouldReturnFailure()
    {
        // Arrange
        var command = CreateValidCommand();
        var existingEntity = CreateExistingEntity();

        _repository.GetByIdAsync(command.MachineId, Arg.Any<CancellationToken>())
            .Returns(Result<ConfigStation?>.Success(existingEntity));
        _repository.UpdateAsync(Arg.Any<ConfigStation>(), Arg.Any<CancellationToken>())
            .Returns(Result.WithFailure("Database update failed"));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Database update failed");
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
        var existingEntity = CreateExistingEntity();

        _repository.GetByIdAsync(command.MachineId, Arg.Any<CancellationToken>())
            .Returns(Result<ConfigStation?>.Success(existingEntity));
        _repository.UpdateAsync(Arg.Any<ConfigStation>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        _repository.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(Result.WithFailure("Transaction commit failed"));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Transaction commit failed");
    }

    /// <summary>
    /// Executes Process_ShouldUpdateEntityWithCommandValues operation.
    /// </summary>
    /// <returns>The result of Process_ShouldUpdateEntityWithCommandValues.</returns>

    [Fact]
    public async Task Process_ShouldUpdateEntityWithCommandValues()
    {
        // Arrange
        var command = CreateValidCommand();
        var existingEntity = CreateExistingEntity();
        ////var dbfactory = new substituteDbFactory();
        ////var logger =  XUnitLogger.CreateLogger<Repository<ConfigStation>();
        //var _repository = Substitute.For<IRepository<ConfigStation>>();

        _repository.GetByIdAsync(command.MachineId, Arg.Any<CancellationToken>())
            .Returns(Result<ConfigStation?>.Success(existingEntity));
        _repository.UpdateAsync(Arg.Any<ConfigStation>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        _repository.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        existingEntity.Client.ShouldBe(command.Client);
        existingEntity.Factory.ShouldBe(command.Factorie);
        existingEntity.Line.ShouldBe(command.Line);
        existingEntity.MachineId.ShouldBe(command.MachineId);
        existingEntity.Project.ShouldBe(command.Project);
        existingEntity.Version.ShouldBe(command.Version);
    }

    private static UpdateConfigStationCommand CreateValidCommand()
    {
        return new UpdateConfigStationCommand
        {
            ConfigId = "IndTrace L1A Updated",
            MachineId = 10000,
            Client = "Acme Updated",
            Factorie = "Acme Updated",
            Line = "CHMSL Updated",
            Machine = "WS100 Updated",
            Project = "IndTrace Updated",
            Version = "4",
            VersionDate = new DateTime(2023, 9, 1, 11, 0, 0),
            ModifiedDate = DateTime.UtcNow
        };
    }

    private static ConfigStation CreateExistingEntity()
    {
        return new ConfigStation
        {
            ConfigAppId = "IndTrace L1A",
            AppId = 100,
            Client = "Acme",
            Factory = "Acme",
            Line = "CHMSL",
            MachineId = 100,
            Project = "IndTrace",
            Version = "3",
            VersionDate = new DateTime(2023, 8, 31, 10, 46, 18),
            ModifiedDate = new DateTime(2023, 8, 31, 10, 46, 18)
        };
    }
}