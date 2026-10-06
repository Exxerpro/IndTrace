// <copyright file="TestDataLoaderTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.TestData.Tests;

/// <summary>
/// Unit tests for the TestData loading infrastructure.
/// </summary>
public class TestDataLoaderTests
{
    private readonly ITestDataLoader _loader;

    public TestDataLoaderTests()
    {
        _loader = new EmbeddedTestDataLoader();
    }

    [Fact]
    public void Constructor_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var loader = new EmbeddedTestDataLoader();

        // Assert
        loader.ShouldNotBeNull();
    }

    [Fact]
    public void GetAvailableFiles_ShouldReturnJsonFiles()
    {
        // Act
        var files = _loader.GetAvailableFiles().ToList();

        // Assert
        files.ShouldNotBeEmpty();
        files.ShouldContain("Rules.json");
        files.ShouldContain("Machines.json");
        files.ShouldAllBe(f => f.EndsWith(".json"));
    }

    [Fact]
    public void Exists_WithValidFile_ShouldReturnTrue()
    {
        // Act & Assert
        _loader.Exists("Rules.json").ShouldBeTrue();
        _loader.Exists("Machines.json").ShouldBeTrue();
        _loader.Exists("rules.json").ShouldBeTrue(); // Case insensitive
    }

    [Fact]
    public void Exists_WithInvalidFile_ShouldReturnFalse()
    {
        // Act & Assert
        _loader.Exists("NonExistent.json").ShouldBeFalse();
        _loader.Exists("").ShouldBeFalse();
    }

    [Fact]
    public async Task LoadListAsync_WithValidRulesFile_ShouldReturnData()
    {
        // Act
        var rules = await _loader.LoadListAsync<Rule>("Rules.json", TestContext.Current.CancellationToken);

        // Assert
        rules.ShouldNotBeNull();
        rules.ShouldNotBeEmpty();
        rules.ShouldAllBe(r => r is Rule);

        // Verify some basic rule properties
        var firstRule = rules.First();
        firstRule.RuleId.ShouldBeGreaterThan(0);
        firstRule.Name.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoadListAsync_WithValidMachinesFile_ShouldReturnDataAndHandleEnums()
    {
        try
        {
            // Act
            var machines = await _loader.LoadListAsync<Machine>("Machines.json", TestContext.Current.CancellationToken);

            // Assert
            machines.ShouldNotBeNull();
            machines.ShouldNotBeEmpty();
            machines.ShouldAllBe(m => m is Machine);

            // Verify some basic machine properties
            var firstMachine = machines.First();
            firstMachine.MachineId.Value.ShouldBeGreaterThanOrEqualTo(0);
            firstMachine.Name.ShouldNotBeNullOrEmpty();
        }
        catch (JsonException ex)
        {
            // Capture the exact error for debugging
            throw new InvalidOperationException($"JSON deserialization failed: {ex.Message}\nInner: {ex.InnerException?.Message}");
        }
        catch (InvalidOperationException ex)
        {
            // Capture our custom error for debugging
            throw new InvalidOperationException($"TestData loading failed: {ex.Message}\nInner: {ex.InnerException?.Message}");
        }
    }

    [Fact]
    public async Task LoadListAsync_WithNonExistentFile_ShouldReturnEmptyList()
    {
        // Act
        var result = await _loader.LoadListAsync<Rule>("NonExistent.json", TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task LoadSingleAsync_WithValidFile_ShouldReturnFirstItem()
    {
        // Act
        var rule = await _loader.LoadSingleAsync<Rule>("Rules.json", TestContext.Current.CancellationToken);

        // Assert
        rule.ShouldNotBeNull();
        rule.RuleId.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task LoadSingleAsync_WithEmptyFile_ShouldReturnNull()
    {
        // Act
        var result = await _loader.LoadSingleAsync<Rule>("NonExistent.json", TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeNull();
    }
}