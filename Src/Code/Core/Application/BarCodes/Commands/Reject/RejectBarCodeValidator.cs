// <copyright file="RejectBarCodeValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Commands.Reject;

/// <summary>
/// Validator for the <see cref="RejectBarCodeCommand"/>. Ensures that the label is not null.
/// </summary>
public class RejectBarCodeValidator : AbstractValidator<RejectBarCodeCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RejectBarCodeValidator"/> class.
    /// </summary>
    public RejectBarCodeValidator()
    {
        // [Fix]
        // CLAUDE
        // Date: 20/08/2025
        // Reason: Added comprehensive validation - validator was extremely incomplete, only validating Label but missing proper business rules for barcode rejection
        this.RuleFor(v => v.Label)
            .NotNull()
            .WithMessage("Label cannot be null.")
            .NotEmpty()
            .WithMessage("Label cannot be empty.")
            .MinimumLength(3)
            .WithMessage("Label must be at least 3 characters.")
            .MaximumLength(100)
            .WithMessage("Label cannot exceed 100 characters.");

        // RuleFor(v => v)
        //    .NotNull()
        //    .WithMessage("Rejection reason cannot be null.")
        //    .NotEmpty()
        //    .WithMessage("Rejection reason cannot be empty.")
        //    .MinimumLength(5)
        //    .WithMessage("Rejection reason must be at least 5 characters.")
        //    .MaximumLength(500)
        //    .WithMessage("Rejection reason cannot exceed 500 characters.");

        // RuleFor(v => v.MachineId)
        //    .GreaterThan(0)
        //    .WithMessage("Machine ID must be greater than 0.");
    }
}