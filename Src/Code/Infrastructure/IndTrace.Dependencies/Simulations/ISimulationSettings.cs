// <copyright file="ISimulationSettings.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Dependencies.Simulations;

public interface ISimulationSettings
{
    int TimeStep { get; set; } // in seconds by step

    double SpeedSimulation { get; set; } // in seconds/seconds

    bool EnableSimulation { get; set; }

    int ApplicationFlagInt { get; set; }

    int StatusFaultReasonInt { get; set; }

    int EventCounter { get; set; }

    int StatusFaultRejectInt { get; set; }

    float TotalProduction { get; set; }

    float ProductionOk { get; set; }

    float ProductionNoK { get; set; }

    TimeSpan RunningTime { get; set; }

    TimeSpan StoppedTime { get; set; }

    TimeSpan FaultedTime { get; set; }

    DateTime CurrentTime { get; set; }

    DateTime StartTime { get; set; }

    DateTime EndTime { get; set; }

    int CurrentTimeInt { get; set; } // seconds since midnight

    int RunningTimeInt { get; set; } // in seconds

    int StoppedTimeInt { get; set; } // in seconds

    int FaultedTimeInt { get; set; } // in seconds

    int StartTimeInt { get; set; } // in seconds

    int EndTimeInt { get; set; } // in seconds

    /// <summary>
    /// Set by EF or by builder on runtime, consumer must check for null before accessing.
    /// </summary>
    string ApplicationFlag { get; set; }

    /// <summary>
    /// Set by EF or by builder on runtime, consumer must check for null before accessing.
    /// </summary>
    string StatusFaultReason { get; set; }

    /// <summary>
    /// Set by EF or by builder on runtime, consumer must check for null before accessing.
    /// </summary>
    string StatusFaultReject { get; set; }
}