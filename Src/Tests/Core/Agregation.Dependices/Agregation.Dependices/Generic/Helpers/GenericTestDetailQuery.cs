// <copyright file="GenericTestDetailQuery.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Generic.Commands.Detail;

namespace IndTrace.Agregation.Dependices.Generic.Helpers;

/// <summary>
/// Represents the GenericTestDetailQuery.
/// </summary>

public class GenericTestDetailQuery : IDetailQuery<GenericTestEntity>
{
    /// <summary>
    /// Gets or sets the EntitieId.
    /// </summary>
    public int Id { get; set; }
}