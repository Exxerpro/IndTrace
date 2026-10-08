// <copyright file="IPlcControllerFactory.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Devices.Plc;

/// <summary>
/// Creates the <see cref="IIndTraceControllerRx"/> a gateway worker tracks a PLC through. This is the seam a
/// PLC driver plugs into: the gateway never constructs a vendor controller itself. Exactly one factory is
/// registered per host; the community edition registers <see cref="SimulatedPlcControllerFactory"/>, a
/// driver edition registers its own (e.g. the Siemens S7 driver).
/// </summary>
public interface IPlcControllerFactory
{
    /// <summary>
    /// Creates an unconfigured controller for <paramref name="plc"/>. The caller owns the result and runs the
    /// setup/validate/connect sequence on it.
    /// </summary>
    /// <param name="plc">The PLC configuration the controller is built from.</param>
    /// <param name="logger">The logger the controller reports through.</param>
    /// <param name="dateTimeMachine">The deterministic time source.</param>
    /// <returns>The created controller, or a failure explaining why this factory cannot serve the PLC.</returns>
    Result<IIndTraceControllerRx> Create(PlcDto plc, ILogger logger, IDateTimeMachine dateTimeMachine);

    /// <summary>
    /// Classifies an exception escaping a controller created by this factory. <see langword="true"/> means the
    /// controller could not be REACHED (a transient condition the configure-retry loop recovers from), as
    /// opposed to a configuration or tag error. Vendor exception types stay inside the driver.
    /// </summary>
    /// <param name="exception">The exception raised by a controller operation.</param>
    /// <returns><see langword="true"/> when the exception signals an unreachable controller.</returns>
    bool IsControllerUnreachable(Exception exception);
}
