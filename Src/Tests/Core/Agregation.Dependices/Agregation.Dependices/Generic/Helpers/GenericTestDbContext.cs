// <copyright file="GenericTestDbContext.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Agregation.Dependices.Generic.Helpers;

/// <summary>
/// Represents the GenericTestDbContext.
/// </summary>
public class GenericTestDbContext : DbContext
{
    /// <summary>
    /// Gets or sets the TestEntities.
    /// </summary>
    public DbSet<GenericTestEntity> TestEntities { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase($"GenericTestDb_{Guid.NewGuid()}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GenericTestEntity>().HasKey(e => e.Id);
        base.OnModelCreating(modelBuilder);
    }
}