// <copyright file="PartStatus.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Enum;

/// <summary>
/// Represents the status of a part in the system.
/// </summary>
public class PartStatus : EnumModel
{
    /// <summary>
    /// Gets the invalid part status.
    /// </summary>
    public static readonly PartStatus Invalid
        = new(-1, "Invalid Value");

    /// <summary>
    /// Gets the none part status.
    /// </summary>
    public static readonly PartStatus None
        = new(0, "None");

    /// <summary>
    /// Gets the OK part status.
    /// </summary>
    public static readonly PartStatus Ok
        = new(1, "Ok");

    /// <summary>
    /// Gets the NOK part status.
    /// </summary>
    public static readonly PartStatus NOk
        = new(2, "nOK");

    /// <summary>
    /// Gets the restored part status.
    /// </summary>
    /// <remarks>
    /// #98 (Option B): value 4 (the natural bit-flag slot) has no matching row in the QA45 <c>PartStatus</c>
    /// lookup seed (0,1,2,8,512). The domain value is correct — the seed simply lacks the row — and Restored is
    /// not persisted on any current write path. Reconciliation if it is ever persisted is to seed the lookup row
    /// (a gated data migration), not to change this member. Guarded by <c>EnumLookupSeedParityOnRealSqlTests</c>.
    /// </remarks>
    public static readonly PartStatus Restored
        = new(4, "Restored");

    /// <summary>
    /// Gets the rejected part status.
    /// </summary>
    public static readonly PartStatus Rejected
        = new(8, "Rejected");

    /// <summary>
    /// Gets the scrap part status.
    /// </summary>
    public static readonly PartStatus Scrap
        = new(512, "Scrap");

    public static implicit operator int(PartStatus enumerator) => enumerator.Value;

    public static implicit operator string(PartStatus enumerator) => enumerator.Value.ToString();

    public static implicit operator PartStatus(int value) => FromValue<PartStatus>(value);

    /// <summary>
    /// Initializes a new instance of the <see cref="PartStatus"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public PartStatus()
    {
    }

    /// <summary>
    /// Gets a value indicating whether this status carries a meaningful, assigned part-status value — i.e. one of
    /// the real positive flags (<see cref="Ok"/>, <see cref="NOk"/>, <see cref="Restored"/>, <see cref="Rejected"/>,
    /// <see cref="Scrap"/>). It is <see langword="false"/> for <see cref="None"/> (0), <see cref="Invalid"/> (-1),
    /// and the unset sentinel a bare <c>new PartStatus()</c> materializes (also -1). Computed from
    /// <see cref="EnumModel.Value"/> — NOT a settable field: the <see cref="PartStatus"/> statics are process-wide
    /// singletons, so a mutable field on them would let any writer corrupt global state for every holder (a
    /// cross-request / cross-thread hazard in the long-lived PLC handlers). Deriving it keeps the singletons
    /// immutable and this flag free of that vector.
    /// </summary>
    public bool HasValue => this.Value > None.Value;

    /// <summary>
    /// Retrieves a <see cref="PartStatus"/> instance from an integer value.
    /// </summary>
    /// <param name="value">The integer value representing the status.</param>
    /// <returns>A <see cref="PartStatus"/> instance corresponding to the specified value.</returns>
    public static PartStatus FromValue(int value) => FromValue<PartStatus>(value);

    private PartStatus(int value, string name, string displayName = "")
        : base(value, name, displayName)
    {
    }
}