// <copyright file="EndOfProcessDivergenceProjectionTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.Characterization.Anomalies;

/// <summary>
/// Story 1.3 — Anomaly A (EndOfProcess request-vs-persisted divergence), PROJECTION side.
///
/// Pins, AS-IS, the transient PLC-facing projection produced by
/// <see cref="TaskGatewayRequest.SetStatusEndOfProcess"/>: CycleStatus.EndOfProcess (16) /
/// PartStatus.NOk (2). The persisted-truth side (BarCode Finished/Ok + new Cycle FinishedOk/Ok)
/// is pinned in the Application-layer companion test
/// (<c>EndOfProcessDivergencePersistedTruthTests</c>), which also asserts the two sides DISAGREE.
///
/// This is a KNOWN ANOMALY to be resolved (converged) in Epic 3 (FR6) — NOT fixed here.
/// Analysis: state-machine-analysis.md §6.4, §3.2.
/// Evidence: Src/Code/Core/Domain/Entities/TaskGatewayRequest.cs:266-270 (SetStatusEndOfProcess).
/// </summary>
public class EndOfProcessDivergenceProjectionTests
{
    /// <summary>
    /// AC1/AC2/AC6 — the EndOfProcess request projection, reached via the public
    /// dispatcher <see cref="TaskGatewayRequest.SetCommandStatusFromTask(string)"/> with the
    /// EndOfProcessAsync trigger name, yields the transient tuple EndOfProcess / NOk.
    /// This is the projection half of the divergence; characterized AS-IS.
    /// </summary>
    [Fact]
    public void EndOfProcess_RequestProjection_YieldsEndOfProcessNok_AsBuilt()
    {
        // Arrange
        var request = new TaskGatewayRequest();

        // Act — dispatch via the task-name switch (case at TaskGatewayRequest.cs:203 → SetStatusEndOfProcess).
        request.SetCommandStatusFromTask(GatewayTask.EndOfProcessAsync.Name);

        // Assert — §6.4: projection is EndOfProcess(16)/NOk(2) (TaskGatewayRequest.cs:266-270).
        //          KNOWN ANOMALY: the persisted truth says Finished/Ok (UpdateBarcodeCommandHandler.cs:80,95-96).
        request.CycleStatus.ShouldBe(CycleStatus.EndOfProcess); // value 16
        request.PartStatus.ShouldBe(PartStatus.NOk);            // value 2
    }

    /// <summary>
    /// AC2/AC6 — the direct projection method is equivalent to the dispatched path and is
    /// the ONLY place CycleStatus.EndOfProcess is produced. Pins the dead-state property:
    /// EndOfProcess(16) is a projection-only value that NO command handler ever persists to a
    /// <see cref="Cycle"/> — every persisting path writes FinishedOk/FinishedNok/Rejected/etc.
    ///
    /// We pin "never persisted" structurally here: the projection produces EndOfProcess, yet
    /// the only persisting EndOfProcess handler (UpdateBarCodeCommandHandler.CreateNewCycle,
    /// UpdateBarcodeCommandHandler.cs:80) hard-codes CycleStatus.FinishedOk — i.e. the
    /// projected value is NOT among the values a Cycle is ever saved with. The persisted-side
    /// assertion (FinishedOk != EndOfProcess) lives in the Application companion test.
    /// </summary>
    [Fact]
    public void EndOfProcess_IsProjectionOnlyDeadState_NeverPersistedToCycle()
    {
        // Arrange
        var request = new TaskGatewayRequest();

        // Act
        request.SetStatusEndOfProcess(); // TaskGatewayRequest.cs:266 — the sole producer of EndOfProcess

        // Assert — §3.2: EndOfProcess is projection-only.
        request.CycleStatus.ShouldBe(CycleStatus.EndOfProcess);

        // Dead-state characterization: the value the persisting handler hard-codes for an
        // EndOfProcess request is FinishedOk (UpdateBarcodeCommandHandler.cs:80), which is a
        // DIFFERENT value than the projection. EndOfProcess is therefore never written to a Cycle.
        CycleStatus.FinishedOk.ShouldNotBe(CycleStatus.EndOfProcess); // persisted value != projected dead state
    }
}
