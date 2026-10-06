// <copyright file="PerformanceDataTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.PerformanceTests;
/// <summary>
/// Represents the PerformanceDataTests.
/// </summary>

public class PerformanceDataTests
{
    /// <summary>
    /// Executes FromPlc_WithValidRegisters_ShouldParseAllPropertiesCorrectly operation.
    /// </summary>
    [Fact]
    public void FromPlc_WithValidRegisters_ShouldParseAllPropertiesCorrectly()
    {
        // Arrange
        var registers = new Dictionary<string, Register>
        {
            ["ApplicationFlag"] = Register.CreateFixture(value: "1"),
            ["EventCounter"] = Register.CreateFixture(value: "123"),
            ["CurrentTime"] = Register.CreateFixture(value: "1000"),
            ["RunningTime"] = Register.CreateFixture(value: "800"),
            ["StoppedTime"] = Register.CreateFixture(value: "150"),
            ["FaultedTime"] = Register.CreateFixture(value: "50"),
            ["StatusFaultReason"] = Register.CreateFixture(value: "4"),
            ["TotalProduction"] = Register.CreateFixture(value: "500.5"),
            ["ProductionOk"] = Register.CreateFixture(value: "490.0"),
            ["ProductionNoK"] = Register.CreateFixture(value: "10.5"),
            ["StatusFaultReject"] = Register.CreateFixture(value: "2"),
            ["RejectEventCounter"] = Register.CreateFixture(value: "5"),
            ["StatusReject"] = Register.CreateFixture(value: "1"),
            ["RejectQuantityUnits"] = Register.CreateFixture(value: "2.5"),
            ["StandardCycleTime"] = Register.CreateFixture(value: "1.5"),
            ["ActualCycleTime"] = Register.CreateFixture(value: "1.6"),
            ["PlanedProductionTime"] = Register.CreateFixture(value: "950.0")
        };

        // Act
        var performanceData = PerformanceData.FromPlc(registers);

        // Assert
        performanceData.ApplicationFlag.ShouldBe(1);
        performanceData.EventCounter.ShouldBe(123);
        performanceData.CurrentTime.ShouldBe(1000);
        performanceData.RunningTime.ShouldBe(800);
        performanceData.StoppedTime.ShouldBe(150);
        performanceData.FaultedTime.ShouldBe(50);
        performanceData.StatusFaultReason.ShouldBe(4);
        performanceData.TotalProduction.ShouldBe(500.5);
        performanceData.ProductionOk.ShouldBe(490.0);
        performanceData.ProductionNoK.ShouldBe(10.5);
        performanceData.StatusFaultReject.ShouldBe(2);
        performanceData.RejectEventCounter.ShouldBe(5);
        performanceData.StatusReject.ShouldBe(1);
        performanceData.RejectQuantityUnits.ShouldBe(2.5);
        performanceData.StandardCycleTime.ShouldBe(1.5);
        performanceData.ActualCycleTime.ShouldBe(1.6);
        performanceData.PlanedProductionTime.ShouldBe(950.0);
    }
    /// <summary>
    /// Executes FromPlc_WithMissingKeys_ShouldDefaultToZero operation.
    /// </summary>

    [Fact]
    public void FromPlc_WithMissingKeys_ShouldDefaultToZero()
    {
        // Arrange
        var registers = new Dictionary<string, Register>();

        // Act
        var performanceData = PerformanceData.FromPlc(registers);

        // Assert
        performanceData.ApplicationFlag.ShouldBe(0);
        performanceData.EventCounter.ShouldBe(0);
        performanceData.TotalProduction.ShouldBe(0.0);
        performanceData.StandardCycleTime.ShouldBe(0.0);
    }
    /// <summary>
    /// Executes FromPlc_WithInvalidData_ShouldDefaultToZero operation.
    /// </summary>

    [Fact]
    public void FromPlc_WithInvalidData_ShouldDefaultToZero()
    {
        // Arrange
        var registers = new Dictionary<string, Register>
        {
            ["ApplicationFlag"] = Register.CreateFixture(value: "abc"),
            ["TotalProduction"] = Register.CreateFixture(value: "xyz"),
            ["RunningTime"] = Register.CreateFixture(value: "1.2.3"), // Invalid double
            ["EventCounter"] = Register.CreateFixture(value: "bad") // Unparseable int
        };

        // Act
        var performanceData = PerformanceData.FromPlc(registers);

        // Assert
        performanceData.ApplicationFlag.ShouldBe(0);
        performanceData.TotalProduction.ShouldBe(0.0);
        performanceData.RunningTime.ShouldBe(0);
        performanceData.EventCounter.ShouldBe(0);
    }
    /// <summary>
    /// Executes FromPlc_WithEmptyRegisterValue_ShouldDefaultToZero operation.
    /// </summary>

    [Fact]
    public void FromPlc_WithEmptyRegisterValue_ShouldDefaultToZero()
    {
        // Arrange — a register legitimately carries an empty reading (null Value is no longer
        // representable: Register.Create rejects null and the column is NOT NULL).
        var registers = new Dictionary<string, Register>
        {
            ["ApplicationFlag"] = Register.CreateFixture(value: string.Empty)
        };

        // Act
        var performanceData = PerformanceData.FromPlc(registers);

        // Assert
        performanceData.ApplicationFlag.ShouldBe(0);
    }
}