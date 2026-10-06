// <copyright file="CycleCompletionTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Reflection;

namespace IndTrace.Domain.UnitTests.Entities;

/// <summary>
/// #40: pins the factory-only <see cref="CycleCompletion"/> idempotency marker — a valid completion is built
/// only through the guarded <see cref="CycleCompletion.Create"/> factory (returning <see cref="Result{T}"/>,
/// never throwing), and a completion for a non-persisted cycle (id &lt;= 0) is rejected.
/// </summary>
public class CycleCompletionTests
{
    [Fact]
    public void Create_WithPositiveCycleId_SucceedsAndCarriesTheValues()
    {
        var completedOn = new DateTime(2026, 7, 4, 10, 0, 0, DateTimeKind.Local);

        var result = CycleCompletion.Create(cycleId: 777, machineId: 100, completedOn: completedOn);

        result.IsSuccess.ShouldBeTrue();
        var marker = result.Value.ShouldNotBeNull();
        marker.CycleId.Value.ShouldBe(777);
        marker.MachineId.ShouldBe(100);
        marker.CompletedOn.ShouldBe(completedOn);
        marker.CycleCompletionId.ShouldBe(0); // surrogate identity left to the DB (Chunk 40-B)
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositiveCycleId_FailsWithoutThrowing(int cycleId)
    {
        var result = CycleCompletion.Create(cycleId, machineId: 100, completedOn: default);

        result.IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public void CycleCompletion_HasNoPublicConstructor_FactoryOnly()
    {
        // Mirrors the Register precedent: the only public construction path is Create(...).
        var publicCtors = typeof(CycleCompletion)
            .GetConstructors(BindingFlags.Public | BindingFlags.Instance);

        publicCtors.ShouldBeEmpty();
    }
}
