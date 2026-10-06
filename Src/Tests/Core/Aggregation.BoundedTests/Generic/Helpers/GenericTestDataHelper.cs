// <copyright file="GenericTestDataHelper.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.Generic.Helpers;

public static class GenericTestDataHelper
{
    public static List<GenericTestEntity> CreateTestEntities(int count)
    {
        return Enumerable.Range(1, count)
            .Select(i => new GenericTestEntity
            {
                Id = i,
                Name = $"Entity_{i:D3}",
                Description = $"Test entity number {i}",
                CreatedAt = DateTime.UtcNow.AddHours(-i)
            })
            .ToList();
    }

    public static List<GenericTestEntity> CreateManufacturingTestEntities(string industry, string equipment, int count)
    {
        return Enumerable.Range(1, count)
            .Select(i => new GenericTestEntity
            {
                Id = i,
                Name = $"{industry}_Component_{i:D3}",
                Description = $"{equipment} - Part {i}",
                CreatedAt = DateTime.UtcNow.AddMinutes(-i * 5)
            })
            .ToList();
    }
}