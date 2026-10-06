// <copyright file="TestGenericDeleteCommand.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Generic.Commands.Delete;

namespace IndTrace.Aggregation.BoundedTests.Generic.Helpers;

/// <summary>
/// Generic test command for delete operations.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class TestGenericDeleteCommand<TEntity> : IDeleteCommand<TEntity> where TEntity : class, new()
{
    public int Id { get; set; }
}