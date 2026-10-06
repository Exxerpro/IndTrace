// <copyright file="ProductWorkflowView.razor.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.ConfigStations.Queries.GetConfigStationList;
using IndTrace.Application.Models.Interfaces;
using IndTrace.Application.Products.Queries.GetProductDetail;
using IndTrace.UI.Models.Products;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using MudBlazor;

namespace IndTrace.Components.Area.Products;

/// <summary>
/// Provides view functionality for product workflows with drag-and-drop machine configuration.
/// </summary>
public partial class ProductWorkflowView
{
    /// <summary>
    /// Gets or sets the Mud dialog instance for modal operations.
    /// </summary>
    [CascadingParameter]
    private IMudDialogInstance? MudDialog { get; set; }

    /// <summary>
    /// Gets or sets the application configuration containing system data.
    /// </summary>
    [Parameter]
    public ApplicationConfiguration? ApplicationConfiguration { get; set; }

    /// <summary>
    /// Gets or sets the view state for managing product workflow operations.
    /// </summary>
    [Parameter]
    public ProductsViewState? ProductState { get; set; }  // init in OnInitialized

    /// <summary>
    /// Gets or sets the dictionary mapping machine IDs to their display names.
    /// </summary>
    [Parameter]
    public Dictionary<int, string> MachineNames { get; set; } = [];

    /// <summary>
    /// Gets or sets the index state for tracking item positions in drag-and-drop zones.
    /// </summary>
    [Parameter]
    public Dictionary<string, Dictionary<ProductMachineItem, int>> IndexState { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of product data transfer objects.
    /// </summary>
    [Parameter]
    public List<ProductDto> ListProductsDto { get; set; } = new();

    /// <summary>
    /// Gets or sets the product data transfer object being viewed.
    /// </summary>
    [Parameter]
    public ProductDto? ProductDto { get; set; }

    /// <summary>
    /// Gets or sets the product service for data operations.
    /// </summary>
    [Parameter]
    public required IProductService ProductService { get; set; }

    private MudDropContainer<ProductMachineItem>? dropContainer;

    private string SelectedCustomer { get; set; } = string.Empty;

    /// <summary>
    /// Gets the filtered list of product definitions for the current product.
    /// </summary>
    private IEnumerable<ProductDefinition> FilteredProducts =>
        (this.ProductState?.ProductsView ?? Enumerable.Empty<ProductDefinition>())
            .Where(p => p.Name == (this.ProductDto?.ProductName ?? string.Empty));

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        if (this.ApplicationConfiguration is null || this.ListProductsDto is null || this.ApplicationConfiguration.WorkFlows is null)
        {
            throw new InvalidOperationException("ProductWorkflowView requires fully initialized parameters.");
        }

        this.ProductState = new ProductsViewState(this.ApplicationConfiguration);
        this.ProductState.SetMachineNames(this.MachineNames ?? []);

        this.ProductState.InitializeProducts(this.ListProductsDto);
        this.ProductState.InitializeMachineItems(this.ListProductsDto, this.ApplicationConfiguration.WorkFlows, this.MachineNames);
    }

    /// <summary>
    /// Handles key down events for dialog actions.
    /// </summary>
    /// <param name="e">The keyboard event arguments.</param>
    private void HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Escape")
        {
            this.MudDialog?.Cancel();
        }
    }

    /// <summary>
    /// Updates product information based on the current new product.
    /// </summary>
    private void UpdateProductInformation()
    {
        if (this.NewProduct is null || string.IsNullOrEmpty(this.NewProduct.ProductName)) return;
        var machines = this.GetMachineWorkflowForProduct(this.NewProduct.ProductName);

        if (machines == null) return;

        this.NewProduct.Machines = machines.Select(m => m.MachineId);
        this.SaveEnabled = this.NewProduct.Machines.Any();
    }

    /// <summary>
    /// Handles the event when a product is updated via drag-and-drop.
    /// </summary>
    /// <param name="info">The drop info for the product machine item.</param>
    private void ProductUpdated(MudItemDropInfo<ProductMachineItem> info)
    {
        if (info.Item is null) return;
        info.Item.SourceZoneIdentifier = info.DropzoneIdentifier;
        // HandleAsync item dropped events
        switch (info.DropzoneIdentifier)
        {
            case "Catalog":
                this.OnItemDroppedIntoCatalog(info);
                break;

            case "Discard":
                this.OnItemDroppedIntoDiscard(info);
                break;

            default:
                this.OnItemDroppedIntoProduct(info);
                break;
        }
    }

    /// <summary>
    /// Gets or sets the new product being added.
    /// </summary>
    private ProductDto? NewProduct { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether saving is enabled.
    /// </summary>
    public bool SaveEnabled { get; set; }

    /// <summary>
    /// Handles the event when an item is dropped into a product zone.
    /// </summary>
    /// <param name="info">The drop info for the product machine item.</param>
    private void OnItemDroppedIntoProduct(MudItemDropInfo<ProductMachineItem> info)
    {
        if (info.Item is null || this.ProductState is null) return;

        if (info.Item.Status == "Catalog")
        {
            this.ProductState.HandleCatalogItemDrop(info);
        }

        this.UpdateProductInformation();
        this.UpdateIndexStateAndRefreshUI();
    }

    /// <summary>
    /// Gets the machine workflow for a given product name.
    /// </summary>
    /// <param name="productName">The product name.</param>
    /// <returns>An enumerable of product machine items.</returns>
    private IEnumerable<ProductMachineItem> GetMachineWorkflowForProduct(string productName)
    {
        this.Logger.LogInformation("Items Information");

        var items = (this.dropContainer?.Items ?? Enumerable.Empty<ProductMachineItem>())
            .Where(i => i.Status == productName);

        foreach (var item in items)
        {
            this.Logger.LogInformation("  Item: {Name}, Status: {Status}, MachineId: {MachineId}, Index: {Index}", item.Name, item.Status, item.MachineId, item.IndexInZone);
        }

        return items;
    }

    /// <summary>
    /// Updates the index state and refreshes the UI.
    /// </summary>
    private void UpdateIndexStateAndRefreshUI()
    {
        if (this.dropContainer is null) return;
        var mudDropZones = this.dropContainer.GetDropZones<ProductMachineItem>();
        var state = new Dictionary<string, Dictionary<ProductMachineItem, int>>();

        foreach (var (zone, dropZone) in mudDropZones)
        {
            var indices = dropZone.GetIndexs();
            state.Add(zone, indices);
        }

        this.ProductState?.RemoveDuplicateMachineItems();

        this.IndexState = state;
        this.InvokeAsync(this.StateHasChanged);
    }

    /// <summary>
    /// Handles the event when an item is dropped into the discard zone.
    /// </summary>
    /// <param name="info">The drop info for the product machine item.</param>
    private void OnItemDroppedIntoDiscard(MudItemDropInfo<ProductMachineItem> info)
    {
        if (this.ProductState is null || info.Item is null) return;
        if (info.Item.Status != "Catalog")
        {
            this.ProductState.RemoveItemFromState(info.Item);
        }
        else
        {
            this.ProductState.HandleCatalogItemDropIntoDiscard(info);
        }

        this.InvokeAsync(this.StateHasChanged);
    }

    /// <summary>
    /// Handles the event when an item is dropped into the catalog zone.
    /// </summary>
    /// <param name="info">The drop info for the product machine item.</param>
    private void OnItemDroppedIntoCatalog(MudItemDropInfo<ProductMachineItem> info)
    {
        // No specific action required for the catalog drop
    }

    /// <summary>
    /// Determines whether a product machine item can be dropped.
    /// </summary>
    /// <param name="item">The product machine item.</param>
    /// <returns>True if the item can be dropped; otherwise, false.</returns>
    private bool CanDrop(ProductMachineItem item)
    {
        return true;
    }
}
