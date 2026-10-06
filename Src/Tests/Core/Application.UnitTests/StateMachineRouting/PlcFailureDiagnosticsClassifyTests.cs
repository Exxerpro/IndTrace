// <copyright file="PlcFailureDiagnosticsClassifyTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.StateMachine;

namespace Application.UnitTests.StateMachineRouting;

/// <summary>
/// Story 3.3 — unit coverage of <see cref="PlcFailureDiagnostics.Classify"/> for the message->code map,
/// focusing on the adversarial-review correctness fixes:
///   FIX 1 — recipe-aware cycle-time verdict (null recipe -> RecipeNotFound; out-of-range -> PartNotValid),
///   FIX 4 — malformed machine id "number invalid" -> MachineNotFound (not the unrelated InvalidMachine),
///   FIX 3 — "master labels" lookup failure -> ReferencesNotFound,
///   FIX 2 — illegal-transition message -> OperationCancelled (inert plumbing; no handler emits it today).
/// </summary>
public class PlcFailureDiagnosticsClassifyTests
{
    // ---- FIX 1: recipe-aware cycle-time verdict ----

    [Fact]
    public void Classify_CycleTimeInvalid_PartOutOfRange_MapsTo_PartNotValid() =>
        PlcFailureDiagnostics.Classify("Cycle time is invalid: part out of range")
            .ShouldBe(ResultValidation.PartNotValid);

    [Fact]
    public void Classify_CycleTimeInvalid_RecipeNotFound_MapsTo_RecipeNotFound() =>
        PlcFailureDiagnostics.Classify("Cycle time is invalid: recipe not found")
            .ShouldBe(ResultValidation.RecipeNotFound);

    [Fact]
    public void Classify_CycleTimeInvalid_LegacyBareMessage_DefaultsTo_RecipeNotFound() =>
        PlcFailureDiagnostics.Classify("Cycle time is invalid")
            .ShouldBe(ResultValidation.RecipeNotFound);

    // ---- FIX 4: malformed machine id ----

    [Fact]
    public void Classify_MachineNumberInvalid_MapsTo_MachineNotFound() =>
        PlcFailureDiagnostics.Classify("Machine 0 number invalid")
            .ShouldBe(ResultValidation.MachineNotFound);

    [Fact]
    public void Classify_MachineCannotCreateLabels_MapsTo_MachineNotFound() =>
        PlcFailureDiagnostics.Classify("Machine 7 does not exist or cannot create labels.")
            .ShouldBe(ResultValidation.MachineNotFound);

    // ---- FIX 3: master-label lookup failure ----

    [Fact]
    public void Classify_FailedToRetrieveMasterLabels_MapsTo_ReferencesNotFound() =>
        PlcFailureDiagnostics.Classify("Failed to retrieve master labels.")
            .ShouldBe(ResultValidation.ReferencesNotFound);

    // ---- FIX 2: illegal-transition plumbing (inert today, realized in Story 3.4) ----

    [Fact]
    public void Classify_IllegalTransition_MapsTo_OperationCancelled() =>
        PlcFailureDiagnostics.Classify("Illegal transition from Created on UpdateCycleOkAsync")
            .ShouldBe(ResultValidation.OperationCancelled);

    [Fact]
    public void Classify_GuardRejectedTransition_MapsTo_OperationCancelled() =>
        PlcFailureDiagnostics.Classify("Guard rejected transition")
            .ShouldBe(ResultValidation.OperationCancelled);

    // ---- Unmatched: never worse than the prior generic code ----

    [Fact]
    public void Classify_UnknownMessage_FallsBackTo_Invalid() =>
        PlcFailureDiagnostics.Classify("something totally unexpected")
            .ShouldBe(ResultValidation.Invalid);
}
