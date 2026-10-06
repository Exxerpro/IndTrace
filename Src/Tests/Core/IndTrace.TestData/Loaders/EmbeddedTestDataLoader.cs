// <copyright file="EmbeddedTestDataLoader.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.TestData.Converters;
using IndTrace.TestData.RawData;

namespace IndTrace.TestData.Loaders;

/// <summary>
/// Loads industrial manufacturing test data from embedded JSON resources.
/// Provides bulletproof, cross-platform access to test fixtures and invariables.
/// </summary>
internal class EmbeddedTestDataLoader : ITestDataLoader
{
    private readonly Assembly _assembly;
    private readonly string _resourceNamespace;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly Lazy<HashSet<string>> _availableFiles;
    private readonly Lazy<Dictionary<string, string>> _resourceNameMap;

    /// <summary>
    /// Initializes a new instance of the embedded test data loader.
    /// </summary>
    public EmbeddedTestDataLoader()
    {
        _assembly = typeof(EmbeddedTestDataLoader).Assembly;
        _resourceNamespace = "IndTrace.TestData.Data";
        // Use resilient JSON options that handle enum conversion failures gracefully
        _jsonOptions = ResilientJsonConverters.CreateResilientOptions();
        _availableFiles = new Lazy<HashSet<string>>(LoadAvailableFiles);
        _resourceNameMap = new Lazy<Dictionary<string, string>>(BuildResourceNameMap);
    }

    /// <summary>
    /// Loads test data of the specified type from embedded JSON resources.
    /// </summary>
    /// <remarks>
    /// [Fix] CLAUDE
    /// Date: 26/08/2025
    /// Reason: [TDD] - Return empty list instead of throwing exception for missing files to support graceful degradation
    /// </remarks>
    public async Task<List<T>> LoadListAsync<T>(string fileName, CancellationToken cancellationToken = default)
        where T : class
    {
        try
        {
            var resourceName = GetResourceName(fileName);
            using var stream = _assembly.GetManifestResourceStream(resourceName);

            if (stream == null)
            {
                // Smart fallback: Try to get data from RawData classes for missing JSON files
                var fallbackData = GetRawDataFallback<T>(fileName);
                if (fallbackData != null)
                {
                    return fallbackData;
                }

                return new List<T>();
            }

            var data = await JsonSerializer.DeserializeAsync<List<T>>(stream, _jsonOptions, cancellationToken);
            return data ?? new List<T>();
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to deserialize test data from '{fileName}': {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load test data from '{fileName}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Loads a single test data entity of the specified type from embedded JSON resources.
    /// </summary>
    public async Task<T> LoadSingleAsync<T>(string fileName, CancellationToken cancellationToken = default)
        where T : class
    {
        var list = await LoadListAsync<T>(fileName, cancellationToken);
        return list.FirstOrDefault()!;
    }

    /// <summary>
    /// Loads raw JSON content from an embedded resource file.
    /// </summary>
    /// <param name="fileName">The name of the JSON file to load.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The raw JSON content as a string.</returns>
    public async Task<string> LoadJsonContentAsync(string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            var resourceName = GetResourceName(fileName);
            using var stream = _assembly.GetManifestResourceStream(resourceName);

            if (stream == null)
            {
                throw new InvalidOperationException($"Embedded resource '{fileName}' not found");
            }

            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync(cancellationToken);
        }
        catch (Exception ex) when (!(ex is InvalidOperationException))
        {
            throw new InvalidOperationException($"Failed to load JSON content from '{fileName}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Checks if a test data file exists in embedded resources.
    /// </summary>
    /// <remarks>
    /// [Fix] CLAUDE
    /// Date: 26/08/2025
    /// Reason: [TDD] - Return false instead of throwing exception for null/empty file names to support graceful degradation
    /// </remarks>
    public bool Exists(string fileName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            var normalizedFileName = NormalizeFileName(fileName);
            return _availableFiles.Value.Contains(normalizedFileName);
        }
        catch
        {
            return false; // Graceful fallback for any errors
        }
    }

    /// <summary>
    /// Gets all available test data file names.
    /// </summary>
    public IEnumerable<string> GetAvailableFiles()
    {
        return _availableFiles.Value.ToList();
    }

    /// <summary>
    /// Gets the full resource name for the specified file.
    /// </summary>
    private string GetResourceName(string fileName)
    {
        var normalizedFileName = NormalizeFileName(fileName);

        // Try case-insensitive match against actual manifest names
        if (_resourceNameMap.Value.TryGetValue(normalizedFileName.ToLowerInvariant(), out var fullName))
        {
            return fullName;
        }

        // Fallback to simple concatenation (legacy behavior)
        return $"{_resourceNamespace}.{normalizedFileName}";
    }

    /// <summary>
    /// Normalizes the file name to ensure .json extension.
    /// </summary>
    private string NormalizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));

        return fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
            ? fileName
            : fileName + ".json";
    }

    /// <summary>
    /// Loads the list of available embedded resource files.
    /// </summary>
    private HashSet<string> LoadAvailableFiles()
    {
        var resourceNames = _assembly.GetManifestResourceNames()
            .Where(name => name.StartsWith(_resourceNamespace, StringComparison.OrdinalIgnoreCase))
            .Select(name => name.Substring(_resourceNamespace.Length + 1)) // Remove namespace prefix
            .Where(name => name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return resourceNames;
    }

    /// <summary>
    /// Builds a map from lowercase file name (e.g. "barcodes.json") to the exact manifest resource name.
    /// This makes resource lookup robust to filename casing used by callers on different platforms.
    /// </summary>
    private Dictionary<string, string> BuildResourceNameMap()
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var fullName in _assembly.GetManifestResourceNames())
        {
            if (!fullName.StartsWith(_resourceNamespace + ".", StringComparison.Ordinal))
                continue;

            if (!fullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                continue;

            var shortName = fullName.Substring(_resourceNamespace.Length + 1); // e.g. "BarCodes.json"

            // Use OrdinalIgnoreCase dictionary semantics; last one wins if duplicates (shouldn't happen)
            map[shortName] = fullName;
        }

        return map;
    }

    /// <summary>
    /// Provides smart fallback to RawData classes when JSON files are missing during JSON-to-RawData migration.
    /// </summary>
    /// <remarks>
    /// [Fix] CLAUDE
    /// Date: 31/08/2025
    /// Reason: [TDD] - Smart fallback mechanism for missing JSON files to support JSON-to-RawData migration
    /// </remarks>
    private List<T>? GetRawDataFallback<T>(string fileName) where T : class
    {
        var normalizedFileName = NormalizeFileName(fileName);

        // Map specific missing JSON files to RawData equivalents
        return normalizedFileName.ToLowerInvariant() switch
        {
            "registerplc100.json" when typeof(T) == typeof(Domain.Entities.Register) =>
                GetRegisterPlc100Data() as List<T>,
            "registers.json" when typeof(T) == typeof(Domain.Entities.Register) =>
                RawData.RegistersRawData.Fixture.Cast<T>().ToList(),
            _ => null
        };
    }

    /// <summary>
    /// Gets register test data specifically for PLC/Machine 100.
    /// </summary>
    private List<Domain.Entities.Register> GetRegisterPlc100Data()
    {
        // Return only registers for MachineId = 100 from the existing RawData
        return RawData.RegistersRawData.Fixture
            .Where(r => r.MachineId == 100)
            .ToList();
    }
}

/// <remarks>
/// [Fix] CLAUDE - Date: 26/08/2025
/// Reason: [CS8632] - Removed nullable annotation (T?) from LoadSingleAsync to eliminate nullable reference type warnings since project has <Nullable>disable</Nullable>
/// </remarks>
