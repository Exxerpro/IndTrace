// <copyright file="ProductWorkflowEdit.razor.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.ConfigStations.Queries.GetConfigStationList;
using IndTrace.Application.Machines.Queries.GetMachinesList;
using IndTrace.Application.Models.Interfaces;
using IndTrace.Application.Products.Queries.GetProductDetail;
using IndTrace.Application.WorkFlows.Dto;
using IndTrace.Domain.Models;
using IndTrace.UI.Models.Products;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using MudBlazor;

namespace IndTrace.Components.Area.Products;

/// <summary>
/// Provides editing functionality for product workflows, including drag-and-drop machine configuration.
/// </summary>
public partial class ProductWorkflowEdit
{
    /// <summary>
    /// Gets or sets the Mud dialog instance for modal operations.
    /// </summary>
    [CascadingParameter]
    private IMudDialogInstance? MudDialog { get; set; }

    /// <summary>
    /// Gets or sets the application configuration containing machine and workflow data.
    /// </summary>
    [Parameter]
    public ApplicationConfiguration? ApplicationConfiguration { get; set; }

    /// <summary>
    /// Gets or sets the view state for managing product workflow operations.
    /// </summary>
    [Parameter]
    public ProductsViewState? ProductsViewState { get; set; }   // reassigned in OnInitializedAsync once ApplicationConfiguration is valid

    /// <summary>
    /// Gets or sets the dictionary mapping machine IDs to their display names.
    /// </summary>
    [Parameter]
    public Dictionary<int, string> MachineNames { get; set; } = new();

    private Dictionary<int, string> filteredMachines = [];

    /// <summary>
    /// Gets or sets the index state for tracking item positions in drag-and-drop zones.
    /// </summary>
    [Parameter]
    public required Dictionary<string, Dictionary<ProductMachineItem, int>> IndexState { get; set; }

    /// <summary>
    /// Gets or sets the list of product data transfer objects.
    /// </summary>
    [Parameter]
    public required List<ProductDto> ListProductsDto { get; set; }

    /// <summary>
    /// Gets or sets the list of workflow data transfer objects.
    /// </summary>
    [Parameter]
    public required List<WorkFlowDto> ListWorkFlowsDto { get; set; }

    /// <summary>
    /// Gets or sets the product data transfer object being edited.
    /// </summary>
    [Parameter]
    public required ProductDto ProductDto { get; set; }

    /// <summary>
    /// Gets or sets the product service for data operations.
    /// </summary>
    [Parameter]
    public required IProductService ProductService { get; set; }

    /// <summary>
    /// Gets or sets the currently selected customer name.
    /// </summary>
    [Parameter]
    public required string SelectedCustomer { get; set; }

    /// <summary>
    /// Gets or sets the callback function executed when workflow configuration is complete.
    /// </summary>
    [Parameter]
    public Func<List<MachineDto>, Task>? OnConfigured { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the save button should be enabled.
    /// </summary>
    public bool EnableSaveButton { get; set; }

    private MudDropContainer<ProductMachineItem>? dropContainer;

    /// <summary>
    /// Gets or sets the authentication state provider.
    /// </summary>
    [Inject]
    private AuthenticationStateProvider? AuthProvider { get; set; }


    /// <inheritdoc/>
    protected override Task OnInitializedAsync()
    {
        if (this.ApplicationConfiguration is null || this.ListProductsDto is null || this.ApplicationConfiguration.WorkFlows is null)
        {
            throw new InvalidOperationException("ProductWorkflowView requires fully initialized parameters.");
        }

        if (string.IsNullOrWhiteSpace(this.SelectedCustomer))
        {
            this.snackbar.Add("New customer detected — all machines available", Severity.Info);
        }
        else
        {
            this.snackbar.Add($"Showing machines for {this.SelectedCustomer}", Severity.Info);
        }

        if (!string.IsNullOrWhiteSpace(this.SelectedCustomer))
        {
            this.filteredMachines = this.ApplicationConfiguration.MachineProductCompatibility
                .Where(m => m.CustomerName.Equals(this.SelectedCustomer.Trim(), StringComparison.OrdinalIgnoreCase))
                .GroupBy(m => m.MachineId)
                .Select(g => g.First()) // To ensure unique MachineId
                .ToDictionary(m => m.MachineId, m => m.MachineName);

            if (this.filteredMachines.Count == 0)
            {
                // New customer — no mapping yet, fallback to full list
                this.filteredMachines = this.MachineNames;
            }
        }
        else
        {
            this.filteredMachines = this.MachineNames;
        }

        this.ProductsViewState = new ProductsViewState(this.ApplicationConfiguration);
        this.ProductsViewState.SetMachineNames(this.filteredMachines);

        this.ProductsViewState.InitializeProducts(this.ListProductsDto);
        this.ProductsViewState.InitializeMachineItems(this.ListProductsDto, this.ApplicationConfiguration.WorkFlows, this.filteredMachines);

        return Task.CompletedTask;
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
    private ProductDto? NewProduct { get; set; } = new ProductDto();
    /// <summary>
    /// Gets or sets a value indicating whether the save operation is enabled.
    /// </summary>
    public bool SaveEnabled { get; set; }
    /// <summary>
    /// Gets or sets the result of the last operation performed.
    /// </summary>
    public Result? Result { get; set; }

    /// <summary>
    /// Handles the event when an item is dropped into a product zone.
    /// </summary>
    /// <param name="info">The drop info for the product machine item.</param>
    private void OnItemDroppedIntoProduct(MudItemDropInfo<ProductMachineItem> info)
    {
        if (info.Item is null || this.ProductsViewState is null) return;

        if (info.Item.Status == "Catalog")
        {
            this.ProductsViewState.HandleCatalogItemDrop(info);
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

        if (this.dropContainer is null) return [];

        var items = this.dropContainer.Items.Where(i => i.Status == productName);

        foreach (var item in items)
        {
            this.Logger.LogInformation("  Item: {Name}, Status: {Status}, MachineId: {MachineId}, Index: {Index}", item.Name, item.Status, item.MachineId, item.IndexInZone);
        }

        return items;
    }

    /// <summary>
    /// Logs the state of the drop container for debugging.
    /// </summary>
    private void LogDropContainerState()
    {
        if (this.dropContainer is null) return;

        foreach (var item in this.dropContainer.Items)
        {
            this.Logger.LogInformation("Item: {Name} Status: {Status} MachineId: {MachineId}", item.Name, item.Status, item.MachineId);
        }

        this.Logger.LogInformation(this.dropContainer.Items.Count().ToString());

        var mudDropZones = this.dropContainer.GetDropZones<ProductMachineItem>();

        foreach (var (zone, dropZone) in mudDropZones)
        {
            var indices = dropZone.GetIndexs();
            foreach (var (item, index) in indices)
            {
                this.Logger.LogInformation("  Item: {Name}, Status: {Status}, Zone: {Zone}, MachineId: {MachineId}, Index: {Index}", item.Name, item.Status, zone, item.MachineId, index);
            }
        }
    }

    /// <summary>
    /// Updates the index state and refreshes the UI.
    /// </summary>
    private void UpdateIndexStateAndRefreshUI()
    {
        if (this.dropContainer is null || this.ProductsViewState is null) return;

        var mudDropZones = this.dropContainer.GetDropZones<ProductMachineItem>();
        var state = new Dictionary<string, Dictionary<ProductMachineItem, int>>();

        foreach (var (zone, dropZone) in mudDropZones)
        {
            var indices = dropZone.GetIndexs();
            state.Add(zone, indices);
        }

        this.ProductsViewState.RemoveDuplicateMachineItems();

        this.IndexState = state;
        this.InvokeAsync(this.StateHasChanged);
    }

    /// <summary>
    /// Handles the event when an item is dropped into the discard zone.
    /// </summary>
    /// <param name="info">The drop info for the product machine item.</param>
    private void OnItemDroppedIntoDiscard(MudItemDropInfo<ProductMachineItem> info)
    {
        if (this.ProductsViewState is null) return;

        if (info.Item is not null && info.Item.Status != "Catalog")
        {
            this.ProductsViewState.RemoveItemFromState(info.Item);
        }
        else
        {
            this.ProductsViewState.HandleCatalogItemDropIntoDiscard(info);
        }

        this.InvokeAsync(this.StateHasChanged);
    }

    /// <summary>
    /// Gets the current user's name asynchronously.
    /// </summary>
    /// <returns>The current user's name, or "System" if not available.</returns>
    private async Task<string> GetCurrentUserNameAsync()
    {
        if (this.AuthProvider is null)
        {
            return "System";
        }

        var authState = await this.AuthProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        return user.Identity?.Name ?? "System";
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

    /// <summary>
    /// Cancels the dialog.
    /// </summary>
    private void CancelDialog()
    {
        this.MudDialog?.Cancel();
    }

    /// <summary>
    /// Submits the workflow configuration asynchronously.
    /// </summary>
    private async Task SubmitWorkflow()
    {
        if (this.dropContainer is null) return;

        var machines = this.dropContainer
            .GetDropZones<ProductMachineItem>()
            .GetValueOrDefault(this.ProductDto.ProductName)?
            .GetIndexs()
            .Select(kvp => kvp.Key?.Machine)  // use null-safe access
            .Where(m => m is not null)
            .Distinct()
            .ToList();

        if (machines is null || machines.Count == 0)
        {
            this.Logger.LogWarning("No valid machines configured for product '{ProductName}'", this.ProductDto.ProductName);
            return;
        }

        // Update NewProduct with sanitized machine IDs
        if (this.NewProduct is not null)
        {
            this.NewProduct.Machines = machines
                .OfType<MachineDto>()
                .Select(m => m.MachineId)
                .ToList();
        }

        // Log result
        this.Logger.LogInformation("Submitting workflow for product '{ProductName}' with {Count} machines", this.ProductDto.ProductName, machines.Count);
        this.snackbar.Add($"Workflow for '{this.ProductDto.ProductName}' submitted with {machines.Count} machine(s).", Severity.Success);

        // Enable save if workflow was valid
        this.SaveEnabled = true;
        this.EnableSaveButton = true;

        // Fire callback
        if (this.OnConfigured is not null)
        {
            var safeMachineList = machines.OfType<MachineDto>().ToList(); // ensures non-null list

            var cb = this.OnConfigured;
            if (cb is not null)
            {
                await cb.Invoke(safeMachineList);
            }
        }

        // Close dialog if open
        this.MudDialog?.Close(DialogResult.Ok(machines));
    }
}
