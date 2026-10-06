// <copyright file="DateTimeMachineIssue126RegressionTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Reflection;
using IndTrace.Domain.Interfaces;
using IndTrace.Domain.Models;
using Shouldly;

namespace IndTrace.Domain.UnitTests;

/// <summary>
/// Regression tests for issue #126 F2 — <see cref="DateTimeMachine"/> defects: the dead public
/// <c>TimeProvider</c> setter (a decoy nothing reads — every member uses the ctor-captured
/// provider) and the unguarded reflective Invoke in <see cref="DateTimeMachine.SetDateTimeNow"/>.
/// </summary>
public class DateTimeMachineIssue126RegressionTests
{
    /// <summary>
    /// #126 F2.2 — the settable <c>TimeProvider</c> auto-property was a decoy: assigning it never
    /// changed the clock. It must not exist on the interface or the implementation.
    /// </summary>
    [Fact]
    public void TimeProvider_DeadSetterProperty_MustNotExist()
    {
        typeof(IDateTimeMachine).GetProperty("TimeProvider").ShouldBeNull();
        typeof(DateTimeMachine).GetProperty("TimeProvider").ShouldBeNull();
    }

    /// <summary>
    /// #126 F2.3 — SetDateTimeNow's reflective <c>Advance</c> invocation must be guarded like its
    /// sibling <see cref="DateTimeMachine.AdvanceTime"/>: a throwing provider yields a failure
    /// Result, never an exception across the boundary.
    /// </summary>
    [Fact]
    public void SetDateTimeNow_WhenAdvanceThrows_ReturnsFailureInsteadOfThrowing()
    {
        // Arrange - a provider whose type NAME matches the reflective lookup but whose Advance throws.
        var machine = new DateTimeMachine(new FakeTimeProvider());

        // Act & Assert - in DEBUG the throwing Advance must surface as a failure Result;
        // in RELEASE the method is unsupported and already returns a failure. Both stay exception-free.
        var result = Should.NotThrow(() => machine.SetDateTimeNow(DateTimeOffset.UtcNow.AddHours(1)));
        result.IsFailure.ShouldBeTrue();
    }

    /// <summary>
    /// Type-name doppelganger for the reflective FakeTimeProvider lookup whose Advance always
    /// throws — simulates a faulting test-time provider. The warning-free sibling
    /// (Microsoft.Extensions.Time.Testing.FakeTimeProvider) is intentionally NOT used here.
    /// </summary>
    private sealed class FakeTimeProvider : TimeProvider
    {
        /// <summary>
        /// Matches the reflective contract (<c>GetMethod("Advance")</c>) and always throws.
        /// </summary>
        /// <param name="delta">The time span to advance by (ignored).</param>
        public void Advance(TimeSpan delta)
            => throw new InvalidOperationException($"Simulated Advance fault ({delta}).");
    }
}
