// <copyright file="GetVariableDetailQueryValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Variables.Queries.GetVariableDetail;

/// <summary>
/// Represents the GetVariableDetailQueryValidator.
/// </summary>
public class GetVariableDetailQueryValidator : AbstractValidator<GetVariableDetailQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetVariableDetailQueryValidator"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public GetVariableDetailQueryValidator()
    {
        // [Fix]
        // CLAUDE
        // Date: 20/08/2025
        // Reason: Fixed incorrect error message - should be Variable ID not RecipeId
        this.RuleFor(v => v.Id)
            .GreaterThan(0)
            .WithMessage("Variable ID must be greater than 0.");
    }
}