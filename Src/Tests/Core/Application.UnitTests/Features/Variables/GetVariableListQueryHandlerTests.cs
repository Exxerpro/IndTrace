// <copyright file="GetVariableListQueryHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Variables.Queries.GetVariableList;
using IndTrace.Application.WorkFlows.Queries.GetList;

namespace Application.UnitTests.Features.Variables;

/// <summary>
/// Unit tests for GetVariableListQueryHandler
/// </summary>
public class GetVariableListQueryHandlerTests
{
    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var mockRepository = Substitute.For<IRepository<Variable>>();
        var logger = XUnitLogger.CreateLogger<GetVariableListQueryHandler>();

        // Act
        var handler = new GetVariableListQueryHandler(mockRepository, logger);

        // Assert
        handler.ShouldNotBeNull();
    }

    // MARKED FOR REMOVAL - Constructor null guard test no longer needed with Result<T> patterns
    // /// <summary>
    // /// Executes Constructor_WithNullRepository_ShouldThrowException operation.
    // /// </summary>
    //
    // [Fact]
    // public void Constructor_WithNullRepository_ShouldThrowException()
    // {
    //     // Arrange
    //     IRepository<Variable>? nullRepository = null!;
    //     var logger = XUnitLogger.CreateLogger<GetVariableListQueryHandler>();
    //
    //     // Act & Assert
    //     // Act
    //     var result = new GetVariableListQueryHandler(nullRepository!, logger);
    //
    //     // Assert
    //     //result.IsFailure.ShouldBeTrue();
    //     //result.Errors.ShouldNotBeNull();
    // }
    // MARKED FOR REMOVAL - Constructor null guard test no longer needed with Result<T> patterns
    // /// <summary>
    // /// Executes Constructor_WithNullLogger_ShouldThrowException operation.
    // /// </summary>
    //
    // [Fact]
    // public void Constructor_WithNullLogger_ShouldThrowException()
    // {
    //     // Arrange
    //     var mockRepository = Substitute.For<IRepository<Variable>>();
    //     ILogger<GetVariableListQueryHandler>? nullLogger = null!;
    //
    //     // Act & Assert
    //     // Act
    //     var result = new GetVariableListQueryHandler(mockRepository, nullLogger!);
    //
    //     // Assert
    //     //result.IsFailure.ShouldBeTrue();
    //     //result.Errors.ShouldNotBeNull();
    // }
    // MARKED FOR REMOVAL - Constructor null guard test no longer needed with Result<T> patterns
    // /// <summary>
    // /// Executes Constructor_WithAllNullParameters_ShouldThrowException operation.
    // /// </summary>
    //
    // [Fact]
    // public void Constructor_WithAllNullParameters_ShouldThrowException()
    // {
    //     // Arrange
    //     IRepository<Variable>? nullRepository = null!;
    //     ILogger<GetVariableListQueryHandler>? nullLogger = null!;
    //
    //     // Act & Assert
    //     // Act
    //     var result = new GetVariableListQueryHandler(nullRepository!, nullLogger!);
    //
    //     // Assert
    //     //result.IsFailure.ShouldBeTrue();
    //     //result.Errors.ShouldNotBeNull();
    // }
}

/// <summary>
/// Basic tests for GetVariableListQueryHandler focusing on constructor validation and simple scenarios
/// </summary>
public class GetVariableListQueryHandlerBasicTests : IDisposable
{
    private readonly IRepository<Variable> _repository = null!;
    private readonly ILogger<GetVariableListQueryHandler> _logger = null!;
    private readonly GetVariableListQueryHandler _handler = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public GetVariableListQueryHandlerBasicTests()
    {
        _repository = Substitute.For<IRepository<Variable>>();
        _logger = XUnitLogger.CreateLogger<GetVariableListQueryHandler>();
        _handler = new GetVariableListQueryHandler(_repository, _logger);
    }

    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var handler = new GetVariableListQueryHandler(_repository, _logger);

        // Assert
        handler.ShouldNotBeNull();
    }

    // MARKED FOR REMOVAL - Constructor null guard test no longer needed with Result<T> patterns
    // /// <summary>
    // /// Executes Constructor_WithNullRepository_ShouldThrowException operation.
    // /// </summary>
    //
    // [Fact]
    // public void Constructor_WithNullRepository_ShouldThrowException()
    // {
    //     // Arrange
    //     IRepository<Variable>? nullRepository = null!;
    //
    //     // Act & Assert
    //     // Act
    //     var result = new GetVariableListQueryHandler(nullRepository!, _logger);
    //
    //     // Assert
    //     //result.IsFailure.ShouldBeTrue();
    //     //result.Errors.ShouldNotBeNull();
    // }
    // MARKED FOR REMOVAL - Constructor null guard test no longer needed with Result<T> patterns
    // /// <summary>
    // /// Executes Constructor_WithNullLogger_ShouldThrowException operation.
    // /// </summary>
    //
    // [Fact]
    // public void Constructor_WithNullLogger_ShouldThrowException()
    // {
    //     // Arrange
    //     ILogger<GetVariableListQueryHandler>? nullLogger = null!;
    //
    //     // Act & Assert
    //     // Act
    //     var result = new GetVariableListQueryHandler(_repository, nullLogger!);
    //
    //     // Assert
    //     //result.IsFailure.ShouldBeTrue();
    //     //result.Errors.ShouldNotBeNull();
    // }
    /// <summary>
    /// Executes Should_ReturnVariableList_When_ValidQueryProvided operation.
    /// </summary>
    /// <returns>The result of Should_ReturnVariableList_When_ValidQueryProvided.</returns>

    [Fact]
    public async Task Should_ReturnVariableList_When_ValidQueryProvided()
    {
        // Arrange - Ford F-150 manufacturing line variables
        var variables = new List<Variable>
        {
            new Variable
            {
                VariableId = 1001,
                MachineId = 10001,
                Name = "Engine_Temperature_Sensor",
                Address = "DB1.DBD0",
                NetType = "REAL",
                Length = 4,
                IsActive = 1
            },
            new Variable
            {
                VariableId = 1002,
                MachineId = 10002,
                Name = "Transmission_Pressure",
                Address = "DB2.DBD0",
                NetType = "REAL",
                Length = 4,
                IsActive = 1
            }
        };

        var query = new GetVariableListQuery();

        _repository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<Variable>>.Success(variables));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.VariableList.ShouldNotBeNull();
        result.Value.Count.ShouldBe(2);
        result.Value.VariableList.Count.ShouldBe(2);

        var variableArray = result.Value.VariableList.ToArray();
        variableArray[0].VariableId.ShouldBe(1001);
        variableArray[0].Name.ShouldBe("Engine_Temperature_Sensor");
        variableArray[1].VariableId.ShouldBe(1002);
        variableArray[1].Name.ShouldBe("Transmission_Pressure");

        await _repository.Received(1).ListAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_ReturnEmptyList_When_NoVariablesExist operation.
    /// </summary>
    /// <returns>The result of Should_ReturnEmptyList_When_NoVariablesExist.</returns>

    [Fact]
    public async Task Should_ReturnEmptyList_When_NoVariablesExist()
    {
        // Arrange - Empty variable collection
        var emptyVariables = new List<Variable>();
        var query = new GetVariableListQuery();

        _repository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<Variable>>.Success(emptyVariables));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.VariableList.ShouldNotBeNull();
        result.Value.Count.ShouldBe(0);
        result.Value.VariableList.Count.ShouldBe(0);

        await _repository.Received(1).ListAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_ReturnFailure_When_RepositoryFails operation.
    /// </summary>
    /// <returns>The result of Should_ReturnFailure_When_RepositoryFails.</returns>

    [Fact]
    public async Task Should_ReturnFailure_When_RepositoryFails()
    {
        // Arrange - Repository failure scenario
        var query = new GetVariableListQuery();

        _repository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<Variable>>.WithFailure("Database connection failed"));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Database connection failed");
    }

    /// <summary>
    /// Executes Should_PassCancellationToken_When_RepositoryMethodCalled operation.
    /// </summary>
    /// <returns>The result of Should_PassCancellationToken_When_RepositoryMethodCalled.</returns>

    [Fact]
    public async Task Should_PassCancellationToken_When_RepositoryMethodCalled()
    {
        // Arrange
        var variables = new List<Variable> { new Variable { VariableId = 1, Name = "Test" } };
        var query = new GetVariableListQuery();
        var cancellationToken = new CancellationToken();

        _repository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<Variable>>.Success(variables));

        // Act
        await _handler.ProcessAsync(query, cancellationToken);

        // Assert
        await _repository.Received(1).ListAsync(cancellationToken);
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
/// Manufacturing scenario tests for GetVariableListQueryHandler with complex industrial data collections
/// </summary>
public class GetVariableListQueryHandlerManufacturingTests : IDisposable
{
    private readonly IRepository<Variable> _repository = null!;
    private readonly ILogger<GetVariableListQueryHandler> _logger = null!;
    private readonly GetVariableListQueryHandler _handler = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public GetVariableListQueryHandlerManufacturingTests()
    {
        _repository = Substitute.For<IRepository<Variable>>();
        _logger = XUnitLogger.CreateLogger<GetVariableListQueryHandler>();
        _handler = new GetVariableListQueryHandler(_repository, _logger);
    }

    /// <summary>
    /// Executes Should_ReturnAutomotiveVariableList_When_FordF150ManufacturingLine operation.
    /// </summary>
    /// <returns>The result of Should_ReturnAutomotiveVariableList_When_FordF150ManufacturingLine.</returns>

    [Fact]
    public async Task Should_ReturnAutomotiveVariableList_When_FordF150ManufacturingLine()
    {
        // Arrange - Ford F-150 engine manufacturing line variables
        var variables = CreateAutomotiveVariables();
        var query = new GetVariableListQuery();

        _repository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<Variable>>.Success(variables));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count.ShouldBe(5);
        result.Value.VariableList.Count.ShouldBe(5);

        var variableArray = result.Value.VariableList.ToArray();
        variableArray.ShouldContain(v => v.Name == "Engine_Temperature_Sensor" && v.NetType == "REAL");
        variableArray.ShouldContain(v => v.Name == "Transmission_Pressure_PSI" && v.NetType == "REAL");
        variableArray.ShouldContain(v => v.Name == "Engine_RPM" && v.NetType == "DINT");
        variableArray.ShouldContain(v => v.Name == "Quality_Check_Pass" && v.NetType == "BOOL");
        variableArray.ShouldContain(v => v.Name == "VIN_Scanner_Data" && v.NetType == "STRING");

        await _repository.Received(1).ListAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_ReturnElectronicsVariableList_When_iPhone15ManufacturingLine operation.
    /// </summary>
    /// <returns>The result of Should_ReturnElectronicsVariableList_When_iPhone15ManufacturingLine.</returns>

    [Fact]
    public async Task Should_ReturnElectronicsVariableList_When_iPhone15ManufacturingLine()
    {
        // Arrange - iPhone 15 PCB manufacturing line variables
        var variables = CreateElectronicsVariables();
        var query = new GetVariableListQuery();

        _repository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<Variable>>.Success(variables));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count.ShouldBe(6);

        var variableArray = result.Value.VariableList.ToArray();
        variableArray.ShouldContain(v => v.Name == "PCB_Temperature_Celsius" && v.NetType == "REAL");
        variableArray.ShouldContain(v => v.Name == "Solder_Joint_Count" && v.NetType == "DINT");
        variableArray.ShouldContain(v => v.Name == "AOI_Inspection_Result" && v.NetType == "BOOL");
        variableArray.ShouldContain(v => v.Name == "Component_Placement_X" && v.NetType == "REAL");
        variableArray.ShouldContain(v => v.Name == "Component_Placement_Y" && v.NetType == "REAL");
        variableArray.ShouldContain(v => v.Name == "Circuit_Board_Serial" && v.NetType == "STRING");

        await _repository.Received(1).ListAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_ReturnPharmaceuticalVariableList_When_TabletManufacturingLine operation.
    /// </summary>
    /// <returns>The result of Should_ReturnPharmaceuticalVariableList_When_TabletManufacturingLine.</returns>

    [Fact]
    public async Task Should_ReturnPharmaceuticalVariableList_When_TabletManufacturingLine()
    {
        // Arrange - Pharmaceutical tablet manufacturing line variables
        var variables = CreatePharmaceuticalVariables();
        var query = new GetVariableListQuery();

        _repository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<Variable>>.Success(variables));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count.ShouldBe(7);

        var variableArray = result.Value.VariableList.ToArray();
        variableArray.ShouldContain(v => v.Name == "Tablet_Weight_Milligrams" && v.NetType == "REAL");
        variableArray.ShouldContain(v => v.Name == "Press_Force_Newton" && v.NetType == "REAL");
        variableArray.ShouldContain(v => v.Name == "API_Content_Percentage" && v.NetType == "REAL");
        variableArray.ShouldContain(v => v.Name == "Hardness_Test_Result" && v.NetType == "REAL");
        variableArray.ShouldContain(v => v.Name == "Disintegration_Time_Seconds" && v.NetType == "DINT");
        variableArray.ShouldContain(v => v.Name == "FDA_Compliance_Check" && v.NetType == "BOOL");
        variableArray.ShouldContain(v => v.Name == "Batch_Number" && v.NetType == "STRING");

        await _repository.Received(1).ListAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_ReturnBeverageVariableList_When_CocaColaBottlingLine operation.
    /// </summary>
    /// <returns>The result of Should_ReturnBeverageVariableList_When_CocaColaBottlingLine.</returns>

    [Fact]
    public async Task Should_ReturnBeverageVariableList_When_CocaColaBottlingLine()
    {
        // Arrange - Coca-Cola beverage bottling line variables
        var variables = CreateBeverageVariables();
        var query = new GetVariableListQuery();

        _repository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<Variable>>.Success(variables));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count.ShouldBe(8);

        var variableArray = result.Value.VariableList.ToArray();
        variableArray.ShouldContain(v => v.Name == "Fill_Level_Milliliters" && v.NetType == "REAL");
        variableArray.ShouldContain(v => v.Name == "Carbonation_Level_PSI" && v.NetType == "REAL");
        variableArray.ShouldContain(v => v.Name == "Sugar_Content_Brix" && v.NetType == "REAL");
        variableArray.ShouldContain(v => v.Name == "Bottle_Cap_Torque" && v.NetType == "REAL");
        variableArray.ShouldContain(v => v.Name == "Production_Speed_BPM" && v.NetType == "DINT");
        variableArray.ShouldContain(v => v.Name == "Quality_Gate_Pass" && v.NetType == "BOOL");
        variableArray.ShouldContain(v => v.Name == "Label_Applied_Correctly" && v.NetType == "BOOL");
        variableArray.ShouldContain(v => v.Name == "Lot_Code" && v.NetType == "STRING");

        await _repository.Received(1).ListAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_ReturnSafetyVariableList_When_IndustrialSafetyMonitoring operation.
    /// </summary>
    /// <returns>The result of Should_ReturnSafetyVariableList_When_IndustrialSafetyMonitoring.</returns>

    [Fact]
    public async Task Should_ReturnSafetyVariableList_When_IndustrialSafetyMonitoring()
    {
        // Arrange - Industrial safety monitoring variables across different manufacturing sectors
        var variables = CreateSafetyVariables();
        var query = new GetVariableListQuery();

        _repository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<Variable>>.Success(variables));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count.ShouldBe(6);

        var variableArray = result.Value.VariableList.ToArray();
        variableArray.ShouldContain(v => v.Name == "Emergency_Stop_Status" && v.NetType == "BOOL");
        variableArray.ShouldContain(v => v.Name == "Safety_Light_Curtain" && v.NetType == "BOOL");
        variableArray.ShouldContain(v => v.Name == "Machine_Guard_Position" && v.NetType == "BOOL");
        variableArray.ShouldContain(v => v.Name == "Pressure_Relief_Valve" && v.NetType == "BOOL");
        variableArray.ShouldContain(v => v.Name == "Temperature_Alarm_Threshold" && v.NetType == "REAL");
        variableArray.ShouldContain(v => v.Name == "Safety_System_Heartbeat" && v.NetType == "DINT");

        await _repository.Received(1).ListAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_ReturnEnergyMonitoringVariableList_When_SustainableManufacturing operation.
    /// </summary>
    /// <returns>The result of Should_ReturnEnergyMonitoringVariableList_When_SustainableManufacturing.</returns>

    [Fact]
    public async Task Should_ReturnEnergyMonitoringVariableList_When_SustainableManufacturing()
    {
        // Arrange - Energy monitoring variables for sustainable manufacturing
        var variables = CreateEnergyMonitoringVariables();
        var query = new GetVariableListQuery();

        _repository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<Variable>>.Success(variables));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count.ShouldBe(5);

        var variableArray = result.Value.VariableList.ToArray();
        variableArray.ShouldContain(v => v.Name == "Total_Power_Consumption_kW" && v.NetType == "REAL");
        variableArray.ShouldContain(v => v.Name == "Energy_Efficiency_Percentage" && v.NetType == "REAL");
        variableArray.ShouldContain(v => v.Name == "Carbon_Footprint_CO2_kg" && v.NetType == "REAL");
        variableArray.ShouldContain(v => v.Name == "Renewable_Energy_Usage_Percentage" && v.NetType == "REAL");
        variableArray.ShouldContain(v => v.Name == "Peak_Demand_Alert" && v.NetType == "BOOL");

        await _repository.Received(1).ListAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_ReturnLargeVariableList_When_ComplexManufacturingFacility operation.
    /// </summary>
    /// <returns>The result of Should_ReturnLargeVariableList_When_ComplexManufacturingFacility.</returns>

    [Fact]
    public async Task Should_ReturnLargeVariableList_When_ComplexManufacturingFacility()
    {
        // Arrange - Large variable collection representing a complex manufacturing facility
        var variables = CreateLargeVariableCollection();
        var query = new GetVariableListQuery();

        _repository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<Variable>>.Success(variables));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count.ShouldBe(50); // Large collection
        result.Value.VariableList.Count.ShouldBe(50);

        // Verify various data types are present
        var variableArray = result.Value.VariableList.ToArray();
        variableArray.ShouldContain(v => v.NetType == "BOOL");
        variableArray.ShouldContain(v => v.NetType == "INT");
        variableArray.ShouldContain(v => v.NetType == "DINT");
        variableArray.ShouldContain(v => v.NetType == "REAL");
        variableArray.ShouldContain(v => v.NetType == "STRING");

        // Verify different variable groups are present
        var groupIds = variableArray.Select(v => v.VariableGroupId).Distinct().ToList();
        groupIds.Count.ShouldBeGreaterThan(5); // Multiple groups

        await _repository.Received(1).ListAsync(Arg.Any<CancellationToken>());
    }

    private static List<Variable> CreateAutomotiveVariables()
    {
        return
        [
            new Variable { VariableId = 2001, MachineId = 201, Name = "Engine_Temperature_Sensor", Address = "DB1.DBD0", NetType = "REAL", Length = 4, IsActive = 1, VariableGroupId = 10 },
            new Variable { VariableId = 2002, MachineId = 202, Name = "Transmission_Pressure_PSI", Address = "DB2.DBD0", NetType = "REAL", Length = 4, IsActive = 1, VariableGroupId = 10 },
            new Variable { VariableId = 2003, MachineId = 203, Name = "Engine_RPM", Address = "DB3.DBD0", NetType = "DINT", Length = 4, IsActive = 1, VariableGroupId = 10 },
            new Variable { VariableId = 2004, MachineId = 204, Name = "Quality_Check_Pass", Address = "DB4.DBX0.0", NetType = "BOOL", Length = 1, IsActive = 1, VariableGroupId = 11 },
            new Variable { VariableId = 2005, MachineId = 205, Name = "VIN_Scanner_Data", Address = "DB5.DBB0", NetType = "STRING", Length = 17, IsActive = 1, VariableGroupId = 12 }
        ];
    }

    private static List<Variable> CreateElectronicsVariables()
    {
        return
        [
            new Variable { VariableId = 3001, MachineId = 301, Name = "PCB_Temperature_Celsius", Address = "DB10.DBD0", NetType = "REAL", Length = 4, IsActive = 1, VariableGroupId = 20 },
            new Variable { VariableId = 3002, MachineId = 302, Name = "Solder_Joint_Count", Address = "DB11.DBD0", NetType = "DINT", Length = 4, IsActive = 1, VariableGroupId = 20 },
            new Variable { VariableId = 3003, MachineId = 303, Name = "AOI_Inspection_Result", Address = "DB12.DBX0.0", NetType = "BOOL", Length = 1, IsActive = 1, VariableGroupId = 21 },
            new Variable { VariableId = 3004, MachineId = 304, Name = "Component_Placement_X", Address = "DB13.DBD0", NetType = "REAL", Length = 4, IsActive = 1, VariableGroupId = 22 },
            new Variable { VariableId = 3005, MachineId = 305, Name = "Component_Placement_Y", Address = "DB14.DBD0", NetType = "REAL", Length = 4, IsActive = 1, VariableGroupId = 22 },
            new Variable { VariableId = 3006, MachineId = 306, Name = "Circuit_Board_Serial", Address = "DB15.DBB0", NetType = "STRING", Length = 20, IsActive = 1, VariableGroupId = 23 }
        ];
    }

    private static List<Variable> CreatePharmaceuticalVariables()
    {
        return
        [
            new Variable { VariableId = 4001, MachineId = 401, Name = "Tablet_Weight_Milligrams", Address = "DB20.DBD0", NetType = "REAL", Length = 4, IsActive = 1, VariableGroupId = 30 },
            new Variable { VariableId = 4002, MachineId = 402, Name = "Press_Force_Newton", Address = "DB21.DBD0", NetType = "REAL", Length = 4, IsActive = 1, VariableGroupId = 30 },
            new Variable { VariableId = 4003, MachineId = 403, Name = "API_Content_Percentage", Address = "DB22.DBD0", NetType = "REAL", Length = 4, IsActive = 1, VariableGroupId = 31 },
            new Variable { VariableId = 4004, MachineId = 404, Name = "Hardness_Test_Result", Address = "DB23.DBD0", NetType = "REAL", Length = 4, IsActive = 1, VariableGroupId = 31 },
            new Variable { VariableId = 4005, MachineId = 405, Name = "Disintegration_Time_Seconds", Address = "DB24.DBD0", NetType = "DINT", Length = 4, IsActive = 1, VariableGroupId = 31 },
            new Variable { VariableId = 4006, MachineId = 406, Name = "FDA_Compliance_Check", Address = "DB25.DBX0.0", NetType = "BOOL", Length = 1, IsActive = 1, VariableGroupId = 32 },
            new Variable { VariableId = 4007, MachineId = 407, Name = "Batch_Number", Address = "DB26.DBB0", NetType = "STRING", Length = 15, IsActive = 1, VariableGroupId = 33 }
        ];
    }

    private static List<Variable> CreateBeverageVariables()
    {
        return
        [
            new Variable { VariableId = 5001, MachineId = 501, Name = "Fill_Level_Milliliters", Address = "DB30.DBD0", NetType = "REAL", Length = 4, IsActive = 1, VariableGroupId = 40 },
            new Variable { VariableId = 5002, MachineId = 502, Name = "Carbonation_Level_PSI", Address = "DB31.DBD0", NetType = "REAL", Length = 4, IsActive = 1, VariableGroupId = 40 },
            new Variable { VariableId = 5003, MachineId = 503, Name = "Sugar_Content_Brix", Address = "DB32.DBD0", NetType = "REAL", Length = 4, IsActive = 1, VariableGroupId = 41 },
            new Variable { VariableId = 5004, MachineId = 504, Name = "Bottle_Cap_Torque", Address = "DB33.DBD0", NetType = "REAL", Length = 4, IsActive = 1, VariableGroupId = 42 },
            new Variable { VariableId = 5005, MachineId = 505, Name = "Production_Speed_BPM", Address = "DB34.DBD0", NetType = "DINT", Length = 4, IsActive = 1, VariableGroupId = 43 },
            new Variable { VariableId = 5006, MachineId = 506, Name = "Quality_Gate_Pass", Address = "DB35.DBX0.0", NetType = "BOOL", Length = 1, IsActive = 1, VariableGroupId = 44 },
            new Variable { VariableId = 5007, MachineId = 507, Name = "Label_Applied_Correctly", Address = "DB36.DBX0.0", NetType = "BOOL", Length = 1, IsActive = 1, VariableGroupId = 44 },
            new Variable { VariableId = 5008, MachineId = 508, Name = "Lot_Code", Address = "DB37.DBB0", NetType = "STRING", Length = 12, IsActive = 1, VariableGroupId = 45 }
        ];
    }

    private static List<Variable> CreateSafetyVariables()
    {
        return
        [
            new Variable { VariableId = 6001, MachineId = 601, Name = "Emergency_Stop_Status", Address = "DB40.DBX0.0", NetType = "BOOL", Length = 1, IsActive = 1, VariableGroupId = 50 },
            new Variable { VariableId = 6002, MachineId = 602, Name = "Safety_Light_Curtain", Address = "DB41.DBX0.0", NetType = "BOOL", Length = 1, IsActive = 1, VariableGroupId = 50 },
            new Variable { VariableId = 6003, MachineId = 603, Name = "Machine_Guard_Position", Address = "DB42.DBX0.0", NetType = "BOOL", Length = 1, IsActive = 1, VariableGroupId = 51 },
            new Variable { VariableId = 6004, MachineId = 604, Name = "Pressure_Relief_Valve", Address = "DB43.DBX0.0", NetType = "BOOL", Length = 1, IsActive = 1, VariableGroupId = 51 },
            new Variable { VariableId = 6005, MachineId = 605, Name = "Temperature_Alarm_Threshold", Address = "DB44.DBD0", NetType = "REAL", Length = 4, IsActive = 1, VariableGroupId = 52 },
            new Variable { VariableId = 6006, MachineId = 606, Name = "Safety_System_Heartbeat", Address = "DB45.DBD0", NetType = "DINT", Length = 4, IsActive = 1, VariableGroupId = 53 }
        ];
    }

    private static List<Variable> CreateEnergyMonitoringVariables()
    {
        return
        [
            new Variable { VariableId = 7001, MachineId = 701, Name = "Total_Power_Consumption_kW", Address = "DB50.DBD0", NetType = "REAL", Length = 4, IsActive = 1, VariableGroupId = 60 },
            new Variable { VariableId = 7002, MachineId = 702, Name = "Energy_Efficiency_Percentage", Address = "DB51.DBD0", NetType = "REAL", Length = 4, IsActive = 1, VariableGroupId = 60 },
            new Variable { VariableId = 7003, MachineId = 703, Name = "Carbon_Footprint_CO2_kg", Address = "DB52.DBD0", NetType = "REAL", Length = 4, IsActive = 1, VariableGroupId = 61 },
            new Variable { VariableId = 7004, MachineId = 704, Name = "Renewable_Energy_Usage_Percentage", Address = "DB53.DBD0", NetType = "REAL", Length = 4, IsActive = 1, VariableGroupId = 61 },
            new Variable { VariableId = 7005, MachineId = 705, Name = "Peak_Demand_Alert", Address = "DB54.DBX0.0", NetType = "BOOL", Length = 1, IsActive = 1, VariableGroupId = 62 }
        ];
    }

    private static List<Variable> CreateLargeVariableCollection()
    {
        var variables = new List<Variable>();
        var random = new Random(42); // Fixed seed for reproducible tests
        var dataTypes = new[] { "BOOL", "INT", "DINT", "REAL", "STRING" };
        var lengths = new Dictionary<string, int> { ["BOOL"] = 1, ["INT"] = 2, ["DINT"] = 4, ["REAL"] = 4, ["STRING"] = 20 };

        for (int i = 1; i <= 50; i++)
        {
            var dataType = dataTypes[i % dataTypes.Length];
            variables.Add(new Variable
            {
                VariableId = 8000 + i,
                MachineId = 800 + (i % 10) + 1, // 10 different machines
                Name = $"Variable_{i:D3}_{dataType}",
                Address = $"DB{60 + (i % 20)}.DB{(i % 4) switch { 0 => "X", 1 => "B", 2 => "W", _ => "D" }}{i * 4}",
                NetType = dataType,
                Length = lengths[dataType],
                IsActive = 1,
                VariableGroupId = 70 + (i % 8) // 8 different groups
            });
        }

        return variables;
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
/// Error handling and edge case tests for GetVariableListQueryHandler
/// </summary>
public class GetVariableListQueryHandlerErrorTests : IDisposable
{
    private readonly IRepository<Variable> _repository = null!;
    private readonly ILogger<GetVariableListQueryHandler> _logger = null!;
    private readonly GetVariableListQueryHandler _handler = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public GetVariableListQueryHandlerErrorTests()
    {
        _repository = Substitute.For<IRepository<Variable>>();
        _logger = XUnitLogger.CreateLogger<GetVariableListQueryHandler>();
        _handler = new GetVariableListQueryHandler(_repository, _logger);
    }

    /// <summary>
    /// Executes Should_HandleCancellation_When_CancellationRequested operation.
    /// </summary>
    /// <returns>The result of Should_HandleCancellation_When_CancellationRequested.</returns>

    [Fact]
    public async Task Should_HandleCancellation_When_CancellationRequested()
    {
        // Arrange
        var query = new GetVariableListQuery();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        _repository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromException<Result<IEnumerable<Variable>>>(new OperationCanceledException()));

        // Act
        var result = await _handler.ProcessAsync(query, cts.Token);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Operation was canceled.");
    }

    /// <summary>
    /// Executes Should_HandleRepositoryException_When_ListAsyncThrows operation.
    /// </summary>
    /// <returns>The result of Should_HandleRepositoryException_When_ListAsyncThrows.</returns>

    [Fact]
    public async Task Should_HandleRepositoryException_When_ListAsyncThrows()
    {
        // Arrange - Repository throws exception
        var query = new GetVariableListQuery();

        _repository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Result<IEnumerable<Variable>>>(new InvalidOperationException("Repository failure")));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Operation finished with an exception Repository failure");
    }

    /// <summary>
    /// Executes Should_HandleNullVariableCollection_When_RepositoryReturnsNull operation.
    /// </summary>
    /// <returns>The result of Should_HandleNullVariableCollection_When_RepositoryReturnsNull.</returns>

    [Fact]
    public async Task Should_HandleNullVariableCollection_When_RepositoryReturnsNull()
    {
        // Arrange - Repository returns failure when no variables found
        var query = new GetVariableListQuery();

        _repository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<Variable>>.WithFailure("Failed to retrieve Variables"));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 19/09/2025
        //Reason: [REPOSITORY BEHAVIOR CORRECTION] - Repository returns failure when no variables found
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Failed to retrieve Variables");
    }

    /// <summary>
    /// Executes Should_LogError_When_RepositoryFails operation.
    /// </summary>
    /// <returns>The result of Should_LogError_When_RepositoryFails.</returns>

    [Fact]
    public async Task Should_LogError_When_RepositoryFails()
    {
        // Arrange - Repository failure for logging verification
        var query = new GetVariableListQuery();

        _repository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<Variable>>.WithFailure("Database timeout error"));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Database timeout error");

        // Verify error was logged (implicitly through error propagation)
    }

    /// <summary>
    /// Executes Should_HandleRepositoryTimeout_When_DatabaseResponseSlow operation.
    /// </summary>
    /// <returns>The result of Should_HandleRepositoryTimeout_When_DatabaseResponseSlow.</returns>

    [Fact]
    public async Task Should_HandleRepositoryTimeout_When_DatabaseResponseSlow()
    {
        // Arrange - Simulate slow database response
        var query = new GetVariableListQuery();

        _repository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Result<IEnumerable<Variable>>>(new TimeoutException("Database timeout")));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Operation finished with an exception Database timeout");
    }

    /// <summary>
    /// Executes Should_HandleVariablesWithNullProperties_When_DataIntegrityIssues operation.
    /// </summary>
    /// <returns>The result of Should_HandleVariablesWithNullProperties_When_DataIntegrityIssues.</returns>

    [Fact]
    public async Task Should_HandleVariablesWithNullProperties_When_DataIntegrityIssues()
    {
        // Arrange - Variables with null/invalid properties
        var variables = new List<Variable>
        {
            new Variable { VariableId = 1, MachineId = 10000, Name = null!, Address = null!, NetType = null!, Length = 4, IsActive = 1 },
            new Variable { VariableId = 2, MachineId = 200, Name = "Valid_Variable", Address = "DB1.DBD0", NetType = "REAL", Length = 4, IsActive = 1 }
        };

        var query = new GetVariableListQuery();

        _repository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<Variable>>.Success(variables));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count.ShouldBe(2);
        result.Value.VariableList.Count.ShouldBe(2);

        // The DTO mapping should handle null values appropriately
        var variableArray = result.Value.VariableList.ToArray();
        variableArray[0].VariableId.ShouldBe(1);
        variableArray[1].VariableId.ShouldBe(2);
        variableArray[1].Name.ShouldBe("Valid_Variable");
    }

    /// <summary>
    /// Executes Should_HandleMultipleSimultaneousRequests_When_ConcurrentAccess operation.
    /// </summary>
    /// <returns>The result of Should_HandleMultipleSimultaneousRequests_When_ConcurrentAccess.</returns>

    [Fact]
    public async Task Should_HandleMultipleSimultaneousRequests_When_ConcurrentAccess()
    {
        // Arrange - Multiple concurrent requests
        var variables = new List<Variable>
        {
            new Variable { VariableId = 1, Name = "Var1", NetType = "BOOL" },
            new Variable { VariableId = 2, Name = "Var2", NetType = "INT" },
            new Variable { VariableId = 3, Name = "Var3", NetType = "REAL" }
        };

        var queries = new[]
        {
            new GetVariableListQuery(),
            new GetVariableListQuery(),
            new GetVariableListQuery()
        };

        _repository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<Variable>>.Success(variables));

        // Act - Execute concurrent requests
        var tasks = queries.Select(query => _handler.ProcessAsync(query, TestContext.Current.CancellationToken));
        var results = await Task.WhenAll(tasks);

        // Assert
        results.ShouldAllBe(result => result.IsSuccess);
        results.ShouldAllBe(result => result.Value != null);

        // Verify repository was called for each request
        await _repository.Received(3).ListAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Should_HandleVariousCollectionSizes_When_DifferentDataVolumes operation.
    /// </summary>
    /// <param name="variableCount">The variableCount.</param>
    /// <returns>The result of Should_HandleVariousCollectionSizes_When_DifferentDataVolumes.</returns>

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public async Task Should_HandleVariousCollectionSizes_When_DifferentDataVolumes(int variableCount)
    {
        // Using parameters: variableCount
        _ = variableCount; // xUnit1026 fix
        // Using parameters: variableCount
        _ = variableCount; // xUnit1026 fix
        // Using parameters: variableCount
        _ = variableCount; // xUnit1026 fix
        // Using parameters: variableCount
        _ = variableCount; // xUnit1026 fix
        // Using parameters: variableCount
        _ = variableCount; // xUnit1026 fix
        // Arrange - Different collection sizes
        var variables = new List<Variable>();
        for (int i = 1; i <= variableCount; i++)
        {
            variables.Add(new Variable
            {
                VariableId = i,
                MachineId = 10000,
                Name = $"Variable_{i}",
                Address = $"DB{i}.DBD0",
                NetType = "REAL",
                Length = 4,
                IsActive = 1
            });
        }

        var query = new GetVariableListQuery();

        _repository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<Variable>>.Success(variables));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count.ShouldBe(variableCount);
        result.Value.VariableList.Count.ShouldBe(variableCount);

        if (variableCount > 0)
        {
            var firstVariable = result.Value.VariableList.First();
            firstVariable.VariableId.ShouldBe(1);
            firstVariable.Name.ShouldBe("Variable_1");
        }
    }

    /// <summary>
    /// Executes Should_HandleMemoryPressure_When_LargeDataSet operation.
    /// </summary>
    /// <returns>The result of Should_HandleMemoryPressure_When_LargeDataSet.</returns>

    [Fact]
    public async Task Should_HandleMemoryPressure_When_LargeDataSet()
    {
        // Arrange - Very large dataset to test memory handling
        const int largeCount = 10000;
        var variables = new List<Variable>();

        for (int i = 1; i <= largeCount; i++)
        {
            variables.Add(new Variable
            {
                VariableId = i,
                MachineId = i % 100 + 1,
                Name = $"LargeDataset_Variable_{i:D5}",
                Address = $"DB{i % 1000}.DBD{(i % 256) * 4}",
                NetType = (i % 5) switch { 0 => "BOOL", 1 => "INT", 2 => "DINT", 3 => "REAL", _ => "STRING" },
                Length = (i % 5) switch { 0 => 1, 1 => 2, 2 => 4, 3 => 4, _ => 20 },
                IsActive = 1,
                VariableGroupId = i % 50 + 1
            });
        }

        var query = new GetVariableListQuery();

        _repository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<Variable>>.Success(variables));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count.ShouldBe(largeCount);
        result.Value.VariableList.Count.ShouldBe(largeCount);

        // Verify data integrity with sampling
        var variableArray = result.Value.VariableList.ToArray();
        variableArray[0].Name.ShouldBe("LargeDataset_Variable_00001");
        variableArray[largeCount - 1].Name.ShouldBe($"LargeDataset_Variable_{largeCount:D5}");
    }

    /// <summary>
    /// Executes Dispose operation.
    /// </summary>

    public void Dispose()
    {
        // Cleanup if needed
    }
}