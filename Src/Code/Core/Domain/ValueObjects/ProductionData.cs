// <copyright file="ProductionData.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.ValueObjects;

using IndTrace.Domain.Entities;

/// <summary>
/// Represents production data for a shift, including part and customer information.
/// </summary>
public class ProductionData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProductionData"/> class.
    /// </summary>
    public ProductionData()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductionData"/> class with specified values.
    /// </summary>
    /// <param name="shiftId">The shift identifier.</param>
    /// <param name="partsOk">The number of OK parts.</param>
    /// <param name="partsNok">The number of NOK parts.</param>
    public ProductionData(int shiftId, int partsOk, int partsNok)
    {
        this.ShiftId = shiftId;
        this.PartsOk = partsOk;
        this.PartsNok = partsNok;
    }

    /// <summary>
    /// Gets or sets the version of the production data.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the part number associated with the production data.
    /// </summary>
    public string ClientNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of OK parts in the current shift.
    /// </summary>
    public int LastShiftPartsOk { get; set; }

    /// <summary>
    /// Gets or sets the number of NOK parts in the current shift.
    /// </summary>
    public int LastShiftPartsNok { get; set; }

    /// <summary>
    /// Gets or sets the Julian date for the production data.
    /// </summary>
    public DateTime JulianDate { get; set; }

    /// <summary>
    /// Gets or sets the name of the customer associated with the production data.
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the shift identifier.
    /// </summary>
    public int ShiftId { get; set; }

    /// <summary>
    /// Gets or sets the number of OK parts.
    /// </summary>
    public int PartsOk { get; set; }

    /// <summary>
    /// Gets or sets the number of NOK parts in the current shift.
    /// </summary>
    public int PartsNok { get; set; }

    /// <summary>
    /// Gets or sets the part number.
    /// </summary>
    public string PartNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer name.
    /// </summary>
    public string Customer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer part number.
    /// </summary>
    public string CustomerPartNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the production data for the last shift.
    /// </summary>
    public ProductionData? LastShift { get; set; }

    // Static methods (before instance methods)
    public static ProductionData CreateFromCalculatedProductionShift(ProductionData source)
    {
        var result = new ProductionData(source.ShiftId, source.PartsOk, source.PartsNok)
        {
            PartNumber = source.PartNumber,
            Customer = source.Customer,
            CustomerPartNumber = source.CustomerPartNumber,
            LastShift = source.LastShift,
        };

        return result;
    }

    /// <summary>
    /// Sets the part information from a product.
    /// </summary>
    /// <param name="product">The product to extract information from.</param>
    /// <returns></returns>
    public IndQuestResults.Result SetPartInformation(Product product)
    {
        if (product == null)
        {
            return IndQuestResults.Result.WithFailure("Product cannot be null.");
        }

        this.PartNumber = product.PartNumber;
        this.Customer = product.CustomerName;
        this.CustomerPartNumber = product.CustomerPartNumber;
        return IndQuestResults.Result.Success();
    }

    /// <summary>
    /// Creates a deep copy of the specified <see cref="ProductionData"/> instance, including all properties.
    /// </summary>
    /// <param name="source">The source <see cref="ProductionData"/> instance to copy.</param>
    /// <returns>A new <see cref="ProductionData"/> instance with all properties copied from the source,
    /// including a limited-depth deep copy of the LastShift property to prevent stack overflow.</returns>
    /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
    /// <remarks>
    /// This method creates a complete deep copy of all properties. For the nested LastShift property,
    /// it uses a non-recursive helper method to limit recursion depth and prevent potential stack overflow issues.
    /// </remarks>
    public static IndQuestResults.Result<ProductionData> CreateFromProductionData(ProductionData source)
    {
        if (source == null)
        {
            return IndQuestResults.Result<ProductionData>.WithFailure("Source cannot be null.");
        }

        ProductionData destination = new(source.ShiftId, source.PartsOk, source.PartsNok)
        {
            PartNumber = source.PartNumber,
            Customer = source.Customer,
            CustomerPartNumber = source.CustomerPartNumber,
            Version = source.Version,
            ClientNumber = source.ClientNumber,
            LastShiftPartsOk = source.LastShiftPartsOk,
            LastShiftPartsNok = source.LastShiftPartsNok,
            JulianDate = source.JulianDate,
            CustomerName = source.CustomerName,
        };

        // Deep clone the LastShift property if it exists
        if (source.LastShift is not null)
        {
            destination.LastShift = Clone(source.LastShift);
        }

        return IndQuestResults.Result<ProductionData>.Success(destination);
    }

    /// <summary>
    /// Adds the last shift's production data.
    /// </summary>
    /// <param name="lastShiftShiftId">The last shift's ID.</param>
    /// <param name="lastPartsOk">The number of OK parts in the last shift.</param>
    /// <param name="lastPartsNok">The number of NOK parts in the last shift.</param>
    public void AddLastShift(int lastShiftShiftId, int lastPartsOk, int lastPartsNok)
    {
        this.LastShift = new ProductionData(lastShiftShiftId, lastPartsOk, lastPartsNok);
    }

    /// <summary>
    /// Returns a string representation of the production data.
    /// </summary>
    /// <returns>A string describing the production data.</returns>
    public override string ToString()
    {
        var result = $"ShiftId: {this.ShiftId}, PartsOk: {this.PartsOk}, PartsNok: {this.PartsNok}";

        if (!string.IsNullOrEmpty(this.PartNumber))
        {
            result += $", PartNumber: {this.PartNumber}, Customer: {this.Customer}, CustomerPartNumber: {this.CustomerPartNumber}";
        }

        if (this.LastShift != null)
        {
            result += $", LastShift: [ {this.LastShift} ]";
        }

        return result;
    }

    /// <summary>
    /// Creates a shallow copy of a <see cref="ProductionData"/> instance without recursively copying the LastShift property.
    /// </summary>
    /// <param name="source">The source <see cref="ProductionData"/> instance to copy.</param>
    /// <returns>A new <see cref="ProductionData"/> instance with all properties copied from the source,
    /// but without copying the LastShift property's LastShift (if any).</returns>
    /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
    /// <remarks>
    /// This private helper method is used by CreateFromProductionData to create a limited-depth clone,
    /// preventing stack overflow exceptions that could occur with deeply nested LastShift chains or circular references.
    /// </remarks>
    private static ProductionData Clone(ProductionData source)
    {
        // Ensure the source is not null before cloning
        // This method Don't violate DRY principle as it is a specific implementation for cloning ProductionData
        // This method is necessary to ensure that the LastShift property is cloned correctly
        // And to avoid recursive calls to CreateFromProductionData
        // and a potential infinite loop with a stack overflow exception.
        if (source == null)
        {
            // Defensive non-throw: return empty clone to avoid exceptions deep in copying
            return new ProductionData();
        }

        ProductionData destination = new(source.ShiftId, source.PartsOk, source.PartsNok)
        {
            PartNumber = source.PartNumber,
            Customer = source.Customer,
            CustomerPartNumber = source.CustomerPartNumber,
            Version = source.Version,
            ClientNumber = source.ClientNumber,
            LastShiftPartsOk = source.LastShiftPartsOk,
            LastShiftPartsNok = source.LastShiftPartsNok,
            JulianDate = source.JulianDate,
            CustomerName = source.CustomerName,
        };

        return destination;
    }
}