// <copyright file="TaskGatewayResponsePersistence.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

/// <summary>
/// Pure down-/up-projection between the immutable wire <see cref="TaskGatewayResponseDto"/> and the persisted
/// <see cref="TaskGatewayResponse"/> entity. Story 32.C2 (#32/F9). The <see cref="ToEntity"/> down-projection is
/// intentionally lossy (it drops every wire-only member — References, Recipe, Cycle, BarCode, MasterLabel,
/// MachineType, WorkFlowType, Description, ExecutionTime, RequestTask, Parameters, Name, PlcId, all of which the EF
/// config already ignores), yielding a row byte-identical to the pre-split store. <see cref="ToDto"/> is the inverse
/// used by the events-list read path: it rehydrates the 17 persisted columns; wire-only members take their defaults.
/// </summary>
public static class TaskGatewayResponsePersistence
{
    /// <summary>
    /// Down-projects the wire DTO onto a fresh persisted entity (the 17 mapped columns only). <c>ResponseId</c> is
    /// left store-generated; <c>CommandId</c> is stamped by the persistence behavior after mapping (as before).
    /// </summary>
    /// <param name="dto">The wire DTO to persist.</param>
    /// <returns>A new entity carrying the 17 persisted columns.</returns>
    public static TaskGatewayResponse ToEntity(TaskGatewayResponseDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new TaskGatewayResponse
        {
            CommandId = dto.CommandId,
            MachineId = dto.MachineId,
            BarCodeId = dto.BarCodeId,
            CycleId = dto.CycleId,
            CyclesOk = dto.CyclesOk,
            ShiftId = dto.ShiftId,
            ResultValidation = dto.ResultValidation,
            PartNumber = dto.PartNumber,
            Label = dto.Label,
            Error = dto.Error,
            LastMachineId = dto.LastMachineId,
            NextMachineId = dto.NextMachineId,
            CycleStatus = dto.CycleStatus,
            FlowStatus = dto.FlowStatus,
            PartStatus = dto.PartStatus,
            TimeStamp = dto.TimeStamp,
        };
    }

    /// <summary>
    /// Up-projects a persisted entity onto a wire DTO (the 17 persisted columns; wire-only members default). Used by
    /// the events-list read path, which reads persisted rows and renders them through the DTO-based monitor mappers.
    /// </summary>
    /// <param name="entity">The persisted entity read from the store.</param>
    /// <returns>A wire DTO carrying the persisted columns.</returns>
    public static TaskGatewayResponseDto ToDto(TaskGatewayResponse entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new TaskGatewayResponseDto
        {
            ResponseId = entity.ResponseId,
            CommandId = entity.CommandId,
            MachineId = entity.MachineId,
            BarCodeId = entity.BarCodeId,
            CycleId = entity.CycleId,
            CyclesOk = entity.CyclesOk,
            ShiftId = entity.ShiftId,
            ResultValidation = entity.ResultValidation,
            PartNumber = entity.PartNumber,
            Label = entity.Label,
            Error = entity.Error,
            LastMachineId = entity.LastMachineId,
            NextMachineId = entity.NextMachineId,
            CycleStatus = entity.CycleStatus,
            FlowStatus = entity.FlowStatus,
            PartStatus = entity.PartStatus,
            TimeStamp = entity.TimeStamp,
        };
    }
}
