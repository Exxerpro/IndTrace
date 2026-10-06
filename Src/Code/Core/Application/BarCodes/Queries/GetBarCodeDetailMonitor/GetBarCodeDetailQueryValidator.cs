// <copyright file="GetBarCodeDetailQueryValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Queries.GetBarCodeDetailMonitor;

/// <summary>
/// Represents the GetBarCodeDetailQueryValidator.
/// </summary>
public class GetBarCodeDetailQueryValidator : AbstractValidator<GetBarCodeDetailMonitorQuery>
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
        // Reason: Fixed incomplete validation - properly validate that either BarCode or BarCodeId is provided

        // At least one identifier must be provided
        this.RuleFor(v => v)
            .Must(v => !string.IsNullOrWhiteSpace(v.BarCode) || v.BarCodeId > 0)
            .WithMessage("Either BarCode or BarCodeId must be provided.");

        // If BarCode is provided, validate its format
        this.When(v => !string.IsNullOrWhiteSpace(v.BarCode), () =>
        {
            this.RuleFor(v => v.BarCode)
                .Length(1, 100)
                .WithMessage("BarCode must be between 1 and 100 characters when provided.")
                .Matches(@"^[A-Za-z0-9\-_/]+$")
                .WithMessage("BarCode can only contain alphanumeric characters, hyphens, underscores, and forward slashes.");
        });

        // If BarCodeId is provided, ensure it's valid
        this.When(v => v.BarCodeId > 0, () =>
        {
            this.RuleFor(v => v.BarCodeId)
                .GreaterThan(0)
                .WithMessage("BarCodeId must be greater than 0 when provided.");
        });
    }
}