// <copyright file="CacheKeyBuilderReadOnlyReposTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Repository;
using IndTrace.Domain.Entities;
using IndTrace.Persistence.Caching;
using Xunit;
using IndTrace.Agregation.Dependices.Dependencies;

namespace IndTrace.Agregation.Dependices.Infrastructure.Caching;

/// <summary>
/// Joined to the non-parallel "CacheKeyBuilderStatics" collection so it never runs concurrently
/// with any other test. The class mutates process-global statics on CacheKeyBuilderReadOnlyRepos,
/// which are also written by every DependenciesFactory fixture via CachePartitionInitializer;
/// serializing removes the clobber race behind GitHub issue #53.
/// </summary>
[Collection("CacheKeyBuilderStatics")]
public class CacheKeyBuilderReadOnlyReposTests
{
    private readonly string _prefix;
    private sealed class TestPartitionProvider(string prefix) : ICachePartitionProvider
    {
        private readonly string _prefix = prefix;
        public string GetPrefix() => _prefix;
    }

    private static ISpecification<Product> NewProductSpec(string? tag = null)
    {
        // Simple, deterministic specification
        var spec = new Specification<Product>(p => p.ProductId.Value > 0)
            .ApplyNoTracking();
        if (!string.IsNullOrWhiteSpace(tag))
            spec = (Specification<Product>)spec.AddInclude(tag!);
        return spec;
    }

    public CacheKeyBuilderReadOnlyReposTests()
    {
        // Use a real GUID partition for assertions to include prefix
        var provider = new TestCachePartitionProvider();
        CacheKeyBuilderReadOnlyRepos.SetPartitionProvider(provider);
        _prefix = provider.GetPrefix();

        // Default: no hashing, short hash length
        CacheKeyBuilderReadOnlyRepos.SetOptions(new CacheKeyOptions { HashSpecKeys = false, HashLength = 8 });
    }

    [Fact]
    public void SpecKey_IncludesTypeAndSpec_WhenHashingDisabled()
    {
        // Arrange
        var spec = NewProductSpec("Include:Category");

        // Act
        var key = CacheKeyBuilderReadOnlyRepos.BuildKey<Product>("List", spec);

        // Assert
        var expected = $"{_prefix}:ListAsync|Type:Product|Spec:{spec.Key}";
        Assert.Equal(expected, key);
    }

    [Fact]
    public void SpecKey_UsesHash_WhenHashingEnabled()
    {
        // Arrange
        CacheKeyBuilderReadOnlyRepos.SetOptions(new CacheKeyOptions { HashSpecKeys = true, HashLength = 8 });
        var spec = NewProductSpec();

        // Act
        var key = CacheKeyBuilderReadOnlyRepos.BuildKey<Product>("ListAsync", spec);

        // Assert
        Assert.Matches($"^{_prefix}:ListAsync\\|Type:Product\\|Spec:[0-9A-F]{{1,8}}$", key);
    }

    [Fact]
    public void SingleId_UsesEntitySpecificFieldName()
    {
        // Arrange
        CacheKeyBuilderReadOnlyRepos.SetOptions(new CacheKeyOptions { HashSpecKeys = false });

        // Act
        var keyMachine = CacheKeyBuilderReadOnlyRepos.BuildKey<Machine>("GetById", 42);
        var keyPlc = CacheKeyBuilderReadOnlyRepos.BuildKey<Plc>("get", 7); // alias should normalize to GetById

        // Assert
        Assert.Equal($"{_prefix}:GetById|Type:Machine|MachineId:42", keyMachine);
        Assert.Equal($"{_prefix}:GetById|Type:Plc|PlcId:7", keyPlc);
    }

    [Fact]
    public void CompositeIds_TwoInts_UsesCompositeFieldNames()
    {
        // Arrange
        CacheKeyBuilderReadOnlyRepos.SetOptions(new CacheKeyOptions { HashSpecKeys = false });

        // Act
        var key = CacheKeyBuilderReadOnlyRepos.BuildKey<MachinePlc>("getbyid", 1, 2);

        // Assert
        Assert.Equal($"{_prefix}:GetById|Type:MachinePlc|MachineId:1|PlcId:2", key);
    }

    [Fact]
    public void CompositeIds_Params_UsesFieldNames_WhenCountMatches()
    {
        // Arrange
        CacheKeyBuilderReadOnlyRepos.SetOptions(new CacheKeyOptions { HashSpecKeys = false });

        // Act
        var key = CacheKeyBuilderReadOnlyRepos.BuildKey<DistinctRegister>("GetById", 12, 34);

        // Assert
        Assert.Equal($"{_prefix}:GetById|Type:DistinctRegister|MachineId:12|VariableId:34", key);
    }

    [Fact]
    public void CompositeIds_Params_FallbacksToPositional_WhenCountDiffers()
    {
        // Arrange
        CacheKeyBuilderReadOnlyRepos.SetOptions(new CacheKeyOptions { HashSpecKeys = false });

        // Act
        var key = CacheKeyBuilderReadOnlyRepos.BuildKey<MachinePlc>("GetById", 10, 20, 30);

        // Assert
        Assert.Equal($"{_prefix}:GetById|Type:MachinePlc|Id0:10|Id1:20|Id2:30", key);
    }

    [Fact]
    public void OperationNormalization_MapsAliases_AndStripsSpaces()
    {
        // Arrange
        CacheKeyBuilderReadOnlyRepos.SetOptions(new CacheKeyOptions { HashSpecKeys = false });

        // Act
        var spec = NewProductSpec();
        var canonical = CacheKeyBuilderReadOnlyRepos.BuildKey<Product>("get all", spec);
        var unknown = CacheKeyBuilderReadOnlyRepos.BuildKey<Product>("Foo Bar", 1);

        // Assert
        Assert.Equal($"{_prefix}:ListAsync|Type:Product|Spec:{spec.Key}", canonical);
        Assert.Equal($"{_prefix}:FooBar|Type:Product|ProductId:1", unknown);
    }
}