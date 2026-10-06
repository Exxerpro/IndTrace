// <copyright file="LineDtoTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Lines;

/// <summary>
/// Unit tests for LineDto data transfer object.
/// Tests the industrial production line management system.
/// </summary>
public class LineDtoTests
{
    /// <summary>
    /// Executes LineDto_Constructor_ShouldInitializeWithDefaults operation.
    /// </summary>
    [Fact]
    public void LineDto_Constructor_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var lineDto = new LineDto();

        // Assert
        lineDto.LineId.ShouldBe(0);
        lineDto.Name.ShouldBe(string.Empty);
        lineDto.Description.ShouldBe(string.Empty);
        lineDto.Status.ShouldBe(0);
    }

    /// <summary>
    /// Executes LineDto_Properties_ShouldAllowGetAndSet operation.
    /// </summary>

    [Fact]
    public void LineDto_Properties_ShouldAllowGetAndSet()
    {
        // Arrange
        var lineDto = new LineDto();

        // Act & Assert
        lineDto.LineId = 1001;
        lineDto.LineId.ShouldBe(1001);

        lineDto.Name = "ASSEMBLY_LINE_A";
        lineDto.Name.ShouldBe("ASSEMBLY_LINE_A");

        lineDto.Description = "High-speed automotive assembly line";
        lineDto.Description.ShouldBe("High-speed automotive assembly line");

        lineDto.Status = 1;
        lineDto.Status.ShouldBe(1);
    }

    /// <summary>
    /// Executes LineDto_WithIndustrialScenarios_ShouldHandleCorrectly operation.
    /// </summary>

    [Theory]
    [InlineData(1001, "AUTOMOTIVE_ASSEMBLY", "Engine block assembly line", 1)]
    [InlineData(1002, "ELECTRONICS_SMT", "Surface mount technology line", 1)]
    [InlineData(1003, "PHARMACEUTICAL_PACKAGING", "Sterile packaging line", 1)]
    [InlineData(1004, "FOOD_BOTTLING", "Beverage bottling line", 0)]
    public void LineDto_WithIndustrialScenarios_ShouldHandleCorrectly(
        int lineId, string name, string description, int status)
    {
        // Arrange
        var lineDto = new LineDto();

        // Act
        lineDto.LineId = lineId;
        lineDto.Name = name;
        lineDto.Description = description;
        lineDto.Status = status;

        // Assert
        lineDto.LineId.ShouldBe(lineId);
        lineDto.Name.ShouldBe(name);
        lineDto.Description.ShouldBe(description);
        lineDto.Status.ShouldBe(status);
    }

    /// <summary>
    /// Executes LineDto_ToDto_WithValidEntity_ShouldConvertCorrectly operation.
    /// </summary>

    [Fact]
    public void LineDto_ToDto_WithValidEntity_ShouldConvertCorrectly()
    {
        // Arrange
        var line = new Line
        {
            LineId = 5001,
            Name = "SMART_MANUFACTURING_LINE",
            Description = "Industry 4.0 flexible manufacturing line with IoT sensors",
            Status = 1
        };

        // Act
        var result = LineDto.ToDto(line);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        var lineDto = result.Value;
        lineDto.ShouldNotBeNull();
        lineDto.ShouldNotBeNull();
        lineDto.ShouldNotBeNull();
        lineDto.LineId.ShouldBe(5001);
        lineDto.Name.ShouldBe("SMART_MANUFACTURING_LINE");
        lineDto.Description.ShouldBe("Industry 4.0 flexible manufacturing line with IoT sensors");
        lineDto.Status.ShouldBe(1);
    }

    /// <summary>
    /// Executes LineDto_ToEntity_WithValidDto_ShouldConvertCorrectly operation.
    /// </summary>

    [Fact]
    public void LineDto_ToEntity_WithValidDto_ShouldConvertCorrectly()
    {
        // Arrange
        var lineDto = new LineDto
        {
            LineId = 6002,
            Name = "PRECISION_MACHINING_CENTER",
            Description = "CNC machining center for aerospace components",
            Status = 1
        };

        // Act
        var result = LineDto.ToEntity(lineDto);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        var entity = result.Value;
        entity.ShouldNotBeNull();
        entity.ShouldNotBeNull();
        entity.ShouldNotBeNull();
        entity.LineId.ShouldBe(6002);
        entity.Name.ShouldBe("PRECISION_MACHINING_CENTER");
        entity.Description.ShouldBe("CNC machining center for aerospace components");
        entity.Status.ShouldBe(1);
    }

    /// <summary>
    /// Executes LineDto_RoundTripConversion_ShouldMaintainDataIntegrity operation.
    /// </summary>

    [Fact]
    public void LineDto_RoundTripConversion_ShouldMaintainDataIntegrity()
    {
        // Arrange
        var originalLine = new Line
        {
            LineId = 7003,
            Name = "CLEAN_ROOM_ASSEMBLY",
            Description = "Semiconductor assembly line in controlled environment",
            Status = 1
        };

        // Act
        var dtoResult = LineDto.ToDto(originalLine);

        // Assert first conversion
        dtoResult.IsSuccess.ShouldBeTrue();
        dtoResult.Value.ShouldNotBeNull();

        //[Fix]
        //CLAUDE
        //Date: 20/08/2025
        //Reason: Access Value after null check to satisfy null safety - round trip conversion test
        var entityResult = LineDto.ToEntity(dtoResult.Value.ShouldNotBeNull());

        // Assert
        entityResult.IsSuccess.ShouldBeTrue();
        entityResult.Value.ShouldNotBeNull();
        var convertedEntity = entityResult.Value;
        convertedEntity.ShouldNotBeNull();
        convertedEntity.ShouldNotBeNull();
        convertedEntity.ShouldNotBeNull();
        convertedEntity.LineId.ShouldBe(originalLine.LineId);
        convertedEntity.Name.ShouldBe(originalLine.Name);
        convertedEntity.Description.ShouldBe(originalLine.Description);
        convertedEntity.Status.ShouldBe(originalLine.Status);
    }

    /// <summary>
    /// Executes LineDto_ToDto_WithNullEntity_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void LineDto_ToDto_WithNullEntity_ShouldReturnFailureResult()
    {
        // Arrange
        Line? nullLine = null!;

        // Act
        var result = LineDto.ToDto(nullLine!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.Count().ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// Executes LineDto_ToEntity_WithNullDto_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void LineDto_ToEntity_WithNullDto_ShouldReturnFailureResult()
    {
        // Arrange
        LineDto? nullDto = null!;

        // Act
        var result = LineDto.ToEntity(nullDto!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.Count().ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// Executes LineDto_WithEdgeCaseValues_ShouldHandleCorrectly operation.
    /// </summary>

    [Theory]
    [InlineData("", "", 0, "Empty values should be handled")]
    [InlineData("A", "Single character", 1, "Minimal valid values")]
    [InlineData("VERY_LONG_LINE_NAME_WITH_MANY_CHARACTERS", "Very long description for testing maximum length handling in industrial systems", 1, "Maximum length values")]
    public void LineDto_WithEdgeCaseValues_ShouldHandleCorrectly(
        string name, string description, int status, string scenario)
    {
        var logger = XUnitLogger.CreateLogger<LineDtoTests>();
        logger.LogInformation("Testing scenario: {Scenario}", scenario);
        // Arrange
        var lineDto = new LineDto();

        // Act
        lineDto.Name = name;
        lineDto.Description = description;
        lineDto.Status = status;

        // Assert
        lineDto.Name.ShouldBe(name);
        lineDto.Description.ShouldBe(description);
        lineDto.Status.ShouldBe(status);
    }

    /// <summary>
    /// Executes LineDto_WithManufacturingScenarios_ShouldHandleIndustrialUseCase operation.
    /// </summary>

    [Fact]
    public void LineDto_WithManufacturingScenarios_ShouldHandleIndustrialUseCase()
    {
        // Arrange - Different manufacturing line types
        var automotiveLine = new LineDto
        {
            LineId = 2001,
            Name = "ENGINE_ASSEMBLY_LINE",
            Description = "V8 engine assembly with robotic welding stations",
            Status = 1
        };

        var electronicsLine = new LineDto
        {
            LineId = 2002,
            Name = "PCB_ASSEMBLY_LINE",
            Description = "High-speed pick and place for smartphone PCBs",
            Status = 1
        };

        var pharmaceuticalLine = new LineDto
        {
            LineId = 2003,
            Name = "STERILE_FILLING_LINE",
            Description = "Aseptic vial filling with integrity testing",
            Status = 0 // Maintenance mode
        };

        // Act & Assert - Validate different industry requirements
        automotiveLine.Name.ShouldContain("ENGINE");
        automotiveLine.Description.ShouldContain("robotic");
        automotiveLine.Status.ShouldBe(1); // Active production

        electronicsLine.Name.ShouldContain("PCB");
        electronicsLine.Description.ShouldContain("High-speed");
        electronicsLine.Status.ShouldBe(1); // Active production

        pharmaceuticalLine.Name.ShouldContain("STERILE");
        pharmaceuticalLine.Description.ShouldContain("Aseptic");
        pharmaceuticalLine.Status.ShouldBe(0); // Under maintenance

        // All lines should have valid IDs
        var allLines = new[] { automotiveLine, electronicsLine, pharmaceuticalLine };
        allLines.All(l => l.LineId > 2000).ShouldBeTrue();
        allLines.All(l => !string.IsNullOrEmpty(l.Name)).ShouldBeTrue();
        allLines.All(l => !string.IsNullOrEmpty(l.Description)).ShouldBeTrue();
    }

    /// <summary>
    /// Executes LineDto_WithStatusValues_ShouldRepresentLineStates operation.
    /// </summary>

    [Theory]
    [InlineData(0, "Inactive/Stopped")]
    [InlineData(1, "Active/Running")]
    [InlineData(2, "Maintenance")]
    [InlineData(3, "Setup/Changeover")]
    public void LineDto_WithStatusValues_ShouldRepresentLineStates(
        int statusValue, string description)
    {
        // Arrange
        var lineDto = new LineDto
        {
            LineId = 9001,
            Name = $"TEST_LINE_{statusValue}",
            Description = $"Test line for {description} status",
            Status = statusValue
        };

        // Act & Assert
        lineDto.Status.ShouldBe(statusValue);
        lineDto.Description.ShouldContain(description);

        // Validate status ranges for industrial systems
        lineDto.Status.ShouldBeInRange(0, 10); // Reasonable status range
    }
}