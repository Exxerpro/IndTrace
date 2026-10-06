// <copyright file="IProductValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Services.Products;

/// <summary>
/// Product business rule validation without external dependencies.
/// Domain service - contains zero dependencies, pure validation logic only.
/// </summary>
public interface IProductValidator
{
    /// <summary>
    /// Validates product data according to business rules.
    /// Performs domain-level validation without database dependencies.
    /// </summary>
    /// <param name="productInput">Product data to validate</param>
    /// <returns>Validation result with any rule violations</returns>
    /// <remarks>
    /// Domain Validation Rules:
    /// - PartNumber must not be null or empty
    /// - ProductName must not be null or empty
    /// - CustomerId must be greater than 0
    /// - LineId must be greater than 0
    /// - Description length limits
    /// - PartNumber format validation
    /// </remarks>
    Result ValidateProductData(ProductInput productInput);

    /// <summary>
    /// Validates PartNumber format according to business rules.
    /// Ensures PartNumber meets organizational standards.
    /// </summary>
    /// <param name="partNumber">Part number to validate</param>
    /// <returns>Validation result for PartNumber format</returns>
    /// <remarks>
    /// PartNumber Rules:
    /// - Must not be null, empty, or whitespace
    /// - Must meet minimum/maximum length requirements
    /// - Must follow organizational naming conventions
    /// - Special character restrictions
    /// </remarks>
    Result ValidatePartNumberFormat(string partNumber);

    /// <summary>
    /// Validates customer-related data consistency.
    /// Ensures CustomerId and CustomerName are logically consistent.
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <param name="customerName">Customer name</param>
    /// <returns>Validation result for customer data consistency</returns>
    Result ValidateCustomerData(int customerId, string customerName);

    /// <summary>
    /// Validates production line assignment data.
    /// Ensures LineId is valid for product assignment.
    /// </summary>
    /// <param name="lineId">Production line identifier</param>
    /// <returns>Validation result for line assignment</returns>
    Result ValidateLineAssignment(int lineId);
}