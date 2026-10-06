// <copyright file="PerformanceDataCreated.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Performance.Request.Command.Create;

/// <summary>
/// Represents the data for a created performance record notification.
/// </summary>
public class PerformanceDataCreated : INotification
{
    /// <summary>
    /// Gets or sets the unique identifier for the performance data.
    /// </summary>
    public long PerformanceDataId { get; set; }

    /// <summary>
    /// Gets or sets the machine identifier.
    /// </summary>
    public int MachineId { get; set; }

    /// <summary>
    /// Gets or sets the PLC identifier.
    /// </summary>
    public int PlcId { get; set; }

    /// <summary>
    /// Gets or sets the barcode identifier.
    /// </summary>
    public int BarCodeId { get; set; }

    /// <summary>
    /// Gets or sets the cycle identifier.
    /// </summary>
    public int CycleId { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the performance data.
    /// </summary>
    public DateTime TimeStamp { get; set; }

    /// <summary>
    /// Gets or sets the application flag.
    /// </summary>
    public int ApplicationFlag { get; set; }

    /// <summary>
    /// Gets or sets the event counter.
    /// </summary>
    public int EventCounter { get; set; }

    /// <summary>
    /// Gets or sets the current time value.
    /// </summary>
    public int CurrentTime { get; set; }

    /// <summary>
    /// Gets or sets the running time value.
    /// </summary>
    public int RunningTime { get; set; }

    /// <summary>
    /// Gets or sets the stopped time value.
    /// </summary>
    public int StoppedTime { get; set; }

    /// <summary>
    /// Gets or sets the faulted time value.
    /// </summary>
    public int FaultedTime { get; set; }

    /// <summary>
    /// Gets or sets the status fault reason.
    /// </summary>
    public int StatusFaultReason { get; set; }

    /// <summary>
    /// Gets or sets the total production value.
    /// </summary>
    public double TotalProduction { get; set; }

    /// <summary>
    /// Gets or sets the number of OK productions.
    /// </summary>
    public double ProductionOk { get; set; }

    /// <summary>
    /// Gets or sets the number of NoK productions.
    /// </summary>
    public double ProductionNoK { get; set; }

    /// <summary>
    /// Gets or sets the status fault reject value.
    /// </summary>
    public int StatusFaultReject { get; set; }

    /// <summary>
    /// Returns a string representation of the performance data created notification.
    /// </summary>
    /// <returns>A formatted string containing the performance data created details.</returns>
    public override string ToString()
    {
        // [Fix]
        // CLAUDE
        // Date: 23/08/2025
        // Reason: Added meaningful ToString() method for better debugging and logging in MessageDto factory
        return $"PerformanceData Created - PerformanceDataId: {this.PerformanceDataId}, MachineId: {this.MachineId}, PlcId: {this.PlcId}";
    }

    /// <summary>
    /// Converts a <see cref="PerformanceDataCommand"/> to a <see cref="PerformanceDataCreated"/> DTO.
    /// </summary>
    /// <param name="performance">The performance data command to convert.</param>
    /// <returns>A <see cref="PerformanceDataCreated"/> DTO.</returns>
    public static PerformanceDataCreated ToDto(PerformanceDataCommand performance)
    {
        // Convert PerformanceData to PerformanceDataCreated DTO
        return new PerformanceDataCreated
        {
            // Map properties from PerformanceData to PerformanceDataCreated
            PerformanceDataId = performance.PerformanceDataId,
            MachineId = performance.MachineId,
            PlcId = performance.PlcId,
            BarCodeId = performance.BarCodeId,
            CycleId = performance.CycleId,
            TimeStamp = performance.TimeStamp,
            ApplicationFlag = performance.ApplicationFlag,
            EventCounter = performance.EventCounter,
            CurrentTime = performance.CurrentTime,

            RunningTime = performance.RunningTime,
            StoppedTime = performance.StoppedTime,
            FaultedTime = performance.FaultedTime,
            StatusFaultReason = performance.StatusFaultReason,
        };
    }

    /// <summary>
    /// Converts a <see cref="PerformanceDataCommand"/> to a <see cref="Domain.Entities.PerformanceData"/> entity.
    /// </summary>
    /// <param name="performance">The performance data command to convert.</param>
    /// <returns>A <see cref="Result{Domain.Entities.PerformanceData}"/> entity.</returns>
    public static IndQuestResults.Result<Domain.Entities.PerformanceData> ToEntity(PerformanceDataCommand performance)
    {
        // [Fix]
        // CLAUDE
        // Date: 22/08/2025
        // Reason: [ToEntity Pattern] - Updated to return Result<T> for Railway-Oriented Programming pattern
        if (performance == null)
        {
            return IndQuestResults.Result<Domain.Entities.PerformanceData>.WithFailure("Performance data command cannot be null");
        }

        try
        {
            // Convert PerformanceData to PerformanceDataCreated DTO
            var entity = new Domain.Entities.PerformanceData
            {
                // Map properties from PerformanceData to PerformanceDataCreated
                PerformanceDataId = performance.PerformanceDataId,
                MachineId = performance.MachineId,
                PlcId = performance.PlcId,
                BarCodeId = performance.BarCodeId,
                CycleId = new CycleId(performance.CycleId),
                TimeStamp = performance.TimeStamp,
                ApplicationFlag = performance.ApplicationFlag,
                EventCounter = performance.EventCounter,
                CurrentTime = performance.CurrentTime,

                RunningTime = performance.RunningTime,
                StoppedTime = performance.StoppedTime,
                FaultedTime = performance.FaultedTime,
                StatusFaultReason = performance.StatusFaultReason,

                // [Fix]
                // CLAUDE
                // Date: 22/08/2025
                // Reason: [MISSING PROPERTIES FIX] - Added missing production properties expected by test
                TotalProduction = performance.TotalProduction,
                ProductionOk = performance.ProductionOk,
                ProductionNoK = performance.ProductionNoK,
                StatusFaultReject = performance.StatusFaultReject,
            };

            return IndQuestResults.Result<Domain.Entities.PerformanceData>.Success(entity);
        }
        catch (Exception ex)
        {
            return IndQuestResults.Result<Domain.Entities.PerformanceData>.WithFailure($"Failed to convert performance data: {ex.Message}");
        }
    }
}