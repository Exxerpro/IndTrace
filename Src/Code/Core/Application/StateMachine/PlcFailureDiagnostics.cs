// <copyright file="PlcFailureDiagnostics.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.StateMachine;

using IndTrace.Domain.Entities;
using IndTrace.Domain.Entities.BarCodes;
using IndTrace.Domain.Enum;

/// <summary>
/// Story 3.3 (AC1-AC4): the single place that turns a PLC-path handler failure into a SPECIFIC negative
/// <see cref="ResultValidation"/> diagnostic that survives the publish path.
///
/// THE MECHANIC (verified against <c>ControllerExtensions.PublishResultToPlc</c>): the transport overwrites
/// the <c>CycleStatus</c>/<c>PartStatus</c>/<c>ResultValidation</c> tags with the literal <c>"-1"</c> when the
/// failed response's <c>Value</c> is <see langword="null"/> OR its <c>Value.ResultValidation.Value &gt;= 0</c>.
/// Therefore a precise negative code reaches the tag IFF the handler returns a failure whose <c>Result.Value</c>
/// is NON-null with <c>Value.ResultValidation.Value &lt; 0</c> AND whose <c>Value.References["ResultValidation"]</c>
/// carries that code (that dict is what <c>SetReferencesForController</c> pushes to the controller).
///
/// This helper builds exactly such a value. It never edits the transport (CR4/NFR1); it only computes the value.
/// </summary>
public static class PlcFailureDiagnostics
{
    /// <summary>
    /// Maps a handler diagnostic message to the specific <see cref="ResultValidation"/> code (Story 3.3 table).
    /// The handlers carry their failure reason in a human-readable message produced by the as-built code; this
    /// classifier selects among existing frozen enum members from that message. Unmatched messages fall back to
    /// <see cref="ResultValidation.Invalid"/> (the prior generic code) so behavior is never made worse.
    /// </summary>
    /// <param name="message">The first failure message from the handler's <c>Result</c>.</param>
    /// <returns>The specific negative <see cref="ResultValidation"/> for the failure, or <see cref="ResultValidation.Invalid"/>.</returns>
    public static ResultValidation Classify(string? message)
    {
        var text = message ?? string.Empty;
        return text switch
        {
            // --- Create path lookups (CreateBarCodeCommandHandler) ---
            //[Fix] CLAUDE Date: 19/06/2026 Reason: [Story 3.3 FIX 4] - a malformed machine id on Create
            //("Machine N number invalid") is the Create machine-validity failure intent -> MachineNotFound(-8)
            //(row 1). It previously borrowed InvalidMachine(-4096), which is the UNRELATED MachineFinalGuard code.
            _ when text.Contains("number invalid", StringComparison.Ordinal) => ResultValidation.MachineNotFound,
            _ when text.Contains("cannot create labels", StringComparison.Ordinal) => ResultValidation.MachineNotFound,
            _ when text.Contains("master labels", StringComparison.OrdinalIgnoreCase) => ResultValidation.ReferencesNotFound,
            _ when text.Contains("Product for", StringComparison.Ordinal) => ResultValidation.ProductNotFound,
            _ when text.Contains("Rule for Machine", StringComparison.Ordinal) => ResultValidation.RuleNotFound,
            _ when text.Contains("References for", StringComparison.Ordinal) => ResultValidation.ReferencesNotFound,

            // --- Illegal transition rejected by the machine (Story 3.3 FIX 2, OPTIONAL plumbing) ---
            //[Fix] CLAUDE Date: 19/06/2026 Reason: [Story 3.3 FIX 2] - IF a handler ever surfaces a machine
            //reject (Illegal/Guard-rejected transition), map it to the machine's specific OperationCancelled(-65536)
            //instead of generic Invalid. NOTE (row 10 DEFERRED): no as-built routed handler currently surfaces this
            //message — the legacy fallback swallows the reject (Story 3.1 AC8), so this branch is inert today and is
            //fully realized only once illegal-transition rejection is ENFORCED (Story 3.4). It flips no behavior.
            _ when text.Contains("Illegal transition", StringComparison.OrdinalIgnoreCase) => ResultValidation.OperationCancelled,
            _ when text.Contains("Guard rejected transition", StringComparison.OrdinalIgnoreCase) => ResultValidation.OperationCancelled,

            // --- Update path lookups (Ok / NotOk) ---
            _ when text.Contains("Cannot create Shift", StringComparison.Ordinal) => ResultValidation.ShiftInvalid,
            _ when text.Contains("Shift entity is null", StringComparison.Ordinal) => ResultValidation.ShiftInvalid,
            //[Fix] CLAUDE Date: 19/06/2026 Reason: [Story 3.3 FIX 1] - recipe-aware cycle-time verdict matching
            //CycleTimeGuard (the authority): out-of-range WITH a valid recipe -> PartNotValid(-64)
            //(CycleTimeGuard.cs:65-66); null recipe -> RecipeNotFound(-512) (CycleTimeGuard.cs:54-57). The
            //"part out of range" arm MUST precede the null-recipe / legacy fallback arm. The bare "Cycle time is
            //invalid" fallback (legacy callers) defaults to RecipeNotFound to preserve prior behavior.
            _ when text.Contains("Cycle time is invalid: part out of range", StringComparison.Ordinal) => ResultValidation.PartNotValid,
            _ when text.Contains("Cycle time is invalid: recipe not found", StringComparison.Ordinal) => ResultValidation.RecipeNotFound,
            _ when text.Contains("Cycle time is invalid", StringComparison.Ordinal) => ResultValidation.RecipeNotFound,
            _ when text.Contains("Cycle not Found", StringComparison.Ordinal) => ResultValidation.CycleNotFound,
            _ when text.Contains("Cycle entity is null", StringComparison.Ordinal) => ResultValidation.CycleNotFound,
            _ when text.Contains("BarCode not Found", StringComparison.Ordinal) => ResultValidation.BarCodeNotFound,
            _ when text.Contains("BarCode entity is null", StringComparison.Ordinal) => ResultValidation.BarCodeNotFound,
            _ when text.Contains("BarCode is null", StringComparison.Ordinal) => ResultValidation.BarCodeNotFound,
            _ when text.Contains("not found", StringComparison.OrdinalIgnoreCase) && text.Contains("BarCode", StringComparison.Ordinal) => ResultValidation.BarCodeNotFound,

            // --- Station / machine cannot start/update cycle (row 9) ---
            // Note: this station-start/update check (a runtime "wrong machine type for this trigger" refusal) is
            // DISTINCT from the CreateCycle MachineGuard, which emits MachineNotFound for a missing machine. Row 9
            // keeps InvalidMachine per the story table on purpose.
            _ when text.Contains("Station cannot update cycles", StringComparison.Ordinal) => ResultValidation.InvalidMachine,
            _ when text.Contains("process station", StringComparison.OrdinalIgnoreCase) => ResultValidation.InvalidMachine,
            _ when text.Contains("Station validation failed", StringComparison.Ordinal) => ResultValidation.InvalidMachine,

            // --- Unhandled exception ---
            _ when text.Contains("exception", StringComparison.OrdinalIgnoreCase) => ResultValidation.ExceptionResultValidation,

            _ => ResultValidation.Invalid,
        };
    }

    /// <summary>
    /// Builds a value-carrying diagnostic <see cref="TaskGatewayResponseDto"/> for a failed
    /// <c>Result&lt;TaskGatewayResponseDto&gt;</c> whose <c>Value</c> is otherwise <see langword="null"/>.
    /// The returned response carries the specific negative <paramref name="code"/> on
    /// <see cref="TaskGatewayResponseDto.ResultValidation"/> AND a References dictionary whose
    /// <c>ResultValidation</c> register reflects the same code, so it survives the publish path.
    /// </summary>
    /// <param name="code">The specific negative <see cref="ResultValidation"/> to publish.</param>
    /// <param name="machineId">The originating machine id (for the audit/projection).</param>
    /// <param name="references">An optional References dictionary to reuse; a new one is created when null/empty.</param>
    /// <returns>A diagnostic response carrying the specific code on both the field and the References tag.</returns>
    public static TaskGatewayResponseDto BuildDiagnosticResponse(
        ResultValidation code,
        int machineId,
        IDictionary<string, Register>? references = null)
    {
        var dict = references is { Count: > 0 }
            ? references
            : new Dictionary<string, Register>();

        EnsureResultValidationRegister(dict);

        // #32 C2: build the immutable wire DTO directly and stamp its References via the pure ReferenceStamper.
        var response = new TaskGatewayResponseDto
        {
            MachineId = machineId,
            ResultValidation = code,
            References = dict,
        };

        var stamped = ReferenceStamper.Apply(response);
        return stamped.Value ?? response;
    }

    /// <summary>
    /// Promotes an existing barcode-projection <see cref="TaskGatewayResponseDto"/> (already built from an
    /// <see cref="IBarCodeResult"/>) to carry the specific negative <paramref name="code"/> so it survives the
    /// publish path. Reuses the response's own References dictionary, adding a <c>ResultValidation</c> register
    /// if the projection did not include one.
    /// </summary>
    /// <param name="response">The projection response to enrich (its References are reused).</param>
    /// <param name="code">The specific negative <see cref="ResultValidation"/> to publish.</param>
    /// <returns>The same response instance, now carrying the specific code on the field and the References tag.</returns>
    public static TaskGatewayResponseDto Promote(TaskGatewayResponseDto response, ResultValidation code)
    {
        ArgumentNullException.ThrowIfNull(response);

        // #32 C2: immutable wire DTO — promote with a `with`, seed the ResultValidation register in the (shared)
        // References dictionary, then stamp via the pure ReferenceStamper and return the enriched DTO.
        var promoted = response with { ResultValidation = code };
        EnsureResultValidationRegister(promoted.References);
        var stamped = ReferenceStamper.Apply(promoted);
        return stamped.Value ?? promoted;
    }

    private static void EnsureResultValidationRegister(IDictionary<string, Register> references)
    {
        if (!references.ContainsKey(nameof(TaskGatewayResponseDto.ResultValidation)))
        {
            // #39: build the placeholder register through the guarded Register.Create factory. Only Name is
            // meaningful here; Description/Value/DataType default to empty (byte-identical to the prior
            // object-initializer). Create cannot fail for these non-null inputs — the guard is defensive.
            var registerResult = Register.Create(
                name: nameof(TaskGatewayResponseDto.ResultValidation),
                description: string.Empty,
                machineId: 0,
                variableId: 0,
                cycleId: 0,
                value: string.Empty,
                dataType: string.Empty,
                statusValueId: 0,
                timeStamp: default);

            if (registerResult.IsSuccess && registerResult.Value is not null)
            {
                references[nameof(TaskGatewayResponseDto.ResultValidation)] = registerResult.Value;
            }
        }
    }
}
