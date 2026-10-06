// <copyright file="SimulationSnapshot.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Dependencies.Simulations;
/// <summary>
/// Represents the SimulationSnapshot.
/// </summary>

public class SimulationSnapshot : ISimulationSnapshot, IEquatable<SimulationSnapshot>, IComparable<SimulationSnapshot>
{
    public int SimulatedTime;
    /// <summary>
    /// Gets or sets the TotalProduction.
    /// </summary>
    public float TotalProduction { get; set; }
    /// <summary>
    /// Gets or sets the ProductionOk.
    /// </summary>
    public float ProductionOk { get; set; }
    /// <summary>
    /// Gets or sets the ProductionNoK.
    /// </summary>
    public float ProductionNoK { get; set; }
    /// <summary>
    /// Gets or sets the EventCounter.
    /// </summary>
    public int EventCounter { get; set; }
    /// <summary>
    /// Gets or sets the RunningTime.
    /// </summary>
    public int RunningTime { get; set; }
    /// <summary>
    /// Gets or sets the StoppedTime.
    /// </summary>
    public int StoppedTime { get; set; }
    /// <summary>
    /// Gets or sets the FaultedTime.
    /// </summary>
    public int FaultedTime { get; set; }
    /// <summary>
    /// Gets or sets the TimeStep.
    /// </summary>
    public int TimeStep { get; set; }
    /// <summary>
    /// Gets or sets the CurrentTime.
    /// </summary>
    public int CurrentTime { get; set; }

    // Single source of truth: the interface's float SimulatedTime projects the one backing store
    // (the public int SimulatedTime field) instead of holding a second, independently-mutable copy
    // that the class's own Equals/CompareTo never saw.
    float ISimulationSnapshot.SimulatedTime
    {
        get => this.SimulatedTime;
        set => this.SimulatedTime = (int)value;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="SimulationSnapshot"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="settings">The settings.</param>

    public SimulationSnapshot(ISimulationSettings settings)
    {
        this.TotalProduction = settings.TotalProduction;
        this.ProductionOk = settings.ProductionOk;
        this.ProductionNoK = settings.ProductionNoK;
        this.EventCounter = settings.EventCounter;
        this.RunningTime = settings.RunningTimeInt;
        this.StoppedTime = settings.StoppedTimeInt;
        this.FaultedTime = settings.FaultedTimeInt;
        this.TimeStep = settings.TimeStep;
        this.CurrentTime = settings.CurrentTimeInt;
        this.RunningTime = settings.RunningTimeInt;
        this.StoppedTime = settings.StoppedTimeInt;
        this.FaultedTime = settings.FaultedTimeInt;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="SimulationSnapshot"/> class.
    /// Initializes a new instance of the class.
    /// </summary>

    public SimulationSnapshot()
    { }

    public static bool operator ==(SimulationSnapshot? left, SimulationSnapshot? right) => Equals(left, right);

    public static bool operator !=(SimulationSnapshot? left, SimulationSnapshot? right) => !Equals(left, right);

    public static bool operator >(SimulationSnapshot? left, SimulationSnapshot? right) =>
        left is not null && left.CompareTo(right) > 0;

    public static bool operator <(SimulationSnapshot? left, SimulationSnapshot? right) =>
        left is null ? right is not null : left.CompareTo(right) < 0;

    private int CompareTo(SimulationSnapshot? right)
    {
        //inherit from ISimulationSnapshot
        if (right is null) return 1; // null is less than any instance
        int cmp;
        if ((cmp = this.SimulatedTime.CompareTo(right.SimulatedTime)) != 0) return cmp;
        if ((cmp = this.TotalProduction.CompareTo(right.TotalProduction)) != 0) return cmp;
        if ((cmp = this.ProductionOk.CompareTo(right.ProductionOk)) != 0) return cmp;
        if ((cmp = this.ProductionNoK.CompareTo(right.ProductionNoK)) != 0) return cmp;
        if ((cmp = this.RunningTime.CompareTo(right.RunningTime)) != 0) return cmp;
        if ((cmp = this.StoppedTime.CompareTo(right.StoppedTime)) != 0) return cmp;
        if ((cmp = this.FaultedTime.CompareTo(right.FaultedTime)) != 0) return cmp;
        if ((cmp = this.CurrentTime.CompareTo(right.CurrentTime)) != 0) return cmp;
        return 0; // equal
    }
    /// <summary>
    /// Executes Equals operation.
    /// </summary>
    /// <param name="other">The other.</param>
    /// <returns>The result of Equals.</returns>

    public bool Equals(ISimulationSnapshot? other)
    //inherit from ISimulationSnapshot
    {
        if (other is null) return false;
        return this.SimulatedTime == other.SimulatedTime &&
               this.TotalProduction == other.TotalProduction &&
               this.ProductionOk == other.ProductionOk &&
               this.ProductionNoK == other.ProductionNoK &&
               this.RunningTime == other.RunningTime &&
               this.StoppedTime == other.StoppedTime &&
               this.FaultedTime == other.FaultedTime &&
               this.CurrentTime == other.CurrentTime;
    }
    /// <summary>
    /// Executes CompareTo operation.
    /// </summary>
    /// <param name="other">The other.</param>
    /// <returns>The result of CompareTo.</returns>

    public int CompareTo(ISimulationSnapshot? other)
    {
        //inherit from ISimulationSnapshot
        if (other is null) return 1; // null is less than any instance

        int cmp;
        if ((cmp = this.SimulatedTime.CompareTo(other.SimulatedTime)) != 0) return cmp;
        if ((cmp = this.TotalProduction.CompareTo(other.TotalProduction)) != 0) return cmp;
        if ((cmp = this.ProductionOk.CompareTo(other.ProductionOk)) != 0) return cmp;
        if ((cmp = this.ProductionNoK.CompareTo(other.ProductionNoK)) != 0) return cmp;
        if ((cmp = this.RunningTime.CompareTo(other.RunningTime)) != 0) return cmp;
        if ((cmp = this.StoppedTime.CompareTo(other.StoppedTime)) != 0) return cmp;
        if ((cmp = this.FaultedTime.CompareTo(other.FaultedTime)) != 0) return cmp;
        if ((cmp = this.CurrentTime.CompareTo(other.CurrentTime)) != 0) return cmp;
        return 0; // equal
    }

    // Override GetHashCode to ensure it is consistent with Equals
    /// <summary>
    /// Executes GetHashCode operation.
    /// </summary>
    /// <returns>The result of GetHashCode.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(
            this.SimulatedTime, this.TotalProduction, this.ProductionOk, this.ProductionNoK,
            this.RunningTime, this.StoppedTime, this.FaultedTime, this.CurrentTime
        );
    }

    // Override Equals to ensure it is consistent with GetHashCode
    /// <summary>
    /// Executes Equals operation.
    /// </summary>
    /// <param name="obj">The obj.</param>
    /// <returns>The result of Equals.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is SimulationSnapshot other)
        {
            return this.Equals(other);
        }
        return false;
    }
    /// <summary>
    /// Executes Equals operation.
    /// </summary>
    /// <param name="other">The other.</param>
    /// <returns>The result of Equals.</returns>

    public bool Equals(SimulationSnapshot? other)
    {
        if (other is null) return false;

        return this.SimulatedTime == other.SimulatedTime &&
               this.TotalProduction == other.TotalProduction &&
               this.ProductionOk == other.ProductionOk &&
               this.ProductionNoK == other.ProductionNoK &&
               this.RunningTime == other.RunningTime &&
               this.StoppedTime == other.StoppedTime &&
               this.FaultedTime == other.FaultedTime &&
               this.CurrentTime == other.CurrentTime;
    }

    int IComparable<SimulationSnapshot>.CompareTo(SimulationSnapshot? other)
    {
        if (other is null) return 1; // null is less than any instance
        int cmp;
        if ((cmp = this.SimulatedTime.CompareTo(other.SimulatedTime)) != 0) return cmp;
        if ((cmp = this.TotalProduction.CompareTo(other.TotalProduction)) != 0) return cmp;
        if ((cmp = this.ProductionOk.CompareTo(other.ProductionOk)) != 0) return cmp;
        if ((cmp = this.ProductionNoK.CompareTo(other.ProductionNoK)) != 0) return cmp;
        if ((cmp = this.RunningTime.CompareTo(other.RunningTime)) != 0) return cmp;
        if ((cmp = this.StoppedTime.CompareTo(other.StoppedTime)) != 0) return cmp;
        if ((cmp = this.FaultedTime.CompareTo(other.FaultedTime)) != 0) return cmp;
        if ((cmp = this.CurrentTime.CompareTo(other.CurrentTime)) != 0) return cmp;
        return 0; // equal
    }

    //Add methods to compare against itself and other instances

}