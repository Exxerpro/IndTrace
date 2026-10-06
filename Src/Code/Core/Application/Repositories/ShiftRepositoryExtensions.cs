// <copyright file="ShiftRepositoryExtensions.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Repositories;

/// <summary>
/// Provides extension methods for <see cref="IRepository{Shift}"/> to support common shift queries and operations.
/// </summary>
public static class ShiftRepositoryExtensions
{
    /// <summary>
    /// Gets the shift entity for the current date and time on a specific machine, using the provided
    /// date/time provider. Scoped by machine because shifts are tracked per machine.
    /// </summary>
    /// <param name="shiftRepository">The shift repository.</param>
    /// <param name="dateTimeMachine">The date/time provider (required; no ambient clock is created).</param>
    /// <param name="machineId">The machine whose current shift is requested.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The shift entity if found, or a failure result.</returns>
    public static async Task<Result<Shift>> GetShiftByDateAsync(
        this IRepository<Shift> shiftRepository,
        IDateTimeMachine dateTimeMachine,
        int machineId,
        CancellationToken cancellationToken)
    {
        if (shiftRepository is null)
        {
            return Result<Shift>.WithFailure("Shift repository cannot be null.");
        }

        if (dateTimeMachine is null)
        {
            return Result<Shift>.WithFailure("A date/time provider is required.");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Result<Shift>.WithFailure("Operation was canceled.");
        }

        var timeShift = dateTimeMachine.Now.ToLocalTime();

        try
        {
            var spec = new Specification<Shift>(b =>
                b.StartBy <= timeShift && b.EndTime >= timeShift && b.MachineId == machineId);
            var shift = await shiftRepository.FirstOrDefaultAsync(spec, cancellationToken).ConfigureAwait(false);
            if (shift.IsFailure)
            {
                return Result<Shift>.WithFailure(shift.Errors);
            }

            if (shift.Value is null)
            {
                return Result<Shift>.WithFailure("Shift not found for current time");
            }

            return Result<Shift>.Success(shift.Value);
        }
        catch (Exception e)
        {
            return Result<Shift>.WithFailure($"Error while retrieving shift {e}");
        }
    }
}