// <copyright file="ProductFactory.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;
using IndTrace.Domain.Entities;
using IndTrace.Domain.Enum;
using IndTrace.Domain.Interfaces;
using IndTrace.Domain.ValueObjects;

namespace IndTrace.Domain.Services.Products;

/// <summary>
/// Pure product creation logic without external dependencies.
/// Handles entity creation and advanced ID parsing logic from PartNumber.
/// Domain service - zero infrastructure dependencies.
/// </summary>
public partial class ProductFactory : IProductFactory
{
    // Compiled, thread-safe regex for extracting the last integer (with or without hyphen)
    // Matches: "ABC-123" → 123, "TEST001" → 1, "L100003" → 687508
    private static readonly Regex LastIntegerRegex = new Regex(@"(\d+)$", RegexOptions.Compiled);

    // Compiled, thread-safe regex for the TryParseAnyNumber fallback: any digit sequence anywhere in the
    // part number. #126 F8: previously recompiled (RegexOptions.Compiled) on every call inside the method.
    private static readonly Regex AnyIntegerRegex = new Regex(@"(\d+)", RegexOptions.Compiled);

    // Deterministic time source (issue #85): audit timestamps stamped on created products must come from the
    // injected clock, never DateTime.Now, so persistence and the §7 golden masters are reproducible.
    private readonly IDateTimeMachine _dateTimeMachine;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductFactory"/> class.
    /// </summary>
    /// <param name="dateTimeMachine">The deterministic time source for the created product's audit timestamps
    /// (<c>CreatedOn</c>/<c>ModifiedOn</c>) — never <c>DateTime.Now</c>.</param>
    public ProductFactory(IDateTimeMachine dateTimeMachine)
    {
        _dateTimeMachine = dateTimeMachine ?? throw new ArgumentNullException(nameof(dateTimeMachine));
    }

    /// <summary>
    /// Creates a result object containing a new product based on the specified input data, customer, and production
    /// line.
    /// </summary>
    /// <param name="productData">The input data used to construct the product. Cannot be null.</param>
    /// <param name="customer">The customer for whom the product is being created. Cannot be null.</param>
    /// <param name="line">The production line associated with the product creation. Cannot be null.</param>
    /// <returns>A <see cref="Result{Product}"/> representing the outcome of the product creation operation. Contains the created
    /// product if successful; otherwise, includes error information.</returns>
    public Result<Product> CreateResultProduct(ProductInput productData, Customer customer, Line line)
    {
        // Railway entry (issue #88): validate identity inputs and return Result failures instead of relying on
        // a throw-then-catch shim. The null guards here guarantee CreateProduct's ArgumentNullException guards
        // never fire, so delegating to it is byte-identical to the previous success path — but no exception can
        // cross this boundary. Product.Create's own Result failure mode is unreachable for these coalesced,
        // guaranteed-non-null identity inputs (see CreateProduct), so success is deterministic.
        if (productData is null)
        {
            return Result<Product>.WithFailure(["productData cannot be null."]);
        }

        if (customer is null)
        {
            return Result<Product>.WithFailure(["customer cannot be null."]);
        }

        if (line is null)
        {
            return Result<Product>.WithFailure(["line cannot be null."]);
        }

        return Result<Product>.Success(CreateProduct(productData, customer, line));
    }

    /// <summary>
    /// Creates a Product entity with intelligent ID assignment preparation.
    /// Does NOT set ProductId - that's the persistence layer's responsibility.
    /// </summary>
    public Product CreateProduct(ProductInput productData, Customer customer, Line line)
    {
        if (productData is null)
            throw new ArgumentNullException(nameof(productData));
        if (customer is null)
            throw new ArgumentNullException(nameof(customer));
        if (line is null)
            throw new ArgumentNullException(nameof(line));

        // Coalesce the (legacy, possibly-null) input exactly as the original object-initializer did, then
        // route construction through the guarded Product.Create factory (Story 2.1 / #26) so there is a
        // single guarded construction path. The coalesced values always satisfy Create's non-null identity
        // invariant, so for valid DI arguments this is byte-identical to the previous behavior.
        var partNumber = productData.PartNumber ?? string.Empty;
        var productName = productData.ProductName ?? string.Empty;

        // Defaults from original handler: both legacy ternary branches always yielded a positive
        // (active) value, so the entity is unconditionally Active here (behavior-preserving).
        var isActive = ActiveStatus.Active;
        var version = productData.Version > 0 ? productData.Version : 1;
        var customerPartNumber = productData.CustomerPartNumber ?? string.Empty;
        var aliasPartNumber = productData.AliasPartNumber ?? string.Empty;
        var description = productData.Description ?? string.Empty;
        var customerName = customer.Name ?? string.Empty;
        var createdBy = productData.CreatedBy ?? string.Empty;
        var timestamp = _dateTimeMachine.Now;

        var createResult = Product.Create(
            partNumber,
            productName,
            isActive,
            version,
            customerPartNumber,
            aliasPartNumber,
            description,
            customer.CustomerId,
            customerName,
            line.LineId,
            0);

        // partNumber/productName are coalesced to non-null above, so Create's only failure mode (null
        // identity) is unreachable here. Fail LOUD on the impossible branch rather than routing the
        // test-only CreateFixture seam into production code (which would silently run a fixture builder in
        // a life-critical writer if a future caller ever introduced a non-coalescing path). This throw is a
        // proven-unreachable invariant assertion, consistent with the ArgumentNullException guards above and
        // caught by CreateResultProduct's wrapper.
        if (createResult.IsFailure || createResult.Value is null)
        {
            throw new InvalidOperationException(
                "ProductFactory.CreateProduct: Product.Create failed for guaranteed-non-null identity inputs — invariant breach. " +
                string.Join(", ", createResult.Errors ?? []));
        }

        var product = createResult.Value;

        // Customer / Line navigation relationships and audit fields are set after construction (Create
        // takes only the entity's scalar shape; ProductId is the persistence layer's responsibility).
        product.Customer = customer;
        product.Line = line;
        product.CreatedBy = createdBy;
        product.CreatedOn = timestamp;
        product.ModifiedBy = createdBy;
        product.ModifiedOn = timestamp;
        product.ProductId = new ProductId(0);

        return product;
    }

    /// <summary>
    /// Attempts to parse the last integer from a part number string.
    /// Preserves EXACT parsing logic from original handler.
    /// </summary>
    /// <param name="partNumber">The part number string to parse</param>
    /// <returns>Tuple with success flag and parsed integer value</returns>
    /// <summary>
    /// Attempts to parse the last integer from a part number string.
    /// Preserves EXACT parsing logic from original handler.
    /// </summary>
    /// <param name="partNumber">The part number string to parse</param>
    /// <returns>Tuple with success flag and parsed integer value</returns>
    public (bool Success, int ParsedId) TryParseLastInteger(string partNumber)
    {
        // Check for null or empty input - exact match to original
        if (string.IsNullOrEmpty(partNumber))
        {
            return (false, 0);
        }

        // Use pattern to find trailing digits in the part number
        // Matches: "ABC-123" → 123, "TEST001" → 1, "L100003" → 687508
        var match = LastIntegerRegex.Match(partNumber);

        if (match.Success && int.TryParse(match.Groups[1].Value, out int parsedValue))
        {
            // If a number is found and is within the range of int, return true and the value
            // Note: int.TryParse guarantees parsedValue is within int range, no additional checks needed
            return (true, Math.Abs(parsedValue));
        }

        // No number found - try recursive parsing for any digits in the string
        return TryParseAnyNumber(partNumber);
    }

    /// <summary>
    /// Fallback method to extract any number from the part number when no trailing digits found.
    /// Searches recursively for any numeric sequence in the part number.
    /// </summary>
    /// <param name="partNumber">The part number to search</param>
    /// <returns>Tuple with success flag and extracted number, or (false, 0) if no numbers found</returns>
    private (bool Success, int ParsedId) TryParseAnyNumber(string partNumber)
    {
        if (string.IsNullOrEmpty(partNumber))
        {
            return (false, 0);
        }

        // Try to find any sequence of digits in the part number
        var matches = AnyIntegerRegex.Matches(partNumber);

        if (matches.Count > 0)
        {
            // Try each numeric sequence found, starting from the last one
            for (int i = matches.Count - 1; i >= 0; i--)
            {
                if (int.TryParse(matches[i].Groups[1].Value, out int parsedValue) && parsedValue > 0)
                {
                    return (true, Math.Abs(parsedValue));
                }
            }
        }

        // No valid numbers found in part number - will need database-generated ID
        return (false, 0);
    }

    /// <summary>
    /// Gets a dynamic offset based on the width of the parsed number.
    /// Preserves EXACT offset calculation from original handler.
    /// </summary>
    /// <param name="parsedNumber">The parsed number from PartNumber</param>
    /// <returns>The dynamic offset for ID calculation</returns>
    public int GetDynamicOffset(int parsedNumber)
    {
        // Calculate offset based on the width of the parsed number
        // This maintains visual comparison while avoiding conflicts
        var numberWidth = parsedNumber.ToString().Length;

        // Exact formula from original handler:
        // For single digit: add 10 (e.g., 5 -> 15)
        // For double digit: add 100 (e.g., 23 -> 123)
        // For triple digit: add 1000 (e.g., 123 -> 1123)
        // For four digit: add 10000 (e.g., 1234 -> 11234)
        // And so on...
        var offset = (int)Math.Pow(10, numberWidth);

        return offset;
    }
}