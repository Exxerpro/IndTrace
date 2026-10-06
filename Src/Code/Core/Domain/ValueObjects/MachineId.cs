// <copyright file="MachineId.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.ValueObjects;

/// <summary>
/// Strongly-typed identity for a <see cref="IndTrace.Domain.Entities.Machine"/>. Story 35.D2 (#35, Cluster 5 — the
/// riskiest, largest cluster): retypes the <c>Machine</c> entity's own identity (the primary key) from a bare
/// <see cref="int"/> to this <c>readonly record struct</c> so an id transposed for another positional <c>int</c> id
/// becomes a COMPILE error, while the stored column and the life-critical §7 wire bytes stay byte-identical. A wrong
/// machine byte routes a part to the wrong machine, so this type exists to make that class of transposition
/// impossible to write. Its fourteen modeled inbound foreign keys (<c>Cycle</c>, <c>DefectRegister</c>,
/// <c>MachineStatus</c>, <c>Rule</c>, <c>RoutingNodeRow</c>, <c>MachinePlc</c>, <c>ConnectionStatus</c>,
/// <c>ProductSpec</c>, <c>Setting</c>, <c>StatusConfiguration</c>, <c>BarCode</c>, <c>StoppageRegister</c>,
/// <c>WorkFlow.NextMachineId</c>, <c>WorkFlow.LastMachineId</c>) are adopted to this same struct so the referencing
/// columns stay type-compatible with the converted principal key (an int FK targeting a <c>MachineId</c> principal key
/// detonates the EF model). Machine's key is caller-supplied (<c>ValueGeneratedNever</c>), not identity-generated.
/// <para>
/// The wrapped <see cref="Value"/> is the raw persisted <c>int</c>. There is <b>no implicit conversion from
/// <see cref="int"/></b> — constructing a <c>MachineId</c> is always an explicit act (<c>new MachineId(id)</c>), so a
/// caller cannot silently pass a plain barcode/cycle/product id where a machine id is required. Narrowing <b>out</b>
/// to the raw <c>int</c> is the explicit <see cref="Value"/> read, which is exactly what every §7 wire / DTO /
/// view-model / structured-log / LINQ-to-SQL boundary and every plain-int reference column does (mirrors the #27
/// <c>BarCodeLabel.Value</c> trick and the #35.D1 <c>BarCodeId</c> pilot).
/// </para>
/// <para>
/// The type is intentionally <b>total</b>: it performs no validation and never throws. Machine id <c>0</c> is a REAL
/// domain sentinel ("no machine" / end-of-line) that rides the §7 wire and the routing graph, and the domain
/// characterization tests seed arbitrary values (including negatives), so a validating <c>Create</c> would both break
/// byte-identity and reject legal sentinel/unpersisted state. Value equality / hashing come free from the
/// <c>record struct</c>.
/// </para>
/// <para>
/// The type carries the <b>natural ordering of its wrapped <see cref="Value"/></b> by implementing both
/// <see cref="IComparable{T}"/> and the non-generic <see cref="IComparable"/>. This is a pure CLR concern with no
/// effect on the stored column, the EF value converter, or the §7 wire (which emits <see cref="Value"/>): it simply
/// restores the ordering the bare <c>int</c> key had before the retype. The <b>non-generic</b> interface is required
/// because <c>Specification.OrderBy*</c> is typed <c>Expression&lt;Func&lt;T, object&gt;&gt;</c>, so the key is boxed
/// to <see cref="object"/> and the EF InMemory provider orders it through <see cref="System.Collections.Comparer"/>,
/// which only consults the non-generic <see cref="IComparable"/>. On real SQL the converter maps
/// <c>OrderBy(m =&gt; m.MachineId)</c> to <c>ORDER BY</c> the int column, so this comparison never runs there.
/// </para>
/// </summary>
/// <param name="Value">The raw identity value as stored in the unchanged <c>int</c> key column.</param>
public readonly record struct MachineId(int Value) : IComparable<MachineId>, IComparable, IIntId
{
    /// <summary>
    /// Compares this identity to another by its wrapped <see cref="Value"/> via the shared <see cref="IntIdComparer"/>.
    /// </summary>
    /// <param name="other">The other identity to compare against.</param>
    /// <returns>A signed number indicating the relative order of the two values.</returns>
    public int CompareTo(MachineId other) => IntIdComparer.Compare(this.Value, other.Value);

    /// <summary>
    /// Compares this identity to another object by wrapped <see cref="Value"/>, honoring the non-generic
    /// <see cref="IComparable"/> contract used by boxed ordering (a <c>null</c> sorts first; a non-<c>MachineId</c>
    /// argument is a programming error and raises <see cref="ArgumentException"/> per the framework contract).
    /// </summary>
    /// <param name="obj">The object to compare against.</param>
    /// <returns>A signed number indicating the relative order of the two values.</returns>
    public int CompareTo(object? obj) => obj switch
    {
        null => 1,
        MachineId other => this.CompareTo(other),
        _ => throw new ArgumentException($"Object must be of type {nameof(MachineId)}.", nameof(obj)),
    };
}
