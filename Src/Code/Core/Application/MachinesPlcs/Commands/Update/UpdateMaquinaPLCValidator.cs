// <copyright file="UpdateMaquinaPLCValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.MachinesPlcs.Commands.Update;

/// <summary>
/// Represents the UpdateMaquinaPlcValidator.
/// </summary>
public class UpdateMaquinaPlcValidator : AbstractValidator<UpdateMachinePlcCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateMaquinaPlcValidator"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public UpdateMaquinaPlcValidator()
    {
        // [Fix]
        // CLAUDE
        // Date: 20/08/2025
        // Reason: Added comprehensive validation for all properties - validator was extremely incomplete, only validating PlcId with NotEmpty() but missing 4 other properties. Changed to GreaterThan(0) for proper null safety and railway-oriented programming

        // Validate required IDs
        this.RuleFor(v => v.MachineId)
            .GreaterThan(0)
            .WithMessage("Machine ID must be greater than 0.");

        this.RuleFor(v => v.PlcId)
            .GreaterThan(0)
            .WithMessage("PLC ID must be greater than 0.");

        // Validate optional nullable properties when provided
        this.When(v => v.IsActive.HasValue, () =>
        {
            this.RuleFor(v => v.IsActive)
                .Must(value => value == 0 || value == 1)
                .WithMessage("IsActive must be 0 (inactive) or 1 (active).");
        });

        this.When(v => v.NewMachineId.HasValue, () =>
        {
            this.RuleFor(v => v.NewMachineId)
                .GreaterThan(0)
                .WithMessage("New Machine ID must be greater than 0.");
        });

        this.When(v => v.NewPlcId.HasValue, () =>
        {
            this.RuleFor(v => v.NewPlcId)
                .GreaterThan(0)
                .WithMessage("New PLC ID must be greater than 0.");
        });

        // Business rule: If updating machine-PLC association, new IDs should be different
        this.When(v => v.NewMachineId.HasValue && v.NewPlcId.HasValue, () =>
        {
            this.RuleFor(v => v)
                .Must(v => v.NewMachineId != v.MachineId || v.NewPlcId != v.PlcId)
                .WithMessage("At least one ID must be different when updating machine-PLC association.")
                .WithName("AssociationChange");
        });
    }
}