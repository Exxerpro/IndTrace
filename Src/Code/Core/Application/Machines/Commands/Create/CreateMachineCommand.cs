// <copyright file="CreateMachineCommand.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Machines.Commands.Create;

/// <summary>
/// Request for creating a new machine in the monitoring system.
/// </summary>
/// <remarks>
/// This request implements the monitor request pattern to establish machine
/// monitoring capabilities and register new production equipment in the system.
/// </remarks>
public class CreateMachineMonitorRequest : IMonitorRequest<MachineCreated>
{
    /// <summary>
    /// Gets or sets the unique identifier for the request.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the machine to be created.
    /// </summary>
    public int MachineId { get; set; }

    /// <summary>
    /// Gets or sets the name of the machine.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the physical location of the machine.
    /// </summary>
    public string Location { get; set; }

    /// <summary>
    /// Gets or sets the type of machine (see machine type enumeration).
    /// </summary>
    public int MachineType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether application traceability is enabled (1 = enabled, 0 = disabled).
    /// </summary>
    public int EnableAppTraceability { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether bypass traceability is enabled (1 = enabled, 0 = disabled).
    /// </summary>
    public int EnableBypassTraceability { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateMachineMonitorRequest"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public CreateMachineMonitorRequest()
    {
        this.Name = string.Empty;
        this.Location = string.Empty;
    }
}