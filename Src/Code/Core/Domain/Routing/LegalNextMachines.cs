// <copyright file="LegalNextMachines.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.ValueObjects;

namespace IndTrace.Domain.Routing;

/// <summary>
/// E6-1 role value-type (#56): the legal-successor <b>set</b> for a part sitting at a
/// <see cref="LastProcessedMachine"/> — the arrival-validation yardstick. A reported arrival is legal iff the
/// <see cref="RequestingMachine"/> is a member of this set (I4), which is the surgical fix E6-1 makes: on a
/// diverter a part legally has more than one possible next machine, so validation is <b>membership</b> in
/// this set, not equality against a single computed next.
/// </summary>
/// <remarks>
/// <para>
/// The set is <c>= graph.NextMachines(LastProcessedMachine)</c> (I3), computed at load time and never stored.
/// It is exactly the <see cref="ProductionGraph.NextMachines(int)"/> projection (∪ the terminal <c>0</c>
/// sentinel for a Final node), promoted from the singular next to the full successor set. An <b>empty</b> set
/// is legal and meaningful (a node with no legal successor); at end-of-line the set is the singleton
/// <c>{ 0 }</c> — the wire boundary sentinel — so the only legal "next" is to leave the line.
/// </para>
/// <para>
/// On linear routing every node has exactly one successor, so this set has size 1 and membership is
/// byte-identical to the legacy equality check — E6-1 changes behaviour only on multi-successor topology.
/// </para>
/// </remarks>
/// <param name="Machines">The legal successor machine ids (may be empty; never <see langword="null"/> in practice — members guard against a <c>default</c> value).</param>
public readonly record struct LegalNextMachines(IReadOnlyList<MachineId> Machines)
{
    /// <summary>
    /// Gets the number of legal successor machines (0 when the set is empty or the value is a
    /// <c>default</c> struct).
    /// </summary>
    public int Count => this.Machines?.Count ?? 0;

    /// <summary>
    /// Determines whether <paramref name="id"/> is a legal successor — i.e. is a member of this set.
    /// </summary>
    /// <param name="id">The candidate next machine id.</param>
    /// <returns><see langword="true"/> when <paramref name="id"/> is in the legal-successor set; otherwise <see langword="false"/>.</returns>
    public bool Contains(MachineId id) => this.Machines is { } machines && machines.Contains(id);

    /// <summary>
    /// Determines whether a <see cref="RequestingMachine"/>'s arrival is permitted — i.e. the requesting
    /// machine is a legal successor (I4). This is the membership predicate the arrival gate calls.
    /// </summary>
    /// <param name="machine">The machine requesting permit-to-work / information.</param>
    /// <returns><see langword="true"/> when the requesting machine is a legal next; otherwise <see langword="false"/>.</returns>
    public bool Permits(RequestingMachine machine) => this.Contains(machine.Value);

    /// <summary>
    /// Value equality by CONTENTS, order-insensitive (#126 F5). The compiler-synthesized record-struct
    /// equality compared the wrapped <see cref="Machines"/> list by REFERENCE, so two instances with
    /// identical contents were unequal (and <see cref="ProductRoutingState"/>, which holds this value,
    /// inherited the defect). This type is a legal-successor <b>set</b> consumed purely through membership
    /// (<see cref="Contains"/> / <see cref="Permits"/>), so the traversal order the constructor happened to
    /// see is an implementation detail, never domain meaning: the same members in a different order are the
    /// same set. Duplicates carry no domain meaning either (construction de-duplicates), but equality still
    /// compares with multiplicity so the observable <see cref="Count"/> can never differ between two equal
    /// values. A <c>default</c> value (null list) equals an instance wrapping an empty list — every member
    /// already treats both as the empty set.
    /// </summary>
    /// <param name="other">The other legal-successor set.</param>
    /// <returns><see langword="true"/> when both wrap the same members (with multiplicity); otherwise <see langword="false"/>.</returns>
    public bool Equals(LegalNextMachines other)
    {
        var mine = this.Machines;
        var theirs = other.Machines;
        if (ReferenceEquals(mine, theirs))
        {
            return true;
        }

        var myCount = mine?.Count ?? 0;
        var theirCount = theirs?.Count ?? 0;
        if (myCount != theirCount)
        {
            return false;
        }

        if (myCount == 0 || mine is null || theirs is null)
        {
            // Equal counts and at least one side empty/null: both are the empty set.
            return myCount == 0;
        }

        // Order-insensitive multiset compare: tally one side, drain with the other.
        var tally = new Dictionary<MachineId, int>(myCount);
        foreach (var id in mine)
        {
            tally[id] = tally.TryGetValue(id, out var seen) ? seen + 1 : 1;
        }

        foreach (var id in theirs)
        {
            if (!tally.TryGetValue(id, out var remaining) || remaining == 0)
            {
                return false;
            }

            tally[id] = remaining - 1;
        }

        return true;
    }

    /// <summary>
    /// Order-insensitive hash consistent with <see cref="Equals(LegalNextMachines)"/>: combines the member
    /// count with the commutative sum of the member hashes, so equal sets hash equal regardless of the
    /// order the constructor saw. The empty set (a <c>default</c> value or an empty list) hashes to <c>0</c>.
    /// </summary>
    /// <returns>The hash code of the set contents.</returns>
    public override int GetHashCode()
    {
        var machines = this.Machines;
        if (machines is null || machines.Count == 0)
        {
            return 0;
        }

        var sum = 0;
        foreach (var id in machines)
        {
            unchecked
            {
                sum += id.GetHashCode();
            }
        }

        return HashCode.Combine(machines.Count, sum);
    }
}
