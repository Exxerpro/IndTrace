// <copyright file="RegisterCleanerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.UnitTests.Cycles.Services;

using IndTrace.Application.Cycles.Services;
using Meziantou.Extensions.Logging.Xunit;

/// <summary>
/// Unit tests for RegisterCleaner.
/// </summary>
public class RegisterCleanerTests
{
    private readonly ILogger<RegisterCleaner> _logger;
    private readonly RegisterCleaner _cleaner;
    private readonly DateTime _testTimestamp = new(2025, 9, 17, 12, 0, 0);

    public RegisterCleanerTests(ITestOutputHelper output)
    {
        _logger = XUnitLogger.CreateLogger<RegisterCleaner>(output);
        _cleaner = new RegisterCleaner(_logger);
    }

    [Fact]
    public void CleanRegisters_WithNullRegisters_ShouldReturnFailure()
    {
        // Act
        var result = _cleaner.CleanRegisters(null!, 1, 100, _testTimestamp);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldContain("registers");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CleanRegisters_WithInvalidCycleId_ShouldReturnFailure(int cycleId)
    {
        // Arrange
        var registers = new Dictionary<string, Register>();

        // Act
        var result = _cleaner.CleanRegisters(registers, cycleId, 100, _testTimestamp);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldContain("cycleId");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CleanRegisters_WithInvalidMachineId_ShouldReturnFailure(int machineId)
    {
        // Arrange
        var registers = new Dictionary<string, Register>();

        // Act
        var result = _cleaner.CleanRegisters(registers, 1, machineId, _testTimestamp);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldContain("machineId");
    }

    [Fact]
    public void CleanRegisters_WithDirtyRegisters_ShouldCleanAndSetMetadata()
    {
        // Arrange
        var registers = new Dictionary<string, Register>
        {
            ["temp"] = Register.CreateFixture(registerId: 999, name: "  Temperature\n\r\t", description: "Temp\tsensor\nreading", machineId: 0, cycleId: 0, value: "25.5\r\n", timeStamp: DateTime.MinValue),
            ["pressure"] = Register.CreateFixture(name: "Pressure", description: string.Empty, value: string.Empty)
        };

        // Act
        var result = _cleaner.CleanRegisters(registers, 123, 456, _testTimestamp);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        
        var cleanedList = result.Value.ToList();
        cleanedList.Count.ShouldBe(2);

        var tempRegister = cleanedList.First(r => r.Name == "Temperature");
        tempRegister.Name.ShouldBe("Temperature");
        tempRegister.Description.ShouldBe("Tempsensorreading");
        tempRegister.Value.ShouldBe("25.5");
        tempRegister.RegisterId.ShouldBe(0);
        tempRegister.CycleId.Value.ShouldBe(123);
        tempRegister.MachineId.ShouldBe(456);
        tempRegister.TimeStamp.ShouldBe(_testTimestamp);

        var pressureRegister = cleanedList.First(r => r.Name == "Pressure");
        pressureRegister.Description.ShouldBe(string.Empty);
        pressureRegister.Value.ShouldBe(string.Empty);
    }

    [Fact]
    public void CleanRegisters_WithNullRegisterInDictionary_ShouldSkipIt()
    {
        // Arrange
        var registers = new Dictionary<string, Register>
        {
            ["good"] = Register.CreateFixture(name: "Good", value: "123"),
            ["null"] = null!,
            ["another"] = Register.CreateFixture(name: "Another", value: "456")
        };

        // Act
        var result = _cleaner.CleanRegisters(registers, 1, 100, _testTimestamp);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count().ShouldBe(2);
        result.Value.Any(r => r.Name == "Good").ShouldBeTrue();
        result.Value.Any(r => r.Name == "Another").ShouldBeTrue();
    }

    [Fact]
    public void CleanRegisters_WithEmptyDictionary_ShouldReturnEmptyList()
    {
        // Arrange
        var registers = new Dictionary<string, Register>();

        // Act
        var result = _cleaner.CleanRegisters(registers, 1, 100, _testTimestamp);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count().ShouldBe(0);
    }
}