// <copyright file="HubMethods.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.HubConnection.Contracts;

public static class HubMethods
{
    public const string BroadcastMessageToClients = nameof(BroadcastMessageToClients);
    public const string BroadcastTaskGatewayRequest = nameof(BroadcastTaskGatewayRequest);
    public const string BroadcastTaskGatewayResponse = nameof(BroadcastTaskGatewayResponse);
    public const string BroadcastHeartbeatSignal = nameof(BroadcastHeartbeatSignal);
}

