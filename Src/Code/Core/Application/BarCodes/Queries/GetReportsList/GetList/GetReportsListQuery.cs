// <copyright file="GetReportsListQuery.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Queries.GetReportsList.GetList;

/// <summary>
/// Represents the GetReportsListQuery.
/// </summary>
public class GetReportsListQuery : IMonitorRequest<BarCodesListVm>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetReportsListQuery"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public GetReportsListQuery()
    {
        this.Line = string.Empty;
        this.RegisterSearch = string.Empty;
        this.CustomerSearch = string.Empty;
        this.Model = string.Empty;
        this.State = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GetReportsListQuery"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public GetReportsListQuery(bool isMaster, DateTime startDate, DateTime endDate,
        bool filterByProduct, bool filterByShift, bool filterByState, string model, int shift, string state,
        bool filterByLine, string line, string registerSearch, bool filterByRegister,
        string customerSearch, bool filterByCustomer)
    {
        this.IsMaster = isMaster;
        this.StartDate = startDate;
        this.EndDate = endDate;
        this.FilterByProduct = filterByProduct;
        this.FilterByShift = filterByShift;
        this.FilterByState = filterByState;
        this.Model = model;
        this.Shift = shift;
        this.State = state;
        this.FilterByLine = filterByLine;
        this.Line = line;
        this.RegisterSearch = registerSearch;
        this.FilterByRegister = filterByRegister;
        this.CustomerSearch = customerSearch;
        this.FilterByCustomer = filterByCustomer;
    }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets the IsMaster.
    /// </summary>
    public bool IsMaster { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets the FilterByShift.
    /// </summary>
    public bool FilterByShift { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets the FilterByLine.
    /// </summary>
    public bool FilterByLine { get; set; }

    /// <summary>
    /// Gets or sets the Line.
    /// </summary>
    public string Line { get; set; }

    /// <summary>
    /// Gets or sets the RegisterSearch.
    /// </summary>
    public string RegisterSearch { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets the FilterByRegister.
    /// </summary>
    public bool FilterByRegister { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets the FilterByProduct.
    /// </summary>
    public bool FilterByProduct { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets the FilterByState.
    /// </summary>
    public bool FilterByState { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets the FilterByCustomer.
    /// </summary>
    public bool FilterByCustomer { get; set; }

    /// <summary>
    /// Gets or sets the CustomerSearch.
    /// </summary>
    public string CustomerSearch { get; set; }

    /// <summary>
    /// Gets or sets the Model.
    /// </summary>
    public string Model { get; set; }

    /// <summary>
    /// Gets or sets the Shift.
    /// </summary>
    public int Shift { get; set; }

    /// <summary>
    /// Gets or sets the State.
    /// </summary>
    public string State { get; set; }

    /// <summary>
    /// Gets or sets the StartDate.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Gets or sets the EndDate.
    /// </summary>
    public DateTime EndDate { get; set; }
}