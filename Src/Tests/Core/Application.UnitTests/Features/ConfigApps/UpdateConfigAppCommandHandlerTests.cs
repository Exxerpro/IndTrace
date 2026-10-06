// <copyright file="UpdateConfigAppCommandHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.ConfigApplication.Commands.Update;

namespace Application.UnitTests.Features.ConfigApps;

/// <summary>
/// Unit tests for UpdateConfigAppCommandHandler using repository pattern
/// </summary>
public class UpdateConfigAppCommandHandlerTests
{
    private readonly IRepository<ConfigApp> _repository = null!;
    private readonly ILogger<UpdateConfigAppCommandHandler> _logger = null!;
    private readonly UpdateConfigAppCommandHandler _handler = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public UpdateConfigAppCommandHandlerTests()
    {
        _repository = Substitute.For<IRepository<ConfigApp>>();
        _logger = XUnitLogger.CreateLogger<UpdateConfigAppCommandHandler>();
        _handler = new UpdateConfigAppCommandHandler(_repository, _logger);
    }

    /// <summary>
    /// Executes Constructor_ShouldCreateValidInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_ShouldCreateValidInstance()
    {
        // Arrange & Act
        var handler = new UpdateConfigAppCommandHandler(_repository, _logger);

        // Assert
        handler.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Should_Update_ConfigApp_Successfully operation.
    /// </summary>
    /// <returns>The result of Should_Update_ConfigApp_Successfully.</returns>

    [Fact]
    public async Task Should_Update_ConfigApp_Successfully()
    {
        // Arrange
        var existingConfigApp = new ConfigApp
        {
            AppId = 1,
            Client = "Old Client",
            Factory = "Old Factory",
            Line = "Old Line",
            Project = "Old Project",
            Version = "1.0.0"
        };

        var command = new UpdateConfigAppCommand
        {
            AppId = 1,
            Client = "New Client",
            Factory = "New Factory",
            Line = "New Line",
            Project = "New Project",
            Version = "2.0.0"
        };

        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS8620] Changed return type from Result<ConfigApp?> to Result<ConfigApp>
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>())
                   .Returns(Result<ConfigApp?>.Success(existingConfigApp));
        _repository.UpdateAsync(Arg.Any<ConfigApp>(), Arg.Any<CancellationToken>())
                   .Returns(Result.Success());
        _repository.CommitAsync(Arg.Any<CancellationToken>())
                   .Returns(Result.Success());

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Client.ShouldBe(command.Client);
        result.Value.Factory.ShouldBe(command.Factory);
        result.Value.Line.ShouldBe(command.Line);
        result.Value.Project.ShouldBe(command.Project);
        result.Value.Version.ShouldBe(command.Version);

        await _repository.Received(1).GetByIdAsync(1, Arg.Any<CancellationToken>());
        await _repository.Received(1).UpdateAsync(Arg.Any<ConfigApp>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_Return_Failure_When_ConfigApp_Not_Found operation.
    /// </summary>
    /// <returns>The result of Should_Return_Failure_When_ConfigApp_Not_Found.</returns>

    [Fact]
    public async Task Should_Return_Failure_When_ConfigApp_Not_Found()
    {
        // Arrange
        var command = new UpdateConfigAppCommand
        {
            AppId = 999,
            Client = "Test Client"
        };

        _repository.GetByIdAsync(999, Arg.Any<CancellationToken>())
                   .Returns(Result<ConfigApp?>.WithFailure("Entity not found in database"));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.Count().ShouldBeGreaterThanOrEqualTo(1);
    }

    /// <summary>
    /// Executes Should_Return_Failure_When_GetById_Fails operation.
    /// </summary>
    /// <returns>The result of Should_Return_Failure_When_GetById_Fails.</returns>

    [Fact]
    public async Task Should_Return_Failure_When_GetById_Fails()
    {
        // Arrange
        var command = new UpdateConfigAppCommand { AppId = 1 };

        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>())
                   .Returns(Result<ConfigApp?>.WithFailure("Entity not found in database"));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.Count().ShouldBeGreaterThanOrEqualTo(1);
    }

    /// <summary>
    /// Executes Should_Return_Failure_When_ConfigApp_Is_Null operation.
    /// </summary>
    /// <returns>The result of Should_Return_Failure_When_ConfigApp_Is_Null.</returns>

    [Fact]
    public async Task Should_Return_Failure_When_ConfigApp_Is_Null()
    {
        // Arrange
        var command = new UpdateConfigAppCommand
        {
            AppId = 1,
            Client = "Test Client"
        };

        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS8620, CS8625] Use proper typed null with Result<ConfigApp>
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>())
                   .Returns(Result<ConfigApp?>.Success((ConfigApp?)null));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("ConfigApp not found");
    }

    /// <summary>
    /// Executes Should_Return_Failure_When_Update_Fails operation.
    /// </summary>
    /// <returns>The result of Should_Return_Failure_When_Update_Fails.</returns>

    [Fact]
    public async Task Should_Return_Failure_When_Update_Fails()
    {
        // Arrange
        var existingConfigApp = new ConfigApp { AppId = 1 };
        var command = new UpdateConfigAppCommand { AppId = 1 };

        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS8620] Changed return type from Result<ConfigApp?> to Result<ConfigApp>
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>())
                   .Returns(Result<ConfigApp?>.Success(existingConfigApp));
        _repository.UpdateAsync(Arg.Any<ConfigApp>(), Arg.Any<CancellationToken>())
                   .Returns(Result.WithFailure("Database update failed"));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Database update failed");
    }

    /// <summary>
    /// Executes Should_Return_Failure_When_Commit_Fails operation.
    /// </summary>
    /// <returns>The result of Should_Return_Failure_When_Commit_Fails.</returns>

    [Fact]
    public async Task Should_Return_Failure_When_Commit_Fails()
    {
        // Arrange
        var existingConfigApp = new ConfigApp { AppId = 1 };
        var command = new UpdateConfigAppCommand { AppId = 1 };

        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS8620] Changed return type from Result<ConfigApp?> to Result<ConfigApp>
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>())
                   .Returns(Result<ConfigApp?>.Success(existingConfigApp));
        _repository.UpdateAsync(Arg.Any<ConfigApp>(), Arg.Any<CancellationToken>())
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
    /// Executes Should_Preserve_Existing_Values_When_Command_Properties_Are_Null operation.
    /// </summary>
    /// <returns>The result of Should_Preserve_Existing_Values_When_Command_Properties_Are_Null.</returns>

    [Fact]
    public async Task Should_Preserve_Existing_Values_When_Command_Properties_Are_Null()
    {
        // Arrange
        var existingConfigApp = new ConfigApp
        {
            AppId = 1,
            Client = "Original Client",
            Factory = "Original Factory",
            Line = "Original Line",
            Project = "Original Project",
            Version = "Original Version"
        };

        var command = new UpdateConfigAppCommand
        {
            AppId = 1,
            //[Fix]
            //CLAUDE
            //Date: 29/08/2025
            //Reason: [CS8625] Use proper null with cast
            Client = (string?)null!, // Should preserve existing
            Factory = "Updated Factory",
            //[Fix]
            //CLAUDE
            //Date: 29/08/2025
            //Reason: [CS8625] Use proper null with cast
            Line = (string?)null!, // Should preserve existing
            Project = "Updated Project",
            //[Fix]
            //CLAUDE
            //Date: 29/08/2025
            //Reason: [CS8625] Use proper null with cast
            Version = (string?)null! // Should preserve existing
        };

        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS8620] Changed return type from Result<ConfigApp?> to Result<ConfigApp>
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>())
                   .Returns(Result<ConfigApp?>.Success(existingConfigApp));
        _repository.UpdateAsync(Arg.Any<ConfigApp>(), Arg.Any<CancellationToken>())
                   .Returns(Result.Success());
        _repository.CommitAsync(Arg.Any<CancellationToken>())
                   .Returns(Result.Success());

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        //[Fix]
        //CLAUDE
        //Date: 20/08/2025
        //Reason: Access Value after success check to satisfy null safety - CS8602 warning fix
        var value = result.Value.ShouldNotBeNull();
        value.Client.ShouldBe("Original Client"); // Preserved
        value.Factory.ShouldBe("Updated Factory"); // Updated
        value.Line.ShouldBe("Original Line"); // Preserved
        value.Project.ShouldBe("Updated Project"); // Updated
        value.Version.ShouldBe("Original Version"); // Preserved
    }

    /// <summary>
    /// Executes Should_Log_Error_When_ConfigApp_Not_Found operation.
    /// </summary>
    /// <returns>The result of Should_Log_Error_When_ConfigApp_Not_Found.</returns>

    [Fact]
    public async Task Should_Log_Error_When_ConfigApp_Not_Found()
    {
        // Arrange
        var command = new UpdateConfigAppCommand { AppId = 999 };

        _repository.GetByIdAsync(999, Arg.Any<CancellationToken>())
                   .Returns(Result<ConfigApp?>.Success(null!));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("ConfigApp not found");
    }

    /// <summary>
    /// Executes Should_Log_Error_When_Update_Fails operation.
    /// </summary>
    /// <returns>The result of Should_Log_Error_When_Update_Fails.</returns>

    [Fact]
    public async Task Should_Log_Error_When_Update_Fails()
    {
        // Arrange
        var existingConfigApp = new ConfigApp { AppId = 1 };
        var command = new UpdateConfigAppCommand { AppId = 1 };

        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS8620] Changed return type from Result<ConfigApp?> to Result<ConfigApp>
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>())
                   .Returns(Result<ConfigApp?>.Success(existingConfigApp));
        _repository.UpdateAsync(Arg.Any<ConfigApp>(), Arg.Any<CancellationToken>())
                   .Returns(Result.WithFailure(["Database update error"]));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Database update error");
    }

    /// <summary>
    /// Executes Should_Log_Error_When_Commit_Fails operation.
    /// </summary>
    /// <returns>The result of Should_Log_Error_When_Commit_Fails.</returns>

    [Fact]
    public async Task Should_Log_Error_When_Commit_Fails()
    {
        // Arrange
        var existingConfigApp = new ConfigApp { AppId = 1 };
        var command = new UpdateConfigAppCommand { AppId = 1 };

        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS8620] Changed return type from Result<ConfigApp?> to Result<ConfigApp>
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>())
                   .Returns(Result<ConfigApp?>.Success(existingConfigApp));
        _repository.UpdateAsync(Arg.Any<ConfigApp>(), Arg.Any<CancellationToken>())
                   .Returns(Result.Success());
        _repository.CommitAsync(Arg.Any<CancellationToken>())
                   .Returns(Result.WithFailure(["Commit transaction failed"]));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Commit transaction failed");
    }
}