// <copyright file="ReferenceStamper.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

using System.Globalization;
using IndTrace.Domain.Models;

/// <summary>
/// Pure §7 reference-stamping transform. Story 32.C2 (#32/F9) — replaces the retired instance method
/// <c>TaskGatewayResponse.ApplyReferencesValues</c>/<c>ApplyReferencesValuesResult</c>: projects the 13 routing
/// scalars of a <see cref="TaskGatewayResponseDto"/> into their matching <c>References[key]</c> registers, returning
/// a NEW DTO with the stamped References. This is the step that writes the life-critical PLC routing bytes, so its
/// output is byte-frozen and pinned by the 32.C1 golden master.
/// </summary>
public static class ReferenceStamper
{
    /// <summary>
    /// Stamps the DTO's 13 routing scalars into the matching reference registers (rebuilding each immutable
    /// <see cref="Register"/> with the applied value, all other metadata preserved — issue #39) and returns a new
    /// DTO carrying the stamped References. A null or empty References dictionary is a <see cref="Result"/> failure,
    /// never a throw.
    /// </summary>
    /// <param name="dto">The wire DTO to stamp.</param>
    /// <returns>A success result carrying the stamped DTO, or a failure when References is null/empty or a rebuild fails.</returns>
    public static Result<TaskGatewayResponseDto> Apply(TaskGatewayResponseDto dto)
    {
        if (dto is null)
        {
            return Result<TaskGatewayResponseDto>.WithFailure($"Parameter '{nameof(dto)}' cannot be null");
        }

        if (dto.References is null)
        {
            return Result<TaskGatewayResponseDto>.WithFailure("references can't be null");
        }

        if (dto.References.Count == 0)
        {
            return Result<TaskGatewayResponseDto>.WithFailure("references can't be empty");
        }

        // #121 F20: the scalar stamps below feed the byte-frozen §7 wire format, so formatting is pinned to
        // the invariant culture — under a culture whose NegativeSign is not the ASCII '-' (e.g. U+2212), a
        // parameterless ToString() would stamp "−1" and diverge from the frozen bytes. Defensive pin only:
        // the output is byte-identical on every invariant-compatible host.
        var referenceValues = new Dictionary<string, string>
        {
            { nameof(dto.LastMachineId), dto.LastMachineId.ToString(CultureInfo.InvariantCulture) },
            { nameof(dto.NextMachineId), dto.NextMachineId.ToString(CultureInfo.InvariantCulture) },
            { nameof(dto.CycleStatus), dto.CycleStatus.Value.ToString(CultureInfo.InvariantCulture) },
            { nameof(dto.FlowStatus), dto.FlowStatus.Value.ToString(CultureInfo.InvariantCulture) },
            { nameof(dto.PartStatus), dto.PartStatus.Value.ToString(CultureInfo.InvariantCulture) },
            { nameof(dto.MachineType), dto.MachineType.Value.ToString(CultureInfo.InvariantCulture) },
            { nameof(dto.WorkFlowType), dto.WorkFlowType.Value.ToString(CultureInfo.InvariantCulture) },
            { nameof(dto.BarCodeId), dto.BarCodeId.ToString(CultureInfo.InvariantCulture) },
            { nameof(dto.CycleId), dto.CycleId.ToString(CultureInfo.InvariantCulture) },
            { nameof(dto.Label), dto.Label },
            { nameof(dto.CyclesOk), dto.CyclesOk.ToString(CultureInfo.InvariantCulture) },
            { nameof(dto.ShiftId), dto.ShiftId.ToString(CultureInfo.InvariantCulture) },
            { nameof(dto.ResultValidation), dto.ResultValidation.Value.ToString(CultureInfo.InvariantCulture) },
        };

        var stamped = new Dictionary<string, Register>(dto.References);

        foreach (var keyValue in referenceValues)
        {
            if (stamped.TryGetValue(keyValue.Key, out var reference))
            {
                // #39: Register is immutable — rebuild the reference carrier as a NEW register with the applied
                // value (all other fields, including RegisterId, preserved) and write it back by key.
                var rebuilt = Register.Create(
                    name: reference.Name,
                    description: reference.Description,
                    machineId: reference.MachineId,
                    variableId: reference.VariableId,
                    cycleId: reference.CycleId.Value,
                    value: keyValue.Value,
                    dataType: reference.DataType,
                    statusValueId: reference.StatusValueId,
                    timeStamp: reference.TimeStamp,
                    registerId: reference.RegisterId);

                if (rebuilt.IsFailure || rebuilt.Value is null)
                {
                    return Result<TaskGatewayResponseDto>.WithFailure(rebuilt.Error);
                }

                stamped[keyValue.Key] = rebuilt.Value;
            }
        }

        return Result<TaskGatewayResponseDto>.Success(dto with { References = stamped });
    }
}
