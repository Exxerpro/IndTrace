// <copyright file="ReadBarCodeQuery.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Queries.GetBarCodeGatewayDetail;

/// <summary>
/// Represents the ReadBarCodeQuery.
/// </summary>
public class ReadBarCodeQuery : IGatewayRequest<TaskGatewayResponseDto>, ICommandData, IResettable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReadBarCodeQuery"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public ReadBarCodeQuery()
    {
        this.Command = new TaskGatewayRequest();
    }

    // Private constructor to enforce factory method usage
    private ReadBarCodeQuery(TaskGatewayRequest taskGatewayRequest)
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
        return new ReadBarCodeQuery(taskGatewayRequest);
    }

    /// <summary>
    /// Executes WithData operation.
    /// </summary>
    /// <param name="taskGatewayRequest">The taskGatewayRequest.</param>
    /// <returns>The result of WithData.</returns>
    public ReadBarCodeQuery WithData(TaskGatewayRequest taskGatewayRequest)
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