// <copyright file="IndTraceHubJsonProtocol.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.HubConnection.Protocols;

using System.Text.Json;
using IndTrace.Domain.ValueObjects;

/// <summary>
/// Single source of truth for the SignalR hub JSON payload serializer options (issue #188). Hub payloads
/// (<c>TaskGatewayRequest</c>, <c>TaskGatewayResponseDto</c>) carry domain types that DEFAULT System.Text.Json
/// cannot round-trip: <c>Register</c> (private ctor + private setters, #39 write-once lockdown) and
/// <c>BarCodeLabel</c> (private ctor) throw <see cref="NotSupportedException"/> during server-side argument
/// binding, while <c>EnumModel</c> smart enums (CycleStatus, PartStatus, GatewayTask, WorkFlowType, ...)
/// silently corrupt to their <c>Invalid(-1)</c> sentinel without throwing. BOTH the hub server
/// (<c>IndTrace.Hub.Server</c> Program) and every client built by <c>HubConnectionFactory</c> must chain
/// <c>.AddJsonProtocol(o =&gt; IndTraceHubJsonProtocol.Configure(o.PayloadSerializerOptions))</c> so the two
/// sides agree on the wire shape.
/// </summary>
public static class IndTraceHubJsonProtocol
{
    /// <summary>
    /// Registers the domain converters the hub JSON payloads require: the composite-flag-aware
    /// <c>EnumModelJsonConverter</c> (preserves the #150 WorkFlowType flag-composite contract),
    /// <c>StronglyTypedIdJsonConverterFactory</c> (IIntId structs as bare ints),
    /// <c>BarCodeLabelJsonConverter</c> (label value object as a bare string), and
    /// <c>RegisterJsonConverter</c> (write-once Register reconstructed through its factory seam).
    /// </summary>
    /// <param name="options">The payload serializer options to configure (server and client alike).</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null (configuration-time programming error).</exception>
    public static void Configure(JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        // The composite-flag-aware EnumModel converter (moved to Domain for #188 so this project can share
        // the exact converter the FusionCache serializers use, including the WorkFlowType #150 contract).
        options.Converters.Add(new EnumModelJsonConverter());

        // Story 35.D2: IIntId strongly-typed id structs (CycleId, BarCodeId, MachineId, ...) as bare ints.
        options.Converters.Add(new StronglyTypedIdJsonConverterFactory());

        // Story 27.2b-2: BarCode.Label value object (private ctor) as a bare string.
        options.Converters.Add(new BarCodeLabelJsonConverter());

        // #188: write-once Register (#39 lockdown) reconstructed through its public Create factory seam.
        options.Converters.Add(new RegisterJsonConverter());
    }
}
