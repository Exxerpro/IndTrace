// <copyright file="CreateCyclesCommandValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Cycles.Commands.Create;

/// <summary>
/// Validates the <see cref="CreateCyclesCommand"/> to ensure all required data is present and valid.
/// </summary>
/// <remarks>
/// This validator enforces business rules for cycle creation including machine validation,
/// part number requirements, barcode integrity, and status validation.
/// </remarks>
public class CreateCyclesCommandValidator : AbstractValidator<CreateCyclesCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateCyclesCommandValidator"/> class.
    /// </summary>
    /// <remarks>
    /// Configures validation rules for the command including:
    /// - Command object null checks
    /// - Machine ID validation (must be greater than 0)
    /// - Part number validation (minimum length and format)
    /// - Barcode validation and part number containment
    /// - Cycle and part status validation.
    /// </remarks>
    public CreateCyclesCommandValidator()
    {
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
        });
    }
}