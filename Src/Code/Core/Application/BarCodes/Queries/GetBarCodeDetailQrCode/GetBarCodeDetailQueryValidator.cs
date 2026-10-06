// <copyright file="GetBarCodeDetailQueryValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Queries.GetBarCodeDetailQrCode;

/// <summary>
/// Represents the GetBarCodeDetailQueryValidator.
/// </summary>
public class GetBarCodeDetailQueryValidator : AbstractValidator<GetBarCodeDetailQrCodeQuery>
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
        // Reason: Fixed wrong query type and added comprehensive validation for QR code barcode queries
        this.RuleFor(v => v.BarCode)
            .NotNull()
            .WithMessage("BarCode cannot be null.")
            .NotEmpty()
            .WithMessage("BarCode cannot be empty.")
            .Length(1, 200)
            .WithMessage("BarCode must be between 1 and 200 characters.")

            // [Fix]
            // CLAUDE
            // Date: 22/08/2025
            // Reason: Pattern A Fix - Added dots (.) and colon (:) properly for URL support like "https://example.com/barcode?id=123&type=QR"
            .Matches(@"^[A-Za-z0-9\-_/:=?&\.]+$")
            .WithMessage("BarCode contains invalid characters for QR code format.");
    }
}