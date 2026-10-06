// <copyright file="OeeState.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.UI.Models.Performance;

using IndTrace.Domain.Interfaces;

/// <summary>
/// Represents the overall OEE state containing machine performance data and state management.
/// </summary>
public class OeeState
{
    private readonly Random random = new();
    private readonly IDateTimeMachine dateTimeMachine;
    private DateTime startTime;

    /// <summary>
    /// Event fired when the OEE state changes.
    /// </summary>
    public event Action? OnChange;

    /// <summary>
    /// Gets the dictionary of machines and their performance records.
    /// </summary>
    public Dictionary<string, PerformanceRecord> Machines { get; private set; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="OeeState"/> class.
    /// </summary>
    /// <param name="dateTimeMachine">The date time machine for time operations.</param>
    public OeeState(IDateTimeMachine dateTimeMachine)
    {
        this.dateTimeMachine = dateTimeMachine;
        this.startTime = this.dateTimeMachine.Now.ToLocalTime().AddHours(-8);

        // Initialize machines with default data
        this.InitializeMachines();

        // Generate initial data points for the past 8 hours
    }

    private void InitializeMachines()
    {
        // Add initial machines to HistoricalData with empty lists
        // Add initial machines to HistoricalData with empty lists
        this.Machines["Power Puncher"] = new PerformanceRecord("Power Puncher", "High impact punching machine", 30);
        this.Machines["Press Power"] = new PerformanceRecord("Press Power", "High power pressing machine", 200);
        this.Machines["Cross Cutter"] = new PerformanceRecord("Cross Cutter", "Precision cutting machine", 100);
        this.Machines["Crosswise Cutter"] = new PerformanceRecord("Crosswise Cutter", "Crosswise cutting machine", 300);
        this.Machines["Press Titan"] = new PerformanceRecord("Press Titan", "Titanium press machine", 100);
        this.Machines["Laser Cutter"] = new PerformanceRecord("Laser Cutter", "Laser precision cutter", 150);
        this.Machines["Hydraulic Press"] = new PerformanceRecord("Hydraulic Press", "Hydraulic pressing machine", 250);
        this.Machines["Automatic Feeder"] = new PerformanceRecord("Automatic Feeder", "Automated feeding machine", 120);
        this.Machines["Conveyor Belt"] = new PerformanceRecord("Conveyor Belt", "Automated conveyor belt", 90);
        this.Machines["Packaging Robot"] = new PerformanceRecord("Packaging Robot", "Automated packaging robot", 180);

        this.GeneratePastData();
    }

    /// <summary>
    /// Generates new data for all machines in the current state.
    /// </summary>
    public void GenerateNewData()
    {
        foreach (var machine in this.Machines.Values)
        {
            var productionData = machine.GenerateNewData(this.random, this.startTime, this.dateTimeMachine.Now.ToLocalTime());
            machine.UpdateData(productionData);
        }

        // Notify subscribers that data has changed
        this.OnChange?.Invoke();
    }

    /// <summary>
    /// Generates historical data for all machines from the start time to current time.
    /// </summary>
    public void GeneratePastData()
    {
        foreach (var machine in this.Machines.Values)
        {
            machine.GeneratePastData(this.random, this.startTime, this.dateTimeMachine.Now.ToLocalTime());
        }

        // Notify subscribers after generating historical data
        this.OnChange?.Invoke();
    }
}
