// <copyright file="TaskGatewayRequestProjectionTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.Characterization.Plc;

/// <summary>
/// Golden-master (characterization) tests for the transient PLC-facing projection
/// produced by <see cref="TaskGatewayRequest.SetCommandStatusFromTask(string)"/>.
///
/// Story 1.1, AC 9 and 10: snapshot the read-only PLC view (the values written back to
/// the PLC) for every <see cref="GatewayTask"/> trigger name, independently of the
/// persisted truth. These tests pin CURRENT as-built behavior; they intentionally do not
/// "fix" the divergences they expose (e.g. EndOfProcess projects EndOfProcess/NOk while
/// the persisted handler writes Finished/Ok — pinned for Story 1.3).
///
/// Source of truth: <c>Src/Code/Core/Domain/Entities/TaskGatewayRequest.cs</c>
/// <c>SetStatus*</c> methods.
/// </summary>
public class TaskGatewayRequestProjectionTests
{
    /// <summary>
    /// AC 10 — the PLC projection golden master. One row per trigger name handled by
    /// <see cref="TaskGatewayRequest.SetCommandStatusFromTask(string)"/>.
    /// Asserts the transient <see cref="TaskGatewayRequest.CycleStatus"/> and
    /// <see cref="TaskGatewayRequest.PartStatus"/> only.
    /// </summary>
    /// <param name="taskName">The <see cref="GatewayTask"/> name driving the projection.</param>
    /// <param name="expectedCycleStatusName">Expected projected cycle status name.</param>
    /// <param name="expectedPartStatusName">Expected projected part status name.</param>
    [Theory]
    [InlineData("ReadBarCodeAsync", nameof(CycleStatus.NotStarted), "Ok")]
    [InlineData("CreateBarCodeAsync", nameof(CycleStatus.NotStarted), "Ok")]
    [InlineData("CreateCycleAsync", nameof(CycleStatus.Started), "Ok")]
    [InlineData("UpdateCycleOkAsync", nameof(CycleStatus.FinishedOk), "Ok")]
    [InlineData("UpdateCycleNotOkAsync", nameof(CycleStatus.FinishedNok), "nOK")]
    [InlineData("RejectPartAsync", nameof(CycleStatus.Rejected), "Rejected")]
    [InlineData("EndOfProcessAsync", nameof(CycleStatus.EndOfProcess), "nOK")]
    public void SetCommandStatusFromTask_ProjectsTransientPlcTuple_AsBuilt(
        string taskName,
        string expectedCycleStatusName,
        string expectedPartStatusName)
    {
        // Arrange
        var request = new TaskGatewayRequest();

        // Act
        request.SetCommandStatusFromTask(taskName);

        // Assert — transient PLC-facing projection only.
        request.CycleStatus.Name.ShouldBe(expectedCycleStatusName);
        request.PartStatus.Name.ShouldBe(expectedPartStatusName);
    }

    /// <summary>
    /// AC 9 — ReadBarCode projection is the read-only PLC view: NotStarted / Ok.
    /// This is the projection-side of the no-op (the persisted-side no-op is asserted
    /// in the Application-layer characterization).
    /// </summary>
    [Fact]
    public void SetCommandStatusFromTask_ReadBarCode_ProjectsNotStartedOk()
    {
        // Arrange
        var request = new TaskGatewayRequest();

        // Act
        request.SetCommandStatusFromTask(GatewayTask.ReadBarCodeAsync.Name);

        // Assert
        request.CycleStatus.ShouldBe(CycleStatus.NotStarted);
        request.PartStatus.ShouldBe(PartStatus.Ok);
    }

    /// <summary>
    /// AC 10 — EndOfProcess divergence is pinned here as-is: the transient projection
    /// is EndOfProcess / NOk, whereas the persisted <c>UpdateBarCodeCommandHandler</c>
    /// writes Finished / Ok. Evidence: <c>TaskGatewayRequest.SetStatusEndOfProcess</c>
    /// vs <c>UpdateBarcodeCommandHandler.UpdateBarcodeState</c>. The convergence decision
    /// is Story 1.3 / Epic 3; do NOT "fix" this here.
    /// </summary>
    [Fact]
    public void SetCommandStatusFromTask_EndOfProcess_ProjectsEndOfProcessNok_DivergesFromPersisted()
    {
        // Arrange
        var request = new TaskGatewayRequest();

        // Act
        request.SetCommandStatusFromTask(GatewayTask.EndOfProcessAsync.Name);

        // Assert — projection says EndOfProcess / NOk (NOT the persisted Finished / Ok).
        request.CycleStatus.ShouldBe(CycleStatus.EndOfProcess);
        request.PartStatus.ShouldBe(PartStatus.NOk);
    }

    /// <summary>
    /// AC 10 — an unknown / unmapped task name leaves the transient projection at its
    /// constructor defaults (CycleStatus.None / PartStatus.None). The switch in
    /// <c>SetCommandStatusFromTask</c> has no default arm — pinned as-is.
    /// </summary>
    [Fact]
    public void SetCommandStatusFromTask_UnknownTask_LeavesDefaults_AsBuilt()
    {
        // Arrange
        var request = new TaskGatewayRequest();

        // Act
        request.SetCommandStatusFromTask("NotARealTaskName");

        // Assert — no arm matched, defaults retained.
        request.CycleStatus.ShouldBe(CycleStatus.None);
        request.PartStatus.ShouldBe(PartStatus.None);
    }
}
