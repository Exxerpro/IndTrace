// <copyright file="UpdateWorkFlowValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.WorkFlows.Commands.Update;

/// <summary>
/// Provides validation rules for <see cref="UpdateWorkFlowCommand"/> instances.
/// </summary>
public class UpdateWorkFlowValidator : AbstractValidator<UpdateWorkFlowCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateWorkFlowValidator"/> class and sets up validation rules.
    /// </summary>
    public UpdateWorkFlowValidator()
    {
        // [Fix]
        // CLAUDE
        // Date: 22/08/2025
        // Reason: [CLUSTER WORKFLOW NEGATIVE FIX] - Fixed error messages to reflect that zero (0) is invalid, but negative values are allowed for end-of-line workflows

        // Validate nullable ID properties when provided
        this.When(v => v.WorkFlowId.HasValue, () =>
        {
            this.RuleFor(v => v.WorkFlowId)
                .GreaterThan(0)
                .WithMessage("WorkFlow ID must be greater than 0.");
        });

        this.When(v => v.ProductId.HasValue, () =>
        {
            this.RuleFor(v => v.ProductId)
                .GreaterThan(0)
                .WithMessage("Product ID must be greater than 0.");
        });

        // Machine IDs can be negative for end-of-line workflow scenarios
        this.When(v => v.NextMachineId.HasValue, () =>
        {
            this.RuleFor(v => v.NextMachineId)
                .NotEqual(0)
                .WithMessage("Next Machine ID cannot be zero (uninitialized). Use positive IDs for normal machines or negative IDs for end-of-line workflows.");
        });

        this.When(v => v.LastMachineId.HasValue, () =>
        {
            this.RuleFor(v => v.LastMachineId)
                .NotEqual(0)
                .WithMessage("Last Machine ID cannot be zero (uninitialized). Use positive IDs for normal machines or negative IDs for end-of-line workflows.");
        });

        // Business rule: Next machine must be different from last machine when both provided
        this.When(v => v.NextMachineId.HasValue && v.LastMachineId.HasValue, () =>
        {
            this.RuleFor(v => v)
                .Must(v => v.NextMachineId != v.LastMachineId)
                .WithMessage("Next Machine ID must be different from Last Machine ID.")
                .WithName("MachineSequence");
        });

        // At least one field should be provided for update
        this.RuleFor(v => v)
            .Must(v => v.WorkFlowId.HasValue || v.ProductId.HasValue ||
                      v.NextMachineId.HasValue || v.LastMachineId.HasValue)
            .WithMessage("At least one field must be provided for update.");
    }
}