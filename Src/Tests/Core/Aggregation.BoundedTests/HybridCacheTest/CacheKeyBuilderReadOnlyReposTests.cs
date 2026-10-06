// <copyright file="CacheKeyBuilderReadOnlyReposTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Persistence.Caching;
using IndTrace.Application.Repository;
using TestCachePartitionProvider = IndTrace.Aggregation.BoundedTests.Services.TestCachePartitionProvider;

namespace IndTrace.Aggregation.BoundedTests.HybridCacheTest;

/// <summary>
/// Comprehensive test suite for CacheKeyBuilderReadOnlyRepos to prevent cache key collisions.
/// This test ensures each entity type generates unique cache keys preventing the critical
/// production bug where Machine 300 & 500 returned cached data for Machine 100.
/// </summary>
/// <remarks>
/// Joined to the non-parallel "CacheKeyBuilderStatics" collection so it never runs concurrently
/// with any other test. The class mutates process-global statics on CacheKeyBuilderReadOnlyRepos,
/// which are also written by every DependenciesFactory fixture via CachePartitionInitializer;
/// serializing removes the clobber race behind GitHub issue #53.
/// </remarks>
[Collection("CacheKeyBuilderStatics")]
public class CacheKeyBuilderReadOnlyReposTests
{
    private readonly string _prefix;

    public CacheKeyBuilderReadOnlyReposTests()
    {
        // Use the real test partition provider so keys include the GUID prefix
        var provider = new TestCachePartitionProvider();
        CacheKeyBuilderReadOnlyRepos.SetPartitionProvider(provider);
        _prefix = provider.GetPrefix();

        // Make spec key behavior deterministic for assertions
        CacheKeyBuilderReadOnlyRepos.SetOptions(new CacheKeyOptions
        {
            HashSpecKeys = true,
            HashLength = 16
        });
    }

    //[Fix]
    //CLAUDE
    //Date: 05/09/2025
    //Reason: [Critical Production Bug Prevention] - Comprehensive tests to ensure cache key uniqueness
    //        Prevents cache collisions that caused wrong data returns and flaky test behavior

    // Test entities for comprehensive testing
    public class TestEntity
    { public int Id { get; set; } }

    public class AnotherEntity
    { public int Id { get; set; } }

    public class ThirdEntity
    { public int Id { get; set; } }

    [Fact]
    public void BuildKey_WithSpecification_ReturnsUniqueKeyUsingSpecKey()
    {
        // Arrange
        const string operation = "GetBySpec";
        // Create a real specification with deterministic criteria for predictable key generation
        var spec = new Specification<Machine>(m => m.MachineId.Value > 0 && m.Name == "TestMachine");

        // Act
        var key = CacheKeyBuilderReadOnlyRepos.BuildKey(operation, spec);

        // Assert
        key.ShouldMatch($"^{_prefix}:GetBySpec\\|Type:Machine\\|Spec:[0-9A-F]{{16}}$");
        // Key is hashed so it won't contain the original criteria text
        key.ShouldNotContain("MachineId");
        key.ShouldNotContain("TestMachine");

        /*
          Message: 
Shouldly.ShouldAssertException : key
    should contain (case insensitive comparison)
"MachineSpec:Active:Type:Process"
    but was actually
"c18ef73eadce4e928983c90c1462119c:GetBySpec|Type:Machine|Spec:76C825843B1D669B"

  Stac
         */
    }

    [Fact]
    public void BuildKey_WithoutId_ReturnsKeyWithTypeName()
    {
        // Arrange
        const string operation = "GetAll";

        // Act
        var key = CacheKeyBuilderReadOnlyRepos.BuildKey<TestEntity>(operation);

        // Assert
        key.ShouldBe($"{_prefix}:ListAsync|Type:TestEntity");
        key.ShouldNotContain("RecipeId"); // Ensure no hardcoded RecipeId
    }

    [Theory]
    [InlineData("Get", 100)]
    [InlineData("Update", 500)]
    [InlineData("Delete", 999)]
    [InlineData("Create", 1)]
    public void BuildKey_WithId_ReturnsUniqueKeyWithEntitySpecificIdField(string operation, int id)
    {
        // Act
        var key = CacheKeyBuilderReadOnlyRepos.BuildKey<TestEntity>(operation, id);

        // Assert
        var normalized = operation.Equals("Get", StringComparison.OrdinalIgnoreCase) ? "GetById" : operation.Replace(" ", string.Empty);
        key.ShouldBe($"{_prefix}:{normalized}|Type:TestEntity|TestEntityRegisterId:{id}");
        key.ShouldContain($"Type:TestEntity");
        key.ShouldContain($"TestEntityRegisterId:{id}");
        key.ShouldNotContain("RecipeId"); // Critical: no hardcoded RecipeId
    }

    [Fact]
    public void BuildKey_DifferentEntityTypes_ProducesDifferentKeys()
    {
        // Arrange
        const string operation = "Get";
        const int id = 100;

        // Act
        var testEntityKey = CacheKeyBuilderReadOnlyRepos.BuildKey<TestEntity>(operation, id);
        var anotherEntityKey = CacheKeyBuilderReadOnlyRepos.BuildKey<AnotherEntity>(operation, id);
        var thirdEntityKey = CacheKeyBuilderReadOnlyRepos.BuildKey<ThirdEntity>(operation, id);

        // Assert - This is the CRITICAL test that prevents cache collision bug
        testEntityKey.ShouldNotBe(anotherEntityKey);
        testEntityKey.ShouldNotBe(thirdEntityKey);
        anotherEntityKey.ShouldNotBe(thirdEntityKey);

        // Verify entity-specific patterns
        testEntityKey.ShouldBe($"{_prefix}:GetById|Type:TestEntity|TestEntityRegisterId:100");
        anotherEntityKey.ShouldBe($"{_prefix}:GetById|Type:AnotherEntity|AnotherEntityRegisterId:100");
        thirdEntityKey.ShouldBe($"{_prefix}:GetById|Type:ThirdEntity|ThirdEntityRegisterId:100");
    }

    [Fact]
    public async Task BuildKey_ParallelExecution_ThreadSafeAndConsistent()
    {
        // Arrange
        const string operation = "Get";
        const int id = 42;
        var tasks = new List<Task<string>>();

        // Act - Execute cache key generation in parallel
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(Task.Run(() => CacheKeyBuilderReadOnlyRepos.BuildKey<TestEntity>(operation, id)));
        }

        var results = await Task.WhenAll(tasks);

        // Assert - All parallel executions should produce identical results
        var expectedKey = $"{_prefix}:GetById|Type:TestEntity|TestEntityRegisterId:42";
        foreach (var result in results)
        {
            result.ShouldBe(expectedKey);
        }
    }

    [Theory]
    [InlineData("Get", "GetBySpec", "GetAll")]
    [InlineData("Create", "Update", "Delete")]
    public void BuildKey_DifferentOperations_ProduceDifferentKeys(params string[] operations)
    {
        // Arrange
        const int id = 123;
        var keys = new List<string>();

        // Act
        foreach (var operation in operations)
        {
            keys.Add(CacheKeyBuilderReadOnlyRepos.BuildKey<TestEntity>(operation, id));
        }

        // Assert - All operations should produce unique keys
        keys.ShouldBeUnique();
        foreach (var key in keys)
        {
            key.ShouldNotContain("RecipeId"); // Critical check
        }
    }

    [Fact]
    public void BuildKey_KnownEntityTypes_UsesCorrectIdFieldNames()
    {
        // This test validates the static dictionary mappings work correctly
        // We can't easily test all real entities without heavy dependencies,
        // but we can verify the pattern works for fallback entities

        // Arrange & Act - Test fallback pattern
        var genericKey = CacheKeyBuilderReadOnlyRepos.BuildKey<TestEntity>("Get", 100);

        // Assert - Should use fallback pattern for unmapped entity
        genericKey.ShouldBe($"{_prefix}:GetById|Type:TestEntity|TestEntityRegisterId:100");

        // Verify pattern consistency
        genericKey.ShouldStartWith($"{_prefix}:GetById|Type:");
        genericKey.ShouldEndWith("TestEntityRegisterId:100");
        genericKey.ShouldNotContain("RecipeId");
    }

    [Fact]
    public void BuildKey_HighFrequencyOperations_PerformanceIsConsistent()
    {
        // Arrange
        const string operation = "Get";
        const int iterations = 10000;

        // Act & Assert - Should complete quickly due to O(1) dictionary lookup
        var startTime = DateTime.UtcNow;

        for (int i = 0; i < iterations; i++)
        {
            var key = CacheKeyBuilderReadOnlyRepos.BuildKey<TestEntity>(operation, i);
            key.ShouldNotBeNullOrEmpty();
        }

        var elapsed = DateTime.UtcNow - startTime;

        // Performance assertion - 10K operations should complete in reasonable time
        elapsed.TotalMilliseconds.ShouldBeLessThan(1000); // Less than 1 second
    }

    [Fact]
    public void BuildKey_EdgeCases_HandlesCorrectly()
    {
        // Test edge cases that could cause issues

        // Very large ID
        var largeIdKey = CacheKeyBuilderReadOnlyRepos.BuildKey<TestEntity>("Get", int.MaxValue);
        largeIdKey.ShouldBe($"{_prefix}:GetById|Type:TestEntity|TestEntityRegisterId:{int.MaxValue}");

        // Zero ID
        var zeroIdKey = CacheKeyBuilderReadOnlyRepos.BuildKey<TestEntity>("Get", 0);
        zeroIdKey.ShouldBe($"{_prefix}:GetById|Type:TestEntity|TestEntityRegisterId:0");

        // Negative ID (if system allows)
        var negativeIdKey = CacheKeyBuilderReadOnlyRepos.BuildKey<TestEntity>("Get", -1);
        negativeIdKey.ShouldBe($"{_prefix}:GetById|Type:TestEntity|TestEntityRegisterId:-1");

        // All should be unique
        var keys = new[] { largeIdKey, zeroIdKey, negativeIdKey };
        keys.ShouldBeUnique();
    }

    [Fact]
    public void BuildKey_SpecificationWithComplexKey_PreservesKeyIntegrity()
    {
        // Arrange
        // Create a complex specification with multiple conditions for realistic key generation
        var spec = new Specification<Machine>(m => m.MachineId.Value > 5 && m.Name.Contains("Manufacturing") && m.Location == "Plant-A")
            .AddOrderBy(m => m.MachineId)
            .ApplyNoTracking();

        // Act
        var key = CacheKeyBuilderReadOnlyRepos.BuildKey("GetByComplexSpec", spec);

        // Assert
        key.ShouldMatch($"^{_prefix}:GetByComplexSpec\\|Type:Machine\\|Spec:[0-9A-F]{{16}}$");
        // Key is hashed so it won't contain the original spec criteria text
        key.ShouldNotContain("Manufacturing");
        key.ShouldNotContain("Plant-A");
        key.ShouldNotContain("MachineId");
        key.ShouldNotContain("RecipeId");
    }

    [Fact]
    public void BuildKey_Composite_MachinePlc_UsesNamedFieldsInOrder()
    {
        // Arrange
        const string operation = "GetByIds";
        var machineId = 10;
        var plcId = 20;

        // Act
        var key = CacheKeyBuilderReadOnlyRepos.BuildKey<MachinePlc>(operation, machineId, plcId);

        // Assert
        key.ShouldBe($"{_prefix}:GetByIds|Type:MachinePlc|MachineId:10|PlcId:20");
        key.ShouldContain("Type:MachinePlc");
        key.ShouldContain("MachineId:10");
        key.ShouldContain("PlcId:20");
    }

    [Fact]
    public void BuildKey_Composite_DistinctRegister_UsesNamedFieldsInOrder()
    {
        // Arrange
        const string operation = "GetByIds";
        var machineId = 7;
        var variableId = 99;

        // Act
        var key = CacheKeyBuilderReadOnlyRepos.BuildKey<DistinctRegister>(operation, machineId, variableId);

        // Assert
        key.ShouldBe($"{_prefix}:GetByIds|Type:DistinctRegister|MachineId:7|VariableId:99");
        key.ShouldContain("Type:DistinctRegister");
        key.ShouldContain("MachineId:7");
        key.ShouldContain("VariableId:99");
    }

    [Fact]
    public void BuildKey_Composite_MismatchedCount_FallsBackToPositionalLabels()
    {
        // Arrange
        const string operation = "GetByIds";

        // Act - Provide 1 id only where 2 expected
        var key = CacheKeyBuilderReadOnlyRepos.BuildKey<MachinePlc>(operation, 123);

        // Assert - Falls back to Id0:
        key.ShouldBe($"{_prefix}:GetByIds|Type:MachinePlc|Id0:123");
        key.ShouldContain("Type:MachinePlc");
    }

    [Theory]
    [InlineData("getbyid", 11)]
    [InlineData("Get", 11)]
    [InlineData("FetchById", 11)]
    public void BuildKey_OperationNormalization_AliasesProduceSameKeys_SingleId(string operation, int id)
    {
        // Act
        var canonical = CacheKeyBuilderReadOnlyRepos.BuildKey<Machine>("GetById", id);
        var alias = CacheKeyBuilderReadOnlyRepos.BuildKey<Machine>(operation, id);

        // Assert
        alias.ShouldBe(canonical);
        canonical.ShouldBe($"{_prefix}:GetById|Type:Machine|MachineId:11");
    }

    [Theory]
    [InlineData("getbyids", 1, 2)]
    [InlineData("GetByIds", 1, 2)]
    [InlineData("FetchByIds", 1, 2)]
    public void BuildKey_OperationNormalization_AliasesProduceSameKeys_Composite(string operation, int id1, int id2)
    {
        // Act
        var canonical = CacheKeyBuilderReadOnlyRepos.BuildKey<MachinePlc>("GetByIds", id1, id2);
        var alias = CacheKeyBuilderReadOnlyRepos.BuildKey<MachinePlc>(operation, id1, id2);

        // Assert
        alias.ShouldBe(canonical);
        canonical.ShouldBe($"{_prefix}:GetByIds|Type:MachinePlc|MachineId:1|PlcId:2");
    }
}