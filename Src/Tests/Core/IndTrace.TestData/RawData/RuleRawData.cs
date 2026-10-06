// <copyright file="RuleRawData.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using IndTrace.Domain.Entities;
using IndTrace.Domain.ValueObjects;

namespace IndTrace.TestData.RawData;

/// <summary>
/// Static test data for Rule entities with O(1) lookup.
/// Generated with ImmutableDictionary for thread-safety and performance.
/// Implements lazy-loaded Dict for best of both worlds: O(1) lookups + List compatibility.
/// Rules define label generation patterns for traceability.
/// </summary>
internal static class RuleRawData
{
    /// <summary>
    /// Rule test data for label generation configuration
    /// </summary>
    private static readonly ImmutableDictionary<int, Rule> _rulesDict =
        new Dictionary<int, Rule>
        {
            [1] = new Rule
            {
                RuleId = 1,
                RuleJson = @"{""ruleId"": ""R001"",""ruleFunction"": [""lineIdentifier"", ""fixedPart"", ""partNumber"", ""lastTwoYearDigits"", ""julianDay"", ""autoIncrement""],""components"": {""lineIdentifier"": {""action"": ""string"",""origin"": ""fixed"",""value"": ""WS""},""fixedPart"": {""action"": ""string"",""origin"": ""fixed"",""value"": ""100""},""partNumber"": {""action"": ""string"",""origin"": ""program"",""lengthMin"": 6,""lengthMax"": 9},""lastTwoYearDigits"": {""action"": ""lastTwoYearDigits"",""origin"": ""program""},""julianDay"": {""action"": ""julianDay"",""origin"": ""program""},""autoIncrement"": {""action"": ""numeric"",""origin"": ""program"",""length"": 4,""incremental"": true}}}",
                Name = "WS100_DEFAULT",
                MachineId = new MachineId(100),
                ProductId = new ProductId(1),
                Description = "Workstation 100 rule for DEFAULT part number baseline testing",
                Version = 1,
                IsActive = true,
                CreatedBy = "Admin",
                CreatedOn = new DateTime(2023, 8, 28, 17, 2, 28),
                ModifiedBy = "Admin",
                ModifiedOn = new DateTime(2023, 8, 25, 12, 9, 37)
            },
            [11] = new Rule
            {
                RuleId = 11,
                RuleJson = @"{""ruleId"": ""V3"",""ruleFunction"": [""lineIdentifier"", ""lineNumber"", ""fixedPart"", ""partNumber"", ""lastTwoYearDigits"", ""julianDay"", ""autoIncrement""],""components"": {""lineIdentifier"": {""action"": ""string"",""origin"": ""fixed"",""value"": ""QA""},""lineNumber"": {""action"": ""string"",""origin"": ""fixed"",""value"": ""4""},""fixedPart"": {""action"": ""string"",""origin"": ""fixed"",""value"": ""5""},""partNumber"": {""action"": ""string"",""origin"": ""program"",""lengthMin"": 6,""lengthMax"": 9},""lastTwoYearDigits"": {""action"": ""lastTwoYearDigits"",""origin"": ""program""},""julianDay"": {""action"": ""julianDay"",""origin"": ""program""},""autoIncrement"": {""action"": ""numeric"",""origin"": ""program"",""length"": 4,""incremental"": true}}}",
                Name = "WS100",
                MachineId = new MachineId(100),
                ProductId = new ProductId(653),
                Description = "CHSML",
                Version = 3,
                IsActive = true,
                CreatedBy = "Admin",
                CreatedOn = new DateTime(2023, 8, 28, 17, 2, 28),
                ModifiedBy = "0",
                ModifiedOn = new DateTime(2023, 8, 25, 12, 9, 37)
            },
            [2005] = new Rule
            {
                // Issue #58 (chunk 58-A): the routing authoring path stamps the constant RoutingRuleId 2005 onto
                // every authored clean edge, and WorkflowOrchestrator now runs a referential guard that requires
                // that rule id to resolve to a real Rule row before authoring. This seed keeps the guard DORMANT
                // in the test bed (authoring stays byte-identical) while it arms the production system.
                RuleId = 2005,
                Name = "ROUTING_DEFAULT",
                MachineId = new MachineId(0),
                ProductId = new ProductId(0),
                Description = "Routing authoring default rule (#58 referential guard fixture)",
                Version = 1,
                IsActive = true,
                CreatedBy = "Admin",
                CreatedOn = new DateTime(2023, 8, 28, 17, 2, 28),
                ModifiedBy = "Admin",
                ModifiedOn = new DateTime(2023, 8, 25, 12, 9, 37)
            }
        }.ToImmutableDictionary();

    /// <summary>
    /// Lazy-loaded cached list for maximum performance - best of both worlds
    /// </summary>
    private static readonly Lazy<IReadOnlyList<Rule>> _fixtureCache =
        new(() => _rulesDict.Values.ToList());

    /// <summary>
    /// Get all Rule entities (cached List from dictionary for backward compatibility)
    /// </summary>
    public static IReadOnlyList<Rule> Fixture => _fixtureCache.Value;

    /// <summary>
    /// Get a specific Rule by ID - O(1) lookup (standardized pattern)
    /// </summary>
    public static Rule? GetById(int id) =>
        _rulesDict.TryGetValue(id, out var rule) ? rule : null;

    /// <summary>
    /// Direct dictionary access for advanced scenarios (standardized pattern)
    /// </summary>
    public static IImmutableDictionary<int, Rule> Dictionary => _rulesDict;

    /// <summary>
    /// Check if a Rule exists by ID - O(1) lookup
    /// </summary>
    public static bool Contains(int id) => _rulesDict.ContainsKey(id);

    /// <summary>
    /// Get count of Rules - O(1) operation
    /// </summary>
    public static int Count => _rulesDict.Count;

    /// <summary>
    /// Get Rule by MachineId - O(n) operation
    /// </summary>
    public static Rule? GetByMachineId(int machineId) =>
        _rulesDict.Values.FirstOrDefault(r => r.MachineId == new MachineId(machineId));

    /// <summary>
    /// Get Rule by ProductId - O(n) operation
    /// </summary>
    public static Rule? GetByProductId(int productId) =>
        _rulesDict.Values.FirstOrDefault(r => r.ProductId == new ProductId(productId));

    /// <summary>
    /// Get Rule by Name - O(n) operation
    /// </summary>
    public static Rule? GetByName(string name) =>
        _rulesDict.Values.FirstOrDefault(r => r.Name == name);
}
