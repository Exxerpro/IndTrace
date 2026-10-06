// <copyright file="ProductCreateTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.ProductsTests;

/// <summary>
/// Story 2.1 (#26) unit tests for the guarded <see cref="Product.Create"/> factory and the internal
/// <see cref="Product.CreateFixture"/> test-data seam. Domain.UnitTests is an <c>InternalsVisibleTo</c>
/// grantee, so it can drive the unguarded fixture seam directly.
/// </summary>
public class ProductCreateTests
{
    /// <summary>
    /// Create with non-null identity strings succeeds and projects every scalar field onto the product.
    /// </summary>
    [Fact]
    public void Create_WithValidValues_ShouldSucceedAndProjectAllScalars()
    {
        // Act
        var result = Product.Create(
            partNumber: "L100001",
            productName: "X1 Housing 2K Housing Assy",
            isActive: ActiveStatus.Active,
            version: 2,
            customerPartNumber: "CUST-001",
            aliasPartNumber: "ALIAS-001",
            description: "Housing assembly",
            customerId: 7,
            customerName: "Volkswagen",
            lineId: 3,
            ruleId: 566);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var product = result.Value;
        product.ShouldNotBeNull();
        product.PartNumber.ShouldBe("L100001");
        product.ProductName.ShouldBe("X1 Housing 2K Housing Assy");
        product.IsActive.Value.ShouldBe(1);
        product.Version.ShouldBe(2);
        product.CustomerPartNumber.ShouldBe("CUST-001");
        product.AliasPartNumber.ShouldBe("ALIAS-001");
        product.Description.ShouldBe("Housing assembly");
        product.CustomerId.ShouldBe(7);
        product.CustomerName.ShouldBe("Volkswagen");
        product.LineId.ShouldBe(3);
        product.RuleId.ShouldBe(566);

        // Identity is the persistence layer's responsibility; Create leaves it at the default.
        product.ProductId.Value.ShouldBe(0);
    }

    /// <summary>
    /// Empty identity strings are intentionally accepted (the legacy ProductFactory contract coalesces
    /// missing input to empty); only <see langword="null"/> is rejected.
    /// </summary>
    [Fact]
    public void Create_WithEmptyIdentityStrings_ShouldSucceed()
    {
        // Act
        var result = Product.Create(
            partNumber: string.Empty,
            productName: string.Empty,
            isActive: ActiveStatus.None,
            version: 1,
            customerPartNumber: string.Empty,
            aliasPartNumber: string.Empty,
            description: string.Empty,
            customerId: 0,
            customerName: string.Empty,
            lineId: 0,
            ruleId: 0);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.PartNumber.ShouldBe(string.Empty);
        result.Value.ProductName.ShouldBe(string.Empty);
    }

    /// <summary>
    /// A null part number violates the identity invariant and yields a failure result.
    /// </summary>
    [Fact]
    public void Create_WithNullPartNumber_ShouldFail()
    {
        // Act
        var result = Product.Create(
            partNumber: null,
            productName: "Valid Name",
            isActive: ActiveStatus.Active,
            version: 1,
            customerPartNumber: string.Empty,
            aliasPartNumber: string.Empty,
            description: string.Empty,
            customerId: 0,
            customerName: string.Empty,
            lineId: 0,
            ruleId: 0);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("PartNumber cannot be null.");
    }

    /// <summary>
    /// A null product name violates the identity invariant and yields a failure result.
    /// </summary>
    [Fact]
    public void Create_WithNullProductName_ShouldFail()
    {
        // Act
        var result = Product.Create(
            partNumber: "Valid-Part",
            productName: null,
            isActive: ActiveStatus.Active,
            version: 1,
            customerPartNumber: string.Empty,
            aliasPartNumber: string.Empty,
            description: string.Empty,
            customerId: 0,
            customerName: string.Empty,
            lineId: 0,
            ruleId: 0);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("ProductName cannot be null.");
    }

    /// <summary>
    /// Both identity violations are aggregated into the failure result rather than fail-fast on the first.
    /// </summary>
    [Fact]
    public void Create_WithBothIdentityStringsNull_ShouldAggregateBothErrors()
    {
        // Act
        var result = Product.Create(
            partNumber: null,
            productName: null,
            isActive: ActiveStatus.Active,
            version: 1,
            customerPartNumber: string.Empty,
            aliasPartNumber: string.Empty,
            description: string.Empty,
            customerId: 0,
            customerName: string.Empty,
            lineId: 0,
            ruleId: 0);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("PartNumber cannot be null.");
        result.Errors.ShouldContain("ProductName cannot be null.");
        result.Errors.Count().ShouldBe(2);
    }

    /// <summary>
    /// The internal CreateFixture seam seeds identity plus every scalar field WITHOUT applying the
    /// Create guards (it can seed values, such as a seeded ProductId, that Create deliberately does not).
    /// </summary>
    [Fact]
    public void CreateFixture_ShouldSeedAllFieldsBypassingGuards()
    {
        // Act
        var product = Product.CreateFixture(
            productId: 566,
            partNumber: "L100001",
            productName: "L100001",
            isActive: ActiveStatus.Active,
            version: 1,
            customerPartNumber: "X1 Housing 2K Housing Assy",
            aliasPartNumber: "X1 Housing 2K Housing Assy",
            description: "X1 Housing 2K Housing Assy",
            customerId: 1,
            customerName: "Volkswagen",
            lineId: 1,
            ruleId: 566);

        // Assert
        product.ShouldNotBeNull();
        product.ProductId.Value.ShouldBe(566);
        product.PartNumber.ShouldBe("L100001");
        product.ProductName.ShouldBe("L100001");
        product.IsActive.Value.ShouldBe(1);
        product.Version.ShouldBe(1);
        product.CustomerName.ShouldBe("Volkswagen");
        product.RuleId.ShouldBe(566);
    }
}
