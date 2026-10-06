// <copyright file="TestDataHelper.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.TestData.RawData;

/// <summary>
/// Helper class for efficiently deserializing test data from raw string literals.
/// </summary>
internal static class TestDataHelper
{
    /// <summary>
    /// Optimized JSON serializer options for test data deserialization.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false, // No need for pretty printing in tests
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Deserializes JSON string to a strongly-typed object with optimized settings.
    /// </summary>
    /// <typeparam name="T">The target type to deserialize to.</typeparam>
    /// <param name="jsonString">The JSON string to deserialize.</param>
    /// <returns>The deserialized object.</returns>
    public static T? Deserialize<T>(string jsonString)
    {
        return JsonSerializer.Deserialize<T>(jsonString, JsonOptions);
    }

    /// <summary>
    /// Deserializes JSON string to a list of strongly-typed objects with optimized settings.
    /// </summary>
    /// <typeparam name="T">The target type to deserialize to.</typeparam>
    /// <param name="jsonString">The JSON string to deserialize.</param>
    /// <returns>The deserialized list of objects.</returns>
    public static List<T>? DeserializeList<T>(string jsonString)
    {
        return JsonSerializer.Deserialize<List<T>>(jsonString, JsonOptions);
    }

    /// <summary>
    /// Deserializes JSON string to an array of strongly-typed objects with optimized settings.
    /// </summary>
    /// <typeparam name="T">The target type to deserialize to.</typeparam>
    /// <param name="jsonString">The JSON string to deserialize.</param>
    /// <returns>The deserialized array of objects.</returns>
    public static T[]? DeserializeArray<T>(string jsonString)
    {
        return JsonSerializer.Deserialize<T[]>(jsonString, JsonOptions);
    }

    /// <summary>
    /// Creates a test data loader that caches deserialized results for better performance.
    /// </summary>
    /// <typeparam name="T">The type of data to load.</typeparam>
    /// <param name="jsonString">The JSON string containing the test data.</param>
    /// <returns>A function that returns the deserialized data (cached after first call).</returns>
    public static Func<T?> CreateCachedLoader<T>(string jsonString)
    {
        T? cachedResult = default;
        bool isLoaded = false;

        return () =>
        {
            if (!isLoaded)
            {
                cachedResult = Deserialize<T>(jsonString);
                isLoaded = true;
            }
            return cachedResult;
        };
    }

    /// <summary>
    /// Creates a test data loader that caches deserialized list results for better performance.
    /// </summary>
    /// <typeparam name="T">The type of data to load.</typeparam>
    /// <param name="jsonString">The JSON string containing the test data.</param>
    /// <returns>A function that returns the deserialized list data (cached after first call).</returns>
    public static Func<List<T>?> CreateCachedListLoader<T>(string jsonString)
    {
        List<T>? cachedResult = null;
        bool isLoaded = false;

        return () =>
        {
            if (!isLoaded)
            {
                cachedResult = DeserializeList<T>(jsonString);
                isLoaded = true;
            }
            return cachedResult;
        };
    }

    /// <summary>
    /// Validates that the JSON string can be deserialized to the specified type.
    /// </summary>
    /// <typeparam name="T">The target type to validate against.</typeparam>
    /// <param name="jsonString">The JSON string to validate.</param>
    /// <returns>True if the JSON can be deserialized to the specified type; otherwise, false.</returns>
    public static bool ValidateJson<T>(string jsonString)
    {
        try
        {
            JsonSerializer.Deserialize<T>(jsonString, JsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Gets a sample object of the specified type for testing purposes.
    /// </summary>
    /// <typeparam name="T">The type to create a sample for.</typeparam>
    /// <param name="jsonString">The JSON string containing sample data.</param>
    /// <returns>The first item from the deserialized list.</returns>
    /// <exception cref="InvalidOperationException">The JSON deserialized to no items — a test-data defect that must fail loudly.</exception>
    public static T GetSample<T>(string jsonString)
    {
        var list = DeserializeList<T>(jsonString);
        return list != null && list.Count > 0
            ? list[0]
            : throw new InvalidOperationException($"Sample JSON contained no items of type {typeof(T).Name}.");
    }
}

//TODO [PERFORMANCE][CURSOR][20/JUNE/2025] - Consider using source generators for even faster deserialization in high-performance test scenarios.
//TODO [BEST PRACTICE][CURSOR][20/JUNE/2025] - Consider adding validation for required properties and data integrity checks. 
