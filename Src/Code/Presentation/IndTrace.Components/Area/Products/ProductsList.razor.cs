// <copyright file="ProductsList.razor.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.ConfigStations.Queries.GetConfigStationList;
using IndTrace.Application.Products.Queries.GetProductDetail;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace IndTrace.Components.Area.Products
{
    /// <summary>
    /// Provides a list component for displaying and selecting products with filtering capabilities.
    /// </summary>
    public partial class ProductsList
    {
        /// <summary>
        /// Gets or sets the child content to be rendered within the component.
        /// </summary>
        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// Gets or sets the collection of products to display in the list.
        /// </summary>
        [Parameter]
        public IEnumerable<ProductDto> Products { get; set; } = Enumerable.Empty<ProductDto>();

        /// <summary>
        /// Gets or sets the application configuration containing system data.
        /// </summary>
        [Parameter]
        public ApplicationConfiguration? ApplicationConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the currently selected product.
        /// </summary>
        [Parameter]
        public ProductDto SelectedProduct { get; set; } = new();

        /// <summary>
        /// Gets or sets the event callback triggered when a product is selected.
        /// </summary>
        [Parameter]
        public EventCallback<ProductDto> OnProductSelected { get; set; }

        private MudTable<ProductDto>? mudTable;
        private string searchString = string.Empty;
        private int selectedRowNumber = -1;
        private readonly List<string> clickedEvents = [];

        /// <summary>
        /// Gets or sets the currently selected customer name for filtering products.
        /// </summary>
        [Parameter]
        public string SelectedCustomer { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the event callback triggered when the customer filter changes.
        /// </summary>
        [Parameter]
        public EventCallback<string> CustomerFilterChanged { get; set; }

        /// <summary>
        /// Gets or sets the event callback triggered when the selected customer changes.
        /// </summary>
        [Parameter]
        public EventCallback<string> OnSelectedCustomerChanged { get; set; }

        /// <summary>
        /// Gets the filtered list of products based on the selected customer.
        /// </summary>
        private IEnumerable<ProductDto> SelectedProducts =>
            string.IsNullOrWhiteSpace(this.SelectedCustomer)
                ? this.Products
                : this.Products.Where(p => p.CustomerName.Equals(this.SelectedCustomer, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Handles the event when a product is selected.
        /// </summary>
        /// <param name="product">The selected product.</param>
        private async Task SelectedItemChanged(ProductDto product)
        {
            await this.OnProductSelected.InvokeAsync(product);
        }

        /// <summary>
        /// Handles the row click event in the product table.
        /// </summary>
        /// <param name="tableRowClickEventArgs">The event arguments for the row click.</param>
        private void RowClickEvent(TableRowClickEventArgs<ProductDto> tableRowClickEventArgs)
        {
            this.clickedEvents.Add("Row has been clicked");
        }

        /// <summary>
        /// Returns the CSS class for the selected row.
        /// </summary>
        /// <param name="element">The product element.</param>
        /// <param name="rowNumber">The row number.</param>
        /// <returns>The CSS class for the row.</returns>
        private string SelectedRowClassFunc(ProductDto element, int rowNumber)
        {
            if (this.selectedRowNumber == rowNumber)
            {
                this.selectedRowNumber = -1;
                this.clickedEvents.Add("Selected Row: None");
                return string.Empty;
            }

            if (this.mudTable?.SelectedItem == null || !this.mudTable.SelectedItem.Equals(element)) return string.Empty;
            this.selectedRowNumber = rowNumber;
            this.clickedEvents.Add($"Selected Row: {rowNumber}");
            return "selected";
        }

        /// <summary>
        /// Filter function for the product table using the current search string.
        /// </summary>
        /// <param name="element">The product element.</param>
        /// <returns>True if the element matches the filter; otherwise, false.</returns>
        private bool FilterFunc1(ProductDto element) => FilterFunc(element, this.searchString);

        /// <summary>
        /// Static filter function for products based on a search term.
        /// </summary>
        /// <param name="element">The product element.</param>
        /// <param name="searchTerm">The search term.</param>
        /// <returns>True if the element matches the search term; otherwise, false.</returns>
        private static bool FilterFunc(ProductDto element, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return true;
            }

            if (element is null)
            {
                return false;
            }

            return Matches(element.PartNumber, searchTerm) ||
                   Matches(element.CustomerPartNumber, searchTerm) ||
                   Matches(element.CustomerName, searchTerm);
        }

        private static bool Matches(string? value, string searchTerm) =>
            !string.IsNullOrEmpty(value) && value.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the URL for the customer logo image.
        /// </summary>
        /// <param name="customerName">The customer name.</param>
        /// <returns>The URL for the customer logo image.</returns>
        private static string GetCustomerLogoUrl(string customerName)
        {
            if (string.IsNullOrWhiteSpace(customerName))
            {
                return string.Empty;
            }

            return $"images/customers/{customerName.ToLowerInvariant()}.png";
        }

        /// <summary>
        /// Handles the event when the selected customer changes.
        /// </summary>
        /// <param name="selectedCustomer">The selected customer name.</param>
        private async Task OnCustomerChanged(string selectedCustomer)
        {
            this.SelectedCustomer = selectedCustomer;
            await this.CustomerFilterChanged.InvokeAsync(selectedCustomer);
            await this.OnSelectedCustomerChanged.InvokeAsync(selectedCustomer);
        }
    }
}