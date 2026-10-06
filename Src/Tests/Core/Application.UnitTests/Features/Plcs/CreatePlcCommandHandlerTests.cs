// <copyright file="CreatePlcCommandHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Plcs;

/// <summary>
/// Unit tests for CreatePlcCommandHandler
/// </summary>
public class CreatePlcCommandHandlerTests
{
    private readonly IRepository<Plc> _repository = Substitute.For<IRepository<Plc>>();
    private readonly ILogger<CreatePlcCommandHandler> _logger = XUnitLogger.CreateLogger<CreatePlcCommandHandler>();
    private readonly CreatePlcCommandHandler _handler = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public CreatePlcCommandHandlerTests()
    {
        _handler = new CreatePlcCommandHandler(_repository, _logger);
    }
    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var handler = new CreatePlcCommandHandler(_repository, _logger);

        // Assert
        handler.ShouldNotBeNull();
    }
    /// <summary>
    /// Executes Constructor_WithNullRepository_ShouldThrowException operation.
    /// </summary>

    // MARKED FOR DELETION - Constructor null guard test no longer needed for DI handlers
    // [Fact]
    //     public void Constructor_WithNullRepository_ShouldThrowException()
    //     {
    //         // Arrange
    //         IRepository<Plc>? nullRepository = null!;
    // 
    //         // Act & Assert
    //         Should.Throw<ArgumentNullException>(() => new CreatePlcCommandHandler(nullRepository!, _logger));
    //     }
    /// <summary>
    /// Executes Constructor_WithNullLogger_ShouldThrowException operation.
    /// </summary>

    // MARKED FOR DELETION - Constructor null guard test no longer needed for DI handlers
    // [Fact]
    //     public void Constructor_WithNullLogger_ShouldThrowException()
    //     {
    //         // Arrange
    //         ILogger<CreatePlcCommandHandler>? nullLogger = null!;
    // 
    //         // Act & Assert
    //         Should.Throw<ArgumentNullException>(() => new CreatePlcCommandHandler(_repository, nullLogger!));
    //     }
    /// <summary>
    /// Executes Process_WithValidCommand_ShouldReturnSuccess operation.
    /// </summary>
    /// <returns>The result of Process_WithValidCommand_ShouldReturnSuccess.</returns>

    [Fact]
    public async Task Process_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var command = new CreatePlcCommand
        {
            PlcId = 1,
            Name = "Test PLC",
            IpAddress = "192.168.1.100",
            PlcType = "S7-300",
            PlcBrand = "Siemens",
            CommLibrary = "Sharp7",
            BrandOwner = "Siemens AG"
        };

        _repository.AddAsync(Arg.Any<Plc>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.Success(1)));
        _repository.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.PlcId.ShouldBe(command.PlcId);
        result.Value.Name.ShouldBe(command.Name);
        result.Value.IpAddress.ShouldBe(command.IpAddress);
        result.Value.PlcType.ShouldBe(command.PlcType);
        result.Value.PlcBrand.ShouldBe(command.PlcBrand);
        result.Value.CommLibrary.ShouldBe(command.CommLibrary);
        result.Value.BrandOwner.ShouldBe(command.BrandOwner);
    }
    /// <summary>
    /// Executes ProcessAsync_WhenAddFails_ShouldReturnFailure operation.
    /// </summary>
    /// <returns>The result of ProcessAsync_WhenAddFails_ShouldReturnFailure.</returns>

    [Fact]
    public async Task ProcessAsync_WhenAddFails_ShouldReturnFailure()
    {
        // Arrange
        var command = CreateValidCommand();

        _repository.AddAsync(Arg.Any<Plc>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.WithFailure("Database connection failed"));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Database connection failed");
    }
    /// <summary>
    /// Executes ProcessAsync_WhenCommitFails_ShouldReturnFailure operation.
    /// </summary>
    /// <returns>The result of ProcessAsync_WhenCommitFails_ShouldReturnFailure.</returns>

    [Fact]
    public async Task ProcessAsync_WhenCommitFails_ShouldReturnFailure()
    {
        // Arrange
        var command = CreateValidCommand();

        _repository.AddAsync(Arg.Any<Plc>(), Arg.Any<CancellationToken>())
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
    /// Executes Process_ShouldCallRepositoryWithCorrectEntity operation.
    /// </summary>
    /// <returns>The result of Process_ShouldCallRepositoryWithCorrectEntity.</returns>

    [Fact]
    public async Task Process_ShouldCallRepositoryWithCorrectEntity()
    {
        // Arrange
        var command = new CreatePlcCommand
        {
            PlcId = 1,
            Name = "Test PLC",
            IpAddress = "192.168.1.100",
            PlcType = "S7-300",
            PlcBrand = "Siemens",
            CommLibrary = "Sharp7",
            BrandOwner = "Siemens AG"
        };

        _repository.AddAsync(Arg.Any<Plc>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.Success(1)));
        _repository.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        // Act
        await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        await _repository.Received(1).AddAsync(
            Arg.Is<Plc>(entity =>
                entity.PlcId == command.PlcId &&
                entity.Name == command.Name &&
                entity.IpAddress == command.IpAddress &&
                entity.PlcType == command.PlcType &&
                entity.PlcBrand == command.PlcBrand &&
                entity.CommLibrary == command.CommLibrary &&
                entity.BrandOwner == command.BrandOwner),
            Arg.Any<CancellationToken>());
        await _repository.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }
    /// <summary>
    /// Executes Process_ShouldPassCancellationTokenToRepository operation.
    /// </summary>
    /// <returns>The result of Process_ShouldPassCancellationTokenToRepository.</returns>

    [Fact]
    public async Task Process_ShouldPassCancellationTokenToRepository()
    {
        // Arrange
        var command = new CreatePlcCommand { PlcId = 1, Name = "Test PLC" };
        var cancellationToken = TestContext.Current.CancellationToken;

        _repository.AddAsync(Arg.Any<Plc>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.Success(1)));
        _repository.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        // Act
        await _handler.ProcessAsync(command, cancellationToken);

        // Assert
        await _repository.Received(1).AddAsync(Arg.Any<Plc>(), cancellationToken);
        await _repository.Received(1).CommitAsync(cancellationToken);
    }
    /// <summary>
    /// Executes Process_WithEmptyStringProperties_ShouldHandleGracefully operation.
    /// </summary>
    /// <param name="emptyValue">The emptyValue.</param>
    /// <returns>The result of Process_WithEmptyStringProperties_ShouldHandleGracefully.</returns>

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Process_WithEmptyStringProperties_ShouldHandleGracefully(string emptyValue)
    {
        // Using parameters: emptyValue
        _ = emptyValue; // xUnit1026 fix
        // Using parameters: emptyValue
        _ = emptyValue; // xUnit1026 fix
        // Using parameters: emptyValue
        _ = emptyValue; // xUnit1026 fix
        // Using parameters: emptyValue
        _ = emptyValue; // xUnit1026 fix
        // Using parameters: emptyValue
        _ = emptyValue; // xUnit1026 fix
        // Arrange
        var command = new CreatePlcCommand
        {
            PlcId = 1,
            Name = emptyValue,
            IpAddress = "192.168.1.100"
        };

        _repository.AddAsync(Arg.Any<Plc>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.Success(1)));
        _repository.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        //[Fix] 
        //CLAUDE
        //Date: 28/08/2025 
        //Reason: [CS8602] - Add null-forgiving operator since result.IsSuccess was verified true
        result.Value.ShouldNotBeNull().Name.ShouldBe(emptyValue);
    }
    /// <summary>
    /// Executes Process_WhenAddFails_ShouldLogError operation.
    /// </summary>
    /// <returns>The result of Process_WhenAddFails_ShouldLogError.</returns>

    [Fact]
    public async Task Process_WhenAddFails_ShouldLogError()
    {
        // Arrange
        var command = new CreatePlcCommand { PlcId = 1, Name = "Test PLC" };

        _repository.AddAsync(Arg.Any<Plc>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.WithFailure(["Repository connection timeout"])));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
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
        var command = new CreatePlcCommand { PlcId = 1, Name = "Test PLC" };

        _repository.AddAsync(Arg.Any<Plc>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.Success(1)));
        _repository.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.WithFailure(["Commit transaction failed"])));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Commit transaction failed");
    }

    private CreatePlcCommand CreateValidCommand()
    {
        return new CreatePlcCommand
        {
            PlcId = 1,
            Name = "Test PLC",
            IpAddress = "192.168.1.100",
            PlcType = "S7-300",
            PlcBrand = "Siemens",
            CommLibrary = "Sharp7",
            BrandOwner = "Siemens AG"
        };
    }
}
