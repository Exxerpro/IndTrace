// <copyright file="ProductsConfiguration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using IndTrace.Domain.Enum;
using IndTrace.Domain.ValueObjects;
using IndTrace.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndTrace.Persistence.Configurations;
/// <summary>
/// Represents the ProductsConfiguration.
/// </summary>

public class ProductsConfiguration : IEntityTypeConfiguration<Product>
{
    /// <summary>
    /// Executes Configure operation.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public void Configure(EntityTypeBuilder<Product> builder)
    {

        // Apply the base configuration
        new AuditableEntityConfiguration<Product>().Configure(builder);

        builder.HasKey(e => e.ProductId)
            .HasName("PK.IndTraceData.Products.ProductId");

        // Story 35.D2 Cluster 4 (#35): Product.ProductId is the strongly-typed ProductId struct KEY, mapped to the SAME
        // unchanged int identity column via the shared byte-preserving converter (model -> DB via id.Value; DB ->
        // model via new ProductId(value)). The generated identity round-trips through id.Value. Making the PK the
        // converted struct is what lets the inbound Rule/ProductSpec/BarCode FKs be adopted typed (an int FK targeting
        // a ProductId principal key detonates the model).
        builder.Property(e => e.ProductId)
            .HasColumnName(nameof(Product.ProductId))
            .HasColumnType("int")
            .HasIntIdConversion(v => new ProductId(v))
            .ValueGeneratedOnAdd()
            .UseIdentityColumn(1, 1)
            .IsRequired();

        builder.Property(e => e.PartNumber)
            .HasColumnName(nameof(Product.PartNumber))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(80);

        builder.Property(e => e.ProductName)
            .HasColumnName(nameof(Product.ProductName))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(80);

        // EF Core value converter: persist the ActiveStatus smart enum as the int column "IsActive".
        // The from-DB side NORMALIZES (positive -> Active, negative -> Inactive, zero -> None) so legacy
        // rows storing arbitrary positive "active" values (>= 2) materialize as Active instead of stranding
        // on the Invalid sentinel. The column type and the historical "positive = active" contract are unchanged.
        // #117 F4: a NULL enum writes the ActiveStatus Invalid sentinel (int.MinValue), never 0/None, and the
        // read maps EXACTLY that sentinel back to Invalid (an exact-match arm — every other stored int keeps
        // the legacy normalization above, so no production value 0/1/-1/legacy changes meaning).
        builder.Property(e => e.IsActive)
            .HasColumnName(nameof(Product.IsActive))
            .HasColumnType("int")
            .IsRequired()
            .HasConversion(
                v => v != null ? v.Value : ActiveStatus.Invalid.Value,                              // To DB: ActiveStatus -> int (null -> Invalid sentinel)
                v => v == ActiveStatus.Invalid.Value ? ActiveStatus.Invalid : (v > 0 ? ActiveStatus.Active : (v < 0 ? ActiveStatus.Inactive : ActiveStatus.None))); // From DB: sentinel -> Invalid; positive -> Active

        builder.Property(e => e.Version)
            .HasColumnName(nameof(Product.Version))
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.CustomerPartNumber)
            .HasColumnName(nameof(Product.CustomerPartNumber))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(80);

        builder.Property(e => e.AliasPartNumber)
            .HasColumnName(nameof(Product.AliasPartNumber))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(80);

        builder.Property(e => e.Description)
            .HasColumnName(nameof(Product.Description))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(240);


        builder.HasIndex(e => e.ProductId)
            .HasDatabaseName("IDX.IndTraceData.Products.ProductId")
            .IsUnique();

        builder.HasIndex(e => e.CustomerId)
            .HasDatabaseName("IDX.IndTraceData.Customer.CustomerId")
            .IsUnique();

        builder.Property(e => e.CustomerName)
            .HasColumnName(nameof(Product.CustomerName))
            .IsRequired()
            .IsUnicode()
            .HasMaxLength(240);

        builder.Property(e => e.CustomerId)
            .HasColumnName(nameof(Product.CustomerId))
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.RuleId)
            .HasColumnName(nameof(Product.RuleId))
            .HasColumnType("int")
            .IsRequired();

        builder.Property(e => e.LineId)
            .HasColumnName(nameof(Product.LineId))
            .HasColumnType("int")
            .IsRequired();

        builder.Ignore(e => e.Line);
        builder.Ignore(e => e.Customer);


        builder.ToTable("Products");
    }
}