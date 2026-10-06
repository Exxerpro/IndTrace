// <copyright file="RegisterDataFilterTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Application.UnitTests.TestDoubles;
using IndTrace.Application.BarCodes.Queries.Filters;

namespace Application.UnitTests.Features.Reportes;

/// <summary>
/// Unit tests for <see cref="RegisterDataFilter"/> — the register-ledger barcode filter behind the Reports list.
/// #229 (Slice B): the filter now issues ONE projected SQL query (a correlated register EXISTS rooted at Cycles
/// via <c>IReadOnlyRepository&lt;Cycle&gt;.FromSqlAsync</c>) instead of the former 3-hop entity chain. These tests
/// pin the preserved #119 (F2) contract on the new seam: the bounded SQL variant carries the report window as
/// TimeStamp parameters (IX_Registers_TimeStamp-supported), a null window selects the content-preserving
/// unbounded variant (#126 review C10, IsMaster path — #119 F6 PO ruling pending), infrastructure failures
/// propagate as failures (never collapsed into a no-match), and a genuine no-match yields an EMPTY success set.
/// </summary>
public class RegisterDataFilterTests
{
    private static readonly DateTime WindowStart = new(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime WindowEnd = new(2026, 1, 20, 23, 59, 59, DateTimeKind.Utc);

    private readonly IReadOnlyRepository<Cycle> _cycleRepository;
    private readonly RegisterDataFilter _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterDataFilterTests"/> class.
    /// </summary>
    public RegisterDataFilterTests()
    {
        _cycleRepository = Substitute.For<IReadOnlyRepository<Cycle>>();
        _sut = new RegisterDataFilter(_cycleRepository, XUnitLogger.CreateLogger<RegisterDataFilter>());
    }

    /// <summary>
    /// #119 (F2): with a full report window the filter must select the BOUNDED SQL variant — the correlated
    /// register EXISTS carries both TimeStamp bounds, and the captured interpolation arguments are exactly the
    /// escaped substring pattern plus the two window ends (bound as SQL parameters, never concatenated).
    /// </summary>
    [Fact]
    public async Task GetMatchingBarCodeIdsAsync_WithFullWindow_UsesTheBoundedSqlVariantWithWindowArgs()
    {
        // Arrange — capture the FormattableString the filter sends to the raw-SQL seam.
        FormattableString? capturedSql = null;
        ArrangeLease([], sql => capturedSql = sql);

        // Act
        var result = await _sut.GetMatchingBarCodeIdsAsync("ABC", WindowStart, WindowEnd, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        capturedSql.ShouldNotBeNull();
        capturedSql.Format.ShouldContain("EXISTS");
        capturedSql.Format.ShouldContain("r.CycleId = c.CycleId");
        capturedSql.Format.ShouldContain("r.Value LIKE {0}");
        capturedSql.Format.ShouldContain("r.TimeStamp >= {1}");
        capturedSql.Format.ShouldContain("r.TimeStamp <= {2}");
        capturedSql.GetArguments().ShouldBe(new object?[] { "%ABC%", WindowStart, WindowEnd });
    }

    /// <summary>
    /// #126 review C10 content-parity: a NULL window requests the content-preserving UNBOUNDED SQL variant
    /// (the pre-#147 whole-ledger scan shape) — used by the IsMaster report whose barcode rows carry no date
    /// bound, pending the #119 F6 PO ruling on the deferred "IsMaster unbounded scan" item. The emitted SQL must
    /// carry NO TimeStamp bound and only the pattern parameter.
    /// </summary>
    [Fact]
    public async Task GetMatchingBarCodeIdsAsync_WithNullWindow_UsesTheUnboundedSqlVariant()
    {
        // Arrange
        FormattableString? capturedSql = null;
        ArrangeLease([], sql => capturedSql = sql);

        // Act
        var result = await _sut.GetMatchingBarCodeIdsAsync("ABC", null, null, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        capturedSql.ShouldNotBeNull();
        capturedSql.Format.ShouldContain("EXISTS");
        capturedSql.Format.ShouldContain("r.Value LIKE {0}");
        capturedSql.Format.ShouldNotContain("TimeStamp");
        capturedSql.GetArguments().ShouldBe(new object?[] { "%ABC%" });
    }

    /// <summary>
    /// The window gate mirrors the pre-#229 shape exactly: BOTH ends must be present to bound the scan, so a
    /// half-open window (either end null) falls back to the content-preserving unbounded variant.
    /// </summary>
    [Fact]
    public async Task GetMatchingBarCodeIdsAsync_WithHalfOpenWindow_FallsBackToTheUnboundedSqlVariant()
    {
        // Arrange
        FormattableString? capturedSql = null;
        ArrangeLease([], sql => capturedSql = sql);

        // Act — only the start is provided; the gate requires BOTH ends.
        var result = await _sut.GetMatchingBarCodeIdsAsync("ABC", WindowStart, null, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        capturedSql.ShouldNotBeNull();
        capturedSql.Format.ShouldNotContain("TimeStamp");
        capturedSql.GetArguments().ShouldBe(new object?[] { "%ABC%" });
    }

    /// <summary>
    /// LIKE-literalness: the search substring must match EF's <c>string.Contains</c> translation, so SQL Server
    /// LIKE metacharacters (<c>[</c>, <c>%</c>, <c>_</c>) are neutralised with <c>[]</c> character-class escaping
    /// BEFORE the <c>%…%</c> wrap — a search containing them matches literally instead of acting as a wildcard.
    /// </summary>
    [Fact]
    public async Task GetMatchingBarCodeIdsAsync_EscapesLikeMetacharactersInThePattern()
    {
        // Arrange
        FormattableString? capturedSql = null;
        ArrangeLease([], sql => capturedSql = sql);

        // Act
        var result = await _sut.GetMatchingBarCodeIdsAsync("50%_[x]", null, null, TestContext.Current.CancellationToken);

        // Assert — [ first ("[[]"), then % ("[%]"), then _ ("[_]"); "]" needs no escape.
        result.IsSuccess.ShouldBeTrue();
        capturedSql.ShouldNotBeNull();
        capturedSql.GetArguments().ShouldBe(new object?[] { "%50[%][_][[]x]%" });
    }

    /// <summary>
    /// #119 (F2) fail LOUD: an infrastructure failure acquiring the raw-SQL lease must propagate as a FAILURE
    /// carrying the repository's own errors — never collapsed into the genuine-no-match message (which would
    /// silently render a wrong report).
    /// </summary>
    [Fact]
    public async Task GetMatchingBarCodeIdsAsync_InfrastructureFailure_PropagatesTheRepositoryErrors()
    {
        // Arrange
        _cycleRepository.FromSqlAsync(Arg.Any<FormattableString>(), Arg.Any<CancellationToken>())
            .Returns(Result<OwnedQueryable<Cycle>>.WithFailure("Ledger unavailable: connection refused"));

        // Act
        var result = await _sut.GetMatchingBarCodeIdsAsync("ABC", WindowStart, WindowEnd, TestContext.Current.CancellationToken);

        // Assert — a failure with the REAL error, not a fabricated no-match.
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("Ledger unavailable"));
    }

    /// <summary>
    /// #119 (F2): a genuine no-match (the projected query succeeds and returns zero rows) is an EMPTY success
    /// set — the report intersects to empty — never a failure that aborts the whole report query.
    /// </summary>
    [Fact]
    public async Task GetMatchingBarCodeIdsAsync_GenuineNoMatch_ReturnsEmptySuccessSet()
    {
        // Arrange
        ArrangeLease([], _ => { });

        // Act
        var result = await _sut.GetMatchingBarCodeIdsAsync("NO-SUCH-VALUE", WindowStart, WindowEnd, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeEmpty();
    }

    /// <summary>
    /// Happy path regression pin: the cycles the projected query returns resolve to their DISTINCT owning
    /// barcode ids — two matched cycles of the same barcode yield the id once, alongside any other owner.
    /// </summary>
    [Fact]
    public async Task GetMatchingBarCodeIdsAsync_WithMatchingRows_ReturnsTheDistinctOwningBarCodeIds()
    {
        // Arrange — cycles 7 and 8 both belong to barcode 100; cycle 9 belongs to barcode 200.
        List<Cycle> cycles =
        [
            new() { CycleId = new CycleId(7), BarCodeId = new BarCodeId(100) },
            new() { CycleId = new CycleId(8), BarCodeId = new BarCodeId(100) },
            new() { CycleId = new CycleId(9), BarCodeId = new BarCodeId(200) },
        ];
        ArrangeLease(cycles, _ => { });

        // Act
        var result = await _sut.GetMatchingBarCodeIdsAsync("ABC", WindowStart, WindowEnd, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBe(new HashSet<int> { 100, 200 });
    }

    /// <summary>
    /// Existing contract kept: a blank search is answered with an empty success set (an empty report), never a
    /// failure and never an unfiltered pass-through — and the raw-SQL seam is never touched.
    /// </summary>
    [Fact]
    public async Task GetMatchingBarCodeIdsAsync_BlankSearch_ReturnsEmptySuccessSet()
    {
        // Act
        var result = await _sut.GetMatchingBarCodeIdsAsync("   ", WindowStart, WindowEnd, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeEmpty();
        await _cycleRepository.DidNotReceive().FromSqlAsync(Arg.Any<FormattableString>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Stubs the raw-SQL seam: captures the emitted <see cref="FormattableString"/> and leases an async-capable
    /// in-memory cycle queryable (null owner — an in-memory stand-in owns no pooled context).
    /// </summary>
    /// <param name="cycles">The cycles the fake projected query returns.</param>
    /// <param name="capture">Receives the emitted SQL for shape/parameter assertions.</param>
    private void ArrangeLease(List<Cycle> cycles, Action<FormattableString> capture)
    {
        _cycleRepository.FromSqlAsync(Arg.Do(capture), Arg.Any<CancellationToken>())
            .Returns(_ => Result<OwnedQueryable<Cycle>>.Success(
                new OwnedQueryable<Cycle>(new TestAsyncEnumerable<Cycle>(cycles), null)));
    }
}
