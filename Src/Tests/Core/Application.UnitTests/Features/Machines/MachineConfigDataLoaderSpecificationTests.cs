// <copyright file="MachineConfigDataLoaderSpecificationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Machines.Queries.GetMachinesConfig.DataLoaders;

namespace Application.UnitTests.Features.Machines;

/// <summary>
/// Issue #118 Chunk B: proves <see cref="MachineConfigDataLoader"/> pushes every filter into a
/// <see cref="Specification{T}"/> instead of materializing whole tables and filtering in memory.
/// Every list stub compiles the captured criteria and returns ONLY matching rows (exactly like the
/// SQL-translated query would), so any filter still applied in memory — or missing from the
/// specification — fails these assertions. Also pins the empty-list-to-not-found mapping for the
/// keyed product lookup (<c>ListAsync</c> returns success with an empty list when nothing matches).
/// </summary>
public class MachineConfigDataLoaderSpecificationTests
{
    private const string PartNumber = "PART566";
    private const int ProductId = 566;

    /// <summary>
    /// Stubs the specification <c>ListAsync</c> overload by compiling the captured criteria and
    /// evaluating it against the supplied sample rows, emulating the SQL-side filter.
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

    /// <summary>
    /// Builds a loader over a small catalog that deliberately contains rows OUTSIDE the requested
    /// product's reach: machine 999 (not on the route), PLC 9 (linked to no participating machine)
    /// and a variable on machine 999. The route is the single clean edge (100-&gt;400).
    /// </summary>
    private static (MachineConfigDataLoader Loader, IRepository<Product> ProductRepository) BuildLoader()
    {
        var product = Product.CreateFixture(productId: ProductId, partNumber: PartNumber);

        var workFlows = new List<WorkFlow>
        {
            new() { WorkFlowId = 1, ProductId = ProductId, LastMachineId = new MachineId(100), NextMachineId = new MachineId(400) },
            new() { WorkFlowId = 2, ProductId = 9999, LastMachineId = new MachineId(999), NextMachineId = new MachineId(998) },
        };

        var machines = new List<Machine>
        {
            new() { MachineId = new MachineId(100) },
            new() { MachineId = new MachineId(400) },
            new() { MachineId = new MachineId(999) },
        };

        var machinePlcs = new List<MachinePlc>
        {
            new(100, 1, ActiveStatus.Active),
            new(400, 4, ActiveStatus.Active),
            new(999, 9, ActiveStatus.Active),
        };

        var plcs = new List<Plc>
        {
            Plc.CreateFixture(plcId: 1, machineId: 100, enabled: ActiveStatus.None, name: "PLC-100", ipAddress: string.Empty, plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty),
            Plc.CreateFixture(plcId: 4, machineId: 400, enabled: ActiveStatus.None, name: "PLC-400", ipAddress: string.Empty, plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty),
            Plc.CreateFixture(plcId: 9, machineId: 999, enabled: ActiveStatus.None, name: "PLC-UNLINKED", ipAddress: string.Empty, plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty),
        };

        var variables = new List<Variable>
        {
            new() { VariableId = 1, MachineId = 100 },
            new() { VariableId = 2, MachineId = 400 },
            new() { VariableId = 3, MachineId = 999 },
        };

        var productRepository = Substitute.For<IRepository<Product>>();
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

        var loader = new MachineConfigDataLoader(
            productRepository,
            workFlowRepository,
            machineRepository,
            plcRepository,
            machinePlcRepository,
            variableRepository,
            logger);

        return (loader, productRepository);
    }

    /// <summary>
    /// An unknown part number yields an empty list from the repository; the loader must map it to
    /// the SAME user-facing failure message the in-memory FirstOrDefault produced.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task LoadByPartNumber_UnknownPartNumber_ReturnsProductNotFoundFailure()
    {
        // Arrange
        var (loader, _) = BuildLoader();

        // Act
        var result = await loader.LoadByPartNumberAsync("MISSING", TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Product with PartNumber MISSING not found");
    }

    /// <summary>
    /// A genuine repository failure on the product lookup must propagate the repository
    /// errors verbatim — NOT be masked as "product not found".
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task LoadByPartNumber_ProductRepositoryFailure_PropagatesRepositoryErrors()
    {
        // Arrange
        var (loader, productRepository) = BuildLoader();
        productRepository.ListAsync(Arg.Any<ISpecification<Product>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<Product>>.WithFailure(["Database connection lost"])));

        // Act
        var result = await loader.LoadByPartNumberAsync(PartNumber, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Database connection lost");
        result.Errors.ShouldNotContain($"Product with PartNumber {PartNumber} not found");
    }

    /// <summary>
    /// The PLC query must load only the PLCs referenced by the participating machine-PLC links
    /// (the assembler consumes PLCs solely through that join), not the whole PLC table.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task LoadByPartNumber_PlcQuery_LoadsOnlyPlcsLinkedToParticipatingMachines()
    {
        // Arrange
        var (loader, _) = BuildLoader();

        // Act
        var result = await loader.LoadByPartNumberAsync(PartNumber, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        var loadedPlcIds = result.Value.Plcs.Select(p => p.PlcId).OrderBy(id => id).ToList();
        loadedPlcIds.ShouldBe(new[] { 1, 4 });
        loadedPlcIds.ShouldNotContain(9); // PLC linked to a machine outside the route is not loaded.
    }

    /// <summary>
    /// The variable query must load only the variables of participating machines; a variable on a
    /// machine outside the route stays in the database.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task LoadByPartNumber_VariableQuery_LoadsOnlyParticipatingMachineVariables()
    {
        // Arrange
        var (loader, _) = BuildLoader();

        // Act
        var result = await loader.LoadByPartNumberAsync(PartNumber, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        var loadedVariableMachineIds = result.Value.Variables.Select(v => v.MachineId).OrderBy(id => id).ToList();
        loadedVariableMachineIds.ShouldBe(new[] { 100, 400 });
        loadedVariableMachineIds.ShouldNotContain(999);
    }

    /// <summary>
    /// The workflow query must load only the requested product's edges — the foreign product's edge
    /// (and therefore its machines) never reaches the context.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task LoadByPartNumber_WorkFlowQuery_LoadsOnlyRequestedProductEdges()
    {
        // Arrange
        var (loader, _) = BuildLoader();

        // Act
        var result = await loader.LoadByPartNumberAsync(PartNumber, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        result.Value.WorkFlows.Count.ShouldBe(1);
        result.Value.WorkFlows[0].WorkFlowId.ShouldBe(1);
        result.Value.MachineIds.OrderBy(id => id).ToList().ShouldBe(new[] { 100, 400 });
    }

    /// <summary>
    /// Regression test (issue #118 adversarial-review blocker): the QA/production SQL collation
    /// (Modern_Spanish_CI_AS) matches case-insensitively, so the pushed-down specification can return a
    /// case-variant row (request "part566" matching stored "PART566"). The loader must re-apply ORDINAL
    /// equality client-side and report not-found — the exact semantics of the replaced in-memory
    /// FirstOrDefault. The stubs simulate the collation-insensitive store on BOTH repository shapes
    /// REGARDLESS of the captured criteria, so this test is red against a loader that trusts the store's
    /// match and green only when the ordinal client-side filter rejects the case variant.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task LoadByPartNumber_CollationInsensitiveStoreReturnsCaseVariantRow_ReportsNotFound()
    {
        // Arrange - the catalog's stored part number is "PART566"; the request differs only by case.
        var (loader, productRepository) = BuildLoader();
        const string requestedPartNumber = "part566";
        var caseVariantProduct = Product.CreateFixture(productId: ProductId, partNumber: PartNumber);

        // Simulate the collation-insensitive store: the case-variant row comes back regardless of criteria.
        productRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<Product>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Product?>.Success(caseVariantProduct)));
        productRepository.ListAsync(Arg.Any<ISpecification<Product>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<Product>>.Success(new List<Product> { caseVariantProduct })));

        // Act
        var result = await loader.LoadByPartNumberAsync(requestedPartNumber, TestContext.Current.CancellationToken);

        // Assert - ordinal semantics: "part566" must NOT match "PART566".
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"Product with PartNumber {requestedPartNumber} not found");
    }
}
