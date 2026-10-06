// <copyright file="PlanSmoke_GetPlcDetailQueryHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.PlanSmoke;

public class PlanSmoke_GetPlcDetailQueryHandler
{
    [Fact(Skip = "Pending refactor wiring")]
    [Trait("PlanSmoke", "PLC-Detail")]
    public async Task Plc_100_Success()
    {
        // Input: plcId = 100
        // Expect: Success; PlcDto.MachineId = 100; variable groups count >= 0; durations logged
        await Task.CompletedTask;
    }

    [Fact(Skip = "Pending refactor wiring")]
    [Trait("PlanSmoke", "PLC-Detail")]
    public async Task Plc_NotFound()
    {
        // Input: plcId = 42424
        // Expect: Failure("Plc not found")
        await Task.CompletedTask;
    }
}

