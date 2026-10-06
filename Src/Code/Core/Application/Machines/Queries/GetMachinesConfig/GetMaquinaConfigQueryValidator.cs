// <copyright file="GetMaquinaConfigQueryValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Machines.Queries.GetMachinesConfig;

/// <summary>
/// Represents the GetMaquinaConfigQueryValidator.
/// </summary>
public class GetMaquinaConfigQueryValidator : AbstractValidator<GetMachineConfigQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetMaquinaConfigQueryValidator"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public GetMaquinaConfigQueryValidator()
    {
        this.RuleFor(v => v)
            .NotNull()
            .WithMessage("Query cannot be null.");

        // [Fix]
        // CLAUDE
        // Date: 20/08/2025
        // Reason: Fixed commented-out rule and added proper validation for PartNumber property
        this.RuleFor(v => v.PartNumber)
            .NotNull()
            .WithMessage("Part number cannot be null.")
            .NotEmpty()
            .WithMessage("Part number cannot be empty.")
            .Length(1, 50)
            .WithMessage("Part number must be between 1 and 50 characters.")
            .Matches(@"^[A-Za-z0-9\-_]+$")
            .WithMessage("Part number can only contain alphanumeric characters, hyphens, and underscores.");
    }
}