// <copyright file="ApplicationUser.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Microsoft.AspNetCore.Identity;

namespace IndTrace.Monitor.Data;

/// <summary>
/// Represents an application user with Identity framework capabilities.
/// Add profile data for application users by adding properties to the ApplicationUser class.
/// </summary>
public class ApplicationUser : IdentityUser
{
}