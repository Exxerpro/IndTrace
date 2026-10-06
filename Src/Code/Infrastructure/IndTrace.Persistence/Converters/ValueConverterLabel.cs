// <copyright file="ValueConverterLabel.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace IndTrace.Persistence.Converters;

/// <summary>
/// Provides a value converter for the Label entity to and from string for Entity Framework.
/// </summary>
public class ValueConverterLabel(ConverterMappingHints? mappingHints = null) : ValueConverter<Label, string>(
    label => label.ToString(),
    value => new Label(value),
    mappingHints)
{
}