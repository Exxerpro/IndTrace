// <copyright file="ICommandData.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Interfaces;

using IndTrace.Domain.Entities;

/// <summary>
/// Defines a contract for command data operations in the IndTrace system.
/// </summary>
public interface ICommandData
{
    /// <summary>
    /// Gets or sets the task gateway request command.
    /// </summary>
    TaskGatewayRequest Command { get; set; }

    /// <summary>
    /// Creates a new command data instance with the specified task gateway request.
    /// </summary>
    /// <param name="taskGatewayRequest">The task gateway request to associate with the command data.</param>
    /// <returns>A new command data instance.</returns>
    ICommandData Create(TaskGatewayRequest taskGatewayRequest);

    /// <summary>
    /// Sets the command data with the specified task gateway request and returns the current instance.
    /// </summary>
    /// <param name="taskGatewayRequest">The task gateway request to set.</param>
    /// <returns>The current command data instance.</returns>
    public ICommandData WithData(TaskGatewayRequest taskGatewayRequest)
    {
        this.Command = taskGatewayRequest;
        return this;
    }
}