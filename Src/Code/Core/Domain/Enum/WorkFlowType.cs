// <copyright file="WorkFlowType.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Enum;

/// <summary>
/// Represents the type of workflow in the system, such as Initial, Serial, Lateral, Diverter, Merger, and Final.
/// </summary>
public class WorkFlowType : EnumModel
{
    /// <summary>
    /// Represents an invalid workflow type.
    /// </summary>
    public static readonly WorkFlowType Invalid
        = new(-1, "Invalid Value");

    /// <summary>
    /// Represents no workflow type.
    /// </summary>
    public static readonly WorkFlowType None
        = new(0, "None");

    /// <summary>
    /// Represents an initial workflow type.
    /// </summary>
    public static readonly WorkFlowType Initial
        = new(1, "Initial");

    /// <summary>
    /// Represents a serial (process) workflow type.
    /// </summary>
    public static readonly WorkFlowType Serial
        = new(2, "Serial");

    /// <summary>
    /// Represents a lateral workflow type.
    /// </summary>
    public static readonly WorkFlowType Lateral
        = new(4, "Lateral");

    /// <summary>
    /// Represents a diverter workflow type.
    /// </summary>
    public static readonly WorkFlowType Diverter
        = new(8, "Diverter");

    /// <summary>
    /// Represents a merger workflow type.
    /// </summary>
    public static readonly WorkFlowType Merger
        = new(16, "Merger");

    /// <summary>
    /// Represents a final workflow type.
    /// </summary>
    public static readonly WorkFlowType Final
        = new(32, "Final");

    /// <summary>
    /// Represents a parallel workflow type (concurrent routing role).
    /// </summary>
    /// <remarks>
    /// #98 (Option B): value 64 (the natural next bit-flag after <see cref="Final"/>=32) has no matching row in
    /// the QA45 <c>WorkFlowType</c> lookup seed (1..32); it is a routing role not yet used or persisted. The
    /// domain value is correct; reconciliation when parallel routing activates is to seed the lookup row (a gated
    /// data migration), not to change this member. Guarded by <c>EnumLookupSeedParityOnRealSqlTests</c>.
    /// </remarks>
    public static readonly WorkFlowType Parallel
        = new(64, "Parallel");

    /// <summary>
    /// The atomic routing flags, in ascending bit order, used to compose and decompose
    /// <see cref="WorkFlowType"/> values.
    /// </summary>
    private static readonly WorkFlowType[] AtomicFlags =
    [
        Initial, Serial, Lateral, Diverter, Merger, Final, Parallel,
    ];

    /// <summary>
    /// The legal-bit mask: the bitwise-OR of every atomic flag, derived ONCE from
    /// <see cref="AtomicFlags"/> so a future flag widens it automatically (#115 finding 10 — this
    /// was previously re-aggregated on every <see cref="From(int)"/> call). Declared AFTER
    /// <see cref="AtomicFlags"/> so static-field initialization order keeps it correct.
    /// </summary>
    private static readonly int AllAtomicBits = AtomicFlags.Aggregate(0, (acc, flag) => acc | flag.Value);

    /// <summary>
    /// Cache of the in-mask <see cref="From(int)"/> results (#115 finding 10): the in-mask value
    /// space is bounded (0..<see cref="AllAtomicBits"/>), so repeated lookups — one per
    /// <c>RoleOf</c> call on the validation and PLC-arrival paths — return the same immutable
    /// instance instead of re-deriving name arrays and allocating a new composite each call.
    /// Safe because <see cref="EnumModel"/> instances are immutable and equality is VALUE equality
    /// (type + <see cref="EnumModel.Value"/>), so instance identity is never observable through
    /// <c>==</c>/<see cref="EnumModel.Equals(EnumModel)"/>/hashing. Concurrent because the PLC
    /// arrival paths call <see cref="From(int)"/> from multiple threads.
    /// </summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<int, WorkFlowType> InMaskCache = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkFlowType"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public WorkFlowType()
    {
    }

    private WorkFlowType(int value, string name, string? displayName = null)
        : base(value, name, displayName ?? string.Empty)
    {
    }

    public static implicit operator int(WorkFlowType enumerator) => enumerator.Value;

    public static implicit operator string(WorkFlowType enumerator) => enumerator.Value.ToString();

    /// <summary>
    /// Inbound <c>int → WorkFlowType</c> classification, routed through <see cref="From(int)"/> so both
    /// inbound paths AGREE (#126 F6): a legal composite bitmask (3 = Initial|Serial, 34 = Serial|Final, …)
    /// composes losslessly instead of collapsing to <see cref="Invalid"/> as the former
    /// <c>FromValue</c>-based operator did; genuinely illegal values (negative, out-of-mask bits) still
    /// yield <see cref="Invalid"/>. Purely inbound — the outbound <c>operator int</c> is untouched, so §7
    /// wire emission is unchanged.
    /// </summary>
    /// <param name="value">The raw integer to classify.</param>
    public static implicit operator WorkFlowType(int value) => From(value);

    /// <summary>
    /// Builds a <see cref="WorkFlowType"/> from an integer value. The contract is precise:
    /// for any combination of the atomic flags (the inclusive range <c>0..allBits</c>, where
    /// <c>allBits</c> is the bitwise-OR of every atomic flag) the result is lossless — a value
    /// matching a defined member (<see cref="None"/> or an atomic flag) returns its named singleton,
    /// and a composite returns an instance carrying the combined value with a name derived from its
    /// constituent atomic flags. Any input that is negative OR carries bits outside the atomic mask
    /// is out of range and returns <see cref="Invalid"/> (it never composes a misleading name).
    /// Total and non-throwing.
    /// </summary>
    /// <param name="value">The integer value to represent.</param>
    /// <returns>
    /// A <see cref="WorkFlowType"/> whose <see cref="EnumModel.Value"/> equals
    /// <paramref name="value"/> for any combination of the atomic bits (0..allBits); otherwise the
    /// <see cref="Invalid"/> singleton.
    /// </returns>
    public static WorkFlowType From(int value)
    {
        // Out of range: negative (incl. two's-complement sign extension) or any stray bit outside the
        // atomic mask routes to the defined-singleton path, which yields Invalid rather than composing
        // a lying name. None (0) has no stray bits, so it falls through to its defined singleton below.
        // NOT cached: the out-of-mask key space is unbounded, and FromValue is already an O(1) cached
        // lookup resolving to the Invalid singleton.
        if (value < 0 || (value & ~AllAtomicBits) != 0)
        {
            return FromValue<WorkFlowType>(value);
        }

        // In-mask values are a bounded space (0..AllAtomicBits): resolve once, then serve the same
        // immutable instance on every subsequent call (#115 finding 10). Value equality is unchanged.
        return InMaskCache.GetOrAdd(value, static v =>
        {
            // A value matching a defined member (atomic flag or None) returns its singleton.
            var defined = FromValue<WorkFlowType>(v);
            if (defined.Value == v)
            {
                return defined;
            }

            // Compose the name from the atomic flags whose bits are set (all within the mask).
            var setFlags = AtomicFlags.Where(flag => (v & flag.Value) == flag.Value).ToArray();
            var name = setFlags.Length > 0
                ? string.Join("|", setFlags.Select(flag => flag.Name))
                : v.ToString(System.Globalization.CultureInfo.InvariantCulture);

            return new WorkFlowType(v, name);
        });
    }

    /// <summary>
    /// Determines whether this workflow value carries the given atomic <paramref name="flag"/>.
    /// </summary>
    /// <param name="flag">The atomic flag to test for.</param>
    /// <returns><see langword="true"/> if the flag's bits are all set; otherwise <see langword="false"/>.</returns>
    public bool Has(WorkFlowType flag)
    {
        ArgumentNullException.ThrowIfNull(flag);
        return flag.Value != 0 && (this.Value & flag.Value) == flag.Value;
    }
}
