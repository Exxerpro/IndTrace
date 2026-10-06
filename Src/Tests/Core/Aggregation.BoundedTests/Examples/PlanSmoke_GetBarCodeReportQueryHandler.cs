// <copyright file="PlanSmoke_GetBarCodeReportQueryHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.PlanSmoke;

public class PlanSmoke_GetBarCodeReportQueryHandler
{
    [Fact(Skip = "Pending refactor wiring")]
    [Trait("PlanSmoke", "BarCodes-Detail-Report")]
    public async Task Found_Label_WS100()
    {
        // Input: label = L1AL100003232372501 (MachineId 100)
        // Expect: Success; VM shows MachineId = 100; counts >= 1; durations logged
        await Task.CompletedTask;
    }

    [Fact(Skip = "Pending refactor wiring")]
    [Trait("PlanSmoke", "BarCodes-Detail-Report")]
    public async Task Found_Label_WS300()
    {
        // Input: label = L1AL100003232372516 (MachineId 300)
        // Expect: Success; MachineId = 300; counts >= 1
        await Task.CompletedTask;
    }

    [Fact(Skip = "Pending refactor wiring")]
    [Trait("PlanSmoke", "BarCodes-Detail-Report")]
    public async Task Label_NotFound()
    {
        // Input: label = L1AL100003232372599
        // Expect: Failure("BarCode not found"); no data loads attempted
        await Task.CompletedTask;
    }
}

