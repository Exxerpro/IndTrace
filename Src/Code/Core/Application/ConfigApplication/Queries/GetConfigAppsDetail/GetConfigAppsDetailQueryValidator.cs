// <copyright file="GetConfigAppsDetailQueryValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.ConfigApplication.Queries.GetConfigAppsDetail;

/// <summary>
/// Represents the GetConfigAppsDetailQueryValidator.
/// </summary>
public class GetConfigAppsDetailQueryValidator : AbstractValidator<GetConfigAppsDetailQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetConfigAppsDetailQueryValidator"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public GetConfigAppsDetailQueryValidator()
    {
        // [Fix]
        // CLAUDE
        // Date: 20/08/2025
        // Reason: Enhanced ID validation with proper error message following railway-oriented pattern
        this.RuleFor(v => v.Id)
            .GreaterThan(0)
            .WithMessage("ConfigApp ID must be greater than 0.");
    }
}