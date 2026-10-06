// <copyright file="AssemblyInfo.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Runtime.CompilerServices;

// Expose internals to the Gateway test project so the P0-7 worker-fault observation seam
// (GatewayWorkerManager.ObserveWorkerTask) is testable. Declared explicitly in source to match the
// project-wide convention (GenerateAssemblyInfo is disabled, so the MSBuild InternalsVisibleTo item is a no-op).
[assembly: InternalsVisibleTo("GateWay.Tests")]
