// <copyright file="PlcDto.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Plcs.Queries.GetDetail;

/// <summary>
/// Represents the PlcDto.
/// </summary>
public class PlcDto
{
    /// <summary>
    /// Gets or sets the PlcId.
    /// </summary>
    public int PlcId { get; set; }

    /// <summary>
    /// Gets or sets the MachineId.
    /// </summary>
    public int MachineId { get; set; }

    /// <summary>
    /// Gets or sets the Name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the PlcType.
    /// </summary>
    public string PlcType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the IpAddress.
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the PlcBrand.
    /// </summary>
    public string PlcBrand { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Options.
    /// </summary>
    public string Options { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the BrandOwner.
    /// </summary>
    public string BrandOwner { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the CommLibrary.
    /// </summary>
    public string CommLibrary { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets the EnableSimulation.
    /// </summary>
    public bool EnableSimulation { get; set; }

    /// <summary>
    /// Gets or sets the Machines.
    /// </summary>
    public IEnumerable<Machine> Machines { get; set; } = [];

    /// <summary>
    /// Gets or sets the VariablesGroups.
    /// </summary>
    public IDictionary<string, VariablesGroup> VariablesGroups { get; set; } = new Dictionary<string, VariablesGroup>();

    /// <summary>
    /// Gets or sets the Variables.
    /// </summary>
    public IDictionary<string, Variable> Variables { get; set; } = new Dictionary<string, Variable>();

    /// <summary>
    /// Gets or sets the Registers.
    /// </summary>
    public IDictionary<string, Register> Registers { get; set; } = new Dictionary<string, Register>();

    /// <summary>
    /// Gets or sets the Perfomances.
    /// </summary>
    public IDictionary<string, Register> Perfomances { get; set; } = new Dictionary<string, Register>();

    /// <summary>
    /// Gets or sets the References.
    /// </summary>
    public IDictionary<string, Register> References { get; set; } = new Dictionary<string, Register>();

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets the Enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets the HasOeeEnabled.
    /// </summary>
    public bool HasOeeEnabled { get; set; }

    /// <summary>
    /// Executes ToDto operation.
    /// </summary>
    /// <param name="src">The src.</param>
    /// <returns>The result of ToDto.</returns>
    public static IndQuestResults.Result<PlcDto> ToDto(Plc src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<PlcDto>.WithFailure("Plc source cannot be null");
        }

        return IndQuestResults.Result<PlcDto>.Success(new PlcDto
        {
            PlcId = src.PlcId,
            MachineId = src.MachineId,
            Name = src.Name,
            PlcType = src.PlcType,
            IpAddress = src.IpAddress,
            PlcBrand = src.PlcBrand,
            Options = src.Options,
            BrandOwner = src.BrandOwner,
            CommLibrary = src.CommLibrary,
            Enabled = src.Enabled.Value != 0,
        });
    }

    /// <summary>
    /// Executes ToEntity operation.
    /// </summary>
    /// <param name="src">The src.</param>
    /// <returns>The result of ToEntity.</returns>
    public static IndQuestResults.Result<Plc> ToEntity(PlcDto src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<Plc>.WithFailure("PlcDto source cannot be null");
        }

        // Story 2.4 (#26): construct through the guarded Plc.Create factory. PlcId (identity) is applied
        // after construction (Create takes only the entity's non-identity scalar shape). For valid data this
        // is byte-identical; a null identity string now surfaces as a graceful failure Result.
        var createResult = Plc.Create(
            machineId: src.MachineId,
            enabled: src.Enabled ? ActiveStatus.Active : ActiveStatus.None,
            name: src.Name,
            ipAddress: src.IpAddress,
            plcType: src.PlcType,
            plcBrand: src.PlcBrand,
            options: src.Options,
            commLibrary: src.CommLibrary,
            brandOwner: src.BrandOwner);
        if (createResult.IsFailure)
        {
            return createResult;
        }

        var plc = createResult.Value;
        if (plc is null)
        {
            return IndQuestResults.Result<Plc>.WithFailure("Plc construction produced a null entity.");
        }

        plc.PlcId = src.PlcId;
        return IndQuestResults.Result<Plc>.Success(plc);
    }
}