// <copyright file="RuleTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.RulesTests;

/// <summary>
/// Unit tests for the <see cref="Rule"/> entity.
/// </summary>
public class RuleTests
{
    /// <summary>
    /// Executes Rule_Rule_Properties_ShouldSetAndGetCorrectly operation.
    /// </summary>
    [Fact]
    public void Rule_Rule_Properties_ShouldSetAndGetCorrectly()
    {
        // Arrange
        var rule = new Rule();
        var components = new List<RuleFragment> { new() { Name = "1" } };
        var functions = new List<string> { "FunctionA", "FunctionB" };

        // Act
        rule.RuleId = 10;
        rule.RuleJson = "{\"key\":\"value\"}";
        rule.Name = "Test Rule";
        rule.Description = "A rule for testing.";
        rule.Version = 2;
        rule.IsActive = true;
        rule.MachineId = new MachineId(5);
        rule.ProductId = new ProductId(20);
        rule.Components = components;
        rule.RuleFunction = functions;

        // Assert
        rule.RuleId.ShouldBe(10);
        rule.RuleJson.ShouldBe("{\"key\":\"value\"}");
        rule.Name.ShouldBe("Test Rule");
        rule.Description.ShouldBe("A rule for testing.");
        rule.Version.ShouldBe(2);
        rule.IsActive.ShouldBeTrue();
        rule.MachineId.Value.ShouldBe(5);
        rule.ProductId.Value.ShouldBe(20);
        rule.Components.ShouldBe(components);
        rule.RuleFunction.ShouldBe(functions);
    }
}