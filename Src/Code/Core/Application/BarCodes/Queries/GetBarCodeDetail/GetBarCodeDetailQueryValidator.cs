// <copyright file="GetBarCodeDetailQueryValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Queries.GetBarCodeDetail;

/// <summary>
/// Represents the GetBarCodeDetailQueryValidator.
/// </summary>
public class GetBarCodeDetailQueryValidator : AbstractValidator<GetBarCodeDetailQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetBarCodeDetailQueryValidator"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public GetBarCodeDetailQueryValidator()
    {
        // [Fix]
        // CLAUDE
        // Date: 20/08/2025
        // Reason: Added comprehensive validation - validator was extremely incomplete, only using NotEmpty() but missing proper barcode format validation and length constraints
        this.RuleFor(v => v.BarCode)
            .NotNull()
            .WithMessage("BarCode cannot be null.")
            .NotEmpty()
            .WithMessage("BarCode cannot be empty.")
            .MinimumLength(3)
            .WithMessage("BarCode must be at least 3 characters.")
            .MaximumLength(100)
            .WithMessage("BarCode cannot exceed 100 characters.");
    }
}