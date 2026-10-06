// <copyright file="ProductCreationDto.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Products.Services;

using IndTrace.Application.RulesEngine.Dto;
using IndTrace.Domain.Routing.Authoring;

/// <summary>
/// Data transfer object containing all the information required to create a complete product with its associated configuration.
/// </summary>
/// <remarks>
/// This DTO aggregates product information along with its machine assignments, business rules, and recipe specifications.
/// It's used during product creation workflows to ensure all related entities are properly configured together.
/// </remarks>
public class ProductCreationDto
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProductCreationDto"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public ProductCreationDto()
    {
        this.Product = new ProductDto();
        this.Machines = new List<int>();
        this.Rule = new RuleDto();
        this.Recipe = new RecipeDto();
    }

    /// <summary>
    /// Gets or sets the core product information including part number, description, and specifications.
    /// </summary>
    /// <value>A ProductDto containing the basic product data.</value>
    public ProductDto Product { get; set; }

    /// <summary>
    /// Gets or sets the collection of machine IDs that are assigned to process this product.
    /// </summary>
    /// <value>An enumerable collection of machine IDs where this product can be manufactured.</value>
    /// <remarks>
    /// These machine assignments determine which production lines can handle this product type.
    /// </remarks>
    public IEnumerable<int> Machines { get; set; }

    /// <summary>
    /// Gets or sets the business rule associated with this product for validation and processing logic.
    /// </summary>
    /// <value>A RuleDto containing the business rule configuration.</value>
    /// <remarks>
    /// Rules define how the product should be validated, processed, and what criteria must be met
    /// during manufacturing and quality control.
    /// </remarks>
    public RuleDto Rule { get; set; }

    /// <summary>
    /// Gets or sets the recipe information that defines how this product should be manufactured.
    /// </summary>
    /// <value>A RecipeDto containing the manufacturing recipe details.</value>
    /// <remarks>
    /// The recipe contains process parameters, settings, and instructions needed to manufacture
    /// this product according to specifications.
    /// </remarks>
    public RecipeDto Recipe { get; set; }

    /// <summary>
    /// Gets or sets the authored node+edge routing (E11.4-4) for the new product, or <see langword="null"/> when
    /// no route was authored. When present, <c>CreateProductCommandHandler</c> persists the product's
    /// <c>RoutingNode</c> + clean edge rows through the C2 authoring path (fail-closed: an invalid/illegal route
    /// fails the whole create and compensates). When <see langword="null"/>, the product is created without
    /// routing — its route may be authored later via the routing editor.
    /// </summary>
    /// <value>An <see cref="AuthoringRoute"/> carrying the operator's explicit ordered/fork route, or <see langword="null"/>.</value>
    public AuthoringRoute? Route { get; set; }
}