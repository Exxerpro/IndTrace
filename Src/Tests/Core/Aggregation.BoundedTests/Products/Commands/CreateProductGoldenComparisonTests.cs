// <copyright file="CreateProductGoldenComparisonTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

// Removed - now in GlobalUsings.cs:
// using IndTrace.Application.Products.Commands.Create;
// using IndTrace.Application.Products.Services;
// using IndTrace.Application.Products.Services.Interfaces;
using IndTrace.Application.Products.Services;

// Using Application layer ProductCreatedEvent (via global usings)
using IndTrace.Application.RulesEngine.Dto;
using IndTrace.Application.WorkFlows.Dto;
using IndTrace.Application.Repositories;
using IndTrace.Aggregation.BoundedTests.Helpers;

namespace IndTrace.Aggregation.BoundedTests.Products.Commands;

/// <summary>
/// Behavioral verification tests for SRP-refactored CreateProductCommandHandler.
/// Validates that the handler produces expected results for all scenarios.
/// CRITICAL for ensuring SRP refactoring maintains business logic correctness.
/// </summary>
public class CreateProductBehaviorVerificationTests : DependenciesFactory
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public CreateProductBehaviorVerificationTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        _outputHelper = outputHelper;
    }

    /// <summary>
    /// CRITICAL: Verifies SRP handler behavior for all business scenarios.
    /// Ensures the refactored handler maintains correct business logic.
    /// Updated to use real TestData: CustomerId=1="Volkswagen", CustomerId=2="Oemx", CustomerId=3="BMW"
    /// </summary>
    [Theory]
    [InlineData("NEW-VW-X1-001", 1, 1, "Volkswagen", "Success_Valid_Product_With_ID_Parsing", true)]
    [InlineData("NEW-OEMX-A4-ABC", 2, 1, "Oemx", "Success_Valid_Product_No_ID_Parsing", true)]
    [InlineData("NEW-BMW-X5-999999", 3, 1, "BMW", "Success_Large_Number_ID_Parsing", true)]
    [InlineData("NEW-VW-X1-RESOLVE", 999, 1, "Volkswagen", "Success_Customer_Resolved_By_Name", true)] // CustomerName resolution despite invalid CustomerId
    [InlineData("DEFAULT", 1, 1, "Test Customer", "Failure_Product_Already_Exists", false)]
    [InlineData("NEW-VW-X1-002", 0, 1, "Volkswagen", "Failure_Invalid_Customer_ID", false)]
    [InlineData("NEW-VW-X1-003", 1, 0, "Volkswagen", "Failure_Invalid_Line_ID", false)]
    [InlineData("NEW-VW-X1-004", 999, 1, "NonExistentCustomer", "Failure_Customer_Not_Found", false)] // Invalid CustomerId + Invalid CustomerName should fail
    [InlineData("NEW-VW-X1-005", 1, 983, "Volkswagen", "Failure_Line_Not_Found", false)]
    [InlineData("NEW-VW-X1-006", 1, 1, "tsla", "Failure_Customer_Name_Lookup_Failed", false)]
    public async Task VerifyHandler_SRPBehavior_ShouldProduceExpectedResults(
        string partNumber, int customerId, int lineId, string customerName, string scenario, bool expectedSuccess)
    {
        // Arrange
        await Initialization;

        // C2 Chunk E11.2: authoring requires a post-D2 (magic-0-free) database; CreateProduct's workflow step
        // authors the C2-clean shape and the F3 sentinel refuses while any magic-0 row remains. Each test owns
        // its own database, so clearing here does not disturb the legacy magic-0 read fixtures elsewhere.
        await ClearMagicZeroWorkFlowsAsync(TestContext.Current.CancellationToken);
        var fixedTimestamp = new DateTime(2025, 9, 21, 14, 0, 0, DateTimeKind.Local);

        var command = CreateTestCommand(partNumber, customerId, lineId, customerName, fixedTimestamp);

        // Act - Run SRP handler through the standard dispatcher
        var result = await DpMonitorRequestDispatcher.ProcessAsync(command, CancellationToken.None);

        // Log detailed results for analysis
        Logger.LogInformation("=== SRP HANDLER VERIFICATION - Scenario: {0} ===", scenario);
        Logger.LogInformation("Input: PartNumber={0}, CustomerId={1}, LineId={2}, CustomerName={3}",
          partNumber, customerId, lineId, customerName);
        Logger.LogInformation("Expected Success: {0}", expectedSuccess);
        Logger.LogInformation("Actual Success: {0}", result.IsSuccess);
        
        if (result.Errors?.Any() == true)
        {
            Logger.LogInformation("Errors: {0}", string.Join("; ", result.Errors));
        }

        // Assert - Verify expected behavior
        result.IsSuccess.ShouldBe(expectedSuccess, 
            $"SRP handler behavior mismatch for scenario: {scenario}. " +
            $"Expected Success: {expectedSuccess}, Actual: {result.IsSuccess}. " +
            $"Errors: {string.Join("; ", result.Errors ?? [])}");

        // Additional verification for success cases
        if (expectedSuccess)
        {
            result.Value.ShouldNotBeNull("Successful operations should return a ProductCreatedEvent");
            result.Errors.ShouldBeEmpty("Successful operations should have no errors");
            
            // Verify the product was created with correct data
            result.Value.PartNumber.ShouldNotBeNull("ProductCreatedEvent should have PartNumber");
            
            // For customer resolution scenarios, verify the resolved CustomerId (not the input CustomerId)
            if (scenario == "Success_Customer_Resolved_By_Name")
            {
                // System correctly resolves "Volkswagen" to CustomerId=1, regardless of invalid input CustomerId=999
                result.Value.CustomerId.ShouldBe(1, "Product should have resolved CustomerId for Volkswagen");
            }
            else
            {
                result.Value.CustomerId.ShouldBe(customerId, "Product should have correct CustomerId");
            }
        }
        else
        {
            result.Errors.ShouldNotBeEmpty("Failed operations should have error messages");
        }
    }

    /// <summary>
    /// Performance verification for SRP handler.
    /// Handler should complete operations within acceptable time limits.
    /// </summary>
    [Fact]
    public async Task VerifyPerformance_SRPHandler_ShouldMeetTimeRequirements()
    {
        // Arrange
        await Initialization;

        // C2 Chunk E11.2: authoring requires a post-D2 (magic-0-free) database; CreateProduct's workflow step
        // authors the C2-clean shape and the F3 sentinel refuses while any magic-0 row remains. Each test owns
        // its own database, so clearing here does not disturb the legacy magic-0 read fixtures elsewhere.
        await ClearMagicZeroWorkFlowsAsync(TestContext.Current.CancellationToken);
        const int iterations = 3;
        var executionTimes = new List<long>();

        // Warm-up
        var warmupCommand = CreateTestCommand("WARMUP-PRODUCT", 1, 1, "Volkswagen", DateTime.Now);
        await DpMonitorRequestDispatcher.ProcessAsync(warmupCommand, CancellationToken.None);

        // Act - Measure SRP handler performance
        for (int i = 0; i < iterations; i++)
        {
            var sw = Stopwatch.StartNew();
            var command = CreateTestCommand($"PERF-TEST-{i}", 1, 1, "Volkswagen", DateTime.Now);
            var result = await DpMonitorRequestDispatcher.ProcessAsync(command, CancellationToken.None);
            sw.Stop();

            if (result.IsSuccess)
            {
                executionTimes.Add(sw.ElapsedMilliseconds);
            }
        }

        // Calculate performance statistics
        var averageMs = executionTimes.Average();
        var maxMs = executionTimes.Max();
        
        // Assert - Performance should meet requirements (E2E < 500ms from CLAUDE.md)
        ((double)averageMs).ShouldBeLessThan(500.0, 
            $"SRP handler average execution time ({averageMs:F1}ms) exceeds 500ms requirement");
        
        ((double)maxMs).ShouldBeLessThan(1000.0, 
            $"SRP handler maximum execution time ({maxMs:F1}ms) exceeds 1000ms acceptable limit");
        
        executionTimes.Count.ShouldBe(iterations, "All test iterations should complete successfully");
        
        // Log performance results
        Logger.LogInformation("SRP Handler Performance: Average={0:F1}ms, Max={1:F1}ms, Iterations={2}", 
            averageMs, maxMs, executionTimes.Count);
    }

    #region Handler Execution Methods

    /// <summary>
    /// Runs the refactored handler by constructing it with all SRP services.
    /// This simulates how the refactored handler would be used in production.
    /// </summary>
    private async Task<Result<ProductCreatedEvent>> RunRefactoredHandler(
        CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            // Convert command to ProductInput format expected by refactored handler
            var productInput = ConvertCommandToProductInput(command);

            // Use the service factory to create the refactored handler
            var factory = new CreateProductServicesFactory(this, _outputHelper);
            var refactoredHandler = factory.CreateRefactoredHandler();

            // Execute refactored handler
            return await refactoredHandler.ProcessAsync(command, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<ProductCreatedEvent>.WithFailure($"Exception in refactored handler: {ex.Message}");
        }
    }

    /// <summary>
    /// Converts CreateProductCommand to ProductInput format for refactored handler.
    /// </summary>
    private ProductInput ConvertCommandToProductInput(CreateProductCommand command)
    {
        return new ProductInput
        {
            PartNumber = command.Product.PartNumber ?? string.Empty,
            ProductName = command.Product.ProductName ?? string.Empty,
            Description = command.Product.Description ?? string.Empty,
            CustomerId = command.Product.CustomerId,
            CustomerName = command.Product.CustomerName ?? string.Empty,
            LineId = command.Product.LineId,
            CustomerPartNumber = command.Product.CustomerPartNumber ?? string.Empty,
            AliasPartNumber = command.Product.AliasPartNumber ?? string.Empty,
            IsActive = command.Product.IsActive,
            Version = command.Product.Version,
            CreatedBy = "TEST_USER" // Default for tests
        };
    }

    private async Task<Result<ProductCreatedEvent>> CreateValidProduct_Original()
    {
        var command = CreateTestCommand("FORD-F150-PERF-ORIG", 1, 1, "Ford Motor", DateTime.Now);
        return await DpMonitorRequestDispatcher.ProcessAsync(command, CancellationToken.None);
    }

    private async Task<Result<ProductCreatedEvent>> CreateValidProduct_Refactored()
    {
        var command = CreateTestCommand("FORD-F150-PERF-REF", 1, 1, "Ford Motor", DateTime.Now);
        return await RunRefactoredHandler(command, CancellationToken.None);
    }

    #endregion Handler Execution Methods

    #region Comparison Logic

    /// <summary>
    /// Compares original and refactored handler results for equivalence.
    /// Returns true if results are behaviorally identical.
    /// </summary>
    private bool CompareResults(
        Result<ProductCreatedEvent> originalResult,
        Result<ProductCreatedEvent> refactoredResult)
    {
        // Compare success/failure status
        if (originalResult.IsSuccess != refactoredResult.IsSuccess)
        {
            return false;
        }

        // If both failed, compare error messages
        if (originalResult.IsFailure && refactoredResult.IsFailure)
        {
            var originalErrors = originalResult.Errors ?? [];
            var refactoredErrors = refactoredResult.Errors ?? [];

            // Must have same error count and same error messages
            return originalErrors.Count() == refactoredErrors.Count() &&
                   originalErrors.All(error => refactoredErrors.Contains(error));
        }

        // If both succeeded, compare core product data
        if (originalResult.IsSuccess && refactoredResult.IsSuccess)
        {
            var originalEvent = originalResult.Value;
            var refactoredEvent = refactoredResult.Value;

            if (originalEvent == null || refactoredEvent == null)
            {
                return originalEvent == null && refactoredEvent == null;
            }

            // Compare core product properties
            return originalEvent.ProductId == refactoredEvent.ProductId &&
                   originalEvent.PartNumber == refactoredEvent.PartNumber &&
                   originalEvent.Name == refactoredEvent.Name &&
                   originalEvent.CustomerId == refactoredEvent.CustomerId;
        }

        return false;
    }

    /// <summary>
    /// Creates a summary of handler result for comparison analysis.
    /// </summary>
    private object CreateResultSummary(Result<ProductCreatedEvent> result)
    {
        return new
        {
            IsSuccess = result.IsSuccess,
            Errors = result.IsFailure ? result.Errors?.ToList() : null,
            ProductId = result.IsSuccess ? result.Value?.ProductId : 0,
            PartNumber = result.IsSuccess ? result.Value?.PartNumber : null
        };
    }

    #endregion Comparison Logic

    #region Helper Methods

    private CreateProductCommand CreateTestCommand(
        string partNumber, int customerId, int lineId, string customerName, DateTime timestamp)
    {
        // Add timestamp suffix to ensure unique product names and avoid state pollution
        // Use shorter format to prevent int32 overflow: max 6 digits (999999 < int.MaxValue)
        var uniquePartNumber = $"{partNumber}-{timestamp:HHmmss}";
        
        var productCreationDto = new ProductCreationDto
        {
            Product = new ProductDto
            {
                PartNumber = uniquePartNumber,
                ProductName = $"Product {uniquePartNumber}",
                CustomerId = customerId,
                CustomerName = customerName,
                LineId = lineId,
                Description = $"Test product for {uniquePartNumber}",
                Customer = new Customer
                {
                    CustomerId = customerId,
                    Name = customerName
                }
            },
            Machines = new List<int> { 100 }, // Default test machine
            Rule = new RuleDto
            {
                Name = $"Rule for {uniquePartNumber}",
                Description = "Test rule",
                RuleJson = "test rule json"
            },
            Recipe = new RecipeDto
            {
                ProductId = 0, // Will be set after product creation
                MachineId = 100, // Default test machine
                CycleTimeMinimum = 5000,
                CycleTimeMaximum = 15000
            }
        };

        return new CreateProductCommand(productCreationDto);
    }

    private ILogger<T> GetLogger<T>()
    {
        // Create a simple logger for testing
        var factory = new LoggerFactory();
        return factory.CreateLogger<T>();
    }

    #endregion Helper Methods

    #region Data Models

    private class HandlerComparison
    {
        public string Scenario { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public object Input { get; set; } = new();
        public object OriginalResult { get; set; } = new();
        public object RefactoredResult { get; set; } = new();
        public bool AreIdentical { get; set; }
    }

    #endregion Data Models
}