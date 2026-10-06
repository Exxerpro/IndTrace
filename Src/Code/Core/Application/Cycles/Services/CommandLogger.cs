// <copyright file="CommandLogger.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Cycles.Services;

/// <summary>
/// Logs gateway commands for audit and tracking.
/// </summary>
public class CommandLogger : ICommandLogger
{
    private readonly IRepository<TaskGatewayRequest> _commandRepository;
    private readonly IDateTimeMachine _dateTimeMachine;
    private readonly ILogger<CommandLogger> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandLogger"/> class.
    /// </summary>
    /// <param name="commandRepository">The command repository.</param>
    /// <param name="dateTimeMachine">The date time service.</param>
    /// <param name="logger">The logger instance.</param>
    public CommandLogger(
        IRepository<TaskGatewayRequest> commandRepository,
        IDateTimeMachine dateTimeMachine,
        ILogger<CommandLogger> logger)
    {
        _commandRepository = commandRepository;
        _dateTimeMachine = dateTimeMachine;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result> LogCommandAsync(
        TaskGatewayRequest command,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("LogCommandAsync cancelled");
            return ResultExtensions.Cancelled();
        }

        if (command is null)
        {
            _logger.LogError("Command is null");
            return ResultExtensions.FailForNullArgument(nameof(command));
        }

        _logger.LogInformation(
            "Logging command: GatewayTask={GatewayTask}, MachineId={MachineId}, BarCodeId={BarCodeId}",
            command.GatewayTask, command.MachineId, command.BarCodeId);

        try
        {
            var result = await _commandRepository
                .AddAsync(command, cancellationToken)
                .ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Command logged successfully with Id={Id}", result.Value);
            }
            else
            {
                _logger.LogError("Failed to log command: {Error}", result.Error);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception logging command");
            return Result.WithFailure($"Exception logging command: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public TaskGatewayRequest CreateCommand(
        IBarCodeResult barCodeInfo,
        GatewayTask gatewayTask,
        string? comment = null)
    {
        if (barCodeInfo is null)
        {
            throw new ArgumentNullException(nameof(barCodeInfo));
        }

        return new TaskGatewayRequest
        {
            MachineId = barCodeInfo.MachineId,
            ProductId = barCodeInfo.Product.ProductId.Value,
            PartNumber = barCodeInfo.Product.PartNumber,
            BarCodeId = barCodeInfo.BarCodeId,
            BarCode = barCodeInfo.BarCode?.Label.Value ?? string.Empty,
            GatewayTask = gatewayTask,
            TimeStamp = _dateTimeMachine.Now,
            Comment = comment ?? string.Empty,
            IsCompleted = false
        };
    }

    /// <inheritdoc/>
    public TaskGatewayRequest CreateCommand(
        CycleUpdateLoadState load,
        GatewayTask gatewayTask,
        string? comment = null)
    {
        if (load is null)
        {
            throw new ArgumentNullException(nameof(load));
        }

        // Story 6.5 (Task 4) — identical field mapping to the IBarCodeResult overload, sourced from the snapshot.
        return new TaskGatewayRequest
        {
            MachineId = load.MachineId,
            ProductId = load.Product.ProductId.Value,
            PartNumber = load.Product.PartNumber,
            BarCodeId = load.BarCodeId,
            BarCode = load.BarCode?.Label.Value ?? string.Empty,
            GatewayTask = gatewayTask,
            TimeStamp = _dateTimeMachine.Now,
            Comment = comment ?? string.Empty,
            IsCompleted = false
        };
    }
}