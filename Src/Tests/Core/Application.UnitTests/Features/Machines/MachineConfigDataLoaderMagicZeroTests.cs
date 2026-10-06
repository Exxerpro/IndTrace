// <copyright file="MachineConfigDataLoaderMagicZeroTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Machines.Queries.GetMachinesConfig.DataLoaders;
using IndTrace.Application.Repository;
using IndTrace.Domain.ValueObjects;

namespace Application.UnitTests.Features.Machines;

/// <summary>
/// Characterization tests pinning the migration-safe machine-id derivation of
/// <see cref="MachineConfigDataLoader"/> for the C2 routing redesign (Chunk E7).
/// <para>
/// The C2 destructive migration (D2) removes the magic-0 boundary pseudo-edges from the
/// <c>WorkFlows</c> table: before the migration a linear route is stored as
/// <c>(0-&gt;first),(first-&gt;..),(..-&gt;last),(last-&gt;0)</c>; afterwards only the clean interior edges
/// <c>(first-&gt;..),(..-&gt;last)</c> remain. The old <c>Select(NextMachineId).Distinct()</c> derivation
/// silently dropped the Initial machine after the migration (it appeared only as the <c>NextMachineId</c>
/// of the deleted <c>(0-&gt;first)</c> row). These tests prove the union-of-endpoints derivation yields the
/// SAME participating machine set <c>{100, 400, 500}</c> for BOTH encodings, retains the Initial machine,
/// and excludes the virtual machine 0.
/// </para>
/// </summary>
public class MachineConfigDataLoaderMagicZeroTests
{
    private const string PartNumber = "PART566";
    private const int ProductId = 566;

    /// <summary>
    /// Stubs the specification <c>ListAsync</c> overload by compiling the captured criteria and evaluating
    /// it against the supplied sample rows — the stub returns ONLY matching rows, exactly like the
    /// SQL-translated query would, so the assertions below prove the loader's specifications actually
    /// express the filters (issue #118 Chunk B: filters pushed into the query, no in-memory filtering left).
    /// </summary>
    private static void StubListAsync<T>(IRepository<T> repository, IReadOnlyList<T> rows)
        where T : class, IPersistable
    {
        repository.ListAsync(Arg.Any<ISpecification<T>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var spec = callInfo.Arg<ISpecification<T>>();
                var filtered = rows.Where(spec.Criteria.Compile()).ToList();
                return Task.FromResult(Result<IEnumerable<T>>.Success(filtered));
            });
    }

    /// <summary>
    /// Read-only overload of the specification <c>ListAsync</c> stub (#95 Slice C: MachinePlc reads go
    /// through <c>IReadOnlyRepository</c>); behavior is identical to the mutating-repository stub.
    /// </summary>
    private static void StubListAsync<T>(IReadOnlyRepository<T> repository, IReadOnlyList<T> rows)
        where T : class, IPersistable
    {
        repository.ListAsync(Arg.Any<ISpecification<T>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var spec = callInfo.Arg<ISpecification<T>>();
                var filtered = rows.Where(spec.Criteria.Compile()).ToList();
                return Task.FromResult(Result<IEnumerable<T>>.Success(filtered));
            });
    }

    private static MachineConfigDataLoader BuildLoader(IReadOnlyList<WorkFlow> workFlows)
    {
        var product = Product.CreateFixture(productId: ProductId, partNumber: PartNumber);

        // Machine 0 is the seeded virtual "End/Start Process" boundary row: it exists in the
        // machine catalog but carries NO machine-PLC links and NO variables.
        var machines = new List<Machine>
        {
            new() { MachineId = new MachineId(0) },
            new() { MachineId = new MachineId(100) },
            new() { MachineId = new MachineId(400) },
            new() { MachineId = new MachineId(500) },
        };

        var machinePlcs = new List<MachinePlc>
        {
            new(100, 1, 1),
            new(400, 4, 1),
            new(500, 5, 1),
        };

        var plcs = new List<Plc>
        {
            Plc.CreateFixture(plcId: 1, machineId: 100, enabled: ActiveStatus.None, name: "PLC-100", ipAddress: string.Empty, plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty),
            Plc.CreateFixture(plcId: 4, machineId: 400, enabled: ActiveStatus.None, name: "PLC-400", ipAddress: string.Empty, plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty),
            Plc.CreateFixture(plcId: 5, machineId: 500, enabled: ActiveStatus.None, name: "PLC-500", ipAddress: string.Empty, plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty),
        };

        var variables = new List<Variable>
        {
            new() { MachineId = 100 },
            new() { MachineId = 400 },
            new() { MachineId = 500 },
        };

        var productRepository = Substitute.For<IRepository<Product>>();

        // Emulate the SQL-side keyed lookup: the captured criteria filters the sample catalog and an
        // unknown part number simply yields an empty list (success), which the loader maps to not-found.
        StubListAsync(productRepository, new[] { product });

        var workFlowRepository = Substitute.For<IReadOnlyRepository<WorkFlow>>();
        StubListAsync(workFlowRepository, workFlows);

        var machineRepository = Substitute.For<IRepository<Machine>>();
        StubListAsync(machineRepository, machines);

        var plcRepository = Substitute.For<IRepository<Plc>>();
        StubListAsync(plcRepository, plcs);

        var machinePlcRepository = Substitute.For<IReadOnlyRepository<MachinePlc>>();
        StubListAsync(machinePlcRepository, machinePlcs);

        var variableRepository = Substitute.For<IRepository<Variable>>();
        StubListAsync(variableRepository, variables);

        var logger = XUnitLogger.CreateLogger<MachineConfigDataLoader>();

        return new MachineConfigDataLoader(
            productRepository,
            workFlowRepository,
            machineRepository,
            plcRepository,
            machinePlcRepository,
            variableRepository,
            logger);
    }

    /// <summary>
    /// Magic-0 encoding (PRE-migration): rows (0-&gt;100),(100-&gt;400),(400-&gt;500),(500-&gt;0).
    /// The participating machine set must be {100,400,500} - the Initial machine 100 is retained
    /// and the virtual machine 0 is excluded.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task LoadByPartNumber_MagicZeroEdges_RetainsInitialMachineAndExcludesZero()
    {
        // Arrange - PRE-migration linear route with magic-0 boundary pseudo-edges.
        var workFlows = new List<WorkFlow>
        {
            new() { WorkFlowId = 1, ProductId = ProductId, LastMachineId = new MachineId(0), NextMachineId = new MachineId(100) },
            new() { WorkFlowId = 2, ProductId = ProductId, LastMachineId = new MachineId(100), NextMachineId = new MachineId(400) },
            new() { WorkFlowId = 3, ProductId = ProductId, LastMachineId = new MachineId(400), NextMachineId = new MachineId(500) },
            new() { WorkFlowId = 4, ProductId = ProductId, LastMachineId = new MachineId(500), NextMachineId = new MachineId(0) },
        };

        var loader = BuildLoader(workFlows);

        // Act
        var result = await loader.LoadByPartNumberAsync(PartNumber, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        var loadedMachineIds = result.Value.Machines.Select(m => m.MachineId.Value).OrderBy(id => id).ToList();
        loadedMachineIds.ShouldBe(new[] { 100, 400, 500 });
        loadedMachineIds.ShouldContain(100); // Initial machine retained.
        loadedMachineIds.ShouldNotContain(0); // Virtual machine excluded.

        var derivedMachineIds = result.Value.MachineIds.OrderBy(id => id).ToList();
        derivedMachineIds.ShouldBe(new[] { 100, 400, 500 });
    }

    /// <summary>
    /// Clean-edge encoding (POST-migration): rows (100-&gt;400),(400-&gt;500).
    /// The participating machine set must be the SAME {100,400,500} as the magic-0 encoding,
    /// proving the derivation is byte-identical across the migration boundary.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task LoadByPartNumber_CleanEdges_YieldsSameMachineSetAsMagicZero()
    {
        // Arrange - POST-migration linear route with only clean interior edges.
        var workFlows = new List<WorkFlow>
        {
            new() { WorkFlowId = 2, ProductId = ProductId, LastMachineId = new MachineId(100), NextMachineId = new MachineId(400) },
            new() { WorkFlowId = 3, ProductId = ProductId, LastMachineId = new MachineId(400), NextMachineId = new MachineId(500) },
        };

        var loader = BuildLoader(workFlows);

        // Act
        var result = await loader.LoadByPartNumberAsync(PartNumber, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        var loadedMachineIds = result.Value.Machines.Select(m => m.MachineId.Value).OrderBy(id => id).ToList();
        loadedMachineIds.ShouldBe(new[] { 100, 400, 500 });
        loadedMachineIds.ShouldContain(100); // Initial machine retained.
        loadedMachineIds.ShouldNotContain(0); // Virtual machine excluded.

        var derivedMachineIds = result.Value.MachineIds.OrderBy(id => id).ToList();
        derivedMachineIds.ShouldBe(new[] { 100, 400, 500 });
    }
}
