// <copyright file="ICycleUpdateInterfaces.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Cycles.Services.Interfaces;

/// <summary>
/// Provides bar code information retrieval functionality.
/// </summary>
public interface IBarCodeInfoProvider
{
    /// <summary>
    /// Loads an immutable <see cref="CycleUpdateContext"/> snapshot for the DECIDE step of the cycle-update use
    /// case. Story 6.5 — the returned context holds references to the SAME tracked cycle/barcode entities the
    /// loader read, so in-place mutations stay observable. Issue #33 (Chunk 3) — sourced from
    /// <see cref="IBarCodeDetailsLoader"/>; the mutable god-object <c>GetBarCodeInfoAsync</c> seam was retired.
    /// </summary>
    /// <param name="machineId">The machine identifier.</param>
    /// <param name="barCode">The bar code value.</param>
    /// <param name="partNumber">The part number.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the cycle-update context if successful.</returns>
    Task<Result<CycleUpdateContext>> GetCycleUpdateContextAsync(
        int machineId,
        string barCode,
        string partNumber,
        CancellationToken cancellationToken);

    /// <summary>
    /// Loads an immutable <see cref="CycleUpdateLoadState"/> snapshot for the whole cycle-update use case (station
    /// validation, command logging and the §7 PLC projection). Story 6.5 (Task 4) / Issue #33 (Chunk 3) — sourced
    /// from <see cref="IBarCodeDetailsLoader"/>; the snapshot captures the loader getters at LOAD as values, while the
    /// <see cref="CycleUpdateLoadState.Cycle"/> / <see cref="CycleUpdateLoadState.BarCode"/> /
    /// <see cref="CycleUpdateLoadState.Product"/> fields hold the SAME tracked instances the god-object holds, so
    /// in-place DECIDE-step mutations stay observable. The god-object never escapes the loader.
    /// </summary>
    /// <param name="machineId">The machine identifier.</param>
    /// <param name="barCode">The bar code value.</param>
    /// <param name="partNumber">The part number.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the cycle-update load state if successful.</returns>
    Task<Result<CycleUpdateLoadState>> GetCycleUpdateLoadStateAsync(
        int machineId,
        string barCode,
        string partNumber,
        CancellationToken cancellationToken);
}

/// <summary>
/// Validates station capabilities for cycle updates.
/// </summary>
public interface IStationValidator
{
    /// <summary>
    /// Validates if a station can update cycles based on business rules.
    /// </summary>
    /// <param name="machineId">The current machine identifier.</param>
    /// <param name="cycleStatus">The target cycle status.</param>
    /// <param name="barCodeInfo">The bar code information.</param>
    /// <returns>A validation result indicating if the update is allowed.</returns>
    Result<StationValidationResult> ValidateStation(
        int machineId,
        CycleStatus cycleStatus,
        IBarCodeResult barCodeInfo);

    /// <summary>
    /// Validates if a station can update cycles, reading the immutable <see cref="CycleUpdateLoadState"/> snapshot
    /// instead of the god-object. Story 6.5 (Task 4) — additive; identical branches, messages and validation codes
    /// to the <see cref="IBarCodeResult"/> overload.
    /// </summary>
    /// <param name="machineId">The current machine identifier.</param>
    /// <param name="cycleStatus">The target cycle status.</param>
    /// <param name="load">The cycle-update load state snapshot.</param>
    /// <returns>A validation result indicating if the update is allowed.</returns>
    Result<StationValidationResult> ValidateStation(
        int machineId,
        CycleStatus cycleStatus,
        CycleUpdateLoadState load);
}

/// <summary>
/// Represents the result of station validation.
/// </summary>
/// <param name="CanUpdate">Indicates if the station can perform the update.</param>
/// <param name="FailureReason">The reason for validation failure if applicable.</param>
/// <param name="Validation">The validation status.</param>
public record StationValidationResult(
    bool CanUpdate,
    string? FailureReason,
    ResultValidation Validation);

/// <summary>
/// Defines the strategy for updating cycles.
/// </summary>
public interface ICycleUpdateStrategy
{
    /// <summary>
    /// Executes the cycle update according to the specific strategy.
    /// </summary>
    /// <param name="command">The update command containing request data.</param>
    /// <param name="context">The immutable cycle-update context (Story 6.5) carrying the tracked entities the DECIDE step mutates.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the update outcome.</returns>
    Task<Result<CycleUpdateResult>> ExecuteAsync(
        IUpdateCycleCommand command,
        CycleUpdateContext context,
        CancellationToken cancellationToken);
}

/// <summary>
/// Represents the command data for cycle updates.
/// </summary>
public interface IUpdateCycleCommand
{
    /// <summary>
    /// Gets the machine identifier.
    /// </summary>
    int MachineId { get; }
    
    /// <summary>
    /// Gets the bar code value.
    /// </summary>
    string BarCode { get; }
    
    /// <summary>
    /// Gets the part number.
    /// </summary>
    string PartNumber { get; }
    
    /// <summary>
    /// Gets the part status.
    /// </summary>
    PartStatus PartStatus { get; }
    
    /// <summary>
    /// Gets the cycle status.
    /// </summary>
    CycleStatus CycleStatus { get; }
    
    /// <summary>
    /// Gets the registers to save.
    /// </summary>
    IDictionary<string, Register> Registers { get; }
}

/// <summary>
/// Represents the result of a cycle update operation.
/// </summary>
/// <param name="UpdatedCycle">The updated cycle entity.</param>
/// <param name="UpdatedBarCode">The updated bar code entity.</param>
/// <param name="RegistersSaved">The number of registers saved.</param>
/// <param name="CyclesOk">The cycles OK count if applicable.</param>
/// <param name="ShiftInfo">Optional shift information.</param>
public record CycleUpdateResult(
    Cycle UpdatedCycle,
    BarCode UpdatedBarCode,
    int RegistersSaved,
    int? CyclesOk = null,
    ShiftInfo? ShiftInfo = null);

/// <summary>
/// Represents shift information.
/// </summary>
/// <param name="ShiftId">The shift identifier.</param>
/// <param name="CyclesOk">The OK cycles count.</param>
public record ShiftInfo(int ShiftId, int CyclesOk);

/// <summary>
/// Cleans and prepares registers for persistence.
/// </summary>
public interface IRegisterCleaner
{
    /// <summary>
    /// Cleans register values and sets required metadata.
    /// </summary>
    /// <param name="registers">The registers to clean.</param>
    /// <param name="cycleId">The cycle identifier.</param>
    /// <param name="machineId">The machine identifier.</param>
    /// <param name="timestamp">The timestamp to set.</param>
    /// <returns>A result containing cleaned registers.</returns>
    Result<IEnumerable<Register>> CleanRegisters(
        IDictionary<string, Register> registers,
        int cycleId,
        int machineId,
        DateTime timestamp);
}

/// <summary>
/// Logs gateway commands for audit and tracking.
/// </summary>
public interface ICommandLogger
{
    /// <summary>
    /// Logs a gateway command execution.
    /// </summary>
    /// <param name="command">The command to log.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating logging success.</returns>
    Task<Result> LogCommandAsync(
        TaskGatewayRequest command,
        CancellationToken cancellationToken);
    
    /// <summary>
    /// Creates a command from bar code information.
    /// </summary>
    /// <param name="barCodeInfo">The bar code information.</param>
    /// <param name="gatewayTask">The gateway task type.</param>
    /// <param name="comment">Optional comment.</param>
    /// <returns>The created command.</returns>
    TaskGatewayRequest CreateCommand(
        IBarCodeResult barCodeInfo,
        GatewayTask gatewayTask,
        string? comment = null);

    /// <summary>
    /// Creates a command from the immutable <see cref="CycleUpdateLoadState"/> snapshot instead of the god-object.
    /// Story 6.5 (Task 4) — additive; identical field mapping to the <see cref="IBarCodeResult"/> overload.
    /// </summary>
    /// <param name="load">The cycle-update load state snapshot.</param>
    /// <param name="gatewayTask">The gateway task type.</param>
    /// <param name="comment">Optional comment.</param>
    /// <returns>The created command.</returns>
    TaskGatewayRequest CreateCommand(
        CycleUpdateLoadState load,
        GatewayTask gatewayTask,
        string? comment = null);
}

/// <summary>
/// Factory for creating cycle update strategies.
/// </summary>
public interface ICycleUpdateStrategyFactory
{
    /// <summary>
    /// Creates the appropriate strategy for the given cycle status.
    /// </summary>
    /// <param name="cycleStatus">The target cycle status.</param>
    /// <returns>The appropriate update strategy.</returns>
    ICycleUpdateStrategy CreateStrategy(CycleStatus cycleStatus);
}

