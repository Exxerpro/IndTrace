// <copyright file="GatewaySimulationOptions.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Gateway.Gateway;

/// <summary>
/// Host-level gateway simulation switch (issue #101 follow-up). The per-PLC <c>PlcDto.EnableSimulation</c>
/// flag has NO persisted source (<c>PlcDto.ToDto</c> never maps it and the <c>Plc</c> entity has no such
/// column), so before this options class existed the advertised NoOpPlc escape hatch was structurally always
/// OFF. When this flag is ON the gateway marks every PLC configuration it hands to a controller as
/// simulation-enabled, which makes controller setup construct the explicit <c>NoOpPlc</c> instead of a real
/// <c>Sharp7Plc</c>, and the console simulation commands are allowed to dispatch. Default is OFF (real PLC
/// I/O — the issue #101 fix's purpose) and parsing is fail-safe: absent or malformed configuration can NEVER
/// silently enable simulation.
/// </summary>
public sealed class GatewaySimulationOptions
{
    /// <summary>
    /// Configuration section name. Bindable from appsettings
    /// (<c>{ "GatewaySimulationOptions": { "EnableSimulation": true } }</c>) or an environment variable
    /// (<c>GatewaySimulationOptions__EnableSimulation=true</c>).
    /// </summary>
    public const string SectionName = "GatewaySimulationOptions";

    /// <summary>
    /// Gets or sets a value indicating whether the gateway runs in simulation mode (NoOpPlc controllers, no
    /// real PLC I/O). Defaults to <see langword="false"/>: the gateway talks to real PLCs.
    /// </summary>
    public bool EnableSimulation { get; set; }

    /// <summary>
    /// Builds the options from host configuration with fail-safe parsing: a missing section, a missing key, a
    /// <see langword="null"/> configuration or a value that is not exactly a parseable boolean all yield the
    /// default (simulation OFF, real PLC I/O). This method never throws — a malformed value on a QA rig must
    /// degrade to the safe default rather than crash the gateway or, worse, silently enable writes.
    /// </summary>
    /// <param name="configuration">The host configuration to read from; may be <see langword="null"/>.</param>
    /// <returns>The parsed options; <see cref="EnableSimulation"/> is <see langword="false"/> unless the
    /// configured value parses exactly to <see langword="true"/>.</returns>
    public static GatewaySimulationOptions FromConfiguration(IConfiguration? configuration)
    {
        // Deliberately NOT the options binder: Bind/GetValue throw on a malformed boolean, and this switch
        // must degrade to the safe default instead. bool.TryParse accepts only "true"/"false" (any casing),
        // so "1", "yes" or a typo can never arm simulation mode.
        var raw = configuration?[$"{SectionName}:{nameof(EnableSimulation)}"];

        return new GatewaySimulationOptions
        {
            EnableSimulation = bool.TryParse(raw, out var parsed) && parsed,
        };
    }
}
