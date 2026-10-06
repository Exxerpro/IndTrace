// <copyright file="ProductsViewStateTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.ConfigStations.Queries.GetConfigStationList;
using IndTrace.Application.Models.Interfaces;
using IndTrace.Application.Products.Commands.Create;
using IndTrace.Application.Products.Events;
using IndTrace.Application.Products.Queries.GetProductDetail;
using IndTrace.Application.Products.Services;
using IndTrace.Application.WorkFlows.Dto;
using IndTrace.UI.Models.Products;

namespace IndTrace.Domain.UnitTests.ProductsState;
/// <summary>
/// Represents the ProductsViewStateTests.
/// </summary>

public class ProductsViewStateTests
{
    /// <summary>
    /// Executes CreateWorkFlowDtos_ShouldReturnCorrectWorkFlowDtos_WhenMachinesAreProvided operation.
    /// </summary>
    [Fact]
    public void CreateWorkFlowDtos_ShouldReturnCorrectWorkFlowDtos_WhenMachinesAreProvided()
    {
        // Arrange
        var machines = new List<int> { 800, 500, 400, 700 }; // Example machine IDs in random order

        // Act
        var result = CreateProductCommand.CreateWorkFlowDtos(machines);

        // Assert
        result.ShouldBeEquivalentTo(new List<WorkFlowDto>
        {
            new WorkFlowDto { NextMachineId = 400, LastMachineId = 0, RuleId = 2005 },
            new WorkFlowDto { NextMachineId = 500, LastMachineId = 400, RuleId = 2005 },
            new WorkFlowDto { NextMachineId = 700, LastMachineId = 500, RuleId = 2005 },
            new WorkFlowDto { NextMachineId = 800, LastMachineId = 700, RuleId = 2005 },
            new WorkFlowDto { NextMachineId = 0, LastMachineId = 800, RuleId = 2005 }
        });
    }
    /// <summary>
    /// Executes CreateWorkFlowDtos_ShouldReturnEmptyList_WhenNoMachinesAreProvided operation.
    /// </summary>

    [Fact]
    public void CreateWorkFlowDtos_ShouldReturnEmptyList_WhenNoMachinesAreProvided()
    {
        // Arrange
        var machines = new List<int>(); // Empty list of machines

        // Act
        var result = CreateProductCommand.CreateWorkFlowDtos(machines);

        // Assert
        result.ShouldBeEmpty();
    }
    /// <summary>
    /// Executes CreateWorkFlowDtos_ShouldReturnCorrectSingleWorkFlow_WhenOneMachineIsProvided operation.
    /// </summary>

    [Fact]
    public void CreateWorkFlowDtos_ShouldReturnCorrectSingleWorkFlow_WhenOneMachineIsProvided()
    {
        // Arrange
        var machines = new List<int> { 500 }; // Only one machine

        // Act
        var result = CreateProductCommand.CreateWorkFlowDtos(machines);

        // Assert
        result.ShouldBeEquivalentTo(new List<WorkFlowDto>
        {
            new WorkFlowDto { NextMachineId = 500, LastMachineId = 0, RuleId = 2005 },
            new WorkFlowDto { NextMachineId = 0, LastMachineId = 500, RuleId = 2005 }
        });

    }
    /// <summary>
    /// Executes CreateWorkFlowDtos_ShouldRemoveDuplicates_WhenMachinesContainDuplicates operation.
    /// </summary>


    [Fact]
    public void CreateWorkFlowDtos_ShouldRemoveDuplicates_WhenMachinesContainDuplicates()
    {
        // Arrange
        var machines = new List<int> { 800, 500, 400, 500, 700, 800 }; // Machines with duplicates

        // Act
        var result = CreateProductCommand.CreateWorkFlowDtos(machines);

        // Assert
        result.ShouldBeEquivalentTo(new List<WorkFlowDto>
        {
            new WorkFlowDto { NextMachineId = 400, LastMachineId = 0, RuleId = 2005 },
            new WorkFlowDto { NextMachineId = 500, LastMachineId = 400, RuleId = 2005 },
            new WorkFlowDto { NextMachineId = 700, LastMachineId = 500, RuleId = 2005 },
            new WorkFlowDto { NextMachineId = 800, LastMachineId = 700, RuleId = 2005 },
            new WorkFlowDto { NextMachineId = 0, LastMachineId = 800, RuleId = 2005 }
        });
    }

    /// <summary>
    /// #72 (P0-12): AddProduct must not stamp the hardcoded QA-line-4 barcode rule or the product-1
    /// recipe onto the product being created. It sends unset rule/recipe DTOs (no fabricated master data).
    /// </summary>
    [Fact]
    public async Task AddProduct_ShouldNotEmitHardcodedRuleOrRecipeMasterData()
    {
        // Arrange
        var state = new ProductsViewState(new ApplicationConfiguration());
        var productService = Substitute.For<IProductService>();

        ProductCreationDto? captured = null;
        productService
            .ExecuteCreateProductCommand(Arg.Do<ProductCreationDto>(dto => captured = dto), Arg.Any<CancellationToken>())
            .Returns(Result<ProductCreatedEvent>.Success(new ProductCreatedEvent()));

        var productDto = new ProductDto
        {
            PartNumber = "PN-9999",
            ProductName = "Widget 9999",
            Machines = new List<int> { 100 },
        };

        // Act
        var result = await state.AddProduct(productService, productDto);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        captured.ShouldNotBeNull();

        // No fabricated barcode rule (previously a ~700-char QA / line-4 "WS100" JSON, Version 3).
        captured!.Rule.RuleJson.ShouldBeEmpty();
        captured.Rule.Name.ShouldBeEmpty();
        captured.Rule.RuleJson.ShouldNotContain("QA");
        captured.Rule.RuleJson.ShouldNotContain("WS100");
        captured.Rule.Version.ShouldBe(0);

        // Recipe is not stamped to the hardcoded product-1.
        captured.Recipe.ProductId.ShouldNotBe(1);
        captured.Recipe.ProductId.ShouldBe(0);
    }
}