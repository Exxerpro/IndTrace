// <copyright file="ReportsFilterInfoVm.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Queries.GetReportsList.FiltersInfo;

/// <summary>
/// Represents the ReportsFilterInfoVm.
/// </summary>
public class ReportsFilterInfoVm
{
    /// <summary>
    /// Gets or sets the States.
    /// </summary>
    public List<string> States { get; set; }

    /// <summary>
    /// Gets or sets the Shifts.
    /// </summary>
    public List<int> Shifts { get; set; }

    /// <summary>
    /// Gets or sets the Products.
    /// </summary>
    public List<string> Products { get; set; }

    /// <summary>
    /// Gets or sets the Customers.
    /// </summary>
    public List<string> Customers { get; set; }

    /// <summary>
    /// Gets or sets the CustomerProducts.
    /// </summary>
    public List<CustomerProduct> CustomerProducts { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportsFilterInfoVm"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public ReportsFilterInfoVm()
    {
        this.States = [];
        this.Shifts = [];
        this.Products = [];
        this.Customers = [];
        this.CustomerProducts = [];
    }
}