// <copyright file="CreateProductCommandTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Products;

/// <summary>
/// Unit tests for CreateProductCommand
/// </summary>
public class CreateProductCommandTests
{
    // MARKED FOR REMOVAL - Constructor null guard test no longer needed with Result<T> patterns
    // /// <summary>
    // /// Executes Constructor_WithValidProductCreationDto_ShouldCreateInstance operation.
    // /// </summary>
    // [Fact]
    // public void Constructor_WithValidProductCreationDto_ShouldCreateInstance()
    // {
    //     // Arrange
    //     var productCreationDto = CreateValidProductCreationDto();

    //     // Act
    //     var command = new CreateProductCommand(productCreationDto);

    //     // Assert
    //     command.ShouldNotBeNull();
    //     command.Product.ShouldNotBeNull();
    //     command.WorkFlows.ShouldNotBeNull();
    //     command.Rule.ShouldNotBeNull();
    //     command.Recipe.ShouldNotBeNull();
    // }
    // /// <summary>
    // /// Executes Constructor_WithNullProductCreationDto_ShouldThrowArgumentNullException operation.
    // /// </summary>

    // [Fact]
    // public void Constructor_WithNullProductCreationDto_ShouldThrowArgumentNullException()
    // {
    //     // Arrange
    //     ProductCreationDto nullDto = null!;

    //     // Act & Assert
    //     Should.Throw<ArgumentNullException>(() => new CreateProductCommand(nullDto));
    // }
    /// <summary>
    /// Executes Constructor_ShouldSetProductFromDto operation.
    /// </summary>

    [Fact]
    public void Constructor_ShouldSetProductFromDto()
    {
        // Arrange
        var productCreationDto = CreateValidProductCreationDto();

        // Act
        var command = new CreateProductCommand(productCreationDto);

        // Assert
        command.Product.ShouldBeSameAs(productCreationDto.Product);
        command.Product.PartNumber.ShouldBe("TEST-PART-123");
        command.Product.ProductName.ShouldBe("Test Product");
        command.Product.CustomerName.ShouldBe("Test Customer");
    }
    /// <summary>
    /// Executes Constructor_ShouldSetRuleFromDto operation.
    /// </summary>

    [Fact]
    public void Constructor_ShouldSetRuleFromDto()
    {
        // Arrange
        var productCreationDto = CreateValidProductCreationDto();

        // Act
        var command = new CreateProductCommand(productCreationDto);

        // Assert
        command.Rule.ShouldBeSameAs(productCreationDto.Rule);
        command.Rule.RuleJson.ShouldBe("{\"temperature\": 85.5, \"enabled\": true}");
        command.Rule.Name.ShouldBe("Test Rule");
    }
    /// <summary>
    /// Executes Constructor_ShouldSetRecipeFromDto operation.
    /// </summary>

    [Fact]
    public void Constructor_ShouldSetRecipeFromDto()
    {
        // Arrange
        var productCreationDto = CreateValidProductCreationDto();

        // Act
        var command = new CreateProductCommand(productCreationDto);

        // Assert
        command.Recipe.ShouldBeSameAs(productCreationDto.Recipe);
        command.Recipe.CycleTimeMinimum.ShouldBe(50);
        command.Recipe.CycleTimeMaximum.ShouldBe(100);
    }
    /// <summary>
    /// Executes Constructor_ShouldCreateWorkFlowsFromMachines operation.
    /// </summary>

    [Fact]
    public void Constructor_ShouldCreateWorkFlowsFromMachines()
    {
        // Arrange
        var productCreationDto = CreateValidProductCreationDto();
        productCreationDto.Machines = new List<int> { 100, 200, 300 };

        // Act
        var command = new CreateProductCommand(productCreationDto);

        // Assert
        command.WorkFlows.ShouldNotBeNull();
        command.WorkFlows.Count.ShouldBe(4); // 0->100, 100->200, 200->300, 300->0
    }
    /// <summary>
    /// Executes Properties_WhenSet_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSet_ShouldReturnCorrectValues()
    {
        // Arrange
        var command = new CreateProductCommand(CreateValidProductCreationDto());
        var newProduct = new ProductDto { PartNumber = "NEW-PART" };
        var newRule = new RuleDto { Name = "New Rule" };
        var newRecipe = new RecipeDto { CycleTimeMinimum = 25 };
        var newWorkFlows = new List<WorkFlowDto> { new() { LastMachineId = 100 } };

        // Act
        command.Product = newProduct;
        command.Rule = newRule;
        command.Recipe = newRecipe;
        command.WorkFlows = newWorkFlows;

        // Assert
        command.Product.ShouldBeSameAs(newProduct);
        command.Rule.ShouldBeSameAs(newRule);
        command.Recipe.ShouldBeSameAs(newRecipe);
        command.WorkFlows.ShouldBeSameAs(newWorkFlows);
    }

    // CreateWorkFlowDtos Static Method Tests
    /// <summary>
    /// Executes CreateWorkFlowDtos_WithNullMachines_ShouldReturnEmptyList operation.
    /// </summary>

    [Fact]
    public void CreateWorkFlowDtos_WithNullMachines_ShouldReturnEmptyList()
    {
        // Arrange
        IEnumerable<int>? nullMachines = null!;

        // Act
        var result = CreateProductCommand.CreateWorkFlowDtos(nullMachines!);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }
    /// <summary>
    /// Executes CreateWorkFlowDtos_WithEmptyMachines_ShouldReturnEmptyList operation.
    /// </summary>

    [Fact]
    public void CreateWorkFlowDtos_WithEmptyMachines_ShouldReturnEmptyList()
    {
        // Arrange
        var emptyMachines = new List<int>();

        // Act
        var result = CreateProductCommand.CreateWorkFlowDtos(emptyMachines);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }
    /// <summary>
    /// Executes CreateWorkFlowDtos_WithSingleMachine_ShouldCreateCorrectFlow operation.
    /// </summary>

    [Fact]
    public void CreateWorkFlowDtos_WithSingleMachine_ShouldCreateCorrectFlow()
    {
        // Arrange
        var machines = new List<int> { 100 };

        // Act
        var result = CreateProductCommand.CreateWorkFlowDtos(machines);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);

        // Flow: 0 -> 100 -> 0
        result[0].LastMachineId.ShouldBe(0);
        result[0].NextMachineId.ShouldBe(100);
        result[0].RuleId.ShouldBe(2005);

        result[1].LastMachineId.ShouldBe(100);
        result[1].NextMachineId.ShouldBe(0);
        result[1].RuleId.ShouldBe(2005);
    }
    /// <summary>
    /// Executes CreateWorkFlowDtos_WithMultipleMachines_ShouldCreateCircularFlow operation.
    /// </summary>

    [Fact]
    public void CreateWorkFlowDtos_WithMultipleMachines_ShouldCreateCircularFlow()
    {
        // Arrange
        var machines = new List<int> { 300, 100, 200 }; // Unsorted intentionally

        // Act
        var result = CreateProductCommand.CreateWorkFlowDtos(machines);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(4);

        // Should be sorted: 0 -> 100 -> 200 -> 300 -> 0
        result[0].LastMachineId.ShouldBe(0);
        result[0].NextMachineId.ShouldBe(100);

        result[1].LastMachineId.ShouldBe(100);
        result[1].NextMachineId.ShouldBe(200);

        result[2].LastMachineId.ShouldBe(200);
        result[2].NextMachineId.ShouldBe(300);

        result[3].LastMachineId.ShouldBe(300);
        result[3].NextMachineId.ShouldBe(0);

        // All should have the same RuleId
        result.ShouldAllBe(wf => wf.RuleId == 2005);
    }
    /// <summary>
    /// Executes CreateWorkFlowDtos_WithDuplicateMachines_ShouldRemoveDuplicates operation.
    /// </summary>

    [Fact]
    public void CreateWorkFlowDtos_WithDuplicateMachines_ShouldRemoveDuplicates()
    {
        // Arrange
        var machines = new List<int> { 100, 200, 100, 300, 200 }; // Duplicates

        // Act
        var result = CreateProductCommand.CreateWorkFlowDtos(machines);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(4); // Should be 3 unique machines + start/end

        // Verify no duplicate machine IDs in the flow
        var machineIds = result.SelectMany(wf => new[] { wf.LastMachineId, wf.NextMachineId })
                              .Where(id => id != 0)
                              .Distinct()
                              .ToList();
        machineIds.Count.ShouldBe(3); // 100, 200, 300
        machineIds.ShouldContain(100!);
        machineIds.ShouldContain(200!);
        machineIds.ShouldContain(300!);
    }
    /// <summary>
    /// Executes CreateWorkFlowDtos_WithVariousMachineCount_ShouldCreateCorrectFlowCount operation.
    /// </summary>
    /// <param name="machines">The machines.</param>
    /// <param name="expectedFlowCount">The expectedFlowCount.</param>

    [Theory]
    [InlineData(new int[] { 100 }, 2)]           // Single machine: 0->100->0
    [InlineData(new int[] { 100, 200 }, 3)]       // Two machines: 0->100->200->0
    [InlineData(new int[] { 100, 200, 300 }, 4)]   // Three machines: 0->100->200->300->0
    [InlineData(new int[] { 100, 200, 300, 400 }, 5)]  // Four machines: 0->100->200->300->400->0
    public void CreateWorkFlowDtos_WithVariousMachineCount_ShouldCreateCorrectFlowCount(int[] machines, int expectedFlowCount)
    {
        // Using parameters: machines, expectedFlowCount
        _ = machines; // xUnit1026 fix
        _ = expectedFlowCount; // xUnit1026 fix
        // Using parameters: machines, expectedFlowCount
        _ = machines; // xUnit1026 fix
        _ = expectedFlowCount; // xUnit1026 fix
        // Using parameters: machines, expectedFlowCount
        _ = machines; // xUnit1026 fix
        _ = expectedFlowCount; // xUnit1026 fix
        // Using parameters: machines, expectedFlowCount
        _ = machines; // xUnit1026 fix
        _ = expectedFlowCount; // xUnit1026 fix
        // Using parameters: machines, expectedFlowCount
        _ = machines; // xUnit1026 fix
        _ = expectedFlowCount; // xUnit1026 fix
        // Act
        var result = CreateProductCommand.CreateWorkFlowDtos(machines);

        // Assert
        result.Count.ShouldBe(expectedFlowCount);
        result.ShouldAllBe(wf => wf.RuleId == 2005);
    }
    /// <summary>
    /// Executes CreateWorkFlowDtos_ShouldAlwaysStartAndEndWithZero operation.
    /// </summary>

    [Fact]
    public void CreateWorkFlowDtos_ShouldAlwaysStartAndEndWithZero()
    {
        // Arrange
        var machines = new List<int> { 100, 200, 300 };

        // Act
        var result = CreateProductCommand.CreateWorkFlowDtos(machines);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThan(0);

        // First workflow should start from 0
        result.First().LastMachineId.ShouldBe(0);

        // Last workflow should end at 0
        result.Last().NextMachineId.ShouldBe(0);
    }
    /// <summary>
    /// Executes CreateWorkFlowDtos_ShouldSortMachinesInAscendingOrder operation.
    /// </summary>

    [Fact]
    public void CreateWorkFlowDtos_ShouldSortMachinesInAscendingOrder()
    {
        // Arrange
        var machines = new List<int> { 500, 100, 300, 200 }; // Unsorted

        // Act
        var result = CreateProductCommand.CreateWorkFlowDtos(machines);

        // Assert
        result.ShouldNotBeNull();

        // Extract the machine sequence (excluding start/end 0s)
        var machineSequence = result.Skip(1).Select(wf => wf.LastMachineId).ToList();
        machineSequence.ShouldBe(new[] { 100, 200, 300, 500 });
    }
    /// <summary>
    /// Executes CreateWorkFlowDtos_WithIndustrialMachineIds_ShouldWorkCorrectly operation.
    /// </summary>

    [Fact]
    public void CreateWorkFlowDtos_WithIndustrialMachineIds_ShouldWorkCorrectly()
    {
        // Arrange - Industrial scenario with realistic machine IDs
        var machines = new List<int> { 100, 300, 200, 500, 400 }; // Assembly line machines

        // Act
        var result = CreateProductCommand.CreateWorkFlowDtos(machines);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(6); // 5 machines + start/end

        // Verify industrial flow sequence
        result[0].LastMachineId.ShouldBe(0);       // Start
        result[0].NextMachineId.ShouldBe(100);    // First machine

        result[1].LastMachineId.ShouldBe(100);
        result[1].NextMachineId.ShouldBe(200);    // Sorted order

        result[2].LastMachineId.ShouldBe(200);
        result[2].NextMachineId.ShouldBe(300);

        result[3].LastMachineId.ShouldBe(300);
        result[3].NextMachineId.ShouldBe(400);

        result[4].LastMachineId.ShouldBe(400);
        result[4].NextMachineId.ShouldBe(500);

        result[5].LastMachineId.ShouldBe(500);
        result[5].NextMachineId.ShouldBe(0);       // End

        // All workflows should have the standard RuleId
        result.ShouldAllBe(wf => wf.RuleId == 2005);
    }


    private static ProductCreationDto CreateValidProductCreationDto()
    {
        return new ProductCreationDto
        {
            Product = new ProductDto
            {
                PartNumber = "TEST-PART-123",
                ProductName = "Test Product",
                CustomerName = "Test Customer",
                Description = "Test product for unit testing",
                CustomerId = 1,
                LineId = 1,
                IsActive = 1,
                Version = 1
            },
            Machines = new List<int> { 100, 200, 300 },
            Rule = new RuleDto
            {
                RuleJson = "{\"temperature\": 85.5, \"enabled\": true}",
                Name = "Test Rule",
                Description = "Test rule for validation",
                Version = 1,
                IsActive = true
            },
            Recipe = new RecipeDto
            {
                CycleTimeMinimum = 50,
                CycleTimeMaximum = 100
            }
        };
    }
}
