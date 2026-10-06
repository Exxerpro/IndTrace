// <copyright file="IPlcDetailAssembler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Plcs.Queries.GetDetail.DataLoaders;

namespace IndTrace.Application.Plcs.Queries.GetDetail.Assemblers;

/// <summary>
/// Assembles PlcDto from loaded context with comprehensive business rules.
/// Based on GetPlcDetailQueryHandler assembly logic with magic number extraction.
/// </summary>
public interface IPlcDetailAssembler
{
    /// <summary>
    /// Assembles complete PLC detail view model.
    /// </summary>
    /// <param name="context">Loaded PLC detail context.</param>
    /// <returns>Result containing assembled PLC DTO or failure reasons.</returns>
    Result<PlcDto> AssembleDetail(PlcDetailContext context);
}

/// <summary>
/// Variable group constants extracted from magic numbers in original implementation.
/// Based on the hardcoded values 128 (registers) and 256 (references).
/// </summary>
public static class VariableGroupIds
{
    /// <summary>
    /// Variable group ID for register variables.
    /// </summary>
    public const int Registers = 128;

    /// <summary>
    /// Variable group ID for reference variables.
    /// </summary>
    public const int References = 256;
}