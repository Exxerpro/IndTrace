// <copyright file="ICycleLimitPolicy.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Cycles.Commands.Create;

namespace IndTrace.Application.Cycles.Policies;

/// <summary>
/// Enforces cycle limit policies for manufacturing safety.
/// Based on CreateCyclesCommandHandler cycle limit validation logic.
/// </summary>
public interface ICycleLimitPolicy
{
    /// <summary>
    /// Evaluates if cycle creation is allowed based on existing cycle counts.
    /// </summary>
    /// <param name="barCodeInfo">The barcode information containing cycle history and recipe limits.</param>
    /// <param name="command">The command containing machine information.</param>
    /// <returns>Result containing cycle limit decision or failure reasons.</returns>
    Result<CycleLimitDecision> EvaluateCycleLimits(
        IBarCodeResult barCodeInfo,
        CreateCyclesCommand command);
}

/// <summary>
/// Decision result for cycle limit evaluation.
/// </summary>
/// <param name="IsAllowed">True if cycle creation is allowed; otherwise false.</param>
/// <param name="Reason">Human-readable reason for the decision.</param>
/// <param name="ValidationResult">Result validation status for workflow tracking.</param>
public sealed record CycleLimitDecision(
    bool IsAllowed,
    string Reason,
    ResultValidation ValidationResult);