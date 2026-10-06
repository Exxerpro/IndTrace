// <copyright file="CustomersFilter.razor.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.ConfigStations.Queries.GetConfigStationList;
using IndTrace.Application.Customers;
using Microsoft.AspNetCore.Components;

namespace IndTrace.Components.Area.Customers
{
    /// <summary>
    /// Provides filtering functionality for customers with logo display and selection capabilities.
    /// </summary>
    public partial class CustomersFilter
    {
        /// <summary>
        /// Gets or sets the application configuration containing active customer information.
        /// </summary>
        [Parameter]
        public ApplicationConfiguration? ApplicationConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the currently selected customer name.
        /// </summary>
        [Parameter]
        public required string SelectedCustomer { get; set; }

        /// <summary>
        /// Gets or sets the event callback triggered when a customer is selected.
        /// </summary>
        [Parameter]
        public EventCallback<CustomerDto> OnConsumerSelected { get; set; }

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
        /// <param name="newValue">The new selected customer name.</param>
        private async Task OnCustomerChanged(string newValue)
        {
            this.SelectedCustomer = newValue;

            var selected = this.ApplicationConfiguration?.ActiveCustomer?.FirstOrDefault(c => c.Name == newValue);
            if (selected is not null)
            {
                await this.OnConsumerSelected.InvokeAsync(selected);
            }
        }
    }
}