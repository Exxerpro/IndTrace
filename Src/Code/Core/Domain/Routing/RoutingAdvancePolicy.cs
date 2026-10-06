// <copyright file="RoutingAdvancePolicy.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Routing;

using IndTrace.Domain.Enum;

/// <summary>
/// #40 (M1): the SINGLE pure-domain authority over the routing advance / disabled-cascade / next-machine
/// decision. It is a thin wrapper over the <see cref="ProductionGraph"/> topology authority that reproduces
/// the decision the read path (<c>BarCodeResult</c>) makes today, so BOTH the read path (later — this chunk
/// does not rewire it) and the write path (<see cref="Entities.BarCodes.BarCode.CompleteOkCycle"/>, now)
/// can share ONE rule rather than each re-implementing it.
/// </summary>
/// <remarks>
/// <para>
/// This type is PURE: no EF, no repository, no I/O; it NEVER throws (every failure is a
/// <see cref="Result{T}"/>). The machine-metadata the disabled-cascade needs (machine type / enablement) is
/// resolved by the caller and passed in as plain values, so the policy stays infrastructure-free.
/// </para>
/// <para>
/// <strong>Byte-parity by extraction.</strong> The two members reproduce the two read-path methods exactly:
/// <see cref="DetermineNextMachine"/> mirrors <c>BarCodeResult.DetermineNextMachineId</c> (advance to the
/// single graph successor iff the cycle finished OK, else stay on the current machine — the legacy
/// <c>vmFt[lastMachineId].NextMachineId</c> read, where the reconstructed magic-0 lookup is byte-identical to
/// the graph's successor edges), and <see cref="ApplyDisabledCascade"/> mirrors
/// <c>BarCodeResult.UpdateNextMachineIdIfDisabled</c> together with its Process-machine gate (when the current
/// machine is a Process machine and the resolved next machine is a DISABLED Process machine, cascade one hop
/// to its successor).
/// </para>
/// </remarks>
public static class RoutingAdvancePolicy
{
    /// <summary>
    /// Reproduces <c>BarCodeResult.DetermineNextMachineId</c>: when the cycle finished OK the part advances to
    /// the current machine's single topological successor (<c>0</c> == end of line, mirroring the legacy
    /// <c>(Final -&gt; 0)</c> boundary the read path's magic-0 lookup returned); otherwise the part stays on
    /// the current machine.
    /// </summary>
    /// <param name="graph">The validated production graph (topology authority).</param>
    /// <param name="currentMachineId">The machine that just processed the part (the read path's <c>lastMachineId</c>).</param>
    /// <param name="cycleStatus">The resolved cycle status; only <see cref="CycleStatus.FinishedOk"/> advances.</param>
    /// <returns>
    /// A success <see cref="Result{T}"/> carrying the next machine id (<c>currentMachineId</c> when the cycle
    /// did not finish OK; <c>0</c> at end of line); a failure when the graph is missing or the current machine
    /// is not a node in the graph.
    /// </returns>
    public static Result<int> DetermineNextMachine(ProductionGraph graph, int currentMachineId, CycleStatus cycleStatus)
    {
        if (graph is null)
        {
            return Result<int>.WithFailure("A production graph is required to determine the next machine.");
        }

        if (cycleStatus is null)
        {
            return Result<int>.WithFailure("A cycle status is required to determine the next machine.");
        }

        // Only a FinishedOk cycle advances the part; otherwise it stays on the current machine (the read
        // path's `return lastMachineId`).
        if (cycleStatus.Value != CycleStatus.FinishedOk.Value)
        {
            return Result<int>.Success(currentMachineId);
        }

        // Linear routing: the single successor (0 == end of line, reproducing the legacy
        // `vmFt[final].NextMachineId == 0` terminal). NextMachine fails loud on a multi-successor node
        // rather than silently picking successors[0] — advance must never go non-deterministic.
        return graph.NextMachine(currentMachineId);
    }

    /// <summary>
    /// Reproduces <c>BarCodeResult.UpdateNextMachineIdIfDisabled</c> together with its Process-machine gate:
    /// when the current machine is a Process machine AND the already-resolved next machine is a DISABLED
    /// Process machine, the part cascades one hop to the next machine's successor; otherwise the resolved next
    /// machine is returned unchanged.
    /// </summary>
    /// <param name="graph">The validated production graph (topology authority).</param>
    /// <param name="nextMachineId">The next machine id already resolved by <see cref="DetermineNextMachine"/>.</param>
    /// <param name="currentIsProcessMachine">Whether the CURRENT machine is a <see cref="MachineType.Process"/> machine (the read path's gate).</param>
    /// <param name="nextMachineType">The next machine's type (resolved by the caller).</param>
    /// <param name="nextMachineEnabled">Whether the next machine is enabled (resolved by the caller).</param>
    /// <returns>
    /// A success <see cref="Result{T}"/> carrying the (possibly cascaded) next machine id; a failure when the
    /// graph or next machine type is missing, or the cascade target is not a node in the graph.
    /// </returns>
    public static Result<int> ApplyDisabledCascade(
        ProductionGraph graph,
        int nextMachineId,
        bool currentIsProcessMachine,
        MachineType nextMachineType,
        bool nextMachineEnabled)
    {
        if (graph is null)
        {
            return Result<int>.WithFailure("A production graph is required to apply the disabled cascade.");
        }

        if (nextMachineType is null)
        {
            return Result<int>.WithFailure("A next machine type is required to apply the disabled cascade.");
        }

        // Gate (read path line: `if (this.Machine.MachineType == MachineType.Process)`): the cascade is only
        // considered when the current machine is a Process machine.
        if (!currentIsProcessMachine)
        {
            return Result<int>.Success(nextMachineId);
        }

        // The disabled-cascade condition: next is a Process machine that is not enabled.
        if (nextMachineType.Value != MachineType.Process.Value || nextMachineEnabled)
        {
            return Result<int>.Success(nextMachineId);
        }

        // Cascade one hop to the disabled machine's single successor. NextMachine fails loud on a
        // multi-successor node rather than silently picking hop[0].
        return graph.NextMachine(nextMachineId);
    }
}
