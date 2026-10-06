// <copyright file="ProductsModal.razor.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndQuestResults;
using IndTrace.Application.ConfigStations.Queries.GetConfigStationList;
using IndTrace.Application.Models.Interfaces;
using IndTrace.Application.Products.Queries.GetProductDetail;
using IndTrace.Domain.Models;
using IndTrace.UI.Models.Products;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using MudBlazor;

namespace IndTrace.Components.Area.Products
{
    /// <summary>
    /// Provides modal dialog functionality for managing products with drag-and-drop workflow configuration.
    /// </summary>
    public partial class ProductsModal
    {
        /// <summary>
        /// Gets or sets the product service for data operations.
        /// </summary>
        [Parameter]
        public required IProductService ProductService { get; set; }

        /// <summary>
        /// Gets or sets the application configuration containing system data.
        /// </summary>
        [Parameter]
        public ApplicationConfiguration? ApplicationConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the view state for managing product operations.
        /// </summary>
        [Parameter]
        public required ProductsViewState ProductState { get; set; }

        /// <summary>
        /// Gets or sets the dictionary mapping machine IDs to their display names.
        /// </summary>
        [Parameter]
        public Dictionary<int, string> MachineNames { get; set; } = [];

        /// <summary>
        /// Gets or sets the logger instance for logging operations.
        /// </summary>
        [Parameter]
        public required ILogger Logger { get; set; }

        /// <summary>
        /// Gets or sets the index state for tracking item positions in drag-and-drop zones.
        /// </summary>
        [Parameter]
        public Dictionary<string, Dictionary<ProductMachineItem, int>> IndexState { get; set; } = new();

        /// <summary>
        /// Gets or sets the event callback triggered when a product is saved.
        /// </summary>
        [Parameter]
        public EventCallback<(string, Dictionary<ProductMachineItem, int>)> OnProductSaved { get; set; }

        /// <summary>
        /// Gets or sets the reference to the MudDropContainer component.
        /// </summary>
        private MudDropContainer<ProductMachineItem>? DropContainer { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the modal has been initialized.
        /// </summary>
        public bool IsInitialized { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the save button should be enabled.
        /// </summary>
        public bool EnableSaveButton { get; set; }

        private readonly NewProductForm newProductSectionModel = new("new section");
        private string SelectedCustomer { get; set; } = string.Empty;

        /// <summary>
        /// Shows the add product section.
        /// </summary>
        /// <param name="obj">The mouse event arguments.</param>
        private void ShowAddProduct(MouseEventArgs obj)
        {
        }

        /// <summary>
        /// Cancels the add product operation.
        /// </summary>
        private void CancelAddProduct()
        {
            this.NewProduct = null;
        }

        /// <summary>
        /// Saves a new product model asynchronously.
        /// </summary>
        /// <param name="newProduct">The new product DTO.</param>
        private async Task SaveNewModel(ProductDto newProduct)
        {
            if (newProduct is null) return;

            this.NewProduct = newProduct;
            this.Logger.LogInformation(this.NewProduct.ProductName);

            var machines = this.GetMachineWorkflowForProduct(this.NewProduct.ProductName);

            if (machines != null)
            {
                this.NewProduct.Machines = machines.Select(m => m.MachineId);
            }

            var result = await this.ProductState.AddProduct(this.ProductService, this.NewProduct);

            this.Result = result;

            this.Logger.LogInformation(this.NewProduct.CustomerName);

            this.Logger.LogInformation("Result: {Result}", this.Result.ToString());

            if (this.Result.IsSuccess)
            {
                this.NewProduct = null;
                this.EnableSaveButton = false;
            }

            this.OpenAddNewProduct();
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
            // ProcessAsync item dropped events
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
        /// Gets or sets a value indicating whether the save operation is enabled.
        /// </summary>
        public bool SaveEnabled { get; set; }

        /// <summary>
        /// Gets or sets the result of the last operation performed.
        /// </summary>
        public Result? Result { get; set; }

        /// <summary>
        /// Handles the event when a new product is added.
        /// </summary>
        /// <param name="newProduct">The new product DTO.</param>
        private void NewProductAdded(ProductDto newProduct)
        {
            this.NewProduct = newProduct;
            this.ProductState.NewProductAdded(newProduct);

            this.EnableSaveButton = true;
        }

        /// <summary>
        /// Handles the selection of a product in the modal.
        /// </summary>
        /// <param name="selectedProduct">The selected product data transfer object.</param>
        public void ProductSelected(ProductDto selectedProduct)
        {
            this.ProductState.ProductSelected(selectedProduct);
        }

        /// <summary>
        /// Handles the event when an item is dropped into a product zone.
        /// </summary>
        /// <param name="info">The drop info for the product machine item.</param>
        private void OnItemDroppedIntoProduct(MudItemDropInfo<ProductMachineItem> info)
        {
            if (info.Item is null) return;

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

            if (this.DropContainer is null) return [];

            var items = this.DropContainer.Items.Where(i => i.Status == productName);

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
            if (this.DropContainer is null) return;

            foreach (var item in this.DropContainer.Items)
            {
                this.Logger.LogInformation("Item: {Name} Status: {Status} MachineId: {MachineId}", item.Name, item.Status, item.MachineId);
            }

            this.Logger.LogInformation(this.DropContainer.Items.Count().ToString());

            var mudDropZones = this.DropContainer.GetDropZones<ProductMachineItem>();

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
            if (this.DropContainer is null) return;

            var mudDropZones = this.DropContainer.GetDropZones<ProductMachineItem>();
            var state = new Dictionary<string, Dictionary<ProductMachineItem, int>>();

            foreach (var (zone, dropZone) in mudDropZones)
            {
                var indices = dropZone.GetIndexs();
                state.Add(zone, indices);
            }

            this.ProductState.RemoveDuplicateMachineItems();

            this.IndexState = state;
        }

        /// <summary>
        /// Handles the event when an item is dropped into the discard zone.
        /// </summary>
        /// <param name="info">The drop info for the product machine item.</param>
        private void OnItemDroppedIntoDiscard(MudItemDropInfo<ProductMachineItem> info)
        {
            if (info.Item is null || this.ProductState is null) return;
            if (info.Item.Status != "Catalog")
            {
                this.ProductState.RemoveItemFromState(info.Item);
            }
            else
            {
                this.ProductState.HandleCatalogItemDropIntoDiscard(info);
            }
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
        /// Opens the add new product section.
        /// </summary>
        private void OpenAddNewProduct()
        {
        }

        /// <summary>
        /// Deletes a product section.
        /// </summary>
        /// <param name="section">The product definition section to delete.</param>
        private void DeleteSection(ProductDefinition section)
        {
            var productDefinition = new ProductDefinition(section.Name, section.NewMachine, section.NewMachineName);
            this.ProductState.DeleteSection(section);
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
}