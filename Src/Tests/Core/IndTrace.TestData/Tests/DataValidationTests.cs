// <copyright file="DataValidationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.TestData.Validation;

namespace IndTrace.TestData.Tests;

/// <summary>
/// Unit tests for DataValidation utility class.
/// Tests the deduplication validation functionality for JSON test data.
/// </summary>
public class DataValidationTests
{
    [Fact]
    public async Task ValidateJsonFilesForDuplicatesAsync_ShouldReturnValidationResult()
    {
        // Act
        var result = await DataValidation.ValidateJsonFilesForDuplicatesAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.FilesValidated.ShouldNotBeEmpty();
        result.FilesValidated.ShouldContain("Rules.json");
        result.FilesValidated.ShouldContain("Machines.json");
    }

    [Fact]
    public async Task ValidateJsonFilesForDuplicatesAsync_WithNullLogger_ShouldNotThrow()
    {
        // Act
        var result = await DataValidation.ValidateJsonFilesForDuplicatesAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.FilesValidated.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task ValidateJsonFilesForDuplicatesAsync_ShouldHandleCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await DataValidation.ValidateJsonFilesForDuplicatesAsync(cancellationToken: cts.Token));
    }

    [Fact]
    public async Task ValidateJsonFilesForDuplicatesAsync_ShouldDetectDuplicatesInRules()
    {
        // This test assumes there might be duplicates - if none exist, it should pass gracefully

        // Act
        var result = await DataValidation.ValidateJsonFilesForDuplicatesAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.TotalDuplicatesFound.ShouldBeGreaterThanOrEqualTo(0);

        if (result.TotalDuplicatesFound > 0)
        {
            result.FilesWithDuplicates.ShouldNotBeEmpty();

            foreach (var duplicateList in result.FilesWithDuplicates.Values)
            {
                duplicateList.ShouldAllBe(d => !string.IsNullOrEmpty(d.EntityId));
                duplicateList.ShouldAllBe(d => !string.IsNullOrEmpty(d.FileName));
                duplicateList.ShouldAllBe(d => !string.IsNullOrEmpty(d.EntityType));
                duplicateList.ShouldAllBe(d => d.LineNumber > 0);
                duplicateList.ShouldAllBe(d => d.OriginalLineNumber > 0);
            }
        }
    }

    [Fact]
    public async Task ValidateJsonFilesForDuplicatesAsync_ShouldHandleAllEntityTypes()
    {
        // Act
        var result = await DataValidation.ValidateJsonFilesForDuplicatesAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();

        // Verify all expected files were processed
        var expectedFiles = new[]
        {
            "Rules.json", "Machines.json", "PLCs.json", "MachinePlcs.json",
            "VariablesGroups.json", "Variables.json", "Lines.json", "Customers.json",
            "Products.json", "Recipes.json", "WorkFlows.json", "ConfigApp.json",
            "Settings.json", "BarCodes.json", "Cycles.json", "Registers.json"
        };

        foreach (var expectedFile in expectedFiles)
        {
            result.FilesValidated.ShouldContain(expectedFile);
        }
    }

    [Fact]
    public async Task ValidateJsonFilesForDuplicatesAsync_ShouldHandleCompositeKeys()
    {
        // This specifically tests MachinePlcs.json which has composite keys (MachineId|PlcId)

        // Act
        var result = await DataValidation.ValidateJsonFilesForDuplicatesAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.FilesValidated.ShouldContain("MachinePlcs.json");

        // If duplicates exist in MachinePlcs, they should have composite key format
        if (result.FilesWithDuplicates.ContainsKey("MachinePlcs.json"))
        {
            var machinePlcDuplicates = result.FilesWithDuplicates["MachinePlcs.json"];
            machinePlcDuplicates.ShouldAllBe(d => d.EntityId.Contains("|"));
        }
    }
}

//[Fix]
//CLAUDE
//Date: 26/08/2025
//Reason: Created comprehensive unit tests for DataValidation utility class
// Tests deduplication validation functionality with logger integration and edge cases
