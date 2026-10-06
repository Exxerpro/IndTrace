// <copyright file="BarCodeService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Services;

/// <summary>
/// Service implementation for BarCode-related operations, replacing BarCodeRepositoryExtensions.
/// Provides proper dependency injection and testability.
/// </summary>
public class BarCodeService : IBarCodeService
{
    // The consecutive is embedded into the label by CreateBarCodeDictionaryExecutor.NumericAction as a
    // zero-padded field of `component.Length ?? 4` digits placed (in the §7 print contract) at the trailing
    // position, isolated from the preceding fields by a non-digit boundary (e.g. "...MINE0100"). Deriving the
    // current sequence value therefore means reading the trailing contiguous digit-run of the most-recent
    // label in scope — its LENGTH is the field width, not a hardcoded constant. A 4-digit field reads exactly
    // as before; a 5-digit field ("...MTLA12345") now reads 12345, not the old trailing-4 "2345" (which would
    // re-mint an already-used 0xxxx identity). The overflow bound is derived from that same width (10^width),
    // mirroring NumericAction's own guard, so the extractor and the formatter agree on the field size.
    //
    // RESIDUAL ASSUMPTION (fallback path only, #186): the consecutive IS the trailing contiguous digit-run.
    // That only holds when a non-digit boundary separates the counter from the preceding numeric fields; legacy
    // corpora (QA45<PN><yy><ddd><cccc>, all digits after the prefix) violate it, so the primary derivation now
    // reads the field width from the generation RULE's trailing autoIncrement length (via
    // CreateBarCodeDictionaryExecutor.ParseRule / TryGetTrailingConsecutiveWidth — the same metadata
    // NumericAction pads with). The digit-run heuristic remains as the fallback for callers without a rule and
    // for boundary-shaped corpora; an over-capturing run there still fails LOUD at the width guard rather than
    // silently minting a duplicate identity.

    // Ceiling on the field width we will honour. NumericAction pads to `component.Length` (an int) and the
    // consecutive itself is an int, so a field wider than the int decimal domain cannot be represented; we fail
    // loud instead of parsing a run that would overflow. 9 keeps every value inside int (10^9 - 1 < int.Max).
    private const int MaxConsecutiveWidth = 9;

    // First consecutive of a brand-new scoped sequence — the master-label templates end in "0001", i.e. the
    // first physical part carries consecutive 1 (NumericAction renders 1 as "0001").
    private const int FirstConsecutive = 1;

    // The repository returns Result.WithFailure with this sentinel when a FirstOrDefault matches nothing
    // (Repository.cs / ReadOnlyRepository.cs). It is the ONE failure that means "empty", not "infrastructure
    // error" — the distinction the pre-fix code collapsed into a single "No BarCodes found" message.
    private const string RepositoryNotFoundSentinel = RepositoryFailures.NotFoundSentinel;

    private readonly IRepository<BarCode> barCodeRepository;
    private readonly IReadOnlyRepository<Product> productRepository;

    public BarCodeService(
        IRepository<BarCode> barCodeRepository,
        IReadOnlyRepository<Product> productRepository)
    {
        this.barCodeRepository = barCodeRepository ?? throw new ArgumentNullException(nameof(barCodeRepository));
        this.productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
    }

    /// <inheritdoc/>
    public async Task<Result<int>> GetConsecutiveByBarCodeLabelAsync(
        string partNumber,
        List<string> masterLabel,
        Rule? rule,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<int>.WithFailure("The consecutive lookup was cancelled.");
        }

        if (string.IsNullOrWhiteSpace(partNumber))
        {
            return Result<int>.WithFailure("A part number is required to scope the consecutive.");
        }

        if (masterLabel is null)
        {
            return Result<int>.WithFailure("A master-label list is required to scope the consecutive.");
        }

        // Honour the partNumber parameter: resolve it to the product so the sequence is scoped to THIS product's
        // barcodes instead of the (buggy) global max across every product/line. ProductId equality is the one
        // predicate that translates server-side — the Label value object is mapped through an EF value converter,
        // so a label prefix/LIKE scope would not translate (see BarCodeConfiguration).
        var productSpec = new Specification<Product>(p => p.PartNumber == partNumber);
        var productResult = await this.productRepository.FirstOrDefaultAsync(productSpec, cancellationToken).ConfigureAwait(false);

        if (productResult.IsFailure)
        {
            // The product genuinely does not exist (not-found sentinel) => cannot scope; any OTHER failure is an
            // infrastructure error surfaced as-is (never collapsed into a false "empty sequence" success).
            return IsNotFound(productResult.Errors)
                ? Result<int>.WithFailure($"No product found for part number '{partNumber}'; cannot scope the consecutive.")
                : Result<int>.WithFailure(productResult.Errors);
        }

        if (productResult.Value is null)
        {
            return Result<int>.WithFailure($"No product found for part number '{partNumber}'; cannot scope the consecutive.");
        }

        var productId = productResult.Value.ProductId;

        var spec = new Specification<BarCode>(b => b.ProductId == productId);
        spec.AddOrderByDescending(b => b.BarCodeId);

        var lastLabelResult = await this.barCodeRepository.FirstOrDefaultAsync(spec, cancellationToken).ConfigureAwait(false);

        if (lastLabelResult.IsFailure)
        {
            // Distinguish a legitimately EMPTY scope (not-found sentinel) from a real infrastructure failure.
            // The repository signals "no rows" as a failure carrying RepositoryNotFoundSentinel; that is the
            // seed case, not an error. Any other failure is a genuine DB error and is propagated.
            return IsNotFound(lastLabelResult.Errors)
                ? Result<int>.Success(FirstConsecutive)
                : Result<int>.WithFailure(lastLabelResult.Errors);
        }

        if (lastLabelResult.Value is null)
        {
            // No barcode exists for this product yet — start the scoped sequence at the first consecutive.
            return Result<int>.Success(FirstConsecutive);
        }

        var lastLabel = lastLabelResult.Value.Label.Value;

        // #186 — label-shape-aware width derivation. The trailing digit-run heuristic below assumes a NON-DIGIT
        // boundary precedes the counter; legacy corpora (e.g. QA45<PN><yy><ddd><cccc>, all digits after the
        // prefix) violate that, so the run over-captured (15-17 digits) and the guard refused every legacy label
        // — including labels this very service had just minted, making create work at most once per product per
        // session. When the caller supplies the generation rule, the trailing autoIncrement component's
        // configured length (defaulted exactly like NumericAction) is the AUTHORITATIVE field width — generation
        // and derivation then share one source of truth.
        var trailingRunWidth = TrailingDigitRunLength(lastLabel);
        var ruleWidth = DeriveConsecutiveWidthFromRule(rule);

        if (ruleWidth is { } metadataWidth
            && metadataWidth >= 1
            && metadataWidth <= MaxConsecutiveWidth
            && trailingRunWidth >= metadataWidth)
        {
            // The counter is the last `metadataWidth` digits by the rule's own construction. The master-template
            // cross-check below is a BOUNDARY-heuristic disambiguator and does not apply here: all-digit master
            // labels carry the same unbounded digit-runs that made the heuristic refuse the corpus.
            return DeriveNextConsecutive(lastLabel, metadataWidth, partNumber);
        }

        // Fallback (pre-#186 behavior, preserved): boundary-based derivation for corpora whose labels DO isolate
        // the counter behind a non-digit boundary, and for callers without a rule in hand. The consecutive is the
        // trailing contiguous digit-run of the highest label; its LENGTH is the field width (consistent with
        // NumericAction, which zero-pads the consecutive to `component.Length` at the trailing position). A label
        // that does not end in a digit carries no sequence field to increment.
        var consecutiveWidth = trailingRunWidth;

        if (consecutiveWidth == 0)
        {
            return Result<int>.WithFailure(
                $"Label '{lastLabel}' does not end in a numeric consecutive; refusing to guess the sequence.");
        }

        if (consecutiveWidth > MaxConsecutiveWidth)
        {
            // A trailing digit-run wider than the int decimal domain cannot be an int consecutive — fail loud
            // rather than parse a value that would overflow.
            return Result<int>.WithFailure(
                $"Label '{lastLabel}' ends in a {consecutiveWidth}-digit run, wider than the supported {MaxConsecutiveWidth}-digit consecutive field; refusing to parse an out-of-range identity.");
        }

        // Cross-check the derived width against the caller's master template(s). A master label ends in the SAME
        // consecutive field (NumericAction renders consecutive 1 as a zero-padded field of the rule's configured
        // Length), so its trailing digit-run width IS the formatter's field width. If a master template's width
        // disagrees with the highest label's, the field width is AMBIGUOUS (e.g. the rule widened the field
        // mid-sequence) — fail loud rather than guess which width to increment. Master templates that carry no
        // trailing digits (e.g. a bare "MASTER") cannot cross-check and are skipped.
        foreach (var master in masterLabel)
        {
            if (string.IsNullOrEmpty(master))
            {
                continue;
            }

            var masterWidth = TrailingDigitRunLength(master);
            if (masterWidth > 0 && masterWidth != consecutiveWidth)
            {
                return Result<int>.WithFailure(
                    $"Ambiguous consecutive width for part '{partNumber}': highest label '{lastLabel}' ends in a {consecutiveWidth}-digit run but master template '{master}' ends in a {masterWidth}-digit run; refusing to guess which field width to increment.");
            }
        }

        return DeriveNextConsecutive(lastLabel, consecutiveWidth, partNumber);
    }

    /// <summary>
    /// Parses the trailing <paramref name="width"/>-digit consecutive field of <paramref name="lastLabel"/> and
    /// returns it incremented by one — failing loud on an un-parseable field or on exhaustion of the fixed-width
    /// sequence (never wrapping into a duplicate identity). Shared by the rule-metadata path and the
    /// boundary-heuristic fallback so both agree on parse, increment, and exhaustion semantics.
    /// </summary>
    private static Result<int> DeriveNextConsecutive(string lastLabel, int width, string partNumber)
    {
        var suffix = lastLabel[^width..];
        if (!int.TryParse(suffix, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var currentConsecutive))
        {
            // Guarded by width <= MaxConsecutiveWidth (all-digit, in int range), so this is only
            // reachable for a genuinely un-parseable run — fail loud rather than mint a guessed identity.
            return Result<int>.WithFailure(
                $"Label '{lastLabel}' does not end in a parseable {width}-digit consecutive; refusing to guess the sequence.");
        }

        var nextConsecutive = currentConsecutive + 1;

        // Largest value the width-digit field can hold without wrapping (10^width - 1), mirroring NumericAction's
        // own range guard so the extractor and formatter agree on the field size.
        var maxConsecutive = PowerOfTen(width) - 1;

        if (nextConsecutive > maxConsecutive)
        {
            // Fail LOUD on exhaustion. The previous `% 10000` silently wrapped 9999 -> 0, which — against the
            // UNIQUE label index — either collides at insert or (without it) mints a duplicate identity. Neither
            // is acceptable in a traceability system; the caller must widen the field or start a new sequence.
            return Result<int>.WithFailure(
                $"Consecutive sequence exhausted for part '{partNumber}': reached {currentConsecutive}, and {nextConsecutive} cannot fit the {width}-digit label field without wrapping into a duplicate identity.");
        }

        return Result<int>.Success(nextConsecutive);
    }

    /// <summary>
    /// Derives the consecutive-field width from the label-generation rule's own metadata (#186): the trailing
    /// autoIncrement component's configured length, read through the SAME parser the formatter uses
    /// (<see cref="CreateBarCodeDictionaryExecutor.ParseRule"/>), so generation and derivation cannot drift.
    /// Returns <see langword="null"/> when the rule is absent, un-parseable, or mints no trailing counter —
    /// the caller then falls back to the boundary-based heuristic (fail-closed stays fail-closed).
    /// </summary>
    private static int? DeriveConsecutiveWidthFromRule(Rule? rule)
    {
        if (rule is null || string.IsNullOrWhiteSpace(rule.RuleJson))
        {
            return null;
        }

        var parsed = CreateBarCodeDictionaryExecutor.ParseRule(rule.RuleJson);
        return CreateBarCodeDictionaryExecutor.TryGetTrailingConsecutiveWidth(parsed);
    }

    /// <summary>
    /// Returns <see langword="true"/> when a repository failure is the "no rows matched" sentinel (an empty
    /// result), as opposed to a genuine infrastructure error.
    /// </summary>
    private static bool IsNotFound(IEnumerable<string>? errors) =>
        errors is not null && errors.Any(e => e is not null && e.Contains(RepositoryNotFoundSentinel, StringComparison.Ordinal));

    /// <summary>
    /// Returns the number of trailing contiguous ASCII digits in <paramref name="value"/> — the width of the
    /// zero-padded consecutive field that <c>NumericAction</c> renders at the tail of a §7 label. Zero means the
    /// value does not end in a digit and therefore carries no consecutive to increment.
    /// </summary>
    private static int TrailingDigitRunLength(string value)
    {
        var count = 0;
        for (var i = value.Length - 1; i >= 0 && char.IsAsciiDigit(value[i]); i--)
        {
            count++;
        }

        return count;
    }

    /// <summary>
    /// Computes 10^<paramref name="exponent"/> using integer arithmetic (no floating point), for
    /// <paramref name="exponent"/> in 0..<see cref="MaxConsecutiveWidth"/> where the result stays inside
    /// <see cref="int"/>.
    /// </summary>
    private static int PowerOfTen(int exponent)
    {
        var result = 1;
        for (var i = 0; i < exponent; i++)
        {
            result *= 10;
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<Result<BarCode>> GetBarCodeByLabelAsync(
        BarCodeLabel label,
        CancellationToken cancellationToken)
    {
        var spec = new Specification<BarCode>(b => b.Label.Equals(label));
        var barCode = await this.barCodeRepository.FirstOrDefaultAsync(spec, cancellationToken);
        if (barCode.IsFailure)
        {
            return Result<BarCode>.WithFailure(barCode.Errors);
        }

        if (barCode.Value is null)
        {
            return Result<BarCode>.WithFailure("BarCode not found");
        }

        return Result<BarCode>.Success(barCode.Value);
    }

    /// <inheritdoc/>
    public async Task<Result<BarCode>> GetBarCodeByIdAsync(
        int barCodeId,
        CancellationToken cancellationToken)
    {
        var spec = new Specification<BarCode>(b => b.BarCodeId == new BarCodeId(barCodeId));
        var barCode = await this.barCodeRepository.FirstOrDefaultAsync(spec, cancellationToken);
        if (barCode.IsFailure)
        {
            return Result<BarCode>.WithFailure(barCode.Errors);
        }

        if (barCode.Value is null)
        {
            return Result<BarCode>.WithFailure("BarCode not found");
        }

        return Result<BarCode>.Success(barCode.Value);
    }

    // #119 (F2): the GetBarCodeByRegisterDataAsync copy that lived here (and its IReadOnlyRepository<Register> /
    // IRepository<Cycle> constructor dependencies) was a dead duplicate of the live Reports-path implementation in
    // BarCodeRepositoryExtensions and carried the same unbounded-ledger-scan and failure-collapsing defects. It had
    // no production caller, so it was deleted rather than fixed twice.
}