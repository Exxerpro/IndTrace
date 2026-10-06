// <copyright file="UpdateCyclesNotOkCommandValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Cycles.Commands.UpdateCyclesNok;

/// <summary>
/// Represents the UpdateCyclesNotOkCommandValidator.
/// </summary>
public class UpdateCyclesNotOkCommandValidator : AbstractValidator<UpdateCyclesNotOkCommand>
{
    private readonly DateTimeMachine dateTimeMachine;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateCyclesNotOkCommandValidator"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="dateTimeMachine">The dateTimeMachine.</param>
    public UpdateCyclesNotOkCommandValidator(DateTimeMachine? dateTimeMachine = default)
    {
        this.dateTimeMachine = dateTimeMachine ?? new DateTimeMachine();

        // Validate Command property is not null
        this.RuleFor(x => x.Command)
            .NotNull()
            .WithMessage("Command must not be null.");

        // Validate properties of TaskRequestEvent within Command
        this.When(x => x.Command != null, () =>
        {
            this.RuleFor(x => x.Command.MachineId)
                .GreaterThan(0)
                .WithMessage("MachineId must be greater than 0.");

            this.RuleFor(x => x.Command.PartNumber)
                .NotNull()
                .WithMessage("PartNumber cannot be null.")
                .MinimumLength(3)
                .WithMessage("PartNumber must be longer than 2 characters.");

            this.RuleFor(x => x.Command.BarCode)
                .NotNull()
                .WithMessage("BarCode cannot be null.")
                .MinimumLength(3)
                .WithMessage("BarCode must be longer than 2 characters.")
                .Must((command, barCode) => barCode != null && command.Command.PartNumber != null && barCode.Contains(command.Command.PartNumber))
                .WithMessage("BarCode must contain PartNumber.");

            this.RuleFor(x => x.Command.CycleStatus)
                .NotNull()
                .WithMessage("CycleStatus cannot be null.");

            this.When(x => x.Command.CycleStatus != null, () =>
            {
                this.RuleFor(x => x.Command.CycleStatus.Value)
                    .GreaterThan(0)
                    .WithMessage("CycleStatus must be greater than 0.");
            });

            this.RuleFor(x => x.Command.PartStatus)
                .NotNull()
                .WithMessage("PartStatus cannot be null.");

            this.When(x => x.Command.PartStatus != null, () =>
            {
                this.RuleFor(x => x.Command.PartStatus.Value)
                    .GreaterThan(0)
                    .WithMessage("PartStatus must be greater than 0.");
            });

            this.RuleFor(x => x.Command.Registers)
                .NotNull()
                .WithMessage("Registers must not be null.")
                .Must(this.HaveValidRegisters)
                .WithMessage("Registers contains invalid entries.");
        }); // rules when monitorRequest is not null
    }

    private bool HaveValidRegisters(IDictionary<string, Register> registers)
    {
        return registers is not null && registers.All(entry => this.IsValidRegister(entry.Value));
    }

    private bool IsValidRegister(Register register)
    {
        return register != null && this.IsValidDate(register.TimeStamp);
    }

    private bool IsValidDate(DateTime timestamp)
    {
        var minDate = this.dateTimeMachine.Now.ToLocalTime().AddHours(-2);
        var maxDate = this.dateTimeMachine.Now.ToLocalTime().AddHours(2);

        var result = timestamp >= minDate && timestamp <= maxDate;
        return result;
    }
}