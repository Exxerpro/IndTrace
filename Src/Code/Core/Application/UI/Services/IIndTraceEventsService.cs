// <copyright file="IIndTraceEventsService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.UI.Services;

public interface IIndTraceEventsService : IObservable<StateChange>
{
    // Property to expose messages in a read-only fashion
    IReadOnlyCollection<string> Messages { get; }

    // Read-only properties to access event data
    IReadOnlyDictionary<int, TaskGatewayResponseDto> ResponseEvents { get; }

    IReadOnlyDictionary<int, TaskGatewayRequest> RequestEvents { get; }

    IReadOnlyDictionary<int, ControllerMonitor> ControllerMonitors { get; }

    IReadOnlyDictionary<int, StationMonitor> StationMonitors { get; }

    // Expose ProductionData in a read-only way
    ProductionData ProductionData { get; }

    // Methods to handle configuration and updates
    void ApplyConfiguration(ApplicationConfiguration configuration);

    void UpdateStationFromGatewayRequest(TaskGatewayRequest request);

    void UpdateStationFromGatewayResponse(TaskGatewayResponseDto response);

    void AddOrUpdateControllerFromGateway(int id, ControllerMonitor heartBeatControllerMonitor);

    // Expose methods for managing state changes
    void PushMessage(string user, string message);

    void AddOrUpdateTaskGatewayRequest(int id, TaskGatewayRequest requestEvent);

    void AddOrUpdateTaskGatewayResponse(int id, TaskGatewayResponseDto responseEvent);

    // Method to notify state change
}