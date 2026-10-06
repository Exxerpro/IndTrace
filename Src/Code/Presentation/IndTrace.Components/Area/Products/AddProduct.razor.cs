// <copyright file="AddProduct.razor.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.ConfigStations.Queries.GetConfigStationList;
using IndTrace.Application.Machines.Queries.GetMachinesList;
using IndTrace.Application.Products.Queries.GetProductDetail;
using IndTrace.Application.WorkFlows.Dto;
using IndTrace.Domain.Models;
using IndTrace.Domain.Routing.Authoring;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MudBlazor;
using System.ComponentModel.DataAnnotations;

namespace IndTrace.Components.Area.Products
{
    /// <summary>
    /// Provides functionality for adding new products to the system with workflow configuration.
    /// </summary>
    public partial class AddProduct
    {
        /// <summary>
        /// Gets or sets the result of the last operation performed.
        /// </summary>
        [Parameter]
        public Result? Result { get; set; }

        /// <summary>
        /// Gets or sets the callback function invoked when a product is successfully added.
        /// </summary>
        [Parameter]
        [Required]
        public Func<ProductDto, Task>? OnProductAdded { get; set; }

        /// <summary>
        /// Gets or sets the event callback for canceling the add operation.
        /// </summary>
        [Parameter]
        public EventCallback OnCancel { get; set; }

        /// <summary>
        /// Gets or sets the message to display to the user.
        /// </summary>
        [Parameter]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the application configuration containing system data.
        /// </summary>
        [Parameter]
        public ApplicationConfiguration? ApplicationConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the dictionary mapping machine IDs to their display names.
        /// </summary>
        [Parameter]
        public Dictionary<int, string> MachineNames { get; set; } = new();

        /// <summary>
        /// Gets or sets the product data transfer object.
        /// </summary>
        [Parameter]
        public ProductDto? ProductDto { get; set; }

        /// <summary>
        /// Gets or sets the list of workflow data transfer objects.
        /// </summary>
        [Parameter]
        public List<WorkFlowDto> WorkFlowsDto { get; set; } = new();

        /// <summary>
        /// Gets or sets the callback function executed when workflow configuration is complete.
        /// </summary>
        [Parameter]
        public Func<List<MachineDto>, Task>? OnConfigured { get; set; }

        /// <summary>
        /// Gets or sets the currently selected customer name.
        /// </summary>
        [Parameter]
        public string SelectedCustomer { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the save operation is enabled.
        /// </summary>
        public bool SaveEnabled { get; set; }

        /// <summary>
        /// Gets or sets the new product being created.
        /// </summary>
        [Parameter]
        public ProductDto? NewProduct { get; set; }

        /// <summary>
        /// Cancels the add product operation and clears the new product data.
        /// </summary>
        public void CancelAddProduct()
        {
            this.NewProduct = null;
            this.OnCancel.InvokeAsync();
        }

        /// <summary>
        /// Saves the new product model to the system.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SaveNewModel()
        {
            if (this.NewProduct is null || this.OnProductAdded is null)
            {
                return;
            }

            this.logger.LogInformation("Adding new product {product} {Customer} ", this.NewProduct.PartNumber, this.NewProduct.CustomerName);

            this.NewProduct.Customer.Name = this.NewProduct.CustomerName;

            // #124 F12: a button handler must never throw — an exception here kills the circuit.
            // Every abnormal path below surfaces a user-visible error message instead.
            var configuration = this.ApplicationConfiguration;
            if (configuration is null)
            {
                this.logger.LogError("AddProduct save failed: ApplicationConfiguration is null.");
                this.ShowError("Cannot save the product: the application configuration is not loaded. Close the dialog and retry.");
                return;
            }

            var matchingCustomers = configuration.Customers
                .Where(e => e.Name == this.NewProduct.Customer.Name)
                .ToList();

            if (matchingCustomers.Count > 1)
            {
                this.logger.LogError(
                    "AddProduct save failed: {Count} customers share the name {Customer}.",
                    matchingCustomers.Count,
                    this.NewProduct.Customer.Name);
                this.ShowError($"Cannot save the product: more than one customer is named '{this.NewProduct.Customer.Name}'. Resolve the duplicate customer names first.");
                return;
            }

            if (matchingCustomers.Count == 0)
            {
                this.logger.LogError(
                    "AddProduct save failed: customer {Customer} not found in the configuration.",
                    this.NewProduct.Customer.Name);
                this.ShowError($"Cannot save the product: customer '{this.NewProduct.Customer.Name}' was not found in the configuration.");
                return;
            }

            var customer = matchingCustomers[0];
            this.NewProduct.Customer.CustomerId = customer.CustomerId;
            this.NewProduct.Customer.Name = customer.Name;

            if (this.OnProductAdded is not null && this.NewProduct is not null)
            {
                await this.OnProductAdded.Invoke(this.NewProduct);
            }
            if (this.Result is not null)
            {
                if (this.Result.IsSuccess)
                {
                    this.NewProduct = null;
                    this.Message = "Product added successfully";
                    this.messageSeverity = Severity.Success;
                }
                else
                {
                    this.ShowError($"Product not added Error {this.Result.Errors.FirstOrDefault()}");
                }
            }
        }

        /// <summary>
        /// Records a user-visible error message rendered by the component's alert (never throws — #124 F12).
        /// </summary>
        /// <param name="message">The error text to display.</param>
        private void ShowError(string message)
        {
            this.Message = message;
            this.messageSeverity = Severity.Error;
        }

        /// <summary>
        /// The severity used to render <see cref="Message"/> in the component's alert.
        /// </summary>
        private Severity messageSeverity = Severity.Info;

        /// <summary>
        /// Triggers the workflow configuration dialog for the new product.
        /// </summary>
        private async Task TriggerConfigureWorkflow()
        {
            var options = new DialogOptions
            {
                CloseOnEscapeKey = true,
                NoHeader = false,
                FullWidth = true,
                MaxWidth = MaxWidth.ExtraLarge,
                CloseButton = true,
                Position = DialogPosition.Center,
            };

            if (this.NewProduct is null)
            {
                this.logger.LogWarning("No product available to configure routing.");
                return;
            }

            // E11.4-4: mount the node+edge ProductRouteEditor (E11.4-3) in place of the flat, fork-incapable
            // ProductWorkflowEdit dialog. The editor produces an immutable AuthoringRoute (ordered nodes, per-node
            // outgoing edges — a diverter is just a node with >1 edge) and hands it to OnRouteConfigured.
            var editorParams = new DialogParameters
            {
                // At add-time the product has no id yet; the create handler rebinds the authored route to the real
                // persisted id, so a placeholder id here is correct.
                { "ProductId", this.NewProduct.ProductId },
                { "ProductName", this.NewProduct.ProductName },
                { "AvailableMachines", (IReadOnlyDictionary<int, string>)this.MachineNames },

                // ProductRouteEditor.OnRouteConfigured is a Func<AuthoringRoute, Task>? (not an EventCallback);
                // pass the matching delegate type. The callback stashes the authored route on NewProduct so Save
                // threads it to the create pipeline.
                { "OnRouteConfigured", (Func<AuthoringRoute, Task>)this.OnRouteConfiguredCallBack },
            };

            var result = await this.dialogService.ShowAsync<ProductRouteEditor>($"Configure route: {this.NewProduct.ProductName}", editorParams, options);

            if (this.Result is not null)
            {
                if (this.Result.IsSuccess)
                {
                    this.NewProduct = null;
                    this.Message = "Product added successfully";
                    this.messageSeverity = Severity.Success;
                }
                else
                {
                    this.ShowError($"Product not added Error {this.Result.Errors.FirstOrDefault()}");
                }
            }
        }

        /// <summary>
        /// Handles the callback when the node+edge route editor completes (E11.4-4). Stashes the authored
        /// <see cref="AuthoringRoute"/> on <see cref="NewProduct"/> so <see cref="SaveNewModel"/> threads it to the
        /// create pipeline, and mirrors the route's node machine ids onto <c>Machines</c> for the summary display.
        /// </summary>
        /// <param name="route">The authored node+edge route (order and fork multiplicity preserved verbatim).</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task OnRouteConfiguredCallBack(AuthoringRoute route)
        {
            if (this.NewProduct is null)
            {
                this.logger.LogWarning("No product available to configure routing.");
                return Task.CompletedTask;
            }

            this.logger.LogInformation(
                "Route configured for product {Product}: {NodeCount} node(s).",
                this.NewProduct.ProductName,
                route.Nodes.Count);

            // Carry the authored route to the create pipeline; also mirror the placed machine ids onto Machines so
            // the add-form summary lists them (the route itself is what persists, not the flat list).
            this.NewProduct.Route = route;
            this.NewProduct.Machines = route.Nodes.Select(n => n.MachineId.Value).ToList();
            this.SaveEnabled = true;

            return this.InvokeAsync(this.StateHasChanged);
        }
    }
}
