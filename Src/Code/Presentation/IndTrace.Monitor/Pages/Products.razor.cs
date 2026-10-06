// <copyright file="Products.razor.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndQuestResults;
using IndTrace.Application.ConfigStations.Queries.GetConfigStationList;
using IndTrace.Application.Configuration.Services;
using IndTrace.Application.Products.Queries.GetProductDetail;
using IndTrace.Application.WorkFlows.Dto;
using IndTrace.Components.Area.Products;
using IndTrace.Domain.Models;
using IndTrace.UI.Models.Products;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace IndTrace.Monitor.Pages;

// TODO [CRITICAL] ADD FILTER CUSTOMER
// TODO [CRITICAL] APPLY TO CHILD COMPONENTS
// TODO [CRITICAL] SHOW MORE INFORMATION ABOUT THE MACHINES

/// <summary>
/// Represents the Products page component that manages product display and operations.
/// </summary>
public partial class Products
{
    /// <summary>
    /// Gets or sets the application configuration.
    /// </summary>
    [Parameter]
    public ApplicationConfiguration? ApplicationConfiguration { get; set; }

    /// <summary>
    /// Gets or sets the products view state. TODO remember initialize when ApplicationConfiguration has valid data.
    /// </summary>
    [Parameter]
    public ProductsViewState? ProductsViewState { get; set; }

    /// <summary>
    /// Gets or sets the dictionary of machine names keyed by machine ID.
    /// </summary>
    [Parameter]
    public Dictionary<int, string> MachineNames { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of product DTOs.
    /// </summary>
    [Parameter]
    public List<ProductDto> ListProductsDto { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of workflow DTOs.
    /// </summary>
    [Parameter]
    public List<WorkFlowDto> ListWorkFlowsDto { get; set; } = [];

    /// <summary>
    /// Gets or sets the authentication state provider.
    /// </summary>
    [Inject]
    private AuthenticationStateProvider? AuthProvider { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the component has been initialized.
    /// </summary>
    public bool IsInitialized { get; set; }

    private bool showAddProduct;

    /// <summary>
    /// Gets or sets the selected customer.
    /// </summary>
    private string SelectedCustomer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the selected product DTO.
    /// </summary>
    public ProductDto? SelectedProductDto { get; set; }

    /// <summary>
    /// Shows the add product dialog.
    /// </summary>
    /// <param name="obj">The mouse event arguments.</param>
    private void ShowAddProduct(MouseEventArgs obj)
    {
        this.showAddProduct = true;
    }

    /// <summary>
    /// Cancels the add product operation.
    /// </summary>
    private void CancelAddProduct()
    {
        this.showAddProduct = false;
        this.NewProduct = null;
    }

    /// <summary>
    /// Initializes the component asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task OnInitializedAsync()
    {
        if (this.IsInitialized) return;

        this.IsInitialized = await this.LoadConfigurationAsync();
    }

    /// <summary>
    /// Called after the component has been rendered.
    /// </summary>
    /// <param name="firstRender">True if this is the first render of the component.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || this.IsInitialized)
            return Task.CompletedTask;

        // Optional: if any render-time logic is required
        // For example, focus a field, trigger animation, etc.
        return Task.CompletedTask;
    }

    /// <summary>
    /// Loads the configuration asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation that returns true if successful.</returns>
    private Task<bool> LoadConfigurationAsync() =>
        this.LoadAndInitializeConfigurationAsync(forceRefresh: false);

    /// <summary>
    /// Reloads the configuration asynchronously with forced refresh.
    /// </summary>
    /// <returns>A task representing the asynchronous operation that returns true if successful.</returns>
    private Task<bool> ReloadConfigurationAsync() =>
        this.LoadAndInitializeConfigurationAsync(forceRefresh: true);

    /// <summary>
    /// Loads and initializes the configuration asynchronously.
    /// </summary>
    /// <param name="forceRefresh">Whether to force a refresh of the configuration.</param>
    /// <returns>A task representing the asynchronous operation that returns true if successful.</returns>
    private async Task<bool> LoadAndInitializeConfigurationAsync(bool forceRefresh)
    {
        var result = await this.IndTraceConfigurationService.GetConfigurationAsync(refresh: forceRefresh);
        if (!result.IsSuccess || result.Value is null)
        {
            // #124 F11: a silent false left the page blank with no log and no user feedback.
            this.logger.LogError(
                "Products page failed to load the application configuration: {Errors}",
                string.Join("; ", result.Errors));
            this.snackbar.Add("Failed to load the products configuration. The page cannot be displayed; reload to retry.", Severity.Error);
            return false;
        }

        this.ApplicationConfiguration = result.Value;
        this.MachineNames = this.ApplicationConfiguration.MachineNames ?? [];

        this.ProductsViewState ??= new ProductsViewState(this.ApplicationConfiguration);
        this.ProductsViewState.SetMachineNames(this.MachineNames);

        this.ListProductsDto = this.ApplicationConfiguration.Products.ToList();
        this.ListWorkFlowsDto = this.ApplicationConfiguration.WorkFlows.ToList();

        this.ProductsViewState.InitializeProducts(this.ListProductsDto);
        this.ProductsViewState.InitializeMachineItems(this.ListProductsDto, this.ListWorkFlowsDto, this.MachineNames);

        return true;
    }

    /// <summary>
    /// Saves a new product model.
    /// </summary>
    /// <param name="NewProduct">The new product to save.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SaveNewModel(ProductDto NewProduct)
    {
        this.logger.LogInformation(NewProduct.ProductName);

        var userName = await this.GetCurrentUserNameAsync();

        NewProduct.CreatedBy = userName;
        NewProduct.ModifiedBy = userName;

        if (this.ProductsViewState is null)
        {
            this.Result = Result<ProductDto>.WithFailure(["ProductsViewState cannot be null."]);
            return;
        }
        var result = await ProductsViewState.AddProduct(this.productService, NewProduct);

        this.Result = result;
        this.logger.LogInformation(NewProduct.CustomerName);
        this.logger.LogInformation("Result: {Result}", this.Result.ToString());

        if (this.Result.IsSuccess)
        {
            this.snackbar.Add($"Success while creating the product {NewProduct.PartNumber}", Severity.Success);
            await this.ReloadConfigurationAsync();
        }
        else
        {
            this.snackbar.Add($"WithFailure while creating the product {NewProduct.PartNumber}", Severity.Error);
        }

        this.showAddProduct = false;
        await this.InvokeAsync(this.StateHasChanged);
        NewProduct = new ProductDto();
    }

    /// <summary>
    /// Gets the current user name from the authentication state.
    /// </summary>
    /// <returns>A task representing the asynchronous operation that returns the user name.</returns>
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
    /// Gets or sets the new product being created.
    /// </summary>
    private ProductDto? NewProduct { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether save operations are enabled.
    /// </summary>
    public bool SaveEnabled { get; set; }

    /// <summary>
    /// Gets or sets the result of the last operation.
    /// </summary>
    public Result? Result { get; set; }

    /// <summary>
    /// Handles the event when a new product is added.
    /// </summary>
    /// <param name="newProduct">The new product that was added.</param>
    private void NewProductAdded(ProductDto newProduct)
    {
        this.NewProduct = newProduct;
        this.ProductsViewState?.NewProductAdded(newProduct);
        if (!string.IsNullOrWhiteSpace(this.SelectedCustomer))
            this.ProductsViewState?.SetSelectedCustomer(this.SelectedCustomer);

        this.InvokeAsync(this.StateHasChanged);
    }

    /// <summary>
    /// Handles the change of the selected customer.
    /// </summary>
    /// <param name="selectedConsumer">The selected customer.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task HandleSelectedCustomerChanged(string selectedConsumer)
    {
        this.SelectedCustomer = selectedConsumer;
        await this.InvokeAsync(this.StateHasChanged);
    }

    /// <summary>
    /// Handles the selection of a product and shows the product workflow view.
    /// </summary>
    /// <param name="selectedProduct">The selected product.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task ProductSelected(ProductDto selectedProduct)
    {
        this.ProductsViewState?.ProductSelected(selectedProduct);
        this.SelectedProductDto = selectedProduct;

        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            NoHeader = false,
            FullWidth = true,
            MaxWidth = MaxWidth.Medium,
            CloseButton = true,
            Position = DialogPosition.Center,
        };

        var parameters = new DialogParameters
        {
            { "ApplicationConfiguration", this.ApplicationConfiguration },
            { "MachineNames", this.MachineNames },
            { "ListProductsDto", this.ListProductsDto },
            { "ProductDto", this.SelectedProductDto },
            { "productService" , this.productService },
        };

        return this.DialogService.ShowAsync<ProductWorkflowView>(this.SelectedProductDto?.ProductName ?? string.Empty, parameters, options);
    }
}