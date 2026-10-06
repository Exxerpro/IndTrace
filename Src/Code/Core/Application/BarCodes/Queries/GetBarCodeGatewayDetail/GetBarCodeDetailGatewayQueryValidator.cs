// <copyright file="GetBarCodeDetailGatewayQueryValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Queries.GetBarCodeGatewayDetail;

/// <summary>
/// Represents the GetBarCodeDetailGatewayQueryValidator.
/// </summary>
public class GetBarCodeDetailGatewayQueryValidator : AbstractValidator<ReadBarCodeQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetBarCodeDetailGatewayQueryValidator"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public GetBarCodeDetailGatewayQueryValidator()
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

                // [Fix]
                // CLAUDE
                // Date: 22/08/2025
                // Reason: Pattern A Fix - Changed .NotNull() to .NotEmpty() so empty string ("") triggers "cannot be null" message as expected by test
                .NotEmpty()
                .WithMessage("PartNumber cannot be null.")
                .MinimumLength(3)
                .WithMessage("PartNumber must be longer than 2 characters.");

            this.RuleFor(x => x.Command.BarCode)

                // [Fix]
                // CLAUDE
                // Date: 22/08/2025
                // Reason: Pattern A Fix - Changed .NotNull() to .NotEmpty() so empty string ("") triggers "cannot be null" message as expected by test
                .NotEmpty()
                .WithMessage("BarCode cannot be null.")
                .MinimumLength(3)
                .WithMessage("BarCode must be longer than 2 characters.")
                .Must((command, barCode) => barCode != null && command.Command.PartNumber != null && barCode.Contains(command.Command.PartNumber))
                .WithMessage("BarCode must contain PartNumber.");
        });
    }
}