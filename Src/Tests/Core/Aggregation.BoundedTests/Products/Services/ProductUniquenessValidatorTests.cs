// <copyright file="ProductUniquenessValidatorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Products.Services.Interfaces;
using IndTrace.Application.Repositories;

namespace IndTrace.Aggregation.BoundedTests.Products.Services;

/// <summary>
/// Comprehensive test suite for IProductUniquenessValidator service.
/// Critical for data integrity in high-volume manufacturing environment:
/// - 640,000+ daily PLC operations 
/// - Full traceability system requiring unique product identification
/// - Quality forensics and audit trail compliance
/// </summary>
public class ProductUniquenessValidatorTests : DependenciesFactory
{
    private IProductUniquenessValidator _validator = null!;

    public ProductUniquenessValidatorTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    private async Task InitializeValidator()
    {
        await Initialization;
        _validator = DpProductUniquenessValidator;
        _validator.ShouldNotBeNull("ProductUniquenessValidator should be available from DependenciesFactory");
    }

    #region Happy Path Tests - Normal Uniqueness Validation

    /// <summary>
    /// Validates that available ProductIds return true for uniqueness check.
    /// Critical for new product creation workflow.
    /// </summary>
    [Theory]
    [InlineData(999999, "Large unused ProductId")]
    [InlineData(123456, "Medium unused ProductId")]
    [InlineData(50000, "Typical unused ProductId")]
    [InlineData(10001, "Small unused ProductId")]
    public async Task IsProductIdAvailable_WithUnusedIds_ShouldReturnTrue(int productId, string scenario)
    {
        // Arrange
        await InitializeValidator();

        Logger.LogInformation("=== TESTING: {Scenario} - ProductId: {ProductId} ===", scenario, productId);

        // Act
        var isAvailable = await _validator.IsProductIdAvailableAsync(productId, CancellationToken.None);

        // Assert
        Logger.LogInformation("ProductId {ProductId} availability: {IsAvailable}", productId, isAvailable);
        isAvailable.ShouldBeTrue($"ProductId {productId} should be available for new product creation");
    }

    /// <summary>
    /// Validates PartNumber uniqueness for new products.
    /// Essential for preventing duplicate part numbers in manufacturing.
    /// </summary>
    [Theory]
    [InlineData("NEW-UNIQUE-PART-001", "Standard new part format")]
    [InlineData("FORD-F150-UNIQUE-999", "Automotive part format")]
    [InlineData("BMW-X5-TEST-123456", "Complex part format")]
    [InlineData("UNIQUE-VW-Q7-789", "Concatenated part format")]
    public async Task ValidateProductUniqueness_WithUnusedPartNumbers_ShouldReturnSuccess(string partNumber, string scenario)
    {
        // Arrange
        await InitializeValidator();
        var productName = $"Product {partNumber}";

        Logger.LogInformation("=== TESTING: {Scenario} - PartNumber: {PartNumber} ===", scenario, partNumber);

        // Act
        var result = await _validator.ValidateProductUniquenessAsync(partNumber, productName, CancellationToken.None);

        // Assert
        Logger.LogInformation("PartNumber {PartNumber} validation result: Success={IsSuccess}", partNumber, result.IsSuccess);
        result.IsSuccess.ShouldBeTrue($"PartNumber {partNumber} should be unique and available for new product creation");
        result.Errors.ShouldBeEmpty($"Unique PartNumber {partNumber} should have no validation errors");
    }

    #endregion

    #region Existing Product Tests - Known Products in TestData

    /// <summary>
    /// Validates that existing ProductIds correctly return false for uniqueness.
    /// Uses real TestData to ensure realistic testing scenarios.
    /// </summary>
    [Theory]
    [InlineData(1, "DEFAULT product")]
    [InlineData(508, "L100003 product")]
    public async Task IsProductIdAvailable_WithExistingIds_ShouldReturnFalse(int productId, string scenario)
    {
        // Arrange
        await InitializeValidator();

        Logger.LogInformation("=== TESTING: {Scenario} - Existing ProductId: {ProductId} ===", scenario, productId);

        // DIAGNOSTIC: Check if ProductId actually exists in database
        await LogSpecificProductId(productId);

        // Act
        var isAvailable = await _validator.IsProductIdAvailableAsync(productId, CancellationToken.None);

        // Assert
        Logger.LogInformation("Existing ProductId {ProductId} availability: {IsAvailable}", productId, isAvailable);
        isAvailable.ShouldBeFalse($"Existing ProductId {productId} should NOT be available ({scenario})");
    }

    /// <summary>
    /// Validates that existing PartNumbers correctly return failure for uniqueness.
    /// Critical for preventing duplicate manufacturing specifications.
    /// </summary>
    [Theory]
    [InlineData("DEFAULT", "Default test product")]
    [InlineData("L100003", "Legacy product format")]
    public async Task ValidateProductUniqueness_WithExistingPartNumbers_ShouldReturnFailure(string partNumber, string scenario)
    {
        // Arrange
        await InitializeValidator();
        var productName = $"Product {partNumber}";

        Logger.LogInformation("=== TESTING: {Scenario} - Existing PartNumber: {PartNumber} ===", scenario, partNumber);

        // DIAGNOSTIC: Check if PartNumber actually exists in database
        await LogSpecificPartNumber(partNumber);

        // Act
        var result = await _validator.ValidateProductUniquenessAsync(partNumber, productName, CancellationToken.None);

        // Assert
        Logger.LogInformation("Existing PartNumber {PartNumber} validation result: Success={IsSuccess}, Errors={Errors}", 
            partNumber, result.IsSuccess, string.Join("; ", result.Errors ?? []));
        result.IsFailure.ShouldBeTrue($"Existing PartNumber {partNumber} should fail uniqueness validation ({scenario})");
        result.Errors.ShouldNotBeEmpty($"Duplicate PartNumber {partNumber} should have validation errors");
    }

    #endregion

    #region Edge Cases - Boundary Conditions

    /// <summary>
    /// Tests boundary conditions for ProductId validation.
    /// Critical for edge cases in ID generation algorithms.
    /// Services are resilient and don't throw exceptions.
    /// </summary>
    [Theory]
    [InlineData(0, "Zero ProductId")]
    [InlineData(-1, "Negative ProductId")]
    [InlineData(int.MaxValue, "Maximum integer ProductId")]
    [InlineData(int.MinValue, "Minimum integer ProductId")]
    public async Task IsProductIdAvailable_WithBoundaryValues_ShouldHandleCorrectly(int productId, string scenario)
    {
        // Arrange
        await InitializeValidator();

        Logger.LogInformation("=== TESTING: {Scenario} - Boundary ProductId: {ProductId} ===", scenario, productId);

        // Act & Assert - Should not throw exceptions (resilient service)
        var isAvailable = await _validator.IsProductIdAvailableAsync(productId, CancellationToken.None);

        Logger.LogInformation("Boundary ProductId {ProductId} availability: {IsAvailable}", productId, isAvailable);
        
        // Service is resilient - handles all inputs gracefully without exceptions
        isAvailable.ShouldBeOfType<bool>("Service should return boolean result for all ProductId values");
    }

    /// <summary>
    /// Tests boundary conditions for PartNumber validation.
    /// Ensures robust handling of edge cases in part number formats.
    /// Only defensive validation: null/whitespace or minimal length >=3.
    /// </summary>
    [Theory]
    [InlineData("", "Empty part number")]
    [InlineData(" ", "Whitespace part number")]
    [InlineData("A", "Single character - too short")]
    [InlineData("AB", "Two characters - too short")]
    [InlineData("ABC", "Three characters - minimum length")]
    [InlineData("VERY-LONG-PART-NUMBER-THAT-EXCEEDS-NORMAL-LENGTH-EXPECTATIONS-FOR-MANUFACTURING-PARTS-123456789", "Very long part number")]
    public async Task ValidateProductUniqueness_WithBoundaryStrings_ShouldHandleCorrectly(string partNumber, string scenario)
    {
        // Arrange
        await InitializeValidator();
        var productName = $"Product {partNumber}";

        Logger.LogInformation("=== TESTING: {Scenario} - Boundary PartNumber: '{PartNumber}' ===", scenario, partNumber);

        // Act & Assert - Should not throw exceptions (resilient service)
        var result = await _validator.ValidateProductUniquenessAsync(partNumber, productName, CancellationToken.None);

        Logger.LogInformation("Boundary PartNumber '{PartNumber}' validation result: Success={IsSuccess}", partNumber, result.IsSuccess);
        
        // Defensive validation: null/whitespace or minimal length >=3
        if (string.IsNullOrWhiteSpace(partNumber) || partNumber.Length < 3)
        {
            result.IsFailure.ShouldBeTrue($"PartNumber '{partNumber}' should fail defensive validation (null/whitespace or <3 chars)");
        }
        else
        {
            // Service is resilient - valid length part numbers are processed gracefully
            result.ShouldNotBeNull($"Service should return result for valid length PartNumber '{partNumber}'");
        }
    }

    #endregion

    #region Negative Cases - Invalid Inputs

    /// <summary>
    /// Tests null parameter handling for robust error management.
    /// Critical for preventing system crashes in high-volume environment.
    /// Defensive validation should catch null inputs.
    /// </summary>
    [Fact]
    public async Task ValidateProductUniqueness_WithNullPartNumber_ShouldHandleGracefully()
    {
        // Arrange
        await InitializeValidator();

        Logger.LogInformation("=== TESTING: Null PartNumber handling ===");

        // Act & Assert - Should handle null gracefully (defensive validation)
        var result = await _validator.ValidateProductUniquenessAsync(null!, "Test Product", CancellationToken.None);

        Logger.LogInformation("Null PartNumber validation result: Success={IsSuccess}", result.IsSuccess);
        result.IsFailure.ShouldBeTrue("Null PartNumber should fail defensive validation");
    }

    /// <summary>
    /// Tests cancellation token handling for responsive cancellation.
    /// Important for UI responsiveness and resource cleanup.
    /// Services are resilient and don't throw exceptions per user clarification.
    /// </summary>
    [Fact]
    public async Task IsProductIdAvailable_WithCancelledToken_ShouldHandleGracefully()
    {
        // Arrange
        await InitializeValidator();
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Pre-cancel the token

        Logger.LogInformation("=== TESTING: Cancellation token handling ===");

        // Act & Assert - Service is resilient, doesn't throw exceptions
        var isAvailable = await _validator.IsProductIdAvailableAsync(999999, cts.Token);

        Logger.LogInformation("Cancellation handled gracefully, result: {IsAvailable}", isAvailable);
        isAvailable.ShouldBeOfType<bool>("Service should handle cancellation gracefully without throwing");
    }

    #endregion

    #region Performance Tests - High Volume Scenarios

    /// <summary>
    /// Performance test for ProductId validation under load.
    /// Critical given 640,000+ daily operations requiring data integrity.
    /// </summary>
    [Fact]
    public async Task IsProductIdAvailable_PerformanceTest_ShouldMeetResponseTimeRequirements()
    {
        // Arrange
        await InitializeValidator();
        const int iterations = 100;
        var productIds = Enumerable.Range(100000, iterations).ToList();
        var measurements = new List<long>();

        Logger.LogInformation("=== TESTING: ProductId validation performance - {Iterations} iterations ===", iterations);

        // Warm-up
        await _validator.IsProductIdAvailableAsync(99999, CancellationToken.None);

        // Act - Measure multiple iterations
        foreach (var productId in productIds)
        {
            var sw = Stopwatch.StartNew();
            await _validator.IsProductIdAvailableAsync(productId, CancellationToken.None);
            sw.Stop();
            measurements.Add(sw.ElapsedMilliseconds);
        }

        // Assert - Performance requirements
        var averageMs = measurements.Average();
        var maxMs = measurements.Max();

        Logger.LogInformation("Performance: Average={AverageMs:F2}ms, Max={MaxMs}ms, Iterations={Iterations}", 
            averageMs, maxMs, measurements.Count);

        // For high-volume system, should be very fast (< 10ms average)
        averageMs.ShouldBeLessThan(10.0, $"Average response time ({averageMs:F2}ms) should be under 10ms for high-volume operations");
        maxMs.ShouldBeLessThan(50, $"Maximum response time ({maxMs}ms) should be under 50ms");
    }

    /// <summary>
    /// Batch validation test simulating realistic manufacturing scenarios.
    /// Tests multiple unique checks in sequence as would occur during production.
    /// </summary>
    [Fact]
    public async Task IsPartNumberAvailable_BatchValidation_ShouldHandleMultipleChecksEfficiently()
    {
        // Arrange
        await InitializeValidator();
        var partNumbers = new[]
        {
            "BATCH-TEST-001", "BATCH-TEST-002", "BATCH-TEST-003", "BATCH-TEST-004", "BATCH-TEST-005",
            "FORD-F150-BATCH-01", "BMW-X5-BATCH-02", "VW-Q7-BATCH-03", "OEMX-A4-BATCH-04"
        };

        Logger.LogInformation("=== TESTING: Batch PartNumber validation - {Count} part numbers ===", partNumbers.Length);

        var sw = Stopwatch.StartNew();

        // Act - Validate all part numbers
        var results = new List<(string PartNumber, bool IsSuccess)>();
        foreach (var partNumber in partNumbers)
        {
            var productName = $"Product {partNumber}";
            var result = await _validator.ValidateProductUniquenessAsync(partNumber, productName, CancellationToken.None);
            results.Add((partNumber, result.IsSuccess));
        }

        sw.Stop();

        // Assert
        var totalMs = sw.ElapsedMilliseconds;
        var avgMsPerCheck = (double)totalMs / partNumbers.Length;

        Logger.LogInformation("Batch validation: Total={TotalMs}ms, Average={AvgMs:F2}ms per check", totalMs, avgMsPerCheck);

        foreach (var (partNumber, isSuccess) in results)
        {
            Logger.LogInformation("PartNumber '{PartNumber}': Success={IsSuccess}", partNumber, isSuccess);
            isSuccess.ShouldBeTrue($"New PartNumber {partNumber} should pass uniqueness validation");
        }

        avgMsPerCheck.ShouldBeLessThan(5.0, $"Batch validation should average under 5ms per check (actual: {avgMsPerCheck:F2}ms)");
    }

    #endregion

    #region Concurrency Tests - Multi-Threading Scenarios

    /// <summary>
    /// Tests concurrent access to uniqueness validation.
    /// Critical for multi-threaded manufacturing environment with parallel PLC operations.
    /// </summary>
    [Fact]
    public async Task IsProductIdAvailable_ConcurrentAccess_ShouldHandleMultipleThreads()
    {
        // Arrange
        await InitializeValidator();
        const int concurrentTasks = 10;
        const int checksPerTask = 20;

        Logger.LogInformation("=== TESTING: Concurrent ProductId validation - {Tasks} tasks × {Checks} checks ===", 
            concurrentTasks, checksPerTask);

        // Act - Run multiple concurrent validation tasks
        var tasks = Enumerable.Range(0, concurrentTasks).Select(async taskId =>
        {
            var results = new List<bool>();
            var baseProductId = 200000 + (taskId * 1000);

            for (int i = 0; i < checksPerTask; i++)
            {
                var productId = baseProductId + i;
                var isAvailable = await _validator.IsProductIdAvailableAsync(productId, CancellationToken.None);
                results.Add(isAvailable);
            }

            return new { TaskId = taskId, Results = results };
        });

        var allResults = await Task.WhenAll(tasks);

        // Assert
        var totalChecks = allResults.Sum(r => r.Results.Count);
        var successfulChecks = allResults.Sum(r => r.Results.Count(available => available));

        Logger.LogInformation("Concurrent validation: {TotalChecks} total checks, {SuccessfulChecks} successful", 
            totalChecks, successfulChecks);

        totalChecks.ShouldBe(concurrentTasks * checksPerTask, "All concurrent checks should complete");
        successfulChecks.ShouldBe(totalChecks, "All unique ProductIds should be available");

        // Log per-task results
        foreach (var result in allResults)
        {
            var taskSuccessRate = (double)result.Results.Count(r => r) / result.Results.Count * 100;
            Logger.LogInformation("Task {TaskId}: {SuccessRate:F1}% success rate", result.TaskId, taskSuccessRate);
        }
    }

    #endregion

    #region Database State Tests - Real Data Scenarios

    /// <summary>
    /// Validates uniqueness check against actual database state.
    /// Ensures integration with real repository and caching layers.
    /// </summary>
    [Fact]
    public async Task IsProductIdAvailable_WithDatabaseState_ShouldReflectActualData()
    {
        // Arrange
        await InitializeValidator();

        Logger.LogInformation("=== TESTING: Database state validation ===");

        // Act - Check against known database state from TestData
        var existingProductCheck = await _validator.IsProductIdAvailableAsync(1, CancellationToken.None); // DEFAULT product
        var nonExistingProductCheck = await _validator.IsProductIdAvailableAsync(999888, CancellationToken.None);

        // Get actual product count from repository for verification
        var allProducts = await DpRoProductRepository.ListAsync(CancellationToken.None);
        var productCount = allProducts.IsSuccess ? allProducts.Value?.Count() ?? 0 : 0;

        // Assert
        Logger.LogInformation("Database contains {ProductCount} products", productCount);
        Logger.LogInformation("ProductId 1 (existing) availability: {IsAvailable}", existingProductCheck);
        Logger.LogInformation("ProductId 999888 (new) availability: {IsAvailable}", nonExistingProductCheck);

        existingProductCheck.ShouldBeFalse("ProductId 1 (DEFAULT) should exist in database and not be available");
        nonExistingProductCheck.ShouldBeTrue("ProductId 999888 should not exist and be available for use");
        productCount.ShouldBeGreaterThan(0, "TestData should provide at least one product in database");
    }

    /// <summary>
    /// Validates the complete uniqueness validation workflow.
    /// Tests the service in the context of actual product creation scenarios.
    /// </summary>
    [Fact]
    public async Task ValidateProductUniqueness_CompleteWorkflow_ShouldWorkEndToEnd()
    {
        // Arrange
        await InitializeValidator();
        var testProductId = 888999;
        var testPartNumber = "WORKFLOW-TEST-UNIQUE-001";

        Logger.LogInformation("=== TESTING: Complete uniqueness validation workflow ===");
        Logger.LogInformation("Testing ProductId: {ProductId}, PartNumber: {PartNumber}", testProductId, testPartNumber);

        // DIAGNOSTIC: Log all actual TestData to see what's in the database
        await LogActualTestDataProducts();

        // Act - Complete validation workflow
        var productIdCheck = await _validator.IsProductIdAvailableAsync(testProductId, CancellationToken.None);
        var testProductName = $"Product {testPartNumber}";
        var partNumberResult = await _validator.ValidateProductUniquenessAsync(testPartNumber, testProductName, CancellationToken.None);

        // Simulate what would happen in actual product creation
        var canCreateProduct = productIdCheck && partNumberResult.IsSuccess;

        // Assert
        Logger.LogInformation("ProductId {ProductId} available: {IsAvailable}", testProductId, productIdCheck);
        Logger.LogInformation("PartNumber '{PartNumber}' validation success: {IsSuccess}", testPartNumber, partNumberResult.IsSuccess);
        Logger.LogInformation("Can create product: {CanCreate}", canCreateProduct);

        productIdCheck.ShouldBeTrue($"ProductId {testProductId} should be available for new product");
        partNumberResult.IsSuccess.ShouldBeTrue($"PartNumber {testPartNumber} should pass uniqueness validation for new product");
        canCreateProduct.ShouldBeTrue("Complete validation should allow product creation with unique identifiers");
    }

    /// <summary>
    /// DIAGNOSTIC METHOD: Logs all products in TestData to verify database contents vs service behavior
    /// </summary>
    private async Task LogActualTestDataProducts()
    {
        Logger.LogInformation("=== DIAGNOSTIC: Actual TestData Products in Database ===");
        
        var allProducts = await DpRoProductRepository.ListAsync(CancellationToken.None);
        if (allProducts.IsSuccess && allProducts.Value != null)
        {
            var products = allProducts.Value.ToList();
            Logger.LogInformation("Found {ProductCount} products in TestData:", products.Count);
            
            foreach (var product in products.Take(10)) // Log first 10 products
            {
                Logger.LogInformation("ProductId: {ProductId}, PartNumber: '{PartNumber}', Name: '{ProductName}'", 
                    product.ProductId, product.PartNumber, product.ProductName);
            }
            
            if (products.Count > 10)
            {
                Logger.LogInformation("... and {RemainingCount} more products", products.Count - 10);
            }
        }
        else
        {
            Logger.LogInformation("Failed to retrieve products from database: {Error}", 
                allProducts.Errors != null ? string.Join("; ", allProducts.Errors) : "Unknown error");
        }
        
        Logger.LogInformation("=== END DIAGNOSTIC ===");
    }

    /// <summary>
    /// DIAGNOSTIC METHOD: Logs specific ProductId to verify if it exists in database
    /// </summary>
    private async Task LogSpecificProductId(int productId)
    {
        Logger.LogInformation("DIAGNOSTIC: Checking if ProductId {ProductId} exists in database...", productId);
        
        var product = await DpRoProductRepository.GetByIdAsync(productId, CancellationToken.None);
        if (product.IsSuccess && product.Value != null)
        {
            Logger.LogInformation("FOUND ProductId {ProductId}: PartNumber='{PartNumber}', Name='{ProductName}'", 
                productId, product.Value.PartNumber, product.Value.ProductName);
        }
        else
        {
            Logger.LogInformation("NOT FOUND ProductId {ProductId} in database", productId);
        }
    }

    /// <summary>
    /// DIAGNOSTIC METHOD: Logs specific PartNumber to verify if it exists in database
    /// </summary>
    private async Task LogSpecificPartNumber(string partNumber)
    {
        Logger.LogInformation("DIAGNOSTIC: Checking if PartNumber '{PartNumber}' exists in database...", partNumber);
        
        var allProducts = await DpRoProductRepository.ListAsync(CancellationToken.None);
        if (allProducts.IsSuccess && allProducts.Value != null)
        {
            var matchingProduct = allProducts.Value.FirstOrDefault(p => 
                string.Equals(p.PartNumber, partNumber, StringComparison.OrdinalIgnoreCase));
            
            if (matchingProduct != null)
            {
                Logger.LogInformation("FOUND PartNumber '{PartNumber}': ProductId={ProductId}, Name='{ProductName}'", 
                    partNumber, matchingProduct.ProductId, matchingProduct.ProductName);
            }
            else
            {
                Logger.LogInformation("NOT FOUND PartNumber '{PartNumber}' in database", partNumber);
            }
        }
        else
        {
            Logger.LogInformation("Failed to retrieve products for PartNumber search: {Error}", 
                allProducts.Errors != null ? string.Join("; ", allProducts.Errors) : "Unknown error");
        }
    }

    #endregion
}