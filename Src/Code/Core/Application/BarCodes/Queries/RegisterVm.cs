// <copyright file="RegisterVm.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Queries;

/// <summary>
/// Represents the RegisterVm.
/// </summary>
public class RegisterVm
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterVm"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public RegisterVm()
    {
        this.Name = string.Empty;
        this.Description = string.Empty;
        this.Alias = string.Empty;
        this.Address = string.Empty;
        this.NetType = string.Empty;
        this.NativeType = string.Empty;
    }

    /// <summary>
    /// Gets or sets the EntitieId.
    /// </summary>
    public int VariableId { get; set; }

    /// <summary>
    /// Gets or sets the MachineId.
    /// </summary>
    public int MachineId { get; set; }

    /// <summary>
    /// Gets or sets the PlcId.
    /// </summary>
    public int PlcId { get; set; }

    /// <summary>
    /// Gets or sets the Name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the Description.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the Alias.
    /// </summary>
    public string Alias { get; set; }

    /// <summary>
    /// Gets or sets the Address.
    /// </summary>
    public string Address { get; set; }

    /// <summary>
    /// Gets or sets the NetType.
    /// </summary>
    public string NetType { get; set; }

    /// <summary>
    /// Gets or sets the Length.
    /// </summary>
    public int Length { get; set; }

    /// <summary>
    /// Gets or sets the Value.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Gets or sets the CycleTime.
    /// </summary>
    public int CycleTime { get; set; }

    /// <summary>
    /// Gets or sets the CycleId.
    /// </summary>
    public int CycleId { get; set; }

    /// <summary>
    /// Gets or sets the NativeType.
    /// </summary>
    public string NativeType { get; set; }

    public string EnumValue
    {
        get
        {
            try
            {
                return this.Name switch
                {
                    "CycleStatusPlc" => (this.Value is string v1 && int.TryParse(v1, out var i1)) ? EnumModel.FromValue<CycleStatus>(i1).Name : "None",
                    "PartStatusPlc" => (this.Value is string v2 && int.TryParse(v2, out var i2)) ? EnumModel.FromValue<PartStatus>(i2).Name : "None",
                    "FlowStatusPlc" => (this.Value is string v3 && int.TryParse(v3, out var i3)) ? EnumModel.FromValue<FlowStatus>(i3).Name : "None",
                    "MachineTypePlc" => (this.Value is string v4 && int.TryParse(v4, out var i4)) ? EnumModel.FromValue<MachineType>(i4).Name : "None",
                    "WorkFlowTypePlc" => (this.Value is string v5 && int.TryParse(v5, out var i5)) ? EnumModel.FromValue<WorkFlowType>(i5).Name : "None",
                    _ => this.Value ?? "None",
                };
            }
            catch (Exception)
            {
                return "None";
            }
        }
    }

    /// <summary>
    /// Executes ToDto operation.
    /// </summary>
    /// <param name="src">The src.</param>
    /// <returns>The result of ToDto.</returns>
    public static IndQuestResults.Result<RegisterVm> ToDto(Register src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<RegisterVm>.WithFailure("Register source cannot be null");
        }

        return IndQuestResults.Result<RegisterVm>.Success(new RegisterVm
        {
            VariableId = src.VariableId,
            MachineId = src.MachineId,
            PlcId = 0, // Set if available in Register
            Name = src.Name,
            Description = src.Description,
            Alias = string.Empty, // Set if available in Register
            Address = string.Empty, // Set if available in Register
            NetType = src.DataType,
            Length = 0, // Set if available in Register
            Value = src.Value,
            CycleTime = 0, // Set if available in Register
            CycleId = src.CycleId.Value,
            NativeType = src.DataType,
        });
    }

    /// <summary>
    /// Executes ToDtoList operation.
    /// </summary>
    /// <param name="src">The src.</param>
    /// <returns>The result of ToDtoList.</returns>
    public static IndQuestResults.Result<List<RegisterVm>> ToDtoList(IEnumerable<Register> src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<List<RegisterVm>>.WithFailure("Register collection cannot be null");
        }

        var list = src.Select(s => new RegisterVm
        {
            VariableId = s.VariableId,
            MachineId = s.MachineId,
            PlcId = 0,
            Name = s.Name,
            Description = s.Description,
            Alias = string.Empty,
            Address = string.Empty,
            NetType = s.DataType,
            Length = 0,
            Value = s.Value,
            CycleTime = 0,
            CycleId = s.CycleId.Value,
            NativeType = s.DataType,
        }).ToList();
        return IndQuestResults.Result<List<RegisterVm>>.Success(list);
    }

    /// <summary>
    /// Executes ToDto operation.
    /// </summary>
    /// <param name="src">The src.</param>
    /// <returns>The result of ToDto.</returns>
    public static IndQuestResults.Result<RegisterVm> ToDto(Variable src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<RegisterVm>.WithFailure("Variable source cannot be null");
        }

        return IndQuestResults.Result<RegisterVm>.Success(new RegisterVm
        {
            VariableId = src.VariableId,
            MachineId = src.MachineId,
            PlcId = src.PlcId,
            Name = src.Name,
            Description = src.Description,
            Alias = src.Alias,
            Address = src.Address,
            NetType = src.NetType,
            Length = src.Length,
            CycleTime = 0,
            CycleId = 0,
        });
    }

    /// <summary>
    /// Executes ToDtoList operation.
    /// </summary>
    /// <param name="src">The src.</param>
    /// <returns>The result of ToDtoList.</returns>
    public static IndQuestResults.Result<List<RegisterVm>> ToDtoList(IEnumerable<Variable> src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<List<RegisterVm>>.WithFailure("Variable collection cannot be null");
        }

        var list = src.Select(s => new RegisterVm
        {
            VariableId = s.VariableId,
            MachineId = s.MachineId,
            PlcId = s.PlcId,
            Name = s.Name,
            Description = s.Description,
            Alias = s.Alias,
            Address = s.Address,
            NetType = s.NetType,
            Length = s.Length,
            CycleTime = 0,
            CycleId = 0,
        }).ToList();
        return IndQuestResults.Result<List<RegisterVm>>.Success(list);
    }
}