// <copyright file="MappingTestHelper.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.Mapper
{
    public static class MappingTestHelper
    {
        public static List<MappingResult> AssertAllPropertiesMapped(object source, object target, ITestOutputHelper output)
        {
            var results = new List<MappingResult>();
            var sourceProps = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var targetProps = target.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .ToDictionary(p => p.Name);
            var sourceType = source.GetType().Name;
            var targetType = target.GetType().Name;

            output.WriteLine($"=== Mapping: {sourceType} → {targetType}");

            foreach (var sp in sourceProps)
            {
                if (GloballyIgnoredProperties.Contains(sp.Name))
                {
                    output.WriteLine($"⚠️ [WARN] Property '{sp.Name}' is globally ignored.");
                    results.Add(new MappingResult
                    {
                        SourceType = sourceType,
                        TargetType = targetType,
                        Property = sp.Name,
                        Status = "WARN",
                        SourceValue = sp.GetValue(source)?.ToString() ?? "",
                        TargetValue = targetProps.TryGetValue(sp.Name, out var tp1) ? tp1.GetValue(target)?.ToString() ?? "" : ""
                    });
                    continue;
                }

                if (targetProps.TryGetValue(sp.Name, out var tp2))
                {
                    var sourceVal = sp.GetValue(source);
                    var targetVal = tp2.GetValue(target);

                    var pass = AreValuesEquivalent(sourceVal, targetVal);
                    var status = pass ? "PASS" : "FAIL";
                    output.WriteLine(pass
                        ? $"✅ [PASS] Property '{sp.Name}' mapped correctly. Source Value: {sourceVal}, Target Value: {targetVal}"
                        : $"❌ [FAIL] Property '{sp.Name}' not mapped correctly. Source Value: {sourceVal}, Target Value: {targetVal}");

                    results.Add(new MappingResult
                    {
                        SourceType = sourceType,
                        TargetType = targetType,
                        Property = sp.Name,
                        Status = status,
                        SourceValue = sourceVal?.ToString() ?? "",
                        TargetValue = targetVal?.ToString() ?? ""
                    });
                }
                else
                {
                    output.WriteLine($"⚠️ [MISS] Property '{sp.Name}' missing in target.");
                    results.Add(new MappingResult
                    {
                        SourceType = sourceType,
                        TargetType = targetType,
                        Property = sp.Name,
                        Status = "MISS",
                        SourceValue = sp.GetValue(source)?.ToString() ?? "",
                        TargetValue = ""
                    });
                }
            }
            return results;
        }

        private static readonly HashSet<string> GloballyIgnoredProperties =
        [
            "CreatedOn",
            "ModifiedOn",
            "CreatedBy",
            "ModifiedBy"
        ];

        private static bool AreValuesEquivalent(object? sourceVal, object? targetVal)
        {
            if (sourceVal is null && targetVal is null) return true;
            if (sourceVal is null || targetVal is null) return false;

            if (targetVal?.GetType().Name.Contains("Enum") == true)
            {
                var val = targetVal.GetType().GetProperty("Value")?.GetValue(targetVal);
                return Equals(sourceVal, val);
            }

            // SmartEnum or enum mapping workaround (match on int or string)
            if (sourceVal.GetType().IsEnum && targetVal is int i)
                return Convert.ToInt32(sourceVal) == i;

            if (targetVal?.GetType().GetProperty("Value") == null) return Equals(sourceVal, targetVal);

            var valProp = targetVal.GetType().GetProperty("Value");
            var value = valProp?.GetValue(targetVal);
            return Equals(value, sourceVal);
        }
    }
}