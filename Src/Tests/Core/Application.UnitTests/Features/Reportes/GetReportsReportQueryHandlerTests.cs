// <copyright file="GetReportsReportQueryHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Application.UnitTests.TestDoubles;
using ReportsReportHandler = IndTrace.Application.BarCodes.Queries.GetReportsReport.GetBarCodeReportQueryHandler;

namespace Application.UnitTests.Features.Reportes;

/// <summary>
/// Unit tests for the GetReportsReport <c>GetBarCodeReportQueryHandler</c> (the Reports export handler — NOT the
/// GetBarCodeDetail one). #229 (Slice A): the handler now issues three PROJECTED raw-SQL queries through
/// <c>IReadOnlyRepository&lt;T&gt;.FromSqlAsync</c> whose keyset rides as ONE JSON-array parameter (OPENJSON —
/// no literal IN-list of any arity). These tests pin the preserved #119 (F6) behavioral intents on the new seam
/// (the register ledger is fetched exactly ONCE; every barcode receives exactly its own cycles' registers in
/// ledger order) plus the #229 contract: the JSON keyset parameter carries exactly the requested ids, an empty
/// id list is an empty SUCCESS without any query, and an infrastructure failure at each of the three stages
/// propagates its preserved stage message.
/// </summary>
public class GetReportsReportQueryHandlerTests
{
    private readonly IReadOnlyRepository<BarCode> _barcodeRepository;
    private readonly IReadOnlyRepository<Cycle> _cyclesRepository;
    private readonly IReadOnlyRepository<Register> _registersRepository;
    private readonly ReportsReportHandler _sut;

    private FormattableString? _barCodeSql;
    private FormattableString? _cycleSql;
    private FormattableString? _registerSql;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetReportsReportQueryHandlerTests"/> class.
    /// </summary>
    public GetReportsReportQueryHandlerTests()
    {
        _barcodeRepository = Substitute.For<IReadOnlyRepository<BarCode>>();
        _cyclesRepository = Substitute.For<IReadOnlyRepository<Cycle>>();
        _registersRepository = Substitute.For<IReadOnlyRepository<Register>>();
        _sut = new ReportsReportHandler(_barcodeRepository, _cyclesRepository, _registersRepository);
    }

    /// <summary>
    /// Seeds the standard two-barcode fixture: barcode 100 owns cycles 1 and 2 (registers 1 and 2), barcode 200
    /// owns cycle 3 (registers 3 and 4). Each raw-SQL seam leases an async-capable in-memory queryable (null
    /// owner) and captures the emitted <see cref="FormattableString"/> for keyset-parameter assertions.
    /// </summary>
    private void SeedTwoBarCodeFixture()
    {
        var barCodes = new List<BarCode>
        {
            new() { BarCodeId = new BarCodeId(100), MachineId = new MachineId(10), Label = BarCodeLabel.FromPersisted("BC-100") },
            new() { BarCodeId = new BarCodeId(200), MachineId = new MachineId(20), Label = BarCodeLabel.FromPersisted("BC-200") },
        };
        var cycles = new List<Cycle>
        {
            new() { CycleId = new CycleId(1), BarCodeId = new BarCodeId(100), MachineId = new MachineId(10), CycleTime = 11 },
            new() { CycleId = new CycleId(2), BarCodeId = new BarCodeId(100), MachineId = new MachineId(10), CycleTime = 12 },
            new() { CycleId = new CycleId(3), BarCodeId = new BarCodeId(200), MachineId = new MachineId(20), CycleTime = 13 },
        };
        var registers = new List<Register>
        {
            Register.CreateFixture(registerId: 1, cycleId: 1, value: "R1"),
            Register.CreateFixture(registerId: 2, cycleId: 2, value: "R2"),
            Register.CreateFixture(registerId: 3, cycleId: 3, value: "R3"),
            Register.CreateFixture(registerId: 4, cycleId: 3, value: "R4"),
        };

        _barcodeRepository.FromSqlAsync(Arg.Do<FormattableString>(s => _barCodeSql = s), Arg.Any<CancellationToken>())
            .Returns(_ => Result<OwnedQueryable<BarCode>>.Success(
                new OwnedQueryable<BarCode>(new TestAsyncEnumerable<BarCode>(barCodes), null)));
        _cyclesRepository.FromSqlAsync(Arg.Do<FormattableString>(s => _cycleSql = s), Arg.Any<CancellationToken>())
            .Returns(_ => Result<OwnedQueryable<Cycle>>.Success(
                new OwnedQueryable<Cycle>(new TestAsyncEnumerable<Cycle>(cycles), null)));
        _registersRepository.FromSqlAsync(Arg.Do<FormattableString>(s => _registerSql = s), Arg.Any<CancellationToken>())
            .Returns(_ => Result<OwnedQueryable<Register>>.Success(
                new OwnedQueryable<Register>(new TestAsyncEnumerable<Register>(registers), null)));
    }

    /// <summary>
    /// #119 (F6, preserved across #229): the handler must fetch the register ledger exactly once — the register
    /// query joins to Cycles SERVER-side, so no second (or per-barcode) register round-trip is ever issued.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_FetchesTheRegisterLedgerExactlyOnce()
    {
        // Arrange
        SeedTwoBarCodeFixture();
        var request = new GetBarCodeReportQuery { BarCodesIdList = [100, 200] };

        // Act
        var result = await _sut.ProcessAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await _registersRepository.Received(1).FromSqlAsync(Arg.Any<FormattableString>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// #119 (F6) equivalence pin carried across the #229 projection rewrite: each barcode receives EXACTLY its
    /// own cycles' registers, in the register list's original (ledger) order, and exactly its own cycles in
    /// fetch order — with the consumed columns mapped through to the views.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_AssignsEachBarCodeExactlyItsOwnCyclesAndRegistersInFetchOrder()
    {
        // Arrange
        SeedTwoBarCodeFixture();
        var request = new GetBarCodeReportQuery { BarCodesIdList = [100, 200] };

        // Act
        var result = await _sut.ProcessAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count.ShouldBe(2);

        var item100 = result.Value.Single(v => v.BarCodeId == 100);
        var item200 = result.Value.Single(v => v.BarCodeId == 200);

        item100.MachineId.ShouldBe(10);
        item100.Label.ShouldBe("BC-100");
        item100.Cycles.Select(c => c.CycleId).ShouldBe([1, 2]);
        item100.Registers.Select(r => r.RegisterId).ShouldBe([1, 2]);
        item200.MachineId.ShouldBe(20);
        item200.Label.ShouldBe("BC-200");
        item200.Cycles.Select(c => c.CycleId).ShouldBe([3]);
        item200.Registers.Select(r => r.RegisterId).ShouldBe([3, 4]);

        // Consumed cycle columns map through; CyclesOk is intentionally never populated (export renders 0).
        item100.Cycles.Select(c => c.CycleTime).ShouldBe([11, 12]);
        item100.Cycles.ShouldAllBe(c => c.CyclesOk == 0);
        item200.Registers.Select(r => r.Value).ShouldBe(["R3", "R4"]);
    }

    /// <summary>
    /// #229: all three queries carry the requested ids as EXACTLY ONE interpolation hole — a JSON array string
    /// listing exactly the requested ids (bound as a single SQL parameter feeding OPENJSON; never a literal
    /// IN-list whose arity would fork query plans).
    /// </summary>
    [Fact]
    public async Task ProcessAsync_SendsTheRequestedIdsAsOneJsonKeysetParameterToAllThreeQueries()
    {
        // Arrange
        SeedTwoBarCodeFixture();
        var request = new GetBarCodeReportQuery { BarCodesIdList = [100, 200] };

        // Act
        var result = await _sut.ProcessAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        foreach (var sql in new[] { _barCodeSql, _cycleSql, _registerSql })
        {
            sql.ShouldNotBeNull();
            sql.Format.ShouldContain("OPENJSON");
            sql.GetArguments().ShouldBe(["[100,200]"]);
        }
    }

    /// <summary>
    /// An empty id list is an empty report SUCCESS (preserved contract) — and no query round-trip is issued.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WithEmptyIdList_ReturnsEmptySuccessWithoutQuerying()
    {
        // Arrange
        var request = new GetBarCodeReportQuery { BarCodesIdList = [] };

        // Act
        var result = await _sut.ProcessAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().ShouldBeEmpty();
        await _barcodeRepository.DidNotReceive().FromSqlAsync(Arg.Any<FormattableString>(), Arg.Any<CancellationToken>());
        await _cyclesRepository.DidNotReceive().FromSqlAsync(Arg.Any<FormattableString>(), Arg.Any<CancellationToken>());
        await _registersRepository.DidNotReceive().FromSqlAsync(Arg.Any<FormattableString>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// An infrastructure failure at the BARCODE stage propagates as a failure carrying the preserved stage
    /// message — never collapsed into an empty report.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WhenTheBarCodeQueryFails_PropagatesTheStageFailure()
    {
        // Arrange
        _barcodeRepository.FromSqlAsync(Arg.Any<FormattableString>(), Arg.Any<CancellationToken>())
            .Returns(Result<OwnedQueryable<BarCode>>.WithFailure("Connection refused"));
        var request = new GetBarCodeReportQuery { BarCodesIdList = [100] };

        // Act
        var result = await _sut.ProcessAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("BarCodes not found");
        await _cyclesRepository.DidNotReceive().FromSqlAsync(Arg.Any<FormattableString>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// An infrastructure failure at the CYCLE stage propagates as a failure carrying the preserved stage message.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WhenTheCycleQueryFails_PropagatesTheStageFailure()
    {
        // Arrange
        var barCodes = new List<BarCode>
        {
            new() { BarCodeId = new BarCodeId(100), Label = BarCodeLabel.FromPersisted("BC-100") },
        };
        _barcodeRepository.FromSqlAsync(Arg.Any<FormattableString>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result<OwnedQueryable<BarCode>>.Success(
                new OwnedQueryable<BarCode>(new TestAsyncEnumerable<BarCode>(barCodes), null)));
        _cyclesRepository.FromSqlAsync(Arg.Any<FormattableString>(), Arg.Any<CancellationToken>())
            .Returns(Result<OwnedQueryable<Cycle>>.WithFailure("Connection refused"));
        var request = new GetBarCodeReportQuery { BarCodesIdList = [100] };

        // Act
        var result = await _sut.ProcessAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Cycles not found");
        await _registersRepository.DidNotReceive().FromSqlAsync(Arg.Any<FormattableString>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// An infrastructure failure at the REGISTER stage propagates as a failure carrying the preserved stage
    /// message.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WhenTheRegisterQueryFails_PropagatesTheStageFailure()
    {
        // Arrange
        var barCodes = new List<BarCode>
        {
            new() { BarCodeId = new BarCodeId(100), Label = BarCodeLabel.FromPersisted("BC-100") },
        };
        var cycles = new List<Cycle>
        {
            new() { CycleId = new CycleId(1), BarCodeId = new BarCodeId(100) },
        };
        _barcodeRepository.FromSqlAsync(Arg.Any<FormattableString>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result<OwnedQueryable<BarCode>>.Success(
                new OwnedQueryable<BarCode>(new TestAsyncEnumerable<BarCode>(barCodes), null)));
        _cyclesRepository.FromSqlAsync(Arg.Any<FormattableString>(), Arg.Any<CancellationToken>())
            .Returns(_ => Result<OwnedQueryable<Cycle>>.Success(
                new OwnedQueryable<Cycle>(new TestAsyncEnumerable<Cycle>(cycles), null)));
        _registersRepository.FromSqlAsync(Arg.Any<FormattableString>(), Arg.Any<CancellationToken>())
            .Returns(Result<OwnedQueryable<Register>>.WithFailure("Connection refused"));
        var request = new GetBarCodeReportQuery { BarCodesIdList = [100] };

        // Act
        var result = await _sut.ProcessAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Registers not found");
    }
}
