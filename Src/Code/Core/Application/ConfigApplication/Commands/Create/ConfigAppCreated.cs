// <copyright file="ConfigAppCreated.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.ConfigApplication.Commands.Create;

/// <summary>
/// Event notification that is raised when a new configuration app has been successfully created in the system.
/// </summary>
/// <remarks>
/// This event contains the deployment and version information for the created configuration.
/// It implements INotification for MediatR pattern and can trigger additional processes such as
/// deployment notifications, audit logging, or integration with external systems.
/// </remarks>
public class ConfigAppCreated : INotification
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigAppCreated"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public ConfigAppCreated()
    {
        this.ConfigId = string.Empty;
        this.Client = string.Empty;
        this.Factory = string.Empty;
        this.Line = string.Empty;
        this.Project = string.Empty;
        this.Version = string.Empty;
    }

    /// <summary>
    /// Gets or sets the unique configuration identifier.
    /// </summary>
    /// <value>The configuration ID as a string.</value>
    public string ConfigId { get; set; }

    /// <summary>
    /// Gets or sets the application identifier for this configuration.
    /// </summary>
    /// <value>The application ID as an integer.</value>
    public int AppId { get; set; }

    /// <summary>
    /// Gets or sets the client name for this configuration deployment.
    /// </summary>
    /// <value>The client name as a string.</value>
    public string Client { get; set; }

    /// <summary>
    /// Gets or sets the factory or manufacturing facility name where this configuration was deployed.
    /// </summary>
    /// <value>The factory name as a string.</value>
    public string Factory { get; set; }

    /// <summary>
    /// Gets or sets the production line identifier within the factory.
    /// </summary>
    /// <value>The line identifier as a string.</value>
    public string Line { get; set; }

    /// <summary>
    /// Gets or sets the machine identifier associated with this configuration.
    /// </summary>
    /// <value>The machine ID as an integer.</value>
    public int MachineId { get; set; }

    /// <summary>
    /// Gets or sets the project name or identifier associated with this configuration.
    /// </summary>
    /// <value>The project name as a string.</value>
    public string Project { get; set; }

    /// <summary>
    /// Gets or sets the version string for this configuration.
    /// </summary>
    /// <value>The version as a string.</value>
    public string Version { get; set; }

    /// <summary>
    /// Gets or sets the date when this version was created or published.
    /// </summary>
    /// <value>The version date and time.</value>
    public DateTime VersionDate { get; set; }

    /// <summary>
    /// Gets or sets the date when this configuration was last modified.
    /// </summary>
    /// <value>The modification date and time.</value>
    public DateTime ModifiedDate { get; set; }

    /// <summary>
    /// Returns a string representation of the configuration app creation notification.
    /// </summary>
    /// <returns>A formatted string containing the configuration deployment details.</returns>
    public override string ToString()
    {
        // [Fix]
        // CLAUDE
        // Date: 23/08/2025
        // Reason: Added meaningful ToString() method for better debugging and logging in MessageDto factory
        return $"Config App Created - ID: {this.ConfigId}, Client: {this.Client}, Factory: {this.Factory}, Line: {this.Line}, Machine: {this.MachineId}, Project: {this.Project}, Version: {this.Version}";
    }

    /// <summary>
    /// Handler for processing ConfigAppCreated event notifications.
    /// </summary>
    /// <remarks>
    /// This handler is responsible for sending notifications when a configuration app is created.
    /// It implements the INotificationHandler pattern for decoupled event processing.
    /// </remarks>
    public class ConfigAppCreatedHandler(INotificationService notification) : Models.Interfaces.INotificationHandler<ConfigAppCreated>
    {
        /// <summary>
        /// Processes the config app created event by sending a notification message.
        /// </summary>
        /// <param name="notification1">The ConfigAppCreated event to process.</param>
        /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// Currently sends an empty MessageDto. This should be enhanced to include deployment notification details.
        /// </remarks>
        public async Task<Result> Process(ConfigAppCreated notification1, CancellationToken cancellationToken)
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
                    .SendAsync(MessageDto.CreateMessage<ConfigAppCreated>(notification1), cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Result.WithFailure([$"Operation finished with an exception {ex.Message}"]);
            }
        }
    }
}