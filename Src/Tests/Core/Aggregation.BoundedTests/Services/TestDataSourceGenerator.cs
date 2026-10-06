// <copyright file="TestDataSourceGenerator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.Services;

/// <summary>
/// Generates static C# classes from test data usage logs for optimal performance.
/// </summary>
public class TestDataSourceGenerator
{
    private readonly string _outputDirectory;
    private readonly string _namespace;

    /// <summary>
    /// Initializes a new instance of the test data source generator.
    /// </summary>
    /// <param name="outputDirectory">Directory to output generated files.</param>
    /// <param name="namespace">Namespace for generated classes.</param>
    public TestDataSourceGenerator(string outputDirectory = "Generated", string @namespace = "IndTrace.Application.UnitTests.TestData.Generated")
    {
        _outputDirectory = outputDirectory;
        _namespace = @namespace;
    }

    /// <summary>
    /// Generates static C# classes from usage logs.
    /// </summary>
    /// <param name="usageLog">The usage log to generate from.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Generation result with file paths and statistics.</returns>
    public async Task<SourceGenerationResult> GenerateFromUsageLogAsync(
        IReadOnlyDictionary<string, HashSet<string>> usageLog,
        CancellationToken cancellationToken = default)
    {
        var result = new SourceGenerationResult
        {
            GeneratedFiles = [],
            TotalEntities = 0,
            GeneratedAt = DateTime.UtcNow
        };

        try
        {
            // Ensure output directory exists
            Directory.CreateDirectory(_outputDirectory);

            // Generate a class for each entity type
            foreach (var (entityType, usedIds) in usageLog)
            {
                if (usedIds.Count == 0) continue;

                var fileName = $"{entityType}GeneratedData.cs";
                var filePath = Path.Combine(_outputDirectory, fileName);

                var classContent = GenerateClassContent(entityType, usedIds);
                await File.WriteAllTextAsync(filePath, classContent, cancellationToken);

                result.GeneratedFiles.Add(filePath);
                result.TotalEntities += usedIds.Count;
            }

            // Generate a summary file
            var summaryContent = GenerateSummaryContent(result);
            var summaryPath = Path.Combine(_outputDirectory, "GeneratedDataSummary.cs");
            await File.WriteAllTextAsync(summaryPath, summaryContent, cancellationToken);

            result.GeneratedFiles.Add(summaryPath);
            result.IsSuccess = true;

            return result;
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            return result;
        }
    }

    /// <summary>
    /// Generates static C# classes from a usage report file.
    /// </summary>
    /// <param name="reportFilePath">Path to the usage report JSON file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Generation result with file paths and statistics.</returns>
    public async Task<SourceGenerationResult> GenerateFromReportFileAsync(
        string reportFilePath,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(reportFilePath))
        {
            return new SourceGenerationResult
            {
                IsSuccess = false,
                ErrorMessage = $"Usage report file not found: {reportFilePath}"
            };
        }

        try
        {
            var jsonContent = await File.ReadAllTextAsync(reportFilePath, cancellationToken);
            var report = JsonSerializer.Deserialize<TestDataUsageReport>(jsonContent);

            if (report == null)
            {
                return new SourceGenerationResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Failed to deserialize usage report"
                };
            }

            // Convert report to usage log format
            var usageLog = new Dictionary<string, HashSet<string>>();

            if (report.AccessedRegisterIds.Count > 0)
            {
                usageLog["Register"] = new HashSet<string>(report.AccessedRegisterIds.Select(id => id.ToString()));
            }

            if (report.AccessedBarCodeIds.Count > 0)
            {
                usageLog["BarCode"] = new HashSet<string>(report.AccessedBarCodeIds.Select(id => id.ToString()));
            }

            if (report.AccessedCycleIds.Count > 0)
            {
                usageLog["Cycle"] = new HashSet<string>(report.AccessedCycleIds.Select(id => id.ToString()));
            }

            if (report.AccessedMachineIds.Count > 0)
            {
                usageLog["Machine"] = new HashSet<string>(report.AccessedMachineIds.Select(id => id.ToString()));
            }

            return await GenerateFromUsageLogAsync(usageLog, cancellationToken);
        }
        catch (Exception ex)
        {
            return new SourceGenerationResult
            {
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private string GenerateClassContent(string entityType, HashSet<string> usedIds)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine($"namespace {_namespace};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Auto-generated test data for {entityType} entities based on usage analysis.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public static class {entityType}GeneratedData");
        sb.AppendLine("{");

        // Generate used IDs array
        sb.AppendLine($"    /// <summary>");
        sb.AppendLine($"    /// IDs of {entityType} entities that were actually used in tests.");
        sb.AppendLine($"    /// Generated from usage analysis on {DateTime.Now:yyyy-MM-dd HH:mm:ss}.");
        sb.AppendLine($"    /// </summary>");
        sb.AppendLine($"    public static readonly int[] UsedIds = new[]");
        sb.AppendLine("    {");

        foreach (var id in usedIds.OrderBy(id => int.TryParse(id, out var num) ? num : 0))
        {
            sb.AppendLine($"        {id},");
        }

        sb.AppendLine("    };");
        sb.AppendLine();

        // Generate count property
        sb.AppendLine($"    /// <summary>");
        sb.AppendLine($"    /// Number of {entityType} entities used in tests.");
        sb.AppendLine($"    /// </summary>");
        sb.AppendLine($"    public static int Count => UsedIds.Length;");
        sb.AppendLine();

        // Generate sample data method
        sb.AppendLine($"    /// <summary>");
        sb.AppendLine($"    /// Gets sample {entityType} data for testing.");
        sb.AppendLine($"    /// </summary>");
        sb.AppendLine($"    public static string GetSampleData()");
        sb.AppendLine("    {");
        sb.AppendLine("        return \"\"\"");
        sb.AppendLine("        [");

        var sampleCount = Math.Min(usedIds.Count, 5); // Limit to 5 samples
        var sampleIds = usedIds.Take(sampleCount).ToList();

        for (int i = 0; i < sampleCount; i++)
        {
            var id = sampleIds[i];
            sb.AppendLine($"            {{");
            sb.AppendLine($"                \"id\": {id},");
            sb.AppendLine($"                \"name\": \"{entityType}-{id}\",");
            sb.AppendLine($"                \"description\": \"Auto-generated sample data for {entityType} {id}\"");
            sb.AppendLine($"            }}{(i < sampleCount - 1 ? "," : "")}");
        }

        sb.AppendLine("        ]");
        sb.AppendLine("        \"\"\";");
        sb.AppendLine("    }");

        sb.AppendLine("}");

        return sb.ToString();
    }

    private string GenerateSummaryContent(SourceGenerationResult result)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"namespace {_namespace};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Summary of auto-generated test data files.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("public static class GeneratedDataSummary");
        sb.AppendLine("{");
        sb.AppendLine($"    /// <summary>");
        sb.AppendLine($"    /// Total number of entities generated: {result.TotalEntities}");
        sb.AppendLine($"    /// </summary>");
        sb.AppendLine($"    public static int TotalEntities => {result.TotalEntities};");
        sb.AppendLine();
        sb.AppendLine($"    /// <summary>");
        sb.AppendLine($"    /// Number of generated files: {result.GeneratedFiles.Count}");
        sb.AppendLine($"    /// </summary>");
        sb.AppendLine($"    public static int GeneratedFilesCount => {result.GeneratedFiles.Count};");
        sb.AppendLine();
        sb.AppendLine($"    /// <summary>");
        sb.AppendLine($"    /// When the data was generated: {result.GeneratedAt:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"    /// </summary>");
        sb.AppendLine($"    public static string GeneratedAt => \"{result.GeneratedAt:yyyy-MM-dd HH:mm:ss}\";");
        sb.AppendLine("}");

        return sb.ToString();
    }
}

/// <summary>
/// Result of source generation process.
/// </summary>
public class SourceGenerationResult
{
    /// <summary>
    /// Whether the generation was successful.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// List of generated file paths.
    /// </summary>
    public List<string> GeneratedFiles { get; set; } = [];

    /// <summary>
    /// Total number of entities generated.
    /// </summary>
    public int TotalEntities { get; set; }

    /// <summary>
    /// When the generation occurred.
    /// </summary>
    public DateTime GeneratedAt { get; set; }

    /// <summary>
    /// Error message if generation failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}