// <copyright file="AdvancedTrapezoidFunction.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.UI.Models.Mahts;

/// <summary>
/// Represents an advanced trapezoidal function with sinusoidal noise and logistic transitions.
/// </summary>
public class AdvancedTrapezoidFunction(double startRise, double endRise, double startFall, double endFall, double peakValue, double frequency, double amplitude)
{
    private readonly double startRise = startRise;
    private readonly double endRise = endRise;
    private readonly double startFall = startFall;
    private readonly double endFall = endFall;
    private readonly double peakValue = peakValue;
    private readonly double frequency = frequency;
    private readonly double amplitude = amplitude;

    private double LogisticRise(double x)
    {
        double midPoint = (this.startRise + this.endRise) / 2;
        double growthRate = 10 / (this.endRise - this.startRise);
        return this.peakValue / (1 + Math.Exp(-growthRate * (x - midPoint)));
    }

    private double LogisticFall(double x)
    {
        double midPoint = (this.startFall + this.endFall) / 2;
        double decayRate = 10 / (this.endFall - this.startFall);
        return this.peakValue * (1 - (1 / (1 + Math.Exp(-decayRate * (x - midPoint)))));
    }

    private double SinusoidalNoise(double x)
    {
        return this.amplitude * Math.Sin(2 * Math.PI * this.frequency * x);
    }

    /// <summary>
    /// Evaluates the trapezoidal function at the specified x value.
    /// </summary>
    /// <param name="x">The x-coordinate at which to evaluate the function.</param>
    /// <returns>The function value at the specified x-coordinate.</returns>
    public double Evaluate(double x)
    {
        if (x < this.startRise)
        {
            return 0;
        }
        else if (x < this.endRise)
        {
            return this.LogisticRise(x);
        }
        else if (x < this.startFall)
        {
            // Adding sinusoidal variations during the stable phase
            return this.peakValue + this.SinusoidalNoise(x);
        }
        else if (x < this.endFall)
        {
            return this.LogisticFall(x);
        }
        else
        {
            return 0;
        }
    }
}

