// <copyright file="TestCommand.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.HybridCacheTest;

public class TestCommand
{
    public int Id { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public string[] Includes { get; set; } = Array.Empty<string>();
    public string Name { get; set; } = string.Empty;
}