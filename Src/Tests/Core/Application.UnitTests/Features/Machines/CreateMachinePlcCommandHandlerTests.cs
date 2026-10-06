// <copyright file="CreateMachinePlcCommandHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Abstractions.Aggregates;
using IndTrace.Application.MachinesPlcs.Commands.Create;

namespace Application.UnitTests.Features.Machines
{
    /// <summary>
    /// Unit tests for CreateMachinePlcCommandHandler.
    /// #95 Phase 2 Slice C: MachinePlc is a member of the Machine aggregate — the handler loads the Machine
    /// root, stages the new mapping on it, and persists through IAggregateRepository&lt;Machine&gt;.SaveAsync
    /// (one explicit transaction). The mocks mirror that contract.
    /// </summary>
    public class CreateMachinePlcCommandHandlerTests
    {
        private readonly IAggregateRepository<Machine> _machineAggregateRepository = null!;
        private readonly IRepository<Machine> _machineRepository = null!;
        private readonly IRepository<Plc> _plcRepository = null!;
        private readonly ILogger<CreateMachinePlcCommandHandler> _logger = null!;
        private readonly CreateMachinePlcCommandHandler _handler = null!;

        // The Machine root the aggregate repository "loads"; staged sets are inspected on this instance.
        private readonly Machine _machineRoot = new() { MachineId = new MachineId(10000) };
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>

        public CreateMachinePlcCommandHandlerTests()
        {
            _machineAggregateRepository = Substitute.For<IAggregateRepository<Machine>>();
            _machineRepository = Substitute.For<IRepository<Machine>>();
            _plcRepository = Substitute.For<IRepository<Plc>>();
            // #97: by default both FK parents exist so the fail-closed probes pass; dangling-parent tests override.
            _machineRepository.CountAsync(Arg.Any<ISpecification<Machine>>(), Arg.Any<CancellationToken>())
                .Returns(Result<int>.Success(1));
            _plcRepository.CountAsync(Arg.Any<ISpecification<Plc>>(), Arg.Any<CancellationToken>())
                .Returns(Result<int>.Success(1));
            // #95 Slice C: by default the root loads and the aggregate save succeeds; failure tests override.
            _machineAggregateRepository.LoadAsync(Arg.Any<int>(), Arg.Any<AggregateLoadOptions>(), Arg.Any<CancellationToken>())
                .Returns(Result<Machine>.Success(_machineRoot));
            _machineAggregateRepository.SaveAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
                .Returns(Result.Success());
            _logger = XUnitLogger.CreateLogger<CreateMachinePlcCommandHandler>();
            _handler = new CreateMachinePlcCommandHandler(_machineAggregateRepository, _machineRepository, _plcRepository, _logger);
        }
        /// <summary>
        /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
        /// </summary>

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateInstance()
        {
            // Arrange & Act
            var handler = new CreateMachinePlcCommandHandler(_machineAggregateRepository, _machineRepository, _plcRepository, _logger);

            // Assert
            handler.ShouldNotBeNull();
        }

        /// <summary>
        /// #97 red-green: a positive-but-dangling MachineId (no parent row -> CountAsync Success(0)) must be
        /// refused gracefully and must NOT reach the aggregate load/save. Pre-fix code proceeded to persist
        /// (SqlException 547 on real SQL).
        /// </summary>
        /// <returns>The result of Process_WithDanglingMachineId_ShouldFailAndNotPersist.</returns>
        [Fact]
        public async Task Process_WithDanglingMachineId_ShouldFailAndNotPersist()
        {
            // Arrange
            var command = CreateValidCommand();
            _machineRepository.CountAsync(Arg.Any<ISpecification<Machine>>(), Arg.Any<CancellationToken>())
                .Returns(Result<int>.Success(0));

            // Act
            var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Errors.ShouldContain($"Machine with id {command.MachineId} does not exist.");
            await _machineAggregateRepository.DidNotReceive().LoadAsync(Arg.Any<int>(), Arg.Any<AggregateLoadOptions>(), Arg.Any<CancellationToken>());
            await _machineAggregateRepository.DidNotReceive().SaveAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>());
        }

        /// <summary>
        /// #97 red-green: a positive-but-dangling PlCsId (no parent Plc row -> CountAsync Success(0)) must be
        /// refused gracefully and must NOT reach the aggregate load/save.
        /// </summary>
        /// <returns>The result of Process_WithDanglingPlcId_ShouldFailAndNotPersist.</returns>
        [Fact]
        public async Task Process_WithDanglingPlcId_ShouldFailAndNotPersist()
        {
            // Arrange
            var command = CreateValidCommand();
            _plcRepository.CountAsync(Arg.Any<ISpecification<Plc>>(), Arg.Any<CancellationToken>())
                .Returns(Result<int>.Success(0));

            // Act
            var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Errors.ShouldContain($"Plc with id {command.PlCsId} does not exist.");
            await _machineAggregateRepository.DidNotReceive().LoadAsync(Arg.Any<int>(), Arg.Any<AggregateLoadOptions>(), Arg.Any<CancellationToken>());
            await _machineAggregateRepository.DidNotReceive().SaveAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>());
        }

        /// <summary>
        /// Executes Process_WithValidCommand_ShouldReturnSuccess operation.
        /// </summary>
        /// <returns>The result of Process_WithValidCommand_ShouldReturnSuccess.</returns>

        [Fact]
        public async Task Process_WithValidCommand_ShouldReturnSuccess()
        {
            // Arrange
            var command = CreateValidCommand();

            // Act
            var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value.MachineId.ShouldBe(command.MachineId);
            result.Value.PlCsId.ShouldBe(command.PlCsId);

            // The mapping was staged on the loaded root and persisted through ONE aggregate save.
            _machineRoot.PendingMachinePlcAppends.Count.ShouldBe(1);
            _machineRoot.PendingMachinePlcAppends[0].PlcId.ShouldBe(command.PlCsId);
            await _machineAggregateRepository.Received(1).LoadAsync(command.MachineId, Arg.Any<AggregateLoadOptions>(), Arg.Any<CancellationToken>());
            await _machineAggregateRepository.Received(1).SaveAsync(_machineRoot, Arg.Any<CancellationToken>());
        }
        /// <summary>
        /// Executes Process_WhenAddFails_ShouldReturnFailure operation (now: the aggregate LOAD fails — the
        /// first write-path step, mirroring the former AddAsync failure point).
        /// </summary>
        /// <returns>The result of Process_WhenAddFails_ShouldReturnFailure.</returns>

        [Fact]
        public async Task Process_WhenAddFails_ShouldReturnFailure()
        {
            // Arrange
            var command = CreateValidCommand();

            _machineAggregateRepository.LoadAsync(Arg.Any<int>(), Arg.Any<AggregateLoadOptions>(), Arg.Any<CancellationToken>())
                .Returns(Result<Machine>.WithFailure("Database connection failed"));

            // Act
            var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Errors.ShouldContain("Database connection failed");
        }
        /// <summary>
        /// Executes Process_WhenCommitFails_ShouldReturnFailure operation (now: the atomic aggregate SAVE fails).
        /// </summary>
        /// <returns>The result of Process_WhenCommitFails_ShouldReturnFailure.</returns>

        [Fact]
        public async Task Process_WhenCommitFails_ShouldReturnFailure()
        {
            // Arrange
            var command = CreateValidCommand();

            _machineAggregateRepository.SaveAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
                .Returns(Result.WithFailure("Transaction commit failed"));

            // Act
            var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Errors.ShouldContain("Transaction commit failed");
        }
        /// <summary>
        /// Executes Process_ShouldPassCancellationTokenToRepository operation.
        /// </summary>
        /// <returns>The result of Process_ShouldPassCancellationTokenToRepository.</returns>

        [Fact]
        public async Task Process_ShouldPassCancellationTokenToRepository()
        {
            // Arrange
            var command = CreateValidCommand();
            var cancellationToken = TestContext.Current.CancellationToken;

            // Act
            await _handler.ProcessAsync(command, cancellationToken);

            // Assert
            await _machineAggregateRepository.Received(1).LoadAsync(command.MachineId, Arg.Any<AggregateLoadOptions>(), cancellationToken);
            await _machineAggregateRepository.Received(1).SaveAsync(_machineRoot, cancellationToken);
        }

        private static CreateMachinePlcCommand CreateValidCommand()
        {
            return new CreateMachinePlcCommand
            {
                MachineId = 10000,
                PlCsId = 200
            };
        }
    }
}
