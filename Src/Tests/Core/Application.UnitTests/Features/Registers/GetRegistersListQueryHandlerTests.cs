// <copyright file="GetRegistersListQueryHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Registers;

/// <summary>
/// Unit tests for GetRegistersListQueryHandler
/// </summary>
public class GetRegistersListQueryHandlerTests
{
    private readonly IRepository<IndTrace.Domain.Entities.Variable> _mockrepositoryVariables = Substitute.For<IRepository<IndTrace.Domain.Entities.Variable>>();
    private readonly IReadOnlyRepository<IndTrace.Domain.Entities.Register> _mockRepositoryRegisters = Substitute.For<IReadOnlyRepository<IndTrace.Domain.Entities.Register>>();

    /// <summary>
    /// Builds a query that resolves variable ids from register names, which forces the handler
    /// through the name-lookup path over the append-only Registers ledger.
    /// </summary>
    /// <returns>A names-based query.</returns>
    private static GetRegistersListQuery CreateNamesQuery() => new()
    {
        RegistersName = new List<string> { "PartStatusPlc" },
        VariablesId = new List<int>(),
        MachineId = new List<int> { 100 },
        StartDate = DateTime.UtcNow.AddDays(-7),
        EndDate = DateTime.UtcNow,
    };

    /// <summary>
    /// Executes Properties_WhenSet_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSet_ShouldReturnCorrectValues()
    {
        // Arrange
        var instance = new GetRegistersListQueryHandler(_mockrepositoryVariables, _mockRepositoryRegisters);

        // Act & Assert
        instance.ShouldNotBeNull();
    }

    /// <summary>
    /// Issue #119 (F1): a repository fault while resolving variable ids from register names must
    /// surface as a FAILURE Result carrying the repository's errors — not be coerced to an empty
    /// id set that the Monitor Metrics page reads as "no data".
    /// </summary>
    /// <returns>The result of the assertion task.</returns>
    [Fact]
    public async Task ProcessAsync_WhenRegisterIdLookupFails_PropagatesRepositoryFailure()
    {
        // Arrange
        const string RepositoryError = "Registers store unreachable.";

        _mockRepositoryRegisters
            .AsQueryableAsync(Arg.Any<ISpecification<IndTrace.Domain.Entities.Register>>(), Arg.Any<CancellationToken>())
            .Returns(Result<OwnedQueryable<IndTrace.Domain.Entities.Register>>.WithFailure([RepositoryError]));

        // Pre-fix path used ListAsync for the same lookup — fail it with the same error so the
        // test discriminates propagation (expected) from empty-coercion (the defect).
        _mockRepositoryRegisters
            .ListAsync(Arg.Any<ISpecification<IndTrace.Domain.Entities.Register>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<IndTrace.Domain.Entities.Register>>.WithFailure([RepositoryError]));

        _mockrepositoryVariables
            .ListAsync(Arg.Any<ISpecification<IndTrace.Domain.Entities.Variable>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<IndTrace.Domain.Entities.Variable>>.Success(new List<IndTrace.Domain.Entities.Variable>()));

        var handler = new GetRegistersListQueryHandler(_mockrepositoryVariables, _mockRepositoryRegisters);

        // Act
        var result = await handler.ProcessAsync(CreateNamesQuery(), TestContext.Current.CancellationToken);

        // Assert — fail loud with the repository's own error, not a misleading "no ids" message.
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(RepositoryError);
    }

    /// <summary>
    /// Issue #119 (F1): a Variables repository fault during the name lookup must also propagate
    /// as a failure Result instead of degrading to an empty id list.
    /// </summary>
    /// <returns>The result of the assertion task.</returns>
    [Fact]
    public async Task ProcessAsync_WhenVariableLookupFails_PropagatesRepositoryFailure()
    {
        // Arrange
        const string RepositoryError = "Variables store unreachable.";

        _mockRepositoryRegisters
            .AsQueryableAsync(Arg.Any<ISpecification<IndTrace.Domain.Entities.Register>>(), Arg.Any<CancellationToken>())
            .Returns(Result<OwnedQueryable<IndTrace.Domain.Entities.Register>>.Success(
                new OwnedQueryable<IndTrace.Domain.Entities.Register>(new List<IndTrace.Domain.Entities.Register>().AsQueryable(), null)));

        _mockRepositoryRegisters
            .ListAsync(Arg.Any<ISpecification<IndTrace.Domain.Entities.Register>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<IndTrace.Domain.Entities.Register>>.Success(new List<IndTrace.Domain.Entities.Register>()));

        _mockrepositoryVariables
            .ListAsync(Arg.Any<ISpecification<IndTrace.Domain.Entities.Variable>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<IndTrace.Domain.Entities.Variable>>.WithFailure([RepositoryError]));

        var handler = new GetRegistersListQueryHandler(_mockrepositoryVariables, _mockRepositoryRegisters);

        // Act
        var result = await handler.ProcessAsync(CreateNamesQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(RepositoryError);
    }

    /// <summary>
    /// Issue #119 (F1): the name lookup must resolve variable ids through a server-side
    /// projection over an <see cref="OwnedQueryable{T}"/> lease — a single ListAsync remains
    /// (the final register fetch) instead of the pre-fix two full-entity materializations.
    /// </summary>
    /// <returns>The result of the assertion task.</returns>
    [Fact]
    public async Task ProcessAsync_WithRegisterNames_ProjectsIdsInsteadOfMaterializingEntities()
    {
        // Arrange — two ledger rows sharing VariableId 7: projection must dedupe server-side.
        var ledgerRows = new List<IndTrace.Domain.Entities.Register>
        {
            IndTrace.Domain.Entities.Register.CreateFixture(name: "PartStatusPlc", machineId: 100, variableId: 7, value: "1", dataType: "int", timeStamp: DateTime.UtcNow),
            IndTrace.Domain.Entities.Register.CreateFixture(name: "PartStatusPlc", machineId: 100, variableId: 7, value: "2", dataType: "int", timeStamp: DateTime.UtcNow),
        };

        _mockRepositoryRegisters
            .AsQueryableAsync(Arg.Any<ISpecification<IndTrace.Domain.Entities.Register>>(), Arg.Any<CancellationToken>())
            .Returns(Result<OwnedQueryable<IndTrace.Domain.Entities.Register>>.Success(
                new OwnedQueryable<IndTrace.Domain.Entities.Register>(ledgerRows.AsQueryable(), null)));

        _mockRepositoryRegisters
            .ListAsync(Arg.Any<ISpecification<IndTrace.Domain.Entities.Register>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<IndTrace.Domain.Entities.Register>>.Success(new List<IndTrace.Domain.Entities.Register> { ledgerRows[0] }));

        _mockrepositoryVariables
            .ListAsync(Arg.Any<ISpecification<IndTrace.Domain.Entities.Variable>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<IndTrace.Domain.Entities.Variable>>.Success(new List<IndTrace.Domain.Entities.Variable>()));

        var handler = new GetRegistersListQueryHandler(_mockrepositoryVariables, _mockRepositoryRegisters);

        // Act
        var result = await handler.ProcessAsync(CreateNamesQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count().ShouldBe(1);

        // The id lookup goes through the projection lease; ListAsync fires exactly once (final fetch).
        await _mockRepositoryRegisters.Received(1)
            .AsQueryableAsync(Arg.Any<ISpecification<IndTrace.Domain.Entities.Register>>(), Arg.Any<CancellationToken>());
        await _mockRepositoryRegisters.Received(1)
            .ListAsync(Arg.Any<ISpecification<IndTrace.Domain.Entities.Register>>(), Arg.Any<CancellationToken>());
    }
}
