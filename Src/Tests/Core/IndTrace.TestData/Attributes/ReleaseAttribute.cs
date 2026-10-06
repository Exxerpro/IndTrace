// <copyright file="ReleaseAttribute.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System;

namespace IndTrace.TestData.Attributes;

/// <summary>
/// Marks classes or methods as release/staging data for infrastructure testing.
/// Used to distinguish between production test data and developmental examples.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class ReleaseAttribute : Attribute
{
    public ReleaseAttribute() { }

    public ReleaseAttribute(string description)
    {
        Description = description;
    }

    public string? Description { get; }
}