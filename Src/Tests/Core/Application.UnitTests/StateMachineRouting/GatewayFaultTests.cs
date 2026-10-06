// <copyright file="GatewayFaultTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.StateMachine;

namespace Application.UnitTests.StateMachineRouting;

/// <summary>
/// Issue #176 — pins the <see cref="GatewayFault"/> value object: the log level defaults to
/// <see cref="LogLevel.Error"/> (behavior-neutral — every as-built handler failure logs LogError today), the
/// message/code are carried verbatim, and record equality/<c>with</c> semantics hold.
/// </summary>
public class GatewayFaultTests
{
    [Fact]
    public void Level_DefaultsToError()
    {
        var fault = new GatewayFault("boom", ResultValidation.Invalid);

        fault.Level.ShouldBe(LogLevel.Error);
    }

    [Fact]
    public void CarriesMessageAndCodeVerbatim()
    {
        var fault = new GatewayFault("station refused", ResultValidation.InvalidMachine);

        fault.Message.ShouldBe("station refused");
        fault.Code.ShouldBe(ResultValidation.InvalidMachine);
    }

    [Fact]
    public void Level_CanBeOverridden()
    {
        var fault = new GatewayFault("soft reject", ResultValidation.PartRejected, LogLevel.Warning);

        fault.Level.ShouldBe(LogLevel.Warning);
    }

    [Fact]
    public void With_ProducesNewInstanceWithoutMutatingSource()
    {
        var original = new GatewayFault("boom", ResultValidation.Invalid);

        var promoted = original with { Code = ResultValidation.CycleNotFound };

        original.Code.ShouldBe(ResultValidation.Invalid);
        promoted.Code.ShouldBe(ResultValidation.CycleNotFound);
        promoted.Message.ShouldBe("boom");
    }
}
