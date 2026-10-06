// <copyright file="DetailQueryHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Generic.Commands.Detail
{
    public interface IDetailQuery<TEntity> : IMonitorRequest<TEntity>
        where TEntity : class, new()
    {
        int Id { get; set; }
    }

    public interface IDetailQueryHandler<TQuery, TEntity>
        where TQuery : class, IDetailQuery<TEntity>, new()
        where TEntity : class, new()
    {
        Task<Result<TEntity>> Find(TQuery command, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Represents the DetailQueryHandler.
    /// </summary>
    public class DetailQueryHandler<TQuery, TEntity> : IDetailQueryHandler<TQuery, TEntity>
        where TQuery : class, IDetailQuery<TEntity>, new()
        where TEntity : class, IndTrace.Domain.Interfaces.IPersistable, new()
    {
        private readonly IReadOnlyRepository<TEntity> repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="DetailQueryHandler{TQuery, TEntity}"/> class.
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public DetailQueryHandler(IReadOnlyRepository<TEntity> repository)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <summary>
        /// Executes Find operation.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="cancellationToken">The cancellationToken.</param>
        /// <returns>The result of Find.</returns>
        public async Task<Result<TEntity>> Find(TQuery query, CancellationToken cancellationToken)
        {
            var entityResult = await this.repository.GetByIdAsync(query.Id, cancellationToken).ConfigureAwait(false);

            if (entityResult.IsFailure)
            {
                return Result<TEntity>.WithFailure(entityResult.Errors);
            }

            if (entityResult.Value is null)
            {
                return Result<TEntity>.WithFailure($"No entity of type {typeof(TEntity).Name} with ID {query.Id} was found");
            }

            return Result<TEntity>.Success(entityResult.Value);
        }
    }
}