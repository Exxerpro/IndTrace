// <copyright file="ProductCustomerIndexTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using IndTrace.Persistence.DBContext;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Architecture.Tests.Layers;

/// <summary>
/// A customer has many products (#247). The index on <c>Products.CustomerId</c> serves lookups by customer and
/// must not be unique; a unique index there makes a customer's second product fail at the database.
/// <para>
/// Checked on the built model: the aggregation tests run on EF InMemory, which does not enforce unique indexes.
/// </para>
/// </summary>
public class ProductCustomerIndexTests
{
    /// <summary>
    /// The <c>Products.CustomerId</c> index exists and is not unique.
    /// </summary>
    [Fact]
    public void ProductsCustomerIdIndex_IsNotUnique()
    {
        using var context = new IndTraceDbContext(new DbContextOptionsBuilder<IndTraceDbContext>()
            .UseSqlServer("Server=architecture-model-build-only;Database=none;Trusted_Connection=True;")
            .Options);
        var index = context.Model?.FindEntityType(typeof(Product))?.GetIndexes()
            .SingleOrDefault(i => i.Properties.Select(p => p.Name).SequenceEqual([nameof(Product.CustomerId)]));

        // A missing index makes both values null, so both assertions also fail when it is absent.
        (index?.GetDatabaseName()).ShouldBe("IDX.IndTraceData.Products.CustomerId");
        (index?.IsUnique).ShouldBe(false);
    }
}
