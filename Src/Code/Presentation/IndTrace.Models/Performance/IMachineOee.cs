// <copyright file="IMachineOee.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.UI.Models.Performance;

/// <summary>
/// Defines the interface for machine Overall Equipment Effectiveness (OEE) calculations.
/// </summary>
public interface IMachineOee
{
    /// <summary>
    /// Gets the availability percentage of the machine.
    /// </summary>
    double Availability { get; }

    /// <summary>
    /// Gets the capacity of the machine in units per time.
    /// </summary>
    double Capacity { get; }

    /// <summary>
    /// Gets the defective rate as a percentage.
    /// </summary>
    double DefectiveRate { get; }

    /// <summary>
    /// Gets the name of the machine.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the Overall Equipment Effectiveness (OEE) percentage.
    /// </summary>
    double Oee { get; }

    /// <summary>
    /// Gets the performance percentage of the machine.
    /// </summary>
    double Performance { get; }

    /// <summary>
    /// Gets the number of pieces produced by the machine.
    /// </summary>
    double ProducedPieces { get; }

    /// <summary>
    /// Gets the quality percentage of the machine output.
    /// </summary>
    double Quality { get; }

    /// <summary>
    /// Gets the number of pieces rejected by the machine.
    /// </summary>
    double RejectedPieces { get; }

    /// <summary>
    /// Gets the running time of the machine (must be non-negative).
    /// </summary>
    double RunningTime
    {
        get;

        // non-negative
    }

    /// <summary>
    /// Gets the stopping time of the machine (must be non-negative).
    /// </summary>
    double StoppingTime
    {
        get;

        // non-negative
    }

    /// <summary>
    /// Sets the initial condition of the machine with production data.
    /// </summary>
    /// <param name="producedPieces">The number of pieces produced.</param>
    /// <param name="rejectedPieces">The number of pieces rejected.</param>
    /// <param name="runningTime">The running time in minutes.</param>
    /// <param name="stoppingTime">The stopping time in minutes.</param>
    void SetInitialCondition(int producedPieces, int rejectedPieces, double runningTime, double stoppingTime);

    /// <summary>
    /// Returns a string representation of the machine OEE data.
    /// </summary>
    /// <returns>A string representation of the machine OEE data.</returns>
    string ToString();

    /// <summary>
    /// Updates the machine information with new production data.
    /// </summary>
    /// <param name="producedPieces">The number of pieces produced.</param>
    /// <param name="rejectedPieces">The number of pieces rejected.</param>
    /// <param name="runningTime">The running time in minutes.</param>
    /// <param name="stoppingTime">The stopping time in minutes.</param>
    void UpdateInfo(int producedPieces, int rejectedPieces, double runningTime, double stoppingTime);
}

