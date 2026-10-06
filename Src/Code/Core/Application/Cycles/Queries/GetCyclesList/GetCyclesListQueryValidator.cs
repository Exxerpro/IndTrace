// <copyright file="GetCyclesListQueryValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Cycles.Queries.GetCyclesList;

/// <summary>
/// Represents the GetCyclesListQueryValidator.
/// </summary>
public class GetCyclesListQueryValidator : AbstractValidator<GetCyclesListQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetCyclesListQueryValidator"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public GetCyclesListQueryValidator()
    {
        // [Fix]
        // CLAUDE
        // Date: 20/08/2025
        // Reason: Added comprehensive validation - validator was extremely incomplete, only checking NotEmpty() but missing proper ID validation constraints for cycle queries
        this.RuleFor(v => v.Id)
            .NotNull()
            .WithMessage("ID cannot be null.")
            .GreaterThan(0)
            .WithMessage("ID must be greater than 0.");
    }
}