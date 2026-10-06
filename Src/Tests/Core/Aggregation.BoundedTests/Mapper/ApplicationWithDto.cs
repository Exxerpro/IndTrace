// <copyright file="ApplicationWithDto.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.Mapper;
/// <summary>
/// Represents the ApplicationWithDto.
/// </summary>

public record ApplicationWithDto(Assembly[] Assemblies, List<ScannedClass> MappingData);