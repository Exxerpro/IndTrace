// <copyright file="StatisticalExtensions.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.UI.Models.Performance;

/// <summary>
/// Provides extension methods for statistical calculations on double arrays.
/// Convention: every method returns 0 for an empty sample instead of throwing or producing NaN,
/// and skewness/kurtosis return 0 for constant samples (zero standard deviation).
/// </summary>
public static class StatisticalExtensions
{
    /// <summary>
    /// Computes the arithmetic mean of the sample values.
    /// </summary>
    /// <param name="samples">The array of sample values.</param>
    /// <returns>The arithmetic mean of the samples, or 0 if the sample is empty.</returns>
    public static double ComputeMean(this double[] samples)
    {
        if (samples.Length == 0)
        {
            return 0;
        }

        return samples.Average();
    }

    /// <summary>
    /// Computes the standard deviation of the sample values.
    /// </summary>
    /// <param name="samples">The array of sample values.</param>
    /// <returns>The standard deviation of the samples, or 0 if the sample is empty.</returns>
    public static double ComputeStandardDeviation(this double[] samples)
    {
        if (samples.Length == 0)
        {
            return 0;
        }

        var mean = samples.ComputeMean();
        return Math.Sqrt(samples.Sum(x => Math.Pow(x - mean, 2)) / samples.Length);
    }

    /// <summary>
    /// Computes the standard deviation of the sample values using a pre-calculated mean.
    /// </summary>
    /// <param name="samples">The array of sample values.</param>
    /// <param name="mean">The pre-calculated mean value.</param>
    /// <returns>The standard deviation of the samples, or 0 if the sample is empty.</returns>
    public static double ComputeStandardDeviation(this double[] samples, double mean)
    {
        if (samples.Length == 0)
        {
            return 0;
        }

        return Math.Sqrt(samples.Sum(x => Math.Pow(x - mean, 2)) / samples.Length);
    }

    /// <summary>
    /// Computes the mode (most frequently occurring value) of the sample values.
    /// </summary>
    /// <param name="samples">The array of sample values.</param>
    /// <returns>The mode of the samples, or 0 if the sample is empty.</returns>
    public static double ComputeMode(this double[] samples)
    {
        if (samples.Length == 0)
        {
            return 0;
        }

        return samples.GroupBy(v => v)
            .OrderByDescending(g => g.Count())
            .First().Key;
    }

    /// <summary>
    /// Computes the skewness of the sample values using pre-calculated mean and standard deviation.
    /// </summary>
    /// <param name="samples">The array of sample values.</param>
    /// <param name="mean">The pre-calculated mean value.</param>
    /// <param name="stdDev">The pre-calculated standard deviation.</param>
    /// <returns>The skewness of the samples, or 0 if the sample is empty or the standard deviation is 0 (constant sample).</returns>
    public static double ComputeSkewness(this double[] samples, double mean, double stdDev)
    {
        if (samples.Length == 0 || stdDev == 0)
        {
            return 0;
        }

        return samples.Sum(x => Math.Pow((x - mean) / stdDev, 3)) / samples.Length;
    }

    /// <summary>
    /// Computes the skewness of the sample values.
    /// </summary>
    /// <param name="samples">The array of sample values.</param>
    /// <returns>The skewness of the samples, or 0 if the sample is empty or constant (zero standard deviation).</returns>
    public static double ComputeSkewness(this double[] samples)
    {
        if (samples.Length == 0)
        {
            return 0;
        }

        var mean = samples.ComputeMean();
        var stdDev = samples.ComputeStandardDeviation();
        if (stdDev == 0)
        {
            return 0;
        }

        return samples.Sum(x => Math.Pow((x - mean) / stdDev, 3)) / samples.Length;
    }

    /// <summary>
    /// Computes the excess kurtosis of the sample values using pre-calculated mean and standard deviation.
    /// </summary>
    /// <param name="samples">The array of sample values.</param>
    /// <param name="mean">The pre-calculated mean value.</param>
    /// <param name="stdDev">The pre-calculated standard deviation.</param>
    /// <returns>The excess kurtosis of the samples, or 0 if the sample is empty or the standard deviation is 0 (constant sample).</returns>
    public static double ComputeKurtosis(this double[] samples, double mean, double stdDev)
    {
        if (samples.Length == 0 || stdDev == 0)
        {
            return 0;
        }

        var kurtosis = samples.Sum(x => Math.Pow((x - mean) / stdDev, 4)) / samples.Length;
        return kurtosis - 3; // Excess kurtosis
    }

    /// <summary>
    /// Computes the excess kurtosis of the sample values.
    /// </summary>
    /// <param name="samples">The array of sample values.</param>
    /// <returns>The excess kurtosis of the samples, or 0 if the sample is empty or constant (zero standard deviation).</returns>
    public static double ComputeKurtosis(this double[] samples)
    {
        if (samples.Length == 0)
        {
            return 0;
        }

        var mean = samples.ComputeMean();
        var stdDev = samples.ComputeStandardDeviation();
        if (stdDev == 0)
        {
            return 0;
        }

        var kurtosis = samples.Sum(x => Math.Pow((x - mean) / stdDev, 4)) / samples.Length;
        return kurtosis - 3; // Excess kurtosis
    }
}
