// <copyright file="CreateBarCodeDictionaryExecutor.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Rules
{
    using Microsoft.Extensions.Logging.Abstractions;

    /// <summary>
    /// Executes barcode creation rules using a dictionary-based approach and a date/time provider.
    /// </summary>
    /// <summary>
    /// Executes barcode creation rules using a dictionary-based approach and a date/time provider.
    /// </summary>
    public class CreateBarCodeDictionaryExecutor
    {
        private readonly IDateTimeMachine dateTimeMachineProvider;
        private readonly ILogger<CreateBarCodeDictionaryExecutor> logger;

        // #126 F2.1 — the single time snapshot for the label currently being built. Set once per
        // ApplyRuleCreateBarCode so year/julian-day components can never tear across a rollover.
        // Falls back to a fresh read when a component action is invoked outside a label build.
        private DateTime? labelInstant;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateBarCodeDictionaryExecutor"/> class.
        /// </summary>
        /// <param name="dateTimeMachineProvider">The date/time machine provider.</param>
        /// <param name="logger">Optional logger; a <see cref="NullLogger{T}"/> is used when omitted so the many
        /// direct-construction call sites (this helper is newed up, not DI-resolved) stay valid.</param>
        public CreateBarCodeDictionaryExecutor(
            IDateTimeMachine dateTimeMachineProvider,
            ILogger<CreateBarCodeDictionaryExecutor>? logger = null)
        {
            this.dateTimeMachineProvider = dateTimeMachineProvider;
            this.logger = logger ?? NullLogger<CreateBarCodeDictionaryExecutor>.Instance;
        }

        /// <summary>
        /// Dictionary to store actions for different component types.
        /// </summary>
        public readonly Dictionary<string, Func<RuleFragment, string, int, IndQuestResults.Result<string?>>> ComponentActions = [];

        /// <summary>
        /// Gets or sets the rule to be used for barcode creation.
        /// </summary>
        public Rule? Rule { get; set; }

        /// <summary>
        /// The counter-field width <see cref="NumericAction"/> zero-pads the consecutive to when the rule's
        /// autoIncrement component carries no explicit <c>length</c>. Shared with
        /// <see cref="TryGetTrailingConsecutiveWidth"/> so the formatter and the width derivation (#186) can
        /// never disagree on the default.
        /// </summary>
        private const int DefaultAutoIncrementLength = 4;

        /// <summary>
        /// Parses a rule from its JSON representation and stores it as the executor's active <see cref="Rule"/>.
        /// </summary>
        /// <param name="ruleJson">The JSON string representing the rule.</param>
        /// <returns>The parsed <see cref="Rule"/> object, or null if parsing fails.</returns>
        public Rule? ParseRuleFromJson(string ruleJson)
        {
            var parsed = ParseRule(ruleJson, this.logger);
            if (parsed is not null)
            {
                // Preserved pre-#186 semantics: a failed parse leaves any previously parsed rule untouched.
                this.Rule = parsed;
            }

            return parsed;
        }

        /// <summary>
        /// Parses a rule from its JSON representation without requiring an executor instance — the single parse
        /// shared by <see cref="ParseRuleFromJson"/> (label generation) and by consumers that only need the
        /// rule's structural metadata, e.g. the consecutive-width derivation in <c>BarCodeService</c> (#186).
        /// </summary>
        /// <param name="ruleJson">The JSON string representing the rule.</param>
        /// <param name="logger">Optional logger for malformed-rule diagnostics; silent when omitted.</param>
        /// <returns>The parsed <see cref="Rule"/> object, or null if parsing fails.</returns>
        public static Rule? ParseRule(string ruleJson, ILogger? logger = null)
        {
            try
            {
                // [Fix]
                // CLAUDE
                // Date: 20/08/2025
                // Reason: Add validation for empty/whitespace JSON to return null for invalid input
                if (string.IsNullOrWhiteSpace(ruleJson))
                {
                    return null;
                }

                var ruleNode = JsonNode.Parse(ruleJson);
                if (ruleNode is null)
                {
                    return null;
                }

                // Parse Rule ID (supports numeric or string). String non-numeric IDs are stored in Rule.Name
                int parsedRuleId = 0;
                string? parsedRuleName = null;
                var ruleIdNode = ruleNode["ruleId"];
                if (ruleIdNode is System.Text.Json.Nodes.JsonValue ruleIdValue)
                {
                    if (ruleIdValue.TryGetValue<int>(out var idInt))
                    {
                        parsedRuleId = idInt;
                    }
                    else if (ruleIdValue.TryGetValue<string>(out var idStr))
                    {
                        if (int.TryParse(idStr, out var idParsed))
                        {
                            parsedRuleId = idParsed;
                        }
                        else
                        {
                            parsedRuleName = idStr;
                        }
                    }
                }

                // Parse RuleFunction
                var ruleFunction = ruleNode["ruleFunction"]?.AsArray()
                    .Select(x => x?.GetValue<string>() ?? string.Empty)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList() ?? [];

                // [Fix]
                // CLAUDE
                // Date: 20/08/2025
                // Reason: Return null if required ruleFunction is missing or empty (invalid JSON structure)
                if (!ruleFunction.Any())
                {
                    return null;
                }

                // Parse Components
                var componentsNode = ruleNode["components"]?.AsObject();
                var components = new List<RuleFragment>();
                if (componentsNode != null)
                {
                    foreach (var kvp in componentsNode)
                    {
                        var comp = kvp.Value?.AsObject();
                        if (comp == null)
                        {
                            continue;
                        }

                        components.Add(new RuleFragment
                        {
                            Name = kvp.Key,
                            Action = comp["action"]?.GetValue<string>() ?? string.Empty,
                            Origin = comp["origin"]?.GetValue<string>() ?? string.Empty,
                            Value = comp["value"]?.GetValue<string>() ?? string.Empty,
                            LengthMin = comp["lengthMin"]?.GetValue<int?>(),
                            LengthMax = comp["lengthMax"]?.GetValue<int?>(),
                            Length = comp["length"]?.GetValue<int?>(),
                            Incremental = comp["incremental"]?.GetValue<bool?>() ?? false,
                        });
                    }
                }

                return new Rule
                {
                    RuleId = parsedRuleId,
                    Name = parsedRuleName ?? string.Empty,
                    RuleFunction = ruleFunction,
                    Components = components,
                    IsActive = true,
                };
            }
            catch (Exception ex) when (ex is JsonException or InvalidOperationException or FormatException or OverflowException or ArgumentException)
            {
                // #128 (Chunk B): this is the HOT mint path. Malformed rule JSON must be tolerated — a bad node
                // shape (wrong JSON kind for GetValue<T>/AsArray/AsObject) throws InvalidOperationException /
                // FormatException / OverflowException, not just JsonException, and any of those escaping here
                // would crash minting. Broaden the catch to every realistic parse fault, log at Warning through
                // the supplied logger (was Console.WriteLine), and return null so the malformed rule is
                // skipped-and-logged exactly as before — never a hard crash.
                logger?.LogWarning(ex, "Error parsing rule JSON; skipping malformed rule.");
                return null;
            }
        }

        /// <summary>
        /// Returns the configured width of the TRAILING auto-increment (consecutive) field of a parsed rule —
        /// the same <c>length</c> (defaulted exactly like <see cref="NumericAction"/>) the formatter zero-pads
        /// the consecutive to at mint time — or <see langword="null"/> when the rule's label does not end in a
        /// program-driven auto-increment component and therefore carries no trailing counter to derive (#186).
        /// </summary>
        /// <param name="rule">The parsed rule (e.g. from <see cref="ParseRule"/>); may be <see langword="null"/>.</param>
        /// <returns>The trailing consecutive-field width, or <see langword="null"/> when unavailable.</returns>
        public static int? TryGetTrailingConsecutiveWidth(Rule? rule)
        {
            if (rule is null || rule.RuleFunction.Count == 0)
            {
                return null;
            }

            // The consecutive is a TRAILING field by the §7 label contract; a rule whose last component is not
            // the program-driven autoIncrement mints no trailing counter, so there is no width to report.
            var trailingComponent = rule.RuleFunction[^1];
            if (!string.Equals(trailingComponent, "autoIncrement", StringComparison.Ordinal))
            {
                return null;
            }

            var fragment = rule.Components.FirstOrDefault(c => c.Name == trailingComponent);
            if (fragment is null || !string.Equals(fragment.Origin, "program", StringComparison.Ordinal))
            {
                return null;
            }

            return fragment.Length ?? DefaultAutoIncrementLength;
        }

        /// <summary>
        /// Initializes the component actions dictionary based on the rule's components.
        /// </summary>
        /// <summary>
        /// Initializes the component actions dictionary based on the rule's components using functional Result semantics.
        /// </summary>
        /// <returns>Result indicating success or failure with an explanatory error.</returns>
        public IndQuestResults.Result InitializeComponentActions()
        {
            if (this.Rule == null)
            {
                return IndQuestResults.Result.WithFailure("Rule is not set");
            }

            this.ComponentActions.Clear();

            foreach (var componentName in this.Rule.RuleFunction)
            {
                var ruleFragment = this.Rule.Components.FirstOrDefault(c => c.Name == componentName);
                if (ruleFragment != null)
                {
                    Func<RuleFragment, string, int, IndQuestResults.Result<string?>>? action = componentName switch
                    {
                        "lineIdentifier" or "lineNumber" or "partNumber" or "fixedPart" => this.StringAction,
                        "julianDay" => this.JulianDayAction,
                        "lastTwoYearDigits" => this.LastTwoYearDigitsAction,
                        "autoIncrement" => this.NumericAction,
                        _ => null,
                    };
                    if (action is null)
                    {
                        return IndQuestResults.Result.WithFailure($"Unknown action for component: {componentName}");
                    }

                    this.ComponentActions[componentName] = action;
                }
            }

            return IndQuestResults.Result.Success();
        }

        /// <summary>
        /// Applies the rule to create a barcode string based on the part number and consecutive number.
        /// </summary>
        /// <param name="partNumber">The part number to be included in the barcode.</param>
        /// <param name="consecutive">The auto-incrementing number to be included in the barcode.</param>
        /// <returns>The constructed barcode string, or an error message if the rule is invalid.</returns>
        /// <summary>
        /// Applies the rule to create a barcode string based on the part number and consecutive number using functional Result semantics.
        /// </summary>
        /// <param name="partNumber">The part number to be included in the barcode.</param>
        /// <param name="consecutive">The auto-incrementing number to be included in the barcode.</param>
        /// <returns>Result with constructed barcode string, or failure with error message.</returns>
        public IndQuestResults.Result<string> ApplyRuleCreateBarCode(string partNumber, int consecutive)
        {
            if (this.dateTimeMachineProvider == null)
            {
                return IndQuestResults.Result<string>.WithFailure("DateTime provider not available");
            }

            if (this.Rule is not { IsActive: true })
            {
                return IndQuestResults.Result<string>.WithFailure("invalid rule for label");
            }

            // #126 F2.1 — one snapshot per label build (see field doc).
            this.labelInstant = this.dateTimeMachineProvider.Now;

            var barcode = new StringBuilder();

            // Iterate through the rule functions in order
            foreach (var componentName in this.Rule.RuleFunction)
            {
                if (this.ComponentActions.TryGetValue(componentName, out var action))
                {
                    var ruleFragment = this.Rule.Components.FirstOrDefault(c => c.Name == componentName);
                    if (ruleFragment != null)
                    {
                        var result = action(ruleFragment, partNumber, consecutive);
                        if (result.IsFailure)
                        {
                            return IndQuestResults.Result<string>.WithFailure(result.Errors);
                        }

                        if (result.Value != null)
                        {
                            barcode.Append(result.Value);
                        }
                    }
                }
                else
                {
                    return IndQuestResults.Result<string>.WithFailure($"No action configured for component: {componentName}");
                }
            }

            return IndQuestResults.Result<string>.Success(barcode.ToString());
        }

        /// <summary>
        /// Processes string-based components for barcode creation.
        /// </summary>
        /// <param name="component">The rule fragment representing the component.</param>
        /// <param name="partNumber">The part number to be included in the barcode.</param>
        /// <param name="consecutive">The auto-incrementing number to be included in the barcode.</param>
        /// <returns>The processed string value for the component.</returns>
        private IndQuestResults.Result<string?> StringAction(RuleFragment component, string partNumber, int consecutive)
        {
            if (component.Origin == "fixed")
            {
                return IndQuestResults.Result<string?>.Success(component.Value);
            }

            if (component.Origin == "program")
            {
                var lengthMin = component.LengthMin ?? 6;
                var lengthMax = component.LengthMax ?? 9;

                var result = partNumber.Length switch
                {
                    var length when length < lengthMin => partNumber.PadLeft(lengthMin, '0'),
                    var length when length >= lengthMin && length <= lengthMax => partNumber,
                    var length when length > lengthMax => partNumber.Substring(0, lengthMax),
                    _ => null,
                };
                return result is null
                    ? IndQuestResults.Result<string?>.WithFailure("Invalid partNumber length.")
                    : IndQuestResults.Result<string?>.Success(result);
            }

            return IndQuestResults.Result<string?>.Success(null);
        }

        /// <summary>
        /// Processes components representing the last two digits of the year.
        /// </summary>
        /// <param name="component">The rule fragment representing the component.</param>
        /// <param name="partNumber">The part number to be included in the barcode.</param>
        /// <param name="consecutive">The auto-incrementing number to be included in the barcode.</param>
        /// <returns>The last two digits of the year as a string.</returns>
        private IndQuestResults.Result<string?> LastTwoYearDigitsAction(RuleFragment component, string partNumber, int consecutive)
        {
            if (component.Origin == "program")
            {
                var instant = this.labelInstant ?? this.dateTimeMachineProvider.Now;
                return IndQuestResults.Result<string?>.Success(instant.Year.ToString().Substring(2, 2));
            }

            return IndQuestResults.Result<string?>.Success(null);
        }

        /// <summary>
        /// Processes components representing the Julian day of the year.
        /// </summary>
        /// <param name="component">The rule fragment representing the component.</param>
        /// <param name="partNumber">The part number to be included in the barcode.</param>
        /// <param name="consecutive">The auto-incrementing number to be included in the barcode.</param>
        /// <returns>The Julian day of the year as a string.</returns>
        private IndQuestResults.Result<string?> JulianDayAction(RuleFragment component, string partNumber, int consecutive)
        {
            if (component.Origin == "program")
            {
                var instant = this.labelInstant ?? this.dateTimeMachineProvider.Now;
                return IndQuestResults.Result<string?>.Success(instant.DayOfYear.ToString("D3"));
            }

            return IndQuestResults.Result<string?>.Success(null);
        }

        /// <summary>
        /// Processes numeric components for barcode creation.
        /// </summary>
        /// <param name="component">The rule fragment representing the component.</param>
        /// <param name="partNumber">The part number to be included in the barcode.</param>
        /// <param name="consecutive">The auto-incrementing number to be included in the barcode.</param>
        /// <returns>The processed numeric value for the component.</returns>
        private IndQuestResults.Result<string?> NumericAction(RuleFragment component, string partNumber, int consecutive)
        {
            if (component.Origin == "fixed")
            {
                return IndQuestResults.Result<string?>.Success(component.Value);
            }

            if (component.Origin == "program")
            {
                var length = component.Length ?? DefaultAutoIncrementLength;

                // Range guard (issue #74/#75): the old `consecutive % 10^length` silently wrapped an
                // out-of-range value into a SHORTER field — e.g. 123456 -> "23456", -1 -> "00-1" — minting a
                // duplicate or malformed barcode identity. Fail LOUD instead: a consecutive that does not fit the
                // configured field width is a caller/exhaustion error, not something to paper over.
                var upperExclusive = Math.Pow(10, length);
                if (consecutive < 0 || consecutive >= upperExclusive)
                {
                    return IndQuestResults.Result<string?>.WithFailure(
                        $"Consecutive {consecutive} does not fit the {length}-digit label field (valid range 0..{(long)upperExclusive - 1}); refusing to wrap into a duplicate or malformed label.");
                }

                var paddedNumber = consecutive.ToString(System.Globalization.CultureInfo.InvariantCulture).PadLeft(length, '0');
                return IndQuestResults.Result<string?>.Success(paddedNumber);
            }

            return IndQuestResults.Result<string?>.Success(null);
        }
    }
}