// <copyright file="DistinctRegisterServiceTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Registers;

/// <summary>
/// Unit tests for DistinctRegisterService
/// </summary>
public class DistinctRegisterServiceTests
{
    private readonly IRepository<DistinctRegister> _distinctRegisterRepository = Substitute.For<IRepository<DistinctRegister>>();
    private readonly IReadOnlyRepository<IndTrace.Domain.Entities.Register> _registerRepository = Substitute.For<IReadOnlyRepository<IndTrace.Domain.Entities.Register>>();

    /// <summary>
    /// Wires the register ledger queryable and the persisted catalog list, with succeeding writes.
    /// </summary>
    /// <param name="ledgerRows">Rows standing in for the append-only Registers ledger.</param>
    /// <param name="persistedCatalog">Rows standing in for the persisted DistinctRegisters catalog.</param>
    private void SetupRepositories(IEnumerable<IndTrace.Domain.Entities.Register> ledgerRows, IEnumerable<DistinctRegister> persistedCatalog)
    {
        _registerRepository
            .AsQueryableAsync(Arg.Any<ISpecification<IndTrace.Domain.Entities.Register>>(), Arg.Any<CancellationToken>())
            .Returns(Result<OwnedQueryable<IndTrace.Domain.Entities.Register>>.Success(
                new OwnedQueryable<IndTrace.Domain.Entities.Register>(ledgerRows.AsQueryable(), null)));

        _distinctRegisterRepository
            .ListAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<DistinctRegister>>.Success(persistedCatalog.ToList()));

        _distinctRegisterRepository
            .DeleteAsync(Arg.Any<DistinctRegister>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        _distinctRegisterRepository
            .AddAsync(Arg.Any<DistinctRegister>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(1));
    }

    /// <summary>
    /// Issue #119 (F4): the sync must be an incremental diff. When the persisted catalog already
    /// matches the ledger, NO delete and NO add may be issued — the pre-fix clear-then-rebuild
    /// deleted every row (readers saw an empty catalog mid-rebuild) and re-inserted it.
    /// </summary>
    /// <returns>The result of the assertion task.</returns>
    [Fact]
    public async Task UpdateDistinctRegistersAsync_WhenCatalogUnchanged_IssuesNoWrites()
    {
        // Arrange — ledger and persisted catalog agree on the same two {Name, VariableId, MachineId} triples.
        var ledgerRows = new List<IndTrace.Domain.Entities.Register>
        {
            IndTrace.Domain.Entities.Register.CreateFixture(name: "PartStatusPlc", machineId: 100, variableId: 1),
            IndTrace.Domain.Entities.Register.CreateFixture(name: "CycleStatusPlc", machineId: 400, variableId: 2),
        };
        var persistedCatalog = new List<DistinctRegister>
        {
            new() { Name = "PartStatusPlc", VariableId = 1, MachineId = 100 },
            new() { Name = "CycleStatusPlc", VariableId = 2, MachineId = 400 },
        };
        SetupRepositories(ledgerRows, persistedCatalog);

        var instance = new DistinctRegisterService(_distinctRegisterRepository, _registerRepository);

        // Act
        var result = await instance.UpdateDistinctRegistersAsync(CancellationToken.None);

        // Assert — a reader can never observe an emptied catalog because unchanged rows are untouched.
        result.IsSuccess.ShouldBeTrue();
        await _distinctRegisterRepository.DidNotReceiveWithAnyArgs().DeleteAsync(Arg.Any<DistinctRegister>(), Arg.Any<CancellationToken>());
        await _distinctRegisterRepository.DidNotReceiveWithAnyArgs().AddAsync(Arg.Any<DistinctRegister>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Issue #119 (F4): the diff must delete ONLY triples that vanished from the ledger and add
    /// ONLY new ones; still-present triples are never touched.
    /// </summary>
    /// <returns>The result of the assertion task.</returns>
    [Fact]
    public async Task UpdateDistinctRegistersAsync_WithStaleAndNewRows_DeletesOnlyStaleAndAddsOnlyNew()
    {
        // Arrange — persisted has {Kept, Stale}; ledger has {Kept, New}.
        var ledgerRows = new List<IndTrace.Domain.Entities.Register>
        {
            IndTrace.Domain.Entities.Register.CreateFixture(name: "Kept", machineId: 100, variableId: 1),
            IndTrace.Domain.Entities.Register.CreateFixture(name: "New", machineId: 400, variableId: 3),
        };
        var keptRow = new DistinctRegister { Name = "Kept", VariableId = 1, MachineId = 100 };
        var staleRow = new DistinctRegister { Name = "Stale", VariableId = 2, MachineId = 200 };
        SetupRepositories(ledgerRows, [keptRow, staleRow]);

        var instance = new DistinctRegisterService(_distinctRegisterRepository, _registerRepository);

        // Act
        var result = await instance.UpdateDistinctRegistersAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        await _distinctRegisterRepository.Received(1).DeleteAsync(
            Arg.Is<DistinctRegister>(r => r.Name == "Stale" && r.VariableId == 2 && r.MachineId == 200),
            Arg.Any<CancellationToken>());
        await _distinctRegisterRepository.DidNotReceive().DeleteAsync(
            Arg.Is<DistinctRegister>(r => r.Name == "Kept"),
            Arg.Any<CancellationToken>());

        await _distinctRegisterRepository.Received(1).AddAsync(
            Arg.Is<DistinctRegister>(r => r.Name == "New" && r.VariableId == 3 && r.MachineId == 400),
            Arg.Any<CancellationToken>());
        await _distinctRegisterRepository.DidNotReceive().AddAsync(
            Arg.Is<DistinctRegister>(r => r.Name == "Kept"),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Issue #119 (F4): write Results must be honored — a failed add surfaces as a failure Result
    /// naming the failed composite key instead of being silently discarded.
    /// </summary>
    /// <returns>The result of the assertion task.</returns>
    [Fact]
    public async Task UpdateDistinctRegistersAsync_WhenAddFails_ReturnsFailureNamingKey()
    {
        // Arrange — one new triple whose insert fails.
        var ledgerRows = new List<IndTrace.Domain.Entities.Register>
        {
            IndTrace.Domain.Entities.Register.CreateFixture(name: "New", machineId: 400, variableId: 3),
        };
        SetupRepositories(ledgerRows, []);

        _distinctRegisterRepository
            .AddAsync(Arg.Any<DistinctRegister>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.WithFailure(["Insert rejected."]));

        var instance = new DistinctRegisterService(_distinctRegisterRepository, _registerRepository);

        // Act
        var result = await instance.UpdateDistinctRegistersAsync(CancellationToken.None);

        // Assert — the failure names the key so operators can locate the divergent triple.
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(error => error.Contains("New") && error.Contains("Insert rejected."));
    }

    /// <summary>
    /// Issue #119 (F4): a failed delete is likewise reported per key, and the sync still attempts
    /// the remaining writes rather than aborting half-way.
    /// </summary>
    /// <returns>The result of the assertion task.</returns>
    [Fact]
    public async Task UpdateDistinctRegistersAsync_WhenDeleteFails_ReturnsFailureNamingKeyAndStillAdds()
    {
        // Arrange — one stale triple whose delete fails, one new triple that inserts fine.
        var ledgerRows = new List<IndTrace.Domain.Entities.Register>
        {
            IndTrace.Domain.Entities.Register.CreateFixture(name: "New", machineId: 400, variableId: 3),
        };
        var staleRow = new DistinctRegister { Name = "Stale", VariableId = 2, MachineId = 200 };
        SetupRepositories(ledgerRows, [staleRow]);

        _distinctRegisterRepository
            .DeleteAsync(Arg.Any<DistinctRegister>(), Arg.Any<CancellationToken>())
            .Returns(Result.WithFailure(["Delete rejected."]));

        var instance = new DistinctRegisterService(_distinctRegisterRepository, _registerRepository);

        // Act
        var result = await instance.UpdateDistinctRegistersAsync(CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(error => error.Contains("Stale") && error.Contains("Delete rejected."));

        await _distinctRegisterRepository.Received(1).AddAsync(
            Arg.Is<DistinctRegister>(r => r.Name == "New"),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// AsQueryableAsync defers database access to enumeration, so a dead database surfaces as an
    /// exception when the service materializes the query. Issue #119 (F4): that fault must surface
    /// as a FAILURE Result — pre-fix it degraded to a silent empty catalog, indistinguishable from
    /// "no registers exist" on the Metrics page.
    /// </summary>
    /// <returns>The result of the assertion task.</returns>
    [Fact]
    public async Task GetDistinctRegistersAsync_WhenEnumerationThrows_ShouldReturnFailureInsteadOfEmpty()
    {
        // Arrange
        var throwingQueryable = new ThrowingEnumerable().AsQueryable();
        _registerRepository
            .AsQueryableAsync(Arg.Any<ISpecification<IndTrace.Domain.Entities.Register>>(), Arg.Any<CancellationToken>())
            .Returns(Result<OwnedQueryable<IndTrace.Domain.Entities.Register>>.Success(
                new OwnedQueryable<IndTrace.Domain.Entities.Register>(throwingQueryable, null)));
        var instance = new DistinctRegisterService(_distinctRegisterRepository, _registerRepository);

        // Act
        var result = await instance.GetDistinctRegistersAsync(CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(error => error.Contains("Simulated deferred database failure at enumeration."));
    }

    /// <summary>
    /// Issue #119 (F4): a failed queryable lease propagates its repository errors.
    /// </summary>
    /// <returns>The result of the assertion task.</returns>
    [Fact]
    public async Task GetDistinctRegistersAsync_WhenLeaseFails_PropagatesRepositoryFailure()
    {
        // Arrange
        _registerRepository
            .AsQueryableAsync(Arg.Any<ISpecification<IndTrace.Domain.Entities.Register>>(), Arg.Any<CancellationToken>())
            .Returns(Result<OwnedQueryable<IndTrace.Domain.Entities.Register>>.WithFailure(["Registers store unreachable."]));
        var instance = new DistinctRegisterService(_distinctRegisterRepository, _registerRepository);

        // Act
        var result = await instance.GetDistinctRegistersAsync(CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Registers store unreachable.");
    }

    private sealed class ThrowingEnumerable : IEnumerable<IndTrace.Domain.Entities.Register>
    {
        public IEnumerator<IndTrace.Domain.Entities.Register> GetEnumerator() =>
            throw new InvalidOperationException("Simulated deferred database failure at enumeration.");

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
