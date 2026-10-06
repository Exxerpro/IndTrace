// <copyright file="SimulationSettings.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Dependencies.Simulations;

/// <summary>
/// Represents the settings for the PLC simulation.
/// </summary>
public class SimulationSettings : ISimulationSettings
{
    /// <summary>
    /// Gets or sets the time step for the simulation in seconds.
    /// </summary>
    public int TimeStep { get; set; }
    /// <summary>
    /// Gets or sets the speed of the simulation (seconds/seconds).
    /// </summary>
    public double SpeedSimulation { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the simulation is enabled.
    /// </summary>
    public bool EnableSimulation { get; set; }
    /// <summary>
    /// Gets or sets the integer representation of the application flag.
    /// </summary>
    public int ApplicationFlagInt { get; set; }
    /// <summary>
    /// Gets or sets the integer representation of the status fault reason.
    /// </summary>
    public int StatusFaultReasonInt { get; set; }
    /// <summary>
    /// Gets or sets the event counter.
    /// </summary>
    public int EventCounter { get; set; }
    /// <summary>
    /// Gets or sets the integer representation of the status fault reject.
    /// </summary>
    public int StatusFaultRejectInt { get; set; }

    /// <summary>
    /// Gets or sets the total production.
    /// </summary>
    public float TotalProduction { get; set; }
    /// <summary>
    /// Gets or sets the production of good items.
    /// </summary>
    public float ProductionOk { get; set; }
    /// <summary>
    /// Gets or sets the production of non-good items.
    /// </summary>
    public float ProductionNoK { get; set; }
    /// <summary>
    /// Gets or sets the running time.
    /// </summary>
    public TimeSpan RunningTime { get; set; }
    /// <summary>
    /// Gets or sets the stopped time.
    /// </summary>
    public TimeSpan StoppedTime { get; set; }
    /// <summary>
    /// Gets or sets the faulted time.
    /// </summary>
    public TimeSpan FaultedTime { get; set; }
    /// <summary>
    /// Gets or sets the current time.
    /// </summary>
    public DateTime CurrentTime { get; set; }
    /// <summary>
    /// Gets or sets the start time.
    /// </summary>
    public DateTime StartTime { get; set; }
    /// <summary>
    /// Gets or sets the end time.
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Gets or sets the current time in seconds since midnight.
    /// </summary>
    public int CurrentTimeInt { get; set; }
    /// <summary>
    /// Gets or sets the running time in seconds.
    /// </summary>
    public int RunningTimeInt { get; set; }
    /// <summary>
    /// Gets or sets the stopped time in seconds.
    /// </summary>
    public int StoppedTimeInt { get; set; }
    /// <summary>
    /// Gets or sets the faulted time in seconds.
    /// </summary>
    public int FaultedTimeInt { get; set; }

    /// <summary>
    /// Gets or sets the start time in seconds.
    /// </summary>
    public int StartTimeInt { get; set; }
    /// <summary>
    /// Gets or sets the end time in seconds.
    /// </summary>
    public int EndTimeInt { get; set; }

    /// <summary>
    /// Gets or sets the application flag as a string.
    /// </summary>
    public string ApplicationFlag { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the status fault reason as a string.
    /// </summary>
    public string StatusFaultReason { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the status fault reject as a string.
    /// </summary>
    public string StatusFaultReject { get; set; } = string.Empty;

    private static readonly Dictionary<string, int> FlagMap = new()
    {
        ["Prod"] = 1,
        ["Test"] = 2,
    };

    private static readonly Dictionary<string, int> ReasonMap = new()
    {
        ["Overload"] = 101,
        ["Overheat"] = 102,
        ["Unknown"] = 0,
    };

    /// <summary>
    /// Converts raw simulation settings to a processed <see cref="SimulationSettings"/> instance.
    /// </summary>
    /// <param name="raw">The raw simulation settings to convert.</param>
    /// <returns>A new <see cref="SimulationSettings"/> instance with processed values.</returns>
    public static SimulationSettings ConvertFrom(SimulationSettings raw)
    {
        return new SimulationSettings
        {
            TimeStep = raw.TimeStep,
            SpeedSimulation = raw.SpeedSimulation,
            EnableSimulation = raw.EnableSimulation,
            EventCounter = raw.EventCounter,
            TotalProduction = raw.TotalProduction,
            ProductionOk = raw.ProductionOk,
            ProductionNoK = raw.ProductionNoK,
            StatusFaultReasonInt = ReasonMap.GetValueOrDefault(raw.StatusFaultReason, 0),
            StatusFaultRejectInt = ReasonMap.GetValueOrDefault(raw.StatusFaultReject, 0),
            ApplicationFlagInt = FlagMap.GetValueOrDefault(raw.ApplicationFlag, 0),
            CurrentTimeInt = (int)raw.CurrentTime.TimeOfDay.TotalSeconds,
            StartTimeInt = (int)raw.StartTime.TimeOfDay.TotalSeconds,
            EndTimeInt = (int)raw.EndTime.TimeOfDay.TotalSeconds,
            RunningTimeInt = (int)raw.RunningTime.TotalSeconds,
            StoppedTimeInt = (int)raw.StoppedTime.TotalSeconds,
            FaultedTimeInt = (int)raw.FaultedTime.TotalSeconds,
        };
    }
}