// <copyright file="AuditableEntity.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Models;

/// <summary>
/// Provides audit tracking fields for domain entities: creation and modification timestamps and the users responsible.
/// </summary>
/// <remarks>
/// This base class holds the audit data only; the timestamps are stamped by the persistence layer on save
/// (a newly constructed, not-yet-persisted entity therefore has no audit timestamps). The timestamp setters
/// coerce any null or pre-2000 date to <see langword="null"/> ("no timestamp") so legacy sentinel dates do not
/// masquerade as real audit instants.
/// </remarks>
public class AuditableEntity
{
    private DateTime? createdOn;
    private DateTime? modifiedOn;

    /// <summary>
    /// Gets or sets the identifier of the user who created the entity.
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the entity was created.
    /// </summary>
    /// <remarks>
    /// Stamped by the persistence layer when the entity is first saved.
    /// Setting this property to null or a date before the year 2000 will result in the value being set to null.
    /// </remarks>
    public DateTime? CreatedOn
    {
        get => this.createdOn;
        set => this.createdOn = NormalizeAuditDate(value);
    }

    /// <summary>
    /// Gets or sets the identifier of the user who last modified the entity.
    /// </summary>
    public string ModifiedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the entity was last modified.
    /// </summary>
    /// <remarks>
    /// Stamped by the persistence layer on each save.
    /// Setting this property to null or a date before the year 2000 will result in the value being set to null.
    /// </remarks>
    public DateTime? ModifiedOn
    {
        get => this.modifiedOn;
        set => this.modifiedOn = NormalizeAuditDate(value);
    }

    /// <summary>
    /// Normalizes an audit timestamp: any null or pre-2000 date is treated as "no timestamp" and
    /// coerced to <see langword="null"/>; all year-2000-or-later dates pass through unchanged.
    /// </summary>
    /// <param name="value">The candidate audit timestamp.</param>
    /// <returns>The supplied date if it is year 2000 or later; otherwise <see langword="null"/>.</returns>
    private static DateTime? NormalizeAuditDate(DateTime? value) =>
        value is { Year: >= 2000 } ? value : null;
}