// <copyright file="IMachineConfigAssembler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Machines.Queries.GetMachinesConfig.DataLoaders;

namespace IndTrace.Application.Machines.Queries.GetMachinesConfig.Assemblers;

/// <summary>
/// Assembles MachineConfigVm from loaded data context with proper validation.
/// </summary>
public interface IMachineConfigAssembler
{
    /// <summary>
    /// Assembles machine configuration view model from provided context.
    /// </summary>
    /// <param name="context">Loaded machine configuration context.</param>
    /// <returns>Result containing assembled view model or failure reasons.</returns>
    Result<MachineConfigVm> AssembleConfiguration(MachineConfigContext context);
}