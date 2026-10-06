// <copyright file="OeeRegisterRawData.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace IndTrace.TestData.RawData;

/// <summary>
/// Static test data for OeeRegister entities with O(1) lookup.
/// Generated with ImmutableDictionary for thread-safety and performance.
/// Implements lazy-loaded Dict for best of both worlds: O(1) lookups + List compatibility.
/// </summary>
internal static class OeeRegisterRawData
{
    /// <summary>
    /// OeeRegister test data - Manufacturing OEE calculation samples
    /// </summary>
    private static readonly ImmutableDictionary<int, OeeRegister> _oeeRegistersDict =
        new Dictionary<int, OeeRegister>
        {
            [1] = new OeeRegister
            {
                OeeRegisterId = 1,
                MachineId = 100,
                PlcId = 100,
                TimeStamp = DateTime.UtcNow.AddHours(-1),
                ApplicationFlag = 1,
                EventCounter = 125,
                CurrentTime = 3600, // 1 hour in seconds
                RunningTime = 3240, // 54 minutes running
                StoppedTime = 300,  // 5 minutes stopped
                FaultedTime = 60,   // 1 minute faulted
                StatusFaultReason = 0,
                ProductId = 1,
                TotalProduction = 100.0,
                StandardCycleTime = 30.0, // 30 seconds per cycle
                ActualCycleTime = 32.0,   // slightly slower
                PlanedProductionTime = 3600.0,
                RejectEventCounter = 5,
                StatusReject = 0,
                RejectQuantityUnits = 5.0,
                ProductionOk = 95.0,
                ProductionNoK = 5.0,
                // Calculated OEE values (will be computed by domain logic)
                Oee = 0.85,
                Availability = 0.90,
                Performance = 0.94,
                Quality = 0.95
            },
            [2] = new OeeRegister
            {
                OeeRegisterId = 2,
                MachineId = 200,
                PlcId = 200,
                TimeStamp = DateTime.UtcNow.AddHours(-2),
                ApplicationFlag = 1,
                EventCounter = 89,
                CurrentTime = 7200, // 2 hours
                RunningTime = 6480, // 1.8 hours running
                StoppedTime = 600,  // 10 minutes stopped
                FaultedTime = 120,  // 2 minutes faulted
                StatusFaultReason = 1,
                ProductId = 2,
                TotalProduction = 180.0,
                StandardCycleTime = 40.0,
                ActualCycleTime = 36.0, // faster than standard
                PlanedProductionTime = 7200.0,
                RejectEventCounter = 10,
                StatusReject = 0,
                RejectQuantityUnits = 10.0,
                ProductionOk = 170.0,
                ProductionNoK = 10.0,
                // Calculated values
                Oee = 0.82,
                Availability = 0.90,
                Performance = 1.11, // over 100% performance
                Quality = 0.94
            },
            [3] = new OeeRegister
            {
                OeeRegisterId = 3,
                MachineId = 300,
                PlcId = 300,
                TimeStamp = DateTime.UtcNow.AddMinutes(-30),
                ApplicationFlag = 1,
                EventCounter = 45,
                CurrentTime = 1800, // 30 minutes
                RunningTime = 1620, // 27 minutes running
                StoppedTime = 180,  // 3 minutes stopped
                FaultedTime = 0,    // no faults
                StatusFaultReason = 0,
                ProductId = 3,
                TotalProduction = 50.0,
                StandardCycleTime = 32.0,
                ActualCycleTime = 32.4,
                PlanedProductionTime = 1800.0,
                RejectEventCounter = 2,
                StatusReject = 0,
                RejectQuantityUnits = 2.0,
                ProductionOk = 48.0,
                ProductionNoK = 2.0,
                // Calculated values
                Oee = 0.88,
                Availability = 0.90,
                Performance = 0.99,
                Quality = 0.96
            },
            [4] = new OeeRegister
            {
                OeeRegisterId = 4,
                MachineId = 400,
                PlcId = 400,
                TimeStamp = DateTime.UtcNow.AddMinutes(-15),
                ApplicationFlag = 1,
                EventCounter = 22,
                CurrentTime = 900,  // 15 minutes
                RunningTime = 810,  // 13.5 minutes running
                StoppedTime = 90,   // 1.5 minutes stopped
                FaultedTime = 0,
                StatusFaultReason = 0,
                ProductId = 4,
                TotalProduction = 25.0,
                StandardCycleTime = 35.0,
                ActualCycleTime = 32.4, // better than standard
                PlanedProductionTime = 900.0,
                RejectEventCounter = 1,
                StatusReject = 0,
                RejectQuantityUnits = 1.0,
                ProductionOk = 24.0,
                ProductionNoK = 1.0,
                // Calculated values
                Oee = 0.92,
                Availability = 0.90,
                Performance = 1.08,
                Quality = 0.96
            },
            [5] = new OeeRegister
            {
                OeeRegisterId = 5,
                MachineId = 500,
                PlcId = 500,
                TimeStamp = DateTime.UtcNow.AddMinutes(-5),
                ApplicationFlag = 1,
                EventCounter = 8,
                CurrentTime = 300,  // 5 minutes
                RunningTime = 270,  // 4.5 minutes running
                StoppedTime = 30,   // 0.5 minutes stopped
                FaultedTime = 0,
                StatusFaultReason = 0,
                ProductId = 5,
                TotalProduction = 8.0,
                StandardCycleTime = 38.0,
                ActualCycleTime = 33.75, // 270/8
                PlanedProductionTime = 300.0,
                RejectEventCounter = 0,
                StatusReject = 0,
                RejectQuantityUnits = 0.0,
                ProductionOk = 8.0,
                ProductionNoK = 0.0,
                // Perfect quality
                Oee = 0.95,
                Availability = 0.90,
                Performance = 1.13,
                Quality = 1.0
            }
        }.ToImmutableDictionary();

    /// <summary>
    /// Lazy-loaded cached list for maximum performance - best of both worlds
    /// </summary>
    private static readonly Lazy<IReadOnlyList<OeeRegister>> _fixtureCache =
        new(() => _oeeRegistersDict.Values.ToList());

    /// <summary>
    /// Get all OeeRegister entities (cached List from dictionary for backward compatibility)
    /// </summary>
    public static IReadOnlyList<OeeRegister> Fixture => _fixtureCache.Value;

    /// <summary>
    /// Get a specific OeeRegister by ID - O(1) lookup (standardized pattern)
    /// </summary>
    public static OeeRegister? GetById(int id) =>
        _oeeRegistersDict.TryGetValue(id, out var oeeRegister) ? oeeRegister : null;

    /// <summary>
    /// Direct dictionary access for advanced scenarios (standardized pattern)
    /// </summary>
    public static IImmutableDictionary<int, OeeRegister> Dictionary => _oeeRegistersDict;

    /// <summary>
    /// Check if an OeeRegister exists by ID - O(1) lookup
    /// </summary>
    public static bool Contains(int id) => _oeeRegistersDict.ContainsKey(id);

    /// <summary>
    /// Get count of OeeRegisters - O(1) operation
    /// </summary>
    public static int Count => _oeeRegistersDict.Count;

    /// <summary>
    /// Get OeeRegisters by Machine ID - business query
    /// </summary>
    public static IEnumerable<OeeRegister> GetByMachineId(int machineId) =>
        _oeeRegistersDict.Values.Where(o => o.MachineId == machineId);

    /// <summary>
    /// Get OeeRegisters by PLC ID - business query
    /// </summary>
    public static IEnumerable<OeeRegister> GetByPlcId(int plcId) =>
        _oeeRegistersDict.Values.Where(o => o.PlcId == plcId);

    /// <summary>
    /// Get recent OeeRegisters within time range
    /// </summary>
    public static IEnumerable<OeeRegister> GetRecent(TimeSpan timeRange) =>
        _oeeRegistersDict.Values.Where(o => DateTime.UtcNow - o.TimeStamp <= timeRange);
}
