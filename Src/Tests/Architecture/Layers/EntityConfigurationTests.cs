// <copyright file="EntityConfigurationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Shouldly;
using IndTrace.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using NetArchTest.Rules;

namespace Architecture.Tests.Layers;
/// <summary>
/// Represents the EntityConfigurationTests.
/// </summary>

public class EntityConfigurationTests
{
    /// <summary>
    /// Executes EntityConfigurationClasses_ShouldImplement_IEntityTypeConfiguration operation.
    /// </summary>
    [Fact]
    public void EntityConfigurationClasses_ShouldImplement_IEntityTypeConfiguration()
    {
        // Act
        var result = Types.InAssembly(typeof(CyclesConfiguration).Assembly)
            .That()
            .ResideInNamespace("IndTrace.Persistence.Configurations")
            .Should()
            .ImplementInterface(typeof(IEntityTypeConfiguration<>))
            .GetResult();

        // Assert
        result.IsSuccessful.ShouldBeTrue("all entity configuration classes should implement IEntityTypeConfiguration<>.");
    }
}