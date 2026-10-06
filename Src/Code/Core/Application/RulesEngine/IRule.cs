// <copyright file="IRule.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.RulesEngine;

/// <summary>
/// Defines a contract for applying a rule to a target entity asynchronously.
/// </summary>
/// <typeparam name="T">The type of the target entity.</typeparam>
public interface IRule<T>
{
    /// <summary>
    /// Applies the rule to the specified target asynchronously.
    /// </summary>
    /// <param name="target">The target entity to apply the rule to.</param>
    /// <returns>A Result indicating success or failure of the rule application.</returns>
    Task<Result> ApplyAsync(T target);
}