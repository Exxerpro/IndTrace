// <copyright file="UpdateVariableCommand.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Variables.Commands.Update;

using IndTrace.Application.Variables.Queries.GetVariableDetail;

/// <summary>
/// Represents a command to update a variable's details in the system.
/// </summary>
public class UpdateVariableCommand : IMonitorRequest<VariableDetailVm>
{
#nullable enable
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateVariableCommand"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public UpdateVariableCommand()
    {
        // [Fix]
        // CLAUDE
        // Date: 24/08/2025
        // Reason: [PATTERN 17 BUG FIX] - Nullable string properties should not be initialized to string.Empty in constructor. They should remain null to allow proper null-coalescing in UpdateVariableCommandHandler
        // Remove string.Empty initialization for nullable properties to allow proper null handling
    }

    /// <summary>
    /// Gets or sets the unique identifier of the variable.
    /// </summary>
    public int? VariableId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the machine associated with the variable.
    /// </summary>
    public int? MachineId { get; set; }

    /// <summary>
    /// Gets or sets the PLC (Programmable Logic Controller) name or identifier.
    /// </summary>
    public string? Plc { get; set; }

    /// <summary>
    /// Gets or sets the name of the variable.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the address of the variable in the PLC.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Gets or sets the alias for the variable.
    /// </summary>
    public string? Alias { get; set; }

    /// <summary>
    /// Gets or sets the data type of the variable.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the length of the variable (if applicable).
    /// </summary>
    public int? Length { get; set; }

    /// <summary>
    /// Gets or sets the event identifier associated with the variable.
    /// </summary>
    public int? Event { get; set; }

    /// <summary>
    /// Gets or sets the direction of the variable (input/output).
    /// </summary>
    public int? Direction { get; set; }

    /// <summary>
    /// Gets or sets the group identifier to which the variable belongs.
    /// </summary>
    public int? VariableGroupId { get; set; }
}