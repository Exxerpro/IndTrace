// <copyright file="GetRegistersListQueryValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Registers.Queries.GetRegisterList;

/// <summary>
/// Represents the GetRegistersListQueryValidator.
/// </summary>
public class GetRegistersListQueryValidator : AbstractValidator<GetRegistersListQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetRegistersListQueryValidator"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="dateTimeMachine">The deterministic time source for the "before the present day" cutoff
    /// (issue #85): evaluated inside the rule at validation time, never frozen at construction.</param>
    public GetRegistersListQueryValidator(IDateTimeMachine dateTimeMachine)
    {
        ArgumentNullException.ThrowIfNull(dateTimeMachine);

        this.RuleFor(x => x.RegistersName)
            .NotNull()
            .WithMessage("RegistersName must not be null.")
            .NotEmpty()
            .WithMessage("RegistersName must not be empty.")
            .When(x => x.VariablesId == null || !x.VariablesId.Any())
            .WithMessage("Either RegistersName or VariablesId must be specified.");

        this.RuleFor(x => x.VariablesId)
            .NotNull()
            .WithMessage("VariablesId must not be null.")
            .NotEmpty()
            .WithMessage("VariablesId must not be empty.")
            .When(x => x.RegistersName == null || !x.RegistersName.Any())
            .WithMessage("Either RegistersName or VariablesId must be specified.");

        this.RuleFor(x => x.MachineId)
            .NotNull()
            .WithMessage("MachineId must not be null.")
            .NotEmpty()
            .WithMessage("At least one MachineId must be specified.");

        this.RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.EndDate)
            .WithMessage("StartDate must be earlier than or equal to EndDate.");

        // [NEW RULE] EndDate must be before today
        this.RuleFor(x => x.EndDate)
            .Must(date => date < dateTimeMachine.Now)
            .WithMessage("EndDate must be before the present day.");
    }
}