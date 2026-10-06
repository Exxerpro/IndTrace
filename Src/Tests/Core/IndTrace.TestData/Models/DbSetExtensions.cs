// <copyright file="DbSetExtensions.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Microsoft.EntityFrameworkCore;
using IndTrace.Domain.Enum.LookUpTable;
using IndTrace.TestData.Converters;

namespace IndTrace.TestData.Models;

/// <summary>
/// Provides extension methods for working with DbSet, including loading and adding entities from JSON files and handling navigation properties.
/// </summary>
internal static class DbSetExtensions
{
    private static ILogger? _logger;
    private static readonly JsonSerializerOptions ResilientOptions = ResilientJsonConverters.CreateResilientOptions();

    /// <summary>
    /// Sets the logger for DbSetExtensions. Call this during application startup.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging operations.</param>
    public static void SetLogger(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Adds a range of entities to the DbSet from a JSON file asynchronously.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="dataSet">The DbSet to add entities to.</param>
    /// <param name="filename">The JSON file name.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The updated DbSet.</returns>
    public static async Task<DbSet<T>> AddRangeAsyncFromJsonAsync<T>(this DbSet<T> dataSet,
        string filename,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var entitiesList = await LoadListFromJsonAsync<T>(filename, cancellationToken: cancellationToken);

        if (entitiesList is null)
        {
            return dataSet;
        }

        foreach (var entity in entitiesList)
        {
            // DetachAsync navigation properties if they exist
            DetachNavigationProperties(entity);
        }

        if (entitiesList.Any())
        {
            await dataSet.AddRangeAsync(entitiesList, cancellationToken);
        }

        return dataSet;
    }

    /// <summary>
    /// Detaches (nullifies) specified navigation properties for the given entity.
    /// This is useful for breaking relationships between entities, such as preventing
    /// serialization of related entities or avoiding Entity Framework tracking issues.
    /// </summary>
    /// <typeparam name="T">The type of the entity, constrained to be a class.</typeparam>
    /// <param name="entity">The entity whose navigation properties will be detached.</param>
    private static void DetachNavigationProperties<T>(T entity) where T : class
    {
        var entityType = entity.GetType();
        Dictionary<string, string[]> navigationPropertyNames = new Dictionary<string, string[]>
        {
            { "Product", new string[] { "Customer", "Line" } },
            { "Variable", new string[] { "VariablesGroup" } },
            { "Register", new string[] { "VariablesGroup" } },
        };

        var propertiesName = navigationPropertyNames
            .Where(k => k.Key == entityType.Name)
            .SelectMany(v => v.Value)
            .ToList();

        foreach (var propertyInfo in propertiesName
                     .Select(propertyName => entityType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance))
                     .OfType<PropertyInfo>()
                     .Where(propertyInfo => propertyInfo.CanWrite))
        {
            propertyInfo.SetValue(entity, null);
        }
    }

    public static async Task<DbSet<TLookUpTable>> AddRangeFromEnumerationAsync<TLookUpTable, TEnumeration>(
        this DbSet<TLookUpTable> entity,
        CancellationToken cancellationToken = default)
        where TLookUpTable : EnumLookUpTable, ILookUpTable, new()
        where TEnumeration : EnumModel, new()
    {
        var lookUpTables = EnumLookUp.ToLookUpTable<TLookUpTable, TEnumeration>();

        await entity.AddRangeAsync(lookUpTables.Where(e => e.Id > 0), cancellationToken);
        return entity;
    }

    public static async Task<IList<T>> LoadListFromJsonAsync<T>(string filename,
        CancellationToken cancellationToken = default)
        where T : class
    {
        try
        {
            var jsonText = await LoadContentFromJsonAsync(filename, cancellationToken);
            var entitiesList = JsonSerializer.Deserialize<List<T>>(jsonText, ResilientOptions);
            return entitiesList ?? [];
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error loading list from JSON file: {FileName}", filename);
        }
        return [];
    }

    public static async Task LoadListFromJsonAsync<T>(this List<T> list, string filename,
       CancellationToken cancellationToken = default)
       where T : class
    {
        try
        {
            var jsonText = await LoadContentFromJsonAsync(filename, cancellationToken);
            var entitiesList = JsonSerializer.Deserialize<List<T>>(jsonText, ResilientOptions);
            if (entitiesList is null)
            {
                return;
            }

            if (entitiesList is { Count: > 0 })
            {
                list.Clear();
                list.AddRange(entitiesList);
                return;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error loading list from JSON file: {FileName}", filename);
        }
        return;
    }

    public static async Task LoadListFromJsonAsync<T>(this List<T> list, string filename, string Key,
        CancellationToken cancellationToken = default)
        where T : class
    {
        try
        {
            if (list.Count > 0)
            {
                return;
            }

            var jsonText = await LoadContentFromJsonAsync(filename, cancellationToken);
            var entitiesList = JsonSerializer.Deserialize<List<T>>(jsonText, ResilientOptions);

            if (entitiesList is { Count: > 0 })
            {
                if (typeof(T).GetProperty(Key) == null)
                {
                    _logger?.LogWarning("Property '{Key}' does not exist in type '{TypeName}'", Key, typeof(T).Name);
                }
                var duplicates = entitiesList.GroupBy(e => e.GetType().GetProperty(Key)?.GetValue(e))
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();
                if (duplicates.Count > 0)
                {
                    entitiesList = entitiesList.Where(e => !duplicates.Contains(e.GetType().GetProperty(Key)?.GetValue(e))).ToList();
                }
                foreach (var duplicate in duplicates)
                {
                    _logger?.LogWarning("Duplicate found: {Duplicate} on list {PropertyKey}", duplicate, typeof(T).GetProperty(Key));
                }

                list.Clear();
                list.AddRange(entitiesList);
                return;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error loading list from JSON file: {FileName} with key: {Key}", filename, Key);
        }
        return;
    }

    public static async Task LoadListFromJsonAsync<T>(this List<T> list, string filename, string key1, string key2,
    CancellationToken cancellationToken = default)
    where T : class, new()
    {
        try
        {
            if (list.Count > 0)
                return;

            var jsonText = await LoadContentFromJsonAsync(filename, cancellationToken);
            var entitiesList = JsonSerializer.Deserialize<List<T>>(jsonText, ResilientOptions);

            if (entitiesList is not { Count: > 0 })
                return;

            var type = typeof(T);
            var prop1 = type.GetProperty(key1);
            var prop2 = type.GetProperty(key2);

            if (prop1 == null || prop2 == null)
            {
                _logger?.LogError("Property '{Key1}' or '{Key2}' not found on type '{TypeName}'", key1, key2, type.Name);
                return;
            }

            var duplicates = entitiesList
                .GroupBy(e =>
                {
                    var val1 = prop1.GetValue(e)?.ToString() ?? "";
                    var val2 = prop2.GetValue(e)?.ToString() ?? "";
                    return $"{val1}|{val2}";
                })
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicates.Count > 0)
            {
                entitiesList = entitiesList
                    .Where(e =>
                    {
                        var key = $"{prop1.GetValue(e)?.ToString() ?? ""}|{prop2.GetValue(e)?.ToString() ?? ""}";
                        return !duplicates.Contains(key);
                    })
                    .ToList();

                foreach (var dup in duplicates)
                    _logger?.LogWarning("Duplicate found: {Duplicate} in {FileName}", dup, filename);
            }

            list.Clear();
            list.AddRange(entitiesList);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error loading list from JSON file: {FileName} with keys: {Key1}, {Key2}", filename, key1, key2);
        }
    }

    public static async Task<Dictionary<string, T>> LoadDictionaryFromJsonAsync<T>(string filename, string key,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var jsonText = await LoadContentFromJsonAsync(filename, cancellationToken);
        var entitiesList = JsonSerializer.Deserialize<List<T>>(jsonText, ResilientOptions) ?? [];

        var type = typeof(T);
        var keyProp = type.GetProperty(key);

        if (keyProp == null)
            throw new ArgumentException($"Property '{key}' does not exist on type '{type.Name}'.");

        // Use the value of the specified property as the dictionary key
        var dict = new Dictionary<string, T>();
        foreach (var entity in entitiesList)
        {
            var keyValue = keyProp.GetValue(entity)?.ToString();
            if (!string.IsNullOrEmpty(keyValue) && !dict.ContainsKey(keyValue))
            {
                dict[keyValue] = entity;
            }
        }
        return dict;
    }

    public static async Task<T> LoadObjectFromJsonAsync<T>(string filename,
        CancellationToken cancellationToken = default)
        where T : class

    {
        var listFromJson = await LoadListFromJsonAsync<T>(filename, cancellationToken: cancellationToken);
        return listFromJson.FirstOrDefault()!;
    }

    public static async Task<string> LoadContentFromJsonAsync(string filename, CancellationToken cancellationToken = default)
    {
        //[Fix]
        //CLAUDE
        //Date: 26/08/2025
        //Reason: [Architecture] - Use embedded resources from IndTrace.TestData instead of DataFileLocator

        // Use IndTrace.TestData embedded resources instead of file system
        var loader = new EmbeddedTestDataLoader();
        var jsonContent = await loader.LoadJsonContentAsync(filename, cancellationToken);

        if (string.IsNullOrEmpty(jsonContent))
            throw new FileNotFoundException($"Could not locate embedded resource: {filename}");

        return jsonContent;
    }
}
