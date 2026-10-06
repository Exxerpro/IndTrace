// <copyright file="CreateWorkFlowValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.WorkFlows.Commands.Create;

/// <summary>
/// Represents the CreateWorkFlowValidator.
/// </summary>
public class CreateWorkFlowValidator : AbstractValidator<CreateWorkFlowCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateWorkFlowValidator"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public CreateWorkFlowValidator()
    {
        // [Fix]
        // CLAUDE
        // Date: 20/08/2025
        // Reason: Added comprehensive validation for all properties - validator was extremely incomplete, only validating LastMachineId with NotEmpty() but missing 3 other properties. Changed to GreaterThan(0) for proper null safety and railway-oriented programming

        // Validate all IDs must be valid positive integers
        this.RuleFor(v => v.WorkFlowId)
            .GreaterThan(0)
            .WithMessage("WorkFlow ID must be greater than 0.");

        this.RuleFor(v => v.ProductId)
            .GreaterThan(0)
            .WithMessage("Product ID must be greater than 0.");

        this.RuleFor(v => v.LastMachineId)
            .GreaterThan(0)
            .WithMessage("Last Machine ID must be greater than 0.");

        this.RuleFor(v => v.NextMachineId)
            .GreaterThan(0)
            .WithMessage("Next Machine ID must be greater than 0.");

        // Business rule: Next machine must be different from last machine
        this.RuleFor(v => v)
            .Must(v => v.NextMachineId != v.LastMachineId)
            .WithMessage("Next Machine ID must be different from Last Machine ID.")
            .WithName("MachineSequence");
    }
}