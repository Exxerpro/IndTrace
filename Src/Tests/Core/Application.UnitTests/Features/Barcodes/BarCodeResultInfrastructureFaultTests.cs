// <copyright file="BarCodeResultInfrastructureFaultTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.Services;
using IndTrace.Domain.Diagnostics;

namespace Application.UnitTests.Features.Barcodes;

/// <summary>
/// Issue #23: proves <see cref="BarCodeResult.GetBarCodeDetails"/> honours the fault category the read path
/// reports. A schema / infrastructure fault on the reference (Variables) read — for example a missing audit
/// column raised as a <c>DbException</c> and carried up as an <see cref="InfrastructureFault"/>-marked failure
/// — surfaces as the distinct <see cref="ResultValidation.InfrastructureFailure"/> outcome and NEVER as
/// <see cref="ResultValidation.ReferencesNotFound"/>. A genuine reference miss still yields
/// <see cref="ResultValidation.ReferencesNotFound"/> exactly as before.
/// </summary>
public class BarCodeResultInfrastructureFaultTests
{
    private const int MachineId = 100;
    private const string Label = "WS100PART01";
    private const string PartNumber = "PART01";

    /// <summary>
    /// An infrastructure-marked reference read failure (schema/DB fault) is surfaced as
    /// <see cref="ResultValidation.InfrastructureFailure"/> — not "References not found".
    /// </summary>
    [Fact]
    public async Task GetBarCodeDetails_WhenReferencesReadFaultsOnSchema_SurfacesInfrastructureFailure()
    {
        var referencesFailure = Result<IEnumerable<Variable>>.WithFailure(
            InfrastructureFault.Compose("Invalid column name 'CreatedBy'."));

        var sut = BuildSut(referencesFailure);

        _ = await sut.GetBarCodeDetails(
            new BarCodeDetailsRequest(MachineId, Label, PartNumber),
            TestContext.Current.CancellationToken);

        sut.ResultValidation.ShouldBe(ResultValidation.InfrastructureFailure);
        sut.ResultValidation.ShouldNotBe(ResultValidation.ReferencesNotFound);
    }

    /// <summary>
    /// A genuine reference miss (plain failure, no infrastructure marker) still reports
    /// <see cref="ResultValidation.ReferencesNotFound"/> — the existing behaviour is preserved.
    /// </summary>
    [Fact]
    public async Task GetBarCodeDetails_WhenReferencesGenuinelyMissing_StillReportsReferencesNotFound()
    {
        var referencesFailure = Result<IEnumerable<Variable>>.WithFailure("no references for this machine");

        var sut = BuildSut(referencesFailure);

        _ = await sut.GetBarCodeDetails(
            new BarCodeDetailsRequest(MachineId, Label, PartNumber),
            TestContext.Current.CancellationToken);

        sut.ResultValidation.ShouldBe(ResultValidation.ReferencesNotFound);
    }

    /// <summary>
    /// Builds a <see cref="BarCodeResult"/> where the current machine read succeeds and the reference
    /// (Variables) read returns <paramref name="variablesResult"/>. Only these two reads are reached before
    /// the reference gate under test; the remaining repositories are bare substitutes.
    /// </summary>
    private static BarCodeResult BuildSut(Result<IEnumerable<Variable>> variablesResult)
    {
        var barCodeRepository = Substitute.For<IRepository<BarCode>>();
        var cycleRepository = Substitute.For<IReadOnlyRepository<Cycle>>();
        var machineRepository = Substitute.For<IReadOnlyRepository<Machine>>();
        var recipeRepository = Substitute.For<IReadOnlyRepository<Recipe>>();
        var masterLabelRepository = Substitute.For<IReadOnlyRepository<MasterLabel>>();
        var shiftRepository = Substitute.For<IRepository<Shift>>();
        var workFlowRepository = Substitute.For<IReadOnlyRepository<WorkFlow>>();
        var routingNodeRepository = Substitute.For<IReadOnlyRepository<RoutingNodeRow>>();
        var variablesRepository = Substitute.For<IReadOnlyRepository<Variable>>();
        var productRepository = Substitute.For<IReadOnlyRepository<Product>>();
        var dateTimeMachine = Substitute.For<IDateTimeMachine>();
        var validationService = Substitute.For<IBarCodeValidationService>();

        var machine = new Machine
        {
            MachineId = new MachineId(MachineId),
            Name = $"Station-{MachineId}",
            MachineType = MachineType.Process,
            EnableAppTraceability = 1,
            EnableBypassTraceability = 0,
        };

        machineRepository
            .FirstOrDefaultAsync(Arg.Any<ISpecification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Machine?>.Success(machine)));

        variablesRepository
            .ListAsync(Arg.Any<ISpecification<Variable>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(variablesResult));

        return new BarCodeResult(
            XUnitLogger.CreateLogger<BarCodeResult>(),
            barCodeRepository,
            cycleRepository,
            machineRepository,
            recipeRepository,
            masterLabelRepository,
            shiftRepository,
            workFlowRepository,
            routingNodeRepository,
            variablesRepository,
            productRepository,
            dateTimeMachine,
            validationService);
    }
}
