// <copyright file="CreateConfigAppValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.ConfigApplication.Commands.Create;

/// <summary>
/// Validator for <see cref="CreateConfigAppCommand"/> that ensures application configuration data meets system requirements.
/// </summary>
/// <remarks>
/// This validator enforces the business rule that configuration IDs must be exactly 10 characters in length,
/// which is required for proper integration with legacy systems and database constraints in the IndTrace platform.
/// </remarks>
public class CreateConfigAppValidator : AbstractValidator<CreateConfigAppCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateConfigAppValidator"/> class
    /// and configures validation rules for application configuration creation.
    /// </summary>
    public CreateConfigAppValidator()
    {
        // [Fix]
        // CLAUDE
        // Date: 20/08/2025
        // Reason: Added comprehensive validation for all properties - validator was extremely incomplete, only validating ConfigId but missing 9 other properties needed for null safety and railway-oriented programming

        // Validate string identifiers
        this.RuleFor(v => v.ConfigId)
            .NotEmpty()
            .Length(10)
            .WithMessage("Config ID must be exactly 10 characters (legacy system requirement).");

        // Validate IDs must be valid positive integers
        this.RuleFor(v => v.AppId)
            .GreaterThan(0)
            .WithMessage("App ID must be greater than 0.");

        this.RuleFor(v => v.MachineId)
            .GreaterThan(0)
            .WithMessage("Machine ID must be greater than 0.");

        // Validate string properties for null safety
        this.RuleFor(v => v.Client)
            .NotNull()
            .NotEmpty()
            .Length(1, 100)
            .WithMessage("Client must be between 1 and 100 characters.");

        this.RuleFor(v => v.Factorie)
            .NotNull()
            .NotEmpty()
            .Length(1, 100)
            .WithMessage("Plant must be between 1 and 100 characters.");

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
        this.RuleFor(v => v.VersionDate)
            .NotEmpty()
            .Must(date => date != default(DateTime))
            .WithMessage("Version date must be a valid date.");

        this.RuleFor(v => v.ModifiedDate)
            .NotEmpty()
            .Must(date => date != default(DateTime))
            .WithMessage("Modified date must be a valid date.")
            .GreaterThanOrEqualTo(v => v.VersionDate)
            .WithMessage("Modified date cannot be before version date.");
    }
}