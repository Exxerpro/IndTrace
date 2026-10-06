// <copyright file="DeleteCommandHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Generic.Commands.Delete
{
    public interface IDeleteCommand<TEntity> : IMonitorRequest<bool>
        where TEntity : class, new()
    {
        int Id { get; set; }
    }

    public interface IDeleteCommandHandler<TCommand, TEntity>
        where TCommand : class, IDeleteCommand<TEntity>, new()
        where TEntity : class, new()
    {
        Task<Result<bool>> Remove(TCommand command, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Represents the DeleteCommandHandler.
    /// </summary>
    public class DeleteCommandHandler<TCommand, TEntity> : IDeleteCommandHandler<TCommand, TEntity>
        where TCommand : class, IDeleteCommand<TEntity>, new()
        where TEntity : class, IndTrace.Domain.Interfaces.IPersistable, new()
    {
        private readonly IRepository<TEntity> repository;

        public DeleteCommandHandler(IRepository<TEntity> repository)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <inheritdoc/>
        public async Task<Result<bool>> Remove(TCommand command, CancellationToken cancellationToken)
        {
            var entityResult = await this.repository.GetByIdAsync(command.Id, cancellationToken).ConfigureAwait(false);
            if (entityResult.IsFailure)
            {
                return Result<bool>.WithFailure(entityResult.Errors);
            }

            if (entityResult.Value is null)
            {
                return Result<bool>.WithFailure($"No entity of type {typeof(TEntity).Name} with ID {command.Id} was found");
            }

            var deleteResult = await this.repository.DeleteAsync(entityResult.Value, cancellationToken).ConfigureAwait(false);
            if (deleteResult.IsFailure)
            {
                return Result<bool>.WithFailure(deleteResult.Errors);
            }

            return Result<bool>.Success(true);
        }
    }
}