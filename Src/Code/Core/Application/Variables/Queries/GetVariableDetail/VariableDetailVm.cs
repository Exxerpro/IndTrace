// <copyright file="VariableDetailVm.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Variables.Queries.GetVariableDetail;

/// <summary>
/// Represents the VariableDetailVm.
/// </summary>
public class VariableDetailVm
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VariableDetailVm"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public VariableDetailVm()
    {
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
    /// Gets or sets set by EF or by builder on runtime, consumer must check for null before accessing.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets set by EF or by builder on runtime, consumer must check for null before accessing.
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets set by EF or by builder on runtime, consumer must check for null before accessing.
    /// </summary>
    public string Alias { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets set by EF or by builder on runtime, consumer must check for null before accessing.
    /// </summary>
    public string NetType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Length.
    /// </summary>
    public int Length { get; set; }

    /// <summary>
    /// Gets or sets the IsActive.
    /// </summary>
    public int IsActive { get; set; }

    /// <summary>
    /// Gets or sets the Direction.
    /// </summary>
    public int Direction { get; set; }

    /// <summary>
    /// Gets or sets the VariableGroupId.
    /// </summary>
    public int VariableGroupId { get; set; }

    /// <summary>
    /// Executes ToDto operation.
    /// </summary>
    /// <param name="src">The src.</param>
    /// <returns>The result of ToDto.</returns>
    public static IndQuestResults.Result<VariableDetailVm> ToDto(Variable src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<VariableDetailVm>.WithFailure("Variable source cannot be null");
        }

        return IndQuestResults.Result<VariableDetailVm>.Success(new VariableDetailVm
        {
            VariableId = src.VariableId,
            MachineId = src.MachineId,
            PlcId = src.PlcId,
            Name = src.Name,
            Address = src.Address,
            Alias = src.Alias,
            NetType = src.NetType,
            Length = src.Length,
            IsActive = src.IsActive.Value,
            Direction = src.Direction,
            VariableGroupId = src.VariableGroupId,
        });
    }

    /// <summary>
    /// Executes ToEntity operation.
    /// </summary>
    /// <param name="src">The src.</param>
    /// <returns>The result of ToEntity.</returns>
    public static IndQuestResults.Result<Variable> ToEntity(VariableDetailVm src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<Variable>.WithFailure("VariableDetailVm source cannot be null");
        }

        // Story 2.2 (#26): construct through the guarded Variable.Create factory. VariableId (identity) is
        // applied after construction. The field this projection does not set (Description) is passed at its
        // prior entity default, so for valid data this is byte-identical; a null identity string now surfaces
        // as a graceful failure Result.
        var createResult = Variable.Create(
            machineId: src.MachineId,
            plcId: src.PlcId,
            name: src.Name,
            description: string.Empty,
            alias: src.Alias,
            address: src.Address,
            netType: src.NetType,
            length: src.Length,

            // Normalize the VM int onto the tri-state status (positive -> Active), avoiding the implicit
            // (ActiveStatus)int cast that would yield Invalid for values >= 2.
            isActive: src.IsActive > 0 ? ActiveStatus.Active : (src.IsActive < 0 ? ActiveStatus.Inactive : ActiveStatus.None),
            direction: src.Direction,
            variableGroupId: src.VariableGroupId);
        if (createResult.IsFailure)
        {
            return createResult;
        }

        var variable = createResult.Value;
        if (variable is null)
        {
            return IndQuestResults.Result<Variable>.WithFailure("Variable construction produced a null entity.");
        }

        variable.VariableId = src.VariableId;
        return IndQuestResults.Result<Variable>.Success(variable);
    }
}