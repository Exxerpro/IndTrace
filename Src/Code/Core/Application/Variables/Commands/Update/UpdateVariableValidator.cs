// <copyright file="UpdateVariableValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Variables.Commands.Update;

public class UpdateVariableValidator : AbstractValidator<UpdateVariableCommand>
{
    public UpdateVariableValidator()
    {
        // [Fix]
        // CLAUDE
        // Date: 20/08/2025
        // Reason: Added comprehensive validation for all nullable properties - validator was extremely incomplete, only validating Length but missing 11 other properties. Update commands need proper nullable validation for railway-oriented programming

        // Validate ID properties when provided
        this.When(v => v.VariableId.HasValue, () =>
        {
            this.RuleFor(v => v.VariableId)
                .GreaterThan(0)
                .WithMessage("Variable ID must be greater than 0.");
        });

        this.When(v => v.MachineId.HasValue, () =>
        {
            this.RuleFor(v => v.MachineId)
                .GreaterThan(0)
                .WithMessage("Machine ID must be greater than 0.");
        });

        this.When(v => v.VariableGroupId.HasValue, () =>
        {
            this.RuleFor(v => v.VariableGroupId)
                .GreaterThan(0)
                .WithMessage("Variable Group ID must be greater than 0.");
        });

        // Validate string properties when provided
        this.When(v => v.Plc != null, () =>
        {
            this.RuleFor(v => v.Plc)
                .NotEmpty().WithMessage("PLC name cannot be empty.")
                .Length(1, 50).WithMessage("PLC name must be between 1 and 50 characters.");
        });

        this.When(v => v.Name != null, () =>
        {
            this.RuleFor(v => v.Name)
                .NotEmpty().WithMessage("Variable name cannot be empty.")
                .Length(1, 100).WithMessage("Variable name must be between 1 and 100 characters.");
        });

        this.When(v => v.Address != null, () =>
        {
            this.RuleFor(v => v.Address)
                .NotEmpty().WithMessage("Address cannot be empty.")
                .Length(1, 50).WithMessage("Address must be between 1 and 50 characters.");
        });

        this.When(v => v.Alias != null, () =>
        {
            this.RuleFor(v => v.Alias)
                .NotEmpty().WithMessage("Alias cannot be empty.")
                .Length(1, 50).WithMessage("Alias must be between 1 and 50 characters.");
        });

        this.When(v => v.Type != null, () =>
        {
            this.RuleFor(v => v.Type)
                .NotEmpty().WithMessage("Type cannot be empty.")
                .Length(1, 20).WithMessage("Type must be between 1 and 20 characters.");
        });

        // Validate numeric properties when provided
        this.When(v => v.Length.HasValue, () =>
        {
            this.RuleFor(v => v.Length)
                .GreaterThan(0)
                .LessThanOrEqualTo(1000)
                .WithMessage("Length must be between 1 and 1000.");
        });

        this.When(v => v.Event.HasValue, () =>
        {
            this.RuleFor(v => v.Event)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Event must be 0 or greater.");
        });

        this.When(v => v.Direction.HasValue, () =>
        {
            this.RuleFor(v => v.Direction)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Direction must be 0 or greater.");
        });

        // At least one field should be provided for update
        this.RuleFor(v => v)
            .Must(v => v.VariableId.HasValue || v.MachineId.HasValue || v.Plc != null ||
                      v.Name != null || v.Address != null || v.Type != null ||
                      v.Length.HasValue || v.Event.HasValue || v.Direction.HasValue ||
                      v.VariableGroupId.HasValue)
            .WithMessage("At least one field must be provided for update.");
    }
}