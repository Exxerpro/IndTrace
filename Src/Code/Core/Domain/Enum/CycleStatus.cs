// <copyright file="CycleStatus.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Enum;

/// <summary>
/// Represents the status of a cycle in the system, such as NotStarted, Started, FinishedOk, FinishedNok, etc.
/// </summary>
public class CycleStatus : EnumModel
{
    /// <summary>
    /// Represents an invalid cycle status.
    /// </summary>
    public static readonly CycleStatus Invalid =
        new(-1, "Invalid Value");

    /// <summary>
    /// Represents no cycle status.
    /// </summary>
    public static readonly CycleStatus None
        = new(0, "None");

    /// <summary>
    /// Represents a cycle that has not started.
    /// </summary>
    public static readonly CycleStatus NotStarted
        = new(1, "NotStarted");

    /// <summary>
    /// Represents a cycle that has started.
    /// </summary>
    public static readonly CycleStatus Started
        = new(2, "Started");

    /// <summary>
    /// Represents a cycle that finished successfully.
    /// </summary>
    public static readonly CycleStatus FinishedOk
        = new(4, "FinishedOk");

    /// <summary>
    /// Represents a cycle that finished with a failure.
    /// </summary>
    public static readonly CycleStatus FinishedNok
        = new(8, "FinishedNok");

    /// <summary>
    /// Represents the end of a process cycle.
    /// </summary>
    public static readonly CycleStatus EndOfProcess
        = new(16, "EndOfProcess");

    /// <summary>
    /// Represents a cycle that was rejected.
    /// </summary>
    public static readonly CycleStatus Rejected
        = new(32, "Rejected");

    /// <summary>
    /// Represents a cycle that was canceled.
    /// </summary>
    /// <remarks>
    /// #98 (Option B): value 64 has no matching row in the QA45 <c>CycleStatus</c> lookup seed (which stops at
    /// <see cref="Rejected"/>=32; the seed's Canceled row is at id 16, already taken here by
    /// <see cref="EndOfProcess"/>). Persisting Canceled would violate the lookup FK on real SQL. It is doubly
    /// dormant today — <c>CancelCycle</c> is flag-gated off (<c>EnableCanceledState=false</c>) and the
    /// <c>Cycles.CycleStatus</c> FK is disabled — so it is never written. The value is kept at 64: correcting it
    /// to 16 would collide with <see cref="EndOfProcess"/>. Reconciliation when <c>CancelCycle</c> is activated is
    /// to seed the lookup row (a gated data migration), not to change this member. Guarded by
    /// <c>EnumLookupSeedParityOnRealSqlTests</c>.
    /// </remarks>
    public static readonly CycleStatus Canceled
        = new(64, "Canceled");

    /// <summary>
    /// Initializes a new instance of the <see cref="CycleStatus"/> class.
    /// </summary>
    public CycleStatus()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CycleStatus"/> class with specified values.
    /// </summary>
    /// <param name="value">The integer value.</param>
    /// <param name="name">The name.</param>
    /// <param name="displayName">The display name.</param>
    private CycleStatus(int value, string name, string? displayName = null)
        : base(value, name, displayName ?? string.Empty)
    {
    }

    /// <summary>
    /// Implicitly converts a CycleStatus to its string representation.
    /// </summary>
    /// <param name="enumerator">The enumerator to convert.</param>
    public static implicit operator string(CycleStatus enumerator) => enumerator.Value.ToString();

    /// <summary>
    /// Implicitly converts an integer value to a CycleStatus.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator CycleStatus(int value) => FromValue<CycleStatus>(value);

    /// <summary>
    /// Implicitly converts a nullable integer value to a CycleStatus.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator CycleStatus(int? value) => FromValue<CycleStatus>(value ?? 0);

    /// <summary>
    /// Retrieves a <see cref="CycleStatus"/> instance from an integer value.
    /// </summary>
    /// <param name="value">The integer value representing the status.</param>
    /// <returns>A <see cref="CycleStatus"/> instance corresponding to the specified value.</returns>
    public static CycleStatus FromValue(int value) => FromValue<CycleStatus>(value);
}
