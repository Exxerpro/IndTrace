// <copyright file="ISimulationSnapshot.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Dependencies.Simulations;

public interface ISimulationSnapshot : IEquatable<ISimulationSnapshot>, IComparable<ISimulationSnapshot>
{
    float TotalProduction { get; set; }

    float ProductionOk { get; set; }

    float ProductionNoK { get; set; }

    int EventCounter { get; set; }

    int RunningTime { get; set; }

    int StoppedTime { get; set; }

    int FaultedTime { get; set; }

    int TimeStep { get; set; }

    int CurrentTime { get; set; }

    float SimulatedTime { get; set; }

    public int GetHashCode() => HashCode.Combine(
        this.SimulatedTime, this.TotalProduction, this.ProductionOk, this.ProductionNoK,
        this.RunningTime, this.StoppedTime, this.FaultedTime, this.CurrentTime
    );

    public new bool Equals(ISimulationSnapshot? other)
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

    public new int CompareTo(ISimulationSnapshot? other)
    {
        if (other == null) return 1;
        int cmp;
        if ((cmp = this.SimulatedTime.CompareTo(other.SimulatedTime)) != 0) return cmp;
        if ((cmp = this.TotalProduction.CompareTo(other.TotalProduction)) != 0) return cmp;
        if ((cmp = this.ProductionOk.CompareTo(other.ProductionOk)) != 0) return cmp;
        if ((cmp = this.ProductionNoK.CompareTo(other.ProductionNoK)) != 0) return cmp;
        if ((cmp = this.RunningTime.CompareTo(other.RunningTime)) != 0) return cmp;
        if ((cmp = this.StoppedTime.CompareTo(other.StoppedTime)) != 0) return cmp;
        if ((cmp = this.FaultedTime.CompareTo(other.FaultedTime)) != 0) return cmp;
        if ((cmp = this.CurrentTime.CompareTo(other.CurrentTime)) != 0) return cmp;
        return 0;
    }
}