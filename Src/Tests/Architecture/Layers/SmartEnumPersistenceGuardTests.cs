// <copyright file="SmartEnumPersistenceGuardTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Reflection;
using IndQuestEnums;
using IndTrace.Domain.Interfaces;
using IndTrace.Persistence.DBContext;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Architecture.Tests.Layers;

/// <summary>
/// Model-side guard (the complement to the <see cref="IPersistable"/> repository constraint): a
/// data-complete smart enumeration (<see cref="EnumModel"/>) is code-authoritative and must NEVER
/// enter the EF Core model — it is neither a <c>DbSet</c> nor configured as an entity; its values are
/// projected into a separate <see cref="ILookupEntity"/> twin (e.g. <c>FlowStatusEntity</c>) and seeded
/// from code. The compile-time constraint stops <c>IRepository&lt;SmartEnum&gt;</c>; this test stops the
/// seam the compiler cannot see — a stray <c>DbSet&lt;SmartEnum&gt;</c>, an <c>IEntityTypeConfiguration</c>
/// for one, or a mapped type that carries no persistence marker at all.
/// </summary>
public class SmartEnumPersistenceGuardTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmartEnumPersistenceGuardTests"/> class.
    /// </summary>
    /// <param name="output">The test output helper.</param>
    public SmartEnumPersistenceGuardTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private static List<Type> SmartEnumTypes() =>
        Assembly.Load("IndTrace.Domain")
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(EnumModel).IsAssignableFrom(t) && t != typeof(EnumModel))
            .ToList();

    private static List<Type> DbSetEntityTypes() =>
        typeof(IndTraceDbContext)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(p => p.PropertyType.GetGenericArguments().First())
            .ToList();

    /// <summary>
    /// No smart enumeration may be exposed as a <c>DbSet&lt;T&gt;</c> on the DbContext.
    /// </summary>
    [Fact]
    public void SmartEnums_ShouldNotBe_RegisteredAsDbSet()
    {
        var smartEnums = SmartEnumTypes().ToHashSet();

        var offenders = DbSetEntityTypes()
            .Where(smartEnums.Contains)
            .ToList();

        foreach (var offender in offenders)
        {
            _output.WriteLine($"Smart enum wrongly exposed as DbSet: {offender.FullName}");
        }

        offenders.ShouldBeEmpty(
            "a smart enumeration (EnumModel) is code-authoritative and must not be a DbSet; persist its ILookupEntity twin instead");
    }

    /// <summary>
    /// No smart enumeration may have an <see cref="IEntityTypeConfiguration{TEntity}"/> — that would map
    /// it into the model without a DbSet.
    /// </summary>
    [Fact]
    public void SmartEnums_ShouldNotHave_EntityTypeConfiguration()
    {
        var smartEnums = SmartEnumTypes().ToHashSet();

        var configuredTypes = Assembly.Load("IndTrace.Persistence")
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .SelectMany(t => t.GetInterfaces())
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
            .Select(i => i.GetGenericArguments().First())
            .ToList();

        var offenders = configuredTypes.Where(smartEnums.Contains).Distinct().ToList();

        foreach (var offender in offenders)
        {
            _output.WriteLine($"Smart enum wrongly given an IEntityTypeConfiguration: {offender.FullName}");
        }

        offenders.ShouldBeEmpty(
            "a smart enumeration must not be configured as an EF entity; configure its ILookupEntity twin instead");
    }

    /// <summary>
    /// Every entity exposed as a <c>DbSet&lt;T&gt;</c> must carry a persistence marker
    /// (<see cref="IPersistable"/> via <see cref="IEntityRoot"/> or <see cref="ILookupEntity"/>).
    /// This is the never-implemented half the marker XML docs promise, and it keeps value objects,
    /// DTOs, and smart enums out of the model.
    /// </summary>
    [Fact]
    public void AllDbSetEntities_ShouldImplement_IPersistable()
    {
        var unmarked = DbSetEntityTypes()
            .Where(t => !typeof(IPersistable).IsAssignableFrom(t))
            .ToList();

        foreach (var offender in unmarked)
        {
            _output.WriteLine($"DbSet entity missing IPersistable marker (IEntityRoot/ILookupEntity): {offender.FullName}");
        }

        unmarked.ShouldBeEmpty(
            "every persisted entity must implement IPersistable (IEntityRoot or ILookupEntity); an unmarked mapped type is how a value object, DTO, or smart enum slips into the database");
    }
}
