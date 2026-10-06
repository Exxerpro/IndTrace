// <copyright file="IEventsService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Notifications;

using IndTrace.Application.Notifications.Events.GetEventList;

public interface IEventsService
{
    int ActualPage { get; set; }

    int PageSize { get; set; }

    Task<Result<EventsListVm>> GetNextEventsAsync(CancellationToken cancellationToken = default);

    Task<Result<EventsListVm>> GetPreviousEventsAsync(CancellationToken cancellationToken = default);
}