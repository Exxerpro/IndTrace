// <copyright file="ReportsListQueryComposerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.BarCodes.Queries.Composers;

using IndTrace.Aggregation.BoundedTests.Services;
using IndTrace.Application.BarCodes.Queries.Composers;
using IndTrace.Domain.Models;
using IndTrace.Domain.ValueObjects;

/// <summary>
/// Behavioral tests for <see cref="ReportsListQueryComposer"/> (GitHub issue #80). Each test pins the FIXED
/// behavior of a single filter facet:
/// <list type="bullet">
/// <item>the LINE filter narrows to exactly the requested line's barcodes (a barcode belongs to a line only
/// through its <see cref="Product"/>), with a non-existent line failing SAFE (empty) rather than open;</item>
/// <item>the SHIFT windows partition the full 24h with no lost sliver (06:30 and 23:59:58 both belong to shift 3);</item>
/// <item>an unparseable STATE filter FAILS LOUD instead of silently returning every state.</item>
/// </list>
/// The line test requires EF-async-capable queryables (the composer calls <c>FirstOrDefaultAsync</c> /
/// <c>ToListAsync</c> over Lines/Products), so it runs against a real EF Core InMemory <see cref="IndTraceDbContext"/>;
/// the shift/state tests need no async materialization, so they use plain <c>List&lt;BarCode&gt;.AsQueryable()</c>.
/// </summary>
public sealed class ReportsListQueryComposerTests : IDisposable
{
    private readonly ITestOutputHelper output;
    private readonly ReportsListQueryComposer composer;
    private readonly string databaseName = $"ReportsListComposerTests_{Guid.NewGuid():N}";
    private IndTraceDbContext? context;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportsListQueryComposerTests"/> class.
    /// </summary>
    /// <param name="output">The xUnit output helper backing the Meziantou logger.</param>
    public ReportsListQueryComposerTests(ITestOutputHelper output)
    {
        this.output = output ?? throw new ArgumentNullException(nameof(output));
        this.composer = new ReportsListQueryComposer(XUnitLogger.CreateLogger<ReportsListQueryComposer>(output));
    }

    /// <summary>
    /// Line filter narrows to ONLY the requested line's barcodes. Two lines each own a product, and one barcode
    /// sits on each product; filtering by "LineA" must return only the barcode whose product is on LineA — never
    /// the cross-line barcode (the pre-fix data leak).
    /// </summary>
    [Fact]
    public async Task ComposeAsync_LineFilter_ReturnsOnlyRequestedLinesBarCodes()
    {
        // Arrange
        var ctx = this.NewSeededContext();

        var customer = new Customer { CustomerId = 1, Name = "Cust" };
        var lineA = new Line { LineId = 1, Name = "LineA" };
        var lineB = new Line { LineId = 2, Name = "LineB" };

        var productA = Product.CreateFixture(productId: 101, partNumber: "PA", productName: "PA", lineId: 1);
        productA.Line = lineA;
        productA.Customer = customer;

        var productB = Product.CreateFixture(productId: 102, partNumber: "PB", productName: "PB", lineId: 2);
        productB.Line = lineB;
        productB.Customer = customer;

        var barCodeA = MakeBarCode(barCodeId: 1, productId: 101, createdOn: DateTime.Today.AddHours(8));
        var barCodeB = MakeBarCode(barCodeId: 2, productId: 102, createdOn: DateTime.Today.AddHours(8));

        await ctx.Set<Customer>().AddAsync(customer, TestContext.Current.CancellationToken);
        await ctx.Set<Line>().AddRangeAsync([lineA, lineB], TestContext.Current.CancellationToken);
        await ctx.Set<Product>().AddRangeAsync([productA, productB], TestContext.Current.CancellationToken);
        await ctx.Set<BarCode>().AddRangeAsync([barCodeA, barCodeB], TestContext.Current.CancellationToken);
        await ctx.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = WideOpenRequest();
        request.FilterByLine = true;
        request.Line = "LineA";

        var support = SupportFor(ctx);

        // Act
        var result = await this.composer.ComposeAsync(
            ctx.Set<BarCode>(), request, support, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        var rows = result.Value.ToList();
        rows.Count.ShouldBe(1);
        rows[0].ProductId.Value.ShouldBe(101);
        rows.ShouldNotContain(bc => bc.ProductId.Value == 102);
    }

    /// <summary>
    /// Line filter with a line that does not exist must FAIL SAFE — return an EMPTY set, never the whole
    /// unfiltered barcode set (the pre-fix "resolve to nothing -> return everything" leak).
    /// </summary>
    [Fact]
    public async Task ComposeAsync_LineFilter_NonExistentLine_ReturnsEmpty()
    {
        // Arrange
        var ctx = this.NewSeededContext();

        var customer = new Customer { CustomerId = 1, Name = "Cust" };
        var lineA = new Line { LineId = 1, Name = "LineA" };

        var productA = Product.CreateFixture(productId: 101, partNumber: "PA", productName: "PA", lineId: 1);
        productA.Line = lineA;
        productA.Customer = customer;

        var barCodeA = MakeBarCode(barCodeId: 1, productId: 101, createdOn: DateTime.Today.AddHours(8));

        await ctx.Set<Customer>().AddAsync(customer, TestContext.Current.CancellationToken);
        await ctx.Set<Line>().AddAsync(lineA, TestContext.Current.CancellationToken);
        await ctx.Set<Product>().AddAsync(productA, TestContext.Current.CancellationToken);
        await ctx.Set<BarCode>().AddAsync(barCodeA, TestContext.Current.CancellationToken);
        await ctx.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = WideOpenRequest();
        request.FilterByLine = true;
        request.Line = "NoSuchLine";

        var support = SupportFor(ctx);

        // Act
        var result = await this.composer.ComposeAsync(
            ctx.Set<BarCode>(), request, support, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ToList().ShouldBeEmpty();
    }

    /// <summary>
    /// A barcode created at 06:30 — inside the span the pre-fix bounds lost (06:00-08:00 belonged to no shift) —
    /// belongs to shift 3 (23:00-07:00 across midnight) and NOT to shift 1 (07:00-15:00).
    /// </summary>
    [Fact]
    public async Task ComposeAsync_ShiftFilter_EarlyMorning0630_BelongsToShift3NotShift1()
    {
        // Arrange
        var createdOn = DateTime.Today.AddHours(6).AddMinutes(30);
        var barCode = MakeBarCode(barCodeId: 1, productId: 1, createdOn: createdOn);
        var baseQuery = new List<BarCode> { barCode }.AsQueryable();

        // Act
        var shift3 = await this.ComposeShiftAsync(baseQuery, shift: 3);
        var shift1 = await this.ComposeShiftAsync(baseQuery, shift: 1);

        // Assert
        shift3.Count.ShouldBe(1, "06:30 must be captured by shift 3 (was lost in the 06:00-08:00 gap).");
        shift1.Count.ShouldBe(0, "06:30 must NOT be captured by shift 1.");
    }

    /// <summary>
    /// A barcode created at 07:30 belongs to shift 1 (07:00-15:00).
    /// </summary>
    [Fact]
    public async Task ComposeAsync_ShiftFilter_0730_BelongsToShift1()
    {
        // Arrange
        var createdOn = DateTime.Today.AddHours(7).AddMinutes(30);
        var barCode = MakeBarCode(barCodeId: 1, productId: 1, createdOn: createdOn);
        var baseQuery = new List<BarCode> { barCode }.AsQueryable();

        // Act
        var shift1 = await this.ComposeShiftAsync(baseQuery, shift: 1);

        // Assert
        shift1.Count.ShouldBe(1);
    }

    /// <summary>
    /// A barcode created at 23:59:58 — inside the final ~3.6s the pre-fix <c>&lt;= 23.999h</c> upper bound dropped —
    /// belongs to shift 3.
    /// </summary>
    [Fact]
    public async Task ComposeAsync_ShiftFilter_EndOfDay235958_BelongsToShift3()
    {
        // Arrange
        var createdOn = DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(58);
        var barCode = MakeBarCode(barCodeId: 1, productId: 1, createdOn: createdOn);
        var baseQuery = new List<BarCode> { barCode }.AsQueryable();

        // Act
        var shift3 = await this.ComposeShiftAsync(baseQuery, shift: 3);

        // Assert
        shift3.Count.ShouldBe(1, "23:59:58 must be captured by shift 3 (was dropped by the <=23.999h bound).");
    }

    /// <summary>
    /// An unparseable state filter must NOT leak every state (the issue #80 regression: silently dropping the
    /// filter and returning EVERY barcode as if the requested state had been applied). With the shipped
    /// <c>EnumModel.FromName</c> semantics an unknown name resolves to the <see cref="FlowStatus.Invalid"/>
    /// sentinel rather than throwing, so the composer narrows the query to the Invalid bucket — none of the
    /// seeded (Created / InProcess / Finished) barcodes survive, proving the unparseable value did not behave as
    /// "no filter". (See the FAIL-LOUD caveat reported for this file: the composer's try/catch fail-loud path is
    /// unreachable for FlowStatus because FromName does not throw for garbage names.)
    /// </summary>
    [Fact]
    public async Task ComposeAsync_StateFilter_UnparseableState_FailsLoud()
    {
        // Arrange — three barcodes in three DISTINCT non-Invalid states.
        var barCodes = new List<BarCode>
        {
            MakeBarCodeWithStatus(FlowStatus.Created),
            MakeBarCodeWithStatus(FlowStatus.InProcess),
            MakeBarCodeWithStatus(FlowStatus.Finished),
        };
        var baseQuery = barCodes.AsQueryable();

        var request = WideOpenRequest();
        request.FilterByState = true;
        request.State = "NOT_A_REAL_STATE";

        // Act
        var result = await this.composer.ComposeAsync(
            baseQuery, request, EmptySupport(), TestContext.Current.CancellationToken);

        // Assert — #80: an unrecognized state must FAIL LOUD, not silently drop the filter (which returned every
        // state) nor silently resolve to the Invalid sentinel (which returned zero rows and quietly misreported).
        // EnumModel.FromName maps garbage to FlowStatus.Invalid without throwing, so the composer explicitly
        // detects that and surfaces a failure the caller/UI can act on.
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("Invalid state filter", StringComparison.Ordinal));
    }

    /// <summary>
    /// A valid state filter (a real <see cref="FlowStatus"/> name) narrows to exactly the barcodes in that state —
    /// proving the state facet actually filters (and contrasting the unparseable case above).
    /// </summary>
    [Fact]
    public async Task ComposeAsync_StateFilter_ValidState_ReturnsOnlyMatchingState()
    {
        // Arrange
        var barCodes = new List<BarCode>
        {
            MakeBarCodeWithStatus(FlowStatus.Created),
            MakeBarCodeWithStatus(FlowStatus.InProcess),
        };
        var baseQuery = barCodes.AsQueryable();

        var request = WideOpenRequest();
        request.FilterByState = true;
        request.State = nameof(FlowStatus.Created);

        // Act
        var result = await this.composer.ComposeAsync(
            baseQuery, request, EmptySupport(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        var rows = result.Value.ToList();
        rows.Count.ShouldBe(1);
        rows[0].FlowStatus.ShouldBe(FlowStatus.Created);
    }

    /// <summary>
    /// F7 (#80): a whitespace-padded valid state name (e.g. <c>" Created "</c>) must be TRIMMED before
    /// <see cref="EnumModel.FromName{T}(string)"/> — an exact-match lookup — so it resolves to the real
    /// <see cref="FlowStatus.Created"/> and filters, rather than resolving to the Invalid sentinel and being
    /// wrongly failed-loud as unrecognized. Proves the trim accepts a padded valid state.
    /// </summary>
    [Fact]
    public async Task ComposeAsync_StateFilter_PaddedValidState_IsTrimmedAndFilters()
    {
        // Arrange
        var barCodes = new List<BarCode>
        {
            MakeBarCodeWithStatus(FlowStatus.Created),
            MakeBarCodeWithStatus(FlowStatus.InProcess),
        };
        var baseQuery = barCodes.AsQueryable();

        var request = WideOpenRequest();
        request.FilterByState = true;
        request.State = $"  {nameof(FlowStatus.Created)}  ";

        // Act
        var result = await this.composer.ComposeAsync(
            baseQuery, request, EmptySupport(), TestContext.Current.CancellationToken);

        // Assert — padded valid name accepted (not failed-loud) and narrows to exactly the Created barcode.
        result.IsSuccess.ShouldBeTrue(string.Join("; ", result.Errors));
        result.Value.ShouldNotBeNull();
        var rows = result.Value.ToList();
        rows.Count.ShouldBe(1);
        rows[0].FlowStatus.ShouldBe(FlowStatus.Created);
    }

    /// <inheritdoc/>
    public void Dispose() => this.context?.Dispose();

    /// <summary>
    /// Builds a request whose non-master date window spans all of time, so the always-applied
    /// <c>ModifiedOn</c> date-range filter (the non-master path) never excludes a seeded row.
    /// </summary>
    private static GetReportsListQuery WideOpenRequest() =>
        new()
        {
            IsMaster = false,
            StartDate = DateTime.MinValue,
            EndDate = DateTime.MaxValue,
        };

    /// <summary>
    /// Support queries backed by the seeded context's DbSets (EF-async capable) for the line-filter path.
    /// </summary>
    private static ReportsSupportQueries SupportFor(IndTraceDbContext ctx) =>
        new(ctx.Set<MasterLabel>(), ctx.Set<Customer>(), ctx.Set<Product>(), ctx.Set<Line>());

    /// <summary>
    /// Support queries over empty in-memory lists for facets (shift/state) that never touch the support sets.
    /// </summary>
    private static ReportsSupportQueries EmptySupport() =>
        new(
            new List<MasterLabel>().AsQueryable(),
            new List<Customer>().AsQueryable(),
            new List<Product>().AsQueryable(),
            new List<Line>().AsQueryable());

    /// <summary>
    /// Creates a barcode with an explicit id (InMemory needs a distinct key) at the given creation time.
    /// </summary>
    private static BarCode MakeBarCode(int barCodeId, int productId, DateTime createdOn) =>
        new BarCode
        {
            BarCodeId = new BarCodeId(barCodeId),
            ProductId = new ProductId(productId),
            MachineId = new MachineId(1),
            Label = BarCodeLabel.FromPersisted($"BC-{barCodeId}"),
            CreatedOn = createdOn,
            ModifiedOn = createdOn,
        };

    /// <summary>
    /// Creates a barcode seeded directly into a given <see cref="FlowStatus"/> via the in-domain fixture seam
    /// (the status setter is <c>private set</c>). Used by the state-filter tests, which only care about status.
    /// </summary>
    private static BarCode MakeBarCodeWithStatus(FlowStatus status) =>
        BarCode.CreateFixture(BarCodeLabel.FromPersisted($"BC-{status.Name}"), status, PartStatus.Ok);

    private async Task<List<BarCode>> ComposeShiftAsync(IQueryable<BarCode> baseQuery, int shift)
    {
        var request = WideOpenRequest();
        request.FilterByShift = true;
        request.Shift = shift;

        var result = await this.composer.ComposeAsync(
            baseQuery, request, EmptySupport(), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        return result.Value.ToList();
    }

    private IndTraceDbContext NewSeededContext()
    {
        var options = new DbContextOptionsBuilder<IndTraceDbContext>()
            .UseInMemoryDatabase(this.databaseName)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
            .Options;

        var ctx = new IndTraceDbContext(options);
        ctx.SetTestingInterfaces(new TesterUserService(), new DateTimeMachine());
        ctx.Database.EnsureCreated();
        this.context = ctx;
        return ctx;
    }
}
