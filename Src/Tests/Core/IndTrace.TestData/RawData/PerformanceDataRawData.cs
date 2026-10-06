// <copyright file="PerformanceDataRawData.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using IndTrace.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace IndTrace.TestData.RawData;

/// <summary>
/// Static test data for PerformanceData entities with O(1) lookup.
/// Generated with ImmutableDictionary for thread-safety and performance.
/// Implements lazy-loaded Dict for best of both worlds: O(1) lookups + List compatibility.
/// </summary>
internal static class PerformanceDataRawData
{
    /// <summary>
    /// PerformanceData test data - Manufacturing performance metrics from PLC
    /// </summary>
    private static readonly ImmutableDictionary<long, PerformanceData> _performanceDataDict =
        new Dictionary<long, PerformanceData>
        {
            [1] = new PerformanceData
            {
                PerformanceDataId = 1,
                MachineId = 100,
                PlcId = 100,
                BarCodeId = 1,
                CycleId = new CycleId(1),
                TimeStamp = DateTime.UtcNow.AddHours(-1),
                ApplicationFlag = 1,
                EventCounter = 125,
                CurrentTime = 3600, // 1 hour in seconds
                RunningTime = 3240, // 54 minutes running
                StoppedTime = 300,  // 5 minutes stopped
                FaultedTime = 60,   // 1 minute faulted
                StatusFaultReason = 0,
                TotalProduction = 100.0,
                ProductionOk = 95.0,
                ProductionNoK = 5.0,
                StatusFaultReject = 0,
                RejectEventCounter = 5,
                StatusReject = 0,
                RejectQuantityUnits = 5.0,
                StandardCycleTime = 30.0,
                ActualCycleTime = 32.0,
                PlanedProductionTime = 3600.0
            },
            [2] = new PerformanceData
            {
                PerformanceDataId = 2,
                MachineId = 200,
                PlcId = 200,
                BarCodeId = 2,
                CycleId = new CycleId(2),
                TimeStamp = DateTime.UtcNow.AddHours(-2),
                ApplicationFlag = 1,
                EventCounter = 89,
                CurrentTime = 7200, // 2 hours
                RunningTime = 6480, // 1.8 hours running
                StoppedTime = 600,  // 10 minutes stopped
                FaultedTime = 120,  // 2 minutes faulted
                StatusFaultReason = 1,
                TotalProduction = 180.0,
                ProductionOk = 170.0,
                ProductionNoK = 10.0,
                StatusFaultReject = 0,
                RejectEventCounter = 10,
                StatusReject = 0,
                RejectQuantityUnits = 10.0,
                StandardCycleTime = 40.0,
                ActualCycleTime = 36.0,
                PlanedProductionTime = 7200.0
            },
            [3] = new PerformanceData
            {
                PerformanceDataId = 3,
                MachineId = 300,
                PlcId = 300,
                BarCodeId = 3,
                CycleId = new CycleId(3),
                TimeStamp = DateTime.UtcNow.AddMinutes(-30),
                ApplicationFlag = 1,
                EventCounter = 45,
                CurrentTime = 1800, // 30 minutes
                RunningTime = 1620, // 27 minutes running
                StoppedTime = 180,  // 3 minutes stopped
                FaultedTime = 0,    // no faults
                StatusFaultReason = 0,
                TotalProduction = 50.0,
                ProductionOk = 48.0,
                ProductionNoK = 2.0,
                StatusFaultReject = 0,
                RejectEventCounter = 2,
                StatusReject = 0,
                RejectQuantityUnits = 2.0,
                StandardCycleTime = 32.0,
                ActualCycleTime = 32.4,
                PlanedProductionTime = 1800.0
            },
            [4] = new PerformanceData
            {
                PerformanceDataId = 4,
                MachineId = 400,
                PlcId = 400,
                BarCodeId = 4,
                CycleId = new CycleId(4),
                TimeStamp = DateTime.UtcNow.AddMinutes(-15),
                ApplicationFlag = 1,
                EventCounter = 22,
                CurrentTime = 900,  // 15 minutes
                RunningTime = 810,  // 13.5 minutes running
                StoppedTime = 90,   // 1.5 minutes stopped
                FaultedTime = 0,
                StatusFaultReason = 0,
                TotalProduction = 25.0,
                ProductionOk = 24.0,
                ProductionNoK = 1.0,
                StatusFaultReject = 0,
                RejectEventCounter = 1,
                StatusReject = 0,
                RejectQuantityUnits = 1.0,
                StandardCycleTime = 35.0,
                ActualCycleTime = 32.4,
                PlanedProductionTime = 900.0
            },
            [5] = new PerformanceData
            {
                PerformanceDataId = 5,
                MachineId = 500,
                PlcId = 500,
                BarCodeId = 5,
                CycleId = new CycleId(5),
                TimeStamp = DateTime.UtcNow.AddMinutes(-5),
                ApplicationFlag = 1,
                EventCounter = 8,
                CurrentTime = 300,  // 5 minutes
                RunningTime = 270,  // 4.5 minutes running
                StoppedTime = 30,   // 0.5 minutes stopped
                FaultedTime = 0,
                StatusFaultReason = 0,
                TotalProduction = 8.0,
                ProductionOk = 8.0,
                ProductionNoK = 0.0,
                StatusFaultReject = 0,
                RejectEventCounter = 0,
                StatusReject = 0,
                RejectQuantityUnits = 0.0,
                StandardCycleTime = 38.0,
                ActualCycleTime = 33.75,
                PlanedProductionTime = 300.0
            }
        }.ToImmutableDictionary();

    /// <summary>
    /// Lazy-loaded cached list for maximum performance - best of both worlds
    /// </summary>
    private static readonly Lazy<IReadOnlyList<PerformanceData>> _fixtureCache =
        new(() => _performanceDataDict.Values.ToList());

    /// <summary>
    /// Get all PerformanceData entities (cached List from dictionary for backward compatibility)
    /// </summary>
    public static IReadOnlyList<PerformanceData> Fixture => _fixtureCache.Value;

    /// <summary>
    /// Get a specific PerformanceData by ID - O(1) lookup (standardized pattern)
    /// </summary>
    public static PerformanceData? GetById(long id) =>
        _performanceDataDict.TryGetValue(id, out var performanceData) ? performanceData : null;

    /// <summary>
    /// Direct dictionary access for advanced scenarios (standardized pattern)
    /// </summary>
    public static IImmutableDictionary<long, PerformanceData> Dictionary => _performanceDataDict;

    /// <summary>
    /// Check if PerformanceData exists by ID - O(1) lookup
    /// </summary>
    public static bool Contains(long id) => _performanceDataDict.ContainsKey(id);

    /// <summary>
    /// Get count of PerformanceData entries - O(1) operation
    /// </summary>
    public static int Count => _performanceDataDict.Count;

    /// <summary>
    /// Get PerformanceData by Machine ID - business query
    /// </summary>
    public static IEnumerable<PerformanceData> GetByMachineId(int machineId) =>
        _performanceDataDict.Values.Where(p => p.MachineId == machineId);

    /// <summary>
    /// Get PerformanceData by PLC ID - business query
    /// </summary>
    public static IEnumerable<PerformanceData> GetByPlcId(int plcId) =>
        _performanceDataDict.Values.Where(p => p.PlcId == plcId);

    /// <summary>
    /// Get PerformanceData by Cycle ID - business query
    /// </summary>
    public static PerformanceData? GetByCycleId(int cycleId) =>
        _performanceDataDict.Values.FirstOrDefault(p => p.CycleId.Value == cycleId);

    /// <summary>
    /// Get PerformanceData by BarCode ID - business query
    /// </summary>
    public static PerformanceData? GetByBarCodeId(int barCodeId) =>
        _performanceDataDict.Values.FirstOrDefault(p => p.BarCodeId == barCodeId);

    /// <summary>
    /// Get recent PerformanceData within time range
    /// </summary>
    public static IEnumerable<PerformanceData> GetRecent(TimeSpan timeRange) =>
        _performanceDataDict.Values.Where(p => DateTime.UtcNow - p.TimeStamp <= timeRange);

    /// <summary>
    /// Get high production PerformanceData (above threshold)
    /// </summary>
    public static IEnumerable<PerformanceData> GetHighProduction(double productionThreshold = 50.0) =>
        _performanceDataDict.Values.Where(p => p.TotalProduction >= productionThreshold);
}
