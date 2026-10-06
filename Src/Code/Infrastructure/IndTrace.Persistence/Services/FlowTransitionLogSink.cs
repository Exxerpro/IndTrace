// <copyright file="FlowTransitionLogSink.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Persistence.Services;

using IndTrace.Application.StateMachine;
using IndTrace.Domain.Entities;
using IndTrace.Persistence.Interfaces;
using Microsoft.Extensions.Logging;

/// <summary>
/// EF-backed implementation of the Story 3.5 best-effort <see cref="IFlowTransitionLogSink"/>. Appends one
/// additive <see cref="FlowTransitionLog"/> row using a fresh, short-lived context from the factory and the
/// synchronous <c>SaveChanges</c> (the decorator's <c>Fire</c> seam is synchronous).
///
/// AC5 (result-integrity): this method NEVER throws — any failure is captured into a failure <see cref="Result"/>
/// (which the decorator logs and discards), so a persistence problem cannot flip the transition's outcome.
/// </summary>
public sealed class FlowTransitionLogSink(
    IIndTraceDbContextFactory contextFactory,
    ILogger<FlowTransitionLogSink> logger) : IFlowTransitionLogSink
{
    /// <inheritdoc/>
    public Result Append(FlowTransitionLog log)
    {
        if (log is null)
        {
            return Result.WithFailure("FlowTransitionLog cannot be null.");
        }

        try
        {
            // No pre-flight connection probe (issue #108): a dead connection surfaces at SaveChanges below and
            // is caught and wrapped into the failure Result — probing first only doubled the round-trips (TOCTOU).
            // SaveChanges stays SYNCHRONOUS on purpose: this sink is rooted in the frozen Epic-2 synchronous
            // IItemStateMachine.Fire seam (see IFlowTransitionLogSink docs); blocking on SaveChangesAsync here
            // would be sync-over-async, which is worse than the genuine sync call.
            using var context = contextFactory.CreateDbContext();
            context.Set<FlowTransitionLog>().Add(log);
            context.SaveChanges();
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "FlowTransitionLogSink: failed to append a transition-log row (discarded).");
            return Result.WithFailure($"FlowTransitionLog append failed: {ex.Message}");
        }
    }
}
