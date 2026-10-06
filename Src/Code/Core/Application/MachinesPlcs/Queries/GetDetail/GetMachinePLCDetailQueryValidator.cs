// <copyright file="GetMachinePLCDetailQueryValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.MachinesPlcs.Queries.GetDetail;

/// <summary>
/// Represents the GetMachinePlcDetailQueryValidator.
/// </summary>
public class GetMachinePlcDetailQueryValidator : AbstractValidator<GetMachinePlcDetailQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetMachinePlcDetailQueryValidator"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public GetMachinePlcDetailQueryValidator()
    {
        // [Fix]
        // CLAUDE
        // Date: 20/08/2025
        // Reason: Enhanced validation to include both MachineId and PlcId with proper ID validation pattern
        this.RuleFor(v => v.MachineId)
            .GreaterThan(0)
            .WithMessage("Machine ID must be greater than 0.");

        this.RuleFor(v => v.PlcId)
            .GreaterThan(0)
            .WithMessage("PLC ID must be greater than 0.");
    }
}