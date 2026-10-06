// <copyright file="CreateBarCodeExecutor.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Rules;

/*
     *
     *
     *

    ///Rule definition

     *
     */

/// <summary>
/// Executes barcode creation rules using a provided date/time provider.
/// </summary>
/// <summary>
/// Executes barcode creation rules using a date/time provider.
/// </summary>
public class CreateBarCodeExecutor : IRule<BarCode>
{
    private readonly IDateTimeMachine dateTimeMachineProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateBarCodeExecutor"/> class.
    /// </summary>
    /// <param name="dateTimeMachineProvider">The date/time machine provider.</param>
    public CreateBarCodeExecutor(IDateTimeMachine dateTimeMachineProvider)
    {
        this.dateTimeMachineProvider = dateTimeMachineProvider;
    }

    /// <summary>
    /// Applies the rule to the specified barcode target asynchronously.
    /// </summary>
    /// <param name="target">The barcode entity to apply the rule to.</param>
    /// <returns>A Result indicating success or failure of the operation.</returns>
    public Task<Result> ApplyAsync(BarCode target)
    {
        return Task.FromResult(Result.WithFailure("ApplyAsync method not yet implemented"));
    }

    /// <summary>
    /// Applies a rule to create a barcode string based on the rule definition, part number, and consecutive number.
    /// </summary>
    /// <param name="rule">The rule to be applied.</param>
    /// <param name="partNumber">The part number to be included in the barcode.</param>
    /// <param name="consecutive">The auto-incrementing number to be included in the barcode.</param>
    /// <returns>A Result containing the constructed barcode string or validation errors.</returns>
    public Result<string> ApplyRuleCreateBarCode(Rule rule, string partNumber, int consecutive)
    {
        if (this.dateTimeMachineProvider is null)
        {
            return Result<string>.WithFailure("DateTimeMachine provider is null");
        }

        // Check if the rule is active
        if (rule is not { IsActive: true })
        {
            return Result<string>.WithFailure("Rule is not active");
        }

        // [Fix]
        // CLAUDE
        // Date: 23/08/2025
        // Reason: Handle null RuleJson before parsing to avoid ArgumentNullException
        if (string.IsNullOrEmpty(rule.RuleJson))
        {
            return Result<string>.WithFailure("Rule JSON is null or empty");
        }

        // Parse the rule JSON to get the rule's function and components
        JsonNode? ruleJson;
        try
        {
            ruleJson = JsonNode.Parse(rule.RuleJson);
        }
        catch (JsonException ex)
        {
            return Result<string>.WithFailure($"Invalid JSON in rule: {ex.Message}");
        }

        if (ruleJson is null)
        {
            return Result<string>.WithFailure("Rule JSON is null");
        }

        // Parse ruleFunction
        var ruleFunction = ruleJson["ruleFunction"]?.AsArray()
            .Select(x => x?.GetValue<string>() ?? string.Empty)
            .Where(x => !string.IsNullOrEmpty(x))
            .ToList();
        if (ruleFunction == null)
        {
            return Result<string>.WithFailure("Rule function not found in JSON");
        }

        // Parse components
        var componentsNode = ruleJson["components"]?.AsObject();
        if (componentsNode == null)
        {
            return Result<string>.WithFailure("Components not found in rule JSON");
        }

        // #126 F2.1 — one time snapshot per label build: reading Year and DayOfYear as separate
        // provider calls can tear across a midnight/new-year rollover and stamp the label with the
        // old year but the new day-of-year.
        var labelInstant = this.dateTimeMachineProvider.Now;

        // The StringBuilder is used for efficiency while building the barcode string
        var barcode = new StringBuilder();

        // Iterate over each component in the rule function
        foreach (var component in ruleFunction)
        {
            if (!componentsNode.ContainsKey(component))
            {
                continue;
            }

            var comp = componentsNode[component]?.AsObject();
            if (comp == null)
            {
                continue;
            }

            var origin = comp["origin"]?.GetValue<string>();
            var action = comp["action"]?.GetValue<string>();

            // Some rules have this on the rule
            const int lenMinDefault = 4;
            const int lenMaxDefault = 10;

            switch (action)
            {
                case "default":
                    if (origin == "fixed")
                    {
                        barcode.Append(comp["value"]?.GetValue<string>());
                    }

                    break;

                case "string":
                    if (origin == "fixed")
                    {
                        barcode.Append(comp["value"]?.GetValue<string>());
                    }
                    else if (origin == "program")
                    {
                        var lengthMin = comp["lengthMin"]?.GetValue<int?>() ?? lenMinDefault;

                        var lengthMax = comp["lengthMax"]?.GetValue<int?>() ?? lenMaxDefault;

                        // [Fix]
                        // CLAUDE
                        // Date: 20/08/2025
                        // Reason: Fix padding and truncation logic - remove incorrect -1 operations and add bounds checking
                        var processedPartNumber = partNumber.Length switch
                        {
                            < lenMinDefault => partNumber.PadLeft(lengthMin, '0'),
                            >= lenMinDefault and <= lenMaxDefault => partNumber,
                            _ => partNumber.Length > lengthMax ? partNumber.Substring(0, lengthMax) : partNumber,
                        };

                        barcode.Append(processedPartNumber);
                    }

                    break;

                case "lastTwoYearDigits":
                    if (origin == "program")
                    {
                        barcode.Append(labelInstant.Year.ToString().Substring(2, 2));
                    }

                    break;

                case "julianDay":
                    if (origin == "program")
                    {
                        var julianDay = labelInstant.DayOfYear.ToString("D3");
                        barcode.Append(julianDay);
                    }

                    break;

                case "numeric":
                    if (origin == "fixed")
                    {
                        barcode.Append(comp["value"]?.GetValue<string>());
                    }
                    else if (origin == "program")
                    {
                        var length = comp["length"]?.GetValue<int?>() ?? 5;
                        var paddedNumber = (consecutive % Math.Pow(10, length)).ToString();
                        paddedNumber = paddedNumber.PadLeft(length, '0');
                        barcode.Append(paddedNumber);
                    }

                    break;
            }
        }

        // Return the built barcode string
        var result = barcode.ToString();
        return string.IsNullOrEmpty(result)
            ? Result<string>.WithFailure("Failed to generate barcode string")
            : Result<string>.Success(result);
    }
}

