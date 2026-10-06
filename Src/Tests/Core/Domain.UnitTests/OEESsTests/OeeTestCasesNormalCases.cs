// <copyright file="OeeTestCasesNormalCases.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.OEESsTests;

public static class OeeTestCasesNormalCases
{
    /// <summary>
    /// good cases for OEE calculations
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<object[]> GetWarningCases()
    {
        yield return new object[] {
            "Normal balanced data",
            new PerformanceData {
                TotalProduction = 100,
                ProductionOk = 95,
                ProductionNoK = 5,
                RunningTime = 1200,
                StoppedTime = 200,
                FaultedTime = 100,
                CurrentTime = 1600
            },
            new OeeRegister {
                StandardCycleTime = 10,
                ActualCycleTime = 12,
                PlanedProductionTime = 1600
            },
            0.95, 0.75, 0.8333333333333334
        };

        yield return new object[] {
            "Normal data low performance",
            new PerformanceData {
                TotalProduction = 100,
                ProductionOk = 95,
                ProductionNoK = 5,
                RunningTime = 2000,
                StoppedTime = 300,
                FaultedTime = 100,
                CurrentTime = 2400
            },
            new OeeRegister {
                StandardCycleTime = 10,
                ActualCycleTime = 20,
                PlanedProductionTime = 2400
            },
            0.95, 0.8333, 0.5 // Quality, Availability, Performance
        };

        yield return new object[] {
            "Normal data medium performance",
            new PerformanceData {
                TotalProduction = 100,
                ProductionOk = 95,
                ProductionNoK = 5,
                RunningTime = 1400,
                StoppedTime = 600,
                FaultedTime = 400,
                CurrentTime = 2400
            },
            new OeeRegister {
                StandardCycleTime = 10,
                ActualCycleTime = 14,
                PlanedProductionTime = 2400
            },
            0.95, 0.5833, 0.7143
        };

        yield return new object[] {
            "Normal data high performance",
            new PerformanceData {
                TotalProduction = 100,
                ProductionOk = 95,
                ProductionNoK = 5,
                RunningTime = 1100,
                StoppedTime = 900,
                FaultedTime = 400,
                CurrentTime = 2400
            },
            new OeeRegister {
                StandardCycleTime = 10,
                ActualCycleTime = 11,
                PlanedProductionTime = 2400
            },
            0.95, 0.4583, 0.9091
        };

        yield return new object[] {
            "Perfect run",
            new PerformanceData {
                TotalProduction = 100,
                ProductionOk = 100,
                ProductionNoK = 0,
                RunningTime = 1000,
                StoppedTime = 0,
                FaultedTime = 0,
                CurrentTime = 1000
            },
            new OeeRegister {
                StandardCycleTime = 10,
                ActualCycleTime = 10,
                PlanedProductionTime = 1000
            },
            1.0, 1.0, 1.0
        };

        yield return new object[] {
            "High efficiency with minor NOKs",
            new PerformanceData {
                TotalProduction = 200,
                ProductionOk = 190,
                ProductionNoK = 10,
                RunningTime = 1800,
                StoppedTime = 100,
                FaultedTime = 100,
                CurrentTime = 2000
            },
            new OeeRegister {
                StandardCycleTime = 8,
                ActualCycleTime = 9,
                PlanedProductionTime = 2000
            },
            0.95, 0.9, 0.8888888888888888
        };

        yield return new object[] {
            "Moderate NOK ratio",
            new PerformanceData {
                TotalProduction = 120,
                ProductionOk = 108,
                ProductionNoK = 12,
                RunningTime = 1500,
                StoppedTime = 50,
                FaultedTime = 50,
                CurrentTime = 1600
            },
            new OeeRegister {
                StandardCycleTime = 10,
                ActualCycleTime = 12,
                PlanedProductionTime = 1600
            },
            0.9, 0.9375, 0.8
        };

        yield return new object[] {
            "Balanced low efficiency",
            new PerformanceData {
                TotalProduction = 100,
                ProductionOk = 70,
                ProductionNoK = 30,
                RunningTime = 2000,
                StoppedTime = 200,
                FaultedTime = 200,
                CurrentTime = 2400
            },
            new OeeRegister {
                StandardCycleTime = 10,
                ActualCycleTime = 12,
                PlanedProductionTime = 2400
            },
            0.7, 0.8333, 0.5
        };

        yield return new object[] {
            "Equal OK and NOK",
            new PerformanceData {
                TotalProduction = 100,
                ProductionOk = 50,
                ProductionNoK = 50,
                RunningTime = 1000,
                StoppedTime = 500,
                FaultedTime = 500,
                CurrentTime = 2000
            },
            new OeeRegister {
                StandardCycleTime = 10,
                ActualCycleTime = 10,
                PlanedProductionTime = 2000
            },
            0.5, 0.5, 1.0
        };

        yield return new object[] {
            "High availability, low performance",
            new PerformanceData {
                TotalProduction = 20,
                ProductionOk = 19,
                ProductionNoK = 1,
                RunningTime = 1500,
                StoppedTime = 50,
                FaultedTime = 50,
                CurrentTime = 1600
            },
            new OeeRegister {
                StandardCycleTime = 10,
                ActualCycleTime = 30,
                PlanedProductionTime = 1600
            },
            0.95, 0.9375, 0.133333
        };

        yield return new object[] {
            "High performance, low availability",
            new PerformanceData {
                TotalProduction = 200,
                ProductionOk = 195,
                ProductionNoK = 5,
                RunningTime = 1000,
                StoppedTime = 900,
                FaultedTime = 500,
                CurrentTime = 2400
            },
            new OeeRegister {
                StandardCycleTime = 10,
                ActualCycleTime = 12,
                PlanedProductionTime = 2400
            },// Quality, Availability, Performance
            0.975, 0.4167, 1.5
        };

        yield return new object[] {
            "Scaled-down perfect run",
            new PerformanceData {
                TotalProduction = 10,
                ProductionOk = 10,
                ProductionNoK = 0,
                RunningTime = 10,
                StoppedTime = 0,
                FaultedTime = 0,
                CurrentTime = 10
            },
            new OeeRegister {
                StandardCycleTime = 1,
                ActualCycleTime = 1,
                PlanedProductionTime = 10
            },
            1.0, 1.0, 1.0
        };
    }
}