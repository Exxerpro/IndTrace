// <copyright file="RegisterDto.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Registers.Queries.GetRegisterDetail;

/// <summary>
/// Represents the RegisterDto.
/// </summary>
public class RegisterDto
{
    /// <summary>
    /// Gets or sets the RegisterId.
    /// </summary>
    public int RegisterId { get; set; }

    /// <summary>
    /// Gets or sets the MachineId.
    /// </summary>
    public int MachineId { get; set; }

    /// <summary>
    /// Gets or sets the Name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the EntitieId.
    /// </summary>
    public int VariableId { get; set; }

    /// <summary>
    /// Gets or sets the CycleId.
    /// </summary>
    public int CycleId { get; set; }

    /// <summary>
    /// Gets or sets the Value.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the DataType.
    /// </summary>
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the StatusValueId.
    /// </summary>
    public int StatusValueId { get; set; }

    /// <summary>
    /// Gets or sets the TimeStamp.
    /// </summary>
    public DateTime TimeStamp { get; set; }

    /// <summary>
    /// Executes ToDto operation.
    /// </summary>
    /// <param name="src">The src.</param>
    /// <returns>The result of ToDto.</returns>
    public static IndQuestResults.Result<RegisterDto> ToDto(Register src)
    {
        // [Fix]
        // CLAUDE
        // Date: 22/08/2025
        // Reason: Pattern 11 Fix - Updated error message to match test expectation "Parameter 'src' cannot be null"
        if (src == null)
        {
            return IndQuestResults.Result<RegisterDto>.WithFailure("Parameter 'src' cannot be null");
        }

        return IndQuestResults.Result<RegisterDto>.Success(new RegisterDto
        {
            RegisterId = src.RegisterId,
            MachineId = src.MachineId,
            Name = src.Name,
            VariableId = src.VariableId,
            CycleId = src.CycleId.Value,
            Value = src.Value ?? string.Empty,
            DataType = src.DataType,
            StatusValueId = src.StatusValueId,
            TimeStamp = src.TimeStamp,
        });
    }

    /// <summary>
    /// Executes ToEntity operation.
    /// </summary>
    /// <param name="src">The src.</param>
    /// <returns>The result of ToEntity.</returns>
    public static IndQuestResults.Result<Register> ToEntity(RegisterDto src)
    {
        // [Fix]
        // CLAUDE
        // Date: 22/08/2025
        // Reason: Pattern 11 Fix - Updated error message to match test expectation "Parameter 'src' cannot be null"
        if (src == null)
        {
            return IndQuestResults.Result<Register>.WithFailure("Parameter 'src' cannot be null");
        }

        // #39: Register is immutable — build through the guarded Create factory (RegisterId carried via the
        // factory's registerId parameter). The DTO's strings are non-null, so Create cannot fail here.
        return Register.Create(
            name: src.Name,
            description: string.Empty,
            machineId: src.MachineId,
            variableId: src.VariableId,
            cycleId: src.CycleId,
            value: src.Value,
            dataType: src.DataType,
            statusValueId: src.StatusValueId,
            timeStamp: src.TimeStamp,
            registerId: src.RegisterId);
    }

    /// <summary>
    /// Executes ToDtoList operation.
    /// </summary>
    /// <param name="src">The src.</param>
    /// <returns>The result of ToDtoList.</returns>
    public static IndQuestResults.Result<List<RegisterDto>> ToDtoList(IEnumerable<Register> src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<List<RegisterDto>>.WithFailure("Register collection cannot be null");
        }

        var list = src.Select(s => new RegisterDto
        {
            RegisterId = s.RegisterId,
            MachineId = s.MachineId,
            Name = s.Name,
            VariableId = s.VariableId,
            CycleId = s.CycleId.Value,
            Value = s.Value ?? string.Empty,
            DataType = s.DataType,
            StatusValueId = s.StatusValueId,
            TimeStamp = s.TimeStamp,
        }).ToList();
        return IndQuestResults.Result<List<RegisterDto>>.Success(list);
    }
}