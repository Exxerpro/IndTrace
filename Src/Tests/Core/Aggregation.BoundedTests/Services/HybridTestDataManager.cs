// <copyright file="HybridTestDataManager.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.Services;

/// <summary>
/// Hybrid test data manager that supports multiple data sources for evolving test beds.
/// </summary>
public class HybridTestDataManager
{
    private readonly string _edgeCaseDataPath;
    private readonly string _regressionDataPath;
    private readonly DbContextTests _fullContextTestsData;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public HybridTestDataManager(string edgeCaseDataPath = "TestData/EdgeCases",
                                string regressionDataPath = "TestData/Regression",
                                DbContextTests? fullContextData = null)
    {
        _edgeCaseDataPath = edgeCaseDataPath;
        _regressionDataPath = regressionDataPath;
        _fullContextTestsData = fullContextData ?? new DbContextTests();
    }

    /// <summary>
    /// Gets focused test data from compile-time generated arrays (fastest).
    /// </summary>
    public FocusedTestDataResult GetFocusedTestData()
    {
        return new FocusedTestDataResult
        {
            Registers = Array.Empty<Register>(),
            BarCodes = Array.Empty<BarCode>(),
            Cycles = Array.Empty<Cycle>(),
            Machines = Array.Empty<Machine>(),
            Source = "CompileTime",
            LoadTime = TimeSpan.Zero
        };
    }

    /// <summary>
    /// Loads edge case data from JSON files (comprehensive).
    /// </summary>
    public async Task<EdgeCaseTestDataResult> LoadEdgeCaseDataAsync(string scenario = "default")
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        var filePath = Path.Combine(_edgeCaseDataPath, $"{scenario}.json");

        if (!File.Exists(filePath))
        {
            return new EdgeCaseTestDataResult
            {
                Registers = Array.Empty<Register>(),
                BarCodes = Array.Empty<BarCode>(),
                Cycles = Array.Empty<Cycle>(),
                Machines = Array.Empty<Machine>(),
                Source = "NotFound",
                LoadTime = stopwatch.Elapsed,
                FilePath = filePath
            };
        }

        var json = await File.ReadAllTextAsync(filePath);
        var data = JsonSerializer.Deserialize<EdgeCaseTestData>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        stopwatch.Stop();

        return new EdgeCaseTestDataResult
        {
            Registers = data?.Registers ?? Array.Empty<Register>(),
            BarCodes = data?.BarCodes ?? Array.Empty<BarCode>(),
            Cycles = data?.Cycles ?? Array.Empty<Cycle>(),
            Machines = data?.Machines ?? Array.Empty<Machine>(),
            Source = "JSON",
            LoadTime = stopwatch.Elapsed,
            FilePath = filePath,
            Metadata = data?.Metadata
        };
    }

    /// <summary>
    /// Generates regression test data dynamically (on-demand).
    /// </summary>
    public async Task<RegressionTestDataResult> GenerateRegressionTestDataAsync(RegressionTestScenario scenario)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        var data = await GenerateDataForScenario(scenario);

        stopwatch.Stop();

        return new RegressionTestDataResult
        {
            Registers = data.Registers,
            BarCodes = data.BarCodes,
            Cycles = data.Cycles,
            Machines = data.Machines,
            Source = "Dynamic",
            LoadTime = stopwatch.Elapsed,
            Scenario = scenario,
            GeneratedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Gets the best available test data based on requirements.
    /// </summary>
    public async Task<ITestDataResult> GetTestDataAsync(TestDataRequirements requirements)
    {
        // Priority 1: Use focused data if it meets requirements
        var focusedData = GetFocusedTestData();
        if (MeetsRequirements(focusedData, requirements))
        {
            return focusedData;
        }

        // Priority 2: Try edge case data
        var edgeCaseData = await LoadEdgeCaseDataAsync(requirements.Scenario);
        if (MeetsRequirements(edgeCaseData, requirements))
        {
            return edgeCaseData;
        }

        // Priority 3: Generate dynamic data
        var regressionData = await GenerateRegressionTestDataAsync(new RegressionTestScenario
        {
            Name = requirements.Scenario,
            RegisterCount = requirements.MinRegisters,
            BarCodeCount = requirements.MinBarCodes,
            CycleCount = requirements.MinCycles,
            MachineCount = requirements.MinMachines
        });

        return regressionData;
    }

    /// <summary>
    /// Saves edge case data to JSON for future use.
    /// </summary>
    public async Task SaveEdgeCaseDataAsync(EdgeCaseTestData data, string scenario)
    {
        var directory = Path.GetDirectoryName(_edgeCaseDataPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var filePath = Path.Combine(_edgeCaseDataPath, $"{scenario}.json");
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = true,
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
        });

        await File.WriteAllTextAsync(filePath, json);
    }

    /// <summary>
    /// Analyzes test data usage and suggests optimizations.
    /// </summary>
    public TestDataAnalysisResult AnalyzeTestDataUsage()
    {
        var focusedData = GetFocusedTestData();
        var focusedStats = new TestDataStats
        {
            Registers = focusedData.Registers.Length,
            BarCodes = focusedData.BarCodes.Length,
            Cycles = focusedData.Cycles.Length,
            Machines = focusedData.Machines.Length
        };

        var edgeCaseFiles = Directory.Exists(_edgeCaseDataPath)
            ? Directory.GetFiles(_edgeCaseDataPath, "*.json")
            : Array.Empty<string>();

        var analysis = new TestDataAnalysisResult
        {
            FocusedDataStats = focusedStats,
            EdgeCaseFiles = edgeCaseFiles.Length,
            Recommendations = GenerateRecommendations(focusedStats, edgeCaseFiles.Length)
        };

        return analysis;
    }

    private bool MeetsRequirements(ITestDataResult data, TestDataRequirements requirements)
    {
        return data.Registers.Length >= requirements.MinRegisters &&
               data.BarCodes.Length >= requirements.MinBarCodes &&
               data.Cycles.Length >= requirements.MinCycles &&
               data.Machines.Length >= requirements.MinMachines;
    }

    private async Task<TestData> GenerateDataForScenario(RegressionTestScenario scenario)
    {
        // Create a temporary context to generate data
        var options = new DbContextOptionsBuilder<IndTraceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new IndTraceDbContext(options);

        try
        {
            await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
            await _fullContextTestsData.LoadReferenceDataAsync(context);

            var data = new TestData();

            // Generate registers
            if (scenario.RegisterCount > 0)
            {
                data.Registers = await context.Registers
                    .Take(scenario.RegisterCount)
                    .ToArrayAsync(TestContext.Current.CancellationToken);
            }

            // Generate bar codes
            if (scenario.BarCodeCount > 0)
            {
                data.BarCodes = await context.BarCodes
                    .Take(scenario.BarCodeCount)
                    .ToArrayAsync(TestContext.Current.CancellationToken);
            }

            // Generate cycles
            if (scenario.CycleCount > 0)
            {
                data.Cycles = await context.Cycles
                    .Take(scenario.CycleCount)
                    .ToArrayAsync(TestContext.Current.CancellationToken);
            }

            // Generate machines
            if (scenario.MachineCount > 0)
            {
                data.Machines = await context.Machines
                    .Take(scenario.MachineCount)
                    .ToArrayAsync(TestContext.Current.CancellationToken);
            }

            return data;
        }
        finally
        {
            context.Database.EnsureDeleted();
            context.Dispose();
        }
    }

    private List<string> GenerateRecommendations(TestDataStats focusedStats, int edgeCaseFiles)
    {
        var recommendations = new List<string>();

        if (focusedStats.TotalEntities < 50)
        {
            recommendations.Add("Consider expanding focused test data for better coverage");
        }

        if (edgeCaseFiles == 0)
        {
            recommendations.Add("Create edge case JSON files for complex scenarios");
        }

        if (focusedStats.Registers < 10)
        {
            recommendations.Add("Add more register data to focused test set");
        }

        return recommendations;
    }
}

/// <summary>
/// Requirements for test data.
/// </summary>
public class TestDataRequirements
{
    /// <summary>
    /// Gets or sets the MinRegisters.
    /// </summary>
    public int MinRegisters { get; set; } = 0;

    /// <summary>
    /// Gets or sets the MinBarCodes.
    /// </summary>
    public int MinBarCodes { get; set; } = 0;

    /// <summary>
    /// Gets or sets the MinCycles.
    /// </summary>
    public int MinCycles { get; set; } = 0;

    /// <summary>
    /// Gets or sets the MinMachines.
    /// </summary>
    public int MinMachines { get; set; } = 0;

    /// <summary>
    /// Gets or sets the Scenario.
    /// </summary>
    public string Scenario { get; set; } = "default";

    /// <summary>
    /// Gets or sets the RequireEdgeCases.
    /// </summary>
    public bool RequireEdgeCases { get; set; } = false;
}

/// <summary>
/// Regression test scenario configuration.
/// </summary>
public class RegressionTestScenario
{
    /// <summary>
    /// Gets or sets the Name.
    /// </summary>
    public string Name { get; set; } = "regression";

    /// <summary>
    /// Gets or sets the RegisterCount.
    /// </summary>
    public int RegisterCount { get; set; } = 10;

    /// <summary>
    /// Gets or sets the BarCodeCount.
    /// </summary>
    public int BarCodeCount { get; set; } = 10;

    /// <summary>
    /// Gets or sets the CycleCount.
    /// </summary>
    public int CycleCount { get; set; } = 5;

    /// <summary>
    /// Gets or sets the MachineCount.
    /// </summary>
    public int MachineCount { get; set; } = 5;
}

/// <summary>
/// Edge case test data structure.
/// </summary>
public class EdgeCaseTestData
{
    /// <summary>
    /// Gets or sets the Registers.
    /// </summary>
    public Register[] Registers { get; set; } = Array.Empty<Register>();

    /// <summary>
    /// Gets or sets the BarCodes.
    /// </summary>
    public BarCode[] BarCodes { get; set; } = Array.Empty<BarCode>();

    /// <summary>
    /// Gets or sets the Cycles.
    /// </summary>
    public Cycle[] Cycles { get; set; } = Array.Empty<Cycle>();

    /// <summary>
    /// Gets or sets the Machines.
    /// </summary>
    public Machine[] Machines { get; set; } = Array.Empty<Machine>();

    /// <summary>
    /// Gets or sets the Metadata.
    /// </summary>
    public EdgeCaseMetadata? Metadata { get; set; }
}

/// <summary>
/// Edge case metadata.
/// </summary>
public class EdgeCaseMetadata
{
    /// <summary>
    /// Gets or sets the Description.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// Gets or sets the Tags.
    /// </summary>
    public string[] Tags { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the CreatedAt.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the CreatedBy.
    /// </summary>
    public string CreatedBy { get; set; } = "";
}

/// <summary>
/// Test data structure.
/// </summary>
public class TestData
{
    /// <summary>
    /// Gets or sets the Registers.
    /// </summary>
    public Register[] Registers { get; set; } = Array.Empty<Register>();

    /// <summary>
    /// Gets or sets the BarCodes.
    /// </summary>
    public BarCode[] BarCodes { get; set; } = Array.Empty<BarCode>();

    /// <summary>
    /// Gets or sets the Cycles.
    /// </summary>
    public Cycle[] Cycles { get; set; } = Array.Empty<Cycle>();

    /// <summary>
    /// Gets or sets the Machines.
    /// </summary>
    public Machine[] Machines { get; set; } = Array.Empty<Machine>();
}

/// <summary>
/// Test data statistics.
/// </summary>
public class TestDataStats
{
    /// <summary>
    /// Gets or sets the Registers.
    /// </summary>
    public int Registers { get; set; }

    /// <summary>
    /// Gets or sets the BarCodes.
    /// </summary>
    public int BarCodes { get; set; }

    /// <summary>
    /// Gets or sets the Cycles.
    /// </summary>
    public int Cycles { get; set; }

    /// <summary>
    /// Gets or sets the Machines.
    /// </summary>
    public int Machines { get; set; }

    public int TotalEntities => Registers + BarCodes + Cycles + Machines;
}

/// <summary>
/// Test data analysis result.
/// </summary>
public class TestDataAnalysisResult
{
    /// <summary>
    /// Gets or sets the FocusedDataStats.
    /// </summary>
    public TestDataStats FocusedDataStats { get; set; } = new();

    /// <summary>
    /// Gets or sets the EdgeCaseFiles.
    /// </summary>
    public int EdgeCaseFiles { get; set; }

    /// <summary>
    /// Gets or sets the Recommendations.
    /// </summary>
    public List<string> Recommendations { get; set; } = [];
}

/// <summary>
/// Base interface for test data results.
/// </summary>
public interface ITestDataResult
{
    Register[] Registers { get; }
    BarCode[] BarCodes { get; }
    Cycle[] Cycles { get; }
    Machine[] Machines { get; }
    string Source { get; }
    TimeSpan LoadTime { get; }
}

/// <summary>
/// Focused test data result.
/// </summary>
public class FocusedTestDataResult : ITestDataResult
{
    /// <summary>
    /// Gets or sets the Registers.
    /// </summary>
    public Register[] Registers { get; set; } = Array.Empty<Register>();

    /// <summary>
    /// Gets or sets the BarCodes.
    /// </summary>
    public BarCode[] BarCodes { get; set; } = Array.Empty<BarCode>();

    /// <summary>
    /// Gets or sets the Cycles.
    /// </summary>
    public Cycle[] Cycles { get; set; } = Array.Empty<Cycle>();

    /// <summary>
    /// Gets or sets the Machines.
    /// </summary>
    public Machine[] Machines { get; set; } = Array.Empty<Machine>();

    /// <summary>
    /// Gets or sets the Source.
    /// </summary>
    public string Source { get; set; } = "CompileTime";

    /// <summary>
    /// Gets or sets the LoadTime.
    /// </summary>
    public TimeSpan LoadTime { get; set; } = TimeSpan.Zero;
}

/// <summary>
/// Edge case test data result.
/// </summary>
public class EdgeCaseTestDataResult : ITestDataResult
{
    /// <summary>
    /// Gets or sets the Registers.
    /// </summary>
    public Register[] Registers { get; set; } = Array.Empty<Register>();

    /// <summary>
    /// Gets or sets the BarCodes.
    /// </summary>
    public BarCode[] BarCodes { get; set; } = Array.Empty<BarCode>();

    /// <summary>
    /// Gets or sets the Cycles.
    /// </summary>
    public Cycle[] Cycles { get; set; } = Array.Empty<Cycle>();

    /// <summary>
    /// Gets or sets the Machines.
    /// </summary>
    public Machine[] Machines { get; set; } = Array.Empty<Machine>();

    /// <summary>
    /// Gets or sets the Source.
    /// </summary>
    public string Source { get; set; } = "JSON";

    /// <summary>
    /// Gets or sets the LoadTime.
    /// </summary>
    public TimeSpan LoadTime { get; set; }

    /// <summary>
    /// Gets or sets the FilePath.
    /// </summary>
    public string FilePath { get; set; } = "";

    /// <summary>
    /// Gets or sets the Metadata.
    /// </summary>
    public EdgeCaseMetadata? Metadata { get; set; }
}

/// <summary>
/// Regression test data result.
/// </summary>
public class RegressionTestDataResult : ITestDataResult
{
    /// <summary>
    /// Gets or sets the Registers.
    /// </summary>
    public Register[] Registers { get; set; } = Array.Empty<Register>();

    /// <summary>
    /// Gets or sets the BarCodes.
    /// </summary>
    public BarCode[] BarCodes { get; set; } = Array.Empty<BarCode>();

    /// <summary>
    /// Gets or sets the Cycles.
    /// </summary>
    public Cycle[] Cycles { get; set; } = Array.Empty<Cycle>();

    /// <summary>
    /// Gets or sets the Machines.
    /// </summary>
    public Machine[] Machines { get; set; } = Array.Empty<Machine>();

    /// <summary>
    /// Gets or sets the Source.
    /// </summary>
    public string Source { get; set; } = "Dynamic";

    /// <summary>
    /// Gets or sets the LoadTime.
    /// </summary>
    public TimeSpan LoadTime { get; set; }

    /// <summary>
    /// Gets or sets the Scenario.
    /// </summary>
    public RegressionTestScenario Scenario { get; set; } = new();

    /// <summary>
    /// Gets or sets the GeneratedAt.
    /// </summary>
    public DateTime GeneratedAt { get; set; }
}