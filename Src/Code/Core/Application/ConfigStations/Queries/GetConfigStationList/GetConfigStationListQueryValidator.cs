// <copyright file="GetConfigStationListQueryValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.ConfigStations.Queries.GetConfigStationList;

/// <summary>
/// Represents the GetConfigStationListQueryValidator.
/// </summary>
public class GetConfigStationListQueryValidator : AbstractValidator<GetConfigStationListQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetConfigStationListQueryValidator"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public GetConfigStationListQueryValidator()
    {
        // [Fix]
        // CLAUDE
        // Date: 21/08/2025
        // Reason: Fixed validator logic - null passes (skip validation), empty string fails, valid strings 4-9 chars
        this.RuleFor(v => v.PartNumber)
            .NotEmpty()
            .WithMessage("PartNumber cannot be empty.")
            .Length(4, 9)
            .WithMessage("PartNumber must be between 4 and 9 characters long.")
            .Must(pn => pn.Trim().Length == pn.Length)
            .WithMessage("PartNumber must not contain leading or trailing spaces.")
            .When(pn => pn.PartNumber != null); // Skip all validation when null (null should pass)
    }
}