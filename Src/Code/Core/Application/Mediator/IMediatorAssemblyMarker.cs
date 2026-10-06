// <copyright file="IMediatorAssemblyMarker.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Mediator;

/// <summary>
/// Marker interface for identifying the application assembly for mediator registration.
/// </summary>
/// <remarks>
/// This interface is used by dependency injection containers to locate the application assembly
/// during automated registration of mediator handlers and services.
/// </remarks>
public interface IMediatorAssemblyMarker
{
}