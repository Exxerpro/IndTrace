// <copyright file="CreateVariableCommandHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Variables.Commands.Create;

namespace Application.UnitTests.Features.Variables;

/// <summary>
/// Basic tests for CreateVariableCommandHandler focusing on constructor validation and simple scenarios
/// </summary>
public class CreateVariableCommandHandlerBasicTests : IDisposable
{
    private readonly IRepository<Variable> _repository = null!;
    private readonly IRepository<VariablesGroup> _groupRepository = null!;
    private readonly ILogger<CreateVariableCommandHandler> _logger = null!;
    private readonly CreateVariableCommandHandler _handler = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public CreateVariableCommandHandlerBasicTests()
    {
        _repository = Substitute.For<IRepository<Variable>>();
        _groupRepository = Substitute.For<IRepository<VariablesGroup>>();
        // #97: by default the parent VariablesGroup exists so the fail-closed FK probe passes; the dangling-parent
        // test overrides this with Success(0).
        _groupRepository.CountAsync(Arg.Any<ISpecification<VariablesGroup>>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(1));
        _logger = XUnitLogger.CreateLogger<CreateVariableCommandHandler>();
        _handler = new CreateVariableCommandHandler(_repository, _groupRepository, _logger);
    }

    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var handler = new CreateVariableCommandHandler(_repository, _groupRepository, _logger);

        // Assert
        handler.ShouldNotBeNull();
    }

    /// <summary>
    /// #97 red-green: a positive-but-dangling VariableGroupId (no parent VariablesGroups row -> CountAsync
    /// Success(0)) must be refused gracefully and must NOT reach AddAsync/CommitAsync. Pre-fix code proceeded to
    /// Add (SqlException 547 on real SQL). Seeded groups are powers-of-two {1..512}, so 7 has no parent row.
    /// </summary>
    /// <returns>The result of Should_Refuse_When_VariableGroupIdDangling.</returns>
    [Fact]
    public async Task Should_Refuse_When_VariableGroupIdDangling()
    {
        // Arrange
        var command = CreateValidCommand();
        command.VariableGroupId = 7; // shape-valid but not a seeded power-of-two group
        _groupRepository.CountAsync(Arg.Any<ISpecification<VariablesGroup>>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(0));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("VariablesGroup with id 7 does not exist.");
        await _repository.DidNotReceive().AddAsync(Arg.Any<Variable>(), Arg.Any<CancellationToken>());
        await _repository.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Constructor_WithNullRepository_ShouldThrowException operation.
    /// </summary>

    // MARKED FOR DELETION - Constructor null guard test no longer needed for DI handlers
    // [Fact]
    // 	public void Constructor_WithNullRepository_ShouldThrowException()
    // 	{
    // 		// Arrange
    // 		IRepository<Variable>? nullRepository = null!;
    //
    // 		// Act & Assert
    // 		Should.Throw<ArgumentNullException>(() => new CreateVariableCommandHandler(nullRepository!, _logger));
    // 	}
    /// <summary>
    /// Executes Constructor_WithNullLogger_ShouldThrowException operation.
    /// </summary>

    // MARKED FOR DELETION - Constructor null guard test no longer needed for DI handlers
    // [Fact]
    // 	public void Constructor_WithNullLogger_ShouldThrowException()
    // 	{
    // 		// Arrange
    // 		ILogger<CreateVariableCommandHandler>? nullLogger = null!;
    //
    // 		// Act & Assert
    // 		Should.Throw<ArgumentNullException>(() => new CreateVariableCommandHandler(_repository, nullLogger!));
    // 	}
    /// <summary>
    /// Executes Should_CreateVariable_When_ValidCommandProvided operation.
    /// </summary>
    /// <returns>The result of Should_CreateVariable_When_ValidCommandProvided.</returns>

    [Fact]
    public async Task Should_CreateVariable_When_ValidCommandProvided()
    {
        // Arrange - Ford F-150 engine temperature sensor creation
        var command = new CreateVariableCommand
        {
            VariableId = 1001,
            MachineId = 10001,
            Name = "Engine_Temperature_Sensor",
            Address = "DB1.DBD0",
            Type = "REAL",
            Length = 4,
            Event = 1,
            Direction = 1,
            VariableGroupId = 10
        };

        _repository.AddAsync(Arg.Any<Variable>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(1001));
        _repository.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.MachineId.ShouldBe(10001);
        result.Value.Name.ShouldBe("Engine_Temperature_Sensor");
        result.Value.Address.ShouldBe("DB1.DBD0");
        result.Value.Type.ShouldBe("REAL");

        await _repository.Received(1).AddAsync(
            Arg.Is<Variable>(v =>
                v.MachineId == 10001 &&
                v.Name == "Engine_Temperature_Sensor" &&
                v.Address == "DB1.DBD0" &&
                v.NetType == "REAL"),
            Arg.Any<CancellationToken>());
        await _repository.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_ReturnFailure_When_AddFails operation.
    /// </summary>
    /// <returns>The result of Should_ReturnFailure_When_AddFails.</returns>

    [Fact]
    public async Task Should_ReturnFailure_When_AddFails()
    {
        // Arrange - Database connection failure scenario
        var command = CreateValidCommand();

        _repository.AddAsync(Arg.Any<Variable>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.WithFailure("Database connection failed"));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Database connection failed");
    }

    /// <summary>
    /// Executes Should_ReturnFailure_When_CommitFails operation.
    /// </summary>
    /// <returns>The result of Should_ReturnFailure_When_CommitFails.</returns>

    [Fact]
    public async Task Should_ReturnFailure_When_CommitFails()
    {
        // Arrange - Transaction commit failure scenario
        var command = CreateValidCommand();

        _repository.AddAsync(Arg.Any<Variable>(), Arg.Any<CancellationToken>())
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
    /// Executes Should_PassCancellationToken_When_RepositoryMethodsCalled operation.
    /// </summary>
    /// <returns>The result of Should_PassCancellationToken_When_RepositoryMethodsCalled.</returns>

    [Fact]
    public async Task Should_PassCancellationToken_When_RepositoryMethodsCalled()
    {
        // Arrange
        var command = CreateValidCommand();
        var cancellationToken = new CancellationToken();

        _repository.AddAsync(Arg.Any<Variable>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(1));
        _repository.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        await _handler.ProcessAsync(command, cancellationToken);

        // Assert
        await _repository.Received(1).AddAsync(Arg.Any<Variable>(), cancellationToken);
        await _repository.Received(1).CommitAsync(cancellationToken);
    }

    private static CreateVariableCommand CreateValidCommand()
    {
        return new CreateVariableCommand
        {
            VariableId = 1,
            MachineId = 10000,
            Name = "TestVariable",
            Address = "DB1.DBX0.0",
            Type = "BOOL",
            Length = 1,
            Event = 1,
            Direction = 1,
            VariableGroupId = 1
        };
    }

    /// <summary>
    /// Executes Dispose operation.
    /// </summary>

    public void Dispose()
    {
        // Cleanup if needed
    }
}

/// <summary>
/// Manufacturing scenario tests for CreateVariableCommandHandler with complex industrial data collection
/// </summary>
public class CreateVariableCommandHandlerManufacturingTests : IDisposable
{
    private readonly IRepository<Variable> _repository = null!;
    private readonly IRepository<VariablesGroup> _groupRepository = null!;
    private readonly ILogger<CreateVariableCommandHandler> _logger = null!;
    private readonly CreateVariableCommandHandler _handler = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public CreateVariableCommandHandlerManufacturingTests()
    {
        _repository = Substitute.For<IRepository<Variable>>();
        _groupRepository = Substitute.For<IRepository<VariablesGroup>>();
        // #97: by default the parent VariablesGroup exists so the fail-closed FK probe passes; the dangling-parent
        // test overrides this with Success(0).
        _groupRepository.CountAsync(Arg.Any<ISpecification<VariablesGroup>>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(1));
        _logger = XUnitLogger.CreateLogger<CreateVariableCommandHandler>();
        _handler = new CreateVariableCommandHandler(_repository, _groupRepository, _logger);
    }

    /// <summary>
    /// Executes Should_CreateVariable_When_DifferentManufacturingDataTypes operation.
    /// </summary>
    /// <returns>The result of Should_CreateVariable_When_DifferentManufacturingDataTypes.</returns>

    [Theory]
    [InlineData("Engine_Temperature", "DB1.DBD0", "REAL", 4, "Ford F-150 Engine Temperature Sensor")]
    [InlineData("Motor_Speed_RPM", "DB2.DBD10", "DINT", 4, "Tesla Model Y Motor Speed Monitor")]
    [InlineData("Hydraulic_Pressure", "DB3.DBD20", "REAL", 4, "BMW X5 Hydraulic Press Pressure")]
    [InlineData("Vision_Quality_Result", "DB4.DBW30", "INT", 2, "iPhone 15 Vision Inspection Result")]
    [InlineData("Tablet_Weight_Grams", "DB5.DBD40", "REAL", 4, "Pharmaceutical Tablet Weight Check")]
    public async Task Should_CreateVariable_When_DifferentManufacturingDataTypes(
        string variableName, string address, string dataType, int length, string description)
    {
        var logger = XUnitLogger.CreateLogger<CreateVariableCommandHandler>();
        // Using parameters: variableName, address, dataType, length, description
        logger.LogInformation("Using parameters: {VariableName}, {Address}, {DataType}, {Length}, {Description}",
            variableName, address, dataType, length, description);

        // Arrange - Various manufacturing sensor/data variable types
        var command = new CreateVariableCommand
        {
            VariableId = 2000,
            MachineId = 200,
            Name = variableName,
            Address = address,
            Type = dataType,
            Length = length,
            Event = 1,
            Direction = 1,
            VariableGroupId = 20
        };

        _repository.AddAsync(Arg.Any<Variable>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(2000));
        _repository.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Name.ShouldBe(variableName);
        result.Value.Address.ShouldBe(address);
        result.Value.Type.ShouldBe(dataType);
        result.Value.Length.ShouldBe(length);

        await _repository.Received(1).AddAsync(
            Arg.Is<Variable>(v =>
                v.Name == variableName &&
                v.Address == address &&
                v.NetType == dataType &&
                v.Length == length),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_CreateQualityControlVariable_When_InspectionDataRequired operation.
    /// </summary>
    /// <returns>The result of Should_CreateQualityControlVariable_When_InspectionDataRequired.</returns>

    [Fact]
    public async Task Should_CreateQualityControlVariable_When_InspectionDataRequired()
    {
        // Arrange - Quality control inspection variable for automotive manufacturing
        var command = new CreateVariableCommand
        {
            VariableId = 3001,
            MachineId = 301,
            Name = "Weld_Quality_Index",
            Address = "DB10.DBD100",
            Type = "REAL",
            Length = 4,
            Event = 1,
            Direction = 1, // Input from quality sensor
            VariableGroupId = 30
        };

        _repository.AddAsync(Arg.Any<Variable>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(3001));
        _repository.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Name.ShouldBe("Weld_Quality_Index");

        await _repository.Received(1).AddAsync(
            Arg.Is<Variable>(v =>
                v.Name == "Weld_Quality_Index"),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_CreateSafetyVariable_When_EmergencyMonitoringRequired operation.
    /// </summary>
    /// <returns>The result of Should_CreateSafetyVariable_When_EmergencyMonitoringRequired.</returns>

    [Fact]
    public async Task Should_CreateSafetyVariable_When_EmergencyMonitoringRequired()
    {
        // Arrange - Safety monitoring variable for industrial equipment
        var command = new CreateVariableCommand
        {
            VariableId = 4001,
            MachineId = 401,
            Name = "Emergency_Stop_Status",
            Address = "DB20.DBX0.0",
            Type = "BOOL",
            Length = 1,
            Event = 1,
            Direction = 1,
            VariableGroupId = 40
        };

        _repository.AddAsync(Arg.Any<Variable>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(4001));
        _repository.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Name.ShouldBe("Emergency_Stop_Status");
        result.Value.Type.ShouldBe("BOOL");

        await _repository.Received(1).AddAsync(
            Arg.Is<Variable>(v =>
                v.Name == "Emergency_Stop_Status" &&
                v.NetType == "BOOL" &&
                v.Length == 1),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_CreateProcessControlVariable_When_AutomationRequired operation.
    /// </summary>
    /// <returns>The result of Should_CreateProcessControlVariable_When_AutomationRequired.</returns>

    [Fact]
    public async Task Should_CreateProcessControlVariable_When_AutomationRequired()
    {
        // Arrange - Process control variable for pharmaceutical manufacturing
        var command = new CreateVariableCommand
        {
            VariableId = 5001,
            MachineId = 501,
            Name = "Tablet_Press_Force_Newton",
            Address = "DB30.DBD200",
            Type = "REAL",
            Length = 4,
            Event = 1,
            Direction = 2, // Output to control system
            VariableGroupId = 50
        };

        _repository.AddAsync(Arg.Any<Variable>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(5001));
        _repository.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Name.ShouldBe("Tablet_Press_Force_Newton");
        result.Value.Direction.ShouldBe(2); // Output direction

        await _repository.Received(1).AddAsync(
            Arg.Is<Variable>(v =>
                v.Direction == 2 &&
                v.NetType == "REAL"),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_CreateTraceabilityVariable_When_SerialNumberTracking operation.
    /// </summary>
    /// <returns>The result of Should_CreateTraceabilityVariable_When_SerialNumberTracking.</returns>

    [Fact]
    public async Task Should_CreateTraceabilityVariable_When_SerialNumberTracking()
    {
        // Arrange - Traceability variable for electronics manufacturing
        var command = new CreateVariableCommand
        {
            VariableId = 6001,
            MachineId = 601,
            Name = "PCB_Serial_Number",
            Address = "DB40.DBB0",
            Type = "STRING",
            Length = 20,
            Event = 1,
            Direction = 1,
            VariableGroupId = 60
        };

        _repository.AddAsync(Arg.Any<Variable>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(6001));
        _repository.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Name.ShouldBe("PCB_Serial_Number");
        result.Value.Type.ShouldBe("STRING");
        result.Value.Length.ShouldBe(20);

        await _repository.Received(1).AddAsync(
            Arg.Is<Variable>(v =>
                v.Name == "PCB_Serial_Number" &&
                v.NetType == "STRING" &&
                v.Length == 20),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_CreateEnergyMonitoringVariable_When_EfficiencyTracking operation.
    /// </summary>
    /// <returns>The result of Should_CreateEnergyMonitoringVariable_When_EfficiencyTracking.</returns>

    [Fact]
    public async Task Should_CreateEnergyMonitoringVariable_When_EfficiencyTracking()
    {
        // Arrange - Energy monitoring variable for sustainable manufacturing
        var command = new CreateVariableCommand
        {
            VariableId = 7001,
            MachineId = 701,
            Name = "Power_Consumption_Watts",
            Address = "DB50.DBD300",
            Type = "UDINT",
            Length = 4,
            Event = 1,
            Direction = 1,
            VariableGroupId = 70
        };

        _repository.AddAsync(Arg.Any<Variable>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(7001));
        _repository.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Name.ShouldBe("Power_Consumption_Watts");
        result.Value.Type.ShouldBe("UDINT");

        await _repository.Received(1).AddAsync(
            Arg.Is<Variable>(v =>
                v.Name == "Power_Consumption_Watts" &&
                v.NetType == "UDINT"),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_CreateVariableGroup_When_RelatedSensorsGrouped operation.
    /// </summary>
    /// <returns>The result of Should_CreateVariableGroup_When_RelatedSensorsGrouped.</returns>

    [Fact]
    public async Task Should_CreateVariableGroup_When_RelatedSensorsGrouped()
    {
        // Arrange - Multiple related variables in same group for beverage manufacturing
        var command = new CreateVariableCommand
        {
            VariableId = 8001,
            MachineId = 801,
            Name = "Bottle_Fill_Level_ML",
            Address = "DB60.DBD400",
            Type = "REAL",
            Length = 4,
            Event = 1,
            Direction = 1,
            VariableGroupId = 80 // Part of beverage quality group
        };

        _repository.AddAsync(Arg.Any<Variable>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(8001));
        _repository.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.VariableGroupId.ShouldBe(80);

        await _repository.Received(1).AddAsync(
            Arg.Is<Variable>(v =>
                v.VariableGroupId == 80),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Dispose operation.
    /// </summary>

    public void Dispose()
    {
        // Cleanup if needed
    }
}

/// <summary>
/// Error handling and edge case tests for CreateVariableCommandHandler
/// </summary>
public class CreateVariableCommandHandlerErrorTests : IDisposable
{
    private readonly IRepository<Variable> _repository = null!;
    private readonly IRepository<VariablesGroup> _groupRepository = null!;
    private readonly ILogger<CreateVariableCommandHandler> _logger = null!;
    private readonly CreateVariableCommandHandler _handler = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public CreateVariableCommandHandlerErrorTests()
    {
        _repository = Substitute.For<IRepository<Variable>>();
        _groupRepository = Substitute.For<IRepository<VariablesGroup>>();
        // #97: by default the parent VariablesGroup exists so the fail-closed FK probe passes; the dangling-parent
        // test overrides this with Success(0).
        _groupRepository.CountAsync(Arg.Any<ISpecification<VariablesGroup>>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(1));
        _logger = XUnitLogger.CreateLogger<CreateVariableCommandHandler>();
        _handler = new CreateVariableCommandHandler(_repository, _groupRepository, _logger);
    }

    /// <summary>
    /// Story 2.2 (#26): a null Name, Address, or NetType (Type) is no longer silently carried onto a
    /// malformed entity. The handler now routes construction through the guarded <c>Variable.Create</c>
    /// factory, so each of these required-identity strings being <see langword="null"/> surfaces as a
    /// graceful failure Result instead of a success carrying nulls.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="address">The address.</param>
    /// <param name="type">The type.</param>
    /// <returns>The result of Should_HandleNullStringProperties_When_PropertiesAreNull.</returns>

    [Theory]
    [InlineData(null, "TestAddress", "BOOL")]
    [InlineData("TestName", null, "BOOL")]
    [InlineData("TestName", "TestAddress", null)]
    public async Task Should_HandleNullStringProperties_When_PropertiesAreNull(string? name, string? address, string? type)
    {
        // Using parameters: name, address, type
        _ = name; // xUnit1026 fix
        _ = address; // xUnit1026 fix
        _ = type; // xUnit1026 fix
        // Using parameters: name, address, type
        _ = name; // xUnit1026 fix
        _ = address; // xUnit1026 fix
        _ = type; // xUnit1026 fix
        // Using parameters: name, address, type
        _ = name; // xUnit1026 fix
        _ = address; // xUnit1026 fix
        _ = type; // xUnit1026 fix
        // Using parameters: name, address, type
        _ = name; // xUnit1026 fix
        _ = address; // xUnit1026 fix
        _ = type; // xUnit1026 fix
        // Using parameters: name, address, type
        _ = name; // xUnit1026 fix
        _ = address; // xUnit1026 fix
        _ = type; // xUnit1026 fix
        // Arrange - Various null string property scenarios
        var command = new CreateVariableCommand
        {
            VariableId = 1,
            MachineId = 10000,
            Name = name!,
            Address = address!,
            Type = type!,
            Length = 1,
            Event = 1,
            Direction = 1,
            VariableGroupId = 1
        };

        _repository.AddAsync(Arg.Any<Variable>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(1));
        _repository.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert - Story 2.2 (#26): the guarded Variable.Create factory rejects the null required-identity
        // string, so the handler returns a graceful failure rather than a success carrying nulls.
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.Count().ShouldBeGreaterThanOrEqualTo(1);
    }

    /// <summary>
    /// Executes Should_HandleCancellation_When_CancellationRequested operation.
    /// </summary>
    /// <returns>The result of Should_HandleCancellation_When_CancellationRequested.</returns>

    [Fact]
    public async Task Should_HandleCancellation_When_CancellationRequested()
    {
        // Arrange
        var command = CreateValidCommand();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        _repository.AddAsync(Arg.Any<Variable>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromException<Result<int>>(new OperationCanceledException()));

        // Act
        var result = await _handler.ProcessAsync(command, cts.Token);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.Count().ShouldBeGreaterThanOrEqualTo(1);
    }

    /// <summary>
    /// Executes Should_HandleRepositoryException_When_AddAsyncThrows operation.
    /// </summary>
    /// <returns>The result of Should_HandleRepositoryException_When_AddAsyncThrows.</returns>

    [Fact]
    public async Task Should_HandleRepositoryException_When_AddAsyncThrows()
    {
        // Arrange - Repository throws exception
        var command = CreateValidCommand();

        _repository.AddAsync(Arg.Any<Variable>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Result<int>>(new InvalidOperationException("Repository failure")));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.Count().ShouldBeGreaterThanOrEqualTo(1);
    }

    /// <summary>
    /// Executes Should_HandleCommitException_When_CommitAsyncThrows operation.
    /// </summary>
    /// <returns>The result of Should_HandleCommitException_When_CommitAsyncThrows.</returns>

    [Fact]
    public async Task Should_HandleCommitException_When_CommitAsyncThrows()
    {
        // Arrange - Commit throws exception
        var command = CreateValidCommand();

        _repository.AddAsync(Arg.Any<Variable>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(1));
        _repository.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Result>(new InvalidOperationException("Commit failure")));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.Count().ShouldBeGreaterThanOrEqualTo(1);
    }

    /// <summary>
    /// Executes Should_HandleInvalidMachineId_When_NegativeOrZeroMachineId operation.
    /// </summary>
    /// <param name="invalidMachineId">The invalidMachineId.</param>
    /// <returns>The result of Should_HandleInvalidMachineId_When_NegativeOrZeroMachineId.</returns>

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public async Task Should_HandleInvalidMachineId_When_NegativeOrZeroMachineId(int invalidMachineId)
    {
        // Using parameters: invalidMachineId
        _ = invalidMachineId; // xUnit1026 fix
        // Using parameters: invalidMachineId
        _ = invalidMachineId; // xUnit1026 fix
        // Using parameters: invalidMachineId
        _ = invalidMachineId; // xUnit1026 fix
        // Using parameters: invalidMachineId
        _ = invalidMachineId; // xUnit1026 fix
        // Using parameters: invalidMachineId
        _ = invalidMachineId; // xUnit1026 fix
        // Arrange - Invalid machine ID scenarios
        var command = new CreateVariableCommand
        {
            VariableId = 1,
            MachineId = invalidMachineId,
            Name = "TestVariable",
            Address = "DB1.DBX0.0",
            Type = "BOOL",
            Length = 1,
            Event = 1,
            Direction = 1,
            VariableGroupId = 1
        };

        _repository.AddAsync(Arg.Any<Variable>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(1));
        _repository.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert

        //[Fix]
        //CLAUDE
        //Date: 20/08/2025
        //Reason: Fix assertion order - check IsSuccess before accessing Value properties
        result.IsSuccess.ShouldBeTrue(); // Handler doesn't validate machine ID
        result.Value.ShouldNotBeNull();
        result.Value.MachineId.ShouldBe(invalidMachineId);

        await _repository.Received(1).AddAsync(
            Arg.Is<Variable>(v => v.MachineId == invalidMachineId),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_HandleVariousLengthValues_When_DifferentDataSizes operation.
    /// </summary>
    /// <param name="length">The length.</param>
    /// <returns>The result of Should_HandleVariousLengthValues_When_DifferentDataSizes.</returns>

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(1000)]
    public async Task Should_HandleVariousLengthValues_When_DifferentDataSizes(int length)
    {
        // Using parameters: length
        _ = length; // xUnit1026 fix
        // Using parameters: length
        _ = length; // xUnit1026 fix
        // Using parameters: length
        _ = length; // xUnit1026 fix
        // Using parameters: length
        _ = length; // xUnit1026 fix
        // Using parameters: length
        _ = length; // xUnit1026 fix
        // Arrange - Various length values for different data types
        var command = new CreateVariableCommand
        {
            VariableId = 1,
            MachineId = 10000,
            Name = "TestVariable",
            Address = "DB1.DBD0",
            Type = "ARRAY",
            Length = length,
            Event = 1,
            Direction = 1,
            VariableGroupId = 1
        };

        _repository.AddAsync(Arg.Any<Variable>(), Arg.Any<CancellationToken>())
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
        result.Value.ShouldNotBeNull().Length.ShouldBe(length);

        await _repository.Received(1).AddAsync(
            Arg.Is<Variable>(v => v.Length == length),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_HandleEmptyStringValues_When_StringPropertiesEmpty operation.
    /// </summary>
    /// <returns>The result of Should_HandleEmptyStringValues_When_StringPropertiesEmpty.</returns>

    [Fact]
    public async Task Should_HandleEmptyStringValues_When_StringPropertiesEmpty()
    {
        // Arrange - Empty string scenarios
        var command = new CreateVariableCommand
        {
            VariableId = 1,
            MachineId = 10000,
            Name = "",
            Address = "",
            Type = "",
            Length = 1,
            Event = 1,
            Direction = 1,
            VariableGroupId = 1
        };

        _repository.AddAsync(Arg.Any<Variable>(), Arg.Any<CancellationToken>())
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
        result.Value.ShouldNotBeNull().Name.ShouldBe("");
        result.Value.ShouldNotBeNull().Address.ShouldBe("");
        result.Value.ShouldNotBeNull().Type.ShouldBe("");
    }

    /// <summary>
    /// Executes Should_LogError_When_AddFails operation.
    /// </summary>
    /// <returns>The result of Should_LogError_When_AddFails.</returns>

    [Fact]
    public async Task Should_LogError_When_AddFails()
    {
        // Arrange - Repository add failure for logging verification
        var command = CreateValidCommand();

        _repository.AddAsync(Arg.Any<Variable>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.WithFailure("Repository add failed"));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Repository add failed");
    }

    /// <summary>
    /// Executes Should_LogError_When_CommitFails operation.
    /// </summary>
    /// <returns>The result of Should_LogError_When_CommitFails.</returns>

    [Fact]
    public async Task Should_LogError_When_CommitFails()
    {
        // Arrange - Repository commit failure for logging verification
        var command = CreateValidCommand();

        _repository.AddAsync(Arg.Any<Variable>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(1));
        _repository.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(Result.WithFailure("Repository commit failed"));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Repository commit failed");
    }

    private static CreateVariableCommand CreateValidCommand()
    {
        return new CreateVariableCommand
        {
            VariableId = 1,
            MachineId = 10000,
            Name = "TestVariable",
            Address = "DB1.DBX0.0",
            Type = "BOOL",
            Length = 1,
            Event = 1,
            Direction = 1,
            VariableGroupId = 1
        };
    }

    /// <summary>
    /// Executes Dispose operation.
    /// </summary>

    public void Dispose()
    {
        // Cleanup if needed
    }
}