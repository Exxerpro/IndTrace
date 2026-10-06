// <copyright file="CreateBarCodeCommandValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Commands.Create;

/// <summary>
/// Provides validation rules for the <see cref="CreateBarCodeCommand"/> class.
/// </summary>
/// <remarks>
/// This validator ensures that the command contains valid data before processing,
/// including proper machine identification and part number formatting.
/// </remarks>
public class CreateBarCodeCommandValidator : AbstractValidator<CreateBarCodeCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateBarCodeCommandValidator"/> class.
    /// </summary>
    /// <remarks>
    /// Sets up validation rules for the command, including null checks and business rules
    /// for machine ID and part number validation.
    /// </remarks>
    public CreateBarCodeCommandValidator()
    {
        // [Fix]
        // CLAUDE
        // Date: 20/08/2025
        // Reason: Added comprehensive validation - validator was extremely incomplete, only validating MachineId and PartNumber but missing BarCode and other critical properties needed for barcode creation

        // Validate that Command is not null
        this.RuleFor(x => x.Command)
            .NotNull()
            .WithMessage("Command cannot be null.");

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
                .WithMessage("PartNumber must be longer than 2 characters.")
                .MaximumLength(50)
                .WithMessage("PartNumber cannot exceed 50 characters.");

            this.RuleFor(x => x.Command.BarCode)
                .NotNull()
                .WithMessage("BarCode cannot be null.")
                .MinimumLength(3)
                .WithMessage("BarCode must be longer than 2 characters.")
                .MaximumLength(100)
                .WithMessage("BarCode cannot exceed 100 characters.")
                .Must((command, barCode) => barCode != null && command.Command.PartNumber != null && barCode.Contains(command.Command.PartNumber))
                .WithMessage("BarCode must contain PartNumber.");
        });
    }
}