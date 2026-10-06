// <copyright file="ConfigAppDto.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.ConfigApplication.Queries.GetConfigAppsDetail;

/// <summary>
/// Data transfer object representing an application configuration with deployment and environment information.
/// </summary>
/// <remarks>
/// This DTO contains configuration details for application deployments, including hardware assignments,
/// client information, facility details, and version tracking. It provides mapping methods for conversion
/// between entity and DTO formats.
/// </remarks>
public class ConfigAppDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the application configuration.
    /// </summary>
    /// <value>The configuration app ID as a string. Defaults to empty string if not specified.</value>
    public string ConfigAppId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the machine where this configuration is deployed.
    /// </summary>
    /// <value>The machine ID as an integer.</value>
    public int MachineId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the PLC (Programmable Logic Controller) associated with this configuration.
    /// </summary>
    /// <value>The PLC ID as an integer.</value>
    public int PlcId { get; set; }

    /// <summary>
    /// Gets or sets the PC (Personal Computer) identifier or number for this configuration.
    /// </summary>
    /// <value>The PC identifier as an integer.</value>
    /// <remarks>
    /// This typically represents the computer or workstation where the application is running.
    /// </remarks>
    public string Pc { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the application identifier for this configuration.
    /// </summary>
    /// <value>The application ID as an integer.</value>
    public int AppId { get; set; }

    /// <summary>
    /// Gets or sets the client name for this configuration deployment.
    /// </summary>
    /// <value>The client name as a string. Defaults to empty string if not specified.</value>
    /// <remarks>
    /// This represents the customer or client organization for whom this configuration is deployed.
    /// </remarks>
    public string Client { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the factory or manufacturing facility name where this configuration is deployed.
    /// </summary>
    /// <value>The factory name as a string. Defaults to empty string if not specified.</value>
    /// <remarks>
    /// This identifies the specific manufacturing facility or plant location for the configuration.
    /// </remarks>
    public string Factory { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the production line identifier within the factory.
    /// </summary>
    /// <value>The line identifier as a string. Defaults to empty string if not specified.</value>
    /// <remarks>
    /// This specifies the particular production line where this configuration is active.
    /// </remarks>
    public string Line { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project name or identifier associated with this configuration.
    /// </summary>
    /// <value>The project name as a string. Defaults to empty string if not specified.</value>
    /// <remarks>
    /// This represents the specific project or initiative under which this configuration was created.
    /// </remarks>
    public string Project { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version string for this configuration.
    /// </summary>
    /// <value>The version as a string. Defaults to empty string if not specified.</value>
    /// <remarks>
    /// This identifies the specific version of the configuration being used.
    /// </remarks>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Converts a ConfigApp entity to a ConfigAppDto using functional Result semantics.
    /// </summary>
    /// <param name="src">The ConfigApp entity to convert.</param>
    /// <returns>A Result containing the converted DTO or a failure when <paramref name="src"/> is null.</returns>
    public static IndQuestResults.Result<ConfigAppDto> ToDto(ConfigApp src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<ConfigAppDto>.WithFailure("ConfigApp source cannot be null");
        }

        return IndQuestResults.Result<ConfigAppDto>.Success(new ConfigAppDto
        {
            ConfigAppId = src.ConfigAppId,
            MachineId = src.MachineId,
            PlcId = src.PlcId,
            Pc = src.Pc,
            AppId = src.AppId,
            Client = src.Client,
            Factory = src.Factory,
            Line = src.Line,
            Project = src.Project,
            Version = src.Version,
        });
    }

    /// <summary>
    /// Converts a ConfigAppDto to a ConfigApp entity using functional Result semantics.
    /// </summary>
    /// <param name="src">The ConfigAppDto to convert.</param>
    /// <returns>A Result containing the entity or a failure when <paramref name="src"/> is null.</returns>
    public static IndQuestResults.Result<ConfigApp> ToEntity(ConfigAppDto src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<ConfigApp>.WithFailure("ConfigAppDto source cannot be null");
        }

        return IndQuestResults.Result<ConfigApp>.Success(new ConfigApp
        {
            ConfigAppId = src.ConfigAppId,
            MachineId = src.MachineId,
            PlcId = src.PlcId,
            Pc = src.Pc,
            AppId = src.AppId,
            Client = src.Client,
            Factory = src.Factory,
            Line = src.Line,
            Project = src.Project,
            Version = src.Version,
        });
    }
}