// <copyright file="BarCodeServiceTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Services
{
    /// <summary>
    /// Unit tests for BarCodeService implementation
    /// </summary>
    public class BarCodeServiceTests
    {
        private readonly IRepository<BarCode> _barCodeRepository = null!;
        private readonly IReadOnlyRepository<Product> _productRepository = null!;
        private readonly BarCodeService _service = null!;

        public BarCodeServiceTests()
        {
            _barCodeRepository = Substitute.For<IRepository<BarCode>>();
            _productRepository = Substitute.For<IReadOnlyRepository<Product>>();
            _service = new BarCodeService(_barCodeRepository, _productRepository);
        }

        /// <summary>
        /// Configures the product repository substitute to resolve the part number to a product with the given id.
        /// </summary>
        private void GivenProduct(int productId)
        {
            var product = new Product { ProductId = new ProductId(productId) };
            _productRepository.FirstOrDefaultAsync(Arg.Any<Specification<Product>>(), Arg.Any<CancellationToken>())
                .Returns(Result<Product?>.Success(product));
        }

        /// <summary>
        /// Tests that Constructor creates instance with valid dependencies
        /// </summary>
        [Fact]
        public void Constructor_WithValidDependencies_ShouldCreateInstance()
        {
            // Arrange & Act
            var service = new BarCodeService(_barCodeRepository, _productRepository);

            // Assert
            service.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that Constructor throws ArgumentNullException with null barCodeRepository
        /// </summary>
        [Fact]
        public void Constructor_WithNullBarCodeRepository_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Should.Throw<ArgumentNullException>(() =>
                new BarCodeService(null!, _productRepository));
        }

        /// <summary>
        /// Tests that Constructor throws ArgumentNullException with null productRepository
        /// </summary>
        [Fact]
        public void Constructor_WithNullProductRepository_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Should.Throw<ArgumentNullException>(() =>
                new BarCodeService(_barCodeRepository, null!));
        }

        /// <summary>
        /// The next consecutive is derived from the trailing sequence digits of the highest label IN SCOPE and
        /// incremented by one — NOT from the global BarCodeId.
        /// </summary>
        [Fact]
        public async Task GetConsecutiveByBarCodeLabelAsync_WithValidData_ShouldReturnNextScopedConsecutive()
        {
            // Arrange: the scoped max label ends in "0100"; BarCodeId is deliberately a DIFFERENT number to prove
            // the sequence is read from the label, not from the identity column.
            GivenProduct(productId: 42);
            var barCode = new BarCode { BarCodeId = new BarCodeId(99999), Label = BarCodeLabel.FromPersisted("TEST-0100") };
            _barCodeRepository.FirstOrDefaultAsync(Arg.Any<Specification<BarCode>>(), Arg.Any<CancellationToken>())
                .Returns(Result<BarCode?>.Success(barCode));

            // Act
            var response = await _service.GetConsecutiveByBarCodeLabelAsync("PART-123", new List<string> { "MASTER" }, rule: null, TestContext.Current.CancellationToken);

            // Assert
            response.IsSuccess.ShouldBeTrue();
            response.Value.ShouldBe(101); // parsed "0100" + 1 — NOT 99999 + 1
        }

        /// <summary>
        /// A rule configured with a 5-digit auto-increment (NumericAction with component.Length = 5) mints a
        /// 5-digit trailing field. The width-aware extractor must read the FULL trailing digit-run (12345), not
        /// the fixed trailing-4 ("2345") that would re-mint an already-used 0xxxx identity.
        /// </summary>
        [Fact]
        public async Task GetConsecutiveByBarCodeLabelAsync_WithFiveDigitField_ShouldIncrementFullWidth()
        {
            // Arrange
            GivenProduct(productId: 42);
            var barCode = new BarCode { BarCodeId = new BarCodeId(7), Label = BarCodeLabel.FromPersisted("L1AL7401MTLA12345") };
            _barCodeRepository.FirstOrDefaultAsync(Arg.Any<Specification<BarCode>>(), Arg.Any<CancellationToken>())
                .Returns(Result<BarCode?>.Success(barCode));

            // Act — master template carries the same 5-digit field ("00001").
            var response = await _service.GetConsecutiveByBarCodeLabelAsync("PART-123", new List<string> { "L1AL7401MTLA00001" }, rule: null, TestContext.Current.CancellationToken);

            // Assert
            response.IsSuccess.ShouldBeTrue($"errors: [{string.Join(";", response.Errors ?? new List<string>())}]");
            response.Value.ShouldBe(12346); // full 5-digit run + 1 — NOT the trailing-4 "2345" -> 2346
        }

        /// <summary>
        /// Exhaustion is evaluated at the TRUE field width: a 5-digit max of 99999 has no next value inside the
        /// 5-digit field and must fail loud (10^5 - 1 = 99999 is the last legal value).
        /// </summary>
        [Fact]
        public async Task GetConsecutiveByBarCodeLabelAsync_AtFiveDigitLimit_ShouldFailLoud()
        {
            // Arrange
            GivenProduct(productId: 42);
            var barCode = new BarCode { BarCodeId = new BarCodeId(7), Label = BarCodeLabel.FromPersisted("L1AL7401MTLA99999") };
            _barCodeRepository.FirstOrDefaultAsync(Arg.Any<Specification<BarCode>>(), Arg.Any<CancellationToken>())
                .Returns(Result<BarCode?>.Success(barCode));

            // Act
            var response = await _service.GetConsecutiveByBarCodeLabelAsync("PART-123", new List<string> { "L1AL7401MTLA00001" }, rule: null, TestContext.Current.CancellationToken);

            // Assert
            response.IsFailure.ShouldBeTrue();
            response.Errors.ShouldContain(e => e.Contains("exhausted", StringComparison.Ordinal));
        }

        /// <summary>
        /// A label whose tail is non-numeric carries no consecutive field to increment and fails loud — the
        /// generator refuses to guess a sequence rather than mint a malformed identity.
        /// </summary>
        [Fact]
        public async Task GetConsecutiveByBarCodeLabelAsync_WithNonNumericSuffix_ShouldFailLoud()
        {
            // Arrange
            GivenProduct(productId: 42);
            var barCode = new BarCode { BarCodeId = new BarCodeId(7), Label = BarCodeLabel.FromPersisted("L1AL7401MTLAWXYZ") };
            _barCodeRepository.FirstOrDefaultAsync(Arg.Any<Specification<BarCode>>(), Arg.Any<CancellationToken>())
                .Returns(Result<BarCode?>.Success(barCode));

            // Act
            var response = await _service.GetConsecutiveByBarCodeLabelAsync("PART-123", new List<string> { "L1AL7401MTLA0001" }, rule: null, TestContext.Current.CancellationToken);

            // Assert
            response.IsFailure.ShouldBeTrue();
            response.Errors.ShouldContain(e => e.Contains("does not end in a numeric consecutive", StringComparison.Ordinal));
        }

        /// <summary>
        /// When the highest label's field width disagrees with the caller's master template width (e.g. the rule
        /// widened the field mid-sequence), the width is AMBIGUOUS and the generator fails loud rather than
        /// guessing which field to increment.
        /// </summary>
        [Fact]
        public async Task GetConsecutiveByBarCodeLabelAsync_WithMasterWidthMismatch_ShouldFailLoud()
        {
            // Arrange: highest label ends in a 5-digit run, master template ends in a 4-digit run.
            GivenProduct(productId: 42);
            var barCode = new BarCode { BarCodeId = new BarCodeId(7), Label = BarCodeLabel.FromPersisted("L1AL7401MTLA12345") };
            _barCodeRepository.FirstOrDefaultAsync(Arg.Any<Specification<BarCode>>(), Arg.Any<CancellationToken>())
                .Returns(Result<BarCode?>.Success(barCode));

            // Act
            var response = await _service.GetConsecutiveByBarCodeLabelAsync("PART-123", new List<string> { "L1AL7401MTLA0001" }, rule: null, TestContext.Current.CancellationToken);

            // Assert
            response.IsFailure.ShouldBeTrue();
            response.Errors.ShouldContain(e => e.Contains("Ambiguous consecutive width", StringComparison.Ordinal));
        }

        /// <summary>
        /// An empty scope (no barcode yet for the product) seeds the sequence at the first consecutive.
        /// </summary>
        [Fact]
        public async Task GetConsecutiveByBarCodeLabelAsync_WithEmptyScope_ShouldSeedAtFirstConsecutive()
        {
            // Arrange
            GivenProduct(productId: 42);
            _barCodeRepository.FirstOrDefaultAsync(Arg.Any<Specification<BarCode>>(), Arg.Any<CancellationToken>())
                .Returns(Result<BarCode?>.Success((BarCode?)null));

            // Act
            var response = await _service.GetConsecutiveByBarCodeLabelAsync("PART-123", new List<string> { "MASTER" }, rule: null, TestContext.Current.CancellationToken);

            // Assert
            response.IsSuccess.ShouldBeTrue();
            response.Value.ShouldBe(1);
        }

        /// <summary>
        /// Exhaustion fails LOUD: a scoped max ending in 9999 must NOT wrap to 0 (which would mint a duplicate
        /// identity) — it returns a failure so the caller widens the field or starts a new sequence.
        /// </summary>
        [Fact]
        public async Task GetConsecutiveByBarCodeLabelAsync_AtSequenceLimit_ShouldFailLoudNotWrap()
        {
            // Arrange
            GivenProduct(productId: 42);
            var barCode = new BarCode { BarCodeId = new BarCodeId(7), Label = BarCodeLabel.FromPersisted("TEST-9999") };
            _barCodeRepository.FirstOrDefaultAsync(Arg.Any<Specification<BarCode>>(), Arg.Any<CancellationToken>())
                .Returns(Result<BarCode?>.Success(barCode));

            // Act
            var response = await _service.GetConsecutiveByBarCodeLabelAsync("PART-123", new List<string> { "MASTER" }, rule: null, TestContext.Current.CancellationToken);

            // Assert
            response.IsFailure.ShouldBeTrue();
            response.Errors.ShouldContain(e => e.Contains("exhausted", StringComparison.Ordinal));
        }

        /// <summary>
        /// A missing product (the repository "no rows" sentinel) cannot scope and fails loud — not a silent
        /// global fallback, and not masked as an infrastructure error.
        /// </summary>
        [Fact]
        public async Task GetConsecutiveByBarCodeLabelAsync_WhenProductNotFound_ShouldFail()
        {
            // Arrange - the repository signals "no match" as a failure carrying the not-found sentinel.
            _productRepository.FirstOrDefaultAsync(Arg.Any<Specification<Product>>(), Arg.Any<CancellationToken>())
                .Returns(Result<Product?>.WithFailure("No matching entity found."));

            // Act
            var response = await _service.GetConsecutiveByBarCodeLabelAsync("PART-123", new List<string> { "MASTER" }, rule: null, TestContext.Current.CancellationToken);

            // Assert
            response.IsFailure.ShouldBeTrue();
            response.Errors.ShouldContain(e => e.Contains("No product found", StringComparison.Ordinal));
        }

        /// <summary>
        /// An empty scope signalled as the repository "no rows" sentinel (the way the real repository reports it)
        /// is treated as an empty sequence and seeds at 1 — NOT surfaced as a failure.
        /// </summary>
        [Fact]
        public async Task GetConsecutiveByBarCodeLabelAsync_WhenScopeEmptyViaSentinel_ShouldSeedAtOne()
        {
            // Arrange
            GivenProduct(productId: 42);
            _barCodeRepository.FirstOrDefaultAsync(Arg.Any<Specification<BarCode>>(), Arg.Any<CancellationToken>())
                .Returns(Result<BarCode?>.WithFailure("No matching entity found."));

            // Act
            var response = await _service.GetConsecutiveByBarCodeLabelAsync("PART-123", new List<string> { "MASTER" }, rule: null, TestContext.Current.CancellationToken);

            // Assert
            response.IsSuccess.ShouldBeTrue();
            response.Value.ShouldBe(1);
        }

        /// <summary>
        /// An infrastructure failure from the barcode repository is surfaced as a failure — NOT masked as an
        /// empty sequence (the pre-fix code mislabeled every failure "No BarCodes found for the given label").
        /// </summary>
        [Fact]
        public async Task GetConsecutiveByBarCodeLabelAsync_WhenRepositoryFails_ShouldSurfaceInfraFailure()
        {
            // Arrange
            GivenProduct(productId: 42);
            _barCodeRepository.FirstOrDefaultAsync(Arg.Any<Specification<BarCode>>(), Arg.Any<CancellationToken>())
                .Returns(Result<BarCode?>.WithFailure("Repository error"));

            // Act
            var response = await _service.GetConsecutiveByBarCodeLabelAsync("PART-123", new List<string> { "MASTER" }, rule: null, TestContext.Current.CancellationToken);

            // Assert
            response.IsFailure.ShouldBeTrue();
            response.Errors.ShouldContain("Repository error");
        }

        /// <summary>
        /// A blank part number cannot scope the sequence and fails loud.
        /// </summary>
        [Fact]
        public async Task GetConsecutiveByBarCodeLabelAsync_WithBlankPartNumber_ShouldFail()
        {
            // Act
            var response = await _service.GetConsecutiveByBarCodeLabelAsync(" ", new List<string> { "MASTER" }, rule: null, TestContext.Current.CancellationToken);

            // Assert
            response.IsFailure.ShouldBeTrue();
        }

        // ---- Issue #186: label-shape-aware width derivation (rule metadata is the source of truth) ----

        /// <summary>
        /// The QA45 corpus rule (RuleId 11 in the QA seed): QA + 4 + 5 + partNumber(6-9) + yy + ddd +
        /// autoIncrement(length 4). For a digit part number the minted label is ALL digits after "QA",
        /// so the boundary-based trailing-run heuristic over-captures (15-17 digit runs).
        /// </summary>
        private const string Qa45RuleJson = @"{""ruleId"": ""V3"",""ruleFunction"": [""lineIdentifier"", ""lineNumber"", ""fixedPart"", ""partNumber"", ""lastTwoYearDigits"", ""julianDay"", ""autoIncrement""],""components"": {""lineIdentifier"": {""action"": ""string"",""origin"": ""fixed"",""value"": ""QA""},""lineNumber"": {""action"": ""string"",""origin"": ""fixed"",""value"": ""4""},""fixedPart"": {""action"": ""string"",""origin"": ""fixed"",""value"": ""5""},""partNumber"": {""action"": ""string"",""origin"": ""program"",""lengthMin"": 6,""lengthMax"": 9},""lastTwoYearDigits"": {""action"": ""lastTwoYearDigits"",""origin"": ""program""},""julianDay"": {""action"": ""julianDay"",""origin"": ""program""},""autoIncrement"": {""action"": ""numeric"",""origin"": ""program"",""length"": 4,""incremental"": true}}}";

        private static Rule Qa45Rule() => new() { RuleId = 11, RuleJson = Qa45RuleJson, IsActive = true };

        /// <summary>
        /// #186 (a): a legacy all-digit QA45 label (QA45 + PN + yy + ddd + cccc, 15-digit trailing run) carries a
        /// 4-digit counter per its generation rule. Width derivation must read the rule's autoIncrement length (4)
        /// and increment the true counter — not refuse the label because the digit-run has no non-digit boundary.
        /// </summary>
        [Fact]
        public async Task GetConsecutiveByBarCodeLabelAsync_LegacyAllDigitLabel_WithRule_DerivesRuleWidth()
        {
            // Arrange: QA45 + 900290 (PN) + 26 (yy) + 020 (ddd) + 4711 (counter).
            GivenProduct(productId: 42);
            var barCode = new BarCode { BarCodeId = new BarCodeId(52700), Label = BarCodeLabel.FromPersisted("QA45900290260204711") };
            _barCodeRepository.FirstOrDefaultAsync(Arg.Any<Specification<BarCode>>(), Arg.Any<CancellationToken>())
                .Returns(Result<BarCode?>.Success(barCode));

            // Act — the master template is all-digit too (same rule shape, counter 0001).
            var response = await _service.GetConsecutiveByBarCodeLabelAsync("900290", new List<string> { "QA45900290252970001" }, Qa45Rule(), TestContext.Current.CancellationToken);

            // Assert
            response.IsSuccess.ShouldBeTrue($"errors: [{string.Join(";", response.Errors ?? new List<string>())}]");
            response.Value.ShouldBe(4712); // rule width 4 -> counter "4711" + 1
        }

        /// <summary>
        /// #186 (b): the self-refusal loop. A label the service's OWN formatter (CreateBarCodeDictionaryExecutor +
        /// the QA45 rule) just minted must be accepted as the basis for the next consecutive — before the fix,
        /// create worked at most once per product per session because the freshly minted all-digit label was
        /// refused by the width guard on the next create.
        /// </summary>
        [Fact]
        public async Task GetConsecutiveByBarCodeLabelAsync_OwnGeneratedLabel_WithRule_IsAcceptedAsBasis()
        {
            // Arrange: mint a label exactly as production does (2026-01-20 -> yy 26, ddd 020; consecutive 7).
            var dateTimeMachine = Substitute.For<IDateTimeMachine>();
            dateTimeMachine.Now.Returns(new DateTime(2026, 1, 20, 10, 0, 0));
            var executor = new CreateBarCodeDictionaryExecutor(dateTimeMachine);
            executor.ParseRuleFromJson(Qa45RuleJson).ShouldNotBeNull();
            executor.InitializeComponentActions().IsSuccess.ShouldBeTrue();
            var minted = executor.ApplyRuleCreateBarCode("900290", 7);
            minted.IsSuccess.ShouldBeTrue($"errors: [{string.Join(";", minted.Errors ?? new List<string>())}]");
            minted.Value.ShouldBe("QA45900290260200007");

            GivenProduct(productId: 42);
            var barCode = new BarCode { BarCodeId = new BarCodeId(52701), Label = BarCodeLabel.FromPersisted(minted.Value ?? string.Empty) };
            _barCodeRepository.FirstOrDefaultAsync(Arg.Any<Specification<BarCode>>(), Arg.Any<CancellationToken>())
                .Returns(Result<BarCode?>.Success(barCode));

            // Act
            var response = await _service.GetConsecutiveByBarCodeLabelAsync("900290", new List<string> { "QA45900290252970001" }, Qa45Rule(), TestContext.Current.CancellationToken);

            // Assert — the just-minted label seeds the NEXT consecutive, closing the mint -> derive loop.
            response.IsSuccess.ShouldBeTrue($"errors: [{string.Join(";", response.Errors ?? new List<string>())}]");
            response.Value.ShouldBe(8);
        }

        /// <summary>
        /// #186: exhaustion stays fail-closed in the metadata path — an all-digit legacy label whose 4-digit
        /// counter sits at 9999 must fail loud ("exhausted"), never wrap or over-capture into a wider field.
        /// </summary>
        [Fact]
        public async Task GetConsecutiveByBarCodeLabelAsync_LegacyAllDigitLabelAtLimit_WithRule_FailsLoudExhausted()
        {
            // Arrange
            GivenProduct(productId: 42);
            var barCode = new BarCode { BarCodeId = new BarCodeId(52702), Label = BarCodeLabel.FromPersisted("QA45900290260209999") };
            _barCodeRepository.FirstOrDefaultAsync(Arg.Any<Specification<BarCode>>(), Arg.Any<CancellationToken>())
                .Returns(Result<BarCode?>.Success(barCode));

            // Act
            var response = await _service.GetConsecutiveByBarCodeLabelAsync("900290", new List<string> { "QA45900290252970001" }, Qa45Rule(), TestContext.Current.CancellationToken);

            // Assert
            response.IsFailure.ShouldBeTrue();
            response.Errors.ShouldContain(e => e.Contains("exhausted", StringComparison.Ordinal));
        }

        /// <summary>
        /// #186 (c): a boundary-shaped label (the E2E sentinel "QA45900290E2E0000") still derives correctly when
        /// the rule is supplied — the metadata width (4) and the boundary run (4) agree, so the sentinel keeps
        /// working but is no longer NECESSARY.
        /// </summary>
        [Fact]
        public async Task GetConsecutiveByBarCodeLabelAsync_SentinelBoundaryLabel_WithRule_StillDerives()
        {
            // Arrange
            GivenProduct(productId: 42);
            var barCode = new BarCode { BarCodeId = new BarCodeId(52701), Label = BarCodeLabel.FromPersisted("QA45900290E2E0000") };
            _barCodeRepository.FirstOrDefaultAsync(Arg.Any<Specification<BarCode>>(), Arg.Any<CancellationToken>())
                .Returns(Result<BarCode?>.Success(barCode));

            // Act
            var response = await _service.GetConsecutiveByBarCodeLabelAsync("900290", new List<string> { "QA45900290E2E0000" }, Qa45Rule(), TestContext.Current.CancellationToken);

            // Assert
            response.IsSuccess.ShouldBeTrue($"errors: [{string.Join(";", response.Errors ?? new List<string>())}]");
            response.Value.ShouldBe(1); // "0000" + 1
        }

        /// <summary>
        /// #186 (fallback preservation): a rule whose JSON cannot be parsed contributes no width metadata; the
        /// boundary-based derivation must keep working exactly as before for boundary-shaped corpora.
        /// </summary>
        [Fact]
        public async Task GetConsecutiveByBarCodeLabelAsync_WithMalformedRuleJson_FallsBackToBoundary()
        {
            // Arrange
            GivenProduct(productId: 42);
            var barCode = new BarCode { BarCodeId = new BarCodeId(99999), Label = BarCodeLabel.FromPersisted("TEST-0100") };
            _barCodeRepository.FirstOrDefaultAsync(Arg.Any<Specification<BarCode>>(), Arg.Any<CancellationToken>())
                .Returns(Result<BarCode?>.Success(barCode));
            var malformedRule = new Rule { RuleId = 99, RuleJson = "not a rule json", IsActive = true };

            // Act
            var response = await _service.GetConsecutiveByBarCodeLabelAsync("PART-123", new List<string> { "MASTER" }, malformedRule, TestContext.Current.CancellationToken);

            // Assert
            response.IsSuccess.ShouldBeTrue($"errors: [{string.Join(";", response.Errors ?? new List<string>())}]");
            response.Value.ShouldBe(101);
        }

        /// <summary>
        /// #186 (fallback preservation): when the label's trailing digit-run is NARROWER than the rule's counter
        /// width (e.g. an older narrower-field corpus), the metadata path cannot read a full-width counter and the
        /// boundary-based derivation takes over — preserving the pre-#186 behavior for that corpus.
        /// </summary>
        [Fact]
        public async Task GetConsecutiveByBarCodeLabelAsync_RuleWidthWiderThanTrailingRun_FallsBackToBoundary()
        {
            // Arrange: trailing run is 3 ("010"), rule width is 4.
            GivenProduct(productId: 42);
            var barCode = new BarCode { BarCodeId = new BarCodeId(7), Label = BarCodeLabel.FromPersisted("L1AL7401MINE010") };
            _barCodeRepository.FirstOrDefaultAsync(Arg.Any<Specification<BarCode>>(), Arg.Any<CancellationToken>())
                .Returns(Result<BarCode?>.Success(barCode));

            // Act — master carries no trailing digits, so the boundary cross-check is skipped.
            var response = await _service.GetConsecutiveByBarCodeLabelAsync("PART-123", new List<string> { "MASTER" }, Qa45Rule(), TestContext.Current.CancellationToken);

            // Assert
            response.IsSuccess.ShouldBeTrue($"errors: [{string.Join(";", response.Errors ?? new List<string>())}]");
            response.Value.ShouldBe(11); // boundary width 3 -> "010" + 1
        }

        /// <summary>
        /// Tests GetBarCodeByLabelAsync returns success with valid label
        /// </summary>
        [Fact]
        public async Task GetBarCodeByLabelAsync_WithValidLabel_ShouldReturnSuccess()
        {
            // Arrange
            var barCode = new BarCode { BarCodeId = new BarCodeId(1), Label = BarCodeLabel.FromPersisted("TEST-LABEL") };
            var result = Result<BarCode?>.Success(barCode);
            _barCodeRepository.FirstOrDefaultAsync(Arg.Any<Specification<BarCode>>(), Arg.Any<CancellationToken>())
                .Returns(result);

            var labelResult = BarCodeLabel.Create("TEST-LABEL");
            labelResult.IsSuccess.ShouldBeTrue();
            labelResult.Value.ShouldNotBeNull();

            // Act
            var response = await _service.GetBarCodeByLabelAsync(labelResult.Value, TestContext.Current.CancellationToken);

            // Assert
            response.IsSuccess.ShouldBeTrue();
            response.Value.ShouldNotBeNull();
            response.Value.Label.Value.ShouldBe("TEST-LABEL");
        }

        /// <summary>
        /// Tests BarCodeLabel.Create rejects an empty label at the lookup boundary,
        /// so the invalid label never reaches GetBarCodeByLabelAsync.
        /// </summary>
        [Fact]
        public void GetBarCodeByLabelAsync_WithEmptyLabel_ShouldFailAtBoundary()
        {
            // Act
            var labelResult = BarCodeLabel.Create(string.Empty);

            // Assert
            labelResult.IsFailure.ShouldBeTrue();
        }

        /// <summary>
        /// Tests GetBarCodeByIdAsync returns success with valid ID
        /// </summary>
        [Fact]
        public async Task GetBarCodeByIdAsync_WithValidId_ShouldReturnSuccess()
        {
            // Arrange
            var barCode = new BarCode { BarCodeId = new BarCodeId(123), Label = BarCodeLabel.FromPersisted("TEST-LABEL") };
            var result = Result<BarCode?>.Success(barCode);
            _barCodeRepository.FirstOrDefaultAsync(Arg.Any<Specification<BarCode>>(), Arg.Any<CancellationToken>())
                .Returns(result);

            // Act
            var response = await _service.GetBarCodeByIdAsync(123, TestContext.Current.CancellationToken);

            // Assert
            response.IsSuccess.ShouldBeTrue();
            response.Value.ShouldNotBeNull();
            response.Value.BarCodeId.Value.ShouldBe(123);
        }

        // #119 (F2): the GetBarCodeByRegisterDataAsync test was deleted together with the dead service copy it
        // exercised. The live Reports-path implementation (BarCodeRepositoryExtensions.GetBarCodeByRegisterDataAsync)
        // is covered by RegisterDataFilterTests.
    }
}