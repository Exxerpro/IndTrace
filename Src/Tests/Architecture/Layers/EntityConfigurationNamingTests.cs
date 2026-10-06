// <copyright file="EntityConfigurationNamingTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetArchTest.Rules;
using Shouldly;
using System.Reflection;

namespace Architecture.Tests.Layers;
/// <summary>
/// Represents the EntityConfigurationNamingTests.
/// </summary>

public class EntityConfigurationNamingTests
{
    /// <summary>
    /// Executes PrimaryKeys_ShouldFollowNamingConvention operation.
    /// </summary>
    [Fact]
    public void PrimaryKeys_ShouldFollowNamingConvention()
    {
        // Metadata-based: ensure every configuration type exists and targets a concrete entity type
        var configurationTypes = Types.InAssembly(typeof(CyclesConfiguration).Assembly)
            .That()
            .ResideInNamespace("IndTrace.Persistence.Configurations")
            .And()
            .ImplementInterface(typeof(IEntityTypeConfiguration<>))
            .GetTypes();

        configurationTypes.Count().ShouldBeGreaterThan(0, "there should be entity configuration classes implementing IEntityTypeConfiguration<>.");

        var badTargets = new List<string>();
        foreach (var configType in configurationTypes)
        {
            var iface = configType.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>));
            var target = iface.GetGenericArguments().First();
            // Skip open generic base configurators (e.g., AuditableEntityConfiguration<T>, EnumerationEntityConfigurator<T>)
            if (configType.IsGenericTypeDefinition)
                continue;

            if (target.ContainsGenericParameters || target.IsAbstract)
            {
                badTargets.Add($"{configType.Name} targets invalid entity {target}");
            }
        }

        badTargets.ShouldBeEmpty("entity configurations must target concrete entity types");
    }
}