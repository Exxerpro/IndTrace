// <copyright file="UpdateSettingValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Settings.Commands.Update;

/// <summary>
/// Represents the UpdateSettingValidator.
/// </summary>
public class UpdateSettingValidator : AbstractValidator<UpdateSettingCommand>
{
    public UpdateSettingValidator()
    {
        // [Fix]
        // CLAUDE
        // Date: 20/08/2025
        // Reason: Added comprehensive validation for all nullable properties - validator was extremely incomplete, only validating SettingId with NotEmpty() but missing 2 other properties. Changed to proper nullable validation for railway-oriented programming

        // Validate nullable ID properties when provided
        this.When(v => v.SettingId.HasValue, () =>
        {
            this.RuleFor(v => v.SettingId)
                .GreaterThan(0)
                .WithMessage("Setting ID must be greater than 0.");
        });

        this.When(v => v.MachineId.HasValue, () =>
        {
            this.RuleFor(v => v.MachineId)
                .GreaterThan(0)
                .WithMessage("Machine ID must be greater than 0.");
        });

        // Validate Config string when provided (optional for updates)
        this.When(v => !string.IsNullOrEmpty(v.Config), () =>
        {
            this.RuleFor(v => v.Config)
                .Length(1, 1000)
                .WithMessage("Config must be between 1 and 1000 characters when provided.");
        });

        // At least one field should be provided for update
        this.RuleFor(v => v)
            .Must(v => v.SettingId.HasValue || v.MachineId.HasValue || !string.IsNullOrEmpty(v.Config))
            .WithMessage("At least one field must be provided for update.");
    }
}