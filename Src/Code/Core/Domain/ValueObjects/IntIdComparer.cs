// <copyright file="IntIdComparer.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.ValueObjects;

/// <summary>
/// Shared ordering helper for the strongly-typed-id sweep (Story 35.D2 (#35) shared infrastructure). Id value structs
/// cannot inherit a common base to share ordering, so each id's <see cref="System.IComparable{T}.CompareTo(T)"/> and
/// non-generic <see cref="System.IComparable.CompareTo(object)"/> bodies delegate to <see cref="Compare"/> to keep
/// those bodies thin and to guarantee every id orders by its wrapped <see cref="IIntId.Value"/> identically. This is
/// a pure CLR concern (it restores the ordering the bare <c>int</c> key had before the retype) with no effect on the
/// stored column, the EF value converter, or the §7 wire.
/// </summary>
public static class IntIdComparer
{
    /// <summary>
    /// Compares two raw identity values by their natural <see cref="int"/> ordering.
    /// </summary>
    /// <param name="left">The wrapped value of the left identity.</param>
    /// <param name="right">The wrapped value of the right identity.</param>
    /// <returns>A signed number indicating the relative order of the two values.</returns>
    public static int Compare(int left, int right) => left.CompareTo(right);
}
