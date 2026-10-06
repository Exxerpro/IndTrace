// <copyright file="BarCodeDetailsValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Services;

/// <summary>
/// Represents the BarCodeDetailsValidator.
/// </summary>
public class BarCodeDetailsValidator : AbstractValidator<BarCodeDetailsRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BarCodeDetailsValidator"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public BarCodeDetailsValidator()
    {
        // [Fix]
        // CLAUDE
        // Date: 20/08/2025
        // Reason: Improved validation consistency - aligned with domain Build method validation and proper ID validation pattern

        // Validate MachineId must be valid positive integer
        this.RuleFor(x => x.MachineId)
            .GreaterThan(0)
            .WithMessage("Machine ID must be greater than 0.");

        // Validate string properties for null safety
        this.RuleFor(x => x.Label)
            .NotNull()
            .NotEmpty()
            .WithMessage("Label must be provided.");

        this.RuleFor(x => x.PartNumber)
            .NotNull()
            .NotEmpty()
            .WithMessage("Part number must be provided.");

        // Business rule: Label must contain the part number
        // [Fix]
        // CLAUDE
        // Date: 21/08/2025
        // Reason: Fixed business rule validation to target Label property for test compatibility - changed from RuleFor(x => x) to RuleFor(x => x.Label) with custom Must condition
        this.RuleFor(x => x.Label)
            .Must((request, label) => !string.IsNullOrEmpty(label) &&
                                    !string.IsNullOrEmpty(request.PartNumber) &&
                                    label.Contains(request.PartNumber, StringComparison.OrdinalIgnoreCase))
            .WithMessage("Label must contain the part number.")
            .When(x => !string.IsNullOrEmpty(x.Label) && !string.IsNullOrEmpty(x.PartNumber));
    }
}