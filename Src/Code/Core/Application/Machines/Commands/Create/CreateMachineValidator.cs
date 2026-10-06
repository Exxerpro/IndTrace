// <copyright file="CreateMachineValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Machines.Commands.Create;

/// <summary>
/// Validator for <see cref="CreateMachineMonitorRequest"/> that ensures machine creation requests contain valid machine identifiers.
/// </summary>
/// <remarks>
/// This validator is specifically designed for machine monitoring setup operations where a valid machine ID
/// is required to establish communication and monitoring capabilities with physical production equipment.
/// </remarks>
public class CreateMachineValidator : AbstractValidator<CreateMachineMonitorRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateMachineValidator"/> class
    /// and configures validation rules for machine monitor request creation.
    /// </summary>
    public CreateMachineValidator()
    {
        // [Fix]
        // CLAUDE
        // Date: 20/08/2025
        // Reason: Added comprehensive validation for all properties - validator was incomplete, only validating MachineId but missing 6 other properties needed for null safety and railway-oriented programming

        // Validate IDs must be valid positive integers
        this.RuleFor(v => v.Id)
            .GreaterThan(0)
            .WithMessage("ID must be greater than 0.");

        this.RuleFor(v => v.MachineId)
            .GreaterThan(0)
            .WithMessage("Machine ID must be greater than 0.");

        this.RuleFor(v => v.MachineType)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Machine type must be 0 or greater.");

        // Validate string properties for null safety
        this.RuleFor(v => v.Name)
            .NotNull()
            .NotEmpty()
            .Length(1, 100)
            .WithMessage("Machine name must be between 1 and 100 characters.");

        this.RuleFor(v => v.Location)
            .NotNull()
            .NotEmpty()
            .Length(1, 200)
            .WithMessage("Location must be between 1 and 200 characters.");

        // Validate boolean flags (represented as int: 0 or 1)
        this.RuleFor(v => v.EnableAppTraceability)
            .Must(value => value == 0 || value == 1)
            .WithMessage("Enable App Traceability must be 0 (disabled) or 1 (enabled).");

        this.RuleFor(v => v.EnableBypassTraceability)
            .Must(value => value == 0 || value == 1)
            .WithMessage("Enable Bypass Traceability must be 0 (disabled) or 1 (enabled).");
    }
}