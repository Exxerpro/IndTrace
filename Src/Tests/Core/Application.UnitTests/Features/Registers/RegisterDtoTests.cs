// <copyright file="RegisterDtoTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Registers;

/// <summary>
/// Unit tests for RegisterDto
/// </summary>
public class RegisterDtoTests
{
    /// <summary>
    /// Executes Constructor_WithDefaultConstructor_ShouldCreateInstanceWithDefaultValues operation.
    /// </summary>
    [Fact]
    public void Constructor_WithDefaultConstructor_ShouldCreateInstanceWithDefaultValues()
    {
        // Act
        var registerDto = new RegisterDto();

        // Assert
        registerDto.ShouldNotBeNull();
        registerDto.RegisterId.ShouldBe(0);
        registerDto.MachineId.ShouldBe(0);
        registerDto.Name.ShouldBe(string.Empty);
        registerDto.VariableId.ShouldBe(0);
        registerDto.CycleId.ShouldBe(0);
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated expectation to match RegisterDto implementation - Value property is initialized with string.Empty, not null
        registerDto.Value.ShouldBe(string.Empty);
        registerDto.DataType.ShouldBe(string.Empty);
        registerDto.StatusValueId.ShouldBe(0);
        registerDto.TimeStamp.ShouldBe(default(DateTime));
    }
    /// <summary>
    /// Executes Properties_WhenSet_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSet_ShouldReturnCorrectValues()
    {
        // Arrange
        var registerDto = new RegisterDto();
        var timestamp = new DateTime(2024, 12, 25, 14, 30, 45);

        // Act
        registerDto.RegisterId = 12345;
        registerDto.MachineId = 7001;
        registerDto.Name = "Temperature_Sensor_A1";
        registerDto.VariableId = 1001;
        registerDto.CycleId = 555;
        registerDto.Value = "85.7";
        registerDto.DataType = "REAL";
        registerDto.StatusValueId = 200;
        registerDto.TimeStamp = timestamp;

        // Assert
        registerDto.RegisterId.ShouldBe(12345);
        registerDto.MachineId.ShouldBe(7001);
        registerDto.Name.ShouldBe("Temperature_Sensor_A1");
        registerDto.VariableId.ShouldBe(1001);
        registerDto.CycleId.ShouldBe(555);
        registerDto.Value.ShouldBe("85.7");
        registerDto.DataType.ShouldBe("REAL");
        registerDto.StatusValueId.ShouldBe(200);
        registerDto.TimeStamp.ShouldBe(timestamp);
    }
    /// <summary>
    /// Executes ToDto_WithNullRegister_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToDto_WithNullRegister_ShouldReturnFailureResult()
    {
        // Arrange
        Register? nullRegister = null!;

        // Act
        var result = RegisterDto.ToDto(nullRegister!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("Parameter 'src' cannot be null");
    }
    /// <summary>
    /// Executes ToDto_WithValidRegister_ShouldMapAllProperties operation.
    /// </summary>

    [Fact]
    public void ToDto_WithValidRegister_ShouldMapAllProperties()
    {
        // Arrange
        var timestamp = new DateTime(2024, 11, 20, 9, 15, 30);
        var register = Register.CreateFixture(registerId: 98765, name: "Pressure_Sensor_B2", machineId: 8001, variableId: 2002, cycleId: 777, value: "150.25", dataType: "REAL", statusValueId: 100, timeStamp: timestamp);

        // Act
        var resultOfT = RegisterDto.ToDto(register);

        // Assert
        resultOfT.IsSuccess.ShouldBeTrue();
        resultOfT.Value.ShouldNotBeNull();
        var dto = resultOfT.Value;
        dto.ShouldNotBeNull();
        dto.ShouldNotBeNull();

        dto.ShouldNotBeNull();
        dto.RegisterId.ShouldBe(98765);
        dto.MachineId.ShouldBe(8001);
        dto.Name.ShouldBe("Pressure_Sensor_B2");
        dto.VariableId.ShouldBe(2002);
        dto.CycleId.ShouldBe(777);
        dto.Value.ShouldBe("150.25");
        dto.DataType.ShouldBe("REAL");
        dto.StatusValueId.ShouldBe(100);
        dto.TimeStamp.ShouldBe(timestamp);
    }
    /// <summary>
    /// Executes ToDto_WithEmptyValue_ShouldRemainEmpty operation.
    /// </summary>

    [Fact]
    public void ToDto_WithEmptyValue_ShouldRemainEmpty()
    {
        // Arrange — an empty reading is the reachable "no value" state (null Value is no longer
        // representable: Register.Create rejects null and the column is NOT NULL).
        var register = Register.CreateFixture(registerId: 123, name: "Empty_Value_Sensor", machineId: 456, value: string.Empty);

        // Act
        var resultOfT = RegisterDto.ToDto(register);

        // Assert
        resultOfT.IsSuccess.ShouldBeTrue();
        resultOfT.Value.ShouldNotBeNull();
        var dto = resultOfT.Value;
        dto.ShouldNotBeNull();
        dto.RegisterId.ShouldBe(123);
        dto.Value.ShouldBe(string.Empty);
        dto.Name.ShouldBe("Empty_Value_Sensor");
    }
    /// <summary>
    /// Executes ToEntity_WithNullRegisterDto_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithNullRegisterDto_ShouldReturnFailureResult()
    {
        // Arrange
        RegisterDto? nullDto = null!;

        // Act
        var result = RegisterDto.ToEntity(nullDto!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("Parameter 'src' cannot be null");
    }
    /// <summary>
    /// Executes ToEntity_WithValidRegisterDto_ShouldMapAllProperties operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithValidRegisterDto_ShouldMapAllProperties()
    {
        // Arrange
        var timestamp = new DateTime(2024, 10, 15, 16, 45, 20);
        var dto = new RegisterDto
        {
            RegisterId = 54321,
            MachineId = 9001,
            Name = "Vibration_Sensor_C3",
            VariableId = 3003,
            CycleId = 888,
            Value = "0.025",
            DataType = "REAL",
            StatusValueId = 150,
            TimeStamp = timestamp
        };

        // Act
        var resultOfT = RegisterDto.ToEntity(dto);

        // Assert
        resultOfT.IsSuccess.ShouldBeTrue();
        resultOfT.Value.ShouldNotBeNull();
        var entity = resultOfT.Value;
        entity.ShouldNotBeNull();
        entity.ShouldNotBeNull();

        entity.ShouldNotBeNull();
        entity.RegisterId.ShouldBe(54321);
        entity.MachineId.ShouldBe(9001);
        entity.Name.ShouldBe("Vibration_Sensor_C3");
        entity.VariableId.ShouldBe(3003);
        entity.CycleId.Value.ShouldBe(888);
        entity.Value.ShouldBe("0.025");
        entity.DataType.ShouldBe("REAL");
        entity.StatusValueId.ShouldBe(150);
        entity.TimeStamp.ShouldBe(timestamp);
    }
    /// <summary>
    /// Executes ToDtoList_WithNullRegisterCollection_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToDtoList_WithNullRegisterCollection_ShouldReturnFailureResult()
    {
        // Arrange
        IEnumerable<Register>? nullCollection = null!;

        // Act
        var result = RegisterDto.ToDtoList(nullCollection!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("Register collection cannot be null");
    }
    /// <summary>
    /// Executes ToDtoList_WithValidRegisterCollection_ShouldMapAllItems operation.
    /// </summary>

    [Fact]
    public void ToDtoList_WithValidRegisterCollection_ShouldMapAllItems()
    {
        // Arrange
        var timestamp = new DateTime(2024, 12, 25, 10, 0, 0);
        var registers = new List<Register>
        {
            Register.CreateFixture(registerId: 1001, name: "Temperature_Zone_1", machineId: 7001, variableId: 5001, value: "82.5", dataType: "REAL", timeStamp: timestamp),
            Register.CreateFixture(registerId: 1002, name: "Pressure_Zone_1", machineId: 7001, variableId: 5002, value: "145.7", dataType: "REAL", timeStamp: timestamp.AddMinutes(1))
        };

        // Act
        var resultOfT = RegisterDto.ToDtoList(registers);

        // Assert
        resultOfT.IsSuccess.ShouldBeTrue();
        resultOfT.Value.ShouldNotBeNull();
        var result = resultOfT.Value.ToList();

        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);

        result[0].RegisterId.ShouldBe(1001);
        result[0].Name.ShouldBe("Temperature_Zone_1");
        result[0].Value.ShouldBe("82.5");
        result[0].DataType.ShouldBe("REAL");

        result[1].RegisterId.ShouldBe(1002);
        result[1].Name.ShouldBe("Pressure_Zone_1");
        result[1].Value.ShouldBe("145.7");
    }
    /// <summary>
    /// Executes ToDto_ThenToEntity_ShouldPreserveAllProperties operation.
    /// </summary>

    [Fact]
    public void ToDto_ThenToEntity_ShouldPreserveAllProperties()
    {
        // Arrange
        var originalRegister = Register.CreateFixture(registerId: 77777, name: "Round_Trip_Test_Register", machineId: 88888, variableId: 99999, cycleId: 11111, value: "123.456", dataType: "REAL", statusValueId: 250, timeStamp: new DateTime(2024, 8, 10, 13, 25, 40));

        // Act
        var dtoResultOfT = RegisterDto.ToDto(originalRegister);
        dtoResultOfT.Value.ShouldNotBeNull();




        var entityResultOfT = RegisterDto.ToEntity(dtoResultOfT.Value);

        // Assert
        dtoResultOfT.IsSuccess.ShouldBeTrue();
        entityResultOfT.IsSuccess.ShouldBeTrue();
        var convertedEntity = entityResultOfT.Value;
        convertedEntity.ShouldNotBeNull();
        convertedEntity.ShouldNotBeNull();
        convertedEntity.ShouldNotBeNull();

        convertedEntity.RegisterId.ShouldBe(originalRegister.RegisterId);
        convertedEntity.MachineId.ShouldBe(originalRegister.MachineId);
        convertedEntity.Name.ShouldBe(originalRegister.Name);
        convertedEntity.VariableId.ShouldBe(originalRegister.VariableId);
        convertedEntity.CycleId.ShouldBe(originalRegister.CycleId);
        convertedEntity.Value.ShouldBe(originalRegister.Value);
        convertedEntity.DataType.ShouldBe(originalRegister.DataType);
        convertedEntity.StatusValueId.ShouldBe(originalRegister.StatusValueId);
        convertedEntity.TimeStamp.ShouldBe(originalRegister.TimeStamp);
    }
}