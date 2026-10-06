// <copyright file="UpdateConfigAppValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.ConfigApplication.Commands.Update;

/// <summary>
/// Validator for UpdateConfigAppCommand that ensures application configuration update data meets system requirements.
/// </summary>
public class UpdateConfigAppValidator : AbstractValidator<UpdateConfigAppCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateConfigAppValidator"/> class.
    /// </summary>
    public UpdateConfigAppValidator()
    {
        // [Fix]
        // CLAUDE
        // Date: 20/08/2025
        // Reason: Added comprehensive validation for all properties - validator was extremely incomplete, only validating AppId with NotEmpty() but missing 9 other properties needed for null safety and railway-oriented programming

        // Validate string identifiers - ConfigAppId is required for updates
        this.RuleFor(v => v.ConfigAppId)
            .NotEmpty()
            .Length(10)
            .WithMessage("Config App ID must be exactly 10 characters (legacy system requirement).");

        // Validate nullable ID properties when provided
        this.When(v => v.AppId.HasValue, () =>
        {
            this.RuleFor(v => v.AppId)
                .GreaterThan(0)
                .WithMessage("App ID must be greater than 0.");
        });

        // Validate required MachineId
        this.RuleFor(v => v.MachineId)
            .GreaterThan(0)
            .WithMessage("Machine ID must be greater than 0.");

        // Validate string properties for null safety
        this.RuleFor(v => v.Client)
            .NotNull()
            .NotEmpty()
            .Length(1, 100)
            .WithMessage("Client must be between 1 and 100 characters.");

        this.RuleFor(v => v.Factory)
            .NotNull()
            .NotEmpty()
            .Length(1, 100)
            .WithMessage("Factory must be between 1 and 100 characters.");

        this.RuleFor(v => v.Line)
            .NotNull()
            .NotEmpty()
            .Length(1, 100)
            .WithMessage("Line must be between 1 and 100 characters.");

        this.RuleFor(v => v.Project)
            .NotNull()
            .NotEmpty()
            .Length(1, 200)
            .WithMessage("Project must be between 1 and 200 characters.");

        this.RuleFor(v => v.Version)
            .NotNull()
            .NotEmpty()
            .Length(1, 50)
            .WithMessage("Version must be between 1 and 50 characters.");

        // Validate date properties
        this.RuleFor(v => v.CreatedOn)
            .NotEmpty()
            .Must(date => date != default(DateTime))
            .WithMessage("Created On date must be a valid date.");

        this.RuleFor(v => v.ModifiedOn)
            .NotEmpty()
            .Must(date => date != default(DateTime))
            .WithMessage("Modified On date must be a valid date.")
            .GreaterThanOrEqualTo(v => v.CreatedOn)
            .WithMessage("Modified date cannot be before created date.");
    }
}