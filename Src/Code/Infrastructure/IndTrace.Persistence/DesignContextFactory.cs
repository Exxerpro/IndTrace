// <copyright file="DesignContextFactory.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Persistence.DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IndTrace.Persistence;
/// <summary>
/// Represents the DesignContextFactory.
/// </summary>

public class DesignContextFactory : IDesignTimeDbContextFactory<IndTraceDbContext>
{
    /// <summary>
    /// Executes CreateDbContext operation.
    /// </summary>
    /// <param name="args">The args.</param>
    /// <returns>The result of CreateDbContext.</returns>
    public IndTraceDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<IndTraceDbContext>();

        options.UseSqlServer("Server=localhost;Database=IndTraceData;Trusted_Connection=True;Encrypt=False");

        return new IndTraceDbContext(options.Options);
    }
}