// <copyright file="PlcDetailDataLoaderTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Plcs.Queries.GetDetail.DataLoaders;

namespace Application.UnitTests.Features.Plcs;

/// <summary>
/// Issue #118 Chunk B: proves <see cref="PlcDetailDataLoader"/> pushes the per-PLC filters into
/// <see cref="Specification{T}"/> instances instead of materializing whole tables and filtering in
/// memory. Every list stub compiles the captured criteria and returns ONLY matching rows (exactly
/// like the SQL-translated query would), so a filter still applied in memory — or missing from the
/// specification — fails these assertions. The <c>VariablesGroup</c> catalog deliberately stays a
/// full load: the assembler exposes ALL groups as the view-model's group dictionary, not just the
/// groups referenced by the PLC's variables.
/// </summary>
public class PlcDetailDataLoaderTests
{
    private const int PlcId = 7;
    private const int ForeignPlcId = 8;

    private readonly IRepository<Plc> plcRepository = Substitute.For<IRepository<Plc>>();
    private readonly IReadOnlyRepository<MachinePlc> machinePlcRepository = Substitute.For<IReadOnlyRepository<MachinePlc>>();
    private readonly IRepository<Machine> machineRepository = Substitute.For<IRepository<Machine>>();
    private readonly IRepository<Variable> variableRepository = Substitute.For<IRepository<Variable>>();
    private readonly IRepository<VariablesGroup> variableGroupRepository = Substitute.For<IRepository<VariablesGroup>>();

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
    /// Builds a loader over a catalog that deliberately contains rows for ANOTHER PLC (id 8) and an
    /// inactive variable on the requested PLC, so any missing filter surfaces as extra rows.
    /// </summary>
    private PlcDetailDataLoader BuildLoader()
    {
        var plc = Plc.CreateFixture(plcId: PlcId, machineId: 100, enabled: ActiveStatus.Active, name: "PLC-7", ipAddress: string.Empty, plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty);

        var machinePlcs = new List<MachinePlc>
        {
            new(100, PlcId, ActiveStatus.Active),
            new(200, PlcId, ActiveStatus.Active),
            new(300, ForeignPlcId, ActiveStatus.Active),
        };

        var machines = new List<Machine>
        {
            new() { MachineId = new MachineId(100) },
            new() { MachineId = new MachineId(200) },
            new() { MachineId = new MachineId(300) },
        };

        var variables = new List<Variable>
        {
            new() { VariableId = 1, PlcId = PlcId, MachineId = 100, Name = "V-Active-7", IsActive = ActiveStatus.Active },
            new() { VariableId = 2, PlcId = PlcId, MachineId = 100, Name = "V-Inactive-7", IsActive = ActiveStatus.Inactive },
            new() { VariableId = 3, PlcId = ForeignPlcId, MachineId = 300, Name = "V-Active-8", IsActive = ActiveStatus.Active },
        };

        var groups = new List<VariablesGroup>
        {
            new() { VariableGroupId = 128, VariableGroupName = "Registers" },
            new() { VariableGroupId = 256, VariableGroupName = "References" },
        };

        this.plcRepository.GetByIdAsync(PlcId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Plc?>.Success(plc)));

        StubListAsync(this.machinePlcRepository, machinePlcs);
        StubListAsync(this.machineRepository, machines);
        StubListAsync(this.variableRepository, variables);

        // The group catalog is a deliberate FULL load (the assembler publishes every group), so the
        // loader keeps using the parameterless overload for it.
        this.variableGroupRepository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<VariablesGroup>>.Success(groups)));

        var logger = XUnitLogger.CreateLogger<PlcDetailDataLoader>();

        return new PlcDetailDataLoader(
            this.plcRepository,
            this.machinePlcRepository,
            this.machineRepository,
            this.variableRepository,
            this.variableGroupRepository,
            logger);
    }

    /// <summary>
    /// Happy path: only the rows belonging to the requested PLC come back — machine-PLC links for
    /// PLC 7, the machines those links reference, the ACTIVE variables of PLC 7, and the FULL
    /// variable-group catalog.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task LoadByPlcId_HappyPath_ReturnsOnlyRowsForRequestedPlc()
    {
        // Arrange
        var loader = this.BuildLoader();

        // Act
        var result = await loader.LoadByPlcIdAsync(PlcId, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        result.Value.Plc.PlcId.ShouldBe(PlcId);

        result.Value.MachinePlcs.Count.ShouldBe(2);
        result.Value.MachinePlcs.ShouldAllBe(mp => mp.PlcId == PlcId);

        var machineIds = result.Value.Machines.Select(m => m.MachineId.Value).OrderBy(id => id).ToList();
        machineIds.ShouldBe(new[] { 100, 200 });
        machineIds.ShouldNotContain(300); // Machine linked only to the foreign PLC is not loaded.

        var variableNames = result.Value.Variables.Select(v => v.Name).ToList();
        variableNames.ShouldBe(new[] { "V-Active-7" });

        result.Value.VariableGroups.Count.ShouldBe(2); // Full catalog — see class remarks.
    }

    /// <summary>
    /// The captured variable specification itself must express BOTH filters (PLC id and active
    /// status): compile the criteria and evaluate it against sample rows.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task LoadByPlcId_VariableSpecification_FiltersByPlcIdAndActiveStatus()
    {
        // Arrange
        var loader = this.BuildLoader();
        ISpecification<Variable>? capturedSpec = null;
        this.variableRepository.ListAsync(Arg.Do<ISpecification<Variable>>(spec => capturedSpec = spec), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<Variable>>.Success([])));

        // Act
        var result = await loader.LoadByPlcIdAsync(PlcId, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        capturedSpec.ShouldNotBeNull();

        var criteria = capturedSpec.Criteria.Compile();
        criteria(new Variable { PlcId = PlcId, IsActive = ActiveStatus.Active }).ShouldBeTrue();
        criteria(new Variable { PlcId = PlcId, IsActive = ActiveStatus.Inactive }).ShouldBeFalse();
        criteria(new Variable { PlcId = PlcId, IsActive = ActiveStatus.None }).ShouldBeFalse();
        criteria(new Variable { PlcId = ForeignPlcId, IsActive = ActiveStatus.Active }).ShouldBeFalse();
    }

    /// <summary>
    /// A repository failure on the machine-PLC query must propagate the repository errors verbatim.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task LoadByPlcId_MachinePlcRepositoryFailure_PropagatesErrors()
    {
        // Arrange
        var loader = this.BuildLoader();
        this.machinePlcRepository.ListAsync(Arg.Any<ISpecification<MachinePlc>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<MachinePlc>>.WithFailure(["MachinePlc query failed"])));

        // Act
        var result = await loader.LoadByPlcIdAsync(PlcId, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("MachinePlc query failed");
    }

    /// <summary>
    /// A PLC without machine links yields empty machine-PLC and machine lists but still succeeds
    /// (the detail view renders the PLC with no machines).
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task LoadByPlcId_NoMachineLinks_ReturnsEmptyMachineCollections()
    {
        // Arrange
        var loader = this.BuildLoader();
        StubListAsync(this.machinePlcRepository, new List<MachinePlc>
        {
            new(300, ForeignPlcId, ActiveStatus.Active),
        });

        // Act
        var result = await loader.LoadByPlcIdAsync(PlcId, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.MachinePlcs.ShouldBeEmpty();
        result.Value.Machines.ShouldBeEmpty();
    }

    /// <summary>
    /// A non-positive PLC id fails fast without touching any repository.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task LoadByPlcId_NonPositivePlcId_FailsFast()
    {
        // Arrange
        var loader = this.BuildLoader();

        // Act
        var result = await loader.LoadByPlcIdAsync(0, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("PlcId must be positive.");
    }
}
