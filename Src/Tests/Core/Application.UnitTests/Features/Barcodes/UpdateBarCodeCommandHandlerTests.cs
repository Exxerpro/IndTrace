// <copyright file="UpdateBarCodeCommandHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Application.BarCodes.Commands.Update;
using IndTrace.Domain.ValueObjects;

namespace Application.UnitTests.Features.Barcodes;

/// <summary>
/// Unit tests for UpdateBarCodeCommandHandler.
/// </summary>
/// <remarks>
/// Issue #33 (Chunk 5): the handler was cut off the mutable god-object <c>IBarCodeResult.GetBarCodeDetails</c> onto
/// the stateless <see cref="IBarCodeDetailsLoader"/> returning an immutable <see cref="BarCodeSnapshot"/>. These
/// tests substitute the loader and hand it a <see cref="BarCodeSnapshot"/> (the snapshot carries the specific
/// ResultValidation code even on a validation failure, exactly as production does). Every pinned value is
/// byte-identical to the god-object era.
/// </remarks>
public class UpdateBarCodeCommandHandlerTests
{
    private readonly IDateTimeMachine _dateTimeMachine;
    private readonly IRepository<TaskGatewayRequest> _repositoryCommand;
    private readonly IAggregateRepository<BarCode> _barCodeAggregateRepository;
    private readonly IBarCodeDetailsLoader _loader;
    private readonly UpdateBarCodeCommandHandler _handler;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public UpdateBarCodeCommandHandlerTests()
    {
        _dateTimeMachine = Substitute.For<IDateTimeMachine>();
        _repositoryCommand = Substitute.For<IRepository<TaskGatewayRequest>>();
        _barCodeAggregateRepository = Substitute.For<IAggregateRepository<BarCode>>();
        _loader = Substitute.For<IBarCodeDetailsLoader>();

        // #114 chunk C: the handler persists the FinishedOk cycle + the barcode status write through ONE
        // aggregate save. Mirror the real repository contract — the store-generated identity is back-filled
        // onto the STAGED cycle instance only after the durable commit (here CycleId 456), exactly as
        // BarCodeAggregateRepository's AcceptAllChanges does. The rows-affected-vs-identity confusion of the
        // retired AddAsync path (PR #182) is unrepresentable here by construction.
        _barCodeAggregateRepository.SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                var root = ci.Arg<BarCode>();
                if (root.PendingNewCycles.Count > 0)
                {
                    root.PendingNewCycles[0].CycleId = new CycleId(456);
                }

                return Task.FromResult(Result.Success());
            });

        _handler = new UpdateBarCodeCommandHandler(
            _dateTimeMachine,
            _barCodeAggregateRepository,
            _loader);
    }

    /// <summary>
    /// Stubs the loader to return a Success snapshot for any request (#33 Chunk 5 seam).
    /// </summary>
    private void GivenSnapshot(BarCodeSnapshot snapshot) =>
        _loader.LoadAsync(Arg.Any<BarCodeDetailsRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<BarCodeSnapshot>.Success(snapshot));

    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var mockDateTimeMachine = Substitute.For<IDateTimeMachine>();
        var mockAggregateRepository = Substitute.For<IAggregateRepository<BarCode>>();
        var mockLoader = Substitute.For<IBarCodeDetailsLoader>();

        // Act
        var instance = new UpdateBarCodeCommandHandler(
            mockDateTimeMachine,
            mockAggregateRepository,
            mockLoader);

        // Assert
        instance.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Process_WithValidRequest_ShouldUpdateBarCodeSuccessfully operation.
    /// </summary>
    /// <returns>The result of Process_WithValidRequest_ShouldUpdateBarCodeSuccessfully.</returns>

    [Fact]
    public async Task Process_WithValidRequest_ShouldUpdateBarCodeSuccessfully()
    {
        // Arrange
        var machineId = 1;
        var barCode = "L1ATEST1230001";
        var partNumber = "TEST123";
        var currentTime = DateTime.Now;
        var barCodeId = 123;

        var command = new UpdateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = machineId,
                BarCode = barCode,
                PartNumber = partNumber
            }
        };

        var snapshot = new BarCodeSnapshot
        {
            BarCodeId = barCodeId,
            MachineId = machineId,
            BarCode = new BarCodeBuilder()
                .Created(PartStatus.Ok)
                .With(b =>
                {
                    b.BarCodeId = new BarCodeId(barCodeId);
                    b.Label = BarCodeLabel.FromPersisted(barCode);
                    b.MachineId = new MachineId(machineId);
                })
                .Build(),
            ResultValidation = ResultValidation.Valid,
        };

        GivenSnapshot(snapshot);
        _dateTimeMachine.Now.Returns(currentTime);

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.BarCode.ShouldNotBeNull().Label.Value.ShouldBe(barCode);
        result.Value.MachineId.ShouldBe(machineId);
    }

    /// <summary>
    /// Executes Process_WithInvalidBarCode_ShouldReturnFailure operation.
    /// </summary>
    /// <returns>The result of Process_WithInvalidBarCode_ShouldReturnFailure.</returns>

    [Fact]
    public async Task Process_WithInvalidBarCode_ShouldReturnFailure()
    {
        // Arrange
        var machineId = 1;
        var invalidBarCode = "INVALID123";
        var partNumber = "TEST123";

        var command = new UpdateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = machineId,
                BarCode = invalidBarCode,
                PartNumber = partNumber
            }
        };

        // #33 Chunk 5: a validation failure is Success(snapshot) with the specific negative code + Error carried on
        // the snapshot; the handler's ResultValidation==Valid gate turns it into the value-less WithFailure.
        var snapshot = new BarCodeSnapshot
        {
            ResultValidation = ResultValidation.BarCodeNotFound,
            Error = "Barcode validation failed.",
        };

        GivenSnapshot(snapshot);

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 18/06/2026
        //Reason: [Stale test] - Owner decision: Update hard-fails on non-Valid validation (only
        //        ResultValidation.Valid proceeds). A BarCodeNotFound result must FAIL, not surface
        //        as success-with-status. Aligned the assertion to the as-built handler contract.
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Barcode validation failed.");
    }

    /// <summary>
    /// Executes Process_WithZeroMachineId_ShouldProcessNormally operation.
    /// </summary>
    /// <returns>The result of Process_WithZeroMachineId_ShouldProcessNormally.</returns>

    [Fact]
    public async Task Process_WithZeroMachineId_ShouldProcessNormally()
    {
        // Arrange
        var machineId = 0;
        var barCode = "L1ATEST1230001";
        var partNumber = "TEST123";
        var currentTime = DateTime.Now;

        var command = new UpdateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = machineId,
                BarCode = barCode,
                PartNumber = partNumber
            }
        };

        var snapshot = new BarCodeSnapshot
        {
            BarCodeId = 1,
            BarCode = new BarCodeBuilder()
                .Created(PartStatus.Ok)
                .With(b =>
                {
                    b.BarCodeId = new BarCodeId(1);
                    b.Label = BarCodeLabel.FromPersisted(barCode);
                    b.MachineId = new MachineId(machineId);
                })
                .Build(),
            ResultValidation = ResultValidation.Valid,
        };

        GivenSnapshot(snapshot);
        _dateTimeMachine.Now.Returns(currentTime);

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.MachineId.ShouldBe(machineId);
    }

    /// <summary>
    /// Executes Process_WithEmptyBarCode_ShouldProcessNormally operation.
    /// </summary>
    /// <returns>The result of Process_WithEmptyBarCode_ShouldProcessNormally.</returns>

    [Fact]
    public async Task Process_WithEmptyBarCode_ShouldProcessNormally()
    {
        // Arrange
        var machineId = 1;
        var emptyBarCode = "";
        var partNumber = "TEST123";

        var command = new UpdateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = machineId,
                BarCode = emptyBarCode,
                PartNumber = partNumber
            }
        };

        var snapshot = new BarCodeSnapshot
        {
            ResultValidation = ResultValidation.BarCodeNotFound,
            Error = "Barcode validation failed.",
        };

        GivenSnapshot(snapshot);

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 18/06/2026
        //Reason: [Stale test] - Owner decision: Update hard-fails on non-Valid validation;
        //        a BarCodeNotFound result must FAIL, not "process normally".
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Barcode validation failed.");
    }

    /// <summary>
    /// Executes Process_WithEmptyPartNumber_ShouldProcessNormally operation.
    /// </summary>
    /// <returns>The result of Process_WithEmptyPartNumber_ShouldProcessNormally.</returns>

    [Fact]
    public async Task Process_WithEmptyPartNumber_ShouldProcessNormally()
    {
        // Arrange
        var machineId = 1;
        var barCode = "L1ATEST1230001";
        var emptyPartNumber = "";

        var command = new UpdateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = machineId,
                BarCode = barCode,
                PartNumber = emptyPartNumber
            }
        };

        var snapshot = new BarCodeSnapshot
        {
            ResultValidation = ResultValidation.BarCodeNotFound,
            Error = "Barcode validation failed.",
        };

        GivenSnapshot(snapshot);

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 18/06/2026
        //Reason: [Stale test] - Owner decision: Update hard-fails on non-Valid validation;
        //        a BarCodeNotFound result must FAIL, not "process normally".
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Barcode validation failed.");
    }

    /// <summary>
    /// Executes Process_WithValidRequest_ShouldCreateCycleWithCorrectProperties operation.
    /// </summary>
    /// <returns>The result of Process_WithValidRequest_ShouldCreateCycleWithCorrectProperties.</returns>

    [Fact]
    public async Task Process_WithValidRequest_ShouldCreateCycleWithCorrectProperties()
    {
        // Arrange
        var machineId = 1;
        var barCode = "L1ATEST1230001";
        var partNumber = "TEST123";
        var currentTime = DateTime.Now;
        var barCodeId = 123;

        var command = new UpdateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = machineId,
                BarCode = barCode,
                PartNumber = partNumber
            }
        };

        var snapshot = new BarCodeSnapshot
        {
            BarCodeId = barCodeId,
            BarCode = new BarCodeBuilder()
                .Created(PartStatus.Ok)
                .With(b =>
                {
                    b.BarCodeId = new BarCodeId(barCodeId);
                    b.Label = BarCodeLabel.FromPersisted(barCode);
                    b.MachineId = new MachineId(machineId);
                })
                .Build(),
            ResultValidation = ResultValidation.Valid,
        };

        GivenSnapshot(snapshot);
        _dateTimeMachine.Now.Returns(currentTime);

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.BarCodeId.ShouldBe(barCodeId);
        result.Value.BarCode.ShouldNotBeNull();
        result.Value.BarCode.BarCodeId.Value.ShouldBe(barCodeId);
    }

    /// <summary>
    /// Executes Process_WithValidRequest_ShouldUpdateBarCodeProperties operation.
    /// </summary>
    /// <returns>The result of Process_WithValidRequest_ShouldUpdateBarCodeProperties.</returns>

    [Fact]
    public async Task Process_WithValidRequest_ShouldUpdateBarCodeProperties()
    {
        // Arrange
        var machineId = 1;
        var barCode = "L1ATEST1230001";
        var partNumber = "TEST123";
        var currentTime = DateTime.Now;
        var barCodeId = 123;

        var command = new UpdateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = machineId,
                BarCode = barCode,
                PartNumber = partNumber
            }
        };

        var snapshot = new BarCodeSnapshot
        {
            BarCodeId = barCodeId,
            BarCode = new BarCodeBuilder()
                .Created(PartStatus.Ok)
                .With(b =>
                {
                    b.BarCodeId = new BarCodeId(barCodeId);
                    b.Label = BarCodeLabel.FromPersisted(barCode);
                    b.MachineId = new MachineId(machineId);
                })
                .Build(),
            ResultValidation = ResultValidation.Valid,
        };

        GivenSnapshot(snapshot);
        _dateTimeMachine.Now.Returns(currentTime);

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.BarCodeId.ShouldBe(barCodeId);
        result.Value.BarCode.ShouldNotBeNull();
        result.Value.BarCode.BarCodeId.Value.ShouldBe(barCodeId);
    }

    /// <summary>
    /// Executes Process_WithValidRequest_ShouldSetCycleInBarCodeInfo operation.
    /// </summary>
    /// <returns>The result of Process_WithValidRequest_ShouldSetCycleInBarCodeInfo.</returns>

    [Fact]
    public async Task Process_WithValidRequest_ShouldSetCycleInBarCodeInfo()
    {
        // Arrange
        var machineId = 1;
        var barCode = "L1ATEST1230001";
        var partNumber = "TEST123";
        var currentTime = DateTime.Now;
        var barCodeId = 123;

        var command = new UpdateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = machineId,
                BarCode = barCode,
                PartNumber = partNumber
            }
        };

        var snapshot = new BarCodeSnapshot
        {
            BarCodeId = barCodeId,
            BarCode = new BarCodeBuilder()
                .Created(PartStatus.Ok)
                .With(b =>
                {
                    b.BarCodeId = new BarCodeId(barCodeId);
                    b.Label = BarCodeLabel.FromPersisted(barCode);
                    b.MachineId = new MachineId(machineId);
                })
                .Build(),
            ResultValidation = ResultValidation.Valid,
        };

        GivenSnapshot(snapshot);
        _dateTimeMachine.Now.Returns(currentTime);

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.BarCodeId.ShouldBe(barCodeId);
        result.Value.BarCode.ShouldNotBeNull();
        result.Value.BarCode.BarCodeId.Value.ShouldBe(barCodeId);

        // #33 Chunk 5: the created cycle is threaded immutably (info with { Cycle = entity }) instead of SetCycle;
        // the projected response carries that cycle with the store-generated id the aggregate save back-fills
        // onto the staged entity (mirrored by the SaveAsync stub) — never a rows-affected count (PR #182).
        result.Value.Cycle.ShouldNotBeNull();
        result.Value.Cycle.CycleId.Value.ShouldBe(456);
    }

    /// <summary>
    /// Executes Process_WithValidRequest_ShouldApplyReferencesValues operation.
    /// </summary>
    /// <returns>The result of Process_WithValidRequest_ShouldApplyReferencesValues.</returns>

    [Fact]
    public async Task Process_WithValidRequest_ShouldApplyReferencesValues()
    {
        // Arrange
        var machineId = 1;
        var barCode = "L1ATEST1230001";
        var partNumber = "TEST123";
        var currentTime = DateTime.Now;
        var barCodeId = 123;

        var command = new UpdateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = machineId,
                BarCode = barCode,
                PartNumber = partNumber
            }
        };

        var snapshot = new BarCodeSnapshot
        {
            BarCodeId = barCodeId,
            BarCode = new BarCodeBuilder()
                .Created(PartStatus.Ok)
                .With(b =>
                {
                    b.BarCodeId = new BarCodeId(barCodeId);
                    b.Label = BarCodeLabel.FromPersisted(barCode);
                    b.MachineId = new MachineId(machineId);
                })
                .Build(),
            ResultValidation = ResultValidation.Valid,
        };

        GivenSnapshot(snapshot);
        _dateTimeMachine.Now.Returns(currentTime);

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.BarCodeId.ShouldBe(barCodeId);
        result.Value.BarCode.ShouldNotBeNull();
        result.Value.BarCode.BarCodeId.Value.ShouldBe(barCodeId);
    }

    /// <summary>
    /// Story 5.1 (FR6): the handler MUST persist the converged barcode on the routed (flag-ON) EndOfProcess
    /// path. #114 chunk C: asserts the single aggregate SaveAsync is called exactly once carrying the
    /// converged Finished/Ok status staged on the root TOGETHER with the staged FinishedOk cycle — closing
    /// the former no-persist defect where Story 3.2 computed the truth and dropped it, and proving both
    /// halves ride ONE save.
    /// </summary>
    /// <returns>The result of Process_WithValidRequest_ShouldPersistConvergedBarCode.</returns>
    [Fact]
    public async Task Process_WithValidRequest_ShouldPersistConvergedBarCode()
    {
        // Arrange — default ctor routing is ON (RouteEndOfProcess = true) => converged outcome drives persistence.
        var machineId = 5;
        var barCode = "L1ATEST1230001";
        var partNumber = "TEST123";
        var currentTime = DateTime.Now;
        var barCodeId = 42;

        var command = new UpdateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = machineId,
                BarCode = barCode,
                PartNumber = partNumber,
            },
        };

        // Source state must be InProcess so the state machine legally fires EndOfProcess -> Finished/Ok.
        var persistedBarCode = new BarCodeBuilder()
            .InProcess(PartStatus.Ok)
            .With(b =>
            {
                b.BarCodeId = new BarCodeId(barCodeId);
                b.Label = BarCodeLabel.FromPersisted(barCode);
                b.MachineId = new MachineId(machineId);
            })
            .Build();

        var snapshot = new BarCodeSnapshot
        {
            BarCodeId = barCodeId,
            MachineId = machineId,
            MachineType = MachineType.Final,
            FlowStatus = FlowStatus.InProcess,
            PartStatus = PartStatus.Ok,
            BarCode = persistedBarCode,
            ResultValidation = ResultValidation.Valid,
            References = new Dictionary<string, Register>(),
        };

        GivenSnapshot(snapshot);
        _dateTimeMachine.Now.Returns(currentTime);

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert — operation succeeds and the converged barcode + its cycle are persisted through EXACTLY ONE
        // aggregate save: the root carries Finished/Ok, the staged status-write flag AND the staged new cycle.
        result.IsSuccess.ShouldBeTrue();
        await _barCodeAggregateRepository.Received(1).SaveAsync(
            Arg.Is<BarCode>(b => b.BarCodeId.Value == barCodeId
                && b.FlowStatus == FlowStatus.Finished
                && b.PartStatus == PartStatus.Ok
                && b.HasPendingStatusWrite
                && b.PendingNewCycles.Count == 1),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Story 5.1 (FR6): a barcode-persistence failure MUST fail the operation (Result railway), not be
    /// swallowed. Asserts a failed UpdateAsync surfaces as IsFailure.
    /// </summary>
    /// <returns>The result of Process_WhenBarCodePersistenceFails_ShouldFail.</returns>
    [Fact]
    public async Task Process_WhenBarCodePersistenceFails_ShouldFail()
    {
        // Arrange
        var machineId = 5;
        var barCode = "L1ATEST1230001";
        var partNumber = "TEST123";
        var currentTime = DateTime.Now;
        var barCodeId = 42;

        var command = new UpdateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = machineId,
                BarCode = barCode,
                PartNumber = partNumber,
            },
        };

        var persistedBarCode = new BarCodeBuilder()
            .InProcess(PartStatus.Ok)
            .With(b =>
            {
                b.BarCodeId = new BarCodeId(barCodeId);
                b.Label = BarCodeLabel.FromPersisted(barCode);
                b.MachineId = new MachineId(machineId);
            })
            .Build();

        var snapshot = new BarCodeSnapshot
        {
            BarCodeId = barCodeId,
            MachineId = machineId,
            MachineType = MachineType.Final,
            FlowStatus = FlowStatus.InProcess,
            PartStatus = PartStatus.Ok,
            BarCode = persistedBarCode,
            ResultValidation = ResultValidation.Valid,
            References = new Dictionary<string, Register>(),
        };

        GivenSnapshot(snapshot);
        _dateTimeMachine.Now.Returns(currentTime);

        // Persistence fails => the operation must fail (no swallow). #114 chunk C: the failure is the ONE
        // aggregate save covering both the cycle INSERT and the barcode status UPDATE.
        _barCodeAggregateRepository.SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.WithFailure("DB write failed.")));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
    }

    /// <summary>
    /// #114 chunk C atomicity regression: when the barcode half of the EndOfProcess persistence fails, NO
    /// cycle row may have been committed through a separate auto-commit — the cycle INSERT and the barcode
    /// Flow/Part status UPDATE must ride ONE aggregate save, so a failure of either half leaves zero writes.
    /// Pre-fix the handler committed the FinishedOk cycle via <c>IRepository&lt;Cycle&gt;.AddAsync</c> BEFORE
    /// the failable barcode update, so the PLC was told "failed" while the cycle row stood — a retry then
    /// minted a second FinishedOk cycle.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task Process_WhenBarCodeHalfFails_MustNotHaveCommittedCycleSeparately()
    {
        // Arrange
        var machineId = 5;
        var barCode = "L1ATEST1230001";
        var partNumber = "TEST123";
        var currentTime = DateTime.Now;
        var barCodeId = 42;

        var command = new UpdateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = machineId,
                BarCode = barCode,
                PartNumber = partNumber,
            },
        };

        var persistedBarCode = new BarCodeBuilder()
            .InProcess(PartStatus.Ok)
            .With(b =>
            {
                b.BarCodeId = new BarCodeId(barCodeId);
                b.Label = BarCodeLabel.FromPersisted(barCode);
                b.MachineId = new MachineId(machineId);
            })
            .Build();

        var snapshot = new BarCodeSnapshot
        {
            BarCodeId = barCodeId,
            MachineId = machineId,
            MachineType = MachineType.Final,
            FlowStatus = FlowStatus.InProcess,
            PartStatus = PartStatus.Ok,
            BarCode = persistedBarCode,
            ResultValidation = ResultValidation.Valid,
            References = new Dictionary<string, Register>(),
        };

        GivenSnapshot(snapshot);
        _dateTimeMachine.Now.Returns(currentTime);

        // The barcode half fails => the ONE aggregate save fails as a whole (the real repository rolls back
        // the cycle INSERT with it — proven on live SQL in the Integration suite).
        _barCodeAggregateRepository.SaveAsync(Arg.Any<BarCode>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.WithFailure("storage offline")));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert — the failure must not coexist with a separately committed cycle write: exactly ONE save
        // attempt carried BOTH halves (the staged cycle + the staged status write), and no other persistence
        // seam exists on the handler (the retired IRepository<Cycle>/IRepository<BarCode> pair is gone).
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("storage offline"));
        await _barCodeAggregateRepository.Received(1).SaveAsync(
            Arg.Is<BarCode>(b => b.PendingNewCycles.Count == 1 && b.HasPendingStatusWrite),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes TryReset_ShouldReturnTrue operation.
    /// </summary>

    [Fact]
    public void TryReset_ShouldReturnTrue()
    {
        // Act
        var result = _handler.TryReset();

        // Assert
        result.ShouldBeTrue();
    }
}
