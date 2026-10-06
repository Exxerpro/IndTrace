// <copyright file="BarCodeInfoProvider.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Cycles.Services;

/// <summary>
/// Provides bar code information retrieval functionality.
/// </summary>
/// <remarks>
/// Issue #33 (Chunk 3) — this SRP read seam was cut off the mutable god-object <c>IBarCodeResult</c> onto the
/// stateless <see cref="IBarCodeDetailsLoader"/> + immutable <see cref="BarCodeSnapshot"/>. The two snapshot
/// loaders below now drive <c>LoadAsync</c> directly and build their immutable state from the snapshot getters;
/// the god-object never enters this provider. The snapshot's <c>Cycle</c>/<c>BarCode</c>/<c>Product</c> are the
/// SAME tracked entity instances the loader read, so the DECIDE step's in-place mutations stay observable.
/// </remarks>
public class BarCodeInfoProvider : IBarCodeInfoProvider
{
    private readonly IBarCodeDetailsLoader _barCodeDetailsLoader;
    private readonly ILogger<BarCodeInfoProvider> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BarCodeInfoProvider"/> class.
    /// </summary>
    /// <param name="barCodeDetailsLoader">The stateless bar code details loader.</param>
    /// <param name="logger">The logger instance.</param>
    public BarCodeInfoProvider(
        IBarCodeDetailsLoader barCodeDetailsLoader,
        ILogger<BarCodeInfoProvider> logger)
    {
        _barCodeDetailsLoader = barCodeDetailsLoader;
        _logger = logger;
    }

    /// <summary>
    /// Loads an immutable <see cref="BarCodeSnapshot"/> for the given parameters, surfacing cancellation and
    /// null/exception loads as failures. Shared by the two snapshot loaders below.
    /// </summary>
    /// <param name="machineId">The machine identifier.</param>
    /// <param name="barCode">The bar code value.</param>
    /// <param name="partNumber">The part number.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result carrying the load snapshot on success.</returns>
    private async Task<Result<BarCodeSnapshot>> LoadSnapshotAsync(
        int machineId,
        string barCode,
        string partNumber,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return ResultExtensions.Cancelled<BarCodeSnapshot>();
        }

        _logger.LogInformation(
            "Getting bar code information for BarCode={BarCode}, MachineId={MachineId}, PartNumber={PartNumber}",
            barCode, machineId, partNumber);

        var loadResult = await _barCodeDetailsLoader
            .LoadAsync(new BarCodeDetailsRequest(machineId, barCode, partNumber), cancellationToken)
            .ConfigureAwait(false);

        if (loadResult.IsFailure || loadResult.Value is null)
        {
            _logger.LogError("Bar code result load failed: {Errors}", string.Join(", ", loadResult.Errors));
            return cancellationToken.IsCancellationRequested
                ? ResultExtensions.Cancelled<BarCodeSnapshot>()
                : Result<BarCodeSnapshot>.WithFailure(loadResult.Errors);
        }

        var info = loadResult.Value;
        _logger.LogInformation(
            "Bar code information retrieved successfully: MachineId={MachineId}, NextMachineId={NextMachineId}, MachineType={MachineType}",
            info.MachineId, info.NextMachineId, info.MachineType);

        return Result<BarCodeSnapshot>.Success(info);
    }

    /// <inheritdoc/>
    public async Task<Result<CycleUpdateContext>> GetCycleUpdateContextAsync(
        int machineId,
        string barCode,
        string partNumber,
        CancellationToken cancellationToken)
    {
        // Story 6.5 / #33 Chunk 3 — load through the stateless loader, then snapshot ONLY the DECIDE-step
        // references into an immutable context. The loaded Cycle/BarCode are the SAME tracked instances the loader
        // read, so downstream in-place mutations remain observable.
        var infoResult = await LoadSnapshotAsync(machineId, barCode, partNumber, cancellationToken)
            .ConfigureAwait(false);

        if (infoResult.IsFailure || infoResult.Value is null)
        {
            return cancellationToken.IsCancellationRequested
                ? ResultExtensions.Cancelled<CycleUpdateContext>()
                : Result<CycleUpdateContext>.WithFailure(infoResult.Errors);
        }

        var info = infoResult.Value;
        var context = new CycleUpdateContext(
            info.Cycle,
            info.BarCode,
            info.MachineType,
            info.Recipe);

        return Result<CycleUpdateContext>.Success(context);
    }

    /// <inheritdoc/>
    public async Task<Result<CycleUpdateLoadState>> GetCycleUpdateLoadStateAsync(
        int machineId,
        string barCode,
        string partNumber,
        CancellationToken cancellationToken)
    {
        // Story 6.5 (Task 4) / #33 Chunk 3 — load through the stateless loader, then snapshot EVERY getter the
        // station validator, command logger and §7 projection need. Scalars (CycleStatus/FlowStatus/PartStatus) are
        // the loader's LOAD-TIME scalar getters (the frozen PLC contract), while Cycle/BarCode/Product hold the SAME
        // tracked instances the loader read, so the DECIDE step's in-place mutations stay observable.
        var infoResult = await LoadSnapshotAsync(machineId, barCode, partNumber, cancellationToken)
            .ConfigureAwait(false);

        if (infoResult.IsFailure || infoResult.Value is null)
        {
            return cancellationToken.IsCancellationRequested
                ? ResultExtensions.Cancelled<CycleUpdateLoadState>()
                : Result<CycleUpdateLoadState>.WithFailure(infoResult.Errors);
        }

        var info = infoResult.Value;
        var loadState = new CycleUpdateLoadState(
            MachineId: info.MachineId,
            BarCodeId: info.BarCodeId,
            CycleId: info.CycleId,
            CyclesOk: info.CyclesOk,
            ShiftId: info.ShiftId,
            CommandId: info.CommandId,
            ResultValidation: info.ResultValidation,
            Error: info.Error,
            Label: info.Label,
            PartNumber: info.PartNumber,
            Description: info.Description,
            LastMachineId: info.LastMachineId,
            NextMachineId: info.NextMachineId,
            CycleStatus: info.CycleStatus,
            FlowStatus: info.FlowStatus,
            PartStatus: info.PartStatus,
            MachineType: info.MachineType,
            WorkFlowType: info.WorkFlowType,
            Recipe: info.Recipe,
            MasterLabel: info.MasterLabel,
            References: info.References,
            Cycle: info.Cycle,
            BarCode: info.BarCode,
            Product: info.Product,

            // E6-1 (#56): carry the context-derived legal-arrival set so the station validator's arrival gate
            // checks membership rather than singular equality. On linear data this is { NextMachineId }.
            LegalArrivalMachines: info.LegalArrivalMachines);

        return Result<CycleUpdateLoadState>.Success(loadState);
    }
}