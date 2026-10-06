// <copyright file="RuleEngineTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.RulesEngine;

/// <summary>
/// Unit tests for the generic RuleEngine&lt;T&gt; implementation.
/// Tests Railway-Oriented Programming patterns, null safety, and rule execution orchestration.
/// </summary>
public class RuleEngineTests
{
    private readonly RuleEngine<TestTarget> _ruleEngine = null!;

    public RuleEngineTests()
    {
        _ruleEngine = new RuleEngine<TestTarget>();
    }

    [Fact]
    public async Task ApplyRulesAsync_WithValidManufacturingRules_ShouldApplyAllRules()
    {
        // Arrange - Manufacturing quality control rules
        var target = new TestTarget { Value = 0, Name = "Ford F-150 Engine Block" };
        var qualityRule = Substitute.For<IRule<TestTarget>>();
        var safetyRule = Substitute.For<IRule<TestTarget>>();
        var rules = new List<IRule<TestTarget>> { qualityRule, safetyRule };

        qualityRule.ApplyAsync(target).Returns(Result.Success());
        safetyRule.ApplyAsync(target).Returns(Result.Success());

        // Act
        var result = await _ruleEngine.ApplyRulesAsync(target, rules);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        await qualityRule.Received(1).ApplyAsync(target);
        await safetyRule.Received(1).ApplyAsync(target);
    }

    [Fact]
    public async Task ApplyRulesAsync_WithEmptyRules_ShouldReturnSuccess()
    {
        // Arrange - No rules to apply (default manufacturing scenario)
        var target = new TestTarget { Value = 0, Name = "Default Manufacturing Part" };
        var rules = new List<IRule<TestTarget>>();

        // Act
        var result = await _ruleEngine.ApplyRulesAsync(target, rules);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public async Task ApplyRulesAsync_WithNullRules_ShouldReturnFailure()
    {
        // Arrange - Null rules collection (defensive programming)
        var target = new TestTarget { Value = 0, Name = "Tesla Model Y Battery" };
        IEnumerable<IRule<TestTarget>>? rules = null!;

        // Act
        var result = await _ruleEngine.ApplyRulesAsync(target, rules!);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
        result.Errors.ShouldContain(e => e.Contains("Rules collection cannot be null"));
    }

    [Fact]
    public async Task ApplyRulesAsync_WithNullTarget_ShouldReturnFailure()
    {
        // Arrange - Null target object (defensive programming)
        TestTarget? target = null!;
        var rule = Substitute.For<IRule<TestTarget>>();
        var rules = new List<IRule<TestTarget>> { rule };

        // Act
        var result = await _ruleEngine.ApplyRulesAsync(target!, rules);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
        result.Errors.ShouldContain(e => e.Contains("Target cannot be null"));
        await rule.Received(0).ApplyAsync(Arg.Any<TestTarget>());
    }

    [Fact]
    public async Task ApplyRulesAsync_WithMultiplePharmaceuticalRules_ShouldApplyInOrder()
    {
        // Arrange - Pfizer vaccine production rules (strict order required)
        var target = new TestTarget { Value = 0, Name = "Pfizer COVID-19 Vaccine Batch" };
        var temperatureRule = Substitute.For<IRule<TestTarget>>();
        var sterileRule = Substitute.For<IRule<TestTarget>>();
        var qualityRule = Substitute.For<IRule<TestTarget>>();
        var rules = new List<IRule<TestTarget>> { temperatureRule, sterileRule, qualityRule };

        temperatureRule.ApplyAsync(target).Returns(Result.Success());
        sterileRule.ApplyAsync(target).Returns(Result.Success());
        qualityRule.ApplyAsync(target).Returns(Result.Success());

        // Act
        var result = await _ruleEngine.ApplyRulesAsync(target, rules);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        await temperatureRule.Received(1).ApplyAsync(target);
        await sterileRule.Received(1).ApplyAsync(target);
        await qualityRule.Received(1).ApplyAsync(target);
    }

    [Fact]
    public async Task ApplyRulesAsync_WhenRuleThrowsException_ShouldContinueWithNextRule()
    {
        // Arrange - Manufacturing scenario where one rule fails but others should continue
        var target = new TestTarget { Value = 0, Name = "BMW 3 Series Engine Component" };
        var dimensionRule = Substitute.For<IRule<TestTarget>>();
        var materialRule = Substitute.For<IRule<TestTarget>>();
        var finishRule = Substitute.For<IRule<TestTarget>>();
        var rules = new List<IRule<TestTarget>> { dimensionRule, materialRule, finishRule };

        dimensionRule.ApplyAsync(target).Returns(Result.Success());
        // materialRule.ApplyAsync(target).ShouldThrow<InvalidOperationException>(new InvalidOperationException("Material specification rule failed"));
        finishRule.ApplyAsync(target).Returns(Result.Success());

        // Act
        var result = await _ruleEngine.ApplyRulesAsync(target, rules);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse(); // Should fail because of exception
        result.Errors.ShouldNotBeEmpty();

        await dimensionRule.Received(1).ApplyAsync(target);
        await materialRule.Received(1).ApplyAsync(target);
        await finishRule.Received(1).ApplyAsync(target); // Should continue even after exception
    }

    [Fact]
    public async Task ApplyRulesAsync_WithRuleReturningFailure_ShouldCollectErrors()
    {
        // Arrange - Manufacturing rule that returns business logic failure
        var target = new TestTarget { Value = 0, Name = "Samsung Galaxy PCB Component" };
        var validationRule = Substitute.For<IRule<TestTarget>>();
        var businessRule = Substitute.For<IRule<TestTarget>>();
        var rules = new List<IRule<TestTarget>> { validationRule, businessRule };

        validationRule.ApplyAsync(target).Returns(Result.Success());
        businessRule.ApplyAsync(target).Returns(Result.WithFailure("Component dimensions out of tolerance"));

        // Act
        var result = await _ruleEngine.ApplyRulesAsync(target, rules);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
        result.Errors.ShouldContain("Component dimensions out of tolerance");
    }

    [Fact]
    public async Task ApplyRulesAsync_WithRuleReturningNull_ShouldHandleGracefully()
    {
        // Arrange - Rule returning null (defensive programming scenario)
        var target = new TestTarget { Value = 0, Name = "Intel Core i7 Processor" };
        var nullResultRule = Substitute.For<IRule<TestTarget>>();
        var rules = new List<IRule<TestTarget>> { nullResultRule };

        //[Fix]
        //CLAUDE
        //Date: 29/08/2025
        //Reason: [CS1729] Use Result success/failure factory methods instead of constructor
        nullResultRule.ApplyAsync(target).Returns(Task.FromResult(Result.Success()));

        // Act
        var result = await _ruleEngine.ApplyRulesAsync(target, rules);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public async Task ApplyRulesAsync_WithNullRuleInCollection_ShouldSkipNullRule()
    {
        // Arrange - Collection containing null rule (defensive programming)
        var target = new TestTarget { Value = 0, Name = "Aerospace Boeing 777X Wing Component" };
        var validRule = Substitute.For<IRule<TestTarget>>();
        var rules = new List<IRule<TestTarget>?> { validRule, null, validRule }.OfType<IRule<TestTarget>>().ToList();
        rules.Insert(1, null!); // Force null in the middle

        validRule.ApplyAsync(target).Returns(Result.Success());

        // Act
        var result = await _ruleEngine.ApplyRulesAsync(target, rules);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse(); // Should fail due to null rule
        result.Errors.ShouldNotBeEmpty();
        result.Errors.ShouldContain(e => e.Contains("rule") && e.Contains("cannot be null"));

        await validRule.Received(2).ApplyAsync(target); // Valid rules should still execute
    }

    [Fact]
    public async Task ApplyRulesAsync_WithLargeNumberOfIndustrialRules_ShouldApplyAllRules()
    {
        // Arrange - Large-scale manufacturing with 50+ quality control rules
        var target = new TestTarget { Value = 0, Name = "Automotive Assembly Line Quality Control" };
        var rules = new List<IRule<TestTarget>>();

        // Create 50 quality control rules for comprehensive manufacturing validation
        for (int i = 0; i < 50; i++)
        {
            var rule = Substitute.For<IRule<TestTarget>>();
            rule.ApplyAsync(target).Returns(Result.Success());
            rules.Add(rule);
        }

        // Act
        var result = await _ruleEngine.ApplyRulesAsync(target, rules);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();

        foreach (var rule in rules)
        {
            await rule.Received(1).ApplyAsync(target);
        }
    }

    [Fact]
    public async Task ApplyRulesAsync_WithMixedRuleResults_ShouldCollectAllErrors()
    {
        // Arrange - Mixed scenario with successful and failed rules
        var target = new TestTarget { Value = 0, Name = "Mixed Manufacturing Quality Control" };
        var successRule = Substitute.For<IRule<TestTarget>>();
        var failRule1 = Substitute.For<IRule<TestTarget>>();
        var failRule2 = Substitute.For<IRule<TestTarget>>();
        var rules = new List<IRule<TestTarget>> { successRule, failRule1, failRule2 };

        successRule.ApplyAsync(target).Returns(Result.Success());
        failRule1.ApplyAsync(target).Returns(Result.WithFailure("Dimensional tolerance exceeded"));
        failRule2.ApplyAsync(target).Returns(Result.WithFailure(new[] { "Surface finish inadequate", "Material hardness below spec" }));

        // Act
        var result = await _ruleEngine.ApplyRulesAsync(target, rules);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
        result.Errors.ShouldContain("Dimensional tolerance exceeded");
        result.Errors.ShouldContain("Surface finish inadequate");
        result.Errors.ShouldContain("Material hardness below spec");
        result.Errors.Count().ShouldBe(3);
    }

    [Fact]
    public async Task ApplyRulesAsync_WithDifferentTargetTypes_ShouldWorkCorrectly()
    {
        // Arrange - Different manufacturing target types (string for part numbers)
        var stringTarget = "PART-BMW-320i-ENGINE-001";
        var stringRule = Substitute.For<IRule<string>>();
        var stringRules = new List<IRule<string>> { stringRule };

        var stringRuleEngine = new RuleEngine<string>();
        stringRule.ApplyAsync(stringTarget).Returns(Result.Success());

        // Act
        var result = await stringRuleEngine.ApplyRulesAsync(stringTarget, stringRules);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        await stringRule.Received(1).ApplyAsync(stringTarget);
    }

    /// <summary>
    /// Test target class representing a manufacturing component for rule testing.
    /// </summary>
    public class TestTarget
    {
        /// <summary>
        /// Gets or sets the numeric value (e.g., dimension, quantity, temperature).
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Gets or sets the component name or part identifier.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the manufacturing status for quality control.
        /// </summary>
        public bool IsQualityApproved { get; set; } = false;
    }
}

//[Fix]
//CLAUDE
//Date: 26/08/2025
//Reason: Moved and enhanced generic RuleEngine tests to appropriate Application.UnitTests layer
// Added Railway-Oriented Programming Result<T> pattern validation, industrial manufacturing scenarios
// Enhanced test coverage for null safety, error collection, and defensive programming patterns
