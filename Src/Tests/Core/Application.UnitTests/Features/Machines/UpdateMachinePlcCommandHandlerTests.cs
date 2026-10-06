// <copyright file="UpdateMachinePlcCommandHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Application.MachinesPlcs.Commands.Update;

namespace Application.UnitTests.Features.Machines;

/// <summary>
/// Unit tests for UpdateMachinePlcCommandHandler.
/// #95 Phase 2 Slice C: the composite-key read stays on IReadOnlyRepository&lt;MachinePlc&gt;; every write is
/// staged on a loaded Machine root and persisted through IAggregateRepository&lt;Machine&gt;.SaveAsync. The
/// IsActive-only update and the SAME-MACHINE key change are each ONE atomic save; only a CROSS-MACHINE move
/// spans two roots/two saves and keeps the #113 F7 insert-first ordering + compensation. The aggregate mock
/// mirrors the real repository contract: staged sets are CONSUMED by every save attempt, and a reload of the
/// same machine returns the same root instance.
/// </summary>
public class UpdateMachinePlcCommandHandlerTests
{
    private readonly IReadOnlyRepository<MachinePlc> _repository = null!;
    private readonly IAggregateRepository<Machine> _machineAggregateRepository = null!;
    private readonly ILogger<UpdateMachinePlcCommandHandler> _logger = null!;
    private readonly UpdateMachinePlcCommandHandler _handler = null!;

    private readonly Dictionary<int, Machine> _roots = [];
    private readonly List<SaveAttempt> _saves = [];
    private Func<Machine, CancellationToken, Result> _saveBehavior = (_, _) => Result.Success();

    /// <summary>
    /// A recorded aggregate save attempt: the root's machine id plus snapshots of the staged member sets the
    /// attempt consumed.
    /// </summary>
    /// <param name="MachineId">The machine id of the saved root.</param>
    /// <param name="Appends">The staged MachinePlc appends the attempt consumed.</param>
    /// <param name="Updates">The staged MachinePlc updates the attempt consumed.</param>
    private sealed record SaveAttempt(int MachineId, IReadOnlyList<MachinePlc> Appends, IReadOnlyList<MachinePlc> Updates);

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    public UpdateMachinePlcCommandHandlerTests()
    {
        _repository = Substitute.For<IReadOnlyRepository<MachinePlc>>();
        _machineAggregateRepository = Substitute.For<IAggregateRepository<Machine>>();

        // LoadAsync mirrors the real repository: early cancellation check, and reloading the same machine id
        // yields the same root instance (so compensation reloads observe prior state).
        _machineAggregateRepository.LoadAsync(Arg.Any<int>(), Arg.Any<AggregateLoadOptions>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                if (callInfo.Arg<CancellationToken>().IsCancellationRequested)
                {
                    return Result<Machine>.WithFailure("Operation was canceled.");
                }

                var id = callInfo.ArgAt<int>(0);
                if (!_roots.TryGetValue(id, out var root))
                {
                    root = new Machine { MachineId = new MachineId(id) };
                    _roots[id] = root;
                }

                return Result<Machine>.Success(root);
            });

        // SaveAsync mirrors the real repository contract: it records the staged sets, applies the
        // test-configured outcome, and CONSUMES the staged sets on EVERY attempt (success or failure).
        _machineAggregateRepository.SaveAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var root = callInfo.Arg<Machine>();
                _saves.Add(new SaveAttempt(
                    root.MachineId.Value,
                    root.PendingMachinePlcAppends.ToList(),
                    root.PendingMachinePlcUpdates.ToList()));
                var outcome = _saveBehavior(root, callInfo.Arg<CancellationToken>());
                root.ClearStagedMachineChanges();
                return outcome;
            });

        _logger = XUnitLogger.CreateLogger<UpdateMachinePlcCommandHandler>();
        _handler = new UpdateMachinePlcCommandHandler(_repository, _machineAggregateRepository, _logger);
    }

    /// <summary>
    /// Mirrors the real repository contract for FirstOrDefaultAsync(spec): the specification criteria is
    /// evaluated against the sample data, a match returns success and no match returns the repository's
    /// "No matching entity found." sentinel failure (Repository.cs:279).
    /// </summary>
    private static Result<MachinePlc?> FindFirst(IEnumerable<MachinePlc> source, ISpecification<MachinePlc> spec)
    {
        var match = source.FirstOrDefault(spec.Criteria.Compile());
        return match is not null
            ? Result<MachinePlc?>.Success(match)
            : Result<MachinePlc?>.WithFailure("No matching entity found.");
    }

    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var realLogger = XUnitLogger.CreateLogger<UpdateMachinePlcCommandHandler>();
        var handler = new UpdateMachinePlcCommandHandler(_repository, _machineAggregateRepository, realLogger);

        // Assert
        handler.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Process_WithValidCommand_ShouldReturnSuccess operation.
    /// </summary>
    /// <returns>The result of Process_WithValidCommand_ShouldReturnSuccess.</returns>

    [Fact]
    public async Task Process_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var existingMachinePlc = MachinePlc.CreateFixture(10000, 200, 0);  // Inactive

        var command = new UpdateMachinePlcCommand
        {
            MachineId = 10000,
            PlcId = 200,
            IsActive = 1,  // Activate (IsActive change: 0 → 1)
            NewMachineId = null,  // Required for ShouldUpdateIsActiveOnly()
            NewPlcId = null       // Required for ShouldUpdateIsActiveOnly()
        };

        // Issue #118 (Chunk A): the handler resolves the row through a composite-key specification;
        // the stub compiles the captured criteria against sample data, mirroring the SQL-side filter.
        _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<MachinePlc>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => FindFirst(new[] { existingMachinePlc }, callInfo.Arg<ISpecification<MachinePlc>>()));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.MachineId.ShouldBe(command.MachineId);
        result.Value.PlcId.ShouldBe(command.PlcId);

        await _repository.Received(1).FirstOrDefaultAsync(Arg.Any<ISpecification<MachinePlc>>(), Arg.Any<CancellationToken>());

        // #95 Slice C: ONE atomic aggregate save carrying exactly the staged IsActive update.
        _saves.Count.ShouldBe(1);
        _saves[0].MachineId.ShouldBe(10000);
        _saves[0].Appends.ShouldBeEmpty();
        _saves[0].Updates.Count.ShouldBe(1);
        _saves[0].Updates[0].ShouldBeSameAs(existingMachinePlc);
        existingMachinePlc.IsActive.ShouldBe(ActiveStatus.Active);
    }

    /// <summary>
    /// Executes Process_WhenEntityNotFound_ShouldReturnFailure operation.
    /// </summary>
    /// <returns>The result of Process_WhenEntityNotFound_ShouldReturnFailure.</returns>

    [Fact]
    public async Task Process_WhenEntityNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new UpdateMachinePlcCommand { MachineId = 10000, PlcId = 200 };
        // Issue #118 (Chunk A): the real repository signals "no rows matched" with the sentinel failure.
        _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<MachinePlc>>(), Arg.Any<CancellationToken>())
            .Returns(Result<MachinePlc?>.WithFailure("No matching entity found."));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain($"MachinePLC with MachineId: {command.MachineId} and PlcId: {command.PlcId} cannot be found");
    }

    /// <summary>
    /// Executes Process_WhenGetByIdFails_ShouldReturnFailure operation.
    /// </summary>
    /// <returns>The result of Process_WhenGetByIdFails_ShouldReturnFailure.</returns>

    [Fact]
    public async Task Process_WhenGetByIdFails_ShouldReturnFailure()
    {
        // Arrange
        var command = new UpdateMachinePlcCommand { MachineId = 10000, PlcId = 200 };
        // Issue #118 (Chunk A): a non-sentinel failure is a genuine repository error and must propagate.
        _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<MachinePlc>>(), Arg.Any<CancellationToken>())
            .Returns(Result<MachinePlc?>.WithFailure("Entity not found in database"));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Entity not found in database");
    }

    /// <summary>
    /// Executes Process_WhenUpdateFails_ShouldReturnFailure operation (now: the atomic aggregate SAVE of
    /// the IsActive update fails).
    /// </summary>
    /// <returns>The result of Process_WhenUpdateFails_ShouldReturnFailure.</returns>

    [Fact]
    public async Task Process_WhenUpdateFails_ShouldReturnFailure()
    {
        // Arrange
        var existingMachinePlc = MachinePlc.CreateFixture(10000, 200, 1);
        var command = new UpdateMachinePlcCommand { MachineId = 10000, PlcId = 200, IsActive = 0 }; // Different IsActive to trigger update

        _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<MachinePlc>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => FindFirst(new[] { existingMachinePlc }, callInfo.Arg<ISpecification<MachinePlc>>()));
        _saveBehavior = (_, _) => Result.WithFailure("Database update failed");

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Database update failed");
    }

    /// <summary>
    /// #95 Slice C (supersedes the #113 F7 no-op-commit pin, which is structurally impossible now — the
    /// injected read repository has no commit surface at all): an IsActive-only update is persisted through
    /// exactly ONE atomic aggregate save.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task Process_IsActiveOnly_ShouldPersistThroughSingleAggregateSave()
    {
        // Arrange
        var existingMachinePlc = MachinePlc.CreateFixture(10000, 200, 1);
        var command = new UpdateMachinePlcCommand
        {
            MachineId = 10000,
            PlcId = 200,
            IsActive = 0  // Different from existing entity's IsActive = 1 to trigger update path
        };

        _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<MachinePlc>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => FindFirst(new[] { existingMachinePlc }, callInfo.Arg<ISpecification<MachinePlc>>()));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert — the update succeeds through a single aggregate save.
        result.IsSuccess.ShouldBeTrue();
        _saves.Count.ShouldBe(1);
        await _machineAggregateRepository.Received(1).SaveAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Process_ShouldPassCancellationTokenToRepository operation.
    /// </summary>
    /// <returns>The result of Process_ShouldPassCancellationTokenToRepository.</returns>

    [Fact]
    public async Task Process_ShouldPassCancellationTokenToRepository()
    {
        // Arrange
        var existingMachinePlc = MachinePlc.CreateFixture(10000, 200, 1);
        var command = new UpdateMachinePlcCommand { MachineId = 10000, PlcId = 200, IsActive = 0 };
        var cancellationToken = TestContext.Current.CancellationToken;

        _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<MachinePlc>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => FindFirst(new[] { existingMachinePlc }, callInfo.Arg<ISpecification<MachinePlc>>()));

        // Act
        var result = await _handler.ProcessAsync(command, cancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await _repository.Received(1).FirstOrDefaultAsync(Arg.Any<ISpecification<MachinePlc>>(), cancellationToken);
        await _machineAggregateRepository.Received(1).LoadAsync(command.MachineId, Arg.Any<AggregateLoadOptions>(), cancellationToken);
        await _machineAggregateRepository.Received(1).SaveAsync(Arg.Any<Machine>(), cancellationToken);
    }

    /// <summary>
    /// #95 Slice C NEW PIN: a SAME-MACHINE key change (only PlcId changes) is ONE atomic aggregate save —
    /// the new mapping's append and the old mapping's deactivation travel in the same staged batch. The
    /// former two-write window (and its compensation) no longer exists on this path.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task Process_SameMachineKeyChange_ShouldPersistBothRowsInOneAtomicSave()
    {
        // Arrange
        var existingMachinePlc = MachinePlc.CreateFixture(10000, 200, 1); // Active
        var command = new UpdateMachinePlcCommand { MachineId = 10000, PlcId = 200, NewPlcId = 300 };

        _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<MachinePlc>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => FindFirst(new[] { existingMachinePlc }, callInfo.Arg<ISpecification<MachinePlc>>()));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert — one save carrying BOTH the new active mapping and the old mapping's deactivation.
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.PlcId.ShouldBe(300);

        _saves.Count.ShouldBe(1);
        _saves[0].MachineId.ShouldBe(10000);
        _saves[0].Appends.Count.ShouldBe(1);
        _saves[0].Appends[0].PlcId.ShouldBe(300);
        _saves[0].Appends[0].IsActive.ShouldBe(ActiveStatus.Active);
        _saves[0].Updates.Count.ShouldBe(1);
        _saves[0].Updates[0].ShouldBeSameAs(existingMachinePlc);
        existingMachinePlc.IsActive.ShouldBe(ActiveStatus.Inactive);
    }

    /// <summary>
    /// #95 Slice C FLIPPED PIN (was: #113 F7 insert-first leaves the old mapping active): the SAME-MACHINE
    /// key change is now one atomic save, so a failure (e.g. the new composite key already exists) is a
    /// SINGLE failed save attempt — the store is untouched by contract, the failure propagates, and NO
    /// compensation save ever runs.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task Process_KeyChange_WhenInsertFails_ShouldLeaveOldMappingActiveAndFail()
    {
        // Arrange
        var existingMachinePlc = MachinePlc.CreateFixture(10000, 200, 1); // Active
        var command = new UpdateMachinePlcCommand { MachineId = 10000, PlcId = 200, NewPlcId = 300 };

        _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<MachinePlc>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => FindFirst(new[] { existingMachinePlc }, callInfo.Arg<ISpecification<MachinePlc>>()));
        _saveBehavior = (_, _) => Result.WithFailure("Violation of PRIMARY KEY constraint 'PK.MachinePlcs'");

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert — the atomic save failure propagates; exactly one attempt, no compensation.
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Violation of PRIMARY KEY constraint 'PK.MachinePlcs'");
        _saves.Count.ShouldBe(1);
    }

    /// <summary>
    /// #113 F7 compensation, now scoped to CROSS-MACHINE moves (#95 Slice C — the only path that still spans
    /// two aggregate saves): when the new mapping is inserted on the target machine but deactivating the OLD
    /// mapping fails, the handler compensates by rolling the new mapping back to inactive through the target
    /// root and fails loud, so the plant is never left in a silently half-changed state.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task Process_KeyChange_WhenDeactivateOldFails_ShouldRollBackNewMappingAndFail()
    {
        // Arrange
        var existingMachinePlc = MachinePlc.CreateFixture(10000, 200, 1); // Active
        var command = new UpdateMachinePlcCommand { MachineId = 10000, PlcId = 200, NewMachineId = 20000, NewPlcId = 300 };

        _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<MachinePlc>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => FindFirst(new[] { existingMachinePlc }, callInfo.Arg<ISpecification<MachinePlc>>()));

        // Deactivating the OLD row (root 10000) fails; the saves on the TARGET root (20000) succeed.
        _saveBehavior = (root, _) => root.MachineId.Value == 10000
            ? Result.WithFailure("Deadlock victim")
            : Result.Success();

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert — loud failure carrying the original error and naming the compensation.
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("Deadlock victim", StringComparison.Ordinal));
        result.Errors.ShouldContain(e => e.Contains("rolled back", StringComparison.OrdinalIgnoreCase));

        // Insert-first ordering: save 1 = insert on target, save 2 = failed deactivate on old root,
        // save 3 = compensating rollback of the new mapping on the target root.
        _saves.Count.ShouldBe(3);
        _saves[0].MachineId.ShouldBe(20000);
        _saves[0].Appends.Count.ShouldBe(1);
        var insertedMapping = _saves[0].Appends[0];
        insertedMapping.PlcId.ShouldBe(300);
        _saves[1].MachineId.ShouldBe(10000);
        _saves[2].MachineId.ShouldBe(20000);
        _saves[2].Updates.Count.ShouldBe(1);
        _saves[2].Updates[0].ShouldBeSameAs(insertedMapping);

        // The compensating write set the just-inserted mapping back to inactive.
        insertedMapping.IsActive.ShouldBe(ActiveStatus.Inactive);
    }

    /// <summary>
    /// #126 adversarial review C5, now scoped to CROSS-MACHINE moves: when the flow fails after the insert
    /// BECAUSE the request token was cancelled mid-flight, the compensating rollback must still run to
    /// completion — it must use <see cref="CancellationToken.None"/> (the sibling compensation idiom, e.g.
    /// CreateProductCommandHandler.CompensateAsync), never the already-cancelled request token, which would
    /// guarantee the rollback also fails and strand the plant with TWO active PLC mappings.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task Process_KeyChange_WhenRequestTokenCancelledAfterInsert_CompensationStillRollsBack()
    {
        // Arrange
        var existingMachinePlc = MachinePlc.CreateFixture(10000, 200, 1); // Active
        var command = new UpdateMachinePlcCommand { MachineId = 10000, PlcId = 200, NewMachineId = 20000, NewPlcId = 300 };
        using var cts = new CancellationTokenSource();

        _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<MachinePlc>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => FindFirst(new[] { existingMachinePlc }, callInfo.Arg<ISpecification<MachinePlc>>()));

        // The cancellation arrives right after the new mapping is saved on the target root; every later
        // operation carrying the request token fails (the LoadAsync mock honors cancellation), while the
        // compensation runs on CancellationToken.None and succeeds.
        _saveBehavior = (_, token) =>
        {
            if (token.IsCancellationRequested)
            {
                return Result.WithFailure("Operation was canceled.");
            }

            cts.Cancel();
            return Result.Success();
        };

        // Act
        var result = await _handler.ProcessAsync(command, cts.Token);

        // Assert - loud failure, but the compensation SUCCEEDED: the new mapping was rolled back to
        // inactive with a non-cancelled token, so the plant never keeps two active mappings.
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("rolled back", StringComparison.OrdinalIgnoreCase));

        _saves[0].MachineId.ShouldBe(20000);
        var insertedMapping = _saves[0].Appends[0];
        insertedMapping.PlcId.ShouldBe(300);
        insertedMapping.IsActive.ShouldBe(ActiveStatus.Inactive);
        await _machineAggregateRepository.Received(1).SaveAsync(
            Arg.Is<Machine>(m => m.MachineId.Value == 20000),
            CancellationToken.None);
    }

    /// <summary>
    /// #113 F7, now scoped to CROSS-MACHINE moves: when deactivating the old mapping fails AND the
    /// compensating rollback of the new mapping also fails, there are TWO active PLC mappings — the handler
    /// must say so explicitly and demand manual intervention, never fail silently.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task Process_KeyChange_WhenDeactivateAndCompensationBothFail_ShouldDemandManualIntervention()
    {
        // Arrange
        var existingMachinePlc = MachinePlc.CreateFixture(10000, 200, 1); // Active
        var command = new UpdateMachinePlcCommand { MachineId = 10000, PlcId = 200, NewMachineId = 20000, NewPlcId = 300 };

        // The very first save (the insert on the target root) succeeds; every later save fails.
        var saveCount = 0;
        _saveBehavior = (_, _) => ++saveCount == 1
            ? Result.Success()
            : Result.WithFailure("Connection lost");

        _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<MachinePlc>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => FindFirst(new[] { existingMachinePlc }, callInfo.Arg<ISpecification<MachinePlc>>()));

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert — both failures surface and the message demands manual intervention.
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("Connection lost", StringComparison.Ordinal));
        result.Errors.ShouldContain(e => e.Contains("manual intervention", StringComparison.OrdinalIgnoreCase));
        result.Errors.ShouldContain(e => e.Contains("TWO active", StringComparison.OrdinalIgnoreCase));
    }
}
