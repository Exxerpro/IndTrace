// <copyright file="MappingScanner.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.Mapper;
/// <summary>
/// Represents the MappingScanner.
/// </summary>

public class MappingScanner(params Assembly[] assemblies)
{
    /// <summary>
    /// Executes GetApplicationWithDto operation.
    /// </summary>
    /// <returns>The result of GetApplicationWithDto.</returns>
    public ApplicationWithDto GetApplicationWithDto()
    {
        return new ApplicationWithDto(assemblies, GetMappingPairs().ToList());
    }
    /// <summary>
    /// Executes GetMappingPairs operation.
    /// </summary>
    /// <returns>The result of GetMappingPairs.</returns>

    public IEnumerable<ScannedClass> GetMappingPairs()
    {
        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericTypeDefinition);

            foreach (var type in types)
            {
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                {
                    if (method.GetParameters().Length != 1) continue;

                    if (new[] { "ToDto", "ToEntity", "ToDtoList" }.Contains(method.Name))
                    {
                        var sourceType = method.GetParameters()[0].ParameterType;
                        var returnType = method.ReturnType;

                        yield return new ScannedClass(method.Name, method, sourceType, returnType);
                    }
                }
            }
        }
    }
}