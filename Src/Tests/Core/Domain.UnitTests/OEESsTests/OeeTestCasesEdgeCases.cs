// <copyright file="OeeTestCasesEdgeCases.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.OEESsTests;

public static class OeeTestCasesEdgeCases
{
    public static IEnumerable<object[]> GetEdgeCasesWithErrors()
    {
        yield return new object[]
        {
            "Zero production, all OK",
            new PerformanceData
            {
                TotalProduction = 0,
                ProductionOk = 0,
                ProductionNoK = 0,
                RunningTime = 1000,
                StoppedTime = 200,
                FaultedTime = 100,
                CurrentTime = 1300
            },
            new OeeRegister
            {
                StandardCycleTime = 10,
                ActualCycleTime = 12,
                PlanedProductionTime = 0
            },
            1.0, 0.7692307692307693, 0.0
        };

        yield return new object[]
        {
            "Negative values failing",
            new PerformanceData
            {
                TotalProduction = -10,
                ProductionOk = -5,
                ProductionNoK = -5,
                RunningTime = -100,
                StoppedTime = 0,
                FaultedTime = 0,
                CurrentTime = 0
            },
            new OeeRegister
            {
                StandardCycleTime = -5,
                ActualCycleTime = -1,
                PlanedProductionTime = -100
            }, // Quality, Availability, Performance
            1.0, 0.0, 1.0
        };

        yield return new object[]
        {
            "No runtime but valid current time",
            new PerformanceData
            {
                TotalProduction = 50,
                ProductionOk = 50,
                ProductionNoK = 0,
                RunningTime = 0,
                StoppedTime = 200,
                FaultedTime = 100,
                CurrentTime = 1000
            },
            new OeeRegister
            {
                StandardCycleTime = 10,
                ActualCycleTime = 10,
                PlanedProductionTime = 0
            },// Quality, Availability, Performance
            1.0, 1.0, 0.625
        };

        yield return new object[]
        {
            "Zero standard and actual cycle times",
            new PerformanceData
            {
                TotalProduction = 100,
                ProductionOk = 90,
                ProductionNoK = 10,
                RunningTime = 500,
                StoppedTime = 0,
                FaultedTime = 0,
                CurrentTime = 500
            },
            new OeeRegister
            {
                StandardCycleTime = 0,
                ActualCycleTime = 0,
                PlanedProductionTime = 500
            },
            0.9, 1.0, 0.2
        };
        yield return new object[]
        {
            "Positive production with zero running time",
            new PerformanceData
            {
                TotalProduction = 100,
                ProductionOk = 100,
                ProductionNoK = 0,
                RunningTime = 0,
                StoppedTime = 1000,
                FaultedTime = 1000,
                CurrentTime = 2000
            },
            new OeeRegister
            {
                StandardCycleTime = 10,
                ActualCycleTime = 12,
                PlanedProductionTime = 2000
            }, // Quality, Availability, Performance
            1.0, 0.5, 1.0 // No running time = 0 performance
        };
        yield return new object[]
        {
            "Valid production with zero planned time",
            new PerformanceData
            {
                TotalProduction = 50,
                ProductionOk = 45,
                ProductionNoK = 5,
                RunningTime = 500,
                StoppedTime = 0,
                FaultedTime = 0,
                CurrentTime = 500
            },
            new OeeRegister
            {
                StandardCycleTime = 10,
                ActualCycleTime = 11,
                PlanedProductionTime = 0
            }, // Quality, Availability, Performance
            0.9, 1.0, 1.0 // Edge case: planned time zero but non-zero data
        };

        yield return new object[]
        {
            "Faulted time greater than current time",
            new PerformanceData
            {
                TotalProduction = 10,
                ProductionOk = 10,
                ProductionNoK = 0,
                RunningTime = 100,
                StoppedTime = 50,
                FaultedTime = 1000,
                CurrentTime = 500
            },
            new OeeRegister
            {
                StandardCycleTime = 10,
                ActualCycleTime = 10,
                PlanedProductionTime = 500
            },
            1.0, 0.2, 1.0 // Availability very low due to inflated fault time
        };

        yield return new object[]
        {
            "Production with no time data",
            new PerformanceData
            {
                TotalProduction = 50,
                ProductionOk = 50,
                ProductionNoK = 0,
                RunningTime = 0,
                StoppedTime = 0,
                FaultedTime = 0,
                CurrentTime = 0
            },
            new OeeRegister
            {
                StandardCycleTime = 10,
                ActualCycleTime = 12,
                PlanedProductionTime = 0
            }, // Quality, Availability, Performance
            1.0, 0.0, 1.0
        };

        yield return new object[]
        {
            "Planned time > Current time",
            new PerformanceData
            {
                TotalProduction = 20,
                ProductionOk = 20,
                ProductionNoK = 0,
                RunningTime = 200,
                StoppedTime = 100,
                FaultedTime = 100,
                CurrentTime = 300
            },
            new OeeRegister
            {
                StandardCycleTime = 10,
                ActualCycleTime = 12,
                PlanedProductionTime = 1000
            },
            1.0, 0.6666666666666666d, 1.0
        };

        yield return new object[]
        {
            "Zero everything",
            new PerformanceData
            {
                TotalProduction = 0,
                ProductionOk = 0,
                ProductionNoK = 0,
                RunningTime = 0,
                StoppedTime = 0,
                FaultedTime = 0,
                CurrentTime = 0
            },
            new OeeRegister
            {
                StandardCycleTime = 0,
                ActualCycleTime = 0,
                PlanedProductionTime = 0
            },
            1.0, 0.0, 1.0
        };

        yield return new object[]
        {
            "Running time becomes negative after fallback",
            new PerformanceData
            {
                TotalProduction = 10,
                ProductionOk = 10,
                ProductionNoK = 0,
                RunningTime = 0,
                StoppedTime = 2000,
                FaultedTime = 0,
                CurrentTime = 1000
            },
            new OeeRegister
            {
                StandardCycleTime = 5,
                ActualCycleTime = 6,
                PlanedProductionTime = 1000
            },
            1.0, 0.0, 1.0
        };

        yield return new object[]
        {
            "ProductionOk exceeds TotalProduction",
            new PerformanceData
            {
                TotalProduction = 100,
                ProductionOk = 150,
                ProductionNoK = 10,
                RunningTime = 1000,
                StoppedTime = 100,
                FaultedTime = 100,
                CurrentTime = 1200
            },
            new OeeRegister
            {
                StandardCycleTime = 10,
                ActualCycleTime = 10,
                PlanedProductionTime = 1200
            },
            1.0, 0.8333, 1.0
        };

        yield return new object[]
        {
            "ProductionNoK > 0 with TotalProduction = 0",
            new PerformanceData
            {
                TotalProduction = 0,
                ProductionOk = 0,
                ProductionNoK = 5,
                RunningTime = 200,
                StoppedTime = 0,
                FaultedTime = 0,
                CurrentTime = 200
            },
            new OeeRegister
            {
                StandardCycleTime = 10,
                ActualCycleTime = 12,
                PlanedProductionTime = 200
            },
            1.0, 1.0, 0.0
        };

        yield return new object[]
        {
            "Negative TotalProduction, valid OK and NOK",
            new PerformanceData
            {
                TotalProduction = -1,
                ProductionOk = 40,
                ProductionNoK = 60,
                RunningTime = 1000,
                StoppedTime = 0,
                FaultedTime = 0,
                CurrentTime = 1000
            },
            new OeeRegister
            {
                StandardCycleTime = 10,
                ActualCycleTime = 12,
                PlanedProductionTime = 1000
            },
            1.0, 1.0, 0.0
        };

        yield return new object[]
        {
            "Negative cycle times",
            new PerformanceData
            {
                TotalProduction = 50,
                ProductionOk = 50,
                ProductionNoK = 0,
                RunningTime = 500,
                StoppedTime = 0,
                FaultedTime = 0,
                CurrentTime = 500
            },
            new OeeRegister
            {
                StandardCycleTime = -5,
                ActualCycleTime = -10,
                PlanedProductionTime = 500
            },
            1.0, 1.0, 0.1
        };

        yield return new object[]
        {
            "Large planned time to test for overflow",
            new PerformanceData
            {
                TotalProduction = 1000,
                ProductionOk = 990,
                ProductionNoK = 10,
                RunningTime = 2000,
                StoppedTime = 0,
                FaultedTime = 0,
                CurrentTime = 2000
            },
            new OeeRegister
            {
                StandardCycleTime = 10,
                ActualCycleTime = 10,
                PlanedProductionTime = 1073741824 // 2^30
            }, // Quality, Availability, Performance
            0.99, 1.0, 1.5 // Performance clamped
        };
        yield return new object[]
        {
            "All times exceed current time",
            new PerformanceData
            {
                TotalProduction = 100,
                ProductionOk = 100,
                ProductionNoK = 0,
                RunningTime = 1000,
                StoppedTime = 500,
                FaultedTime = 300,
                CurrentTime = 100
            },
            new OeeRegister
            {
                StandardCycleTime = 10,
                ActualCycleTime = 10,
                PlanedProductionTime = 100
            },// Quality, Availability, Performance
            1.0, 1.0, 1.0 // Should trigger checks or corrections
        };
    }

    public static IEnumerable<object[]> GetEdgeCases()
    {
        yield return new object[]
        {
            "All production NOK",
            new PerformanceData
            {
                TotalProduction = 80,
                ProductionOk = 0,
                ProductionNoK = 80,
                RunningTime = 600,
                StoppedTime = 200,
                FaultedTime = 200,
                CurrentTime = 1000
            },
            new OeeRegister
            {
                StandardCycleTime = 7,
                ActualCycleTime = 10,
                PlanedProductionTime = 1000
            }, // Quality, Availability, Performance
            0.0, 0.6, 0.9333333333333333d // Zero quality
        };

        yield return new object[]
        {
            "Extremely high standard cycle time",
            new PerformanceData
            {
                TotalProduction = 5,
                ProductionOk = 5,
                ProductionNoK = 0,
                RunningTime = 100,
                StoppedTime = 0,
                FaultedTime = 0,
                CurrentTime = 100
            },
            new OeeRegister
            {
                StandardCycleTime = 9999,
                ActualCycleTime = 0,
                PlanedProductionTime = 100
            },// Quality, Availability, Performance
            1.0, 1.0, 1.5
        };
        yield return new object[]
        {
            "High precision values",
            new PerformanceData
            {
                TotalProduction = 1,
                ProductionOk = 1,
                ProductionNoK = 0,
                RunningTime = 1,
                StoppedTime = 0,
                FaultedTime = 0,
                CurrentTime = 1
            },
            new OeeRegister
            {
                StandardCycleTime = 0.000001,
                ActualCycleTime = 0.000001,
                PlanedProductionTime = 1
            },
            1.0, 1.0, 0.000001 // Stress test for floating-point math
        };
        yield return new object[]
        {
            "Large input values",
            new PerformanceData
            {
                TotalProduction = int.MaxValue,
                ProductionOk = int.MaxValue - 10,
                ProductionNoK = 10,
                RunningTime = int.MaxValue,
                StoppedTime = 0,
                FaultedTime = 0,
                CurrentTime = int.MaxValue
            },
            new OeeRegister
            {
                StandardCycleTime = 1,
                ActualCycleTime = 1,
                PlanedProductionTime = int.MaxValue
            },
            (double)(int.MaxValue - 10) / int.MaxValue, 1.0, 1.0
        };
        yield return new object[]
        {
            "Negative actual cycle time",
            new PerformanceData
            {
                TotalProduction = 30,
                ProductionOk = 30,
                ProductionNoK = 0,
                RunningTime = 300,
                StoppedTime = 100,
                FaultedTime = 100,
                CurrentTime = 500
            },
            new OeeRegister
            {
                StandardCycleTime = 10,
                ActualCycleTime = -5,
                PlanedProductionTime = 500
            },
            1.0, 0.6, 1.0 // Quality, Availability, Performance
        };
        yield return new object[]
        {
            "Performance exceeds upper bound",
            new PerformanceData
            {
                TotalProduction = 500,
                ProductionOk = 500,
                ProductionNoK = 0,
                RunningTime = 100,
                StoppedTime = 0,
                FaultedTime = 0,
                CurrentTime = 100
            },
            new OeeRegister
            {
                StandardCycleTime = 1,
                ActualCycleTime = 0,
                PlanedProductionTime = 100
            },// Quality, Availability, Performance
            1.0, 1.0, 1.5 // Clamped
        };
    }
}