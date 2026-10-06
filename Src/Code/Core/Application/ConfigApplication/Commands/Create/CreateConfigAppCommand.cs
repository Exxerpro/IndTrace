// <copyright file="CreateConfigAppCommand.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.ConfigApplication.Commands.Create;

/// <summary>
/// Command for creating a new application configuration with project and deployment information.
/// </summary>
/// <remarks>
/// This command contains all the necessary information to create a configuration app entry,
/// including client information, deployment location, project details, and versioning data.
/// It implements IMonitorRequest to support monitoring and tracking of the creation process.
/// </remarks>
public class CreateConfigAppCommand : IMonitorRequest<ConfigAppCreated>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateConfigAppCommand"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public CreateConfigAppCommand()
    {
        this.ConfigId = string.Empty;
        this.Client = string.Empty;
        this.Factorie = string.Empty;
        this.Machine = string.Empty;
        this.Line = string.Empty;
        this.Project = string.Empty;
        this.Version = string.Empty;
        this.Pc = string.Empty;
    }

    /// <summary>
    /// Gets or sets the unique configuration identifier.
    /// </summary>
    /// <value>The configuration ID as a string.</value>
    public string ConfigId { get; set; }

    /// <summary>
    /// Gets or sets the application identifier for this configuration.
    /// </summary>
    /// <value>The application ID as an integer.</value>
    public int AppId { get; set; }

    /// <summary>
    /// Gets or sets the client name for this configuration.
    /// </summary>
    /// <value>The client name as a string.</value>
    /// <remarks>
    /// This represents the customer or client organization for whom this configuration is being created.
    /// </remarks>
    public string Client { get; set; }

    /// <summary>
    /// Gets or sets the plant/facility name where this configuration will be deployed.
    /// </summary>
    /// <value>The plant name as a string.</value>
    /// <remarks>
    /// This identifies the specific manufacturing facility or location for the configuration.
    /// </remarks>
    public string Factorie { get; set; }

    /// <summary>
    /// Gets or sets the Machine name where this configuration will be deployed.
    /// </summary>
    /// <value>The Machine name as a string.</value>
    /// <remarks>
    /// This identifies Machine for the configuration.
    /// </remarks>
    public string Machine { get; set; }

    /// <summary>
    /// Gets or sets the Pc name where this configuration will be deployed.
    /// </summary>
    /// <value>The Pc name as a string.</value>
    /// <remarks>
    /// This identifies Pc for the configuration.
    /// </remarks>
    public string Pc { get; set; }

    /// <summary>
    /// Gets or sets the production line identifier within the plant.
    /// </summary>
    /// <value>The line identifier as a string.</value>
    /// <remarks>
    /// This specifies the particular production line where this configuration applies.
    /// </remarks>
    public string Line { get; set; }

    /// <summary>
    /// Gets or sets the machine identifier associated with this configuration.
    /// </summary>
    /// <value>The machine ID as an integer.</value>
    public int MachineId { get; set; }

    /// <summary>
    /// Gets or sets the project name or identifier for this configuration.
    /// </summary>
    /// <value>The project name as a string.</value>
    /// <remarks>
    /// This represents the specific project or initiative under which this configuration is being created.
    /// </remarks>
    public string Project { get; set; }

    /// <summary>
    /// Gets or sets the version string for this configuration.
    /// </summary>
    /// <value>The version as a string.</value>
    /// <remarks>
    /// This identifies the specific version of the configuration being created.
    /// </remarks>
    public string Version { get; set; }

    /// <summary>
    /// Gets or sets the date when this version was created or published.
    /// </summary>
    /// <value>The version date and time.</value>
    public DateTime VersionDate { get; set; }

    /// <summary>
    /// Gets or sets the date when this configuration was last modified.
    /// </summary>
    /// <value>The modification date and time.</value>
    public DateTime ModifiedDate { get; set; }
}