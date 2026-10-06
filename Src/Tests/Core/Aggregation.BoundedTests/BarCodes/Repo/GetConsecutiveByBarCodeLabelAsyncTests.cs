// <copyright file="GetConsecutiveByBarCodeLabelAsyncTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.ValueObjects;

namespace IndTrace.Aggregation.BoundedTests.BarCodes.Repo;

/// <summary>
/// Real-DB (EF Core InMemory) behavioural coverage for the consecutive-label generator
/// <see cref="BarCodeService.GetConsecutiveByBarCodeLabelAsync"/> after the P0-14 hardening (#74/#75):
/// the sequence is scoped to the part's product, derived from the label (not the global BarCodeId), seeds at
/// 1 on an empty scope, and FAILS LOUD on exhaustion instead of wrapping at 10000.
/// </summary>
public class GetConsecutiveByBarCodeLabelAsyncTests : DependenciesFactory
{
    public GetConsecutiveByBarCodeLabelAsyncTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    private BarCodeService CreateService() =>
        new(DpBarCodeRepository, DpRoProductRepository);

    private async Task<int> SeedProductAsync(string partNumber, CancellationToken cancellationToken)
    {
        var product = Product.Create(partNumber, $"Name-{partNumber}", ActiveStatus.Active, 1, string.Empty, string.Empty, string.Empty, 0, string.Empty, 0, 0);
        product.IsSuccess.ShouldBeTrue();
        product.Value.ShouldNotBeNull();

        await DpProductRepository.AddAsync(product.Value, cancellationToken);
        await DpProductRepository.CommitAsync(cancellationToken);

        // InMemory stamps the identity on commit.
        return product.Value.ProductId.Value;
    }

    private async Task SeedBarCodeAsync(int barCodeId, int productId, string label, CancellationToken cancellationToken)
    {
        var barcode = new BarCode
        {
            BarCodeId = new BarCodeId(barCodeId),
            ProductId = new ProductId(productId),
            Label = BarCodeLabel.FromPersisted(label),
            CreatedOn = DpDateTimeMachine.Now,
            ModifiedOn = DpDateTimeMachine.Now,
        };

        await DpBarCodeRepository.AddAsync(barcode, cancellationToken);
        await DpBarCodeRepository.CommitAsync(cancellationToken);
    }

    /// <summary>
    /// The generator HONOURS its part-number parameter: it returns the next consecutive scoped to THIS product,
    /// ignoring a higher label that belongs to a DIFFERENT product.
    /// </summary>
    [Fact]
    public async Task GetConsecutive_IsScopedToProduct_NotGlobalMax()
    {
        await Initialization;
        var ct = TestContext.Current.CancellationToken;
        DpDateTimeMachine.SetDateTimeNow(new DateTimeOffset(2020, 06, 06, 06, 06, 06, 6, TimeSpan.Zero));

        var mineId = await SeedProductAsync("P74-MINE", ct);
        var otherId = await SeedProductAsync("P74-OTHER", ct);

        // My product's highest scoped label ends in 0100.
        await SeedBarCodeAsync(74010, mineId, "L1AL7401MINE0099", ct);
        await SeedBarCodeAsync(74011, mineId, "L1AL7401MINE0100", ct);

        // A DIFFERENT product with a far higher consecutive AND a higher BarCodeId — must NOT leak into my scope.
        await SeedBarCodeAsync(74999, otherId, "L1AL7401OTHR0500", ct);

        var result = await CreateService().GetConsecutiveByBarCodeLabelAsync("P74-MINE", new List<string> { "L1AL7401MINE0001" }, rule: null, ct);

        result.IsSuccess.ShouldBeTrue($"errors: [{string.Join(";", result.Errors ?? new List<string>())}]");
        result.Value.ShouldBe(101); // 0100 + 1, scoped — NOT 501 and NOT a BarCodeId-derived value
    }

    /// <summary>
    /// A rule configured with a 5-digit auto-increment field (NumericAction with component.Length = 5) mints
    /// 5-digit trailing consecutives. The width-aware generator reads the FULL trailing digit-run (12345 -> 12346),
    /// not the fixed trailing-4 ("2345") that would re-mint an already-used 0xxxx identity.
    /// </summary>
    [Fact]
    public async Task GetConsecutive_FiveDigitField_IncrementsFullWidth_NotTrailingFour()
    {
        await Initialization;
        var ct = TestContext.Current.CancellationToken;
        DpDateTimeMachine.SetDateTimeNow(new DateTimeOffset(2020, 06, 06, 06, 06, 06, 6, TimeSpan.Zero));

        var productId = await SeedProductAsync("P74-WIDE5", ct);
        await SeedBarCodeAsync(74200, productId, "L1AL7401WIDE12345", ct);

        var result = await CreateService().GetConsecutiveByBarCodeLabelAsync("P74-WIDE5", new List<string> { "L1AL7401WIDE00001" }, rule: null, ct);

        result.IsSuccess.ShouldBeTrue($"errors: [{string.Join(";", result.Errors ?? new List<string>())}]");
        result.Value.ShouldBe(12346); // full 5-digit run + 1 — NOT 2346
    }

    /// <summary>
    /// Exhaustion is evaluated at the TRUE field width: a 5-digit max of 99999 fails loud (10^5 - 1 = 99999 is
    /// the last legal value), never wrapping into a duplicate identity.
    /// </summary>
    [Fact]
    public async Task GetConsecutive_FiveDigitAtLimit_FailsLoud()
    {
        await Initialization;
        var ct = TestContext.Current.CancellationToken;
        DpDateTimeMachine.SetDateTimeNow(new DateTimeOffset(2020, 06, 06, 06, 06, 06, 6, TimeSpan.Zero));

        var productId = await SeedProductAsync("P74-WIDE5LIMIT", ct);
        await SeedBarCodeAsync(74201, productId, "L1AL7401WIDE99999", ct);

        var result = await CreateService().GetConsecutiveByBarCodeLabelAsync("P74-WIDE5LIMIT", new List<string> { "L1AL7401WIDE00001" }, rule: null, ct);

        result.IsFailure.ShouldBeTrue("A 5-digit consecutive at 99999 must fail loud, never wrap.");
        result.Errors.ShouldContain(e => e.Contains("exhausted", StringComparison.Ordinal));
    }

    /// <summary>
    /// A label whose tail is non-numeric carries no consecutive field to increment and fails loud rather than
    /// guessing a sequence.
    /// </summary>
    [Fact]
    public async Task GetConsecutive_NonNumericSuffix_FailsLoud()
    {
        await Initialization;
        var ct = TestContext.Current.CancellationToken;
        DpDateTimeMachine.SetDateTimeNow(new DateTimeOffset(2020, 06, 06, 06, 06, 06, 6, TimeSpan.Zero));

        var productId = await SeedProductAsync("P74-NONNUM", ct);
        await SeedBarCodeAsync(74202, productId, "L1AL7401NONNWXYZ", ct);

        var result = await CreateService().GetConsecutiveByBarCodeLabelAsync("P74-NONNUM", new List<string> { "L1AL7401NONN0001" }, rule: null, ct);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("does not end in a numeric consecutive", StringComparison.Ordinal));
    }

    /// <summary>
    /// An empty scope (product exists, no barcode yet) seeds the sequence at the first consecutive (1),
    /// matching the master-label template ending in "0001".
    /// </summary>
    [Fact]
    public async Task GetConsecutive_EmptyScope_SeedsAtOne()
    {
        await Initialization;
        var ct = TestContext.Current.CancellationToken;
        DpDateTimeMachine.SetDateTimeNow(new DateTimeOffset(2020, 06, 06, 06, 06, 06, 6, TimeSpan.Zero));

        await SeedProductAsync("P74-EMPTY", ct);

        var result = await CreateService().GetConsecutiveByBarCodeLabelAsync("P74-EMPTY", new List<string> { "L1AL7401MTLA0001" }, rule: null, ct);

        result.IsSuccess.ShouldBeTrue($"errors: [{string.Join(";", result.Errors ?? new List<string>())}]");
        result.Value.ShouldBe(1);
    }

    /// <summary>
    /// Exhaustion FAILS LOUD: a scoped label ending in 9999 must return a failure (not wrap to 0), because a
    /// wrapped consecutive would collide with an existing label — corrupting product identity.
    /// </summary>
    [Fact]
    public async Task GetConsecutive_AtLimit_FailsLoud_DoesNotWrapToZero()
    {
        await Initialization;
        var ct = TestContext.Current.CancellationToken;
        DpDateTimeMachine.SetDateTimeNow(new DateTimeOffset(2020, 06, 06, 06, 06, 06, 6, TimeSpan.Zero));

        var productId = await SeedProductAsync("P74-LIMIT", ct);
        await SeedBarCodeAsync(74777, productId, "L1AL7401LIMT9999", ct);

        var result = await CreateService().GetConsecutiveByBarCodeLabelAsync("P74-LIMIT", new List<string> { "L1AL7401LIMT0001" }, rule: null, ct);

        result.IsFailure.ShouldBeTrue("A consecutive at 9999 must fail loud, never wrap to 0.");
        result.Errors.ShouldContain(e => e.Contains("exhausted", StringComparison.Ordinal));
    }

    /// <summary>
    /// A missing product cannot be scoped and fails loud (no silent global fallback).
    /// </summary>
    [Fact]
    public async Task GetConsecutive_UnknownProduct_Fails()
    {
        await Initialization;
        var ct = TestContext.Current.CancellationToken;

        var result = await CreateService().GetConsecutiveByBarCodeLabelAsync("P74-DOES-NOT-EXIST", new List<string> { "X" }, rule: null, ct);

        result.IsFailure.ShouldBeTrue();
    }

    /// <summary>
    /// DOCUMENTS THE RESIDUAL RACE. The generator is a read-then-increment, so N concurrent callers that observe
    /// the same committed state all compute the SAME next value. Uniqueness is NOT guaranteed here — it is enforced
    /// downstream by the UNIQUE index IDX.IndTraceData.BarCodes.Label, which rejects the colliding INSERT on real
    /// SQL (turning a would-be duplicate identity into a loud create failure). The EF InMemory provider used here
    /// does not enforce that index, so this test asserts only the (expected) collision at the generator level.
    /// </summary>
    [Fact]
    public async Task GetConsecutive_ConcurrentCallers_ObserveSameValue_ResidualRaceIsDbEnforced()
    {
        await Initialization;
        var ct = TestContext.Current.CancellationToken;
        DpDateTimeMachine.SetDateTimeNow(new DateTimeOffset(2020, 06, 06, 06, 06, 06, 6, TimeSpan.Zero));

        var productId = await SeedProductAsync("P74-RACE", ct);
        await SeedBarCodeAsync(74500, productId, "L1AL7401RACE0100", ct);

        var service = CreateService();
        var tasks = Enumerable.Range(0, 8)
            .Select(_ => service.GetConsecutiveByBarCodeLabelAsync("P74-RACE", new List<string> { "L1AL7401RACE0001" }, rule: null, ct))
            .ToList();

        var results = await Task.WhenAll(tasks);

        results.ShouldAllBe(r => r.IsSuccess);
        // All observed the same committed max -> all computed the same next value (the race the DB index guards).
        results.Select(r => r.Value).Distinct().Count().ShouldBe(1);
        results[0].Value.ShouldBe(101);
    }

    // ---- Issue #186: label-shape-aware width derivation (rule metadata is the source of truth) ----

    /// <summary>
    /// The QA45 corpus rule (RuleId 11 in the QA seed): QA + 4 + 5 + partNumber(6-9) + yy + ddd +
    /// autoIncrement(length 4). For a digit part number the minted label is ALL digits after "QA".
    /// </summary>
    private const string Qa45RuleJson = @"{""ruleId"": ""V3"",""ruleFunction"": [""lineIdentifier"", ""lineNumber"", ""fixedPart"", ""partNumber"", ""lastTwoYearDigits"", ""julianDay"", ""autoIncrement""],""components"": {""lineIdentifier"": {""action"": ""string"",""origin"": ""fixed"",""value"": ""QA""},""lineNumber"": {""action"": ""string"",""origin"": ""fixed"",""value"": ""4""},""fixedPart"": {""action"": ""string"",""origin"": ""fixed"",""value"": ""5""},""partNumber"": {""action"": ""string"",""origin"": ""program"",""lengthMin"": 6,""lengthMax"": 9},""lastTwoYearDigits"": {""action"": ""lastTwoYearDigits"",""origin"": ""program""},""julianDay"": {""action"": ""julianDay"",""origin"": ""program""},""autoIncrement"": {""action"": ""numeric"",""origin"": ""program"",""length"": 4,""incremental"": true}}}";

    private static Rule Qa45Rule() => new() { RuleId = 11, RuleJson = Qa45RuleJson, IsActive = true };

    /// <summary>
    /// #186 (a): a legacy all-digit QA45 label (15-digit trailing run, no non-digit boundary before the counter)
    /// derives its next consecutive from the RULE's configured counter width (4), not from the boundary heuristic
    /// that refused every legacy label.
    /// </summary>
    [Fact]
    public async Task GetConsecutive_LegacyAllDigitCorpus_WithRule_DerivesRuleWidth()
    {
        await Initialization;
        var ct = TestContext.Current.CancellationToken;
        DpDateTimeMachine.SetDateTimeNow(new DateTimeOffset(2026, 01, 20, 10, 0, 0, TimeSpan.Zero));

        var productId = await SeedProductAsync("918611", ct);
        // QA45 + 900290 (PN) + 26 (yy) + 020 (ddd) + 4711 (counter) — the shape from the QA45 legacy corpus.
        await SeedBarCodeAsync(18600, productId, "QA45918611260204711", ct);

        var result = await CreateService().GetConsecutiveByBarCodeLabelAsync("918611", new List<string> { "QA45918611252970001" }, Qa45Rule(), ct);

        result.IsSuccess.ShouldBeTrue($"errors: [{string.Join(";", result.Errors ?? new List<string>())}]");
        result.Value.ShouldBe(4712); // rule width 4 -> counter "4711" + 1
    }

    /// <summary>
    /// #186 (b): the self-refusal loop, end to end against the real repositories. Derive (empty scope -> 1),
    /// mint with the production formatter, persist the minted label, derive again -> 2. Before the fix, the
    /// second derivation refused the label the service itself had just produced, so create worked at most once
    /// per product per session.
    /// </summary>
    [Fact]
    public async Task GetConsecutive_OwnMintedLabel_WithRule_DoesNotSelfRefuse()
    {
        await Initialization;
        var ct = TestContext.Current.CancellationToken;
        DpDateTimeMachine.SetDateTimeNow(new DateTimeOffset(2026, 01, 20, 10, 0, 0, TimeSpan.Zero));

        var productId = await SeedProductAsync("918612", ct);
        var service = CreateService();
        var rule = Qa45Rule();

        // First create: empty scope seeds at 1.
        var first = await service.GetConsecutiveByBarCodeLabelAsync("918612", new List<string> { "QA45918612252970001" }, rule, ct);
        first.IsSuccess.ShouldBeTrue($"errors: [{string.Join(";", first.Errors ?? new List<string>())}]");
        first.Value.ShouldBe(1);

        // Mint the label exactly as production does and persist it.
        var executor = new CreateBarCodeDictionaryExecutor(DpDateTimeMachine);
        executor.ParseRuleFromJson(Qa45RuleJson).ShouldNotBeNull();
        executor.InitializeComponentActions().IsSuccess.ShouldBeTrue();
        var minted = executor.ApplyRuleCreateBarCode("918612", first.Value);
        minted.IsSuccess.ShouldBeTrue($"errors: [{string.Join(";", minted.Errors ?? new List<string>())}]");
        await SeedBarCodeAsync(18601, productId, minted.Value ?? string.Empty, ct);

        // Second create: the just-minted all-digit label must seed the NEXT consecutive, not be refused.
        var second = await service.GetConsecutiveByBarCodeLabelAsync("918612", new List<string> { "QA45918612252970001" }, rule, ct);
        second.IsSuccess.ShouldBeTrue($"errors: [{string.Join(";", second.Errors ?? new List<string>())}]");
        second.Value.ShouldBe(2);
    }

    /// <summary>
    /// #186 (c): the E2E sentinel label "QA45918611E2E0000" (non-digit boundary before the counter) still derives
    /// as before when the rule is supplied — the sentinel keeps working but is no longer necessary.
    /// </summary>
    [Fact]
    public async Task GetConsecutive_SentinelBoundaryLabel_WithRule_StillDerivesOne()
    {
        await Initialization;
        var ct = TestContext.Current.CancellationToken;
        DpDateTimeMachine.SetDateTimeNow(new DateTimeOffset(2026, 01, 20, 10, 0, 0, TimeSpan.Zero));

        var productId = await SeedProductAsync("918613", ct);
        await SeedBarCodeAsync(18602, productId, "QA45918613E2E0000", ct);

        var result = await CreateService().GetConsecutiveByBarCodeLabelAsync("918613", new List<string> { "QA45918613E2E0000" }, Qa45Rule(), ct);

        result.IsSuccess.ShouldBeTrue($"errors: [{string.Join(";", result.Errors ?? new List<string>())}]");
        result.Value.ShouldBe(1); // "0000" + 1
    }
}
