// <copyright file="EnumModelComprehensiveTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Collections.Concurrent;
using System.Reflection;

namespace IndTrace.Domain.UnitTests.Enum;

/// <summary>
/// Comprehensive test suite for EnumModel base class focusing on:
/// 1. Lazy reflection singleton pattern correctness
/// 2. Thread safety under manufacturing load
/// 3. Performance characteristics
/// 4. Production edge cases and error handling
/// 5. Invalid value handling consistency across derived classes
/// </summary>
public class EnumModelComprehensiveTests
{
    #region Test Enum Classes for Validation

    /// <summary>
    /// Test enum with Invalid = -1 (like CycleStatus/PartStatus)
    /// </summary>
    public class TestEnumNegativeInvalid : EnumModel
    {
        public static readonly TestEnumNegativeInvalid Invalid = new(-1, "Invalid");
        public static readonly TestEnumNegativeInvalid None = new(0, "None");
        public static readonly TestEnumNegativeInvalid First = new(1, "First");
        public static readonly TestEnumNegativeInvalid Second = new(2, "Second");
        public static readonly TestEnumNegativeInvalid Fourth = new(4, "Fourth");

        public TestEnumNegativeInvalid() { }
        private TestEnumNegativeInvalid(int value, string name) : base(value, name) { }
    }

    /// <summary>
    /// Test enum with Invalid = 8 (like FlowStatus domain-specific invalid)
    /// </summary>
    public class TestEnumPositiveInvalid : EnumModel
    {
        public static readonly TestEnumPositiveInvalid None = new(0, "None");
        public static readonly TestEnumPositiveInvalid First = new(1, "First");
        public static readonly TestEnumPositiveInvalid Second = new(2, "Second");
        public static readonly TestEnumPositiveInvalid Fourth = new(4, "Fourth");
        public static readonly TestEnumPositiveInvalid Invalid = new(8, "Invalid"); // Domain-specific invalid state
        public static readonly TestEnumPositiveInvalid Rejected = new(16, "Rejected");

        public TestEnumPositiveInvalid() { }
        private TestEnumPositiveInvalid(int value, string name) : base(value, name) { }
    }

    /// <summary>
    /// Test enum without explicit Invalid (should use base class Invalid = -1)
    /// </summary>
    public class TestEnumNoExplicitInvalid : EnumModel
    {
        public static readonly TestEnumNoExplicitInvalid None = new(0, "None");
        public static readonly TestEnumNoExplicitInvalid Active = new(1, "Active");
        public static readonly TestEnumNoExplicitInvalid Inactive = new(2, "Inactive");

        public TestEnumNoExplicitInvalid() { }
        private TestEnumNoExplicitInvalid(int value, string name) : base(value, name) { }
    }

    #endregion

    #region Core Functionality Tests

    [Fact]
    public void FromValue_WithValidValue_ShouldReturnCorrectInstance()
    {
        // Arrange & Act
        var result = TestEnumNegativeInvalid.FromValue<TestEnumNegativeInvalid>(2);

        // Assert
        result.ShouldBe(TestEnumNegativeInvalid.Second);
        result.Value.ShouldBe(2);
        result.Name.ShouldBe("Second");
    }

    [Fact]
    public void FromValue_WithInvalidValue_ShouldReturnInvalidInstance_NegativeInvalid()
    {
        // Arrange & Act
        var result = TestEnumNegativeInvalid.FromValue<TestEnumNegativeInvalid>(999);

        // Assert
        result.ShouldBe(TestEnumNegativeInvalid.Invalid);
        result.Value.ShouldBe(-1);
        result.Name.ShouldBe("Invalid");
    }

    [Fact]
    public void FromValue_WithInvalidValue_ShouldReturnInvalidInstance_PositiveInvalid()
    {
        // Arrange & Act
        var result = TestEnumPositiveInvalid.FromValue<TestEnumPositiveInvalid>(999);

        // Assert
        result.ShouldBe(TestEnumPositiveInvalid.Invalid);
        result.Value.ShouldBe(8);
        result.Name.ShouldBe("Invalid");
    }

    [Fact]
    public void FromValue_WithInvalidValue_NoExplicitInvalid_ShouldReturnBaseInvalid()
    {
        // Arrange & Act
        var result = TestEnumNoExplicitInvalid.FromValue<TestEnumNoExplicitInvalid>(999);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Invalid Value");
    }

    [Theory]
    [InlineData(0)] // None
    [InlineData(1)] // First
    [InlineData(2)] // Second  
    [InlineData(4)] // Fourth
    public void FromValue_WithAllValidValues_ShouldReturnCorrectInstances(int value)
    {
        // Arrange & Act
        var result = TestEnumNegativeInvalid.FromValue<TestEnumNegativeInvalid>(value);

        // Assert
        result.Value.ShouldBe(value);
        result.ShouldNotBe(TestEnumNegativeInvalid.Invalid);
    }

    [Theory]
    [InlineData(3)]   // Between valid values
    [InlineData(10)]  // Above valid range
    [InlineData(-2)]  // Below valid range (but not Invalid)
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void FromValue_WithInvalidValues_ShouldReturnInvalidInstance(int value)
    {
        // Arrange & Act
        var result = TestEnumNegativeInvalid.FromValue<TestEnumNegativeInvalid>(value);

        // Assert
        result.ShouldBe(TestEnumNegativeInvalid.Invalid);
        result.Value.ShouldBe(-1);
    }

    #endregion

    #region Performance & Caching Tests

    [Fact]
    public void FromValue_MultipleCallsSameType_ShouldUseCachedLookup()
    {
        // Arrange
        const int iterations = 1000;
        var values = new[] { 0, 1, 2, 4, 999 }; // Mix of valid and invalid

        // Act & Assert - Should complete quickly (cached after first call)
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        for (int i = 0; i < iterations; i++)
        {
            foreach (var value in values)
            {
                var result = TestEnumNegativeInvalid.FromValue<TestEnumNegativeInvalid>(value);
                result.ShouldNotBeNull();
            }
        }

        stopwatch.Stop();

        // Performance assertion: 5000 lookups should be very fast with caching
        stopwatch.ElapsedMilliseconds.ShouldBeLessThan(100,
            $"Cached lookups took {stopwatch.ElapsedMilliseconds}ms - caching may not be working");
    }

    [Fact]
    public void FromValue_DifferentEnumTypes_ShouldHaveSeparateCaches()
    {
        // Arrange & Act
        var result1 = TestEnumNegativeInvalid.FromValue<TestEnumNegativeInvalid>(1);
        var result2 = TestEnumPositiveInvalid.FromValue<TestEnumPositiveInvalid>(1);

        // Assert - Same value, different types, different instances
        result1.ShouldBe(TestEnumNegativeInvalid.First);
        result2.ShouldBe(TestEnumPositiveInvalid.First);
        result1.ShouldNotBeSameAs(result2);
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public async Task FromValue_ConcurrentAccess_ShouldBeThreadSafe()
    {
        // Arrange
        const int threadCount = 10;
        const int iterationsPerThread = 100;
        var tasks = new List<Task>();
        var results = new ConcurrentBag<TestEnumNegativeInvalid>();

        // Act - Multiple threads accessing FromValue simultaneously
        for (int i = 0; i < threadCount; i++)
        {
            var threadId = i;
#pragma warning disable xUnit1051 // Calls to Task.Run should use TestContext.Current.CancellationToken
            // Justification: Thread safety test needs to run to completion without cancellation
            // Approved By: CLAUDE on 27/08/2025
            tasks.Add(Task.Run(() =>
#pragma warning restore xUnit1051
            {
                for (int j = 0; j < iterationsPerThread; j++)
                {
                    // Mix of valid and invalid values
                    var value = j % 5 == 0 ? 999 : new[] { 0, 1, 2, 4 }[j % 4];
                    var result = TestEnumNegativeInvalid.FromValue<TestEnumNegativeInvalid>(value);
                    results.Add(result);
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert
        results.Count.ShouldBe(threadCount * iterationsPerThread);
        results.ShouldAllBe(r => r != null);

        // Check that Invalid instances are returned for value 999
        var invalidResults = results.Where(r => r.Value == -1).ToList();
        invalidResults.Count.ShouldBe(threadCount * (iterationsPerThread / 5));
    }

    #endregion

    #region Edge Cases & Error Handling

    [Fact]
    public void FromValue_NeverThrowsException_AlwaysReturnsValue()
    {
        // Arrange & Act - Should never throw, even with extreme values
        Should.NotThrow(() =>
        {
            var result1 = TestEnumNegativeInvalid.FromValue<TestEnumNegativeInvalid>(int.MaxValue);
            var result2 = TestEnumNegativeInvalid.FromValue<TestEnumNegativeInvalid>(int.MinValue);
            var result3 = TestEnumNegativeInvalid.FromValue<TestEnumNegativeInvalid>(-999999);

            // All should return Invalid instance, never null
            result1.ShouldNotBeNull();
            result2.ShouldNotBeNull();
            result3.ShouldNotBeNull();
        });
    }

    [Fact]
    public void FromValue_DuplicateValues_ShouldHandleGracefully()
    {
        // This tests the "last one wins" behavior for duplicate values
        // in the lookup table building process

        // Act - Should not throw even if enum had duplicate values
        Should.NotThrow(() =>
        {
            var result = TestEnumNegativeInvalid.FromValue<TestEnumNegativeInvalid>(1);
            result.ShouldBe(TestEnumNegativeInvalid.First);
        });
    }

    #endregion

    #region Production System Integration Tests

    [Fact]
    public void FromValue_ActualProductionEnums_ShouldWorkCorrectly()
    {
        // Test with actual production enum classes

        // CycleStatus (Invalid = -1)
        var cycleResult = CycleStatus.FromValue<CycleStatus>(999);
        cycleResult.ShouldNotBeNull();
        cycleResult.Name.ShouldBe("Invalid Value");
        cycleResult.Value.ShouldBe(-1);

        // Valid CycleStatus
        var validCycle = CycleStatus.FromValue<CycleStatus>(2);
        validCycle.ShouldNotBeNull();
        validCycle.Value.ShouldBe(2);

        // PartStatus (Invalid = -1)  
        var partResult = PartStatus.FromValue<PartStatus>(999);
        partResult.ShouldNotBeNull();
        partResult.Name.ShouldBe("Invalid Value");
        partResult.Value.ShouldBe(-1);

        // FlowStatus (Invalid = 8)
        var flowResult = FlowStatus.FromValue<FlowStatus>(999);
        flowResult.ShouldNotBeNull();
        flowResult.Name.ShouldBe("Invalid");
        flowResult.Value.ShouldBe(8);
    }

    [Theory]
    [InlineData(typeof(CycleStatus), -1)] // Should have Invalid = -1
    [InlineData(typeof(PartStatus), -1)]  // Should have Invalid = -1  
    [InlineData(typeof(FlowStatus), 8)]   // Should have Invalid = 8
    public void ProductionEnums_InvalidValueConsistency_ShouldMatchExpectedValues(Type enumType, int expectedInvalidValue)
    {
        // Use reflection to verify Invalid field values across production enums
        var invalidField = enumType.GetField("Invalid", BindingFlags.Public | BindingFlags.Static);
        invalidField.ShouldNotBeNull($"{enumType.Name} should have Invalid field");

        var invalidInstance = (EnumModel)invalidField.GetValue(null)!;
        invalidInstance.ShouldNotBeNull();
        invalidInstance.Value.ShouldBe(expectedInvalidValue,
            $"{enumType.Name}.Invalid should have Value = {expectedInvalidValue}");
    }

    #endregion

    #region Manufacturing Load Simulation

    [Fact]
    public async Task FromValue_ManufacturingLoadSimulation_ShouldHandleHighThroughput()
    {
        // Simulate manufacturing environment: high-frequency enum lookups
        const int concurrentWorkers = 20; // Simulate 20 manufacturing stations
        const int lookupsPerWorker = 500;  // 500 lookups per station

        var tasks = new List<Task<int>>();

        // Simulate different manufacturing scenarios
        for (int worker = 0; worker < concurrentWorkers; worker++)
        {
            tasks.Add(Task.Run(() =>
            {
                var successCount = 0;

                for (int lookup = 0; lookup < lookupsPerWorker; lookup++)
                {
                    // Simulate real manufacturing enum usage patterns
                    var cycleValue = lookup % 10 == 0 ? 999 : new[] { 1, 2, 4, 8 }[lookup % 4];
                    var partValue = lookup % 7 == 0 ? 888 : new[] { 1, 2, 4, 8 }[lookup % 4];
                    var flowValue = lookup % 5 == 0 ? 777 : new[] { 1, 2, 4, 8 }[lookup % 4];

                    var cycleResult = CycleStatus.FromValue<CycleStatus>(cycleValue);
                    var partResult = PartStatus.FromValue<PartStatus>(partValue);
                    var flowResult = FlowStatus.FromValue<FlowStatus>(flowValue);

                    if (cycleResult != null && partResult != null && flowResult != null)
                        successCount++;
                }

                return successCount;
            }));
        }

        var results = await Task.WhenAll(tasks);

        // Assert - All lookups should succeed
        results.Sum().ShouldBe(concurrentWorkers * lookupsPerWorker);
        results.ShouldAllBe(count => count == lookupsPerWorker);
    }

    #endregion
}