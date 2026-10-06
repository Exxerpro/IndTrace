// <copyright file="BarCodeDetailsLoader.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Services;

/// <summary>
/// Stateless implementation of <see cref="IBarCodeDetailsLoader"/>. Issue #33 (Chunk 2, additive).
/// </summary>
/// <remarks>
/// <para>
/// The loader holds only the same singleton-safe repositories / services the god-object holds and keeps NO
/// per-call mutable state. For this chunk it drives the existing <c>BarCodeResult.GetBarCodeDetails</c> ONCE per
/// call to reuse the verified fetch pipeline, then maps the result into an immutable <see cref="BarCodeSnapshot"/>.
/// </para>
/// <para>
/// <b>God-object-never-escapes mechanism:</b> the <c>BarCodeResult</c> is constructed FRESH as a local variable on
/// every <see cref="LoadAsync"/> call and is never stored, returned or otherwise published. Only the immutable
/// snapshot leaves the method. Because a new instance is created per call, there is no shared mutable instance and
/// the captive-dependency race cannot occur through this seam, regardless of the loader's DI lifetime.
/// </para>
/// </remarks>
public class BarCodeDetailsLoader(
    ILogger<BarCodeDetailsLoader> logger,
    ILogger<BarCodeResult> barCodeResultLogger,
    IRepository<BarCode> barCodeRepository,
    IReadOnlyRepository<Cycle> cycleRepository,
    IReadOnlyRepository<Machine> machineRepository,
    IReadOnlyRepository<Recipe> recipeRepository,
    IReadOnlyRepository<MasterLabel> masterLabelRepository,
    IRepository<Shift> shiftRepository,
    IReadOnlyRepository<WorkFlow> workFlowRepository,
    IReadOnlyRepository<RoutingNodeRow> routingNodeRepository,
    IReadOnlyRepository<Variable> variablesRepository,
    IReadOnlyRepository<Product> productRepository,
    IDateTimeMachine dateTimeMachine,
    IBarCodeValidationService validationService,
    IProductionGraphCache? graphCache = null,
    IProductRoutingVersionProbe? routingVersionProbe = null) : IBarCodeDetailsLoader
{
    /// <inheritdoc/>
    public async Task<Result<BarCodeSnapshot>> LoadAsync(BarCodeDetailsRequest request, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return ResultExtensions.Cancelled<BarCodeSnapshot>();
        }

        if (request is null)
        {
            return Result<BarCodeSnapshot>.WithFailure($"Parameter '{nameof(request)}' cannot be null");
        }

        try
        {
            // Construct the god-object FRESH per call as a LOCAL — it never escapes this method.
            var godObject = new BarCodeResult(
                barCodeResultLogger,
                barCodeRepository,
                cycleRepository,
                machineRepository,
                recipeRepository,
                masterLabelRepository,
                shiftRepository,
                workFlowRepository,
                routingNodeRepository,
                variablesRepository,
                productRepository,
                dateTimeMachine,
                validationService,
                graphCache,
                routingVersionProbe);

            _ = await godObject.GetBarCodeDetails(request, cancellationToken).ConfigureAwait(false);

            // Map the driven concrete instance into an immutable snapshot; only the snapshot leaves the loader.
            var snapshot = ToSnapshot(godObject);
            return Result<BarCodeSnapshot>.Success(snapshot);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "BarCodeDetailsLoader:: exception loading bar code details for Label={Label}, MachineId={MachineId}", request.Label, request.MachineId);
            return Result<BarCodeSnapshot>.WithFailure($"Exception loading bar code details: {ex.Message}");
        }
    }

    /// <summary>
    /// Maps the driven concrete <see cref="BarCodeResult"/> into an immutable <see cref="BarCodeSnapshot"/>,
    /// getter-for-getter. The concrete type is used (not <see cref="IBarCodeResult"/>) so the god-object-only
    /// <see cref="BarCodeResult.Name"/> getter is captured too.
    /// </summary>
    /// <param name="info">The driven god-object instance (local to the loader).</param>
    /// <returns>The immutable snapshot carrying every read value.</returns>
    private static BarCodeSnapshot ToSnapshot(BarCodeResult info) => new()
    {
        MachineId = info.MachineId,
        BarCodeId = info.BarCodeId,
        CycleId = info.CycleId,
        CyclesOk = info.CyclesOk,
        ShiftId = info.ShiftId,
        CommandId = info.CommandId,
        ResultValidation = info.ResultValidation,
        Error = info.Error,
        Label = info.Label,
        PartNumber = info.PartNumber,
        Name = info.Name,
        Description = info.Description,
        LastMachineId = info.LastMachineId,
        NextMachineId = info.NextMachineId,
        LegalArrivalMachines = info.LegalArrivalMachines,
        RegistersSaved = info.RegistersSaved,
        CycleStatus = info.CycleStatus,
        FlowStatus = info.FlowStatus,
        PartStatus = info.PartStatus,
        MachineType = info.MachineType,
        WorkFlowType = info.WorkFlowType,
        Recipe = info.Recipe,
        Product = info.Product,
        Cycle = info.Cycle,
        Cycles = info.Cycles,
        BarCode = info.BarCode,
        MasterLabel = info.MasterLabel,
        References = info.References,
    };
}
