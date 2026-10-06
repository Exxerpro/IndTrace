// <copyright file="OeeRegisterDto.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Performance.Request.Command.Create;

/// <summary>
/// Data transfer object for OEE register information.
/// </summary>
public class OeeRegisterDto
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OeeRegisterDto"/> class.
    /// </summary>
    public OeeRegisterDto()
    {
    }

    /// <summary>
    /// Gets or sets the OEE register identifier.
    /// </summary>
    public int OeeRegisterId { get; set; }

    /// <summary>
    /// Gets or sets the machine identifier.
    /// </summary>
    public int MachineId { get; set; }

    /// <summary>
    /// Gets or sets the PLC identifier.
    /// </summary>
    public int PlcId { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the OEE register.
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
    /// Gets or sets the product identifier.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets the total production value.
    /// </summary>
    public double TotalProduction { get; set; }

    /// <summary>
    /// Gets or sets the standard cycle time.
    /// </summary>
    public double StandardCycleTime { get; set; }

    /// <summary>
    /// Gets or sets the actual cycle time.
    /// </summary>
    public double ActualCycleTime { get; set; }

    /// <summary>
    /// Gets or sets the planned production time.
    /// </summary>
    public double PlanedProductionTime { get; set; }

    /// <summary>
    /// Gets or sets the reject event counter.
    /// </summary>
    public int RejectEventCounter { get; set; }

    /// <summary>
    /// Gets or sets the status reject value.
    /// </summary>
    public int StatusReject { get; set; }

    /// <summary>
    /// Gets or sets the reject quantity in units.
    /// </summary>
    public double RejectQuantityUnits { get; set; }

    /// <summary>
    /// Gets or sets the number of OK productions.
    /// </summary>
    public double ProductionOk { get; set; }

    /// <summary>
    /// Gets or sets the number of NoK productions.
    /// </summary>
    public double ProductionNoK { get; set; }

    /// <summary>
    /// Gets or sets the OEE value.
    /// </summary>
    public double Oee { get; set; }

    /// <summary>
    /// Gets or sets the availability value.
    /// </summary>
    public double Availability { get; set; }

    /// <summary>
    /// Gets or sets the performance value.
    /// </summary>
    public double Performance { get; set; }

    /// <summary>
    /// Gets or sets the quality value.
    /// </summary>
    public double Quality { get; set; }

    /// <summary>
    /// Converts an <see cref="OeeRegister"/> entity to an <see cref="OeeRegisterDto"/>.
    /// </summary>
    /// <param name="entity">The OEE register entity to convert.</param>
    /// <returns>An <see cref="OeeRegisterDto"/> instance.</returns>
    /// <summary>
    /// Converts an <see cref="OeeRegister"/> entity to an <see cref="OeeRegisterDto"/>.
    /// </summary>
    /// <param name="entity">The OEE register entity to convert.</param>
    /// <returns>A Result containing the OeeRegisterDto instance or failure information.</returns>
    public static Result<OeeRegisterDto> ToDto(OeeRegister entity)
    {
        if (entity == null)
        {
            return Result<OeeRegisterDto>.WithFailure($"Parameter '{nameof(entity)}' cannot be null");
        }

        return Result<OeeRegisterDto>.Success(new OeeRegisterDto
        {
            OeeRegisterId = entity.OeeRegisterId,
            MachineId = entity.MachineId,
            PlcId = entity.PlcId,
            TimeStamp = entity.TimeStamp,
            ApplicationFlag = entity.ApplicationFlag,
            EventCounter = entity.EventCounter,
            CurrentTime = entity.CurrentTime,
            RunningTime = entity.RunningTime,
            StoppedTime = entity.StoppedTime,
            FaultedTime = entity.FaultedTime,
            StatusFaultReason = entity.StatusFaultReason,
            ProductId = entity.ProductId,
            TotalProduction = entity.TotalProduction,
            StandardCycleTime = entity.StandardCycleTime,
            ActualCycleTime = entity.ActualCycleTime,
            PlanedProductionTime = entity.PlanedProductionTime,
            RejectEventCounter = entity.RejectEventCounter,
            StatusReject = entity.StatusReject,
            RejectQuantityUnits = entity.RejectQuantityUnits,
            ProductionOk = entity.ProductionOk,
            ProductionNoK = entity.ProductionNoK,
            Oee = entity.Oee,
            Availability = entity.Availability,
            Performance = entity.Performance,
            Quality = entity.Quality,
        });
    }
}