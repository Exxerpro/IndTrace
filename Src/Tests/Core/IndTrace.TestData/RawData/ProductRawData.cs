// <copyright file="ProductRawData.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using IndTrace.Domain.Entities;
using IndTrace.Domain.Enum;

namespace IndTrace.TestData.RawData;

/// <summary>
/// Static test data for Product entities with O(1) lookup.
/// Generated with ImmutableDictionary for thread-safety and performance.
/// </summary>
internal static class ProductRawData
{
    private static readonly ImmutableDictionary<int, Product> DictDict =
        new Dictionary<int, Product>
        {
            [1] = Seed(
                productId: 1,
                partNumber: "DEFAULT",
                productName: "Default Test Product",
                isActive: 1,
                version: 1,
                customerPartNumber: "TEST-001",
                aliasPartNumber: "TEST-001",
                description: "Default product for test data",
                ruleId: 1,
                createdBy: "Admin",
                createdOn: new DateTime(2023, 8, 28, 17, 2, 24),
                modifiedBy: "Admin",
                modifiedOn: new DateTime(2023, 8, 25, 12, 6, 30),
                customerName: "Test Customer",
                customerId: 1,
                lineId: 1),

            [508] = Seed(
                productId: 508,
                partNumber: "L100003",
                productName: "L100003",
                isActive: 1,
                version: 1,
                customerPartNumber: "Housing CHMSL X1",
                aliasPartNumber: "Housing CHMSL X1",
                description: "Housing CHMSL X1",
                ruleId: 508,
                createdBy: "Admin",
                createdOn: new DateTime(2023, 8, 28, 17, 2, 24),
                modifiedBy: "0",
                modifiedOn: new DateTime(2023, 8, 25, 12, 6, 30),
                customerName: "Volkswagen",
                customerId: 1,
                lineId: 1),

            [566] = Seed(
                productId: 566,
                partNumber: "L100001",
                productName: "L100001",
                isActive: 1,
                version: 1,
                customerPartNumber: "X1 Housing 2K Housing Assy",
                aliasPartNumber: "X1 Housing 2K Housing Assy",
                description: "X1 Housing 2K Housing Assy",
                ruleId: 566,
                createdBy: "Admin",
                createdOn: new DateTime(2023, 8, 28, 17, 2, 24),
                modifiedBy: "0",
                modifiedOn: new DateTime(2023, 8, 25, 12, 6, 30),
                customerName: "Volkswagen",
                customerId: 1,
                lineId: 1),

            [581] = Seed(
                productId: 581,
                partNumber: "L100002",
                productName: "L100002",
                isActive: 1,
                version: 1,
                customerPartNumber: "X1 Housing PCB LED Assy",
                aliasPartNumber: "X1 Housing PCB LED Assy",
                description: "X1 Housing PCB LED Assy",
                ruleId: 581,
                createdBy: "Admin",
                createdOn: new DateTime(2023, 8, 28, 17, 2, 24),
                modifiedBy: "0",
                modifiedOn: new DateTime(2023, 8, 25, 12, 6, 30),
                customerName: "Volkswagen",
                customerId: 1,
                lineId: 1),

            [629] = Seed(
                productId: 629,
                partNumber: "L90164629",
                productName: "L90164629",
                isActive: 1,
                version: 1,
                customerPartNumber: "PCBA CHMSL X1",
                aliasPartNumber: "PCBA CHMSL X1",
                description: "PCBA CHMSL X1",
                ruleId: 629,
                createdBy: "Admin",
                createdOn: new DateTime(2023, 8, 28, 17, 2, 24),
                modifiedBy: "0",
                modifiedOn: new DateTime(2023, 8, 25, 12, 6, 30),
                customerName: "Volkswagen",
                customerId: 1,
                lineId: 1),

            [630] = Seed(
                productId: 630,
                partNumber: "900290",
                productName: "900290",
                isActive: 1,
                version: 1,
                customerPartNumber: "900290",
                aliasPartNumber: "900290",
                description: "Description",
                ruleId: 630,
                createdBy: "operator@example.com",
                createdOn: new DateTime(2023, 12, 4, 17, 9, 59),
                modifiedBy: "",
                modifiedOn: new DateTime(2023, 12, 4, 17, 9, 59),
                customerName: "Volkswagen",
                customerId: 1,
                lineId: 1),

            [631] = Seed(
                productId: 631,
                partNumber: "900300",
                productName: "900300",
                isActive: 1,
                version: 1,
                customerPartNumber: "900300",
                aliasPartNumber: "900300",
                description: "Description",
                ruleId: 631,
                createdBy: "operator@example.com",
                createdOn: new DateTime(2023, 12, 4, 17, 10, 8),
                modifiedBy: "",
                modifiedOn: new DateTime(2023, 12, 4, 17, 10, 8),
                customerName: "Oemx",
                customerId: 2,
                lineId: 1),

            [632] = Seed(
                productId: 632,
                partNumber: "R150750",
                productName: "Ram Off-Road Bumper",
                isActive: 1,
                version: 1,
                customerPartNumber: "R150750",
                aliasPartNumber: "Ram Bumper Heavy Duty",
                description: "Heavy-duty off-road bumper for Ram trucks",
                ruleId: 1501,
                createdBy: "Admin",
                createdOn: new DateTime(2024, 7, 22, 16, 10, 42),
                modifiedBy: "Admin",
                modifiedOn: new DateTime(2024, 7, 22, 16, 10, 42),
                customerName: "Oemx",
                customerId: 2,
                lineId: 1),

            [633] = Seed(
                productId: 633,
                partNumber: "R150751",
                productName: "Ram LED Headlights",
                isActive: 1,
                version: 1,
                customerPartNumber: "R150751",
                aliasPartNumber: "Ram Headlights LED",
                description: "High-performance LED headlights for enhanced visibility",
                ruleId: 1502,
                createdBy: "Admin",
                createdOn: new DateTime(2024, 7, 22, 16, 10, 42),
                modifiedBy: "Admin",
                modifiedOn: new DateTime(2024, 7, 22, 16, 10, 42),
                customerName: "Oemx",
                customerId: 2,
                lineId: 1),

            [634] = Seed(
                productId: 634,
                partNumber: "R150752",
                productName: "Ram Performance Exhaust",
                isActive: 1,
                version: 1,
                customerPartNumber: "R150752",
                aliasPartNumber: "Ram Exhaust System",
                description: "Performance exhaust system for improved horsepower",
                ruleId: 1503,
                createdBy: "Admin",
                createdOn: new DateTime(2024, 7, 22, 16, 10, 42),
                modifiedBy: "Admin",
                modifiedOn: new DateTime(2024, 7, 22, 16, 10, 42),
                customerName: "BMW",
                customerId: 3,
                lineId: 1),

            [635] = Seed(
                productId: 635,
                partNumber: "R150753",
                productName: "Ram All-Terrain Tires",
                isActive: 1,
                version: 1,
                customerPartNumber: "R150753",
                aliasPartNumber: "Ram Tires AT",
                description: "Durable all-terrain tires for various driving conditions",
                ruleId: 1504,
                createdBy: "Admin",
                createdOn: new DateTime(2024, 7, 22, 16, 10, 42),
                modifiedBy: "Admin",
                modifiedOn: new DateTime(2024, 7, 22, 16, 10, 42),
                customerName: "BMW",
                customerId: 3,
                lineId: 1),

            [636] = Seed(
                productId: 636,
                partNumber: "T200500",
                productName: "Toyota Hybrid Battery Pack",
                isActive: 1,
                version: 1,
                customerPartNumber: "T200500",
                aliasPartNumber: "Toyota Battery Pack Hybrid",
                description: "High-efficiency hybrid battery pack for Toyota vehicles",
                ruleId: 2001,
                createdBy: "Admin",
                createdOn: new DateTime(2024, 7, 22, 16, 11, 56),
                modifiedBy: "Admin",
                modifiedOn: new DateTime(2024, 7, 22, 16, 11, 56),
                customerName: "BMW",
                customerId: 3,
                lineId: 1),

            [637] = Seed(
                productId: 637,
                partNumber: "T200501",
                productName: "Toyota Advanced Navigation System",
                isActive: 1,
                version: 1,
                customerPartNumber: "T200501",
                aliasPartNumber: "Toyota Nav System",
                description: "State-of-the-art navigation system with real-time updates",
                ruleId: 2002,
                createdBy: "Admin",
                createdOn: new DateTime(2024, 7, 22, 16, 11, 56),
                modifiedBy: "Admin",
                modifiedOn: new DateTime(2024, 7, 22, 16, 11, 56),
                customerName: "Mercedes-Benz",
                customerId: 4,
                lineId: 1),

            [638] = Seed(
                productId: 638,
                partNumber: "T200502",
                productName: "Toyota Eco-Friendly Tires",
                isActive: 1,
                version: 1,
                customerPartNumber: "T200502",
                aliasPartNumber: "Toyota Tires Eco",
                description: "Environmentally friendly tires designed for low rolling resistance",
                ruleId: 2003,
                createdBy: "Admin",
                createdOn: new DateTime(2024, 7, 22, 16, 11, 56),
                modifiedBy: "Admin",
                modifiedOn: new DateTime(2024, 7, 22, 16, 11, 56),
                customerName: "Toyota",
                customerId: 5,
                lineId: 1),

            [639] = Seed(
                productId: 639,
                partNumber: "T200503",
                productName: "Toyota High-Performance Brake Pads",
                isActive: 1,
                version: 1,
                customerPartNumber: "T200503",
                aliasPartNumber: "Toyota Brake Pads",
                description: "Durable and high-performance brake pads for enhanced safety",
                ruleId: 2004,
                createdBy: "Admin",
                createdOn: new DateTime(2024, 7, 22, 16, 11, 56),
                modifiedBy: "Admin",
                modifiedOn: new DateTime(2024, 7, 22, 16, 11, 56),
                customerName: "Ford",
                customerId: 6,
                lineId: 1),

            [640] = Seed(
                productId: 640,
                partNumber: "T200504",
                productName: "Toyota Smart Infotainment System",
                isActive: 1,
                version: 1,
                customerPartNumber: "T200504",
                aliasPartNumber: "Toyota Infotainment",
                description: "Next-gen infotainment system with touch screen and connectivity features",
                ruleId: 2005,
                createdBy: "Admin",
                createdOn: new DateTime(2024, 7, 22, 16, 11, 56),
                modifiedBy: "Admin",
                modifiedOn: new DateTime(2024, 7, 22, 16, 11, 56),
                customerName: "Ford",
                customerId: 6,
                lineId: 1),

            [644] = Seed(
                productId: 644,
                partNumber: "L90164629",
                productName: "L90164629",
                isActive: 1,
                version: 1,
                customerPartNumber: "PCBA CHMSL X1",
                aliasPartNumber: "PCBA CHMSL X1",
                description: "PCBA CHMSL X1",
                ruleId: 629,
                createdBy: "Admin",
                createdOn: new DateTime(2023, 8, 28, 17, 2, 24),
                modifiedBy: "Admin",
                modifiedOn: new DateTime(2023, 8, 25, 12, 6, 30),
                customerName: "Volkswagen",
                customerId: 1,
                lineId: 1),

            [645] = Seed(
                productId: 645,
                partNumber: "L100005",
                productName: "L100005",
                isActive: 1,
                version: 1,
                customerPartNumber: "L100005",
                aliasPartNumber: "L100005",
                description: "L100005",
                ruleId: 511,
                createdBy: "Admin",
                createdOn: new DateTime(2023, 8, 28, 17, 2, 24),
                modifiedBy: "Admin",
                modifiedOn: new DateTime(2023, 8, 28, 17, 2, 24),
                customerName: "Oemx",
                customerId: 2,
                lineId: 1),

            [646] = Seed(
                productId: 646,
                partNumber: "L100006",
                productName: "L100006",
                isActive: 1,
                version: 1,
                customerPartNumber: "L100006",
                aliasPartNumber: "L100006",
                description: "L100006",
                ruleId: 618,
                createdBy: "Admin",
                createdOn: new DateTime(2023, 8, 28, 17, 2, 24),
                modifiedBy: "Admin",
                modifiedOn: new DateTime(2023, 8, 28, 17, 2, 24),
                customerName: "Oemx",
                customerId: 2,
                lineId: 1),

            [647] = Seed(
                productId: 647,
                partNumber: "L100007",
                productName: "L100007",
                isActive: 1,
                version: 1,
                customerPartNumber: "L100007",
                aliasPartNumber: "L100007",
                description: "L100007",
                ruleId: 630,
                createdBy: "Admin",
                createdOn: new DateTime(2023, 8, 28, 17, 2, 24),
                modifiedBy: "Admin",
                modifiedOn: new DateTime(2023, 8, 28, 17, 2, 24),
                customerName: "Oemx",
                customerId: 2,
                lineId: 1),

            [648] = Seed(
                productId: 648,
                partNumber: "L100004",
                productName: "L100004",
                isActive: 1,
                version: 1,
                customerPartNumber: "L100004",
                aliasPartNumber: "L100004",
                description: "L100004",
                ruleId: 753,
                createdBy: "Admin",
                createdOn: new DateTime(2023, 8, 28, 17, 2, 24),
                modifiedBy: "Admin",
                modifiedOn: new DateTime(2023, 8, 28, 17, 2, 24),
                customerName: "Oemx",
                customerId: 2,
                lineId: 1),

            [649] = Seed(
                productId: 649,
                partNumber: "L100001",
                productName: "L100001",
                isActive: 1,
                version: 1,
                customerPartNumber: "X1 Housing 2K Housing Assy",
                aliasPartNumber: "X1 Housing 2K Housing Assy",
                description: "X1 Housing 2K Housing Assy",
                ruleId: 566,
                createdBy: "Admin",
                createdOn: new DateTime(2023, 8, 28, 17, 2, 24),
                modifiedBy: "Admin",
                modifiedOn: new DateTime(2023, 8, 25, 12, 6, 30),
                customerName: "BMW",
                customerId: 3,
                lineId: 1),

            [650] = Seed(
                productId: 650,
                partNumber: "431580",
                productName: "431580",
                isActive: 1,
                version: 1,
                customerPartNumber: "431580",
                aliasPartNumber: "431580",
                description: "Description",
                ruleId: 632,
                createdBy: "operator@example.com",
                createdOn: new DateTime(2024, 1, 9, 10, 51, 25),
                modifiedBy: "operator@example.com",
                modifiedOn: new DateTime(2024, 1, 9, 10, 51, 25),
                customerName: "BMW",
                customerId: 3,
                lineId: 1),

            [651] = Seed(
                productId: 651,
                partNumber: "431581",
                productName: "431581",
                isActive: 1,
                version: 1,
                customerPartNumber: "431581",
                aliasPartNumber: "431581",
                description: "Description",
                ruleId: 1632,
                createdBy: "operator@example.com",
                createdOn: new DateTime(2024, 4, 12, 17, 10, 31),
                modifiedBy: "operator@example.com",
                modifiedOn: new DateTime(2024, 4, 12, 17, 10, 31),
                customerName: "Mercedes-Benz",
                customerId: 4,
                lineId: 1),

            [653] = Seed(
                productId: 653,
                partNumber: "L90164629",
                productName: "L90164629",
                isActive: 1,
                version: 1,
                customerPartNumber: "PCBA CHMSL X1",
                aliasPartNumber: "PCBA CHMSL X1",
                description: "PCBA CHMSL X1",
                ruleId: 11,
                createdBy: "Admin",
                createdOn: new DateTime(2023, 8, 28, 17, 2, 24),
                modifiedBy: "0",
                modifiedOn: new DateTime(2023, 8, 25, 12, 6, 30),
                customerName: "Volkswagen",
                customerId: 1,
                lineId: 1),


            // Missing products referenced by RecipeRawData
            [643] = Seed(
                productId: 643,
                partNumber: "P643TEST",
                productName: "Product 643 Test",
                isActive: 1,
                version: 1,
                customerPartNumber: "TEST-643",
                aliasPartNumber: "TEST-643",
                description: "Test Product 643 for Recipe References",
                ruleId: 2006,
                createdBy: "Admin",
                createdOn: new DateTime(2024, 8, 28, 17, 59, 22),
                modifiedBy: "Admin",
                modifiedOn: new DateTime(2024, 8, 28, 17, 59, 22),
                customerName: "Test Customer",
                customerId: 1,
                lineId: 1),

            [652] = Seed(
                productId: 652,
                partNumber: "P652TEST",
                productName: "Product 652 Test",
                isActive: 1,
                version: 1,
                customerPartNumber: "TEST-652",
                aliasPartNumber: "TEST-652",
                description: "Test Product 652 for Recipe References",
                ruleId: 2006,
                createdBy: "Admin",
                createdOn: new DateTime(2024, 8, 28, 17, 59, 22),
                modifiedBy: "Admin",
                modifiedOn: new DateTime(2024, 8, 28, 17, 59, 22),
                customerName: "Test Customer",
                customerId: 1,
                lineId: 1),

            // Duplicate entry [508] removed (keeping first occurrence)
            // Duplicate entry [653] removed (keeping first occurrence)

            // Duplicate entry [653] removed (keeping first occurrence)
            // Duplicate entry [508] removed (keeping first occurrence)

        }.ToImmutableDictionary();

    /// <summary>
    /// Static list for backward compatibility
    /// </summary>
    public static readonly List<Product> FixtureProducts = DictDict.Values.ToList();

    /// <summary>
    /// Sample products for static data strategy (serialized as JSON)
    /// </summary>
    public static string SampleProducts => System.Text.Json.JsonSerializer.Serialize(FixtureProducts);

    /// <summary>
    /// Get a specific Product by ID - O(1) lookup
    /// </summary>
    public static Product? GetProduct(int id) =>
        DictDict.TryGetValue(id, out var product) ? product : null;

    /// <summary>
    /// Get all Product entities
    /// </summary>
    public static IReadOnlyList<Product> GetProducts() => DictDict.Values.ToList();

    /// <summary>
    /// Get all Product entities
    /// </summary>
    public static IReadOnlyList<Product> Fixture => DictDict.Values.ToList();

    /// <summary>
    /// Direct dictionary access for advanced scenarios
    /// </summary>
    public static IImmutableDictionary<int, Product> Dict => DictDict;

    /// <summary>
    /// Check if a Product exists by ID
    /// </summary>
    public static bool Contains(int id) => DictDict.ContainsKey(id);

    /// <summary>
    /// Get count of Dict
    /// </summary>
    public static int Count => DictDict.Count;

    /// <summary>
    /// Get Product by PartNumber - O(n) operation
    /// </summary>
    public static Product? GetByPartNumber(string partNumber) =>
        DictDict.Values.FirstOrDefault(p => p.PartNumber == partNumber);

    /// <summary>
    /// Story 26.A1 (#26): seeds a fully-populated Product fixture. The value scalars are now <c>private set</c>,
    /// so they are seeded through the in-Domain <c>Product.CreateFixture</c> seam; the audit fields
    /// (<c>AuditableEntity</c>) keep public setters and are applied afterwards. Byte-identical to the former
    /// <c>new Product { ... }</c> object initializers.
    /// </summary>
    private static Product Seed(
        int productId,
        string partNumber,
        string productName,
        int isActive,
        int version,
        string customerPartNumber,
        string aliasPartNumber,
        string description,
        int ruleId,
        string createdBy,
        DateTime createdOn,
        string modifiedBy,
        DateTime modifiedOn,
        string customerName,
        int customerId,
        int lineId)
    {
        ActiveStatus activeStatus = isActive;
        var product = Product.CreateFixture(
            productId: productId,
            partNumber: partNumber,
            productName: productName,
            isActive: activeStatus,
            version: version,
            customerPartNumber: customerPartNumber,
            aliasPartNumber: aliasPartNumber,
            description: description,
            customerId: customerId,
            customerName: customerName,
            lineId: lineId,
            ruleId: ruleId);
        product.CreatedBy = createdBy;
        product.CreatedOn = createdOn;
        product.ModifiedBy = modifiedBy;
        product.ModifiedOn = modifiedOn;
        return product;
    }
}
