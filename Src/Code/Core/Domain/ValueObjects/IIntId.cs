// <copyright file="IIntId.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.ValueObjects;

/// <summary>
/// Marker contract for a strongly-typed identity value struct that wraps a single raw <see cref="int"/> key
/// (Story 35.D2 (#35) shared infrastructure). Every id struct produced by the strongly-typed-id sweep
/// (<c>BarCodeId</c>, <c>MachineId</c>, <c>ProductId</c>, <c>CycleId</c>, <c>ShiftId</c>, ...) implements this so a
/// single reusable <see cref="StronglyTypedIdJsonConverterFactory"/> and a single
/// <c>HasIntIdConversion&lt;TId&gt;</c> EF extension can serve them all instead of one bespoke converter per id.
/// <para>
/// The wrapped <see cref="Value"/> is the raw persisted <c>int</c>. Exposing it is the sanctioned narrowing seam:
/// the §7 wire, structured-log templates, and EF value converters all read <see cref="Value"/> so the stored column
/// and the wire bytes stay byte-identical to the pre-retype bare <c>int</c>. The interface intentionally declares no
/// constructor — id structs are constructed explicitly via <c>new TId(value)</c>, which the JSON factory reaches
/// generically through a cached compiled constructor delegate rather than a parameterless-ctor requirement.
/// </para>
/// </summary>
public interface IIntId
{
    /// <summary>
    /// Gets the raw identity value as stored in the unchanged <c>int</c> key column.
    /// </summary>
    int Value { get; }
}
