// <copyright file="GatewayAuditFactory.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Repositories;

namespace IndTrace.Application.Gateway.Auditing;

/// <summary>
/// Creates and persists TaskGatewayRequest audit entries.
/// Based on CreateCyclesCommandHandler audit logic.
/// Implements CLAUDE.md compliance: Result pattern, cancellation support, defensive validation.
/// </summary>
public class GatewayAuditFactory : IGatewayAuditFactory
{
    private readonly IRepository<TaskGatewayRequest> _auditRepository;
    private readonly ILogger<GatewayAuditFactory> _logger;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="GatewayAuditFactory"/> class.
    /// </summary>
    /// <param name="auditRepository">Repository for audit entry persistence.</param>
    /// <param name="logger">Logger for recording audit operations.</param>
    public GatewayAuditFactory(
        IRepository<TaskGatewayRequest> auditRepository,
        ILogger<GatewayAuditFactory> logger)
    {
        _auditRepository = auditRepository ?? throw new ArgumentNullException(nameof(auditRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /// <summary>
    /// Creates and persists gateway audit entry.
    /// </summary>
    /// <param name="request">Request containing audit parameters.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>Result containing the created audit entry or failure reasons.</returns>
    public async Task<Result<TaskGatewayRequest>> CreateAuditEntryAsync(
        GatewayAuditRequest request,
        CancellationToken cancellationToken)
    {
        // CLAUDE.md compliance: early cancellation check
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<TaskGatewayRequest>.WithFailure(["Operation was canceled."]);
        }
        
        // CLAUDE.md compliance: defensive validation
        if (request is null)
        {
            _logger.LogError("GatewayAuditRequest cannot be null");
            return Result<TaskGatewayRequest>.WithFailure(["Request cannot be null."]);
        }
            
        try
        {
            _logger.LogInformation(
                "Creating gateway audit entry for BarCodeId {BarCodeId}, CycleId {CycleId}, Task {GatewayTask}",
                request.BarCodeId, request.CycleId, request.GatewayTask);
                
            // Create audit entry based on original CreateCyclesCommandHandler logic
            var auditEntry = new TaskGatewayRequest
            {
                MachineId = request.MachineId,
                BarCodeId = request.BarCodeId,
                CycleId = request.CycleId,
                CycleStatus = request.CycleStatus,
                PartStatus = request.PartStatus,
                FlowStatus = request.FlowStatus,
                ResultValidation = request.ResultValidation,
                GatewayTask = request.GatewayTask,
                TimeStamp = request.TimeStamp.ToLocalTime().DateTime,
            };
            
            var addResult = await _auditRepository.AddAsync(auditEntry, cancellationToken).ConfigureAwait(false);
            if (addResult.IsFailure)
            {
                // Issue #123 (F3): repositories fail railway-style (they do NOT throw), so this Result was
                // silently discarded — a dropped audit row logged "Successfully created" and returned Success.
                // Callers already gate on this factory's Result, so surface the failure to them.
                _logger.LogError(
                    "Failed to persist gateway audit entry for BarCodeId {BarCodeId}, CycleId {CycleId}: {Errors}",
                    request.BarCodeId, request.CycleId, string.Join("; ", addResult.Errors ?? []));
                return Result<TaskGatewayRequest>.WithFailure(addResult.Errors ?? ["Failed to persist gateway audit entry."]);
            }

            _logger.LogInformation(
                "Successfully created gateway audit entry {CommandId} for BarCodeId {BarCodeId}, CycleId {CycleId}",
                auditEntry.CommandId, request.BarCodeId, request.CycleId);

            return Result<TaskGatewayRequest>.Success(auditEntry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Failed to create audit entry for BarCodeId {BarCodeId}, CycleId {CycleId}", 
                request.BarCodeId, request.CycleId);
            return Result<TaskGatewayRequest>.WithFailure([$"Failed to create audit entry: {ex.Message}"]);
        }
    }
}