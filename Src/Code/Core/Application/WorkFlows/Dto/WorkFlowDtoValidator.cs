// <copyright file="WorkFlowDtoValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.WorkFlows.Dto;

/// <summary>
/// Provides validation rules for <see cref="WorkFlowDto"/> instances.
/// </summary>
public class WorkFlowDtoValidator : AbstractValidator<WorkFlowDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WorkFlowDtoValidator"/> class and sets up validation rules.
    /// </summary>
    public WorkFlowDtoValidator()
    {
        // Validate NextMachineId is greater than 0
        this.RuleFor(v => v.NextMachineId)
            .GreaterThanOrEqualTo(0).WithMessage("NextMachineId must be greater than or equal to 0.");

        // Validate LastMachineId is greater than 0
        this.RuleFor(v => v.LastMachineId)
            .GreaterThanOrEqualTo(0).WithMessage("LastMachineId must be greater than or equal to 0.");

        // Custom validation: At least one of LastMachineId or NextMachineId must be distinct from 0
        this.RuleFor(v => v)
            .Must(dto => dto.LastMachineId != 0 || dto.NextMachineId != 0)
            .WithMessage("At least one of LastMachineId or NextMachineId must be distinct from 0.");
    }
}