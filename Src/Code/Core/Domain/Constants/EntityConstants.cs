// <copyright file="EntityConstants.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Constants;

/// <summary>
/// Contains entity-related constants for consistent naming across the application.
/// </summary>
public static class EntityConstants
{
    /// <summary>
    /// Standard primary key suffix for Entity Framework configurations.
    /// Usage: builder.HasKey(e => e.{nameof(ClassName)}Id);
    /// </summary>
    public const string Id = "Id";
}