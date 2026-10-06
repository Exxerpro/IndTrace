// <copyright file="PlanSmoke_GetBarCodeDetailQueryQrCodeHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.PlanSmoke;

public class PlanSmoke_GetBarCodeDetailQueryQrCodeHandler
{
    [Fact(Skip = "Pending refactor wiring")]
    [Trait("PlanSmoke", "BarCodes-Detail-QR")]
    public async Task Known_Label_WS100_QR()
    {
        // Input: label = L1AL100003232372501
        // Expect: Success; MachineId = 100; matches monitor handler behavior/logs
        await Task.CompletedTask;
    }

    [Fact(Skip = "Pending refactor wiring")]
    [Trait("PlanSmoke", "BarCodes-Detail-QR")]
    public async Task Known_Label_WS300_QR()
    {
        // Input: label = L1AL100003232372516
        // Expect: Success; MachineId = 300
        await Task.CompletedTask;
    }

    [Fact(Skip = "Pending refactor wiring")]
    [Trait("PlanSmoke", "BarCodes-Detail-QR")]
    public async Task Invalid_Label_QR()
    {
        // Input: label = Q
        // Expect: Failure("Label must be at least 3 characters")
        await Task.CompletedTask;
    }
}

