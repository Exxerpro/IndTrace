// <copyright file="GetShiftDetailQueryValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Shifts.Queries.GetShiftDetail;

/// <summary>
/// Represents the GetShiftDetailQueryValidator.
/// </summary>
public class GetShiftDetailQueryValidator : AbstractValidator<GetShiftDetailQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetShiftDetailQueryValidator"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public GetShiftDetailQueryValidator()
    {
        this.RuleFor(v => v.ShiftId).GreaterThan(0).LessThan(100);
    }
}