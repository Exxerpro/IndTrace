// <copyright file="CreateSettingValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Settings.Commands.Create;

/// <summary>
/// Validator for <see cref="CreateSettingCommand"/> that ensures required setting data is provided.
/// </summary>
public class CreateSettingValidator : AbstractValidator<CreateSettingCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateSettingValidator"/> class
    /// and configures validation rules for setting creation.
    /// </summary>
    public CreateSettingValidator()
    {
        // [Fix]
        // CLAUDE
        // Date: 20/08/2025
        // Reason: Added comprehensive validation for all properties - validator was incomplete, only validating SettingId with NotEmpty() but missing MachineId and Setting string property. Changed to GreaterThan(0) for proper null safety and railway-oriented programming

        // Validate IDs must be valid positive integers
        this.RuleFor(v => v.SettingId)
            .GreaterThan(0)
            .WithMessage("Setting ID must be greater than 0.");

        this.RuleFor(v => v.MachineId)
            .GreaterThan(0)
            .WithMessage("Machine ID must be greater than 0.");

        // Validate string properties for null safety
        this.RuleFor(v => v.Setting)
            .NotNull()
            .NotEmpty()
            .Length(1, 500)
            .WithMessage("Setting must be between 1 and 500 characters.");
    }
}