// <copyright file="CreateBarCodeCommand.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Commands.Create;

/// <summary>
/// Represents a command for creating a new barcode within the IndTrace system.
/// </summary>
/// <remarks>
/// This command implements the gateway request pattern and provides factory methods
/// for creating instances with proper initialization.
/// </remarks>
public class CreateBarCodeCommand : IGatewayRequest<TaskGatewayResponseDto>, ICommandData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateBarCodeCommand"/> class.
    /// </summary>
    /// <remarks>
    /// This is the default constructor that creates an empty command instance.
    /// </remarks>
    public CreateBarCodeCommand()
    {
        this.Command = new TaskGatewayRequest();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateBarCodeCommand"/> class with the specified task gateway request.
    /// </summary>
    /// <param name="taskGatewayRequest">The task gateway request containing the command data.</param>
    /// <remarks>
    /// This private constructor enforces the use of factory methods for creating instances.
    /// </remarks>
    private CreateBarCodeCommand(TaskGatewayRequest taskGatewayRequest)
    {
        this.Command = taskGatewayRequest;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="CreateBarCodeCommand"/> class with the specified task gateway request.
    /// </summary>
    /// <param name="taskGatewayRequest">The task gateway request containing the command data.</param>
    /// <returns>A new instance of <see cref="ICommandData"/> initialized with the provided data.</returns>
    /// <remarks>
    /// This factory method provides a controlled way to create command instances with proper initialization.
    /// </remarks>
    public ICommandData Create(TaskGatewayRequest taskGatewayRequest)
    {
        return new CreateBarCodeCommand(taskGatewayRequest);
    }

    /// <summary>
    /// Sets the command data for this instance.
    /// </summary>
    /// <param name="taskGatewayRequest">The task gateway request containing the command data.</param>
    /// <returns>This instance with the updated command data.</returns>
    /// <remarks>
    /// This method allows for fluent configuration of the command instance.
    /// </remarks>
    public ICommandData WithData(TaskGatewayRequest taskGatewayRequest)
    {
        this.Command = taskGatewayRequest;
        return this;
    }

    /// <summary>
    /// Attempts to reset the command to its initial state.
    /// </summary>
    /// <returns>Always returns <c>true</c> indicating successful reset.</returns>
    /// <remarks>
    /// This implementation always succeeds as the command can be reset without any constraints.
    /// </remarks>
    public bool TryReset()
    {
        return true;
    }

    /// <summary>
    /// Gets or sets the task gateway request containing the command data.
    /// </summary>
    /// <value>
    /// The task gateway request that contains all the necessary information for creating a barcode.
    /// </value>
    public TaskGatewayRequest Command { get; set; }
}