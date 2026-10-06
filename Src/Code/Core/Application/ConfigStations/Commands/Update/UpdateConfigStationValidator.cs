// <copyright file="UpdateConfigStationValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.ConfigStations.Commands.Update;

/// <summary>
/// Represents the UpdateConfigStationValidator.
/// </summary>
public class UpdateConfigStationValidator : AbstractValidator<UpdateConfigStationCommand>
{
    public UpdateConfigStationValidator()
    {
        // [Fix]
        // CLAUDE
        // Date: 20/08/2025
        // Reason: Added comprehensive validation for all properties - validator was extremely incomplete, only validating MachineId but missing 9 other properties needed for null safety and railway-oriented programming

        // Validate MachineId when greater than 0 (required for meaningful updates)
        this.RuleFor(v => v.MachineId)
            .GreaterThan(0)
            .WithMessage("Machine ID must be greater than 0.");

        // Validate string properties when provided (empty string means "don't update")
        this.When(v => !string.IsNullOrEmpty(v.ConfigId), () =>
        {
            this.RuleFor(v => v.ConfigId)
                .Length(1, 50)
                .WithMessage("Config ID must be between 1 and 50 characters.");
        });

        this.When(v => !string.IsNullOrEmpty(v.Client), () =>
        {
            this.RuleFor(v => v.Client)
                .Length(1, 100)
                .WithMessage("Client must be between 1 and 100 characters.");
        });

        this.When(v => !string.IsNullOrEmpty(v.Factorie), () =>
        {
            this.RuleFor(v => v.Factorie)
                .Length(1, 100)
                .WithMessage("Factory must be between 1 and 100 characters.");
        });

        this.When(v => !string.IsNullOrEmpty(v.Line), () =>
        {
            this.RuleFor(v => v.Line)
                .Length(1, 100)
                .WithMessage("Line must be between 1 and 100 characters.");
        });

        this.When(v => !string.IsNullOrEmpty(v.Machine), () =>
        {
            this.RuleFor(v => v.Machine)
                .Length(1, 100)
                .WithMessage("Machine must be between 1 and 100 characters.");
        });

        this.When(v => !string.IsNullOrEmpty(v.Project), () =>
        {
            this.RuleFor(v => v.Project)
                .Length(1, 200)
                .WithMessage("Project must be between 1 and 200 characters.");
        });

        this.When(v => !string.IsNullOrEmpty(v.Version), () =>
        {
            this.RuleFor(v => v.Version)
                .Length(1, 50)
                .WithMessage("Version must be between 1 and 50 characters.");
        });

        // Validate date properties when not default
        this.When(v => v.VersionDate != default(DateTime), () =>
        {
            this.RuleFor(v => v.VersionDate)
                .Must(date => date > DateTime.MinValue)
                .WithMessage("Version date must be a valid date.");
        });

        this.When(v => v.ModifiedDate != default(DateTime), () =>
        {
            this.RuleFor(v => v.ModifiedDate)
                .Must(date => date > DateTime.MinValue)
                .WithMessage("Modified date must be a valid date.");

            // Cross-validation only when both dates are provided
            this.When(v => v.VersionDate != default(DateTime), () =>
            {
                this.RuleFor(v => v.ModifiedDate)
                    .GreaterThanOrEqualTo(v => v.VersionDate)
                    .WithMessage("Modified date cannot be before version date.");
            });
        });

        // At least one meaningful field should be provided for update
        this.RuleFor(v => v)
            .Must(v => !string.IsNullOrEmpty(v.ConfigId) || !string.IsNullOrEmpty(v.Client) ||
                      !string.IsNullOrEmpty(v.Factorie) || !string.IsNullOrEmpty(v.Line) ||
                      !string.IsNullOrEmpty(v.Machine) || !string.IsNullOrEmpty(v.Project) ||
                      !string.IsNullOrEmpty(v.Version) || v.VersionDate != default(DateTime) ||
                      v.ModifiedDate != default(DateTime))
            .WithMessage("At least one field must be provided for update.");
    }
}