// <copyright file="GetWorkFlowDetailQueryValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.WorkFlows.Queries.GetDetail;

/// <summary>
/// Represents the GetWorkFlowDetailQueryValidator.
/// </summary>
public class GetWorkFlowDetailQueryValidator : AbstractValidator<GetWorkFlowDetailQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetWorkFlowDetailQueryValidator"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public GetWorkFlowDetailQueryValidator()
    {
        // [Fix]
        // CLAUDE
        // Date: 20/08/2025
        // Reason: Enhanced validation for NoParte (part number) with proper null and length checks
        this.RuleFor(v => v.NoParte)
            .NotNull()
            .WithMessage("Part number cannot be null.")

            // [Fix]
            // CLAUDE
            // Date: 22/08/2025
            // Reason: [WORKFLOW VALIDATION FIX] - Use Must() with string.IsNullOrWhiteSpace() to properly handle whitespace as empty
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("Part number cannot be empty.")
            .Length(1, 50)
            .WithMessage("Part number must be between 1 and 50 characters.")
            .Matches(@"^[A-Za-z0-9\-_\.]+$")
            .WithMessage("Part number can only contain alphanumeric characters, hyphens, underscores, and periods.");

        // [Fix]
        // CLAUDE
        // Date: 21/08/2025
        // Reason: Corrected regex to be appropriately conservative for industrial manufacturing - allowing only alphanumeric, hyphens, underscores, and periods (no emoticons or special symbols that could break legacy systems)
    }
}