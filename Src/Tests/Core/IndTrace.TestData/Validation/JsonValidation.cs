// <copyright file="JsonValidation.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.TestData.Validation;

/// <summary>
/// Provides JSON validation utilities for test data quality assurance.
/// These utilities help ensure JSON data can be properly deserialized to expected types.
/// </summary>
internal static class JsonValidation
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    /// <summary>
    /// Validates that the JSON string can be deserialized to the specified type.
    /// </summary>
    /// <typeparam name="T">The target type to validate against.</typeparam>
    /// <param name="jsonString">The JSON string to validate.</param>
    /// <param name="options">Optional custom serializer options. If null, uses default options.</param>
    /// <returns>True if the JSON can be deserialized to the specified type; otherwise, false.</returns>
    public static bool ValidateJson<T>(string jsonString, JsonSerializerOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(jsonString))
            return false;

        try
        {
            JsonSerializer.Deserialize<T>(jsonString, options ?? DefaultOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Validates that the JSON string can be deserialized to the specified type and returns detailed error information.
    /// </summary>
    /// <typeparam name="T">The target type to validate against.</typeparam>
    /// <param name="jsonString">The JSON string to validate.</param>
    /// <param name="error">When validation fails, contains the error message; otherwise, null.</param>
    /// <param name="options">Optional custom serializer options. If null, uses default options.</param>
    /// <returns>True if the JSON can be deserialized to the specified type; otherwise, false.</returns>
    public static bool ValidateJsonWithError<T>(string jsonString, out string? error, JsonSerializerOptions? options = null)
    {
        error = null;

        if (string.IsNullOrWhiteSpace(jsonString))
        {
            error = "JSON string is null or whitespace";
            return false;
        }

        try
        {
            JsonSerializer.Deserialize<T>(jsonString, options ?? DefaultOptions);
            return true;
        }
        catch (JsonException ex)
        {
            error = $"JSON deserialization failed: {ex.Message}";
            if (ex.LineNumber.HasValue)
                error += $" at line {ex.LineNumber}";
            if (ex.BytePositionInLine.HasValue)
                error += $", position {ex.BytePositionInLine}";
            return false;
        }
    }

    /// <summary>
    /// Validates JSON from an embedded resource file.
    /// </summary>
    /// <typeparam name="T">The target type to validate against.</typeparam>
    /// <param name="fileName">The name of the embedded resource file.</param>
    /// <returns>A result containing success status and error message if validation fails.</returns>
    public static async Task<(bool IsValid, string Error)> ValidateEmbeddedJsonFileAsync<T>(string fileName)
        where T : class
    {
        var loader = new EmbeddedTestDataLoader();

        if (!loader.Exists(fileName))
        {
            return (false, $"File '{fileName}' not found in embedded resources");
        }

        try
        {
            var data = await loader.LoadListAsync<T>(fileName);
            return (true, string.Empty);
        }
        catch (InvalidOperationException ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>
    /// Validates all embedded JSON files for proper deserialization.
    /// </summary>
    /// <returns>A dictionary of file names to validation results.</returns>
    public static async Task<Dictionary<string, JsonFileValidationResult>> ValidateAllEmbeddedJsonFilesAsync()
    {
        var results = new Dictionary<string, JsonFileValidationResult>();
        var loader = new EmbeddedTestDataLoader();

        var validationTasks = new[]
        {
            ValidateFileAsync<Domain.Entities.Rule>("Rules.json", loader),
            ValidateFileAsync<Domain.Entities.Machine>("Machines.json", loader),
            ValidateFileAsync<Domain.Entities.Plc>("PLCs.json", loader),
            ValidateFileAsync<Domain.Entities.MachinePlc>("MachinePlcs.json", loader),
            ValidateFileAsync<Domain.Entities.VariablesGroup>("VariablesGroups.json", loader),
            ValidateFileAsync<Domain.Entities.Variable>("Variables.json", loader),
            ValidateFileAsync<Domain.Entities.Line>("Lines.json", loader),
            ValidateFileAsync<Domain.Entities.Customer>("Customers.json", loader),
            ValidateFileAsync<Domain.Entities.Product>("Products.json", loader),
            ValidateFileAsync<Domain.Entities.Recipe>("Recipes.json", loader),
            ValidateFileAsync<Domain.Entities.WorkFlow>("WorkFlows.json", loader),
            ValidateFileAsync<Domain.Entities.ConfigApp>("ConfigApp.json", loader),
            ValidateFileAsync<Domain.Entities.Setting>("Settings.json", loader),
            ValidateFileAsync<Domain.Entities.BarCodes.BarCode>("BarCodes.json", loader),
            ValidateFileAsync<Domain.Entities.Cycle>("Cycles.json", loader),
            ValidateFileAsync<Domain.Entities.Register>("Registers.json", loader)
        };

        var taskResults = await Task.WhenAll(validationTasks);

        foreach (var result in taskResults)
        {
            results[result.FileName] = result;
        }

        return results;
    }

    private static async Task<JsonFileValidationResult> ValidateFileAsync<T>(string fileName, ITestDataLoader loader)
        where T : class
    {
        var result = new JsonFileValidationResult { FileName = fileName };

        try
        {
            var data = await loader.LoadListAsync<T>(fileName);
            result.IsValid = true;
            result.RecordCount = data?.Count ?? 0;
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.Error = ex.Message;
        }

        return result;
    }
}

//[Fix] CLAUDE - Date: 26/08/2025
//Reason: [CS8632] - Removed nullable annotations (JsonSerializerOptions?, string?, etc.) to eliminate nullable reference type warnings since project has <Nullable>disable</Nullable>
// Also moved and enhanced ValidateJson functionality from TestDataHelper to TestData assembly
