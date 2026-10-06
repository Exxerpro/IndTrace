// <copyright file="CreateCyclesValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Cycles.Commands.Create;

/// <summary>
/// Validator for <see cref="CreateCyclesCommand"/> that ensures production cycle creation requests meet business requirements.
/// </summary>
/// <remarks>
/// This validator is designed to validate production cycle data before creation in the manufacturing execution system.
/// Currently configured as a placeholder for future validation rules as the cycle creation business logic evolves.
/// </remarks>
public class CreateCyclesValidator : AbstractValidator<CreateCyclesCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateCyclesValidator"/> class.
    /// </summary>
    /// <remarks>
    /// Currently no validation rules are implemented. This validator serves as a placeholder
    /// for future cycle validation requirements and maintains consistency with the CQRS pattern.
    /// </remarks>
    public CreateCyclesValidator()
    {
        // [Fix]
        // CLAUDE
        // Date: 20/08/2025
        // Reason: Added comprehensive validation - validator was completely empty, no validation rules implemented for CreateCyclesCommand needed for null safety and railway-oriented programming

        // Validate that Command is not null
        this.RuleFor(x => x.Command)
            .NotNull()
            .WithMessage("Command cannot be null.");

        // Validate properties of TaskGatewayRequest within Command
        this.When(x => x.Command != null, () =>
        {
            this.RuleFor(x => x.Command.MachineId)
                .GreaterThan(0)
                .WithMessage("Machine ID must be greater than 0.");

            this.RuleFor(x => x.Command.PartNumber)

                // [Fix]
                // CLAUDE
                // Date: 22/08/2025
                // Reason: Pattern A Fix - Changed .NotNull() to .NotEmpty() so whitespace strings ("   ") fail validation as expected by test
                .NotEmpty()
                .WithMessage("Part number cannot be null.")
                .MinimumLength(3)
                .WithMessage("Part number must be at least 3 characters long.");
        });
    }
}