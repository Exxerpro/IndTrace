// <copyright file="ConfigStationUpdated.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.ConfigStations.Commands.Update;

/// <summary>
/// Represents the ConfigStationUpdated.
/// </summary>
public class ConfigStationUpdated : INotification
{
    /// <summary>
    /// Gets or sets the ConfigStationId.
    /// </summary>
    public int ConfigStationId { get; set; }
    public string ConfigAppId { get; set; } = string.Empty;
    public string Client { get; set; } = string.Empty;
    public string Factory { get; set; } = string.Empty;
    public string Line { get; set; } = string.Empty;
    public int MachineId { get; set; }
    public string Project { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public DateTime VersionDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public int PlcId { get; set; }
    public int Pc { get; set; }
    // Audit fields
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? CreatedOn { get; set; }
    public string ModifiedBy { get; set; } = string.Empty;
    public DateTime? ModifiedOn { get; set; }

    public static Result<ConfigStationUpdated> From(ConfigStation? entity)
    {
        if (entity == null)
            return Result<ConfigStationUpdated>.WithFailure("ConfigStation entity cannot be null");

        var dto = new ConfigStationUpdated
        {
            ConfigStationId = entity.AppId,
            ConfigAppId = entity.ConfigAppId,
            Client = entity.Client,
            Factory = entity.Factory,
            Line = entity.Line,
            MachineId = entity.MachineId,
            Project = entity.Project,
            Version = entity.Version,
            VersionDate = entity.VersionDate,
            ModifiedDate = entity.ModifiedDate,
            PlcId = entity.PlcId,
            Pc = entity.Pc,
            CreatedBy = entity.CreatedBy,
            CreatedOn = entity.CreatedOn,
            ModifiedBy = entity.ModifiedBy,
            ModifiedOn = entity.ModifiedOn
        };

        return Result<ConfigStationUpdated>.Success(dto);
    }

    /// <summary>
    /// Returns a string representation of the config station updated notification.
    /// </summary>
    /// <returns>A formatted string containing the config station updated details.</returns>
    public override string ToString()
    {
        // [Fix]
        // CLAUDE
        // Date: 23/08/2025
        // Reason: Added meaningful ToString() method for better debugging and logging in MessageDto factory
        return $"ConfigStation Updated - ConfigStationId: {this.ConfigStationId}, Client: {this.Client}, Factory: {this.Factory}, Line: {this.Line}, Project: {this.Project}, Version: {this.Version}";
    }

    /// <summary>
    /// Represents the ConfigStationUpdatedHandler.
    /// </summary>
    public class ConfigStationUpdatedHandler(INotificationService notification)
        : Models.Interfaces.INotificationHandler<ConfigStationUpdated>
    {
        /// <summary>
        /// Executes Process operation.
        /// </summary>
        /// <param name="notification1">The notification1.</param>
        /// <param name="cancellationToken">The cancellationToken.</param>
        /// <returns>The result of Process.</returns>
        public async Task<Result> Process(ConfigStationUpdated notification1, CancellationToken cancellationToken)
        {
            // [Fix]
            // CLAUDE
            // Date: 23/08/2025
            // Reason: Added null check for notification1 parameter to ensure Railway-Oriented Programming compliance
            if (notification1 is null)
            {
                return Result.WithFailure(["Notification cannot be null."]);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return Result.WithFailure(["Operation was canceled."]);
            }

            // [Fix]
            // CLAUDE
            // Date: 23/08/2025
            // Reason: Refactored to use MessageDto.CreateMessage factory method and return SendAsync result
            try
            {
                return await notification
                    .SendAsync(MessageDto.CreateMessage<ConfigStationUpdated>(notification1), cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Result.WithFailure([$"Operation finished with an exception {ex.Message}"]);
            }
        }
    }
}