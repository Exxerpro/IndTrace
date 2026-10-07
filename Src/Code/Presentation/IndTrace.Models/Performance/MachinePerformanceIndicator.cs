// <copyright file="MachinePerformanceIndicator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.UI.Models.Performance;

/// <summary>
/// Represents a machine performance indicator containing machine status and maintenance information.
/// </summary>
public class MachinePerformanceIndicator(int machineId, string machineName, string imageName, string status, DateTime lastMaintenanceDate)
{
    /// <summary>
    /// Gets or sets the unique identifier of the machine.
    /// </summary>
    public int MachineId { get; set; } = machineId;

    /// <summary>
    /// Gets or sets the name of the machine.
    /// </summary>
    public string MachineName { get; set; } = machineName;

    /// <summary>
    /// Gets or sets the current status of the machine.
    /// </summary>
    public string Status { get; set; } = status;

    /// <summary>
    /// Gets or sets the image name associated with the machine.
    /// </summary>
    public string ImageName { get; set; } = imageName;

    /// <summary>
    /// Gets or sets the date of the last maintenance performed on the machine.
    /// </summary>
    public DateTime LastMaintenanceDate { get; set; } = lastMaintenanceDate;

    // Constructor

    // Method to generate bogus data

    /// <summary>
    /// Generates bogus data for testing purposes.
    /// </summary>
    /// <param name="numberOfMachines">The number of machines to generate data for.</param>
    /// <returns>A list of machine performance indicators with sample data.</returns>
    public static List<MachinePerformanceIndicator> GenerateBogusData(int numberOfMachines)
    {
        var random = new Random();
        var statuses = new List<string> { "Running", "Stopped", "Maintenance", "Idle" };
        var machineNames = new List<string>
        {
            "Power Puncher",
            "Press Power",
            "Cross Cutter",
            "Crosswise Cutter",
            "Press Titan",
            "Laser Cutter",
            "Hydraulic Press",
            "Automatic Feeder",
            "Conveyor Belt",
            "Packaging Robot",
        };

        return machineNames.Select((t, i) => new MachinePerformanceIndicator(machineId: i + 1, machineName: t, imageName: "/_content/IndTrace.Components/img/machine" + i.ToString("D1") + ".png", status: statuses[random.Next(statuses.Count)], lastMaintenanceDate: DateTime.Now.ToLocalTime().AddDays(-random.Next(30)))).ToList();
    }

    // Override ToString method for easy display

    /// <summary>
    /// Returns a string representation of the machine performance indicator.
    /// </summary>
    /// <returns>A formatted string containing machine information.</returns>
    public override string ToString()
    {
        return $"ID: {this.MachineId}, Name: {this.MachineName}, Status: {this.Status}, Last Maintenance: {this.LastMaintenanceDate.ToShortDateString()}";
    }

}