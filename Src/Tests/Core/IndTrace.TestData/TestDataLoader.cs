// <copyright file="TestDataLoader.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.IO.MemoryMappedFiles;
using IndTrace.TestData.RawData;
using System.Text.Json;
using IndTrace.TestData.Loaders;
using IndTrace.TestData.Converters;

namespace IndTrace.TestData;

/// <summary>
/// Hybrid test data loader that optimizes loading based on file size and type.
/// </summary>
public static class TestDataLoader
{
    /// <summary>
    /// Threshold in bytes to determine whether to use raw string literals or file loading.
    /// </summary>
    private const int RawStringThreshold = 100 * 1024; // 100KB

    /// <summary>
    /// Optimized JSON serializer options for test data.
    /// </summary>
    //[Fix]
    //CLAUDE
    //Date: 30/08/2025
    //Reason: [Pattern SmartEnum] - Use ResilientJsonConverters to handle SmartEnum serialization
    private static readonly JsonSerializerOptions JsonOptions =
        ResilientJsonConverters.CreateResilientOptions();

    /// <summary>
    /// Cache for large files to avoid repeated loading.
    /// </summary>
    private static readonly Dictionary<string, object> _largeFileCache = [];

    /// <summary>
    /// Loads test data using the most appropriate method based on file characteristics.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="fileName">The name of the file to load.</param>
    /// <returns>The deserialized data.</returns>
    public static async Task<List<T>> LoadDataAsync<T>(string fileName) where T : class
    {
        // Check if it's a small file that can use raw string literals
        if (IsSmallFile(fileName))
        {
            var data = await LoadFromRawStringAsync<T>(fileName);
            LogUsageForList(typeof(T).Name, data);
            return data;
        }

        // For large files, use optimized file loading
        var result = await LoadFromFileOptimizedAsync<T>(fileName);
        LogUsageForList(typeof(T).Name, result);
        return result;
    }

    /// <summary>
    /// Determines if a file should use raw string literals based on size and type.
    /// </summary>
    private static bool IsSmallFile(string fileName)
    {
        // Small files that are good candidates for raw string literals
        var smallFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "BarCodes.json",
            "Config.json",
            "ConfigApp.json",
            "Settings.json",
            "Rules.json",
            "Customers.json",
            "Dict.json",
            "WorkFlows.json",
            "Lines.json",
            "PLCs.json",
            "Machines.json",
            "MachinePlcs.json",
            "VariablesGroups.json",
            "Cycles.json",      // Add this
            "Registers.json",   // Add this
            "Variables.json"    // Add this
        };

        return smallFiles.Contains(fileName);
    }

    /// <summary>
    /// Loads data from raw string literals for small files.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    private static Task<List<T>> LoadFromRawStringAsync<T>(string fileName) where T : class
    {
        // Try direct C# object access first
        var data = GetStaticData<T>(fileName);
        if (data != null)
        {
            LogUsageForList(typeof(T).Name, data);
            //[Fix]
            //CLAUDE
            //Date: 30/08/2025
            //Reason: [Pattern X] - Direct object return, no serialization
            return Task.FromResult(data);
        }

        // For JSON string data (Machines, PLCs, Dict, Customers)
        var jsonString = GetRawStringData(fileName);
        if (!string.IsNullOrEmpty(jsonString))
        {
            var result = JsonSerializer.Deserialize<List<T>>(jsonString, JsonOptions);
            return Task.FromResult(result ?? []);
        }

        throw new InvalidOperationException($"No raw string data found for {fileName}");
    }

    /// <summary>
    /// Gets static data directly from RawData classes without serialization.
    /// </summary>
    private static List<T>? GetStaticData<T>(string fileName) where T : class
    {
        return fileName switch
        {
            "BarCodes.json" => BarCodeRawData.Fixture as List<T>,
            "Config.json" => ConfigAppRawData.Fixture as List<T>,
            "ConfigApp.json" => ConfigAppRawData.Fixture as List<T>,
            "Settings.json" => SettingsRawData.Fixture as List<T>,
            "Cycles.json" => CyclesRawData.Fixture as List<T>,
            "Registers.json" => RegistersRawData.Fixture as List<T>,
            "Variables.json" => VariablesRawData.Fixture as List<T>,
            "Customers.json" => CustomerRawData.Fixture as List<T>,
            "Dict.json" => ProductRawData.FixtureProducts as List<T>,
            "WorkFlows.json" => WorkFlowRawData.Fixture as List<T>,
            "Lines.json" => LineRawData.Fixture as List<T>,
            "PLCs.json" => PlcRawData.Fixture as List<T>,
            "Machines.json" => MachineRawData.Fixture as List<T>,
            "Recipes.json" => RecipeRawData.Fixture as List<T>,
            _ => null
        };
    }

    /// <summary>
    /// Loads data from file using memory-mapped files for large files.
    /// </summary>
    private static async Task<List<T>> LoadFromFileOptimizedAsync<T>(string fileName) where T : class
    {
        // Check cache first
        if (_largeFileCache.TryGetValue(fileName, out var cachedData))
        {
            return (List<T>)cachedData;
        }

        var filePath = GetFilePath(fileName);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Test data file not found: {filePath}");
        }

        // Use memory-mapped files for very large files
        if (new FileInfo(filePath).Length > 1024 * 1024) // > 1MB
        {
            var result = await LoadWithMemoryMapping<T>(filePath);
            _largeFileCache[fileName] = result;
            return result;
        }

        // Use regular file reading for medium files
        var jsonString = await File.ReadAllTextAsync(filePath);
        var data = JsonSerializer.Deserialize<List<T>>(jsonString, JsonOptions);
        var resultList = data ?? [];
        _largeFileCache[fileName] = resultList;
        return resultList;
    }

    /// <summary>
    /// Loads very large files using memory-mapped files for better performance.
    /// </summary>
    private static async Task<List<T>> LoadWithMemoryMapping<T>(string filePath) where T : class
    {
        using var mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
        using var stream = mmf.CreateViewStream(0, 0, MemoryMappedFileAccess.Read);
        using var reader = new StreamReader(stream);

        var jsonString = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);
        var data = JsonSerializer.Deserialize<List<T>>(jsonString, JsonOptions);
        return data ?? [];
    }

    /// <summary>
    /// Gets the file path for test data files.
    /// </summary>
    private static string GetFilePath(string fileName)
    {
        var currentDir = Directory.GetCurrentDirectory();
        var seedDataPath = Path.Combine(currentDir, "SeedDataFiles", fileName);

        if (File.Exists(seedDataPath))
            return seedDataPath;

        // Fallback to parent directories
        var parentDir = Directory.GetParent(currentDir);
        while (parentDir != null)
        {
            seedDataPath = Path.Combine(parentDir.FullName, "SeedDataFiles", fileName);
            if (File.Exists(seedDataPath))
                return seedDataPath;
            parentDir = parentDir.Parent;
        }

        throw new FileNotFoundException($"Could not find {fileName} in any SeedDataFiles directory");
    }

    /// <summary>
    /// Gets raw string data for small files. This would be populated with actual JSON data.
    /// </summary>
    private static string GetRawStringData(string fileName)
    {
        // JSON strategy is deprecated - returning empty strings as placeholders
        // All data should use GetStaticData method instead
        return string.Empty;
    }

    /// <summary>
    /// Clears the cache for large files.
    /// </summary>
    public static void ClearCache()
    {
        _largeFileCache.Clear();
    }

    /// <summary>
    /// Gets cache statistics for monitoring.
    /// </summary>
    public static (int CachedFiles, long TotalSize) GetCacheStats()
    {
        var totalSize = _largeFileCache.Values.Sum(x =>
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(x).Length);
        return (_largeFileCache.Count, totalSize);
    }

    private static void LogUsageForList<T>(string type, List<T> list)
    {
        if (list == null) return;
        foreach (var item in list)
        {
            var idProp = item?.GetType().GetProperty("UserId");
            if (idProp != null)
            {
                var idValue = idProp.GetValue(item)?.ToString();
                if (!string.IsNullOrEmpty(idValue))
                    TestEntityDataUsageTracker.LogUsage(type, idValue);
            }
        }
    }
}