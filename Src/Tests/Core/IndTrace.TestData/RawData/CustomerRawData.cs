// <copyright file="CustomerRawData.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using IndTrace.Domain.Entities;

namespace IndTrace.TestData.RawData;

/// <summary>
/// Static test data for Customer entities with O(1) lookup.
/// Generated with ImmutableDictionary for thread-safety and performance.
/// </summary>
internal static class CustomerRawData
{
    private static readonly ImmutableDictionary<int, Customer> _customersDict =
        new Dictionary<int, Customer>
        {
            [1] = new Customer
            {
                CustomerId = 1,
                Name = "Volkswagen",
                IsActive = true,
                CreatedBy = "Exxerpro",
                CreatedOn = new DateTime(2024, 7, 22, 15, 38, 43),
                ModifiedBy = "Exxerpro",
                ModifiedOn = new DateTime(2024, 7, 22, 15, 38, 43)
            },
            [2] = new Customer
            {
                CustomerId = 2,
                Name = "Oemx",
                IsActive = true,
                CreatedBy = "Exxerpro",
                CreatedOn = new DateTime(2024, 7, 22, 15, 38, 43),
                ModifiedBy = "Exxerpro",
                ModifiedOn = new DateTime(2024, 7, 22, 15, 38, 43)
            },
            [3] = new Customer
            {
                CustomerId = 3,
                Name = "BMW",
                IsActive = true,
                CreatedBy = "Exxerpro",
                CreatedOn = new DateTime(2024, 7, 22, 15, 38, 43),
                ModifiedBy = "Exxerpro",
                ModifiedOn = new DateTime(2024, 7, 22, 15, 38, 43)
            },
            [4] = new Customer
            {
                CustomerId = 4,
                Name = "Mercedes-Benz",
                IsActive = true,
                CreatedBy = "Exxerpro",
                CreatedOn = new DateTime(2024, 7, 22, 15, 38, 43),
                ModifiedBy = "Exxerpro",
                ModifiedOn = new DateTime(2024, 7, 22, 15, 38, 43)
            },
            [5] = new Customer
            {
                CustomerId = 5,
                Name = "Toyota",
                IsActive = true,
                CreatedBy = "Exxerpro",
                CreatedOn = new DateTime(2024, 7, 22, 15, 38, 43),
                ModifiedBy = "Exxerpro",
                ModifiedOn = new DateTime(2024, 7, 22, 15, 38, 43)
            },
            [6] = new Customer
            {
                CustomerId = 6,
                Name = "Ford",
                IsActive = true,
                CreatedBy = "Exxerpro",
                CreatedOn = new DateTime(2024, 7, 22, 15, 38, 43),
                ModifiedBy = "Exxerpro",
                ModifiedOn = new DateTime(2024, 7, 22, 15, 38, 43)
            },
            [7] = new Customer
            {
                CustomerId = 7,
                Name = "Honda",
                IsActive = true,
                CreatedBy = "Exxerpro",
                CreatedOn = new DateTime(2024, 7, 22, 15, 38, 43),
                ModifiedBy = "Exxerpro",
                ModifiedOn = new DateTime(2024, 7, 22, 15, 38, 43)
            },
            [8] = new Customer
            {
                CustomerId = 8,
                Name = "Chevrolet",
                IsActive = true,
                CreatedBy = "Exxerpro",
                CreatedOn = new DateTime(2024, 7, 22, 15, 38, 43),
                ModifiedBy = "Exxerpro",
                ModifiedOn = new DateTime(2024, 7, 22, 15, 38, 43)
            },
            [9] = new Customer
            {
                CustomerId = 9,
                Name = "Nissan",
                IsActive = true,
                CreatedBy = "Exxerpro",
                CreatedOn = new DateTime(2024, 7, 22, 15, 38, 43),
                ModifiedBy = "Exxerpro",
                ModifiedOn = new DateTime(2024, 7, 22, 15, 38, 43)
            },
            [10] = new Customer
            {
                CustomerId = 10,
                Name = "Hyundai",
                IsActive = true,
                CreatedBy = "Exxerpro",
                CreatedOn = new DateTime(2024, 7, 22, 15, 38, 43),
                ModifiedBy = "Exxerpro",
                ModifiedOn = new DateTime(2024, 7, 22, 15, 38, 43)
            },
            [11] = new Customer
            {
                CustomerId = 11,
                Name = "Kia",
                IsActive = true,
                CreatedBy = "Exxerpro",
                CreatedOn = new DateTime(2024, 7, 22, 15, 45, 31),
                ModifiedBy = "Exxerpro",
                ModifiedOn = new DateTime(2024, 7, 22, 15, 45, 31)
            },
            [12] = new Customer
            {
                CustomerId = 12,
                Name = "Mazda",
                IsActive = true,
                CreatedBy = "Exxerpro",
                CreatedOn = new DateTime(2024, 7, 22, 15, 45, 31),
                ModifiedBy = "Exxerpro",
                ModifiedOn = new DateTime(2024, 7, 22, 15, 45, 31)
            },
            [13] = new Customer
            {
                CustomerId = 13,
                Name = "Subaru",
                IsActive = true,
                CreatedBy = "Exxerpro",
                CreatedOn = new DateTime(2024, 7, 22, 15, 45, 31),
                ModifiedBy = "Exxerpro",
                ModifiedOn = new DateTime(2024, 7, 22, 15, 45, 31)
            },
            [14] = new Customer
            {
                CustomerId = 14,
                Name = "Tesla",
                IsActive = true,
                CreatedBy = "Exxerpro",
                CreatedOn = new DateTime(2024, 7, 22, 15, 45, 31),
                ModifiedBy = "Exxerpro",
                ModifiedOn = new DateTime(2024, 7, 22, 15, 45, 31)
            },
            [15] = new Customer
            {
                CustomerId = 15,
                Name = "Porsche",
                IsActive = true,
                CreatedBy = "Exxerpro",
                CreatedOn = new DateTime(2024, 7, 22, 15, 45, 31),
                ModifiedBy = "Exxerpro",
                ModifiedOn = new DateTime(2024, 7, 22, 15, 45, 31)
            },
            [16] = new Customer
            {
                CustomerId = 16,
                Name = "Jaguar",
                IsActive = true,
                CreatedBy = "Exxerpro",
                CreatedOn = new DateTime(2024, 7, 22, 15, 45, 31),
                ModifiedBy = "Exxerpro",
                ModifiedOn = new DateTime(2024, 7, 22, 15, 45, 31)
            },
            [17] = new Customer
            {
                CustomerId = 17,
                Name = "Land Rover",
                IsActive = true,
                CreatedBy = "Exxerpro",
                CreatedOn = new DateTime(2024, 7, 22, 15, 45, 31),
                ModifiedBy = "Exxerpro",
                ModifiedOn = new DateTime(2024, 7, 22, 15, 45, 31)
            },
            [18] = new Customer
            {
                CustomerId = 18,
                Name = "Volvo",
                IsActive = true,
                CreatedBy = "Exxerpro",
                CreatedOn = new DateTime(2024, 7, 22, 15, 45, 31),
                ModifiedBy = "Exxerpro",
                ModifiedOn = new DateTime(2024, 7, 22, 15, 45, 31)
            },
            [19] = new Customer
            {
                CustomerId = 19,
                Name = "Mitsubishi",
                IsActive = true,
                CreatedBy = "Exxerpro",
                CreatedOn = new DateTime(2024, 7, 22, 15, 45, 31),
                ModifiedBy = "Exxerpro",
                ModifiedOn = new DateTime(2024, 7, 22, 15, 45, 31)
            },
            [20] = new Customer
            {
                CustomerId = 20,
                Name = "Lexus",
                IsActive = true,
                CreatedBy = "Exxerpro",
                CreatedOn = new DateTime(2024, 7, 22, 15, 45, 31),
                ModifiedBy = "Exxerpro",
                ModifiedOn = new DateTime(2024, 7, 22, 15, 45, 31)
            },
            [21] = new Customer
            {
                CustomerId = 21,
                Name = "Infiniti",
                IsActive = true,
                CreatedBy = "Exxerpro",
                CreatedOn = new DateTime(2024, 7, 22, 15, 45, 31),
                ModifiedBy = "Exxerpro",
                ModifiedOn = new DateTime(2024, 7, 22, 15, 45, 31)
            },
            [22] = new Customer
            {
                CustomerId = 22,
                Name = "Acura",
                IsActive = true,
                CreatedBy = "Exxerpro",
                CreatedOn = new DateTime(2024, 7, 22, 15, 45, 31),
                ModifiedBy = "Exxerpro",
                ModifiedOn = new DateTime(2024, 7, 22, 15, 45, 31)
            },
            [23] = new Customer
            {
                CustomerId = 23,
                Name = "Cadillac",
                IsActive = true,
                CreatedBy = "Exxerpro",
                CreatedOn = new DateTime(2024, 7, 22, 15, 45, 31),
                ModifiedBy = "Exxerpro",
                ModifiedOn = new DateTime(2024, 7, 22, 15, 45, 31)
            },
            [24] = new Customer
            {
                CustomerId = 24,
                Name = "Chrysler",
                IsActive = true,
                CreatedBy = "Exxerpro",
                CreatedOn = new DateTime(2024, 7, 22, 15, 45, 31),
                ModifiedBy = "Exxerpro",
                ModifiedOn = new DateTime(2024, 7, 22, 15, 45, 31)
            },
            [25] = new Customer
            {
                CustomerId = 25,
                Name = "Dodge",
                IsActive = true,
                CreatedBy = "Exxerpro",
                CreatedOn = new DateTime(2024, 7, 22, 15, 45, 31),
                ModifiedBy = "Exxerpro",
                ModifiedOn = new DateTime(2024, 7, 22, 15, 45, 31)
            },
            [26] = new Customer
            {
                CustomerId = 26,
                Name = "Jeep",
                IsActive = true,
                CreatedBy = "Exxerpro",
                CreatedOn = new DateTime(2024, 7, 22, 15, 45, 31),
                ModifiedBy = "Exxerpro",
                ModifiedOn = new DateTime(2024, 7, 22, 15, 45, 31)
            },
            [27] = new Customer
            {
                CustomerId = 27,
                Name = "Ram",
                IsActive = true,
                CreatedBy = "Exxerpro",
                CreatedOn = new DateTime(2024, 7, 22, 15, 45, 31),
                ModifiedBy = "Exxerpro",
                ModifiedOn = new DateTime(2024, 7, 22, 15, 45, 31)
            },
            [28] = new Customer
            {
                CustomerId = 28,
                Name = "Buick",
                IsActive = true,
                CreatedBy = "Exxerpro",
                CreatedOn = new DateTime(2024, 7, 22, 15, 45, 31),
                ModifiedBy = "Exxerpro",
                ModifiedOn = new DateTime(2024, 7, 22, 15, 45, 31)
            },
            [29] = new Customer
            {
                CustomerId = 29,
                Name = "GMC",
                IsActive = true,
                CreatedBy = "Exxerpro",
                CreatedOn = new DateTime(2024, 7, 22, 15, 45, 31),
                ModifiedBy = "Exxerpro",
                ModifiedOn = new DateTime(2024, 7, 22, 15, 45, 31)
            },
            [30] = new Customer
            {
                CustomerId = 30,
                Name = "Alfa Romeo",
                IsActive = true,
                CreatedBy = "Exxerpro",
                CreatedOn = new DateTime(2024, 7, 22, 15, 45, 31),
                ModifiedBy = "Exxerpro",
                ModifiedOn = new DateTime(2024, 7, 22, 15, 45, 31)
            },
            [31] = new Customer
            {
                CustomerId = 31,
                Name = "Fiat",
                IsActive = true,
                CreatedBy = "Exxerpro",
                CreatedOn = new DateTime(2024, 7, 22, 15, 45, 31),
                ModifiedBy = "Exxerpro",
                ModifiedOn = new DateTime(2024, 7, 22, 15, 45, 31)
            },
            [32] = new Customer
            {
                CustomerId = 32,
                Name = "Peugeot",
                IsActive = true,
                CreatedBy = "Exxerpro",
                CreatedOn = new DateTime(2024, 7, 22, 15, 45, 31),
                ModifiedBy = "Exxerpro",
                ModifiedOn = new DateTime(2024, 7, 22, 15, 45, 31)
            },
            [33] = new Customer
            {
                CustomerId = 33,
                Name = "Renault",
                IsActive = true,
                CreatedBy = "Exxerpro",
                CreatedOn = new DateTime(2024, 7, 22, 15, 45, 31),
                ModifiedBy = "Exxerpro",
                ModifiedOn = new DateTime(2024, 7, 22, 15, 45, 31)
            }
        }.ToImmutableDictionary();

    /// <summary>
    /// Static list for backward compatibility
    /// </summary>
    public static readonly List<Customer> Fixture = _customersDict.Values.ToList();

    /// <summary>
    /// Get a specific Customer by ID - O(1) lookup
    /// </summary>
    public static Customer? GetCustomer(int id) =>
        _customersDict.TryGetValue(id, out var customer) ? customer : null;

    /// <summary>
    /// Get all Customer entities
    /// </summary>
    public static IReadOnlyList<Customer> GetCustomers() =>
        _customersDict.Values.ToList();

    /// <summary>
    /// Get count of Customers
    /// </summary>
    public static int Count => _customersDict.Count;

    /// <summary>
    /// Get Customer by Name - O(n) operation
    /// </summary>
    public static Customer? GetByName(string name) =>
        _customersDict.Values.FirstOrDefault(c => c.Name == name);
}
