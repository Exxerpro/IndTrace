// <copyright file="PLCCreated.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Plcs.Commands.Create;

/// <summary>
/// Notification representing the creation of a PLC.
/// </summary>
public class PlcCreated : INotification
{
    /// <summary>
    /// Gets or sets the unique identifier of the PLC.
    /// </summary>
    public int PlcId { get; set; }

    private string ipAddress = string.Empty;
    private string plcType = string.Empty;
    private string plcBrand = string.Empty;
    private string commLibrary = string.Empty;
    private string name = string.Empty;
    private string brandOwner = string.Empty;

    /// <summary>
    /// Gets or sets the IP address of the PLC.
    /// </summary>
    public string IpAddress
    {
        get => this.ipAddress;
        set => this.ipAddress = value ?? string.Empty;
    }

    /// <summary>
    /// Gets or sets the type of the PLC.
    /// </summary>
    public string PlcType
    {
        get => this.plcType;
        set => this.plcType = value ?? string.Empty;
    }

    /// <summary>
    /// Gets or sets the brand of the PLC.
    /// </summary>
    public string PlcBrand
    {
        get => this.plcBrand;
        set => this.plcBrand = value ?? string.Empty;
    }

    /// <summary>
    /// Gets or sets the communication library used by the PLC.
    /// </summary>
    public string CommLibrary
    {
        get => this.commLibrary;
        set => this.commLibrary = value ?? string.Empty;
    }

    /// <summary>
    /// Gets or sets the name of the PLC.
    /// </summary>
    public string Name
    {
        get => this.name;
        set => this.name = value ?? string.Empty;
    }

    /// <summary>
    /// Gets or sets the brand owner of the PLC.
    /// </summary>
    public string BrandOwner
    {
        get => this.brandOwner;
        set => this.brandOwner = value ?? string.Empty;
    }

    /// <summary>
    /// Returns a string representation of the PLC created notification.
    /// </summary>
    /// <returns>A formatted string containing the PLC created details.</returns>
    public override string ToString()
    {
        // [Fix]
        // CLAUDE
        // Date: 23/08/2025
        // Reason: Added meaningful ToString() method for better debugging and logging in MessageDto factory
        return $"PLC Created - PlcId: {this.PlcId}, Name: {this.Name}, IpAddress: {this.IpAddress}";
    }

    /// <summary>
    /// Handler for processing <see cref="PlcCreated"/> notifications.
    /// </summary>
    public class PlcCreatedHandler(INotificationService notification) : Application.Models.Interfaces.INotificationHandler<PlcCreated>
    {
        /// <summary>
        /// Processes the PLC created notification.
        /// </summary>
        /// <param name="notification1">The notification instance.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task<Result> Process(PlcCreated notification1, CancellationToken cancellationToken)
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
                    .SendAsync(MessageDto.CreateMessage<PlcCreated>(notification1), cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Result.WithFailure([$"Operation finished with an exception {ex.Message}"]);
            }
        }
    }
}
