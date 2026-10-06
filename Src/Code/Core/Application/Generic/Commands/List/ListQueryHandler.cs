// <copyright file="ListQueryHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Generic.Commands.List
{
    public interface TCommandList
    {
        string[] Includes { get; set; }

        int Page { get; set; }

        int PageSize { get; set; }
    }

    public interface IListQuery<TCommand, TResponse> : IMonitorRequestHandler<TCommand, TResponse>
            where TCommand : IMonitorRequest<TResponse>, TCommandList, new()
            where TResponse : class // #39: relaxed from `new()` — TResponse is never instantiated here, and the write-once Register flows through as a list response.
    {
        IEnumerable<TResponse> Entities { get; set; }

        public Task<Result<IEnumerable<TResponse>>> ListProcessAsync(TCommand command, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Represents the ListQueryHandler.
    /// </summary>
    public class ListQueryHandler<TCommand, TResponse>(IReadOnlyRepository<TResponse> repository) : IListQuery<TCommand, TResponse>
            where TCommand : class, IMonitorRequest<TResponse>, TCommandList, new()
            where TResponse : class, IndTrace.Domain.Interfaces.IPersistable // #39: relaxed from `new()` — TResponse is never instantiated here, and the write-once Register flows through as a list response. TResponse is the listed entity, so it must be persistable.
    {
        private readonly IReadOnlyRepository<TResponse> repository = repository ?? throw new ArgumentNullException(nameof(repository));

        /// <summary>
        /// Gets or sets the Includes.
        /// </summary>
        public string[] Includes { get; set; } = [];

        /// <summary>
        /// Gets or sets the Page.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Gets or sets the PageSize.
        /// </summary>
        public int PageSize { get; set; }

        public const int PageSizeMin = 10;
        public const int PageSizeMax = 100;

        /// <summary>
        /// Gets or sets the Entities.
        /// </summary>
        public IEnumerable<TResponse> Entities { get; set; } = [];

        /// <summary>
        /// Executes ListProcessAsync operation.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="cancellationToken">The cancellationToken.</param>
        /// <returns>The result of ListProcessAsync.</returns>
        public async Task<Result<IEnumerable<TResponse>>> ListProcessAsync(TCommand command, CancellationToken cancellationToken)
        {
            this.Page = command.Page > 0 ? command.Page : 1;

            // [Fix]
            // CLAUDE
            // Date: 24/08/2025
            // Reason: [LOGIC BUG FIX] - Fixed PageSize calculation logic. When PageSize <= 0, use PageSizeMin. When PageSize > PageSizeMax, use PageSizeMax.
            this.PageSize = command.PageSize > 0 ?
                (command.PageSize > PageSizeMax ? PageSizeMax : command.PageSize) :
                PageSizeMin;

            // #118: push pagination into the query via the specification pattern so Skip/Take execute
            // in the data store instead of materializing the entire table and paging in memory.
            var pagedSpecification = new Specification<TResponse>(_ => true)
                .ApplyPaging((this.Page - 1) * this.PageSize, this.PageSize);

            var repositoryResult = await this.repository.ListAsync(pagedSpecification, cancellationToken).ConfigureAwait(false);

            if (repositoryResult.IsFailure)
            {
                return Result<IEnumerable<TResponse>>.WithFailure(repositoryResult.Errors);
            }

            if (repositoryResult.Value is null)
            {
                return Result<IEnumerable<TResponse>>.WithFailure($"No entities of type {typeof(TResponse).Name} were found");
            }

            var paginatedResult = repositoryResult.Value.ToList();

            return Result<IEnumerable<TResponse>>.Success(paginatedResult);
        }

        /// <summary>
        /// Processes the list query request asynchronously.
        /// </summary>
        /// <param name="request">The list query request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A Result containing the list of responses or failure information.</returns>
        public async Task<Result<TResponse>> ProcessAsync(TCommand request, CancellationToken cancellationToken)
        {
            var listResult = await this.ListProcessAsync(request, cancellationToken);
            if (listResult.IsFailure)
            {
                return Result<TResponse>.WithFailure(listResult.Errors);
            }

            // For list operations, we typically return the first item or a summary
            // This is a placeholder implementation - adjust based on your needs
            var firstItem = listResult.Value?.FirstOrDefault();
            if (firstItem == null)
            {
                return Result<TResponse>.WithFailure("No items found in the list");
            }

            return Result<TResponse>.Success(firstItem);
        }
    }
}