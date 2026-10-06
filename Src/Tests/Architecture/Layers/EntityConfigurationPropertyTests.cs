// <copyright file="EntityConfigurationPropertyTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Reflection;
using Shouldly;
using IndTrace.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetArchTest.Rules;

namespace Architecture.Tests.Layers;
/// <summary>
/// Represents the EntityConfigurationPropertyTests.
/// </summary>

public class EntityConfigurationPropertyTests
{
    /// <summary>
    /// Executes AllEntityConfigurations_ShouldHaveRequiredProperties operation.
    /// </summary>
    [Fact]
    public void AllEntityConfigurations_ShouldHaveRequiredProperties()
    {
        var configurationTypes = Types.InAssembly(typeof(CyclesConfiguration).Assembly)
            .That()
            .ResideInNamespace("IndTrace.Persistence.Configurations")
            .And()
            .ImplementInterface(typeof(IEntityTypeConfiguration<>))
            .GetTypes();

        configurationTypes.ShouldNotBeEmpty("there should be entity configuration classes implementing IEntityTypeConfiguration<>.");

        // Metadata-based: ensure Configure(T) signature exists and parameterized with concrete entity type
        var offenders = new List<string>();
        foreach (var configType in configurationTypes)
        {
            // Skip open generic base configurators
            if (configType.IsGenericTypeDefinition)
                continue;

            var configureMethod = configType.GetMethod("Configure", BindingFlags.Public | BindingFlags.Instance);
            if (configureMethod == null)
            {
                offenders.Add($"{configType.Name} missing Configure method");
                continue;
            }

            var parameters = configureMethod.GetParameters();
            if (parameters.Length != 1)
            {
                offenders.Add($"{configType.Name} Configure should take one parameter");
                continue;
            }

            var paramType = parameters[0].ParameterType;
            if (!paramType.IsGenericType || paramType.GetGenericTypeDefinition() != typeof(EntityTypeBuilder<>))
            {
                offenders.Add($"{configType.Name} Configure parameter must be EntityTypeBuilder<T>");
                continue;
            }

            var target = paramType.GetGenericArguments()[0];
            if (target.ContainsGenericParameters || target.IsAbstract)
            {
                offenders.Add($"{configType.Name} targets invalid entity {target}");
            }
        }

        offenders.ShouldBeEmpty("entity configurations must expose Configure(EntityTypeBuilder<T>) for a concrete entity type");
    }
}