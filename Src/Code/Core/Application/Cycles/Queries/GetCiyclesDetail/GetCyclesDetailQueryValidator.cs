// <copyright file="GetCyclesDetailQueryValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Cycles.Queries.GetCiyclesDetail;

/// <summary>
/// Represents the GetCyclesDetailQueryValidator.
/// </summary>
public class GetCyclesDetailQueryValidator : AbstractValidator<GetCyclesDetailQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetCyclesDetailQueryValidator"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public GetCyclesDetailQueryValidator()
    {
        // [Fix]
        // CLAUDE
        // Date: 20/08/2025
        // Reason: Changed from NotEmpty to GreaterThan(0) for proper ID validation including negative values
        this.RuleFor(v => v.Id).GreaterThan(0);
    }
}