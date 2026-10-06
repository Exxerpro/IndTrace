// <copyright file="IndTraceDbIdentity.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Identity.Data;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Represents the Entity Framework database context for application identity management.
/// </summary>
/// <param name="options">The options to be used by the DbContext.</param>
public class IndTraceDbIdentity(DbContextOptions<IndTraceDbIdentity> options) : IdentityDbContext<ApplicationUser>(options)
{
}
