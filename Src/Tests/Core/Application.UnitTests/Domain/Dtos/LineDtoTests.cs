// <copyright file="LineDtoTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Domain.Dtos;

/// <summary>
/// Unit tests for LineDto - Manufacturing Production Line System
/// Tests production lines for automotive assembly, electronics manufacturing, pharmaceutical production, and aerospace manufacturing
/// </summary>
public class LineDtoTests
{
    /// <summary>
    /// Executes Constructor_WithDefaultParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithDefaultParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var lineDto = new LineDto();

        // Assert
        lineDto.ShouldNotBeNull();
        lineDto.LineId.ShouldBe(0);
        lineDto.Name.ShouldBe(string.Empty); // Property can be null
        lineDto.Description.ShouldBe(string.Empty); // Property can be null
        lineDto.Status.ShouldBe(0); // Default integer value
    }

    /// <summary>
    /// Executes Properties_WhenSetWithValidValues_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSetWithValidValues_ShouldReturnCorrectValues()
    {
        // Arrange
        var lineDto = new LineDto();

        // Act
        lineDto.LineId = 101;
        lineDto.Name = "F-150 Final Assembly Line";
        lineDto.Description = "Ford F-150 SuperCrew Cab Final Assembly and Quality Control";
        lineDto.Status = 1;

        // Assert
        lineDto.LineId.ShouldBe(101);
        lineDto.Name.ShouldBe("F-150 Final Assembly Line");
        lineDto.Description.ShouldBe("Ford F-150 SuperCrew Cab Final Assembly and Quality Control");
        lineDto.Status.ShouldBe(1);
    }

    /// <summary>
    /// Executes Properties_WithManufacturingScenarios_ShouldSetCorrectly operation.
    /// </summary>

    [Theory]
    [MemberData(nameof(ManufacturingLineScenarios))]
    public void Properties_WithManufacturingScenarios_ShouldSetCorrectly(
        int lineId, string lineName, string description, int status, string industry)
    {

        var logger = XUnitLogger.CreateLogger<LineDtoTests>();
        logger.LogInformation("Testing scenario: {description} with lineId={lineId}, lineName={lineName}, status={status}, industry={industry}",
            description, lineId, lineName, status, industry);

        // Arrange
        var lineDto = new LineDto();

        // Act
        lineDto.LineId = lineId;
        lineDto.Name = lineName;
        lineDto.Description = description;
        lineDto.Status = status;

        // Assert
        lineDto.LineId.ShouldBe(lineId);
        lineDto.Name.ShouldBe(lineName);
        lineDto.Description.ShouldBe(description);
        lineDto.Status.ShouldBe(status);
    }

    /// <summary>
    /// Executes ToDto_WithValidLineEntity_ShouldConvertCorrectly operation.
    /// </summary>

    [Fact]
    public void ToDto_WithValidLineEntity_ShouldConvertCorrectly()
    {
        // Arrange - Tesla Model S Battery Assembly Line
        var line = new Line
        {
            LineId = 201,
            Name = "Model S Battery Pack Assembly",
            Description = "Tesla Gigafactory Nevada Battery Cell Assembly and Pack Integration",
            Status = 2
        };

        // Act
        var resultOfT = LineDto.ToDto(line);

        // Assert
        resultOfT.IsSuccess.ShouldBeTrue();
        resultOfT.Value.ShouldNotBeNull();
        var lineDto = resultOfT.Value;
        lineDto.ShouldNotBeNull();
        lineDto.ShouldNotBeNull();

        lineDto.ShouldNotBeNull();
        lineDto.LineId.ShouldBe(201);
        lineDto.Name.ShouldBe("Model S Battery Pack Assembly");
        lineDto.Description.ShouldBe("Tesla Gigafactory Nevada Battery Cell Assembly and Pack Integration");
        lineDto.Status.ShouldBe(2);
    }

    /// <summary>
    /// Executes ToDto_WithNullLine_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToDto_WithNullLine_ShouldReturnFailureResult()
    {
        // Arrange
        Line nullLine = null!;

        // Act
        var result = LineDto.ToDto(nullLine);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldNotBeEmpty();
    }

    /// <summary>
    /// Executes ToEntity_WithValidLineDto_ShouldConvertCorrectly operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithValidLineDto_ShouldConvertCorrectly()
    {
        // Arrange - Boeing 777 Wing Assembly Line
        var lineDto = new LineDto
        {
            LineId = 301,
            Name = "777 Wing Assembly Line",
            Description = "Boeing Everett Factory Wing Box Assembly and Integration",
            Status = 1
        };

        // Act
        var resultOfT = LineDto.ToEntity(lineDto);

        // Assert
        resultOfT.IsSuccess.ShouldBeTrue();
        resultOfT.Value.ShouldNotBeNull();
        var line = resultOfT.Value;
        line.ShouldNotBeNull();
        line.ShouldNotBeNull();

        line.ShouldNotBeNull();
        line.LineId.ShouldBe(301);
        line.Name.ShouldBe("777 Wing Assembly Line");
        line.Description.ShouldBe("Boeing Everett Factory Wing Box Assembly and Integration");
        line.Status.ShouldBe(1);
    }

    /// <summary>
    /// Executes ToEntity_WithNullLineDto_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithNullLineDto_ShouldReturnFailureResult()
    {
        // Arrange
        LineDto nullDto = null!;

        // Act
        var result = LineDto.ToEntity(nullDto);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldNotBeEmpty();
    }

    /// <summary>
    /// Executes RoundTripConversion_WithCompleteLineData_ShouldMaintainDataIntegrity operation.
    /// </summary>

    [Fact]
    public void RoundTripConversion_WithCompleteLineData_ShouldMaintainDataIntegrity()
    {
        // Arrange - iPhone PCB Manufacturing Line
        var originalLine = new Line
        {
            LineId = 401,
            Name = "iPhone PCB SMT Line",
            Description = "Apple iPhone 15 Pro PCB Surface Mount Technology Assembly",
            Status = 3
        };

        // Act - Round trip conversion
        var dtoResultOfT = LineDto.ToDto(originalLine);

        // Assert first conversion succeeded
        dtoResultOfT.IsSuccess.ShouldBeTrue();
        dtoResultOfT.Value.ShouldNotBeNull();

        //[Fix] 
        //CLAUDE
        //Date: 20/08/2025 
        //Reason: Access Value after null check to satisfy null safety - round trip conversion test
        var convertedBackResultOfT = LineDto.ToEntity(dtoResultOfT.Value.ShouldNotBeNull());

        // Assert
        convertedBackResultOfT.IsSuccess.ShouldBeTrue();

        dtoResultOfT.Value.ShouldNotBeNull();
        convertedBackResultOfT.Value.ShouldNotBeNull();

        var convertedBackLine = convertedBackResultOfT.Value;
        convertedBackLine.ShouldNotBeNull();
        convertedBackLine.ShouldNotBeNull();
        convertedBackLine.ShouldNotBeNull();
        convertedBackLine.LineId.ShouldBe(originalLine.LineId);
        convertedBackLine.Name.ShouldBe(originalLine.Name);
        convertedBackLine.Description.ShouldBe(originalLine.Description);
        convertedBackLine.Status.ShouldBe(originalLine.Status);
    }

    /// <summary>
    /// Executes LineId_WithEdgeValues_ShouldSetCorrectly operation.
    /// </summary>
    /// <param name="lineId">The lineId.</param>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData(int.MaxValue, "Maximum Line ID")]
    [InlineData(int.MinValue, "Minimum Line ID")]
    [InlineData(0, "Zero Line ID")]
    [InlineData(-1, "Negative Line ID")]
    public void LineId_WithEdgeValues_ShouldSetCorrectly(int lineId, string scenario)
    {
        // Using parameters: lineId, scenario
        _ = lineId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: lineId, scenario
        _ = lineId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: lineId, scenario
        _ = lineId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: lineId, scenario
        _ = lineId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: lineId, scenario
        _ = lineId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Arrange
        var lineDto = new LineDto();

        // Act
        lineDto.LineId = lineId;

        // Assert
        lineDto.LineId.ShouldBe(lineId);
    }

    /// <summary>
    /// Executes Status_WithManufacturingStates_ShouldRepresentOperationalModes operation.
    /// </summary>
    /// <param name="status">The status.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData(0, "Offline/Shutdown")]
    [InlineData(1, "Active/Running")]
    [InlineData(2, "Maintenance Mode")]
    [InlineData(3, "Setup/Changeover")]
    [InlineData(4, "Emergency Stop")]
    [InlineData(5, "Quality Hold")]
    public void Status_WithManufacturingStates_ShouldRepresentOperationalModes(int status, string description)
    {
        // Using parameters: status, description
        _ = status; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: status, description
        _ = status; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: status, description
        _ = status; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: status, description
        _ = status; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: status, description
        _ = status; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Arrange
        var lineDto = new LineDto();

        // Act
        lineDto.Status = status;

        // Assert
        lineDto.Status.ShouldBe(status);
    }

    /// <summary>
    /// Executes Properties_WhenSetWithNullValues_ShouldAllowNulls operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSetWithNullValues_ShouldAllowNulls()
    {
        // Arrange
        var lineDto = new LineDto();

        // Act
        //[Fix] 
        //CLAUDE
        //Date: 20/08/2025 
        //Reason: Intentional null assignment test - using null-forgiving operator to suppress CS8625 warnings
        lineDto.Name = null!;
        lineDto.Description = null!;

        // Assert
        lineDto.Name.ShouldBeNull();
        lineDto.Description.ShouldBeNull();
    }

    /// <summary>
    /// Executes Properties_WhenSetWithEmptyStrings_ShouldAllowEmptyValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSetWithEmptyStrings_ShouldAllowEmptyValues()
    {
        // Arrange
        var lineDto = new LineDto();

        // Act
        lineDto.Name = string.Empty;
        lineDto.Description = string.Empty;

        // Assert
        lineDto.Name.ShouldBe(string.Empty);
        lineDto.Description.ShouldBe(string.Empty);
    }

    /// <summary>
    /// Executes Name_WithVariousStringFormats_ShouldHandleCorrectly operation.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData("Special characters: àéìöü ñç ß", "Unicode and special characters should be handled")]
    [InlineData("Very long line name with many details about the manufacturing process and equipment specifications", "Long strings should be supported")]
    [InlineData("Line with numbers 12345 and symbols !@#$%", "Mixed content should be supported")]
    public void Name_WithVariousStringFormats_ShouldHandleCorrectly(string name, string scenario)
    {
        // Using parameters: name, scenario
        _ = name; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: name, scenario
        _ = name; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: name, scenario
        _ = name; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: name, scenario
        _ = name; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: name, scenario
        _ = name; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Arrange
        var lineDto = new LineDto();

        // Act
        lineDto.Name = name;

        // Assert
        lineDto.Name.ShouldBe(name);
    }

    /// <summary>
    /// Test data for manufacturing line scenarios across different industries
    /// </summary>
    public static IEnumerable<object[]> ManufacturingLineScenarios =>
        new List<object[]>
        {
            // Automotive Industry
            new object[] { 1001, "F-150 Engine Assembly", "Ford F-150 3.5L EcoBoost V6 Engine Assembly Line", 1, "Automotive" },
            new object[] { 1002, "Tesla Model Y Body Shop", "Tesla Gigafactory Texas Model Y Body Welding and Assembly", 2, "Automotive" },
            new object[] { 1003, "BMW i4 Battery Integration", "BMW Group Munich Battery Pack Integration Line", 1, "Automotive" },
            new object[] { 1004, "VW ID.4 Final Assembly", "Volkswagen Chattanooga ID.4 Final Assembly and QC", 3, "Automotive" },

            // Electronics Industry
            new object[] { 2001, "iPhone 15 Pro PCB Line", "Apple iPhone 15 Pro A17 Pro PCB Surface Mount Technology", 1, "Electronics" },
            new object[] { 2002, "Samsung Galaxy S24 Assembly", "Samsung Galaxy S24 Ultra Final Device Assembly", 1, "Electronics" },
            new object[] { 2003, "Intel Core i9 Package Line", "Intel Wafer Packaging and Test Line for Core i9 CPUs", 2, "Electronics" },
            new object[] { 2004, "NVIDIA RTX 4090 Line", "NVIDIA GeForce RTX 4090 GPU Assembly and Test", 1, "Electronics" },

            // Aerospace Industry
            new object[] { 3001, "777X Wing Assembly", "Boeing 777X Composite Wing Box Assembly Line", 1, "Aerospace" },
            new object[] { 3002, "A350 Fuselage Section", "Airbus A350 Forward Fuselage Section Assembly", 2, "Aerospace" },
            new object[] { 3003, "F-35 Final Assembly", "Lockheed Martin F-35 Lightning II Final Assembly", 1, "Aerospace" },
            new object[] { 3004, "Falcon 9 Rocket Assembly", "SpaceX Falcon 9 First Stage Assembly Line", 3, "Aerospace" },

            // Pharmaceutical Industry
            new object[] { 4001, "COVID-19 Vaccine Fill", "Pfizer COVID-19 Vaccine Filling and Packaging Line", 1, "Pharmaceutical" },
            new object[] { 4002, "Insulin Production Line", "Eli Lilly Insulin Vial Filling and Labeling", 1, "Pharmaceutical" },
            new object[] { 4003, "Tablet Compression Line", "Johnson & Johnson Tablet Compression and Coating", 2, "Pharmaceutical" },

            // Food & Beverage Industry
            new object[] { 5001, "Coca-Cola Bottling Line", "Coca-Cola Atlanta Bottling and Capping Line", 1, "Food & Beverage" },
            new object[] { 5002, "Kraft Mac & Cheese Line", "Kraft Heinz Macaroni and Cheese Packaging Line", 1, "Food & Beverage" },

            // Heavy Industry
            new object[] { 6001, "Caterpillar 797F Line", "Caterpillar 797F Mining Truck Final Assembly", 1, "Heavy Industry" },
            new object[] { 6002, "John Deere Tractor Line", "John Deere 8R Series Tractor Assembly Line", 2, "Heavy Industry" }
        };
}