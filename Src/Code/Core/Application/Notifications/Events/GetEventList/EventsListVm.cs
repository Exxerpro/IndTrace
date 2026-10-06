// <copyright file="EventsListVm.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Notifications.Events.GetEventList;

/// <summary>
/// Represents the EventsListVm.
/// </summary>
public class EventsListVm
{
    /// <summary>
    /// Executes CreateEventsListVm operation.
    /// </summary>
    /// <param name="requests">The requests.</param>
    /// <param name="responses">The responses.</param>
    /// <returns>The result of CreateEventsListVm.</returns>
    public static Result<EventsListVm> CreateEventsListVm(Result<IEnumerable<TaskGatewayRequest>> requests, Result<IEnumerable<TaskGatewayResponse>> responses)
    {
        // #32 C2: the events list reads persisted entities; up-project them onto the immutable wire DTO so the whole
        // monitor/UI surface (StationMonitor mapping, Player controls) speaks a single wire type.
        var responseDtos = (responses.Value ?? []).Select(TaskGatewayResponsePersistence.ToDto);
        var result = new EventsListVm(requests.Value ?? [], responseDtos);

        if (requests.IsSuccess && responses.IsSuccess)
        {
            return Result<EventsListVm>.Success(result);
        }

        return Result<EventsListVm>.CombineErrors<EventsListVm>(requests.Errors, responses.Errors, result);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EventsListVm"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="requests">The requests.</param>
    /// <param name="responses">The responses.</param>
    public EventsListVm(IEnumerable<TaskGatewayRequest> requests, IEnumerable<TaskGatewayResponseDto> responses)
    {
        this.Requests = requests;
        this.Responses = responses;
    }

    /// <summary>
    /// Gets the Requests.
    /// </summary>
    public IEnumerable<TaskGatewayRequest> Requests { get; private set; }

    /// <summary>
    /// Gets the Responses.
    /// </summary>
    public IEnumerable<TaskGatewayResponseDto> Responses { get; private set; }
}