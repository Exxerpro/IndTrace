// <copyright file="ProductId.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.ValueObjects;

/// <summary>
/// Strongly-typed identity for a <see cref="IndTrace.Domain.Entities.Product"/>. Story 35.D2 (#35, Cluster 4):
/// retypes the <c>Product</c> entity's own identity (the primary key) from a bare <see cref="int"/> to this
/// <c>readonly record struct</c> so an id transposed for another positional <c>int</c> id becomes a COMPILE error,
/// while the stored column stays byte-identical. Its three modeled inbound foreign keys
/// (<c>Rule.ProductId</c>, <c>ProductSpec.ProductId</c>, <c>BarCode.ProductId</c>) are adopted to this same struct so
/// the referencing columns stay type-compatible with the converted principal key (an int FK targeting a
/// <c>ProductId</c> principal key detonates the EF model). <c>ProductId</c> is NOT on the §7 wire, so no DTO/References
/// field is affected — but the nested cached entities that carry it must still round-trip byte-identical.
/// <para>
/// The wrapped <see cref="Value"/> is the raw persisted <c>int</c>. There is <b>no implicit conversion from
/// <see cref="int"/></b> — constructing a <c>ProductId</c> is always an explicit act (<c>new ProductId(id)</c>), so a
/// caller cannot silently pass a plain machine/barcode/cycle id where a product id is required. Narrowing <b>out</b>
/// to the raw <c>int</c> is the explicit <see cref="Value"/> read, which is exactly what every DTO / view-model /
/// structured-log / LINQ-to-SQL boundary and every plain-int reference column does (mirrors the #27
/// <c>BarCodeLabel.Value</c> trick and the #35.D1 <c>BarCodeId</c> pilot).
/// </para>
/// <para>
/// The type is intentionally <b>total</b>: it performs no validation and never throws. A product id is legitimately
/// <c>0</c> before the identity column assigns it (unpersisted) and the domain characterization tests seed arbitrary
/// values (including negatives), so a validating <c>Create</c> would both break byte-identity and reject legal
/// unpersisted state. Value equality / hashing come free from the <c>record struct</c>.
/// </para>
/// <para>
/// The type carries the <b>natural ordering of its wrapped <see cref="Value"/></b> by implementing both
/// <see cref="IComparable{T}"/> and the non-generic <see cref="IComparable"/>. This is a pure CLR concern with no
/// effect on the stored column or the EF value converter: it simply restores the ordering the bare <c>int</c> key had
/// before the retype. The <b>non-generic</b> interface is required because <c>Specification.OrderBy*</c> is typed
/// <c>Expression&lt;Func&lt;T, object&gt;&gt;</c>, so the key is boxed to <see cref="object"/> and the EF InMemory
/// provider orders it through <see cref="System.Collections.Comparer"/>, which only consults the non-generic
/// <see cref="IComparable"/>. On real SQL the converter maps <c>OrderBy(p =&gt; p.ProductId)</c> to <c>ORDER BY</c>
/// the int column, so this comparison never runs there.
/// </para>
/// </summary>
/// <param name="Value">The raw identity value as stored in the unchanged <c>int</c> identity column.</param>
public readonly record struct ProductId(int Value) : IComparable<ProductId>, IComparable, IIntId
{
    /// <summary>
    /// Compares this identity to another by its wrapped <see cref="Value"/> via the shared <see cref="IntIdComparer"/>.
    /// </summary>
    /// <param name="other">The other identity to compare against.</param>
    /// <returns>A signed number indicating the relative order of the two values.</returns>
    public int CompareTo(ProductId other) => IntIdComparer.Compare(this.Value, other.Value);

    /// <summary>
    /// Compares this identity to another object by wrapped <see cref="Value"/>, honoring the non-generic
    /// <see cref="IComparable"/> contract used by boxed ordering (a <c>null</c> sorts first; a non-<c>ProductId</c>
    /// argument is a programming error and raises <see cref="ArgumentException"/> per the framework contract).
    /// </summary>
    /// <param name="obj">The object to compare against.</param>
    /// <returns>A signed number indicating the relative order of the two values.</returns>
    public int CompareTo(object? obj) => obj switch
    {
        null => 1,
        ProductId other => this.CompareTo(other),
        _ => throw new ArgumentException($"Object must be of type {nameof(ProductId)}.", nameof(obj)),
    };
}
