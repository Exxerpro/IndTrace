// <copyright file="PLCData.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.TestData.StaticData;

//[Fix] 
//CLAUDE
//Date: 27/08/2025 
//Reason: [Pattern Consolidation] - Moved static PLC data from DbContextStaticData for hybrid strategy optimization

/// <summary>
/// Raw PLC test data for static loading strategy - optimized for fast access.
/// This is a good candidate for static conversion due to stable, small dataset.
/// </summary>
internal static class PLCData
{

    /// <summary>
    /// Gets the predefined list of PLCs.
    /// </summary>
    public static List<Plc>
        GetPlcs() => new()
        {
            // Story 26.A2 (#26): the Plc value scalars are now private set, so these fixtures are seeded through the
            // in-Domain Plc.CreateFixture seam. Byte-identical to the former object initializers (unset fields keep
            // their defaults: Enabled = ActiveStatus.None, the descriptive strings = string.Empty).
            Plc.CreateFixture(
                plcId: 500,
                machineId: 500,
                enabled: IndTrace.Domain.Enum.ActiveStatus.None,
                name: "S7-1500",
                ipAddress: "192.168.0.140",
                plcType: "S7-1500",
                plcBrand: string.Empty,
                options: string.Empty,
                commLibrary: string.Empty,
                brandOwner: string.Empty),
            Plc.CreateFixture(
                plcId: 600,
                machineId: 600,
                enabled: IndTrace.Domain.Enum.ActiveStatus.None,
                name: "S7-1500",
                ipAddress: "192.168.1.45",
                plcType: "S7-1500",
                plcBrand: string.Empty,
                options: string.Empty,
                commLibrary: string.Empty,
                brandOwner: string.Empty),
        };

}
