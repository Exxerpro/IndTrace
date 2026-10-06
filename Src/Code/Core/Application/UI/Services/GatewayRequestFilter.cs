// <copyright file="GatewayRequestFilter.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.UI.Services;

/// <summary>
/// Provides filtering utilities for gateway requests based on model logic.
/// </summary>
public static class GatewayRequestFilter
{
    /// <summary>
    /// Filters a collection of <see cref="TaskGatewayRequest"/> based on model logic.
    /// This logic identifies duplicates, prioritizes recent data, checks for part number consistency,
    /// and applies expiration for stale data.
    ///
    /// Filtering Strategy:
    /// A. For duplicate Names with selected PartNumber → keep most recent per Name
    /// B. For duplicate Names with different PartNumber → keep most recent per Name not already in result
    /// C. For non-duplicated Names with selected PartNumber → keep all
    /// D. Remaining entries → keep only if not expired.
    /// </summary>
    /// <typeparam name="TSource">The type of the source entity, implementing <see cref="IMonitorFilter"/>.</typeparam>
    /// <param name="source">Input dictionary of TaskGatewayRequest by ID.</param>
    /// <param name="expirationTime">Optional expiration window (default 60s).</param>
    /// <returns>Filtered dictionary based on described logic.</returns>
    public static IReadOnlyDictionary<int, TSource> FilterByModel<TSource>(
     this IDictionary<int, TSource> source,
     TimeSpan? expirationTime = null)
        where TSource : IMonitorFilter
    {
        if (source == null || source.Count <= 1)
        {
            return source == null ? new Dictionary<int, TSource>() : new Dictionary<int, TSource>(source);
        }

        var now = source.Values.Max(x => x.TimeStamp).AddSeconds(1);
        var effectiveExpiration = expirationTime ?? TimeSpan.FromMinutes(45);

        var result = new Dictionary<int, TSource>();
        var processedKeys = new HashSet<int>();


        // Step 1: Identify duplicated names
        var duplicatedGroups = source
            .GroupBy(kvp => kvp.Value.Name)
            .Where(g => g.Count() > 1)
            .ToList();

        var duplicatedNames = duplicatedGroups.Select(g => g.Key).ToHashSet();

        if (duplicatedNames.Count == 0)
        {
            return new Dictionary<int, TSource>(source);
        }

        // Step 2: From each group, select the most recent instance as the group "winner"
        var groupWinners = new List<KeyValuePair<int, TSource>>();
        foreach (var group in duplicatedGroups)
        {
            var ordered = group.OrderByDescending(kvp => kvp.Value.TimeStamp).ToList();
            groupWinners.Add(ordered[0]); // Most recent per duplicated name

            // Mark all items in the group as processed (win or lose)
            foreach (var kvp in ordered)
            {
                processedKeys.Add(kvp.Key);
            }
        }

        //// var selectedPartNumber = groupWinners
        ////     .OrderByDescending(kvp => kvp.Value.TimeStamp)
        ////     .First().Value.PartNumber;

        // Step 3: Determine the most frequent part number from group winners (not just most recent)
        var selectedPartNumber = groupWinners
            .GroupBy(kvp => kvp.Value.PartNumber)
            .OrderByDescending(g => g.Count())
            .ThenByDescending(g => g.Max(x => x.Value.TimeStamp))
            .First()
            .Key;

        // Step 4: From winners, select ones that match the canonical part number
        foreach (var kvp in groupWinners)
        {
            if (kvp.Value.PartNumber == selectedPartNumber)
            {
                result[kvp.Key] = kvp.Value; // Keep winner with matching part number
            }
            else
            {
                // Only keep if no other winner for this Name was already selected
                if (result.Values.All(v => v.Name != kvp.Value.Name))
                {
                    result[kvp.Key] = kvp.Value;
                }
            }
        }

        // Step 5: Unique Name + Matching PartNumber → Keep all //++ it must consider all the remaining entries ??
        var uniqueMatching = source
            .Where(kvp => !duplicatedNames.Contains(kvp.Value.Name) && kvp.Value.PartNumber == selectedPartNumber);

        foreach (var kvp in uniqueMatching)
        {
            result[kvp.Key] = kvp.Value;
            processedKeys.Add(kvp.Key);
        }

        // Step 6: Remaining entries → Keep only if not expired o is this what is wrong ?
        var remainder = source
            .Where(kvp => !processedKeys.Contains(kvp.Key) &&
                          (now - kvp.Value.TimeStamp) <= effectiveExpiration);

        foreach (var kvp in remainder)
        {
            result[kvp.Key] = kvp.Value;
            processedKeys.Add(kvp.Key);
        }

        return result;
    }
}