// <copyright file="DevicesServiceCollectionExtensions.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Devices;

using IndTrace.Devices.Plc;
using IndTrace.Devices.Scanning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
/// Registers the community device defaults.
/// </summary>
public static class DevicesServiceCollectionExtensions
{
    /// <summary>
    /// Registers the community defaults — the simulation-only <see cref="IPlcControllerFactory"/> and the
    /// <see cref="NullBarCodeReader"/> — for every device contract no driver has registered yet. Call it AFTER
    /// the driver registrations: <c>TryAdd</c> never overrides a driver.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddCommunityDeviceDefaults(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<IPlcControllerFactory, SimulatedPlcControllerFactory>();
        services.TryAddSingleton<IBarCodeReader, NullBarCodeReader>();
        return services;
    }
}
