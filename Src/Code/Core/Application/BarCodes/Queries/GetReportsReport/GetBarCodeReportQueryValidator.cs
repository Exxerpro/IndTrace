// <copyright file="GetBarCodeReportQueryValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Queries.GetReportsReport;

/// <summary>
/// Represents the GetBarCodeReportQueryValidator.
/// </summary>
public class GetBarCodeReportQueryValidator : AbstractValidator<GetBarCodeReportQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetBarCodeReportQueryValidator"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public GetBarCodeReportQueryValidator()
    {
        // [Fix]
        // CLAUDE
        // Date: 23/08/2025
        // Reason: Fixed validation chain - Must() was being evaluated on null, added When() condition
        this.RuleFor(v => v.BarCodesIdList)
            .NotNull()
            .WithMessage("BarCodesIdList cannot be null.");
        this.RuleFor(v => v.BarCodesIdList)
            .NotNull()
            .NotEmpty()
            .WithMessage("BarCodesIdList must contain at least one barcode ID.")
            .When(v => v.BarCodesIdList is not null)
            .Must(list => list?.Count() <= 1000);
        this.RuleFor(v => v.BarCodesIdList)
            .NotNull()
            .NotEmpty()
            .When(v => v.BarCodesIdList is not null)
            .WithMessage("BarCodesIdList cannot contain more than 1000 items for performance reasons.")
            .ForEach(id => id
                .GreaterThan(0)
                .WithMessage("Each barcode ID must be greater than 0."));
    }
}