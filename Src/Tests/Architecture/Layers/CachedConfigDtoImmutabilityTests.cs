// <copyright file="CachedConfigDtoImmutabilityTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using IndTrace.Application.Customers;
using IndTrace.Application.WorkFlows.Dto;
using Shouldly;
using MachineDto = IndTrace.Application.Machines.Queries.GetMachinesList.MachineDto;
using MachinePlcDto = IndTrace.Application.MachinesPlcs.Queries.GetMachinesList.MachinePlcDto;

namespace Architecture.Tests.Layers;

/// <summary>
/// #225 (Slice 1) DTO-immutability ratchet: the DTO types stored inside the 60-minute cached
/// <c>ApplicationConfiguration</c> are served BY REFERENCE to every consumer, so any public settable
/// property is a plant-wide mutation vector (precedent: the #217 IndTraceControllerRx incident, where a
/// consumer mutating cached state leaked it to every other consumer). This test reflects over every public
/// writable instance property of the hardened DTOs and fails if any setter is a plain <c>set</c> instead of
/// <c>init</c> (detected via the <see cref="IsExternalInit"/> modreq on the setter's return modifiers).
/// Adding a new mutable property to one of these DTOs turns this into a build-red event.
/// </summary>
public class CachedConfigDtoImmutabilityTests
{
    /// <summary>
    /// The cached-config DTO types init-hardened in #225 Slice 1. Slice 2 candidates (e.g. <c>PlcDto</c>)
    /// join this list when they are detached and hardened.
    /// </summary>
    public static IEnumerable<object[]> HardenedCachedConfigDtos()
    {
        yield return new object[] { typeof(WorkFlowDto) };
        yield return new object[] { typeof(MachineDto) };
        yield return new object[] { typeof(CustomerDto) };
        yield return new object[] { typeof(MachinePlcDto) };
    }

    /// <summary>
    /// Every public writable instance property of a hardened cached-config DTO must be init-only.
    /// </summary>
    /// <param name="dtoType">The DTO type under ratchet.</param>
    [Theory]
    [MemberData(nameof(HardenedCachedConfigDtos))]
    public void PublicWritableProperties_MustBeInitOnly(Type dtoType)
    {
        var offenders = dtoType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.SetMethod is { IsPublic: true } setter
                        && !setter.ReturnParameter.GetRequiredCustomModifiers().Contains(typeof(IsExternalInit)))
            .Select(p => p.Name)
            .ToList();

        offenders.ShouldBeEmpty(
            $"{dtoType.Name} lives inside the cached ApplicationConfiguration served by reference for 60 minutes; " +
            $"these properties must use 'init', not 'set' (#225): {string.Join(", ", offenders)}");
    }
}
