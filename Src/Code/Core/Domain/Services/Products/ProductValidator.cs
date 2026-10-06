// <copyright file="ProductValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndQuestResults;

namespace IndTrace.Domain.Services.Products;

/// <summary>
/// Product business rule validation without external dependencies.
/// Pure domain service - no infrastructure dependencies allowed.
/// </summary>
public class ProductValidator : IProductValidator
{

    /// <summary>
    /// Validates product data according to business rules.
    /// Performs domain-level validation without database dependencies.
    /// </summary>
    public Result ValidateProductData(ProductInput productInput)
    {
        if (productInput is null)
        {
            return Result.WithFailure("ProductInput cannot be null.");
        }

        var errors = new List<string>();

        // PartNumber validation - critical for business logic
        if (string.IsNullOrWhiteSpace(productInput.PartNumber))
        {
            errors.Add("PartNumber is required and cannot be empty.");
        }
        else if (productInput.PartNumber.Length < 3)
        {
            errors.Add("PartNumber must be at least 3 characters long.");
        }
        else if (productInput.PartNumber.Length > 80)
        {
            errors.Add("PartNumber cannot exceed 80 characters.");
        }

        // ProductName validation
        if (string.IsNullOrWhiteSpace(productInput.ProductName))
        {
            errors.Add("ProductName is required and cannot be empty.");
        }
        else if (productInput.ProductName.Length > 80)
        {
            errors.Add("ProductName cannot exceed 80 characters.");
        }

        // CustomerId validation - must be positive
        if (productInput.CustomerId <= 0)
        {
            errors.Add("CustomerId must be greater than 0.");
        }

        // LineId validation - must be positive
        if (productInput.LineId <= 0)
        {
            errors.Add("LineId must be greater than 0.");
        }

        // Description validation - optional but has length limit
        if (!string.IsNullOrEmpty(productInput.Description) && productInput.Description.Length > 500)
        {
            errors.Add("Description cannot exceed 500 characters.");
        }

        // CustomerName validation - optional for dual resolution logic
        if (!string.IsNullOrWhiteSpace(productInput.CustomerName) && productInput.CustomerName.Length > 100)
        {
            errors.Add("CustomerName cannot exceed 100 characters.");
        }

        // Return validation result
        return errors.Count == 0 
            ? Result.Success() 
            : Result.WithFailure(errors);
    }

    /// <summary>
    /// Validates PartNumber format according to business rules.
    /// Ensures PartNumber meets organizational standards.
    /// </summary>
    public Result ValidatePartNumberFormat(string partNumber)
    {
        if (string.IsNullOrWhiteSpace(partNumber))
        {
            return Result.WithFailure("PartNumber cannot be null, empty, or whitespace.");
        }

        var errors = new List<string>();

        // Length validation
        if (partNumber.Length < 3)
        {
            errors.Add("PartNumber must be at least 3 characters long.");
        }

        if (partNumber.Length > 80)
        {
            errors.Add("PartNumber cannot exceed 80 characters.");
        }

        // Format validation - alphanumeric with hyphens allowed
        if (!System.Text.RegularExpressions.Regex.IsMatch(partNumber, @"^[A-Za-z0-9\-]+$"))
        {
            errors.Add("PartNumber can only contain letters, numbers, and hyphens.");
        }

        // Business rule: Cannot start or end with hyphen
        if (partNumber.StartsWith("-") || partNumber.EndsWith("-"))
        {
            errors.Add("PartNumber cannot start or end with a hyphen.");
        }

        // Business rule: No consecutive hyphens
        if (partNumber.Contains("--"))
        {
            errors.Add("PartNumber cannot contain consecutive hyphens.");
        }

        return errors.Count > 0
            ? Result.WithFailure(errors)
            : Result.Success();
    }

    /// <summary>
    /// Validates customer-related data consistency.
    /// Ensures CustomerId and CustomerName are logically consistent.
    /// </summary>
    public Result ValidateCustomerData(int customerId, string customerName)
    {
        var errors = new List<string>();

        if (customerId <= 0)
        {
            errors.Add("CustomerId must be greater than 0.");
        }

        if (string.IsNullOrWhiteSpace(customerName))
        {
            errors.Add("CustomerName is required when validating customer data.");
        }

        // Business rule: CustomerName should not be generic placeholders
        if (!string.IsNullOrWhiteSpace(customerName))
        {
            var invalidNames = new[] { "TBD", "TODO", "UNKNOWN", "N/A", "NA" };
            if (invalidNames.Any(invalid => customerName.Equals(invalid, StringComparison.OrdinalIgnoreCase)))
            {
                errors.Add($"CustomerName '{customerName}' is not a valid customer name.");
            }
        }

        return errors.Count > 0
            ? Result.WithFailure(errors)
            : Result.Success();
    }

    /// <summary>
    /// Validates production line assignment data.
    /// Ensures LineId is valid for product assignment.
    /// </summary>
    public Result ValidateLineAssignment(int lineId)
    {
        if (lineId <= 0)
        {
            return Result.WithFailure("LineId must be greater than 0.");
        }

        // Additional business rules for line assignment
        if (lineId > 9999)
        {
            return Result.WithFailure("LineId exceeds maximum allowed value of 9999.");
        }

        return Result.Success();
    }
}