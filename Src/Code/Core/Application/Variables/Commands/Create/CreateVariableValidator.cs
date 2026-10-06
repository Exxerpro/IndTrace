// <copyright file="CreateVariableValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Variables.Commands.Create;

/// <summary>
/// Validator for <see cref="CreateVariableCommand"/> that ensures PLC variable creation requests contain valid data specifications.
/// </summary>
/// <remarks>
/// This validator enforces that PLC variables have a defined length, which is critical for proper memory allocation
/// and data type mapping when establishing communication with Siemens S7 PLCs in the IndTrace system.
/// </remarks>
public class CreateVariableValidator : AbstractValidator<CreateVariableCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateVariableValidator"/> class
    /// and configures validation rules for PLC variable creation.
    /// </summary>
    public CreateVariableValidator()
    {
        // [Fix]
        // CLAUDE
        // Date: 20/08/2025
        // Reason: Added comprehensive validation for all properties - validator was extremely incomplete, only validating Length but missing 11 other properties needed for null safety and railway-oriented programming

        // Validate IDs must be valid positive integers
        this.RuleFor(v => v.VariableId)
            .GreaterThan(0)
            .WithMessage("Variable ID must be greater than 0.");

        this.RuleFor(v => v.MachineId)
            .GreaterThan(0)
            .WithMessage("Machine ID must be greater than 0.");

        this.RuleFor(v => v.VariableGroupId)
            .GreaterThan(0)
            .WithMessage("Variable Group ID must be greater than 0.");

        // Validate string properties for null safety
        this.RuleFor(v => v.Plc)
            .NotNull()
            .NotEmpty()
            .Length(1, 50)
            .WithMessage("PLC name must be between 1 and 50 characters.");

        this.RuleFor(v => v.Name)
            .NotNull()
            .NotEmpty()
            .Length(1, 100)
            .WithMessage("Variable name must be between 1 and 100 characters.");

        this.RuleFor(v => v.Address)
            .NotNull()
            .NotEmpty()
            .Length(1, 50)
            .WithMessage("Address must be between 1 and 50 characters.");

        this.RuleFor(v => v.Type)
            .NotNull()
            .NotEmpty()
            .Length(1, 20)
            .WithMessage("Type must be between 1 and 20 characters.");

        // Validate numeric properties for plausible values
        this.RuleFor(v => v.Length)
            .GreaterThan(0)
            .LessThanOrEqualTo(1000)
            .WithMessage("Length must be between 1 and 1000.");

        this.RuleFor(v => v.Event)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Event must be 0 or greater.");

        this.RuleFor(v => v.Direction)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Direction must be 0 or greater.");
    }
}