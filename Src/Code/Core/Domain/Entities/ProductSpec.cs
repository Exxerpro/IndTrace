// <copyright file="ProductSpec.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

using IndTrace.Domain.Interfaces;
using IndTrace.Domain.ValueObjects;

/// <summary>
/// Represents a product specification, including machine, tool, recipe, and performance specification details.
/// </summary>
public class ProductSpec : IEntityRoot
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProductSpec"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public ProductSpec()
    {
        this.RecipeType = string.Empty;
        this.PerformanceSpecsName = string.Empty;
        this.Machine = new Machine();
        this.Tooling = new Tooling();
        this.Product = Product.CreateEmpty();
        this.Recipe = new Recipe();
    }

    /// <summary>
    /// Gets or sets the unique identifier for the product specification.
    /// </summary>
    public int ProductSpecId { get; set; }

    /// <summary>
    /// Gets or sets the machine identifier associated with the product specification. Story 35.D2 Cluster 5 (#35):
    /// retyped to the strongly-typed <see cref="MachineId"/> struct (modeled FK to <c>Machine</c>).
    /// </summary>
    public MachineId MachineId { get; set; }

    /// <summary>
    /// Gets or sets the tool identifier associated with the product specification.
    /// </summary>
    public int ToolId { get; set; }

    /// <summary>
    /// Gets or sets the product identifier associated with the product specification. Story 35.D2 Cluster 4 (#35):
    /// retyped from a bare <see cref="int"/> to the strongly-typed <see cref="ValueObjects.ProductId"/> struct so the
    /// modeled FK into <c>Product</c>'s converted PK is type-compatible (an int FK targeting a <c>ProductId</c>
    /// principal key detonates the EF model). Narrows to the raw <c>int</c> via <c>.Value</c> at every read site.
    /// </summary>
    public ProductId ProductId { get; set; }

    /// <summary>
    /// Gets or sets the recipe type for the product specification.
    /// </summary>
    public string RecipeType { get; set; }

    /// <summary>
    /// Gets or sets the recipe identifier associated with the product specification.
    /// </summary>
    public int RecipeId { get; set; }

    /// <summary>
    /// Gets or sets the name of the performance specification.
    /// </summary>
    public string PerformanceSpecsName { get; set; }

    /// <summary>
    /// Gets or sets the performance specification identifier.
    /// </summary>
    public int PerformanceSpecId { get; set; }

    /// <summary>
    /// Gets or sets the machine entity associated with the product specification.
    /// </summary>
    public virtual Machine Machine { get; set; }

    /// <summary>
    /// Gets or sets the tooling entity associated with the product specification.
    /// </summary>
    public virtual Tooling Tooling { get; set; }

    /// <summary>
    /// Gets or sets the product entity associated with the product specification.
    /// </summary>
    public virtual Product Product { get; set; }

    /// <summary>
    /// Gets or sets the recipe entity associated with the product specification.
    /// </summary>
    public virtual Recipe Recipe { get; set; }

    /// <summary>
    /// Returns a string representation of the ProductSpec.
    /// </summary>
    /// <returns>A string containing the product specification ID, recipe type, and performance specification name.</returns>
    // [Fix]
    // CLAUDE
    // Date: 23/08/2025
    // Reason: Added ToString() implementation for better debugging and logging experience
    public override string ToString() => $"ProductSpec {this.ProductSpecId}: {this.RecipeType} - {this.PerformanceSpecsName}";
}