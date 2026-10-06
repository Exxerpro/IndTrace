// <copyright file="ShiftDetailVm.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Shifts.Queries.GetShiftDetail;

/// <summary>
/// Represents the ShiftDetailVm.
/// </summary>
public class ShiftDetailVm
{
    /// <summary>
    /// Gets or sets the ShiftId.
    /// </summary>
    public int ShiftId { get; set; }

    /// <summary>
    /// Gets or sets the PartNumber.
    /// </summary>
    // [Fix]
    // CLAUDE
    // Date: 22/08/2025
    // Reason: #84 - genuinely-optional VM string; typed `string?` (default null) so the null default the tests
    // assert is preserved WITHOUT a banned null-forgiving initializer (the mapper only sets a subset of props).
    public string? PartNumber { get; set; }

    /// <summary>
    /// Gets or sets the ShiftName.
    /// </summary>
    // [Fix]
    // CLAUDE
    // Date: 22/08/2025
    // Reason: #84 - genuinely-optional VM string; typed `string?` (default null) so the null default the tests
    // assert is preserved WITHOUT a banned null-forgiving initializer (the mapper only sets a subset of props).
    public string? ShiftName { get; set; }

    /// <summary>
    /// Gets or sets the IsActive.
    /// </summary>
    public int IsActive { get; set; }

    /// <summary>
    /// Gets or sets the Version.
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Gets or sets the CustomerPartNumber.
    /// </summary>
    // [Fix]
    // CLAUDE
    // Date: 22/08/2025
    // Reason: #84 - genuinely-optional VM string; typed `string?` (default null) so the null default the tests
    // assert is preserved WITHOUT a banned null-forgiving initializer (the mapper only sets a subset of props).
    public string? CustomerPartNumber { get; set; }

    /// <summary>
    /// Gets or sets the AliasPartNumber.
    /// </summary>
    // [Fix]
    // CLAUDE
    // Date: 22/08/2025
    // Reason: #84 - genuinely-optional VM string; typed `string?` (default null) so the null default the tests
    // assert is preserved WITHOUT a banned null-forgiving initializer (the mapper only sets a subset of props).
    public string? AliasPartNumber { get; set; }

    /// <summary>
    /// Gets or sets the Description.
    /// </summary>
    // [Fix]
    // CLAUDE
    // Date: 22/08/2025
    // Reason: #84 - genuinely-optional VM string; typed `string?` (default null) so the null default the tests
    // assert is preserved WITHOUT a banned null-forgiving initializer (the mapper only sets a subset of props).
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the CreatedBy.
    /// </summary>
    // [Fix]
    // CLAUDE
    // Date: 22/08/2025
    // Reason: #84 - genuinely-optional VM string; typed `string?` (default null) so the null default the tests
    // assert is preserved WITHOUT a banned null-forgiving initializer (the mapper only sets a subset of props).
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the ModifiedBy.
    /// </summary>
    // [Fix]
    // CLAUDE
    // Date: 22/08/2025
    // Reason: #84 - genuinely-optional VM string; typed `string?` (default null) so the null default the tests
    // assert is preserved WITHOUT a banned null-forgiving initializer (the mapper only sets a subset of props).
    public string? ModifiedBy { get; set; }

    /// <summary>
    /// Gets or sets the CreatedOn.
    /// </summary>
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// Gets or sets the ModifiedOn.
    /// </summary>
    public DateTime ModifiedOn { get; set; }

    /// <summary>
    /// Converts a Shift entity to ShiftDetailVm.
    /// </summary>
    /// <param name="src">The source Shift entity.</param>
    /// <returns>A Result containing the ShiftDetailVm or failure information.</returns>
    public static Result<ShiftDetailVm> ToDto(Shift src)
    {
        if (src == null)
        {
            return Result<ShiftDetailVm>.WithFailure($"Parameter '{nameof(src)}' cannot be null");
        }

        return Result<ShiftDetailVm>.Success(new ShiftDetailVm
        {
            ShiftId = src.ShiftId.Value,
            ShiftName = src.ShiftType ?? string.Empty,
            CreatedOn = src.CreatedOn ?? DateTime.MinValue,
            ModifiedOn = src.ModifiedOn ?? DateTime.MinValue,

            // Only map properties that exist in both Shift and ShiftDetailVm
        });
    }
}