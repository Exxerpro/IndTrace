// <copyright file="IndTraceDbIdentityDesignFactory.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Identity.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

/// <summary>
/// Design-time factory used by the EF Core tooling (migrations add / database update) to
/// construct an <see cref="IndTraceDbIdentity"/> without spinning up the full application host.
/// Targets the named SQL Server instance that accepts integrated authentication.
/// </summary>
public class IndTraceDbIdentityDesignFactory : IDesignTimeDbContextFactory<IndTraceDbIdentity>
{
    /// <summary>
    /// Creates a configured <see cref="IndTraceDbIdentity"/> for design-time tooling.
    /// </summary>
    /// <param name="args">Arguments passed by the EF Core tooling (unused).</param>
    /// <returns>A configured <see cref="IndTraceDbIdentity"/> instance.</returns>
    public IndTraceDbIdentity CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<IndTraceDbIdentity>();

        options.UseSqlServer(
            "Server=localhost;Database=IndTraceIdentity;Integrated Security=true;TrustServerCertificate=true",
            sql => sql.MigrationsAssembly("IndTrace.Identity"));

        return new IndTraceDbIdentity(options.Options);
    }
}
