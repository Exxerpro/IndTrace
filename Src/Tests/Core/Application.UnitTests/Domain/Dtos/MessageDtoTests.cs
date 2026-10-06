// <copyright file="MessageDtoTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Domain.Dtos;

/// <summary>
/// Unit tests for MessageDto - Manufacturing Notification & Communication System
/// Tests production notifications, alerts, and communications for automotive, electronics, pharmaceutical, and aerospace manufacturing
/// </summary>
public class MessageDtoTests
{
    /// <summary>
    /// Executes Constructor_WithDefaultParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithDefaultParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var messageDto = new MessageDto();

        // Assert
        messageDto.ShouldNotBeNull();
        //[Fix]
        //CLAUDE
        //Date: 22/08/2025
        //Reason: Pattern 15 Fix - MessageDto properties are initialized to string.Empty, not null
        messageDto.From.ShouldBe(string.Empty);
        messageDto.To.ShouldBe(string.Empty);
        messageDto.Subject.ShouldBe(string.Empty);
        messageDto.Body.ShouldBe(string.Empty);
    }

    /// <summary>
    /// Executes Properties_WhenSetWithValidValues_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSetWithValidValues_ShouldReturnCorrectValues()
    {
        // Arrange
        var messageDto = new MessageDto();

        // Act
        messageDto.From = "production.supervisor@example.com";
        messageDto.To = "plant.manager@example.com";
        messageDto.Subject = "F-150 Production Line Alert - Conveyor Speed Deviation";
        messageDto.Body = "Alert: Assembly Line 3 conveyor belt speed is 5% below optimal. " +
                          "Immediate attention required to maintain production targets.";

        // Assert
        messageDto.From.ShouldBe("production.supervisor@example.com");
        messageDto.To.ShouldBe("plant.manager@example.com");
        messageDto.Subject.ShouldBe("F-150 Production Line Alert - Conveyor Speed Deviation");
        messageDto.Body.ShouldBe("Alert: Assembly Line 3 conveyor belt speed is 5% below optimal. " +
                                 "Immediate attention required to maintain production targets.");
    }

    /// <summary>
    /// Executes Properties_WithManufacturingScenarios_ShouldSetCorrectly operation.
    /// </summary>

    [Theory]
    [MemberData(nameof(ManufacturingNotificationScenarios))]
    public void Properties_WithManufacturingScenarios_ShouldSetCorrectly(
        string from, string to, string subject, string body, string industry)
    {
        // Arrange
        industry.ShouldNotBeNull(); // Validates manufacturing industry parameter

        var messageDto = new MessageDto();

        // Act
        messageDto.From = from;
        messageDto.To = to;
        messageDto.Subject = subject;
        messageDto.Body = body;

        // Assert
        messageDto.From.ShouldBe(from);
        messageDto.To.ShouldBe(to);
        messageDto.Subject.ShouldBe(subject);
        messageDto.Body.ShouldBe(body);
    }

    /// <summary>
    /// Executes Properties_WhenSetWithEmptyStrings_ShouldAllowEmptyValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSetWithEmptyStrings_ShouldAllowEmptyValues()
    {
        // Arrange
        var messageDto = new MessageDto();

        // Act
        messageDto.From = string.Empty;
        messageDto.To = string.Empty;
        messageDto.Subject = string.Empty;
        messageDto.Body = string.Empty;

        // Assert
        messageDto.From.ShouldBe(string.Empty);
        messageDto.To.ShouldBe(string.Empty);
        messageDto.Subject.ShouldBe(string.Empty);
        messageDto.Body.ShouldBe(string.Empty);
    }

    /// <summary>
    /// Executes From_WithVariousEmailFormats_ShouldAcceptValidFormats operation.
    /// </summary>

    [Fact]
    public void From_WithVariousEmailFormats_ShouldAcceptValidFormats()
    {
        // Arrange
        var messageDto = new MessageDto();

        // Act & Assert - Standard email
        messageDto.From = "supervisor@example.com";
        messageDto.From.ShouldBe("supervisor@example.com");

        // Act & Assert - Email with display name
        messageDto.From = "Production Supervisor <supervisor@example.com>";
        messageDto.From.ShouldBe("Production Supervisor <supervisor@example.com>");

        // Act & Assert - Internal system address
        messageDto.From = "mes-system@factory.local";
        messageDto.From.ShouldBe("mes-system@factory.local");
    }

    /// <summary>
    /// Executes To_WithMultipleRecipients_ShouldHandleCorrectly operation.
    /// </summary>

    [Fact]
    public void To_WithMultipleRecipients_ShouldHandleCorrectly()
    {
        // Arrange
        var messageDto = new MessageDto();

        // Act
        messageDto.To = "manager1@example.com;manager2@example.com;supervisor@example.com";

        // Assert
        messageDto.To.ShouldBe("manager1@example.com;manager2@example.com;supervisor@example.com");
    }

    /// <summary>
    /// Executes Subject_WithLongManufacturingDescriptions_ShouldHandleCorrectly operation.
    /// </summary>

    [Fact]
    public void Subject_WithLongManufacturingDescriptions_ShouldHandleCorrectly()
    {
        // Arrange
        var messageDto = new MessageDto();
        var longSubject = "CRITICAL ALERT: iPhone 15 Pro PCB Assembly Line 7 - SMT Component Placement Error - " +
                         "Pick and Place Machine #3 - Immediate Action Required - Production Halt Imminent";

        // Act
        messageDto.Subject = longSubject;

        // Assert
        messageDto.Subject.ShouldBe(longSubject);
    }

    /// <summary>
    /// Executes Body_WithDetailedManufacturingReport_ShouldHandleComplexContent operation.
    /// </summary>

    [Fact]
    public void Body_WithDetailedManufacturingReport_ShouldHandleComplexContent()
    {
        // Arrange
        var messageDto = new MessageDto();
        var detailedBody = @"PRODUCTION ALERT DETAILS:

        Facility: Tesla Gigafactory Texas
        Production Line: Model Y Battery Pack Assembly Line 4
        Alert Time: 2024-01-15 14:30:45 UTC
        Alert Level: WARNING

        ISSUE DESCRIPTION:
        Battery cell temperature sensors detected temperatures exceeding optimal range:
        - Cell Bank A: 42.5°C (Optimal: 35-40°C)
        - Cell Bank B: 43.1°C (Optimal: 35-40°C)
        - Cell Bank C: 41.8°C (Optimal: 35-40°C)

        RECOMMENDED ACTIONS:
        1. Check cooling system functionality
        2. Verify thermal management settings
        3. Inspect for blocked air vents
        4. Consider temporary production slowdown

        CONTACT INFORMATION:
        Battery Engineering: battery.eng@example.com
        Production Control: prod.control@example.com
        Emergency Hotline: +1-512-555-0199";

        // Act
        messageDto.Body = detailedBody;

        // Assert
        messageDto.Body.ShouldBe(detailedBody);
        messageDto.Body.ShouldContain("Tesla Gigafactory Texas");
        messageDto.Body.ShouldContain("Battery cell temperature");
        messageDto.Body.ShouldContain("RECOMMENDED ACTIONS");
    }

    /// <summary>
    /// Executes Body_WithVariousStringFormats_ShouldHandleCorrectly operation.
    /// </summary>
    /// <param name="body">The body.</param>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData("Special characters: àéìöü ñç ß", "Unicode and special characters should be handled")]
    [InlineData("Symbols and numbers: !@#$%^&*()_+ 12345", "Mixed content should be supported")]
    [InlineData("Line breaks:\nNew line content\nAnother line", "Multi-line content should work")]
    [InlineData("Tabs:\tTabbed content\tMore tabs", "Tab characters should be preserved")]
    public void Body_WithVariousStringFormats_ShouldHandleCorrectly(string body, string scenario)
    {
        // Using parameters: body, scenario
        _ = body; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: body, scenario
        _ = body; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: body, scenario
        _ = body; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: body, scenario
        _ = body; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: body, scenario
        _ = body; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Arrange
        var messageDto = new MessageDto();

        // Act
        messageDto.Body = body;

        // Assert
        messageDto.Body.ShouldBe(body);
    }

    /// <summary>
    /// Executes Properties_WhenSetWithNullValues_ShouldAllowNulls operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSetWithNullValues_ShouldAllowNulls()
    {
        // Arrange
        var messageDto = new MessageDto();

        // Act
        messageDto.From = null!;
        messageDto.To = null!;
        messageDto.Subject = null!;
        messageDto.Body = null!;

        // Assert
        messageDto.From.ShouldBeNull();
        messageDto.To.ShouldBeNull();
        messageDto.Subject.ShouldBeNull();
        messageDto.Body.ShouldBeNull();
    }

    /// <summary>
    /// Executes MessageDto_WithCompleteProductionAlert_ShouldFormValidNotification operation.
    /// </summary>

    [Fact]
    public void MessageDto_WithCompleteProductionAlert_ShouldFormValidNotification()
    {
        // Arrange & Act - Complete pharmaceutical production alert
        var messageDto = new MessageDto
        {
            From = "mes-system@example.com",
            To = "production.manager@example.com;quality.supervisor@example.com",
            Subject = "GMP Alert: Temperature Deviation in Sterile Processing Area 3",
            Body = "GOOD MANUFACTURING PRACTICE ALERT\n\n" +
                   "Facility: Pfizer Manufacturing Belgium\n" +
                   "Area: Sterile Processing Area 3\n" +
                   "Product: COVID-19 Vaccine (Batch: CV-2024-001)\n" +
                   "Alert Time: 2024-01-15 09:15:30 CET\n\n" +
                   "DEVIATION DETAILS:\n" +
                   "Temperature exceeded acceptable range for 2.5 minutes:\n" +
                   "- Recorded: 22.8°C\n" +
                   "- Acceptable Range: 20-22°C\n" +
                   "- Duration: 2.5 minutes\n\n" +
                   "ACTIONS TAKEN:\n" +
                   "1. HVAC system automatically adjusted\n" +
                   "2. Batch CV-2024-001 placed on quality hold\n" +
                   "3. Quality review initiated\n\n" +
                   "REQUIRED FOLLOW-UP:\n" +
                   "- Quality investigation per SOP-QC-001\n" +
                   "- Engineering review of HVAC system\n" +
                   "- Regulatory notification if required per 21 CFR 211\n\n" +
                   "System: IndTrace MES v2024.1"
        };

        // Assert
        messageDto.From.ShouldBe("mes-system@example.com");
        messageDto.To.ShouldContain("production.manager@example.com");
        messageDto.To.ShouldContain("quality.supervisor@example.com");
        messageDto.Subject.ShouldContain("GMP Alert");
        messageDto.Subject.ShouldContain("Temperature Deviation");
        messageDto.Body.ShouldContain("GOOD MANUFACTURING PRACTICE ALERT");
        messageDto.Body.ShouldContain("COVID-19 Vaccine");
        messageDto.Body.ShouldContain("21 CFR 211");
        messageDto.Body.ShouldContain("IndTrace MES");
    }

    /// <summary>
    /// Test data for manufacturing notification scenarios across different industries
    /// </summary>
    public static IEnumerable<object[]> ManufacturingNotificationScenarios =>
        new List<object[]>
        {
            // Automotive Industry
            new object[] {
                "production.line@example.com",
                "shift.supervisor@example.com",
                "F-150 Production Target Achieved",
                "Daily production target of 1,200 units achieved on Assembly Line 3. Excellent work team!",
                "Automotive"
            },
            new object[] {
                "quality.control@example.com",
                "production.manager@example.com",
                "Model Y Battery QC Warning",
                "Cell voltage variance detected in Batch BY-2024-045. Quality hold initiated pending investigation.",
                "Automotive"
            },
            new object[] {
                "automation@example.com",
                "maintenance.team@example.com",
                "Robotic Welding Cell Maintenance Due",
                "Robot Cell #7 scheduled for preventive maintenance at 22:00. Estimated downtime: 4 hours.",
                "Automotive"
            },

            // Electronics Industry
            new object[] {
                "smt.line@example.com",
                "production.lead@example.com",
                "iPhone 15 Pro PCB Assembly Alert",
                "Component placement accuracy below 99.8% on Line 12. AOI system flagged 15 boards for rework.",
                "Electronics"
            },
            new object[] {
                "cleanroom@example.com",
                "facility.manager@example.com",
                "Clean Room Environmental Alert",
                "Particle count elevated in Class 100 area. HVAC system responding. Monitor closely.",
                "Electronics"
            },
            new object[] {
                "wafer.fab@example.com",
                "engineering.team@example.com",
                "CPU Fab Process Optimization",
                "Yield improvement of 2.3% achieved on 14nm process. New parameters saved to recipe database.",
                "Electronics"
            },

            // Pharmaceutical Industry
            new object[] {
                "sterile.ops@example.com",
                "qa.manager@example.com",
                "Sterile Fill Line Environmental Alarm",
                "ISO 5 environment compromised for 30 seconds. Batch evaluation required per cGMP guidelines.",
                "Pharmaceutical"
            },
            new object[] {
                "packaging@example.com",
                "compliance.officer@example.com",
                "Serialization System Update",
                "Track and trace serialization updated for EU FMD compliance. All units properly coded.",
                "Pharmaceutical"
            },

            // Aerospace Industry
            new object[] {
                "composite.shop@example.com",
                "engineering.manager@example.com",
                "777X Wing Composite Curing Complete",
                "Autoclave Cycle #445 completed successfully. Part BW-777X-001 ready for inspection.",
                "Aerospace"
            },
            new object[] {
                "assembly@example.com",
                "quality.director@example.com",
                "F-35 Torque Specification Alert",
                "Fastener torque variance detected on Frame Station 12. Engineering review requested.",
                "Aerospace"
            },

            // Food & Beverage Industry
            new object[] {
                "bottling@example.com",
                "plant.supervisor@example.com",
                "Fill Level Adjustment Required",
                "Line 3 fill levels averaging 2ml below target. Automatic adjustment in progress.",
                "Food & Beverage"
            },

            // Heavy Industry
            new object[] {
                "hydraulics@example.com",
                "test.engineer@example.com",
                "797F Mining Truck Hydraulic Test Pass",
                "Unit 797F-2024-089 completed pressure testing. All systems within specification.",
                "Heavy Industry"
            }
        };
}