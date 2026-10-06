// <copyright file="TesterUserService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.Services;
/// <summary>
/// Represents the TesterUserService.
/// </summary>

public class TesterUserService : IIndTraceUserService
{
    /// <summary>
    /// Gets the CurrentUserId. Deliberately DISTINCT from <see cref="CurrentUserName"/> (mirroring the
    /// real service, where the id is an opaque token and the name is human-readable) so the audit-stamping
    /// regression guard (#52) can detect a revert to stamping ModifiedBy from the id instead of the name.
    /// </summary>
    public Task<string> CurrentUserId { get; } = Task.FromResult("user-id-0001");
    /// <summary>
    /// Gets or sets the CurrentUserName.
    /// </summary>
    public Task<string> CurrentUserName { get; } = Task.FromResult("Admin");
    /// <summary>
    /// Gets or sets the IsAuthenticated.
    /// </summary>
    public Task<bool> IsAuthenticated { get; } = Task.FromResult(true);
}