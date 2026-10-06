// <copyright file="MachineCreated.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Machines.Commands.Create;

/// <summary>
/// Represents the MachineCreated.
/// </summary>
public class MachineCreated : INotification
{
    /// <summary>
    /// Gets or sets the RegisterId.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the MachineId.
    /// </summary>
    public int MachineId { get; set; }

    /// <summary>
    /// Gets or sets the Name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the Location.
    /// </summary>
    public string Location { get; set; }

    /// <summary>
    /// Gets or sets the MachineType.
    /// </summary>
    public int MachineType { get; set; }

    /// <summary>
    /// Gets or sets the EnableAppTraceability.
    /// </summary>
    public int EnableAppTraceability { get; set; }

    /// <summary>
    /// Gets or sets the EnableBypassTraceability.
    /// </summary>
    public int EnableBypassTraceability { get; set; }

    /// <summary>
    /// Returns a string representation of the machine creation notification.
    /// </summary>
    /// <returns>A formatted string containing the machine creation details.</returns>
    public override string ToString()
    {
        // [Fix]
        // CLAUDE
        // Date: 23/08/2025
        // Reason: Added meaningful ToString() method for better debugging and logging in MessageDto factory
        return $"Machine Created - ID: {this.MachineId}, Name: {this.Name}, Location: {this.Location}, Type: {this.MachineType}, App Traceability: {this.EnableAppTraceability}, Bypass Traceability: {this.EnableBypassTraceability}";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MachineCreated"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public MachineCreated()
    {
        this.Name = string.Empty;
        this.Location = string.Empty;
    }

    /// <summary>
    /// Executes ToDto operation.
    /// </summary>
    /// <param name="machineName">The machineName.</param>
    /// <returns>The result of ToDto.</returns>
    public static IndQuestResults.Result<MachineCreated> ToDto(string machineName)
    {
        if (string.IsNullOrWhiteSpace(machineName))
        {
            return IndQuestResults.Result<MachineCreated>.WithFailure("machineName cannot be null or whitespace");
        }

        return IndQuestResults.Result<MachineCreated>.Success(new MachineCreated
        {
            Name = machineName,

            // Set other properties as needed if more context is available
        });
    }

    /// <summary>
    /// Executes ToDto operation.
    /// </summary>
    /// <param name="src">The src.</param>
    /// <returns>The result of ToDto.</returns>
    public static IndQuestResults.Result<MachineCreated> ToDto(Machine src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<MachineCreated>.WithFailure("Machine source cannot be null");
        }

        return IndQuestResults.Result<MachineCreated>.Success(new MachineCreated
        {
            Id = src.MachineId.Value,
            MachineId = src.MachineId.Value,
            Name = src.Name,
            Location = src.Location,
            MachineType = src.MachineType,
            EnableAppTraceability = src.EnableAppTraceability,
            EnableBypassTraceability = src.EnableBypassTraceability,
        });
    }

    /// <summary>
    /// Represents the MachineCreatedHandler.
    /// </summary>
    public class MachineCreatedHandler(INotificationService notification) : Models.Interfaces.INotificationHandler<MachineCreated>
    {
        /// <summary>
        /// Executes Process operation.
        /// </summary>
        /// <param name="notification1">The notification1.</param>
        /// <param name="cancellationToken">The cancellationToken.</param>
        /// <returns>The result of Process.</returns>
        public async Task<Result> Process(MachineCreated notification1, CancellationToken cancellationToken)
        {
            // [Fix]
            // CLAUDE
            // Date: 23/08/2025
            // Reason: Added null check for notification1 parameter to ensure Railway-Oriented Programming compliance
            if (notification1 is null)
            {
                return Result.WithFailure(["Notification cannot be null."]);
            }

            // [Fix]
            // CLAUDE
            // Date: 23/08/2025
            // Reason: INotification refactoring - use MessageDto.CreateMessage<T> factory method instead of new MessageDto()
            if (cancellationToken.IsCancellationRequested)
            {
                return Result.WithFailure(["Operation was canceled."]);
            }

            try
            {
                return await notification
                    .SendAsync(MessageDto.CreateMessage<MachineCreated>(notification1), cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Result.WithFailure([$"Operation finished with an exception {ex.Message}"]);
            }
        }
    }
}