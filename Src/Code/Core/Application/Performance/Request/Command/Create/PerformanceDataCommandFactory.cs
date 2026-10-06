// <copyright file="PerformanceDataCommandFactory.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Globalization;

namespace IndTrace.Application.Performance.Request.Command.Create;

/// <summary>
/// Represents a command for performance data operations, including creation and conversion utilities.
/// </summary>
public class PerformanceDataCommand : Domain.Entities.PerformanceData, IGatewayRequest<TaskGatewayResponseDto>, ICommandData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceDataCommand"/> class with a task gateway request.
    /// </summary>
    /// <param name="taskGatewayRequest">The task gateway request to associate with the command.</param>
    public PerformanceDataCommand(TaskGatewayRequest taskGatewayRequest)
    {
        this.Command = taskGatewayRequest;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceDataCommand"/> class.
    /// </summary>
    public PerformanceDataCommand()
    {
    }

    /// <summary>
    /// Creates a new <see cref="PerformanceDataCommand"/> instance from a task gateway request.
    /// </summary>
    /// <param name="taskGatewayRequest">The task gateway request to use for creation.</param>
    /// <returns>A new <see cref="PerformanceDataCommand"/> instance.</returns>
    public ICommandData Create(TaskGatewayRequest taskGatewayRequest)
    {
        return new PerformanceDataCommand(taskGatewayRequest);
    }

    /// <summary>
    /// Gets or sets the cycle identifier as its raw <see cref="int"/> reading. PLC/DTO consumers of the
    /// command speak raw ints while the base entity stores the strongly-typed
    /// <see cref="Domain.ValueObjects.CycleId"/> (#35 Cluster 3). This accessor holds NO separate state —
    /// it reads and writes the BASE property, so base-typed consumers (EF, serializers, polymorphic code)
    /// always observe the same value (#126 F7a: the former <c>public new</c> auto-property shadowed the
    /// base and left it at its default). The remaining 16 formerly-shadowed properties were deleted
    /// outright — their base declarations are type-identical.
    /// </summary>
    public new int CycleId
    {
        get => base.CycleId.Value;
        set => base.CycleId = new CycleId(value);
    }

    /// <summary>
    /// Resets all performance data properties to their default values.
    /// </summary>
    /// <returns>True if the reset was successful; otherwise, false.</returns>
    public bool TryReset()
    {
        this.PerformanceDataId = 0;
        this.MachineId = 0;
        this.PlcId = 0;
        this.BarCodeId = 0;
        this.CycleId = 0;
        this.TimeStamp = DateTime.MinValue;
        this.ApplicationFlag = 0;
        this.EventCounter = 0;
        this.CurrentTime = 0;
        this.RunningTime = 0;
        this.StoppedTime = 0;
        this.FaultedTime = 0;
        this.StatusFaultReason = 0;
        this.TotalProduction = 0.0;
        this.ProductionOk = 0.0;
        this.ProductionNoK = 0.0;
        this.StatusFaultReject = 0;
        return true;
    }

    /// <summary>
    /// Converts a <see cref="PerformanceDataCommand"/> to a <see cref="Domain.Entities.PerformanceData"/> entity.
    /// </summary>
    /// <param name="command">The performance data command to convert.</param>
    /// <returns>A <see cref="Domain.Entities.PerformanceData"/> entity.</returns>
    public static Domain.Entities.PerformanceData ToEntity(PerformanceDataCommand command)
    {
        return new Domain.Entities.PerformanceData
        {
            PerformanceDataId = command.PerformanceDataId,
            MachineId = command.MachineId,
            PlcId = command.PlcId,
            BarCodeId = command.BarCodeId,
            CycleId = new CycleId(command.CycleId),
            TimeStamp = command.TimeStamp,
            ApplicationFlag = command.ApplicationFlag,
            EventCounter = command.EventCounter,
            CurrentTime = command.CurrentTime,
            RunningTime = command.RunningTime,
            StoppedTime = command.StoppedTime,
            FaultedTime = command.FaultedTime,
            StatusFaultReason = command.StatusFaultReason,
            TotalProduction = command.TotalProduction,
            ProductionOk = command.ProductionOk,
            ProductionNoK = command.ProductionNoK,
            StatusFaultReject = command.StatusFaultReject,
        };
    }

    /// <summary>
    /// Updates the command's data from a result object.
    /// </summary>
    /// <param name="result">The result containing updated data.</param>
    public void UpdateDataFromResult(Result<TaskGatewayResponseDto> result)
    {
        if (result is null || !result.IsSuccess || result.Value is null)
        {
            return;
        }

        var v = result.Value;
        this.Command = new TaskGatewayRequest(v.MachineId, v.PartNumber, v.CycleStatus, v.PartStatus);
        this.BarCodeId = v.BarCodeId;
        this.CycleId = v.CycleId;
        this.TimeStamp = v.TimeStamp;
    }

    /// <summary>
    /// Creates a <see cref="PerformanceDataCommand"/> from a dictionary of performance registers. Values
    /// parse culture-invariantly, mirroring <see cref="Domain.Entities.PerformanceData.FromPlc"/>'s
    /// <c>TryParse(..., CultureInfo.InvariantCulture)</c> idiom; a missing register (or a null reading)
    /// leaves the field at its default (0 / 0.0). Unlike the base's silent default-to-zero, a PRESENT but
    /// malformed reading fails the railway naming every offending register (#126 F7b — the former
    /// current-culture <c>Convert.ToInt32</c> let <see cref="FormatException"/> escape the Result contract
    /// on the live PLC read path).
    /// </summary>
    /// <param name="perfomances">The dictionary of performance registers.</param>
    /// <returns>A Result containing the PerformanceDataCommand instance or failure information.</returns>
    public static new Result<PerformanceDataCommand> FromPlc(IDictionary<string, Register> perfomances)
    {
        if (perfomances == null)
        {
            return Result<PerformanceDataCommand>.WithFailure($"Parameter '{nameof(perfomances)}' cannot be null");
        }

        var errors = new List<string>();
        var result = new PerformanceDataCommand
        {
            ApplicationFlag = ReadInt(perfomances, nameof(ApplicationFlag), errors),
            EventCounter = ReadInt(perfomances, nameof(EventCounter), errors),
            CurrentTime = ReadInt(perfomances, nameof(CurrentTime), errors),
            RunningTime = ReadInt(perfomances, nameof(RunningTime), errors),
            StoppedTime = ReadInt(perfomances, nameof(StoppedTime), errors),
            FaultedTime = ReadInt(perfomances, nameof(FaultedTime), errors),
            StatusFaultReason = ReadInt(perfomances, nameof(StatusFaultReason), errors),
            TotalProduction = ReadDouble(perfomances, nameof(TotalProduction), errors),
            ProductionOk = ReadDouble(perfomances, nameof(ProductionOk), errors),
            ProductionNoK = ReadDouble(perfomances, nameof(ProductionNoK), errors),
            StatusFaultReject = ReadInt(perfomances, nameof(StatusFaultReject), errors),
        };

        return errors.Count > 0
            ? Result<PerformanceDataCommand>.WithFailure(errors)
            : Result<PerformanceDataCommand>.Success(result);
    }

    /// <summary>
    /// Reads an int register invariantly. Missing register or null reading yields 0 (the base entity's
    /// default); a present, non-null reading that is not a plain invariant integer records a failure
    /// naming the register and yields 0.
    /// </summary>
    /// <param name="registers">The register dictionary keyed by property name.</param>
    /// <param name="registerName">The register key (and target property name).</param>
    /// <param name="errors">The failure accumulator for malformed readings.</param>
    /// <returns>The parsed value, or 0 for a missing/null/malformed reading.</returns>
    private static int ReadInt(IDictionary<string, Register> registers, string registerName, List<string> errors)
    {
        if (!registers.TryGetValue(registerName, out var register) || register?.Value is null)
        {
            return 0;
        }

        if (int.TryParse(register.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
        {
            return value;
        }

        errors.Add($"Register '{registerName}' carries a non-integer value '{register.Value}'.");
        return 0;
    }

    /// <summary>
    /// Reads a double register invariantly. Missing register or null reading yields 0.0 (the base
    /// entity's default); a present, non-null reading that is not an invariant number records a failure
    /// naming the register and yields 0.0.
    /// </summary>
    /// <param name="registers">The register dictionary keyed by property name.</param>
    /// <param name="registerName">The register key (and target property name).</param>
    /// <param name="errors">The failure accumulator for malformed readings.</param>
    /// <returns>The parsed value, or 0.0 for a missing/null/malformed reading.</returns>
    private static double ReadDouble(IDictionary<string, Register> registers, string registerName, List<string> errors)
    {
        if (!registers.TryGetValue(registerName, out var register) || register?.Value is null)
        {
            return 0.0;
        }

        if (double.TryParse(register.Value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var value))
        {
            return value;
        }

        errors.Add($"Register '{registerName}' carries a non-numeric value '{register.Value}'.");
        return 0.0;
    }
}