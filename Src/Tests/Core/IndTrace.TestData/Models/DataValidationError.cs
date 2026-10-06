// <copyright file="DataValidationError.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.TestData.Models;

/// <summary>
/// Represents an error that occurred during data validation.
/// </summary>
internal sealed class DataValidationError
{
    public required string FileName { get; set; }
    public required string EntityType { get; set; }
    public required string ErrorMessage { get; set; }
}
