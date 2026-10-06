// <copyright file="RandomExtensions.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.UI.Models.Performance;

/// <summary>
/// Provides extension methods for Random class to generate values with statistical distributions.
/// </summary>
public static class RandomExtensions
{
    /// <summary>
    /// Generates a random decimal value between specified minimum and maximum values.
    /// </summary>
    /// <param name="random">The random number generator.</param>
    /// <param name="minValue">The minimum value (inclusive).</param>
    /// <param name="maxValue">The maximum value (inclusive).</param>
    /// <returns>A random decimal value within the specified range.</returns>
    public static double NextDecimal(this Random random, int minValue, int maxValue)
    {
        return random.Next(minValue, maxValue + 1);
    }

    /// <summary>
    /// Generates a sample with tendency towards higher values using Beta distribution.
    /// </summary>
    /// <param name="random">The random number generator.</param>
    /// <returns>A value between 0 and 1 with right-skewed distribution.</returns>
    public static double SampleWithTendencyToTheRight(this Random random)
    {
        var alpha = 75.0;
        var beta = 5.0;
        return random.BetaSample(alpha, beta);
    }

    /// <summary>
    /// Generates a sample with tendency towards lower values using Beta distribution.
    /// </summary>
    /// <param name="random">The random number generator.</param>
    /// <returns>A value between 0 and 1 with left-skewed distribution.</returns>
    public static double SampleWithTendencyToTheLeft(this Random random)
    {
        var alpha = 5.0;
        var beta = 75.0;
        return random.BetaSample(alpha, beta);
    }

    private static double BetaSample(this Random random, double alpha, double beta)
    {
        var sampleAlpha = random.GammaSample(alpha, 1.0);
        var sampleBeta = random.GammaSample(beta, 1.0);
        return sampleAlpha / (sampleAlpha + sampleBeta);
    }

    private static double GammaSample(this Random random, double shape, double scale)
    {
        var d = shape - (1.0 / 3.0);
        var c = 1.0 / Math.Sqrt(9.0 * d);
        double v;

        do
        {
            double x, u;
            do
            {
                x = random.NormalSample();
                v = 1.0 + (c * x);
            }
            while (v <= 0);

            v = v * v * v;
            u = random.NextDouble();

            if (u < 1.0 - (0.0331 * x * x * x * x) || Math.Log(u) < (0.5 * x * x) + (d * (1.0 - v + Math.Log(v))))
            {
                break;
            }
        }
        while (true);

        return d * v * scale;
    }

    private static double NormalSample(this Random random)
    {
        var u1 = random.NextDouble();
        var u2 = random.NextDouble();
        return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
    }
}