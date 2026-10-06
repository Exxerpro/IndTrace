// <copyright file="CycleCompletion.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities.BarCodes;

using IndTrace.Domain.Interfaces;
using IndTrace.Domain.ValueObjects;

/// <summary>
/// #40 (B2-idempotency): the one-per-cycle completion marker, a member of the <see cref="BarCode"/> aggregate.
/// A cycle is <c>Started</c> once and <c>Finished</c> once — and rework is a NEW <see cref="Cycle"/> (a new
/// <see cref="ValueObjects.CycleId"/>) — so "this cycle completed" is a one-shot fact keyed by
/// <see cref="CycleId"/>. The BarCode aggregate stages exactly one marker per cycle-completion inside its atomic
/// batch; the (Chunk 40-B) <c>UNIQUE(CycleId)</c> index turns a PLC re-send into a single-row collision that the
/// repository (Chunk 40-C) recognises and converts into an idempotent no-op. It doubles as a clean completion
/// audit.
/// </summary>
/// <remarks>
/// Lives in the <c>IndTrace.Domain.Entities.BarCodes</c> aggregate namespace (alongside <see cref="BarCode"/>),
/// so it is intentionally OUTSIDE the flat <c>IndTrace.Domain.Entities</c> namespace the DbContext-DbSet
/// architecture audit scans — Chunk 40-A adds the domain type only; Chunk 40-B adds the <c>DbSet</c>, the
/// EF configuration (<c>UNIQUE(CycleId)</c>, FK to <c>Cycle</c> Restrict) and the schema. Mirrors
/// <see cref="Register"/> exactly as the factory-only / append-only precedent: a private parameterless
/// constructor (for EF materialisation in Chunk 40-B), every property <c>private set</c>, and the sole
/// construction path is the guarded static <see cref="Create"/> factory returning <see cref="Result{T}"/>
/// (NEVER throwing). Once built, a completion marker is immutable — it is a write-once audit fact.
/// </remarks>
public class CycleCompletion : IEntityRoot
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CycleCompletion"/> class. Private by design (#40): the
    /// only construction path is <see cref="Create"/>. EF Core (Chunk 40-B) uses this constructor to
    /// materialize markers from the database.
    /// </summary>
    private CycleCompletion()
    {
    }

    /// <summary>
    /// Gets the surrogate identity for the completion marker. DB identity (<c>ValueGeneratedOnAdd</c>, Chunk
    /// 40-B); assigned by EF after insert through the private setter. Immutable to callers.
    /// </summary>
    public int CycleCompletionId { get; private set; }

    /// <summary>
    /// Gets the identifier of the completed cycle — the idempotency key (<c>UNIQUE(CycleId)</c>, Chunk 40-B).
    /// Strongly typed as <see cref="ValueObjects.CycleId"/> so the (Chunk 40-B) FK into <c>Cycle</c>'s
    /// converted principal key is type-compatible (mirrors <see cref="Register.CycleId"/>).
    /// </summary>
    public CycleId CycleId { get; private set; }

    /// <summary>
    /// Gets the machine on which the completion was recorded (a completion audit field).
    /// </summary>
    public int MachineId { get; private set; }

    /// <summary>
    /// Gets the instant the completion was recorded. Sourced from <see cref="IDateTimeMachine"/> by the
    /// aggregate root (never <c>DateTime.Now</c>).
    /// </summary>
    public DateTime CompletedOn { get; private set; }

    /// <summary>
    /// Returns a string representation of the completion marker.
    /// </summary>
    /// <returns>A string containing the marker id, cycle id, and machine id.</returns>
    public override string ToString() =>
        $"CycleCompletion {this.CycleCompletionId}: Cycle {this.CycleId.Value} on Machine {this.MachineId}";

    /// <summary>
    /// #40 PUBLIC creation seam. Constructs a completion marker for a persisted cycle. Enforces the only
    /// invariant that makes the marker meaningful: the completed cycle must be a real, persisted cycle
    /// (<paramref name="cycleId"/> strictly positive) — a completion for cycle <c>0</c> (a not-yet-persisted
    /// cycle) is never a valid idempotency fact. Non-throwing: returns a failure <see cref="Result{T}"/>
    /// rather than throwing (unlike <c>BarCodeResult.ToEntity</c>). The surrogate identity
    /// (<see cref="CycleCompletionId"/>) is left to the database (Chunk 40-B).
    /// </summary>
    /// <param name="cycleId">The completed cycle's identifier; must be strictly positive.</param>
    /// <param name="machineId">The machine on which the completion was recorded.</param>
    /// <param name="completedOn">The completion instant (from <see cref="IDateTimeMachine"/>).</param>
    /// <returns>A success result carrying the marker, or a failure describing the violated invariant.</returns>
    public static Result<CycleCompletion> Create(int cycleId, int machineId, DateTime completedOn)
    {
        if (cycleId <= 0)
        {
            return Result<CycleCompletion>.WithFailure(
                $"A cycle-completion marker requires a persisted cycle id (> 0); got {cycleId}.");
        }

        return Result<CycleCompletion>.Success(new CycleCompletion
        {
            CycleId = new CycleId(cycleId),
            MachineId = machineId,
            CompletedOn = completedOn,
        });
    }
}
