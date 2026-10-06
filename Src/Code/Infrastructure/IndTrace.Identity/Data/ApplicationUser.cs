// <copyright file="ApplicationUser.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Identity.Data;

using IndTrace.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

/// <summary>
/// Represents an application user for identity management and authentication.
/// </summary>
/// <remarks>
/// Implements <see cref="IIndTraceApplicationUser"/> so the identity abstraction stays valid while
/// giving EF Core a concrete <see cref="IdentityUser"/> to bind its store to. Every interface member is
/// already satisfied structurally by the <see cref="IdentityUser"/> base, so no additional members are needed.
/// </remarks>
public class ApplicationUser : IdentityUser, IIndTraceApplicationUser
{
}
