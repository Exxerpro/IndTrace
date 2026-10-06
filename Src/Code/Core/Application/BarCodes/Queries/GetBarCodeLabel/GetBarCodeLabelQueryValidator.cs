// <copyright file="GetBarCodeLabelQueryValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Queries.GetBarCodeLabel;

/// <summary>
/// Represents the GetBarCodeLabelQueryValidator.
/// </summary>
public class GetBarCodeLabelQueryValidator : AbstractValidator<GetBarCodesLabelQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetBarCodeLabelQueryValidator"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public GetBarCodeLabelQueryValidator()
    {
        // [Fix]
        // CLAUDE
        // Date: 20/08/2025
        // Reason: Added comprehensive validation rules for Label property following railway-oriented programming pattern
        this.RuleFor(v => v.Label)
            .NotNull()
            .WithMessage("Label cannot be null.")
            .NotEmpty()
            .WithMessage("Label cannot be empty.")
            .Length(1, 50)
            .WithMessage("Label must be between 1 and 50 characters.")
            .Matches(@"^[A-Za-z0-9\-_]+$")
            .WithMessage("Label can only contain alphanumeric characters, hyphens, and underscores.");
    }
}