// <copyright file="Shift.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities
{
    using IndTrace.Domain.Enum;
    using IndTrace.Domain.Interfaces;
    using IndTrace.Domain.Models;
    using IndTrace.Domain.ValueObjects;

    /// <summary>
    /// Represents a work shift, including timing, type, and duration constraints.
    /// </summary>
    public class Shift : AuditableEntity, IEntityRoot
    {
        /// <summary>
        /// The default minimum shift duration (2 hours).
        /// </summary>
        private static readonly TimeSpan DefaultMinDuration = new(2, 0, 0);

        /// <summary>
        /// The default maximum shift duration (16 hours).
        /// </summary>
        private static readonly TimeSpan DefaultMaxDuration = new(16, 0, 0);

        /// <summary>
        /// The default normal shift duration (8 hours 30 minutes).
        /// </summary>
        private static readonly TimeSpan DefaultNormalDuration = new(8, 30, 0);

        // The clock is only used by the backward-compatible <see cref="IsRunningNow"/>
        // property. It is nullable because the EF Core materialization constructor cannot
        // (and must not) construct a concrete IDateTimeMachine — deterministic time is
        // supplied through the running-check overload instead.
        private readonly IDateTimeMachine? dateTimeMachine;

        /// <summary>
        /// Initializes a new instance of the <see cref="Shift"/> class.
        /// </summary>
        /// <param name="dateTimeMachine">The deterministic clock used by <see cref="IsRunningNow"/>.</param>
        public Shift(IDateTimeMachine dateTimeMachine)
        {
            this.dateTimeMachine = dateTimeMachine;
            this.ShiftType = string.Empty;
        }

        // [Fix]
        // Chunk 1 (issue #50)
        // Reason: EF Core materialization constructor no longer news up a DateTimeMachine
        // (doctrine: never `new DateTimeMachine()`). The instance clock stays null for
        // materialized entities; running checks go through IsRunningAt(DateTime).

        /// <summary>
        /// Initializes a new instance of the <see cref="Shift"/> class.
        /// Private parameterless constructor for EF Core entity materialization and the
        /// <see cref="Create"/> factory.
        /// </summary>
        private Shift()
        {
            this.dateTimeMachine = null;
            this.ShiftType = string.Empty;
        }

        /// <summary>
        /// Gets or sets the unique identifier for the shift. Story 35.D2 (#35, Cluster 2): the strongly-typed
        /// <see cref="ValueObjects.ShiftId"/> value struct, mapped to the SAME unchanged <c>int</c> identity column
        /// via a value-preserving EF converter. Read <see cref="ValueObjects.ShiftId.Value"/> to narrow to the raw
        /// <c>int</c> at DTO/view-model/log boundaries.
        /// </summary>
        public ShiftId ShiftId { get; set; }

        /// <summary>
        /// Gets the identifier of the machine this shift belongs to. Shifts are tracked per machine:
        /// the pair (<see cref="MachineId"/>, <see cref="StartBy"/>) is unique. A value of 0 denotes an
        /// unassigned/legacy shift.
        /// </summary>
        public int MachineId { get; init; }

        /// <summary>
        /// Gets the start time of the shift.
        /// </summary>
        public DateTime StartBy { get; init; }

        /// <summary>
        /// Gets the duration of the shift.
        /// </summary>
        public TimeSpan Duration { get; init; }

        /// <summary>
        /// Gets the end time of the shift. Always equals <see cref="StartBy"/> plus <see cref="Duration"/>.
        /// </summary>
        public DateTime EndTime { get; init; }

        /// <summary>
        /// Gets the shift type as text (e.g. "First"/"Second"/"Third"). This string is the
        /// <b>canonical persisted representation</b> of the shift type (column <c>dbo.Shifts.ShiftType</c>);
        /// the <see cref="Type"/> enum is a derived, never-persisted projection of it. Do NOT persist the
        /// enum's integer instead — its members are power-of-two bit flags (First=1, Second=2, Third=4),
        /// so the readable string is the intentional stored form.
        /// </summary>
        public string ShiftType { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the maximum allowed duration for the shift.
        /// </summary>
        public TimeSpan MaxDuration { get; set; } = DefaultMaxDuration;

        /// <summary>
        /// Gets or sets the minimum allowed duration for the shift.
        /// </summary>
        public TimeSpan MinDuration { get; set; } = DefaultMinDuration;

        /// <summary>
        /// Gets or sets the normal duration for the shift.
        /// </summary>
        public TimeSpan NormalDuration { get; set; } = DefaultNormalDuration;

        /// <summary>
        /// Gets a value indicating whether the shift is currently running, evaluated against
        /// the clock supplied at construction. Materialized (EF Core) instances have no clock
        /// and therefore report <see langword="false"/>; use <see cref="IsRunningAt(DateTime)"/>
        /// with an injected <see cref="IDateTimeMachine"/> for a deterministic check.
        /// </summary>
        public bool IsRunningNow => this.dateTimeMachine is not null && this.IsRunningAt(this.dateTimeMachine.Now);

        /// <summary>
        /// Gets or sets the number of successful cycles during the shift.
        /// </summary>
        public int CyclesOk { get; set; }

        /// <summary>
        /// Gets the type enumeration for the shift, derived (never persisted) from the string
        /// <see cref="ShiftType"/>. Unknown or empty text safely resolves to
        /// <see cref="Enum.ShiftType.None"/> — the derivation never throws.
        /// </summary>
        public ShiftType Type => DeriveType(this.ShiftType);

        /// <summary>
        /// Creates a validated <see cref="Shift"/> enforcing its invariants:
        /// the end time is derived from <paramref name="startBy"/> plus <paramref name="duration"/>,
        /// the duration falls within the min/max bounds, and the type is a concrete shift
        /// (First, Second or Third — never None).
        /// </summary>
        /// <param name="startBy">The shift start instant.</param>
        /// <param name="duration">The shift duration.</param>
        /// <param name="type">The detected concrete shift type.</param>
        /// <param name="machineId">The identifier of the machine the shift belongs to (must be greater than zero).</param>
        /// <param name="minDuration">Optional minimum duration bound (defaults to 2 hours).</param>
        /// <param name="maxDuration">Optional maximum duration bound (defaults to 16 hours).</param>
        /// <param name="normalDuration">Optional normal duration (defaults to 8 hours 30 minutes).</param>
        /// <returns>A successful <see cref="Result{T}"/> containing the shift, or a failure describing the broken invariant.</returns>
        public static Result<Shift> Create(
            DateTime startBy,
            TimeSpan duration,
            ShiftType type,
            int machineId,
            TimeSpan? minDuration = null,
            TimeSpan? maxDuration = null,
            TimeSpan? normalDuration = null)
        {
            if (type is null)
            {
                return Result<Shift>.WithFailure("A shift type is required.");
            }

            if (machineId <= 0)
            {
                return Result<Shift>.WithFailure(
                    $"A shift must belong to a real machine (machineId greater than zero); received {machineId}.");
            }

            var isConcreteType = type.Value == Enum.ShiftType.First.Value
                || type.Value == Enum.ShiftType.Second.Value
                || type.Value == Enum.ShiftType.Third.Value;

            if (!isConcreteType)
            {
                return Result<Shift>.WithFailure(
                    $"A created shift must have a concrete type (First, Second or Third); received '{type.Name}'.");
            }

            // #91 guard: a shift must last a positive amount of time. A zero or negative duration is degenerate
            // (empty or reversed interval) and would otherwise slip through whenever a caller supplies a
            // matching zero/negative custom minDuration. Reject it outright, independent of the min/max bounds.
            if (duration <= TimeSpan.Zero)
            {
                return Result<Shift>.WithFailure(
                    $"Shift duration must be positive; received {duration}.");
            }

            var min = minDuration ?? DefaultMinDuration;
            var max = maxDuration ?? DefaultMaxDuration;
            var normal = normalDuration ?? DefaultNormalDuration;

            if (min > max)
            {
                return Result<Shift>.WithFailure(
                    $"Minimum duration ({min}) cannot exceed maximum duration ({max}).");
            }

            if (duration < min || duration > max)
            {
                return Result<Shift>.WithFailure(
                    $"Shift duration ({duration}) must be between the minimum ({min}) and maximum ({max}) durations.");
            }

            var shift = new Shift
            {
                StartBy = startBy,
                Duration = duration,
                EndTime = startBy + duration,
                MachineId = machineId,
                ShiftType = type.Name,
                MinDuration = min,
                MaxDuration = max,
                NormalDuration = normal,
            };

            return Result<Shift>.Success(shift);
        }

        /// <summary>
        /// Derives the concrete <see cref="ShiftType"/> from its text representation without throwing.
        /// Empty or unrecognized text resolves to <see cref="Enum.ShiftType.None"/>.
        /// </summary>
        /// <param name="shiftTypeName">The stored shift-type text.</param>
        /// <returns>The matching <see cref="ShiftType"/>, or <see cref="Enum.ShiftType.None"/>.</returns>
        private static ShiftType DeriveType(string shiftTypeName)
        {
            if (string.IsNullOrWhiteSpace(shiftTypeName))
            {
                return Enum.ShiftType.None;
            }

            // #91: compare case-INSENSITIVELY. The persisted canonical form is the exact enum Name (PascalCase),
            // but legacy/hand-entered rows ("first", "FIRST") must still derive to the concrete type rather than
            // silently collapse to None. A trimmed, ordinal-ignore-case match keeps the happy path identical.
            var trimmed = shiftTypeName.Trim();

            if (string.Equals(trimmed, Enum.ShiftType.First.Name, StringComparison.OrdinalIgnoreCase))
            {
                return Enum.ShiftType.First;
            }

            if (string.Equals(trimmed, Enum.ShiftType.Second.Name, StringComparison.OrdinalIgnoreCase))
            {
                return Enum.ShiftType.Second;
            }

            if (string.Equals(trimmed, Enum.ShiftType.Third.Name, StringComparison.OrdinalIgnoreCase))
            {
                return Enum.ShiftType.Third;
            }

            return Enum.ShiftType.None;
        }

        /// <summary>
        /// Determines whether the shift is running at the supplied instant.
        /// </summary>
        /// <param name="now">The instant to evaluate, typically sourced from an <see cref="IDateTimeMachine"/>.</param>
        /// <returns><see langword="true"/> when <paramref name="now"/> is within the half-open interval
        /// <c>[StartBy, StartBy + Duration)</c>; otherwise <see langword="false"/>.</returns>
        /// <remarks>
        /// #91: the interval is HALF-OPEN — the end instant is EXCLUSIVE. The previous inclusive-end test
        /// (<c>now &lt;= StartBy + Duration</c>) made the boundary instant belong to BOTH this shift and the next
        /// (whose StartBy equals this EndTime), double-attributing a cycle reported exactly at the changeover.
        /// </remarks>
        public bool IsRunningAt(DateTime now)
        {
            return this.StartBy <= now && now < this.StartBy + this.Duration;
        }
    }
}
