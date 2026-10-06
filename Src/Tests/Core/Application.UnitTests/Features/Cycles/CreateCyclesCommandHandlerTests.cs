// <copyright file="CreateCyclesCommandHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.Services;
using IndTrace.Application.Cycles.Policies;
using IndTrace.Application.Cycles.Services;
using IndTrace.Application.Cycles.Validation;
using IndTrace.Application.Gateway.Auditing;

namespace Application.UnitTests.Features.Cycles;

/// <summary>
/// Unit tests for the <see cref="CreateCyclesCommandHandler"/> class.
/// </summary>
/// <remarks>
/// Issue #33 (Chunk 4): the handler loads via the stateless <see cref="IBarCodeDetailsLoader"/> returning an
/// immutable <see cref="BarCodeSnapshot"/>, so these tests substitute the loader (not the mutable god-object).
/// </remarks>
public class CreateCyclesCommandHandlerTests
{
    private readonly IDateTimeMachine _dateTimeMachineSub;
    private readonly IBarCodeDetailsLoader _barCodeDetailsLoaderSub;
    private readonly IStationValidator _stationValidatorSub;
    private readonly ICycleLimitPolicy _cycleLimitPolicySub;
    private readonly ICycleCreator _cycleCreatorSub;
    private readonly IGatewayAuditFactory _gatewayAuditFactorySub;
    private readonly CreateCyclesCommandHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateCyclesCommandHandlerTests"/> class.
    /// </summary>
    public CreateCyclesCommandHandlerTests()
    {
        // Mock all dependencies (#114 chunk B: the separate IBarCodeUpdater step was retired — the barcode
        // status write rides the ICycleCreator's single transactional aggregate save).
        _dateTimeMachineSub = Substitute.For<IDateTimeMachine>();
        _barCodeDetailsLoaderSub = Substitute.For<IBarCodeDetailsLoader>();
        _stationValidatorSub = Substitute.For<IStationValidator>();
        _cycleLimitPolicySub = Substitute.For<ICycleLimitPolicy>();
        _cycleCreatorSub = Substitute.For<ICycleCreator>();
        _gatewayAuditFactorySub = Substitute.For<IGatewayAuditFactory>();

        // Instantiate handler with mocked dependencies
        var logger = XUnitLogger.CreateLogger<CreateCyclesCommandHandler>();
        _handler = new CreateCyclesCommandHandler(
            logger,
            _dateTimeMachineSub,
            _barCodeDetailsLoaderSub,
            _stationValidatorSub,
            _cycleLimitPolicySub,
            _cycleCreatorSub,
            _gatewayAuditFactorySub);
    }

    /// <summary>
    /// Test that the handler can be constructed successfully.
    /// </summary>
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var logger = XUnitLogger.CreateLogger<CreateCyclesCommandHandler>();
        var handler = new CreateCyclesCommandHandler(
            logger,
            _dateTimeMachineSub,
            _barCodeDetailsLoaderSub,
            _stationValidatorSub,
            _cycleLimitPolicySub,
            _cycleCreatorSub,
            _gatewayAuditFactorySub);

        // Assert
        handler.ShouldNotBeNull();
    }

    /// <summary>
    /// Test that the handler fails when machine type cannot start cycles.
    /// </summary>
    [Theory]
    [InlineData("None")] // MachineType.None
    [InlineData("DashBoard")] // MachineType.DashBoard
    public async Task Handle_ShouldFail_WhenMachineTypeCannotStartCycles(string machineTypeName)
    {
        var machineType = EnumModel.FromName<MachineType>(machineTypeName);

        // Arrange
        var command = new CreateCyclesCommand { Command = new TaskGatewayRequest() };

        //[Fix]
        //CLAUDE
        //Date: 26/09/2025
        //Reason: [CONSTRUCTOR UPDATE] - Updated to mock new station validator service for refactored handler
        var snapshot = new BarCodeSnapshot
        {
            MachineType = machineType,
            Cycles = new List<IndTrace.Domain.Entities.Cycle>(),
            Recipe = Recipe.Create(0, 0, 0, 216000, 10, 3, 1).Value.ShouldNotBeNull(),
        };

        _barCodeDetailsLoaderSub.LoadAsync(Arg.Any<BarCodeDetailsRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<BarCodeSnapshot>.Success(snapshot)));

        // Mock station validator to reject non-process stations
        _stationValidatorSub.ValidateCanStartCycles(Arg.Any<IBarCodeResult>())
            .Returns(Result.WithFailure("Just process station can invoke Update cycles."));

        var logger = XUnitLogger.CreateLogger<CreateCyclesCommandHandler>();
        logger.LogInformation(machineType.Name);
        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Just process station can invoke Update cycles.");
    }

    /// <summary>
    /// Test that the handler calls the correct repository methods when processing a valid command.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldCallRepositoryMethods_WhenProcessingValidCommand()
    {
        // Arrange
        var command = new CreateCyclesCommand
        {
            Command = new TaskGatewayRequest
            {
                BarCode = "TEST123",
                MachineId = 100,
                TimeStamp = DateTime.UtcNow,
            },
        };

        //[Fix]
        //CLAUDE
        //Date: 26/09/2025
        //Reason: [CONSTRUCTOR UPDATE] - Updated to mock all new services for refactored handler architecture
        var snapshot = new BarCodeSnapshot
        {
            MachineId = 1,
            NextMachineId = 2,
            CycleStatus = CycleStatus.NotStarted,
            FlowStatus = FlowStatus.Created,
            PartStatus = PartStatus.Ok,
            MachineType = MachineType.Process,
            WorkFlowType = WorkFlowType.Initial,
            BarCodeId = 1,
            CycleId = 0,
            Label = "TEST123",
            PartNumber = "PartNumberExample",
            CyclesOk = 0,
            ShiftId = 1,
            ResultValidation = ResultValidation.Valid,

            // Set up collections to prevent null reference exceptions in validation methods
            Cycles = new List<IndTrace.Domain.Entities.Cycle>(),
            Recipe = Recipe.Create(0, 0, 0, 216000, 10, 3, 1).Value.ShouldNotBeNull(),

            // BarCode in the Created source state so the CreateCycleAsync transition resolves (not rejected).
            BarCode = new BarCodeBuilder().Created(PartStatus.Ok).With(b => { b.BarCodeId = new BarCodeId(1); b.MachineId = new MachineId(1); }).Build(),
        };

        _barCodeDetailsLoaderSub.LoadAsync(Arg.Any<BarCodeDetailsRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<BarCodeSnapshot>.Success(snapshot)));

        // Mock all new services for successful flow
        _stationValidatorSub.ValidateCanStartCycles(Arg.Any<IBarCodeResult>())
            .Returns(Result.Success());

        _cycleLimitPolicySub.EvaluateCycleLimits(Arg.Any<IBarCodeResult>(), Arg.Any<CreateCyclesCommand>())
            .Returns(Result<CycleLimitDecision>.Success(new CycleLimitDecision(true, "Allowed", ResultValidation.Valid)));

        var createdCycle = new IndTrace.Domain.Entities.Cycle { CycleId = new CycleId(123) };
        _cycleCreatorSub.CreateAsync(Arg.Any<CycleCreateRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IndTrace.Domain.Entities.Cycle>.Success(createdCycle)));

        _gatewayAuditFactorySub.CreateAuditEntryAsync(Arg.Any<GatewayAuditRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<TaskGatewayRequest>.Success(new TaskGatewayRequest())));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);
        result.Value.ShouldNotBeNull();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        // #114 chunk B: the barcode status write rides the SAME cycle-create request (one aggregate save).
        await _cycleCreatorSub.Received(1).CreateAsync(
            Arg.Is<CycleCreateRequest>(r => r.FlowStatus == FlowStatus.InProcess),
            Arg.Any<CancellationToken>());
        await _gatewayAuditFactorySub.Received(1).CreateAuditEntryAsync(Arg.Any<GatewayAuditRequest>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Issue #59 (traceability integrity): when the load carries a soft validation error (the loader returns
    /// <c>Success(snapshot)</c> but the snapshot carries a negative <c>ResultValidation</c> + non-empty
    /// <c>Error</c> — e.g. a diverter arrival at a non-legal machine flagged <c>DestinationNotValid</c>), the
    /// handler must refuse the request and persist NOTHING. Before the fix, the cycle, the barcode update and the
    /// audit were all written and only THEN was the load error noticed — leaving a phantom cycle for a refused part.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldPersistNothing_WhenSnapshotCarriesLoadValidationError()
    {
        // Arrange — the request passes the station + cycle-limit gates (it is a process station, limits allow),
        // so the ONLY thing that must stop it is the load-carried validation error.
        var command = new CreateCyclesCommand
        {
            Command = new TaskGatewayRequest
            {
                BarCode = "TEST123",
                MachineId = 99, // arrived at a machine the load flagged as not a legal next
                TimeStamp = DateTime.UtcNow,
            },
        };

        var snapshot = new BarCodeSnapshot
        {
            MachineId = 99,
            NextMachineId = 2,
            CycleStatus = CycleStatus.FinishedOk,
            FlowStatus = FlowStatus.InProcess,
            PartStatus = PartStatus.Ok,
            MachineType = MachineType.Process,
            WorkFlowType = WorkFlowType.Serial,
            BarCodeId = 1,
            CycleId = 0,
            Label = "TEST123",
            PartNumber = "PartNumberExample",
            CyclesOk = 0,
            ShiftId = 1,

            // The load-time arrival gate soft-rejected: negative validation + non-empty Error carried on the snapshot.
            ResultValidation = ResultValidation.DestinationNotValid,
            Error = "Validation failed DestinationNotValid",

            Cycles = new List<IndTrace.Domain.Entities.Cycle>(),
            Recipe = Recipe.Create(0, 0, 0, 216000, 10, 3, 1).Value.ShouldNotBeNull(),
            BarCode = new BarCodeBuilder().Created(PartStatus.Ok).With(b => { b.BarCodeId = new BarCodeId(1); b.MachineId = new MachineId(99); }).Build(),
        };

        _barCodeDetailsLoaderSub.LoadAsync(Arg.Any<BarCodeDetailsRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<BarCodeSnapshot>.Success(snapshot)));

        // Station + cycle-limit gates PASS, so they cannot be what stops the request.
        _stationValidatorSub.ValidateCanStartCycles(Arg.Any<IBarCodeResult>())
            .Returns(Result.Success());
        _cycleLimitPolicySub.EvaluateCycleLimits(Arg.Any<IBarCodeResult>(), Arg.Any<CreateCyclesCommand>())
            .Returns(Result<CycleLimitDecision>.Success(new CycleLimitDecision(true, "Allowed", ResultValidation.Valid)));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert — refused, and NOTHING persisted (no phantom cycle / barcode / audit).
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Validation failed DestinationNotValid");
        await _cycleCreatorSub.DidNotReceive().CreateAsync(Arg.Any<CycleCreateRequest>(), Arg.Any<CancellationToken>());
        await _gatewayAuditFactorySub.DidNotReceive().CreateAuditEntryAsync(Arg.Any<GatewayAuditRequest>(), Arg.Any<CancellationToken>());

        // PO-ratified §7 change (2026-07-07): the SPECIFIC load-carried code must reach the PLC tag, not a
        // misleading generic BarCodeNotFound(-2). SpecificDiagnostics is default-ON, so the failure carries a
        // non-null value whose ResultValidation is the snapshot's own DestinationNotValid(-128).
        result.Value.ShouldNotBeNull();
        result.Value.ShouldNotBeNull().ResultValidation.ShouldBe(ResultValidation.DestinationNotValid);
    }

    /// <summary>
    /// PO-ratified §7 change (2026-07-07): a FAILED load Result (an infrastructure/exception load — the loader
    /// only returns a failed <c>Result</c> from its catch block, never a genuine not-found) must surface the
    /// specific <c>InfrastructureFailure(-262144)</c>, NOT a misleading generic <c>BarCodeNotFound(-2)</c>.
    /// BarCodeNotFound is reserved for a genuine barcode-not-found only.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldSurfaceInfrastructureFailure_WhenLoadResultFails()
    {
        var command = new CreateCyclesCommand
        {
            Command = new TaskGatewayRequest { BarCode = "BC", MachineId = 7, PartNumber = "PART" },
        };

        _barCodeDetailsLoaderSub.LoadAsync(Arg.Any<BarCodeDetailsRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<BarCodeSnapshot>.WithFailure("Exception loading bar code details: boom")));

        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeFalse();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldNotBeNull().ResultValidation.ShouldBe(ResultValidation.InfrastructureFailure);
    }

    /// <summary>
    /// #114 chunk B: a barcode-status persistence failure now happens INSIDE the creator's single
    /// transactional aggregate save (the cycle INSERT + barcode UPDATE roll back together), so it surfaces
    /// through the cycle-create branch as <c>CycleNotFound(-16)</c> with NO audit written — the pre-fix shape
    /// (cycle already committed, then -131072 from the separate barcode auto-commit) pinned the orphan-cycle
    /// defect and is gone.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldSurfaceCycleStepFailure_WhenBarcodeWriteFailsInsideAtomicSave()
    {
        var command = new CreateCyclesCommand
        {
            Command = new TaskGatewayRequest { BarCode = "BC", MachineId = 1, PartNumber = "PART", CycleStatus = CycleStatus.Started, PartStatus = PartStatus.Ok },
        };

        var snapshot = new BarCodeSnapshot
        {
            MachineId = 1,
            BarCodeId = 10,
            FlowStatus = FlowStatus.Created,
            PartStatus = PartStatus.Ok,
            MachineType = MachineType.Process,
            WorkFlowType = WorkFlowType.Initial,
            PartNumber = "PART",
            ResultValidation = ResultValidation.Valid,
            Cycles = new List<IndTrace.Domain.Entities.Cycle>(),
            Recipe = Recipe.Create(0, 0, 0, 216000, 10, 3, 1).Value.ShouldNotBeNull(),
            BarCode = new BarCodeBuilder().Created(PartStatus.Ok).With(b => { b.BarCodeId = new BarCodeId(10); b.MachineId = new MachineId(1); }).Build(),
            References = new Dictionary<string, Register>(),
        };

        _barCodeDetailsLoaderSub.LoadAsync(Arg.Any<BarCodeDetailsRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<BarCodeSnapshot>.Success(snapshot)));
        _stationValidatorSub.ValidateCanStartCycles(Arg.Any<IBarCodeResult>())
            .Returns(Result.Success());
        _cycleLimitPolicySub.EvaluateCycleLimits(Arg.Any<IBarCodeResult>(), Arg.Any<CreateCyclesCommand>())
            .Returns(Result<CycleLimitDecision>.Success(new CycleLimitDecision(true, "Allowed", ResultValidation.Valid)));

        // The barcode WAS found; the barcode UPDATE half of the single atomic save fails — the whole
        // transaction (cycle INSERT included) rolls back inside the creator, which reports the failure.
        _cycleCreatorSub.CreateAsync(Arg.Any<CycleCreateRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IndTrace.Domain.Entities.Cycle>.WithFailure("Failed to persist barcode update")));

        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeFalse();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldNotBeNull().ResultValidation.ShouldBe(ResultValidation.CycleNotFound);

        // Zero rows persisted -> nothing to audit (no phantom CycleId).
        await _gatewayAuditFactorySub.DidNotReceive().CreateAuditEntryAsync(Arg.Any<GatewayAuditRequest>(), Arg.Any<CancellationToken>());
    }
}
