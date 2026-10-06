// <copyright file="MeasureExecutionTimeAttribute.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Dependencies.Interceptors;

/// <summary>
/// Attribute to measure the execution time of a method for diagnostics or logging purposes.
/// </summary>
/// <remarks>
/// Can be applied to methods to enable timing via an AOP interceptor or custom logic.
/// </remarks>
/// <summary>
/// Represents the MeasureExecutionTimeAttribute.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class MeasureExecutionTimeAttribute : Attribute
{
    /// <summary>
    /// Gets the label associated with the timing measurement, if any.
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MeasureExecutionTimeAttribute"/> class.
    /// </summary>
    /// <param name="label">An optional label to identify the timing measurement.</param>
    public MeasureExecutionTimeAttribute(string? label = null)
    {
        this.Label = label ?? string.Empty;
    }
}