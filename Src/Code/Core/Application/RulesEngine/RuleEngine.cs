// <copyright file="RuleEngine.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.RulesEngine;

/// <summary>
/// Default implementation of the rule engine that applies business rules sequentially to target objects.
/// </summary>
/// <typeparam name="T">The type of object that rules will be applied to.</typeparam>
/// <remarks>
/// This implementation applies rules in the order they are provided in the collection.
/// Each rule is applied asynchronously, and execution continues to the next rule even if a previous rule fails.
/// For more sophisticated error handling or rule execution strategies, consider implementing a custom rule engine.
/// </remarks>
public class RuleEngine<T> : IRuleEngine<T>
{
    /// <summary>
    /// Applies a collection of business rules to the specified target object sequentially and asynchronously.
    /// </summary>
    /// <param name="target">The target object to which rules will be applied.</param>
    /// <param name="rules">The collection of rules to apply to the target object.</param>
    /// <returns>A task representing the asynchronous rule application operation.</returns>
    /// <remarks>
    /// Rules are applied in sequence using a foreach loop. Each rule's ApplyAsync method is awaited
    /// before proceeding to the next rule. If a rule throws an exception, it will propagate up
    /// and stop the execution of subsequent rules.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when target or rules is null.</exception>
    /// <summary>
    /// Executes ApplyRulesAsync operation.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="rules">The rules.</param>
    /// <returns>The result of ApplyRulesAsync.</returns>
    public async Task<Result> ApplyRulesAsync(T target, IEnumerable<IRule<T>> rules)
    {
        // [Fix]
        // CLAUDE
        // Date: 21/08/2025
        // Reason: Pattern 9 Fix - Added null safety checks to prevent null reference exceptions
        if (target == null)
        {
            return Result.WithFailure("Target cannot be null");
        }

        if (rules == null)
        {
            return Result.WithFailure($"{nameof(rules)} Rules collection cannot be null");
        }

        List<string> errorsList = [];
        foreach (var rule in rules)
        {
            if (rule == null)
            {
                errorsList.Add($"rule {nameof(rule)} cannot be null");
                continue;
            }

            try
            {
                var result = await rule.ApplyAsync(target).ConfigureAwait(false);
                if (result is not null && result.IsFailure)
                {
                    errorsList.AddRange(result.Errors);
                }

                if (result is null)
                {
                    errorsList.Add($"Error processing Rule {nameof(rule)}: result is null");
                }
            }
            catch (Exception e)
            {
                errorsList.Add($"rule {nameof(rule)} encountered an error: {e.Message}");
            }
        }

        return errorsList.Count > 0 ? Result.WithFailure(errorsList) : Result.Success();
    }
}