// <copyright file="EnumerationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using NetArchTest.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Shouldly;

namespace Architecture.Tests.Enumeration;
/// <summary>
/// Represents the EnumModelTests.
/// </summary>

public class EnumModelTests
{
    /// <summary>
    /// Executes All_Enumerations_Should_Not_Have_Duplicate_Ids operation.
    /// </summary>
    [Fact]
    public void All_Enumerations_Should_Not_Have_Duplicate_Ids()
    {
        var types = Types.InAssembly(Assembly.Load("IndTrace.Domain"))
            .That()
            .ResideInNamespace("IndTrace.Domain.Enum")
            .And()
            .Inherit(typeof(IndQuestEnums.EnumModel))
            .GetTypes();

        foreach (var type in types)
        {
            var instances = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(f => f.FieldType == type)
                .Select(f => f.GetValue(null))
                .Cast<IndQuestEnums.EnumModel>()
                .ToList();

            var ids = instances.Select(e => e.Value).ToList();
            ids.Distinct().Count().ShouldBe(ids.Count, $"because {type.Name} should not have duplicate Ids");
        }
    }
    /// <summary>
    /// Executes All_Enumerations_Should_Not_Have_Duplicate_Names operation.
    /// </summary>

    [Fact]
    public void All_Enumerations_Should_Not_Have_Duplicate_Names()
    {
        var types = Types.InAssembly(Assembly.Load("IndTrace.Domain"))
            .That()
            .ResideInNamespace("IndTrace.Domain.Enum")
            .And()
            .Inherit(typeof(IndQuestEnums.EnumModel))
            .GetTypes();

        foreach (var type in types)
        {
            var instances = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(f => f.FieldType == type)
                .Select(f => f.GetValue(null))
                .Cast<IndQuestEnums.EnumModel>()
                .ToList();

            var names = instances.Select(e => e.Name).ToList();
            names.Distinct().Count().ShouldBe(names.Count, $"because {type.Name} should not have duplicate Names");
        }
    }

    /// <summary>
    /// Owned/project classes should not use common BCL names to avoid ambiguity.
    /// </summary>
    [Fact]
    public void ProjectTypes_ShouldNotUse_CommonBclTypeNames()
    {
        var forbidden = new HashSet<string>(StringComparer.Ordinal)
        {
            nameof(String), nameof(Object), nameof(Task), nameof(ValueTask), nameof(TimeSpan), nameof(DateTime),
            "List", "Dictionary", "HashSet", "Stream", "File", "Path", "Action", "Func", "EventArgs"
        };

        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => (a.GetName().Name ?? string.Empty).StartsWith("IndTrace."))
            .ToArray();

        var offenders = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && forbidden.Contains(t.Name))
            .Select(t => t.FullName ?? t.Name)
            .ToList();

        offenders.ShouldBeEmpty("avoid common BCL names for owned/project types");
    }
}
