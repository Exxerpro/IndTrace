// <copyright file="UpdateCyclesOkCommand.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Cycles.Commands.UpdateCyclesOk;

/// <summary>
/// Represents the UpdateCyclesOkCommand.
/// </summary>
public class UpdateCyclesOkCommand : IGatewayRequest<TaskGatewayResponseDto>, ICommandData, IResettable
{
    // Default constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateCyclesOkCommand"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public UpdateCyclesOkCommand()
    {
        this.Command = new TaskGatewayRequest();
    }

    // Private constructor to enforce factory method usage
    private UpdateCyclesOkCommand(TaskGatewayRequest taskGatewayRequest)
    {
        this.Command = taskGatewayRequest;
    }

    // Factory method implementation for creating instances

    /// <summary>
    /// Executes Create operation.
    /// </summary>
    /// <param name="taskGatewayRequest">The taskGatewayRequest.</param>
    /// <returns>The result of Create.</returns>
    public ICommandData Create(TaskGatewayRequest taskGatewayRequest)
    {
        return new UpdateCyclesOkCommand(taskGatewayRequest);
    }

    /// <summary>
    /// Executes WithData operation.
    /// </summary>
    /// <param name="taskGatewayRequest">The taskGatewayRequest.</param>
    /// <returns>The result of WithData.</returns>
    public UpdateCyclesOkCommand WithData(TaskGatewayRequest taskGatewayRequest)
    {
        this.Command = taskGatewayRequest;

        return this;
    }

    // Property implementation from ICommandData

    /// <summary>
    /// Gets or sets the Command.
    /// </summary>
    public TaskGatewayRequest Command { get; set; }

    /// <summary>
    /// Executes TryReset operation.
    /// </summary>
    /// <returns>The result of TryReset.</returns>
    public bool TryReset()
    {
        return true;
    }
}