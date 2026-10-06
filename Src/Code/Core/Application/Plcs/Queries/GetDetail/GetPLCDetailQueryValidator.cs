// <copyright file="GetPLCDetailQueryValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Plcs.Queries.GetDetail;

/// <summary>
/// Represents the GetPlcDetailQueryValidator.
/// </summary>
public class GetPlcDetailQueryValidator : AbstractValidator<GetPlcDetailQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlcDetailQueryValidator"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public GetPlcDetailQueryValidator()
    {
        // [Fix]
        // CLAUDE
        // Date: 20/08/2025
        // Reason: Fixed syntax error (extra dot) and wrong error message - said "RecipeId" but should be "PlcId" for PLC detail query
        this.RuleFor(v => v.Id)
            .GreaterThan(0)
            .WithMessage("PLC ID must be greater than 0.");
    }
}