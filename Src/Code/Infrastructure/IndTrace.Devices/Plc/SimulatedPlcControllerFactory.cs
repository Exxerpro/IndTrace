// <copyright file="SimulatedPlcControllerFactory.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Devices.Plc;

/// <summary>
/// The community-edition <see cref="IPlcControllerFactory"/>: it serves only simulation-enabled PLCs, with an
/// in-memory <see cref="SimulatedControllerRx"/>. A PLC that is NOT simulation-enabled is refused loudly — no
/// PLC driver is installed, and a tracking system must never pretend to talk to real equipment.
/// </summary>
public sealed class SimulatedPlcControllerFactory : IPlcControllerFactory
{
    /// <inheritdoc/>
    public Result<IIndTraceControllerRx> Create(PlcDto plc, ILogger logger, IDateTimeMachine dateTimeMachine)
    {
        if (plc is null || logger is null || dateTimeMachine is null)
        {
            return Result<IIndTraceControllerRx>.WithFailure("PLC configuration, logger and time source are required to create a controller.");
        }

        if (!plc.EnableSimulation)
        {
            return Result<IIndTraceControllerRx>.WithFailure(
                $"No PLC driver is installed: PLC {plc.PlcId} ({plc.Name}) cannot be served. " +
                "Enable GatewaySimulationOptions:EnableSimulation to run the in-memory simulated controller.");
        }

        return Result<IIndTraceControllerRx>.Success(new SimulatedControllerRx(logger, plc, dateTimeMachine));
    }

    /// <inheritdoc/>
    public bool IsControllerUnreachable(Exception exception) => false;
}
