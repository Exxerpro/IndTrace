// <copyright file="UpdateBarcodeCommand.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Commands.Update;

/// <summary>
/// Represents a command for updating an existing barcode within the IndTrace system.
/// </summary>
/// <remarks>
/// This command implements the gateway request pattern and provides factory methods
/// for creating instances with proper initialization. It also supports resetting to allow for reuse.
/// </remarks>
public class UpdateBarCodeCommand : IGatewayRequest<TaskGatewayResponseDto>, ICommandData, IResettable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateBarCodeCommand"/> class.
    /// </summary>
    /// <remarks>
    /// This is the default constructor that creates an empty command instance.
    /// </remarks>
    public UpdateBarCodeCommand()
    {
        this.Command = new TaskGatewayRequest();
        this.Registers = new Dictionary<string, Register>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateBarCodeCommand"/> class with the specified task gateway request.
    /// </summary>
    /// <param name="taskGatewayRequest">The task gateway request containing the command data.</param>
    /// <remarks>
    /// This private constructor enforces the use of factory methods for creating instances.
    /// </remarks>
    private UpdateBarCodeCommand(TaskGatewayRequest taskGatewayRequest)
    {
        this.Command = taskGatewayRequest;
        this.Registers = new Dictionary<string, Register>();
    }

    /// <summary>
    /// Creates a new instance of the <see cref="UpdateBarCodeCommand"/> class with the specified task gateway request.
    /// </summary>
    /// <param name="taskGatewayRequest">The task gateway request containing the command data.</param>
    /// <returns>A new instance of <see cref="ICommandData"/> initialized with the provided data.</returns>
    /// <remarks>
    /// This factory method provides a controlled way to create command instances with proper initialization.
    /// </remarks>
    public ICommandData Create(TaskGatewayRequest taskGatewayRequest)
    {
        return new UpdateBarCodeCommand(taskGatewayRequest);
    }

    /// <summary>
    /// Sets the command data for this instance and optionally copies register data.
    /// </summary>
    /// <param name="taskGatewayRequest">The task gateway request containing the command data.</param>
    /// <returns>This instance with the updated command data.</returns>
    /// <remarks>
    /// This method allows for fluent configuration of the command instance.
    /// If the task gateway request contains register data, it will be copied to the Registers property.
    /// </remarks>
    public UpdateBarCodeCommand WithData(TaskGatewayRequest taskGatewayRequest)
    {
        this.Command = taskGatewayRequest;
        if (this.Command.Registers is not null)
        {
            this.Registers = this.Command.Registers;
        }

        return this;
    }

    /// <summary>
    /// Gets or sets the task gateway request containing the command data.
    /// </summary>
    /// <value>
    /// The task gateway request that contains all the necessary information for updating a barcode.
    /// </value>
    public TaskGatewayRequest Command { get; set; }

    /// <summary>
    /// Gets or sets the dictionary of registers associated with this command.
    /// </summary>
    /// <value>
    /// A dictionary containing register data indexed by register keys.
    /// </value>
    /// <remarks>
    /// This property is populated when the command contains register data
    /// and is used to store additional context for the barcode update operation.
    /// </remarks>
    public IDictionary<string, Register> Registers { get; set; }

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
}