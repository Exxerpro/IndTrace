// <copyright file="IndTraceDbIdentity.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IndTrace.Monitor.Data;

/// <summary>
/// Database context for IndTrace application with Identity framework support.
/// </summary>
/// <param name="options">The database context options.</param>
public class IndTraceDbIdentity(DbContextOptions<IndTraceDbIdentity> options) : IdentityDbContext<ApplicationUser>(options)
{
}