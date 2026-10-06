// <copyright file="SuccessorMachineMetadata.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Enum;

namespace IndTrace.Domain.Routing;

/// <summary>
/// #60: the per-successor machine facts the one-hop disabled-Process cascade needs, supplied by the caller as
/// plain domain values so the pure-domain routing layer (<see cref="ProductRoutingState"/> /
/// <see cref="RoutingAdvancePolicy"/>) stays free of EF/repository. It mirrors
/// <see cref="RoutingAdvancePolicy.ApplyDisabledCascade"/>'s caller-supplied contract: the (machine type,
/// enablement) pair the cascade reads for a candidate next machine, resolved by the application layer and
/// passed in as immutable values.
/// </summary>
/// <param name="MachineType">The successor machine's type (the cascade fires only for <see cref="MachineType.Process"/>).</param>
/// <param name="IsEnabled">Whether the successor machine is enabled (a DISABLED Process successor is the one that cascades).</param>
public readonly record struct SuccessorMachineMetadata(MachineType MachineType, bool IsEnabled);
