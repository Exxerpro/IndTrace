// <copyright file="TestCachePartitionProvider.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Agregation.Dependices.Dependencies;

/// <summary>
/// Test implementation that provides per-test GUID partition
/// </summary>
public class TestCachePartitionProvider : ICachePartitionProvider
{
    private readonly string _partitionGuid;

    /// <summary>
    /// Creates a new instance with a unique partition GUID
    /// </summary>
    public TestCachePartitionProvider()
    {
        _partitionGuid = Guid.NewGuid().ToString("N");
    }

    /// <summary>
    /// Returns the unique partition GUID for this test
    /// </summary>
    public string GetPrefix() => _partitionGuid;
}