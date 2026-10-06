// <copyright file="DuplicateEntry.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.TestData.Models;

/// <summary>
/// Represents a duplicate entry found during validation.
/// </summary>
internal sealed class DuplicateEntry
{
    public required string FileName { get; set; }
    public required string EntityType { get; set; }
    public required string EntityId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public int OriginalLineNumber { get; set; }
}

//[Fix] CLAUDE - Date: 26/08/2025
//Reason: [CS8632] - Removed nullable annotation (string?) from EntityName to eliminate nullable reference type warnings since project has <Nullable>disable</Nullable>
