// <copyright file="AssemblyInfo.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Runtime.CompilerServices;

// Expose the simulated controller's timing seams (heartbeat scheduler, settle delay) to its tests only.
[assembly: InternalsVisibleTo("IndTrace.Devices.Tests")]
