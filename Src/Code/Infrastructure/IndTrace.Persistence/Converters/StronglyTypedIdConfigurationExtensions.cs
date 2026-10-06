// <copyright file="StronglyTypedIdConfigurationExtensions.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Persistence.Converters;

using System;
using IndTrace.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Reusable EF Core configuration extension for the strongly-typed-id sweep (Story 35.D2 (#35) shared
/// infrastructure). Lets any entity configuration map an <see cref="IIntId"/> struct property to the SAME unchanged
/// <c>int</c> column with a single call — <c>builder.Property(e =&gt; e.X).HasIntIdConversion(v =&gt; new XId(v));</c>
/// — instead of hand-writing the value converter and comparer at every key/FK site.
/// </summary>
public static class StronglyTypedIdConfigurationExtensions
{
    /// <summary>
    /// Maps a strongly-typed id property to its raw <see cref="int"/> column via a byte-preserving value converter
    /// (model → DB via <see cref="IIntId.Value"/>; DB → model via the supplied <paramref name="factory"/>) plus a
    /// matching <see cref="ValueComparer{T}"/> so change tracking compares/snapshots by the wrapped value.
    /// </summary>
    /// <typeparam name="TId">The strongly-typed identity struct.</typeparam>
    /// <param name="builder">The property builder for the id property.</param>
    /// <param name="factory">The explicit <c>new TId(int)</c> constructor delegate reconstructing the id from the column.</param>
    /// <returns>The same property builder for fluent chaining.</returns>
    public static PropertyBuilder<TId> HasIntIdConversion<TId>(this PropertyBuilder<TId> builder, Func<int, TId> factory)
        where TId : struct, IIntId
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(factory);

        return builder.HasConversion(
            id => id.Value,
            value => factory(value),
            new ValueComparer<TId>(
                (left, right) => left.Value == right.Value,
                id => id.Value.GetHashCode(),
                id => id));
    }
}
