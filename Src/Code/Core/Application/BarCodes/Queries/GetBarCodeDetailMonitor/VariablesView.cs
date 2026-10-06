// <copyright file="VariablesView.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Queries.GetBarCodeDetailMonitor;

/// <summary>
/// Represents the VariablesView.
/// </summary>
public class VariablesView
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VariablesView"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public VariablesView()
    {
        this.Name = string.Empty;
        this.Description = string.Empty;
        this.Alias = string.Empty;
        this.Address = string.Empty;
        this.NetType = string.Empty;
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
    /// Converts a <see cref="Variable"/> entity into a <see cref="VariablesView"/> using functional Result semantics.
    /// </summary>
    /// <param name="src">The source <see cref="Variable"/> entity.</param>
    /// <returns>A Result with the mapped <see cref="VariablesView"/>, or a failure when <paramref name="src"/> is null.</returns>
    public static IndQuestResults.Result<VariablesView> ToDto(Variable src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<VariablesView>.WithFailure("Variable source cannot be null");
        }

        return IndQuestResults.Result<VariablesView>.Success(new VariablesView
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
            IsActive = src.IsActive.Value,
            Direction = src.Direction,
            VariableGroupId = src.VariableGroupId,

            // Only map properties that exist in both classes
        });
    }

    /// <summary>
    /// Converts a collection of <see cref="Variable"/> entities into a list of <see cref="VariablesView"/>.
    /// </summary>
    /// <param name="src">Source collection of <see cref="Variable"/> entities.</param>
    /// <returns>A Result with the mapped list of <see cref="VariablesView"/>, or a failure when <paramref name="src"/> is null.</returns>
    public static IndQuestResults.Result<List<VariablesView>> ToDtoList(IEnumerable<Variable> src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<List<VariablesView>>.WithFailure("Variable collection cannot be null");
        }

        var list = src.Select(s => new VariablesView
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
            IsActive = s.IsActive.Value,
            Direction = s.Direction,
            VariableGroupId = s.VariableGroupId,
        }).ToList();
        return IndQuestResults.Result<List<VariablesView>>.Success(list);
    }

    /// <summary>
    /// Converts a <see cref="VariablesView"/> into a <see cref="Variable"/> entity using functional Result semantics.
    /// </summary>
    /// <param name="src">The source <see cref="VariablesView"/>.</param>
    /// <returns>A Result with the mapped <see cref="Variable"/>, or a failure when <paramref name="src"/> is null.</returns>
    public static IndQuestResults.Result<Variable> ToEntity(VariablesView src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<Variable>.WithFailure("VariablesView source cannot be null");
        }

        // Story 2.2 (#26): construct through the guarded Variable.Create factory. VariableId (identity) is
        // applied after construction (Create takes only the entity's non-identity scalar shape). For valid
        // data this is byte-identical; a null identity string now surfaces as a graceful failure Result.
        var createResult = Variable.Create(
            machineId: src.MachineId,
            plcId: src.PlcId,
            name: src.Name,
            description: src.Description,
            alias: src.Alias,
            address: src.Address,
            netType: src.NetType,
            length: src.Length,

            // Normalize the view int onto the tri-state status (positive -> Active), avoiding the implicit
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