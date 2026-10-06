// <copyright file="TestPriorityAttribute.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.CommonTests;
/// <summary>
/// Represents the TestPriorityAttribute.
/// </summary>

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class TestPriorityAttribute(int priority) : Attribute
{
    /// <summary>
    /// Gets or sets the Priority.
    /// </summary>
    public int Priority { get; } = priority;
}