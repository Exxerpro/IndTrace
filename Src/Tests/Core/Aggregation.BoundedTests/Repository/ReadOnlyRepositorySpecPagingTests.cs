// <copyright file="ReadOnlyRepositorySpecPagingTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.ValueObjects;

namespace IndTrace.Aggregation.BoundedTests.Repository;

/// <summary>
/// Aggregation tests (real EF Core InMemory) covering issue #117 F7: <c>ReadOnlyRepository</c> must apply
/// a specification's <c>Skip</c> and <c>Take</c> independently (matching <c>Repository</c>). Previously it
/// required BOTH to be set, so a Take-only specification silently materialized the whole table.
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="ReadOnlyRepositorySpecPagingTests"/> class.</remarks>
public class ReadOnlyRepositorySpecPagingTests(ITestOutputHelper outputHelper) : DependenciesFactory(outputHelper)
{
    // Composite-key space chosen high to avoid collision with MachinePlcRawData.Fixture and other
    // repository test ranges. InMemory does not enforce the FK to Machine/Plc.
    private const int BaseMachineId = 9400;
    private const int SeededRowCount = 5;

    /// <summary>
    /// Issue #117 F7: a Take-only specification must return only Take rows, not the whole matching set.
    /// </summary>
    [Fact]
    public async Task ListAsync_TakeOnlySpecification_ReturnsOnlyTakeRows()
    {
        var ct = TestContext.Current.CancellationToken;
        const int machineId = BaseMachineId + 1;
        await SeedRowsAsync(machineId, ct);

        var takeOnly = new PartialPagingSpecification<MachinePlc>(
            mp => mp.MachineId == new MachineId(machineId),
            skip: null,
            take: 2);

        var result = await DpRoMachinePlcRepository.ListAsync(takeOnly, ct);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldNotBeNull().Count().ShouldBe(2);
    }

    /// <summary>
    /// Issue #117 F7: a Skip-only specification must skip the leading rows even without a Take.
    /// </summary>
    [Fact]
    public async Task ListAsync_SkipOnlySpecification_SkipsRows()
    {
        var ct = TestContext.Current.CancellationToken;
        const int machineId = BaseMachineId + 2;
        await SeedRowsAsync(machineId, ct);

        var skipOnly = new PartialPagingSpecification<MachinePlc>(
            mp => mp.MachineId == new MachineId(machineId),
            skip: 3,
            take: null);

        var result = await DpRoMachinePlcRepository.ListAsync(skipOnly, ct);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldNotBeNull().Count().ShouldBe(SeededRowCount - 3);
    }

    private async Task SeedRowsAsync(int machineId, CancellationToken ct)
    {
        for (var plcId = 1; plcId <= SeededRowCount; plcId++)
        {
            (await DpMachinePlcRepository.AddAsync(new MachinePlc(machineId, plcId, ActiveStatus.Active), ct)).IsSuccess.ShouldBeTrue();
        }
    }

    /// <summary>
    /// Minimal <see cref="ISpecification{T}"/> implementation able to express Take-only or Skip-only
    /// paging — something the concrete <c>Specification</c> cannot (its <c>ApplyPaging</c> always sets
    /// both) — so the repository's independent Skip/Take handling can be exercised.
    /// </summary>
    private sealed class PartialPagingSpecification<TEntity>(
        Expression<Func<TEntity, bool>> criteria,
        int? skip,
        int? take) : ISpecification<TEntity>
        where TEntity : class
    {
        public Expression<Func<TEntity, bool>> Criteria { get; } = criteria;

        public List<Expression<Func<TEntity, object>>> Includes { get; } = [];

        public List<string> IncludeStrings { get; } = [];

        public Expression<Func<TEntity, object>>? OrderBy { get; private set; }

        public Expression<Func<TEntity, object>>? OrderByDescending { get; private set; }

        public Expression<Func<TEntity, object>>? ThenBy { get; private set; }

        public Expression<Func<TEntity, object>>? ThenByDescending { get; private set; }

        public Expression<Func<TEntity, object>>? Select { get; private set; }

        public string Key { get; } =
            $"PartialPaging:{typeof(TEntity).Name}:Skip={skip?.ToString(CultureInfo.InvariantCulture) ?? "null"}:Take={take?.ToString(CultureInfo.InvariantCulture) ?? "null"}";

        public int? Skip { get; private set; } = skip;

        public int? Take { get; private set; } = take;

        public bool IsTracking { get; private set; } = true;

        public ISpecification<TEntity> AddInclude(Expression<Func<TEntity, object>> includeExpression)
        {
            this.Includes.Add(includeExpression);
            return this;
        }

        public ISpecification<TEntity> AddInclude(string includeString)
        {
            this.IncludeStrings.Add(includeString);
            return this;
        }

        public ISpecification<TEntity> AddOrderBy(Expression<Func<TEntity, object>> orderByExpression)
        {
            this.OrderBy = orderByExpression;
            return this;
        }

        public ISpecification<TEntity> AddOrderByDescending(Expression<Func<TEntity, object>> orderByDescExpression)
        {
            this.OrderByDescending = orderByDescExpression;
            return this;
        }

        public ISpecification<TEntity> AddThenBy(Expression<Func<TEntity, object>> thenByExpression)
        {
            this.ThenBy = thenByExpression;
            return this;
        }

        public ISpecification<TEntity> AddThenByDescending(Expression<Func<TEntity, object>> thenByDescExpression)
        {
            this.ThenByDescending = thenByDescExpression;
            return this;
        }

        public ISpecification<TEntity> ApplyPaging(int skip, int take)
        {
            this.Skip = skip;
            this.Take = take;
            return this;
        }

        public ISpecification<TEntity> ApplyNoTracking()
        {
            this.IsTracking = false;
            return this;
        }

        public ISpecification<TEntity> And(ISpecification<TEntity> specification) => this;

        public ISpecification<TEntity> Or(ISpecification<TEntity> specification) => this;

        public ISpecification<TEntity> AddSelect(Expression<Func<TEntity, object>> selectExpression)
        {
            this.Select = selectExpression;
            return this;
        }
    }
}
