// <copyright file="GetVariableListQueryValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Variables.Queries.GetVariableList;

/// <summary>
/// Represents the GetVariableListQueryValidator.
/// </summary>
public class GetVariableListQueryValidator : AbstractValidator<GetVariableListQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetVariableListQueryValidator"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public GetVariableListQueryValidator()
    {
        // [Fix]
        // CLAUDE
        // Date: 20/08/2025
        // Reason: No validation rules needed - GetVariableListQuery has no properties to validate
    }
}