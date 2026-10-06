// <copyright file="IGatewayAuditFactory.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Gateway.Auditing;

/// <summary>
/// Creates and persists TaskGatewayRequest audit entries.
/// Based on CreateCyclesCommandHandler audit logic.
/// </summary>
public interface IGatewayAuditFactory
{
    /// <summary>
    /// Creates and persists gateway audit entry.
    /// </summary>
    /// <param name="request">Request containing audit parameters.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>Result containing the created audit entry or failure reasons.</returns>
    Task<Result<TaskGatewayRequest>> CreateAuditEntryAsync(
        GatewayAuditRequest request,
        CancellationToken cancellationToken);
}

/// <summary>
/// Request for creating gateway audit entry.
/// </summary>
/// <param name="MachineId">The machine ID where the operation occurred.</param>
/// <param name="BarCodeId">The barcode ID involved in the operation.</param>
/// <param name="CycleId">The cycle ID created or referenced.</param>
/// <param name="CycleStatus">The cycle status at audit time.</param>
/// <param name="PartStatus">The part status at audit time.</param>
/// <param name="FlowStatus">The flow status at audit time.</param>
/// <param name="ResultValidation">The result validation status.</param>
/// <param name="GatewayTask">The gateway task being audited.</param>
/// <param name="TimeStamp">The timestamp of the operation.</param>
public sealed record GatewayAuditRequest(
    int MachineId,
    int BarCodeId,
    int CycleId,
    CycleStatus CycleStatus,
    PartStatus PartStatus,
    FlowStatus FlowStatus,
    ResultValidation ResultValidation,
    GatewayTask GatewayTask,
    DateTimeOffset TimeStamp);