// <copyright file="UpdateMachineValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Machines.Commands.Update;

/// <summary>
/// Represents the UpdateMachineValidator.
/// </summary>
public class UpdateMachineValidator : AbstractValidator<MachineUpdateCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateMachineValidator"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public UpdateMachineValidator()
    {
        // [Fix]
        // CLAUDE
        // Date: 23/08/2025
        // Reason: Fixed validation logic - empty strings should fail validation, not skip it

        // Validate MachineId is required for updates
        this.RuleFor(v => v.MachineId)
            .GreaterThan(0)
            .WithMessage("Machine ID must be greater than 0.");

        // [Fix]
        // CLAUDE
        // Date: 23/08/2025
        // Reason: Pattern A Fix - Fixed validation condition from != null to !string.IsNullOrWhiteSpace() to properly handle string.Empty initialization from MachineUpdateCommand
        // Validate string properties - only when they have meaningful content
        this.RuleFor(v => v.Name)
    .NotEmpty()
    .Length(2, 100)
    .When(v => !string.IsNullOrWhiteSpace(v.Name))
    .WithMessage("Machine name must be between 2 and 100 characters.");

        this.RuleFor(v => v.Location)
    .NotEmpty()
    .Length(1, 150)
    .When(v => !string.IsNullOrWhiteSpace(v.Location))
    .WithMessage("Location must be between 1 and 150 characters.");

        // At least one meaningful field should be provided for update beyond MachineId
        // Check for actual content, not just default empty strings
        this.RuleFor(v => v)
            .Must(v => !string.IsNullOrWhiteSpace(v.Name) || !string.IsNullOrWhiteSpace(v.Location) ||
                      v.MachineType != null || v.WorkFlowType != null)
            .WithMessage("At least one property must be provided for update beyond MachineId.");
    }
}