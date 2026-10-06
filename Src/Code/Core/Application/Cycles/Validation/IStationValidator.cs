// <copyright file="IStationValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Cycles.Validation;

/// <summary>
/// Validates station rules for cycle creation.
/// Based on CreateCyclesCommandHandler.StationCanNotStartCycles logic.
/// </summary>
public interface IStationValidator
{
    /// <summary>
    /// Validates if station can start cycles based on machine type.
    /// </summary>
    /// <param name="barCodeInfo">The barcode information containing machine type.</param>
    /// <returns>Result indicating if validation passed or failed with reasons.</returns>
    Result ValidateCanStartCycles(IBarCodeResult barCodeInfo);
}