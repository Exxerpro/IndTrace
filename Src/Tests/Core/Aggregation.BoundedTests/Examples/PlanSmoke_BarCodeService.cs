// <copyright file="PlanSmoke_BarCodeService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.PlanSmoke;

public class PlanSmoke_BarCodeService
{
    [Fact(Skip = "Pending refactor wiring")]
    [Trait("PlanSmoke", "BarCode-Service")]
    public async Task List_By_Product_508()
    {
        // Input: productId = 508
        // Expect: Success; items >= 1; durations logged
        await Task.CompletedTask;
    }

    [Fact(Skip = "Pending refactor wiring")]
    [Trait("PlanSmoke", "BarCode-Service")]
    public async Task Consecutive_Next_From_Last()
    {
        // Input: last label from highest L1AL1000032323725xx
        // Expect: Success; next calculation according to policy; no exceptions on gaps
        await Task.CompletedTask;
    }

    [Fact(Skip = "Pending refactor wiring")]
    [Trait("PlanSmoke", "BarCode-Service")]
    public async Task Invalid_Input_Label()
    {
        // Input: empty label
        // Expect: Failure("Label must be at least 3 characters")
        await Task.CompletedTask;
    }
}

