// <copyright file="GetEventsListQueryHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Notifications.Events.GetEventList;

using System.Drawing.Printing;

/// <summary>
/// Represents the GetEventsListQueryHandler.
/// </summary>
public class GetEventsListQueryHandler(IRepository<TaskGatewayRequest> repositoryRequests,
        IReadOnlyRepository<TaskGatewayResponse> repositoryResponses) :
    IMonitorRequestHandler<GetEventsListQuery, EventsListVm>
{
    /// <inheritdoc/>
    public async Task<Result<EventsListVm>> ProcessAsync(GetEventsListQuery request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result<EventsListVm>.WithFailure("request cannot be null.");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Result<EventsListVm>.WithFailure("Operation was canceled.");
        }

        try
        {
            // var specRequests = new Specification<TaskGatewayRequest>(e =>
            //    e.TimeStamp >= monitorRequest.StartDate && e.TimeStamp <= monitorRequest.EndDate);

            // var specResponses = new Specification<TaskGatewayResponse>(e =>
            //    e.TimeStamp >= monitorRequest.StartDate && e.TimeStamp <= monitorRequest.EndDate);
            //// Define your criteria here
            // Define a specification with no filtering (fetch all records)
            var specRequests = new Specification<TaskGatewayRequest>(_ => true) // No filtering criterion
                .AddOrderByDescending(e => e.TimeStamp) // Order by the TimeStamp property
                .ApplyPaging((request.PageNumber - 1) * request.PageSize, request.PageSize); // Apply paging (skip, take)

            var specResponses = new Specification<TaskGatewayResponse>(_ => true) // No filtering criterion
                .AddOrderByDescending(e => e.TimeStamp) // Order by the TimeStamp property
                .ApplyPaging((request.PageNumber - 1) * request.PageSize, request.PageSize);

            // Fetch the data using the specification
            var requests = await repositoryRequests.ListAsync(specRequests, cancellationToken).ConfigureAwait(false);

            var responses = await repositoryResponses.ListAsync(specResponses, cancellationToken).ConfigureAwait(false);

            var result = EventsListVm.CreateEventsListVm(requests, responses);

            return result;
        }
        catch (Exception ex)
        {
            return Result<EventsListVm>.WithFailure($"Operation finished with an exception {ex.Message}");
        }
    }
}
