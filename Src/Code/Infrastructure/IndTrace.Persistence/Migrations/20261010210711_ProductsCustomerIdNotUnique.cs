// <copyright file="20261010210711_ProductsCustomerIdNotUnique.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IndTrace.Persistence.Migrations
{
    /// <summary>
    /// Makes the index on <c>Products.CustomerId</c> non-unique, so a customer can have many products (#247), and
    /// names it after its table. The statements are guarded: databases brought to the model by hand-written
    /// scripts may not have the old unique index, and may already have the new one.
    /// </summary>
    public partial class ProductsCustomerIdNotUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Products') AND name = N'IDX.IndTraceData.Customer.CustomerId')
                    DROP INDEX [IDX.IndTraceData.Customer.CustomerId] ON [dbo].[Products];
                """);

            migrationBuilder.Sql(
                """
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Products') AND name = N'IDX.IndTraceData.Products.CustomerId')
                    CREATE INDEX [IDX.IndTraceData.Products.CustomerId] ON [dbo].[Products] ([CustomerId]);
                """);
        }

        /// <inheritdoc />
        /// <remarks>Restoring the unique index fails while any customer has more than one product.</remarks>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Products') AND name = N'IDX.IndTraceData.Products.CustomerId')
                    DROP INDEX [IDX.IndTraceData.Products.CustomerId] ON [dbo].[Products];
                """);

            migrationBuilder.Sql(
                """
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Products') AND name = N'IDX.IndTraceData.Customer.CustomerId')
                    CREATE UNIQUE INDEX [IDX.IndTraceData.Customer.CustomerId] ON [dbo].[Products] ([CustomerId]);
                """);
        }
    }
}
