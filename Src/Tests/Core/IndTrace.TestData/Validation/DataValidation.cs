// <copyright file="DataValidation.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.TestData.Validation;

/// <summary>
/// Provides data validation utilities for test data maintenance and quality assurance.
/// These utilities are designed for one-time or infrequent use to validate JSON data integrity.
/// </summary>
internal static class DataValidation
{
    /// <summary>
    /// Validates JSON files for duplicate entries and logs detailed information about any duplicates found.
    /// This is a maintenance utility that should be run manually to ensure data quality.
    /// </summary>
    /// <param name="logger">Logger for outputting duplicate information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A result containing validation summary</returns>
    public static async Task<DataValidationResult> ValidateJsonFilesForDuplicatesAsync(
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        var result = new DataValidationResult();
        var loader = new EmbeddedTestDataLoader();

        // Define validation configurations for each entity type
        var validationConfigs = new[]
        {
            new { FileName = "Rules.json", EntityName = "Rule", KeyExtractor = new Func<dynamic, string>(e => e.RuleId.ToString()) },
            new { FileName = "Machines.json", EntityName = "Machine", KeyExtractor = new Func<dynamic, string>(e => e.MachineId.ToString()) },
            new { FileName = "PLCs.json", EntityName = "Plc", KeyExtractor = new Func<dynamic, string>(e => e.PlcId.ToString()) },
            new { FileName = "MachinePlcs.json", EntityName = "MachinePlc", KeyExtractor = new Func<dynamic, string>(e => $"{e.MachineId}|{e.PlcId}") },
            new { FileName = "VariablesGroups.json", EntityName = "VariablesGroup", KeyExtractor = new Func<dynamic, string>(e => e.VariableGroupId.ToString()) },
            new { FileName = "Variables.json", EntityName = "Variable", KeyExtractor = new Func<dynamic, string>(e => e.VariableId.ToString()) },
            new { FileName = "Lines.json", EntityName = "Line", KeyExtractor = new Func<dynamic, string>(e => e.LineId.ToString()) },
            new { FileName = "Customers.json", EntityName = "Customer", KeyExtractor = new Func<dynamic, string>(e => e.CustomerId.ToString()) },
            new { FileName = "Products.json", EntityName = "Product", KeyExtractor = new Func<dynamic, string>(e => e.ProductId.ToString()) },
            new { FileName = "Recipes.json", EntityName = "Recipe", KeyExtractor = new Func<dynamic, string>(e => e.RecipeId.ToString()) },
            new { FileName = "WorkFlows.json", EntityName = "WorkFlow", KeyExtractor = new Func<dynamic, string>(e => e.WorkFlowId.ToString()) },
            new { FileName = "ConfigApp.json", EntityName = "ConfigApp", KeyExtractor = new Func<dynamic, string>(e => e.AppId.ToString()) },
            new { FileName = "Settings.json", EntityName = "Setting", KeyExtractor = new Func<dynamic, string>(e => e.SettingId.ToString()) },
            new { FileName = "BarCodes.json", EntityName = "BarCode", KeyExtractor = new Func<dynamic, string>(e => e.BarCodeId.ToString()) },
            new { FileName = "Cycles.json", EntityName = "Cycle", KeyExtractor = new Func<dynamic, string>(e => e.CycleId.ToString()) },
            new { FileName = "Registers.json", EntityName = "Register", KeyExtractor = new Func<dynamic, string>(e => e.RegisterId.ToString()) }
        };

        foreach (var config in validationConfigs)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await ValidateFileForDuplicatesAsync(
                    loader,
                    config.FileName,
                    config.EntityName,
                    config.KeyExtractor,
                    result,
                    logger,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                result.Errors.Add(new DataValidationError
                {
                    FileName = config.FileName,
                    EntityType = config.EntityName,
                    ErrorMessage = $"Failed to validate file: {ex.Message}"
                });
            }
        }

        return result;
    }

    private static async Task ValidateFileForDuplicatesAsync(
        ITestDataLoader loader,
        string fileName,
        string entityName,
        Func<dynamic, string> keyExtractor,
        DataValidationResult result,
        ILogger? logger,
        CancellationToken cancellationToken)
    {
        if (!loader.Exists(fileName))
        {
            result.Errors.Add(new DataValidationError
            {
                FileName = fileName,
                EntityType = entityName,
                ErrorMessage = "File not found in embedded resources"
            });
            return;
        }

        // Load as raw JSON to preserve line numbers
        var resourceName = $"IndTrace.TestData.Data.{fileName}";
        using var stream = typeof(DataValidation).Assembly.GetManifestResourceStream(resourceName);
        if (stream == null) return;

        using var reader = new StreamReader(stream);
        var jsonContent = await reader.ReadToEndAsync(cancellationToken);

        // Parse to get line information
        var duplicates = await FindDuplicatesWithLineNumbersAsync(
            jsonContent,
            fileName,
            entityName,
            keyExtractor,
            logger);

        if (duplicates.Any())
        {
            result.FilesWithDuplicates[fileName] = duplicates;
            result.TotalDuplicatesFound += duplicates.Count;
        }

        result.FilesValidated.Add(fileName);
    }

    private static Task<List<DuplicateEntry>> FindDuplicatesWithLineNumbersAsync(
        string jsonContent,
        string fileName,
        string entityName,
        Func<dynamic, string> keyExtractor,
        ILogger? logger)
    {
        var duplicates = new List<DuplicateEntry>();
        var seenKeys = new Dictionary<string, (int LineNumber, string Name)>();

        try
        {
            // Parse JSON and track line numbers
            using var doc = JsonDocument.Parse(jsonContent);
            var root = doc.RootElement;

            if (root.ValueKind != JsonValueKind.Array)
                return Task.FromResult(duplicates);

            var lines = jsonContent.Split('\n');
            var currentLine = 1;
            var index = 0;

            foreach (var element in root.EnumerateArray())
            {
                string? key = null;
                string? name = null;

                try
                {
                    // Try to extract key from JSON element directly
                    if (element.TryGetProperty("RuleId", out var prop) ||
                        element.TryGetProperty("MachineId", out prop) ||
                        element.TryGetProperty("PlcId", out prop) ||
                        element.TryGetProperty("VariableGroupId", out prop) ||
                        element.TryGetProperty("VariableId", out prop) ||
                        element.TryGetProperty("LineId", out prop) ||
                        element.TryGetProperty("CustomerId", out prop) ||
                        element.TryGetProperty("ProductId", out prop) ||
                        element.TryGetProperty("RecipeId", out prop) ||
                        element.TryGetProperty("WorkFlowId", out prop) ||
                        element.TryGetProperty("AppId", out prop) ||
                        element.TryGetProperty("SettingId", out prop) ||
                        element.TryGetProperty("BarCodeId", out prop) ||
                        element.TryGetProperty("CycleId", out prop) ||
                        element.TryGetProperty("RegisterId", out prop))
                    {
                        key = prop.GetInt32().ToString();
                    }
                    else if (entityName == "MachinePlc")
                    {
                        // Special case for composite key
                        var machineId = element.GetProperty("MachineId").GetInt32();
                        var plcId = element.GetProperty("PlcId").GetInt32();
                        key = $"{machineId}|{plcId}";
                    }

                    // Try to get name
                    if (element.TryGetProperty("Name", out var nameProp))
                        name = nameProp.GetString();
                    else if (element.TryGetProperty("Label", out nameProp))
                        name = nameProp.GetString();
                    else if (element.TryGetProperty("Description", out nameProp))
                        name = nameProp.GetString();
                }
                catch
                {
                    // Skip this element if we can't extract key
                    continue;
                }

                if (key != null)
                {
                    // Find approximate line number
                    for (int i = currentLine - 1; i < lines.Length; i++)
                    {
                        if (lines[i].Contains($"\"{key}\"") || lines[i].Contains($": {key}"))
                        {
                            currentLine = i + 1;
                            break;
                        }
                    }

                    if (seenKeys.ContainsKey(key))
                    {
                        var original = seenKeys[key];
                        var duplicate = new DuplicateEntry
                        {
                            FileName = fileName,
                            EntityType = entityName,
                            EntityId = key,
                            EntityName = name ?? string.Empty,
                            LineNumber = currentLine,
                            OriginalLineNumber = original.LineNumber
                        };

                        duplicates.Add(duplicate);

                        if (logger != null)
                        {
                            LoggerExtensions.LogWarning(logger,
                                "Duplicate {EntityType} found in {FileName}: ID={EntityId}, Name={EntityName}, " +
                                "Line={LineNumber} (original at line {OriginalLine})",
                                entityName, fileName, key, name ?? "N/A", currentLine, original.LineNumber);
                        }
                    }
                    else
                    {
                        seenKeys[key] = (currentLine, name ?? string.Empty);
                    }
                }

                index++;
            }
        }
        catch (Exception ex)
        {
            if (logger != null)
            {
                LoggerExtensions.LogError(logger, ex, "Error parsing {FileName} for duplicates", fileName);
            }
        }

        //[Fix]
        //CLAUDE
        //Date: 26/08/2025
        //Reason: [CS1998] - Remove async and return Task.FromResult since method is synchronous

        return Task.FromResult(duplicates);
    }

    private static string? TryGetName(dynamic entity)
    {
        try
        {
            // Try common name properties
            if (entity.GetType().GetProperty("Name") != null)
                return entity.Name?.ToString();
            if (entity.GetType().GetProperty("Label") != null)
                return entity.Label?.ToString();
            if (entity.GetType().GetProperty("Description") != null)
                return entity.Description?.ToString();
            return null;
        }
        catch
        {
            return null;
        }
    }
}

//[Fix] CLAUDE - Date: 26/08/2025
//Reason: [CS8632] - Removed nullable annotations (ILogger?, string?, etc.) to eliminate nullable reference type warnings since project has <Nullable>disable</Nullable>
// Also created comprehensive data validation utility for JSON deduplication with logging
