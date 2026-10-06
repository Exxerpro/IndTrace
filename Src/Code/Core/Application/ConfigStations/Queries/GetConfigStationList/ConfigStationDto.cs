// <copyright file="ConfigStationDto.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.ConfigStations.Queries.GetConfigStationList;

/// <summary>
/// Represents the ConfigStationDto.
/// </summary>
public class ConfigStationDto
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigStationDto"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public ConfigStationDto()
    {
        this.ConfigStation = new ConfigStation();
        this.ConfigAppId = string.Empty;
        this.Cliente = string.Empty;
        this.Planta = string.Empty;
        this.Linea = string.Empty;
        this.Proyecto = string.Empty;
        this.Version = string.Empty;
    }

    /// <summary>
    /// Gets or sets the ConfigStationId.
    /// </summary>
    public int ConfigStationId { get; set; }

    /// <summary>
    /// Gets or sets the ProductId.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets the NextMachineId.
    /// </summary>
    public int NextMachineId { get; set; }

    /// <summary>
    /// Gets or sets the LastMachineId.
    /// </summary>
    public int LastMachineId { get; set; }

    /// <summary>
    /// Gets or sets the ConfigStation.
    /// </summary>
    public virtual ConfigStation ConfigStation { get; set; }

    /// <summary>
    /// Gets or sets the ConfigAppId.
    /// </summary>
    public string ConfigAppId { get; set; }

    /// <summary>
    /// Gets or sets the AppId.
    /// </summary>
    public int AppId { get; set; }

    /// <summary>
    /// Gets or sets the Client.
    /// </summary>
    public string Cliente { get; set; }

    /// <summary>
    /// Gets or sets the Factory.
    /// </summary>
    public string Planta { get; set; }

    /// <summary>
    /// Gets or sets the Line.
    /// </summary>
    public string Linea { get; set; }

    /// <summary>
    /// Gets or sets the MachineId.
    /// </summary>
    public int MaquinaId { get; set; }

    /// <summary>
    /// Gets or sets the Project.
    /// </summary>
    public string Proyecto { get; set; }

    /// <summary>
    /// Gets or sets the Version.
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// Gets or sets the VersionDate.
    /// </summary>
    public DateTime VersionDate { get; set; }

    /// <summary>
    /// Gets or sets the ModifiedDate.
    /// </summary>
    public DateTime ModifiedDate { get; set; }

    /// <summary>
    /// Executes ToDto operation.
    /// </summary>
    /// <param name="src">The src.</param>
    /// <returns>The result of ToDto.</returns>
    public static IndQuestResults.Result<ConfigStationDto> ToDto(ConfigStation src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<ConfigStationDto>.WithFailure("ConfigStation source cannot be null");
        }

        return IndQuestResults.Result<ConfigStationDto>.Success(new ConfigStationDto
        {
            // ConfigStationId, ProductId, NextMachineId, LastMachineId are not present in entity, so skip
            ConfigStation = src,
            ConfigAppId = src.ConfigAppId,
            AppId = src.AppId,
            Cliente = src.Client,
            Planta = src.Factory,
            Linea = src.Line,
            MaquinaId = src.MachineId,
            Proyecto = src.Project,
            Version = src.Version,
            VersionDate = src.VersionDate,
            ModifiedDate = src.ModifiedDate,
        });
    }

    /// <summary>
    /// Executes ToEntity operation.
    /// </summary>
    /// <param name="src">The src.</param>
    /// <returns>The result of ToEntity.</returns>
    public static IndQuestResults.Result<ConfigStation> ToEntity(ConfigStationDto src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<ConfigStation>.WithFailure("ConfigStationDto source cannot be null");
        }

        return IndQuestResults.Result<ConfigStation>.Success(new ConfigStation
        {
            ConfigAppId = src.ConfigAppId,
            AppId = src.AppId,
            Client = src.Cliente,
            Factory = src.Planta,
            Line = src.Linea,
            MachineId = src.MaquinaId,
            Project = src.Proyecto,
            Version = src.Version,
            VersionDate = src.VersionDate,
            ModifiedDate = src.ModifiedDate,
        });
    }
}