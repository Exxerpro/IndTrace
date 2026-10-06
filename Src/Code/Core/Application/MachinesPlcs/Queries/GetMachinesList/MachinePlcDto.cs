// <copyright file="MachinePlcDto.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.MachinesPlcs.Queries.GetMachinesList;

/// <summary>
/// Represents the MachinePlcDto.
/// </summary>
public class MachinePlcDto
{
    /// <summary>
    /// Gets the MachineId. Init-only (#225): instances live inside the 60-minute cached
    /// <c>ApplicationConfiguration</c> served by reference to every consumer.
    /// </summary>
    public int MachineId { get; init; }

    /// <summary>
    /// Gets the PlcId. Init-only (#225).
    /// </summary>
    public int PlcId { get; init; }

    /// <summary>
    /// Executes ToDto operation.
    /// </summary>
    /// <param name="src">The src.</param>
    /// <returns>The result of ToDto.</returns>
    public static IndQuestResults.Result<MachinePlcDto> ToDto(MachinePlc src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<MachinePlcDto>.WithFailure("MachinePlc source cannot be null");
        }

        return IndQuestResults.Result<MachinePlcDto>.Success(new MachinePlcDto
        {
            MachineId = src.MachineId.Value,
            PlcId = src.PlcId,
        });
    }

    /// <summary>
    /// Executes ToEntity operation.
    /// </summary>
    /// <param name="src">The src.</param>
    /// <returns>The result of ToEntity.</returns>
    public static IndQuestResults.Result<MachinePlc> ToEntity(MachinePlcDto src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<MachinePlc>.WithFailure("MachinePlcDto source cannot be null");
        }

        // Story 2.4 (#26): construct through the guarded MachinePlc.Create seam (behaviour-identical).
        var createResult = MachinePlc.Create(src.MachineId, src.PlcId, ActiveStatus.Active);
        if (createResult.IsFailure || createResult.Value is null)
        {
            return IndQuestResults.Result<MachinePlc>.WithFailure(createResult.Errors);
        }

        return IndQuestResults.Result<MachinePlc>.Success(createResult.Value);
    }
}