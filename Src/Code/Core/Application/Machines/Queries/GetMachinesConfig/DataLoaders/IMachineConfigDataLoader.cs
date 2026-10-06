// <copyright file="IMachineConfigDataLoader.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Machines.Queries.GetMachinesConfig.DataLoaders;

/// <summary>
/// Loads and filters machine configuration data with industrial validation patterns.
/// </summary>
public interface IMachineConfigDataLoader
{
    /// <summary>
    /// Loads all related machine configuration data for a specific part number.
    /// </summary>
    /// <param name="partNumber">Product part number for configuration lookup.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>Result containing loaded configuration context.</returns>
    Task<Result<MachineConfigContext>> LoadByPartNumberAsync(
        string partNumber, 
        CancellationToken cancellationToken);
}

/// <summary>
/// Contains all related data for machine configuration assembly.
/// </summary>
public sealed record MachineConfigContext(
    Product Product,
    IReadOnlyList<WorkFlow> WorkFlows,
    IReadOnlyList<Machine> Machines,
    IReadOnlyList<MachinePlc> MachinePlcs,
    IReadOnlyList<Plc> Plcs,
    IReadOnlyList<Variable> Variables)
{
    /// <summary>
    /// Gets the set of participating machine IDs derived from both endpoints of every workflow edge,
    /// excluding the virtual machine 0 boundary marker. This is byte-identical before and after the C2
    /// routing migration that removes the magic-0 boundary pseudo-edges, and (unlike a NextMachineId-only
    /// projection) retains the Initial machine after the migration.
    /// </summary>
    public IReadOnlyList<int> MachineIds => WorkFlows
        .SelectMany(wf => new[] { wf.LastMachineId.Value, wf.NextMachineId.Value })
        .Where(id => id != 0)
        .Distinct()
        .ToList();
}