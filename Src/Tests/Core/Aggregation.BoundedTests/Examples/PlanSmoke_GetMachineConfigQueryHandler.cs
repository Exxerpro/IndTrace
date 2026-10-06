// <copyright file="PlanSmoke_GetMachineConfigQueryHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.PlanSmoke;

public class PlanSmoke_GetMachineConfigQueryHandler
{
    [Fact(Skip = "Pending refactor wiring")]
    [Trait("PlanSmoke", "Machine-Config")]
    public async Task Known_PartNumber_L100003()
    {
        // Input: partNumber = L100003
        // Expect: Success; VM with product/workflows; PLCs from Machines/PLCs data; counts logged
        await Task.CompletedTask;
    }

    [Fact(Skip = "Pending refactor wiring")]
    [Trait("PlanSmoke", "Machine-Config")]
    public async Task Invalid_PartNumber_TooShort()
    {
        // Input: partNumber = L (len < 3)
        // Expect: Failure(validation); no data loads
        await Task.CompletedTask;
    }

    [Fact(Skip = "Pending refactor wiring")]
    [Trait("PlanSmoke", "Machine-Config")]
    public async Task Non_Existent_PartNumber()
    {
        // Input: partNumber = ZZZ9999
        // Expect: Failure("Product not found")
        await Task.CompletedTask;
    }
}

