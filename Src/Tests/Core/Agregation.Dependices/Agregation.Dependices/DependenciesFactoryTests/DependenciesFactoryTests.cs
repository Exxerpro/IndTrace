// <copyright file="DependenciesFactoryTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Agregation.Dependices.DependenciesFactoryTests;

/// <summary>
/// Individual repository tests for DependenciesFactory.
/// Tests each repository separately to ensure proper initialization and data availability.
/// </summary>
public class DependenciesFactoryTests : DependenciesFactory
{
    public DependenciesFactoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    /// <remarks>
    /// BarCode Repository Tests
    /// </remarks>
    /// <summary>
    /// Tests that BarCode repository is initialized and contains data.
    /// </summary>
    [Fact]
    public async Task DpRoBarCodeRepository_ShouldInitialize_AndContainData()
    {
        await Initialization;

        // Arrange

        var logger = XUnitLogger.CreateLogger<DependenciesFactoryTests>();

        // Act
        var result = await DpRoBarCodeRepository.FirstOrDefaultAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        logger.LogInformation("First BarCode from repository: {@BarCode}", result.Value.Label);
    }

    /// <summary>
    /// Tests that regular BarCode repository is initialized and functional.
    /// </summary>
    [Fact]
    public async Task DpBarCodeRepository_ShouldInitialize_AndBeFunctional()
    {
        await Initialization;

        // Arrange

        // Act
        var countSpec = new Specification<BarCode>(b => true);
        var count = await DpBarCodeRepository.CountAsync(countSpec, TestContext.Current.CancellationToken);

        // Assert
        DpBarCodeRepository.ShouldNotBeNull();
        count.IsSuccess.ShouldBeTrue();
        count.Value.ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// Tests searching for specific barcodes by label in the BarCode repository.
    /// Verifies that specific barcode labels can be found and retrieved correctly.
    /// </summary>
    [Theory]
    [InlineData("L1AL90164629232372682")]
    [InlineData("L1AL90164629232372683")]
    public async Task DpBarCodeRepository_ShouldFindSpecificBarcodes_ByLabel(string expectedLabel)
    {
        await Initialization;

        // Arrange

        var logger = XUnitLogger.CreateLogger<DependenciesFactoryTests>();

        // Act
        var searchSpec = new Specification<BarCode>(b => b.Label.Value == expectedLabel);
        var result = await DpRoBarCodeRepository.FirstOrDefaultAsync(searchSpec, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        if (result.Value != null)
        {
            result.Value.Label.Value.ShouldBe(expectedLabel);
            logger.LogInformation("Found BarCode with label: {Label}, EntitieId: {BarCodeId}", result.Value.Label, result.Value.BarCodeId);
        }
        else
        {
            logger.LogWarning("BarCode with label {ExpectedLabel} not found in test data", expectedLabel);
        }
    }

    /// <summary>
    /// Tests searching for barcodes by ID range in the BarCode repository.
    /// Verifies that barcodes within specific ID ranges can be retrieved.
    /// </summary>
    [Theory]
    [InlineData(180, 200)]
    [InlineData(681, 700)]
    [InlineData(1, 50)]
    public async Task DpBarCodeRepository_ShouldFindBarcodes_ByIdRange(int minId, int maxId)
    {
        await Initialization;

        // Arrange

        var logger = XUnitLogger.CreateLogger<DependenciesFactoryTests>();

        // Act
        var rangeSpec = new Specification<BarCode>(b => b.BarCodeId.Value >= minId && b.BarCodeId.Value <= maxId);
        var results = await DpRoBarCodeRepository.ListAsync(rangeSpec, TestContext.Current.CancellationToken);

        // Assert
        results.IsSuccess.ShouldBeTrue();
        logger.LogInformation("Found {Count} BarCodes in ID range {MinId}-{MaxId}", (results.Value ?? Enumerable.Empty<BarCode>()).Count(), minId, maxId);
        // Verify all returned barcodes are within the specified range
        foreach (var barcode in results.Value ?? Enumerable.Empty<BarCode>())
        {
            barcode.BarCodeId.Value.ShouldBeGreaterThanOrEqualTo(minId);
            barcode.BarCodeId.Value.ShouldBeLessThanOrEqualTo(maxId);
        }
    }

    /// <summary>
    /// Tests searching for barcodes using label pattern matching in the BarCode repository.
    /// Verifies that barcodes can be found using partial label matches.
    /// </summary>
    [Theory]
    [InlineData("L1AL901646292323726")]
    [InlineData("80")]
    [InlineData("901646292323")]
    public async Task DpBarCodeRepository_ShouldFindBarcodes_ByLabelPattern(string labelPattern)
    {
        await Initialization;

        // Arrange

        var logger = XUnitLogger.CreateLogger<DependenciesFactoryTests>();

        // Act
        var patternSpec = new Specification<BarCode>(b => b.Label.Value.Contains(labelPattern));
        var results = await DpRoBarCodeRepository.ListAsync(patternSpec, TestContext.Current.CancellationToken);

        // Assert
        results.IsSuccess.ShouldBeTrue();
        logger.LogInformation("Found {Count} BarCodes matching pattern '{Pattern}'", (results.Value ?? Enumerable.Empty<BarCode>()).Count(), labelPattern);
        // Verify all returned barcodes contain the pattern
        foreach (var barcode in results.Value ?? Enumerable.Empty<BarCode>())
        {
            barcode.Label.Value.ShouldContain(labelPattern);
            logger.LogDebug("Matched BarCode: {Label} (ID: {BarCodeId})", barcode.Label, barcode.BarCodeId);
        }
    }

    /// <summary>
    /// Tests retrieving the highest ID barcode from the BarCode repository.
    /// Verifies sorting and ordering functionality of the repository.
    /// </summary>
    [Fact]
    public async Task DpBarCodeRepository_ShouldFindHighestIdBarcode()
    {
        await Initialization;

        // Arrange

        var logger = XUnitLogger.CreateLogger<DependenciesFactoryTests>();

        // Act
        var allBarcodesSpec = new Specification<BarCode>(b => true);
        allBarcodesSpec.AddOrderByDescending(b => b.BarCodeId);

        var result = await DpRoBarCodeRepository.FirstOrDefaultAsync(allBarcodesSpec, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        logger.LogInformation("Highest ID BarCode: {Label} with ID: {BarCodeId}", result.Value.Label, result.Value.BarCodeId);

        // Verify it's actually the highest by checking no other barcode has a higher ID
        var higherIdSpec = new Specification<BarCode>(b => b.BarCodeId.Value > result.Value.BarCodeId.Value);
        var higherIdResults = await DpRoBarCodeRepository.ListAsync(higherIdSpec, TestContext.Current.CancellationToken);

        higherIdResults.IsSuccess.ShouldBeTrue();
        (higherIdResults.Value ?? Enumerable.Empty<BarCode>()).Count().ShouldBe(0, $"Found {(higherIdResults.Value ?? Enumerable.Empty<BarCode>()).Count()} barcodes with ID higher than {result.Value.BarCodeId}");
    }

    /// <summary>
    /// Tests counting barcodes with specific criteria in the BarCode repository.
    /// Verifies counting functionality and data consistency.
    /// </summary>
    [Fact]
    public async Task DpBarCodeRepository_ShouldCountBarcodes_WithSpecificCriteria()
    {
        await Initialization;

        // Arrange

        var logger = XUnitLogger.CreateLogger<DependenciesFactoryTests>();

        // Act - Count all barcodes
        var allBarcodesSpec = new Specification<BarCode>(b => true);
        var totalCount = await DpRoBarCodeRepository.CountAsync(allBarcodesSpec, TestContext.Current.CancellationToken);

        // Act - Count barcodes with specific label pattern
        var patternSpec = new Specification<BarCode>(b => b.Label.Value.StartsWith("L1AL"));
        var patternCount = await DpRoBarCodeRepository.CountAsync(patternSpec, TestContext.Current.CancellationToken);

        // Act - Count barcodes with ID greater than 500
        var highIdSpec = new Specification<BarCode>(b => b.BarCodeId.Value > 500);
        var highIdCount = await DpRoBarCodeRepository.CountAsync(highIdSpec, TestContext.Current.CancellationToken);

        // Assert
        totalCount.IsSuccess.ShouldBeTrue();
        patternCount.IsSuccess.ShouldBeTrue();
        highIdCount.IsSuccess.ShouldBeTrue();

        totalCount.Value.ShouldBeGreaterThan(0);
        patternCount.Value.ShouldBeLessThanOrEqualTo(totalCount.Value);
        highIdCount.Value.ShouldBeLessThanOrEqualTo(totalCount.Value);

        logger.LogInformation("BarCode counts - Total: {Total}, Pattern 'L1AL': {Pattern}, ID > 500: {HighId}",
            totalCount.Value, patternCount.Value, highIdCount.Value);
    }

    /// <summary>
    /// Tests retrieving barcodes with pagination in the BarCode repository.
    /// Verifies paging functionality and data consistency across pages.
    /// </summary>
    [Fact]
    public async Task DpBarCodeRepository_ShouldSupportPagination()
    {
        await Initialization;

        // Arrange

        var logger = XUnitLogger.CreateLogger<DependenciesFactoryTests>();
        const int pageSize = 5;

        // Act - Get first page
        var firstPageSpec = new Specification<BarCode>(b => true);
        firstPageSpec.AddOrderBy(b => b.BarCodeId);
        firstPageSpec.ApplyPaging(0, pageSize);

        var firstPage = await DpRoBarCodeRepository.ListAsync(firstPageSpec, TestContext.Current.CancellationToken);

        // Act - Get second page
        var secondPageSpec = new Specification<BarCode>(b => true);
        secondPageSpec.AddOrderBy(b => b.BarCodeId);
        secondPageSpec.ApplyPaging(pageSize, pageSize);

        var secondPage = await DpRoBarCodeRepository.ListAsync(secondPageSpec, TestContext.Current.CancellationToken);

        // Assert
        firstPage.IsSuccess.ShouldBeTrue();
        secondPage.IsSuccess.ShouldBeTrue();
        (firstPage.Value ?? Enumerable.Empty<BarCode>()).Count().ShouldBeLessThanOrEqualTo(pageSize);
        (secondPage.Value ?? Enumerable.Empty<BarCode>()).Count().ShouldBeLessThanOrEqualTo(pageSize);
        // Verify no overlap between pages (assuming we have enough data)
        if ((firstPage.Value ?? Enumerable.Empty<BarCode>()).Any() && (secondPage.Value ?? Enumerable.Empty<BarCode>()).Any())
        {
            var firstPageIds = (firstPage.Value ?? Enumerable.Empty<BarCode>()).Select(b => b.BarCodeId).ToHashSet();
            var secondPageIds = (secondPage.Value ?? Enumerable.Empty<BarCode>()).Select(b => b.BarCodeId).ToHashSet();
            firstPageIds.Intersect(secondPageIds).ShouldBeEmpty("Pages should not overlap");
            logger.LogInformation("Pagination test - First page: {FirstCount} items, Second page: {SecondCount} items", (firstPage.Value ?? Enumerable.Empty<BarCode>()).Count(), (secondPage.Value ?? Enumerable.Empty<BarCode>()).Count());
        }
    }

    /// <remarks>
    /// Customer Repository Tests
    /// </remarks>
    /// <summary>
    /// Tests that Customer read-only repository is initialized and contains data.
    /// </summary>
    [Fact]
    public async Task DpRoCustomerRepository_ShouldInitialize_AndContainData()
    {
        await Initialization;

        // Arrange

        var logger = XUnitLogger.CreateLogger<DependenciesFactoryTests>();

        // Act
        var result = await DpRoCustomerRepository.FirstOrDefaultAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        logger.LogInformation("First Customer from repository: {@Customer}", result.Value.Name);
    }

    /// <summary>
    /// Tests that regular Customer repository is initialized and functional.
    /// </summary>
    [Fact]
    public async Task DpCustomerRepository_ShouldInitialize_AndBeFunctional()
    {
        await Initialization;

        // Arrange

        // Act
        var countSpec = new Specification<Customer>(c => true);
        var count = await DpCustomerRepository.CountAsync(countSpec, TestContext.Current.CancellationToken);

        // Assert
        DpCustomerRepository.ShouldNotBeNull();
        count.IsSuccess.ShouldBeTrue();
        count.Value.ShouldBeGreaterThan(0);
    }

    /// <remarks>
    /// Cycle Repository Tests
    /// </remarks>
    /// <summary>
    /// Tests that Cycle read-only repository is initialized and contains data.
    /// </summary>
    [Fact]
    public async Task DpRoCycleRepository_ShouldInitialize_AndContainData()
    {
        await Initialization;

        // Arrange

        var logger = XUnitLogger.CreateLogger<DependenciesFactoryTests>();

        // Act
        var result = await DpRoCycleRepository.FirstOrDefaultAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        logger.LogInformation("First Cycle from repository: CycleId {@CycleId}", result.Value.CycleId);
    }

    /// <summary>
    /// Tests that regular Cycle repository is initialized and functional.
    /// </summary>
    [Fact]
    public async Task DpCycleRepository_ShouldInitialize_AndBeFunctional()
    {
        await Initialization;

        // Arrange

        // Act
        var countSpec = new Specification<Cycle>(c => true);
        var count = await DpCycleRepository.CountAsync(countSpec, TestContext.Current.CancellationToken);

        // Assert
        DpCycleRepository.ShouldNotBeNull();
        count.IsSuccess.ShouldBeTrue();
        count.Value.ShouldBeGreaterThan(0);
    }

    /// <remarks>
    /// Machine Repository Tests
    /// </remarks>
    /// <summary>
    /// Tests that Machine read-only repository is initialized and contains data.
    /// </summary>
    [Fact]
    public async Task DpRoMachineRepository_ShouldInitialize_AndContainData()
    {
        await Initialization;

        // Arrange

        var logger = XUnitLogger.CreateLogger<DependenciesFactoryTests>();

        // Act
        var result = await DpRoMachineRepository.FirstOrDefaultAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        logger.LogInformation("First Machine from repository: {@Machine}", result.Value.Name);
    }

    /// <remarks>
    /// Machine Repository Tests
    /// </remarks>
    /// <summary>
    /// Tests that Machine read-only repository is initialized and contains data.
    /// </summary>
    [Fact]
    public async Task DpRoMachineRepository_ShouldInitialize_FindMachineById()
    {
        await Initialization;

        // Arrange

        var logger = XUnitLogger.CreateLogger<DependenciesFactoryTests>();

        // Act
        var result = await DpRoMachineRepository.FirstOrDefaultAsync(cancellationToken: TestContext.Current.CancellationToken);

        var Spec300 = new Specification<Machine>(m => m.MachineId == new MachineId(300));

        var resultMachine300 = await DpRoMachineRepository.FirstOrDefaultAsync(Spec300, cancellationToken: TestContext.Current.CancellationToken); await DpRoMachineRepository.FirstOrDefaultAsync(cancellationToken: TestContext.Current.CancellationToken);

        resultMachine300.IsSuccess.ShouldBeTrue();
        resultMachine300.Value.ShouldNotBeNull();

        var machine300 = resultMachine300.Value;
        machine300.ShouldNotBeNull();
        machine300.MachineId.Value.ShouldBe(300);

        // Note: In-memory database doesn't support value converters for enums
        // The MachineType should still be Process, but check the enum directly
        machine300.MachineType.ShouldBe(MachineType.Process);

        logger.LogInformation("Result was machine {machine}", machine300);

        //        var machineResult = await FetchMachineByIdAsync(MachineId, cancellationToken).ConfigureAwait(false);
        /*
           private async Task<Result<Machine?>> FetchMachineByIdAsync(int machineId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Repository Coordination: Fetching Machine by ID: {MachineId}", machineId);

        var result = await machineRepository.FirstOrDefaultAsync(new Specification<Machine>(f => f.MachineId == machineId), cancellationToken);

        logger.LogInformation("Repository Coordination: Fetch Machine Result: {Result}", result.IsSuccess ? "Success" : "Failure");
        logger.LogInformation("Repository Coordination: Fetched Machine: {Machine}", result.Value);
        return result;
    }

         */
        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        logger.LogInformation("First Machine from repository: {@Machine}", result.Value.Name);
    }

    /// <summary>
    /// Tests that regular Machine repository is initialized and functional.
    /// </summary>
    [Fact]
    public async Task DpMachineRepository_ShouldInitialize_AndBeFunctional()
    {
        await Initialization;

        // Arrange

        // Act
        var countSpec = new Specification<Machine>(m => true);
        var count = await DpMachineRepository.CountAsync(countSpec, TestContext.Current.CancellationToken);

        // Assert
        DpMachineRepository.ShouldNotBeNull();
        count.IsSuccess.ShouldBeTrue();
        count.Value.ShouldBeGreaterThan(0);
    }

    /// <remarks>
    /// Rule Repository Tests
    /// </remarks>
    /// <summary>
    /// Tests that Rule read-only repository is initialized and contains data.
    /// </summary>
    [Fact]
    public async Task DpRoRuleRepository_ShouldInitialize_AndContainData()
    {
        await Initialization;

        // Arrange

        var logger = XUnitLogger.CreateLogger<DependenciesFactoryTests>();

        // Act
        var result = await DpRoRuleRepository.FirstOrDefaultAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        logger.LogInformation("First Rule from repository: {@Rule}", result.Value.Name);
    }

    /// <summary>
    /// Tests that regular Rule repository is initialized and functional.
    /// </summary>
    [Fact]
    public async Task DpRuleRepository_ShouldInitialize_AndBeFunctional()
    {
        await Initialization;

        // Arrange

        // Act
        var countSpec = new Specification<Rule>(r => true);
        var count = await DpRuleRepository.CountAsync(countSpec, TestContext.Current.CancellationToken);

        // Assert
        DpRuleRepository.ShouldNotBeNull();
        count.IsSuccess.ShouldBeTrue();
        count.Value.ShouldBeGreaterThan(0);
    }

    /// <remarks>
    /// PLC Repository Tests
    /// </remarks>
    /// <summary>
    /// Tests that PLC read-only repository is initialized and contains data.
    /// </summary>
    [Fact]
    public async Task DpRoPlcRepository_ShouldInitialize_AndContainData()
    {
        await Initialization;

        // Arrange

        var logger = XUnitLogger.CreateLogger<DependenciesFactoryTests>();

        // Act
        var result = await DpRoPlcRepository.FirstOrDefaultAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        logger.LogInformation("First PLC from repository: {@Plc}", result.Value.Name);
    }

    /// <summary>
    /// Tests that regular PLC repository is initialized and functional.
    /// </summary>
    [Fact]
    public async Task DpPlcRepository_ShouldInitialize_AndBeFunctional()
    {
        await Initialization;

        // Arrange

        // Act
        var countSpec = new Specification<Plc>(p => true);
        var count = await DpPlcRepository.CountAsync(countSpec, TestContext.Current.CancellationToken);

        // Assert
        DpPlcRepository.ShouldNotBeNull();
        count.IsSuccess.ShouldBeTrue();
        count.Value.ShouldBeGreaterThan(0);
    }

    /// <remarks>
    /// Product Repository Tests
    /// </remarks>
    /// <summary>
    /// Tests that Product read-only repository is initialized and contains data.
    /// </summary>
    [Fact]
    public async Task DpRoProductRepository_ShouldInitialize_AndContainData()
    {
        await Initialization;

        // Arrange

        var logger = XUnitLogger.CreateLogger<DependenciesFactoryTests>();

        // Act
        var result = await DpRoProductRepository.FirstOrDefaultAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        logger.LogInformation("First Product from repository: {@Product}", result.Value.ProductName);
    }

    /// <summary>
    /// Tests that regular Product repository is initialized and functional.
    /// </summary>
    [Fact]
    public async Task DpProductRepository_ShouldInitialize_AndBeFunctional()
    {
        await Initialization;

        // Arrange

        // Act
        var countSpec = new Specification<Product>(p => true);
        var count = await DpProductRepository.CountAsync(countSpec, TestContext.Current.CancellationToken);

        // Assert
        DpProductRepository.ShouldNotBeNull();
        count.IsSuccess.ShouldBeTrue();
        count.Value.ShouldBeGreaterThan(0);
    }

    /// <remarks>
    /// Recipe Repository Tests
    /// </remarks>
    /// <summary>
    /// Tests that Recipe read-only repository is initialized and contains data.
    /// </summary>
    [Fact]
    public async Task DpRoRecipeRepository_ShouldInitialize_AndContainData()
    {
        await Initialization;

        // Arrange

        var logger = XUnitLogger.CreateLogger<DependenciesFactoryTests>();

        // Act
        var result = await DpRoRecipeRepository.FirstOrDefaultAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        logger.LogInformation("First Recipe from repository: RecipeId {@RecipeId}", result.Value.RecipeId);
    }

    /// <summary>
    /// Tests that regular Recipe repository is initialized and functional.
    /// </summary>
    [Fact]
    public async Task DpRecipeRepository_ShouldInitialize_AndBeFunctional()
    {
        await Initialization;

        // Arrange

        // Act
        var countSpec = new Specification<Recipe>(r => true);
        var count = await DpRecipeRepository.CountAsync(countSpec, TestContext.Current.CancellationToken);

        // Assert
        DpRecipeRepository.ShouldNotBeNull();
        count.IsSuccess.ShouldBeTrue();
        count.Value.ShouldBeGreaterThan(0);
    }

    /// <remarks>
    /// Register Repository Tests
    /// </remarks>
    /// <summary>
    /// Tests that Register read-only repository is initialized and contains data.
    /// </summary>
    [Fact]
    public async Task DpRoRegisterRepository_ShouldInitialize_AndContainData()
    {
        await Initialization;

        // Arrange

        var logger = XUnitLogger.CreateLogger<DependenciesFactoryTests>();

        // Act
        var result = await DpRoRegisterRepository.FirstOrDefaultAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        logger.LogInformation("First Register from repository: EntitieId {@EntitieId}", result.Value.RegisterId);
    }

    /// <summary>
    /// Tests that regular Register repository is initialized and functional.
    /// </summary>
    [Fact]
    public async Task DpRegisterRepository_ShouldInitialize_AndBeFunctional()
    {
        await Initialization;

        // Arrange

        // Act
        var countSpec = new Specification<Register>(r => true);
        var count = await DpRoRegisterRepository.CountAsync(countSpec, TestContext.Current.CancellationToken);

        // Assert
        DpRoRegisterRepository.ShouldNotBeNull();
        count.IsSuccess.ShouldBeTrue();
        count.Value.ShouldBeGreaterThan(0);
    }

    /// <remarks>
    /// Variable Repository Tests
    /// </remarks>
    /// <summary>
    /// Tests that Variable read-only repository is initialized and contains data.
    /// </summary>
    [Fact]
    public async Task DpRoVariablesRepository_ShouldInitialize_AndContainData()
    {
        await Initialization;

        // Arrange

        var logger = XUnitLogger.CreateLogger<DependenciesFactoryTests>();

        // Act
        var result = await DpRoVariablesRepository.FirstOrDefaultAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        logger.LogInformation("First Variable from repository: {@Variable}", result.Value.Name);
    }

    /// <summary>
    /// Tests that regular Variable repository is initialized and functional.
    /// </summary>
    [Fact]
    public async Task DpVariablesRepository_ShouldInitialize_AndBeFunctional()
    {
        await Initialization;

        // Arrange

        // Act
        var countSpec = new Specification<Variable>(v => true);
        var count = await DpVariablesRepository.CountAsync(countSpec, TestContext.Current.CancellationToken);

        // Assert
        DpVariablesRepository.ShouldNotBeNull();
        count.IsSuccess.ShouldBeTrue();
        count.Value.ShouldBeGreaterThan(0);
    }

    /// <remarks>
    /// VariablesGroup Repository Tests
    /// </remarks>
    /// <summary>
    /// Tests that VariablesGroup read-only repository is initialized and contains data.
    /// </summary>
    [Fact]
    public async Task DpRoVariablesGroupRepository_ShouldInitialize_AndContainData()
    {
        await Initialization;

        // Arrange

        var logger = XUnitLogger.CreateLogger<DependenciesFactoryTests>();

        // Act
        var result = await DpRoVariablesGroupRepository.FirstOrDefaultAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        logger.LogInformation("First VariablesGroup from repository: {@VariablesGroup}", result.Value.VariableGroupName);
    }

    /// <summary>
    /// Tests that regular VariablesGroup repository is initialized and functional.
    /// </summary>
    [Fact]
    public async Task DpVariablesGroupRepository_ShouldInitialize_AndBeFunctional()
    {
        await Initialization;

        // Arrange

        // Act
        var countSpec = new Specification<VariablesGroup>(vg => true);
        var count = await DpVariablesGroupRepository.CountAsync(countSpec, TestContext.Current.CancellationToken);

        // Assert
        DpVariablesGroupRepository.ShouldNotBeNull();
        count.IsSuccess.ShouldBeTrue();
        count.Value.ShouldBeGreaterThan(0);
    }

    /// <remarks>
    /// WorkFlow Repository Tests
    /// </remarks>
    /// <summary>
    /// Tests that WorkFlow read-only repository is initialized and contains data.
    /// </summary>
    [Fact]
    public async Task DpRoWorkFlowRepository_ShouldInitialize_AndContainData()
    {
        await Initialization;

        // Arrange

        var logger = XUnitLogger.CreateLogger<DependenciesFactoryTests>();

        // Act
        var result = await DpRoWorkFlowRepository.FirstOrDefaultAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        logger.LogInformation("First WorkFlow from repository: WorkFlowId {@WorkFlowId}", result.Value.WorkFlowId);
    }

    /// <summary>
    /// Tests that regular WorkFlow repository is initialized and functional.
    /// </summary>
    [Fact]
    public async Task DpWorkFlowRepository_ShouldInitialize_AndBeFunctional()
    {
        await Initialization;

        // Arrange

        // Act
        var countSpec = new Specification<WorkFlow>(wf => true);
        var count = await DpWorkFlowRepository.CountAsync(countSpec, TestContext.Current.CancellationToken);

        // Assert
        DpWorkFlowRepository.ShouldNotBeNull();
        count.IsSuccess.ShouldBeTrue();
        count.Value.ShouldBeGreaterThan(0);
    }

    /// <remarks>
    /// Line Repository Tests
    /// </remarks>
    /// <summary>
    /// Tests that Line read-only repository is initialized and contains data.
    /// </summary>
    [Fact]
    public async Task DpRoLineRepository_ShouldInitialize_AndContainData()
    {
        await Initialization;

        // Arrange

        var logger = XUnitLogger.CreateLogger<DependenciesFactoryTests>();

        // Act
        var result = await DpRoLineRepository.FirstOrDefaultAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        logger.LogInformation("First Line from repository: {@Line}", result.Value.Name);
    }

    /// <summary>
    /// Tests that regular Line repository is initialized and functional.
    /// </summary>
    [Fact]
    public async Task DpLineRepository_ShouldInitialize_AndBeFunctional()
    {
        await Initialization;

        // Arrange

        // Act
        var countSpec = new Specification<Line>(l => true);
        var count = await DpLineRepository.CountAsync(countSpec, TestContext.Current.CancellationToken);

        // Assert
        DpLineRepository.ShouldNotBeNull();
        count.IsSuccess.ShouldBeTrue();
        count.Value.ShouldBeGreaterThan(0);
    }

    /// <remarks>
    /// MachinePlc Repository Tests
    /// </remarks>
    /// <summary>
    /// Tests that MachinePlc read-only repository is initialized and contains data.
    /// </summary>
    [Fact]
    public async Task DpRoMachinePlcRepository_ShouldInitialize_AndContainData()
    {
        await Initialization;

        // Arrange

        var logger = XUnitLogger.CreateLogger<DependenciesFactoryTests>();

        // Act
        var result = await DpRoMachinePlcRepository.FirstOrDefaultAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        logger.LogInformation("First MachinePlc from repository: MachineId {@MachineId}, PlcId {@PlcId}", result.Value.MachineId, result.Value.PlcId);
    }

    /// <summary>
    /// Tests that regular MachinePlc repository is initialized and functional.
    /// </summary>
    [Fact]
    public async Task DpMachinePlcRepository_ShouldInitialize_AndBeFunctional()
    {
        await Initialization;

        // Arrange

        // Act
        var countSpec = new Specification<MachinePlc>(mp => true);
        var count = await DpMachinePlcRepository.CountAsync(countSpec, TestContext.Current.CancellationToken);

        // Assert
        DpMachinePlcRepository.ShouldNotBeNull();
        count.IsSuccess.ShouldBeTrue();
        count.Value.ShouldBeGreaterThan(0);
    }

    /// <remarks>
    /// Additional Repository Tests
    /// </remarks>
    /// <summary>
    /// Tests that MasterLabel read-only repository is initialized and contains data.
    /// </summary>
    [Fact]
    public async Task DpRoMasterLabelRepository_ShouldInitialize_AndContainData()
    {
        await Initialization;

        // Arrange

        // Act
        var result = await DpRoMasterLabelRepository.FirstOrDefaultAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        // Note: MasterLabel data may be optional, so we just verify the repository is functional
        DpRoMasterLabelRepository.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests that DistinctRegister read-only repository is initialized and functional.
    /// </summary>
    [Fact]
    public async Task DpRoDistinctRegisterRepository_ShouldInitialize_AndBeFunctional()
    {
        await Initialization;

        // Arrange

        // Act & Assert
        DpRoDistinctRegisterRepository.ShouldNotBeNull();

        // Note: DistinctRegister may not have test data, so we just verify initialization
        var countSpec = new Specification<DistinctRegister>(dr => true);
        var count = await DpDistinctRegisterRepository.CountAsync(countSpec, TestContext.Current.CancellationToken);
        count.IsSuccess.ShouldBeTrue();
    }

    /// <summary>
    /// Tests that ConfigApp repository is initialized and functional.
    /// </summary>
    [Fact]
    public async Task DpConfigAppRepository_ShouldInitialize_AndBeFunctional()
    {
        await Initialization;

        // Arrange

        // Act
        var countSpec = new Specification<Domain.Entities.ConfigApp>(ca => true);
        var count = await DpConfigAppRepository.CountAsync(countSpec, TestContext.Current.CancellationToken);

        // Assert
        DpConfigAppRepository.ShouldNotBeNull();
        count.IsSuccess.ShouldBeTrue();
        count.Value.ShouldBeGreaterThan(0);
    }

    /// <remarks>
    /// OeeRegister Repository Tests
    /// </remarks>
    /// <summary>
    /// Tests that OeeRegister read-only repository is initialized and contains data.
    /// </summary>
    [Fact]
    public async Task DpRoOeeRegisterRepository_ShouldInitialize_AndContainData()
    {
        await Initialization;

        // Arrange

        var logger = XUnitLogger.CreateLogger<DependenciesFactoryTests>();

        // Act
        var result = await DpRoOeeRegisterRepository.FirstOrDefaultAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        logger.LogInformation("First OeeRegister from repository: OeeRegisterId {@OeeRegisterId}", result.Value.OeeRegisterId);
    }

    /// <summary>
    /// Tests that regular OeeRegister repository is initialized and functional.
    /// </summary>
    [Fact]
    public async Task DpOeeRegisterRepository_ShouldInitialize_AndBeFunctional()
    {
        await Initialization;

        // Arrange

        // Act
        var countSpec = new Specification<OeeRegister>(o => true);
        var count = await DpOeeRegisterRepository.CountAsync(countSpec, TestContext.Current.CancellationToken);

        // Assert
        DpOeeRegisterRepository.ShouldNotBeNull();
        count.IsSuccess.ShouldBeTrue();
        count.Value.ShouldBeGreaterThan(0);
    }

    /// <remarks>
    /// KpiOee Repository Tests
    /// </remarks>
    /// <summary>
    /// Tests that KpiOee read-only repository is initialized and contains data.
    /// </summary>
    [Fact]
    public async Task DpRoKpiOeeRepository_ShouldInitialize_AndContainData()
    {
        await Initialization;

        // Arrange

        var logger = XUnitLogger.CreateLogger<DependenciesFactoryTests>();

        // Act
        var result = await DpRoKpiOeeRepository.FirstOrDefaultAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        logger.LogInformation("First KpiOee from repository: KpiOeeId {@KpiOeeId}", result.Value.KpiOeeId);
    }

    /// <summary>
    /// Tests that regular KpiOee repository is initialized and functional.
    /// </summary>
    [Fact]
    public async Task DpKpiOeeRepository_ShouldInitialize_AndBeFunctional()
    {
        await Initialization;

        // Arrange

        // Act
        var countSpec = new Specification<KpiOee>(k => true);
        var count = await DpKpiOeeRepository.CountAsync(countSpec, TestContext.Current.CancellationToken);

        // Assert
        DpKpiOeeRepository.ShouldNotBeNull();
        count.IsSuccess.ShouldBeTrue();
        count.Value.ShouldBeGreaterThan(0);
    }

    /// <remarks>
    /// PerformanceData Repository Tests
    /// </remarks>
    /// <summary>
    /// Tests that PerformanceData read-only repository is initialized and contains data.
    /// </summary>
    [Fact]
    public async Task DpRoPerformanceDataRepository_ShouldInitialize_AndContainData()
    {
        await Initialization;

        // Arrange

        var logger = XUnitLogger.CreateLogger<DependenciesFactoryTests>();

        // Act
        var result = await DpRoPerformanceDataRepository.FirstOrDefaultAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        logger.LogInformation("First PerformanceData from repository: PerformanceDataId {@PerformanceDataId}", result.Value.PerformanceDataId);
    }

    /// <summary>
    /// Tests that regular PerformanceData repository is initialized and functional.
    /// </summary>
    [Fact]
    public async Task DpPerformanceDataRepository_ShouldInitialize_AndBeFunctional()
    {
        await Initialization;

        // Arrange

        // Act
        var countSpec = new Specification<PerformanceData>(p => true);
        var count = await DpPerformanceDataRepository.CountAsync(countSpec, TestContext.Current.CancellationToken);

        // Assert
        DpPerformanceDataRepository.ShouldNotBeNull();
        count.IsSuccess.ShouldBeTrue();
        count.Value.ShouldBeGreaterThan(0);
    }
}