// <copyright file="CreateBarCodeCommandHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Commands.Create;

using IndQuestResults.Operations;
using Microsoft.Extensions.Logging.Abstractions;
using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Application.Shifts.Commands.Create;
using IndTrace.Application.StateMachine;
using IndTrace.Domain.StateMachine;
using Microsoft.Extensions.Options;
using Machine = IndTrace.Domain.Entities.Machine;

/// <summary>
/// Handles the creation of new bar codes by processing CreateBarCodeCommand requests.
/// Validates machine existence, creates bar code with consecutive numbering, and initializes associated cycle.
/// </summary>
// [Fix]
// CLAUDE
// Date: 25/08/2025
// Reason: [ARCHITECTURAL CLEANUP] - Add IBarCodeService to complete extension method migration
// [Fix]
// CLAUDE
// Date: 03/08/2026
// Reason: [#114 chunk A partial-write saga] - All failable lookups (shift, reference variables) now run
//         BEFORE any persistence, and the barcode + Started cycle are persisted ATOMICALLY through
//         IAggregateRepository<BarCode> (one explicit transaction). A failure after the old
//         barcode/cycle AddAsync commits left phantom traceability rows and a burned consecutive; now a
//         failure at any stage leaves zero rows. The §7 response and failure codes are unchanged.
public class CreateBarCodeCommandHandler(
    IReadOnlyRepository<Rule> ruleRepository,
    IAggregateRepository<BarCode> barCodeAggregateRepository,
    IReadOnlyRepository<Machine> machineRepository,
    IReadOnlyRepository<Product> productRepository,
    IReadOnlyRepository<Variable> variableRepository,
    IRepository<TaskGatewayRequest> requestRepository,
    IShiftService shiftService,
    IDateTimeMachine dateTimeMachine,
    IMasterLabelService masterLabelService,
    IBarCodeService barCodeService,
    ILogger<CreateBarCodeCommandHandler> logger,
    IItemStateMachine? stateMachine = null,
    IOptions<StateMachineRoutingOptions>? routingOptions = null,
    ILoggerFactory? loggerFactory = null) : IGatewayRequestHandler<CreateBarCodeCommand, TaskGatewayResponseDto>, IResettable
{
    // Story 3.1: when no machine/options are injected (legacy positional construction in characterization
    // tests), fall back to the as-built engine and default-ON routing so behavior is identical to DI.
    private readonly IItemStateMachine machine = stateMachine ?? new ItemStateMachine();
    private readonly StateMachineRoutingOptions routing = routingOptions?.Value ?? new StateMachineRoutingOptions();

    /// <summary>
    /// Processes the create barcode command asynchronously.
    /// </summary>
    /// <param name="cmd">The create barcode command containing the request details.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a <see cref="Result{T}"/> with a <see cref="TaskGatewayResponseDto"/> if successful,
    /// or error information if the operation fails.
    /// </returns>
    /// <remarks>
    /// This method performs the following steps (#114 chunk A: all failable lookups precede persistence):
    /// 1. Validates the machine ID and ensures it's a printer type
    /// 2. Looks up the product by part number
    /// 3. Retrieves the applicable rule for the machine-product combination
    /// 4. Generates a unique barcode label using the rule engine
    /// 5. Manages shift information
    /// 6. Validates the reference variables exist
    /// 7. Persists the new barcode and its Started cycle ATOMICALLY (one transaction)
    /// 8. Builds the reference registers for the response from the persisted cycle
    /// 9. Returns a comprehensive response with all related data.
    /// </remarks>
    // The pipeline populates this context one field per railway step (GetProduct -> GetRule -> GenerateLabel ->
    // GetShift -> GetVariables -> PersistBarcodeAndCycle -> BuildReferences). The not-yet-populated members are
    // genuinely absent at seed time, so they are nullable reference types (seeded null, never null!). Each
    // consuming step requires its inputs fluently (RequireValue) and fails the railway if a prior step left them
    // unpopulated — replacing the previous seven `null!` suppressions with real, railway-compliant null resolution.
    // #114 chunk A: the failable shift/variables lookups run BEFORE persistence, so Shift and Variables are
    // populated before Barcode/Cycle; References is built LAST from the persisted cycle's real identity.
    private record CreateBarcodeContext(TaskGatewayRequest Request, Machine Machine, Product? Product, Rule? Rule, int Consecutive, string? Label, BarCode? Barcode, Cycle? Cycle, ShiftCreatedEvent? Shift, Dictionary<string, Register>? References, IReadOnlyList<Variable>? Variables = null);

    public async Task<Result<TaskGatewayResponseDto>> ProcessAsync(CreateBarCodeCommand cmd, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<TaskGatewayResponseDto>.WithFailure("Operation was canceled.");
        }

        // #181: the former post-chain `if (result.IsFailure)` funnel is dissolved into chained side effects —
        // TapErrorAsync persists the failure audit (fires only on failure), ThenTap persists the success audit
        // (fires only on success), and ThenRecover shapes the frozen §7 failure response LAST, after the audit
        // side effects have completed, preserving the previous imperative ordering exactly.
        // #114 chunk A: every failable lookup (shift, reference variables) runs BEFORE the single atomic
        // barcode+cycle persistence, so a refusal can no longer leave phantom rows; the reference registers
        // are built AFTER the save from the persisted cycle's real identity (values only — nothing written).
        return await Result.Success(cmd.Command)
            .ValidateNotNull(req => (req, nameof(req)))
            .Ensure(req => req.MachineId > 0, $"Machine {cmd.Command.MachineId} number invalid")
            .ThenAsync(req => GetMachineAsync(req, cancellationToken))
            .ThenAsync(context => GetProductAsync(context, cancellationToken))
            .ThenAsync(context => GetRuleAsync(context, cancellationToken))
            .ThenAsync(context => GetMasterLabelAndConsecutiveAsync(context, cancellationToken))
            .Then(context => GenerateLabel(context))
            .ThenAsync(context => GetShiftInfoAsync(context, cancellationToken))
            .ThenAsync(context => GetVariablesAsync(context, cancellationToken))
            .ThenAsync(context => PersistBarcodeAndCycleAsync(context, cancellationToken))
            .Then(context => BuildReferences(context))
            .ThenAsync(context => Task.FromResult(BuildFinalResponse(context)))
            .TapErrorAsync(errors => PersistFailureAuditAsync(cmd.Command, errors, cancellationToken))
            .ThenTap(response => PersistSuccessAuditAsync(cmd.Command, response, cancellationToken))
            .ThenRecover(errors => Task.FromResult(ShapeFailureResponse(errors, cmd.Command.MachineId)));
    }

    //[Fix]
    //CLAUDE
    //Date: 19/06/2026
    //Reason: [Story 3.3 specific PLC diagnostics] - Map the failing step to a specific ResultValidation
    //        (InvalidMachine / MachineNotFound / ProductNotFound / RuleNotFound / ReferencesNotFound)
    //        from the handler's own diagnostic message. Frozen enum values unchanged; this only selects
    //        among existing members.
    private async Task PersistFailureAuditAsync(TaskGatewayRequest request, IEnumerable<string> errors, CancellationToken cancellationToken)
    {
        var error = errors.ToList();
        var specificCode = MapFailureValidation(error);
        logger.LogError("CreateBarCode failed for machine {MachineId} ({Code}): {Errors}", request.MachineId, specificCode.Name, string.Join("; ", error));
        var command = new TaskGatewayRequest
        {
            MachineId = request.MachineId,
            ResultValidation = specificCode,
            Comment = string.Join("; ", error),
            GatewayTask = GatewayTask.CreateBarCodeAsync,
            TimeStamp = dateTimeMachine.Now.ToLocalTime(),
        };
        // #65 (traceability integrity): §7 failure response is FROZEN and must stay byte-identical, so a
        // dropped failure-audit write cannot flip the wire response — log-and-flag the discarded Result
        // instead so the missing gateway record is observable.
        await requestRepository.AddAsync(command, cancellationToken)
            .ThenLogErrors(auditErrors => logger.LogError(
                "CreateBarCode failure-audit write did not land for machine {MachineId}: {Error}",
                request.MachineId, auditErrors.FirstOrDefault()));
    }

    //[Fix]
    //CLAUDE
    //Date: 18/06/2026
    //Reason: [Success audit] - Persist a CreateBarCodeAsync gateway request on the success path
    //        (ResultValidation.Valid), mirroring the failure-path audit so every creation attempt
    //        leaves a traceable gateway record.
    private async Task PersistSuccessAuditAsync(TaskGatewayRequest request, TaskGatewayResponseDto response, CancellationToken cancellationToken)
    {
        var successAudit = new TaskGatewayRequest
        {
            MachineId = request.MachineId,
            BarCodeId = response.BarCodeId,
            CycleId = response.CycleId,
            PartNumber = request.PartNumber,
            ResultValidation = ResultValidation.Valid,
            GatewayTask = GatewayTask.CreateBarCodeAsync,
            TimeStamp = dateTimeMachine.Now.ToLocalTime(),
        };
        // #65 (traceability integrity): §7 success response is FROZEN and must stay byte-identical, so a
        // dropped success-audit write cannot flip the wire response — log-and-flag the discarded Result so
        // the missing gateway record is observable while the PLC still sees the created barcode/cycle.
        await requestRepository.AddAsync(successAudit, cancellationToken)
            .ThenLogErrors(auditErrors => logger.LogError(
                "CreateBarCode success-audit write did not land for machine {MachineId}, BarCodeId {BarCodeId}: {Error}",
                request.MachineId, successAudit.BarCodeId, auditErrors.FirstOrDefault()));
    }

    //[Fix]
    //CLAUDE
    //Date: 19/06/2026
    //Reason: [Story 3.3 AC3/AC4] - When SpecificDiagnostics is ON (default), return a value-carrying
    //        failure whose non-null Value holds the specific NEGATIVE ResultValidation + References so it
    //        survives ControllerExtensions.PublishResultToPlc (its `ResultValidation.Value >= 0` collapse
    //        branch is naturally false). When OFF, fall through to the value-less failure so the legacy
    //        generic `-1` collapse re-applies (AC8 zero-redeploy rollback). Transport unchanged (CR4).
    private Result<TaskGatewayResponseDto> ShapeFailureResponse(IEnumerable<string> errors, int machineId)
    {
        var error = errors.ToList();
        if (routing.SpecificDiagnostics)
        {
            var diagnostic = PlcFailureDiagnostics.BuildDiagnosticResponse(MapFailureValidation(error), machineId);
            return Result<TaskGatewayResponseDto>.WithFailure(error, diagnostic);
        }

        return Result<TaskGatewayResponseDto>.WithFailure(error);
    }

    // Story 3.3: single source of truth for the failure->ResultValidation map lives in
    // PlcFailureDiagnostics.Classify (covers Machine/Product/Rule/References + the update-path rows).
    private static ResultValidation MapFailureValidation(IEnumerable<string> errors) =>
        PlcFailureDiagnostics.Classify(errors.FirstOrDefault());

    // The sentinel a repository lookup (ReadOnlyRepository/Repository FirstOrDefault) returns when no entity
    // matches. The read-through cache round-trips failures through its serializer, so it is matched as an
    // ordinal substring rather than exact equality.
    private const string RepositoryNotFoundSentinel = RepositoryFailures.NotFoundSentinel;

    //[Fix]
    //CLAUDE
    //Date: 20/06/2026
    //Reason: [Not-found path] - the package RequireValue propagates the repository's generic
    //        "No matching entity found." sentinel verbatim, leaking infrastructure detail in place of the
    //        handler's business diagnostic. A required-entity lookup with no value (null read OR that sentinel)
    //        is the business "does not exist" case, so surface the caller's message; genuine infrastructure
    //        failures carry other messages and are preserved.
    private static Result<T> RequireEntity<T>(Result<T?> lookup, string businessMessage)
        where T : class
    {
        if (lookup.IsSuccess && lookup.Value is not null)
        {
            return Result<T>.Success(lookup.Value);
        }

        var noValue = lookup.Value is null
            || lookup.Errors.Any(e => e is not null && e.Contains(RepositoryNotFoundSentinel, StringComparison.Ordinal));

        return noValue
            ? Result<T>.WithFailure(businessMessage)
            : Result<T>.WithFailure(lookup.Errors);
    }

    private async Task<Result<CreateBarcodeContext>> GetMachineAsync(TaskGatewayRequest request, CancellationToken cancellationToken)
    {
        // #61 defense-in-depth: hoist the captured value to a single-hop closure local so the cache-key
        // visitor evaluates it to its runtime value (a two-hop closure previously collapsed to one key).
        var machineId = new MachineId(request.MachineId);
        var spec = new Specification<Machine>(m => m.MachineId == machineId && (m.MachineType == MachineType.Printer || m.MachineType == MachineType.InitialPrinter));
        var lookup = await machineRepository.FirstOrDefaultAsync(spec, cancellationToken).ConfigureAwait(false);
        return RequireEntity(lookup, $"Machine {request.MachineId} does not exist or cannot create labels.")
            .Then(machine => new CreateBarcodeContext(request, machine, null, null, 0, null, null, null, null, null));
    }

    private async Task<Result<CreateBarcodeContext>> GetProductAsync(CreateBarcodeContext context, CancellationToken cancellationToken)
    {
        // #61 defense-in-depth: hoist the two-hop closure to a single-hop closure local.
        var partNumber = context.Request.PartNumber;
        var spec = new Specification<Product>(p => p.PartNumber == partNumber);
        var lookup = await productRepository.FirstOrDefaultAsync(spec, cancellationToken).ConfigureAwait(false);
        return RequireEntity(lookup, $"Product for {context.Request.PartNumber} does not exist.")
            .Then(product => context with { Product = product });
    }

    private async Task<Result<CreateBarcodeContext>> GetRuleAsync(CreateBarcodeContext context, CancellationToken cancellationToken)
    {
        return await Task.FromResult(Result.Success(context.Product))
            .RequireValue("Product missing from create-barcode pipeline context.")
            .ThenAsync(product =>
            {
                // #61 defense-in-depth: hoist the two-hop closures to single-hop closure locals.
                var machineId = context.Machine.MachineId;
                var productId = product.ProductId;
                var spec = new Specification<Rule>(r => r.MachineId == machineId && r.ProductId == productId && r.IsActive);
                spec.AddOrderByDescending(r => r.Version);
                return ruleRepository.FirstOrDefaultAsync(spec, cancellationToken)
                    .RequireValue($"Rule for Machine {context.Machine.MachineId.Value} does not exist.")
                    .ThenMap(rule => context with { Rule = rule });
            });
    }

    private async Task<Result<CreateBarcodeContext>> GetMasterLabelAndConsecutiveAsync(CreateBarcodeContext context, CancellationToken cancellationToken)
    {
        var masterLabelResult = await masterLabelService.GetMasterLabelByPartNumberAsync(context.Request.PartNumber, cancellationToken);
        if (masterLabelResult is null || masterLabelResult.IsFailure)
        {
            return Result<CreateBarcodeContext>.WithFailure("Failed to retrieve master labels.");
        }

        // #186: pass the resolved label-generation rule so the consecutive derivation can read the counter-field
        // width from the SAME metadata the formatter mints with (legacy all-digit corpora have no non-digit
        // boundary for the trailing-run heuristic).
        return await barCodeService.GetConsecutiveByBarCodeLabelAsync(context.Request.PartNumber, masterLabelResult.Value ?? [], context.Rule, cancellationToken)
            .ThenMap(consecutive => context with { Consecutive = consecutive });
    }

    private Result<CreateBarcodeContext> GenerateLabel(CreateBarcodeContext context)
    {
        return Result.Success(context.Rule)
            .RequireValue("Rule missing from create-barcode pipeline context.")
            .Then(rule =>
            {
                var ruleExecutor = new CreateBarCodeDictionaryExecutor(
                    dateTimeMachine,
                    (loggerFactory ?? NullLoggerFactory.Instance).CreateLogger<CreateBarCodeDictionaryExecutor>());
                ruleExecutor.ParseRuleFromJson(rule.RuleJson ?? string.Empty);
                ruleExecutor.InitializeComponentActions();
                return ruleExecutor.ApplyRuleCreateBarCode(context.Request.PartNumber, context.Consecutive)
                    .Then(label => context with { Label = label });
            });
    }

    // #114 chunk A: the ONE persistence step. Constructs the barcode root and its Started cycle (byte-identical
    // field population to the retired CreateBarcodeEntityAsync/CreateCycleEntityAsync pair — including the
    // Story 6.1/6.4 create/apply seams and the shift-derived CyclesOk, which is now applied BEFORE the row is
    // written instead of mutating an already-saved instance) and saves BOTH through the BarCode aggregate's
    // single explicit transaction: a mid-save failure rolls the pair back, so no phantom barcode/cycle row and
    // no burned consecutive can leak to a PLC retry.
    private async Task<Result<CreateBarcodeContext>> PersistBarcodeAndCycleAsync(CreateBarcodeContext context, CancellationToken cancellationToken)
    {
        return await Task.FromResult(Result.Success(context.Label))
            .RequireValue("Generated label missing from create-barcode pipeline context.")
            .ThenAsync(label => Task.FromResult(Result.Success(context.Product))
                .RequireValue("Product missing from create-barcode pipeline context.")
                .ThenAsync(product => Task.FromResult(Result.Success(context.Shift))
                    .RequireValue("Shift missing from create-barcode pipeline context.")
                    .ThenAsync(async shift =>
                    {
                        // Story 6.1: route construction through the public BarCode.Create seam (Created/Ok). The routed
                        // status below still overwrites it from the machine outcome (or legacy literals when the flag is
                        // OFF), so the persisted result is byte-identical to the prior inline construction.
                        var barcode = BarCode.Create(
                            label,
                            product.ProductId.Value,
                            context.Request.MachineId,
                            dateTimeMachine.Now.ToLocalTime(),
                            dateTimeMachine.Now.ToLocalTime());

                        // ResolveCreateStatuses fires the machine on its OWN None-state probe (not this now-Created
                        // barcode), preserving the (None, CreateBarCodeAsync) -> Created resolution exactly.
                        var resolved = ResolveCreateStatuses(context);

                        // Story 6.4: the routed (machine-resolved or legacy-literal) status is copied through the trusted
                        // create/apply seam (setters now private set) — byte-equal with the prior assignments.
                        barcode.ApplyFlowAndPartStatus(resolved.FlowStatus, resolved.BarCodePartStatus);

                        // Story 6.1: route construction through the public Cycle.CreateStarted seam. #114: the
                        // shift-derived CyclesOk is applied at construction — the retired pipeline assigned it AFTER the
                        // cycle row was committed, so the value was silently never persisted (latent side-bug, now fixed).
                        var cycle = Cycle.CreateStarted(
                            context.Request.MachineId,
                            barcode.BarCodeId.Value,
                            cyclesOk: shift.CyclesOk,
                            dateTimeMachine.Now.ToLocalTime(),
                            dateTimeMachine.Now.ToLocalTime());
                        cycle.ApplyCycleAndPartStatus(resolved.CycleStatus, resolved.CyclePartStatus);

                        var staged = barcode.StageNewCycle(cycle);
                        if (staged is null || staged.IsFailure)
                        {
                            return Result<CreateBarcodeContext>.WithFailure(staged?.Errors ?? ["Failed to stage the new cycle on the barcode aggregate."]);
                        }

                        // ONE transactional save: the brand-new root INSERT and its Started cycle ride a single
                        // explicit transaction (BarCodeAggregateRepository.SaveAsync); the store-generated
                        // BarCodeId/CycleId are written back onto the instances only after a durable commit.
                        var saved = await barCodeAggregateRepository.SaveAsync(barcode, cancellationToken).ConfigureAwait(false);
                        if (saved is null || saved.IsFailure)
                        {
                            return Result<CreateBarcodeContext>.WithFailure(saved?.Errors ?? ["The barcode aggregate save returned no result."]);
                        }

                        return Result<CreateBarcodeContext>.Success(context with { Barcode = barcode, Cycle = cycle });
                    })));
    }

    /// <summary>
    /// Story 3.1 (AC1): resolves the new barcode's <see cref="FlowStatus"/>/<see cref="PartStatus"/> and the
    /// new cycle's <see cref="CycleStatus"/>/<see cref="PartStatus"/> from a single
    /// <see cref="GatewayTask.CreateBarCodeAsync"/> transition, replacing the inline <c>FlowStatus.Created</c>/
    /// <c>CycleStatus.Started</c>/<c>PartStatus.Ok</c> literals. The fail-closed lookup flags
    /// (Machine/Product/Rule) are set from this handler's OWN successful lookups — a context reaching this
    /// point already passed GetMachine/GetProduct/GetRule. The intended cycle/part status (Started/Ok, mirrored
    /// by the machine on the success path) is fed into the context. When the flag is OFF, or the machine
    /// rejects, the legacy literal values are preserved (AC6/AC8).
    /// </summary>
    private (FlowStatus FlowStatus, PartStatus BarCodePartStatus, CycleStatus CycleStatus, PartStatus CyclePartStatus) ResolveCreateStatuses(CreateBarcodeContext context)
    {
        var legacyFlow = FlowStatus.Created;
        var legacyCycle = CycleStatus.Started;
        var legacyPart = PartStatus.Ok;

        if (!routing.RouteCreateBarCode)
        {
            return (legacyFlow, legacyPart, legacyCycle, legacyPart);
        }

        // Story 6.1: fire a fresh None-state probe (the persisted barcode is now constructed Created via
        // BarCode.Create) so the table resolves (None, CreateBarCodeAsync) -> Created exactly as before.
        // The cycle/part status mirror the context the machine receives.
        // Story 27.2b-2: a throwaway state-machine seed — Fire() reads only FlowStatus (defaults to None), never the
        // label, and this probe is never persisted. Give it a marker label so the required BarCodeLabel is satisfied.
        var probe = new BarCode { Label = BarCodeLabel.FromPersisted("CreateBarCodeProbe") };
        var transitionContext = new TransitionContext(
            context.Machine.MachineType,
            legacyCycle,
            legacyPart,
            CycleTime: 0,
            Recipe: null,
            MachineFound: true,
            ProductFound: true,
            RuleFound: true);

        var outcome = machine.Fire(probe, GatewayTask.CreateBarCodeAsync, transitionContext);
        if (outcome.IsFailure || outcome.Value is null)
        {
            logger.LogWarning(
                "CreateBarCode: state machine rejected CreateBarCodeAsync ({Errors}); preserving legacy Created/Started/Ok.",
                string.Join("; ", outcome.Errors));
            return (legacyFlow, legacyPart, legacyCycle, legacyPart);
        }

        return (outcome.Value.NextFlowStatus, outcome.Value.NextPartStatus, outcome.Value.NextCycleStatus, outcome.Value.NextPartStatus);
    }

    // #114 chunk A: the shift lookup runs BEFORE any persistence — it no longer needs (or mutates) a cycle.
    // The shift-derived CyclesOk is applied to the cycle at construction time in PersistBarcodeAndCycleAsync.
    private async Task<Result<CreateBarcodeContext>> GetShiftInfoAsync(CreateBarcodeContext context, CancellationToken cancellationToken)
    {
        return await shiftService.CreateOrRetrieveShiftAndCyclesOkAsync(context.Request.MachineId, cancellationToken)
            .ThenMap(shift => context with { Shift = shift });
    }

    // #114 chunk A: ONLY the failable lookup + the frozen "References ... not found." refusal run here, BEFORE
    // persistence. The reference registers themselves are values built AFTER the atomic save (BuildReferences)
    // because they embed the persisted cycle's real identity.
    private async Task<Result<CreateBarcodeContext>> GetVariablesAsync(CreateBarcodeContext context, CancellationToken cancellationToken)
    {
        // #61 defense-in-depth: hoist the multi-hop closure to a single-hop closure local.
        var machineId = context.Machine.MachineId.Value;
        var spec = new Specification<Variable>(r => r.MachineId == machineId && r.IsActive == ActiveStatus.Active && r.VariableGroupId == TagsGroups.ReferenceTags.Value);
        return await variableRepository.ListAsync(spec, cancellationToken)
            .Ensure(variables => variables.Any(), $"References for {context.Request.PartNumber} not found.")
            .ThenMap(variables => context with { Variables = variables.ToList() });
    }

    // #114 chunk A: pure value construction from the already-validated variables and the PERSISTED cycle (real
    // CycleId) — nothing here talks to the database, so it cannot fail the request after the atomic save.
    private Result<CreateBarcodeContext> BuildReferences(CreateBarcodeContext context)
    {
        return Result.Success(context.Cycle)
            .RequireValue("Cycle missing from create-barcode pipeline context.")
            .Then(cycle => Result.Success(context.Variables)
                .RequireValue("Variables missing from create-barcode pipeline context.")
                .Then(variables =>
                {
                    // #39: build the reference registers through the guarded Register.Create factory. `.Add`
                    // preserves the original ToDictionary duplicate-key throw. For valid (non-null) variable strings
                    // this is byte-identical to the previous object-initializer.
                    var references = new Dictionary<string, Register>();
                    foreach (var v in variables)
                    {
                        if (Register.Create(
                            name: v.Name,
                            description: string.Empty,
                            machineId: 0,
                            variableId: v.VariableId,
                            cycleId: cycle.CycleId.Value,

                            // #43: ReferenceTags.Value is always empty in the DB and is overwritten downstream, so
                            // seeding string.Empty is byte-equivalent. (Variable.Value dropped.)
                            value: string.Empty,

                            // #43: source DataType from the real .NET type (NetType), not the role-tag-polluted NativeType.
                            dataType: v.NetType,
                            statusValueId: 1,
                            timeStamp: default) is { IsSuccess: true, Value: { } register })
                        {
                            references.Add(v.Name, register);
                        }
                    }

                    return context with { References = references };
                }));
    }

    private Result<TaskGatewayResponseDto> BuildFinalResponse(CreateBarcodeContext context)
    {
        const string missingContext = "BarCode, Cycle or References missing from create-barcode pipeline context.";
        return Result.Success(context.Barcode)
            .RequireValue(missingContext)
            .Then(barcode => Result.Success(context.Cycle)
                .RequireValue(missingContext)
                .Then(cycle => Result.Success(context.References)
                    .RequireValue(missingContext)
                    .Then(references =>
                    {
                        // #32 C2: build the immutable wire DTO directly (the retired fluent With* chain is gone). WithBarCode's
                        // derived Label rule (absent -> empty, present -> label verbatim) is inlined; the barcode is non-null here.
                        var response = new TaskGatewayResponseDto
                        {
                            MachineId = context.Request.MachineId,
                            Description = context.Machine.Name ?? "Unknown Machine",
                            Name = context.Machine.Name ?? "Unknown Machine",
                            BarCodeId = barcode.BarCodeId.Value,
                            CycleId = cycle.CycleId.Value,
                            CyclesOk = cycle.CyclesOk,
                            ResultValidation = ResultValidation.Valid,
                            PartNumber = context.Request.PartNumber,
                            LastMachineId = context.Request.MachineId,
                            NextMachineId = context.Request.MachineId,
                            CycleStatus = cycle.CycleStatus,
                            FlowStatus = barcode.FlowStatus,
                            PartStatus = barcode.PartStatus,
                            MachineType = context.Machine.MachineType,
                            WorkFlowType = context.Machine.WorkFlowType,
                            Cycle = cycle,
                            BarCode = barcode,
                            Label = barcode.Label.Value,
                            References = references,
                        };

                        // ReferenceStamper.Apply always yields a non-null value on success, so its Result is the
                        // handler's result verbatim — same errors on failure, same stamped DTO on success.
                        return ReferenceStamper.Apply(response);
                    })));
    }

    /// <summary>
    /// Resets the command handler to its initial state.
    /// </summary>
    /// <returns>True if the reset operation was successful.</returns>
    public bool TryReset()
    {
        return true;
    }
}
