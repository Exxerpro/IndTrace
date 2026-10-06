// <copyright file="CreateCyclesCommand.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Cycles.Commands.Create;

/// <summary>
/// Command to create a new cycle, implementing gateway request and resettable interfaces.
/// </summary>
public class CreateCyclesCommand : IGatewayRequest<TaskGatewayResponseDto>, ICommandData, IResettable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateCyclesCommand"/> class.
    /// </summary>
    public CreateCyclesCommand()
    {
        this.Command = new TaskGatewayRequest(GatewayTask.CreateCycleAsync);
        this.Command.GatewayTask = GatewayTask.CreateCycleAsync;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateCyclesCommand"/> class.
    /// Private constructor to enforce factory method usage.
    /// </summary>
    /// <param name="taskGatewayRequest">The gateway request to initialize the command with.</param>
    private CreateCyclesCommand(TaskGatewayRequest taskGatewayRequest)
    {
        this.Command = taskGatewayRequest;
        this.Command.GatewayTask = GatewayTask.CreateCycleAsync;
    }

    /// <summary>
    /// Factory method implementation for creating instances.
    /// </summary>
    /// <param name="taskGatewayRequest">The gateway request to initialize the command with.</param>
    /// <returns>A new instance of <see cref="CreateCyclesCommand"/>.</returns>
    public ICommandData Create(TaskGatewayRequest taskGatewayRequest)
    {
        return new CreateCyclesCommand(taskGatewayRequest);
    }

    /// <summary>
    /// Sets the command data and returns the updated instance.
    /// </summary>
    /// <param name="taskGatewayRequest">The gateway request to set.</param>
    /// <returns>The updated <see cref="CreateCyclesCommand"/> instance.</returns>
    public CreateCyclesCommand WithData(TaskGatewayRequest taskGatewayRequest)
    {
        this.Command = taskGatewayRequest;
        return this;
    }

    /// <summary>
    /// Sets the cycle status using the enum name.
    /// </summary>
    /// <param name="cycleStatusName">The name of the cycle status.</param>
    /// <returns>The updated <see cref="CreateCyclesCommand"/> instance.</returns>
    public CreateCyclesCommand WithCycleStatus(string cycleStatusName)
    {
        if (this.Command == null)
        {
            this.Command = new TaskGatewayRequest();
        }

        this.Command.CycleStatus = EnumModel.FromName<CycleStatus>(cycleStatusName);
        return this;
    }

    /// <summary>
    /// Sets the part status using the enum name.
    /// </summary>
    /// <param name="partStatusName">The name of the part status.</param>
    /// <returns>The updated <see cref="CreateCyclesCommand"/> instance.</returns>
    public CreateCyclesCommand WithPartStatus(string partStatusName)
    {
        if (this.Command == null)
        {
            this.Command = new TaskGatewayRequest();
        }

        this.Command.PartStatus = EnumModel.FromName<PartStatus>(partStatusName);
        return this;
    }

    /// <summary>
    /// Sets the flow status using the enum name.
    /// </summary>
    /// <param name="flowStatusName">The name of the flow status.</param>
    /// <returns>The updated <see cref="CreateCyclesCommand"/> instance.</returns>
    public CreateCyclesCommand WithFlowStatus(string flowStatusName)
    {
        if (this.Command == null)
        {
            this.Command = new TaskGatewayRequest();
        }

        this.Command.FlowStatus = EnumModel.FromName<FlowStatus>(flowStatusName);
        return this;
    }

    /// <summary>
    /// Sets the gateway task using the enum name.
    /// </summary>
    /// <param name="gatewayTaskName">The name of the gateway task.</param>
    /// <returns>The updated <see cref="CreateCyclesCommand"/> instance.</returns>
    public CreateCyclesCommand WithGatewayTask(string gatewayTaskName)
    {
        if (this.Command is null)
        {
            this.Command = new TaskGatewayRequest();
        }

        this.Command.GatewayTask = EnumModel.FromName<GatewayTask>(gatewayTaskName);
        return this;
    }

    /// <summary>
    /// Sets the machine type using the enum name.
    /// </summary>
    /// <param name="machineTypeName">The name of the machine type.</param>
    /// <returns>The updated <see cref="CreateCyclesCommand"/> instance.</returns>
    public CreateCyclesCommand WithMachineType(string machineTypeName)
    {
        if (this.Command == null)
        {
            this.Command = new TaskGatewayRequest();
        }

        this.Command.MachineType = EnumModel.FromName<MachineType>(machineTypeName);
        return this;
    }

    /// <summary>
    /// Sets the result validation using the enum name.
    /// </summary>
    /// <param name="resultValidationName">The name of the result validation.</param>
    /// <returns>The updated <see cref="CreateCyclesCommand"/> instance.</returns>
    public CreateCyclesCommand WithResultValidation(string resultValidationName)
    {
        if (this.Command == null)
        {
            this.Command = new TaskGatewayRequest();
        }

        this.Command.ResultValidation = EnumModel.FromName<ResultValidation>(resultValidationName);
        return this;
    }

    /// <summary>
    /// Gets or sets the gateway request command data.
    /// </summary>
    public TaskGatewayRequest Command { get; set; }

    /// <summary>
    /// Attempts to reset the command state.
    /// </summary>
    /// <returns>True if reset is successful; otherwise, false.</returns>
    public bool TryReset()
    {
        return true;
    }
}