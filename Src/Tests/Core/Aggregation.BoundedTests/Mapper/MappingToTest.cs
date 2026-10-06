// <copyright file="MappingToTest.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.Mapper;
/// <summary>
/// Represents the MappingToTest.
/// </summary>

public class MappingToTest
{
    /// <summary>
    /// Gets or sets the MapMethod.
    /// </summary>
    public required MethodInfo MapMethod { get; set; }
    /// <summary>
    /// Gets or sets the SourceType.
    /// </summary>
    public required Type SourceType { get; set; }
    /// <summary>
    /// Gets or sets the TargetType.
    /// </summary>
    public required Type TargetType { get; set; }
}