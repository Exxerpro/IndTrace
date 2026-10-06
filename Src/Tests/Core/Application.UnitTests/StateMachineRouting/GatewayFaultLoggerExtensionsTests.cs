// <copyright file="GatewayFaultLoggerExtensionsTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Application.UnitTests.TestDoubles;
using IndTrace.Application.StateMachine;

namespace Application.UnitTests.StateMachineRouting;

/// <summary>
/// Issue #176 — pins the terminal <see cref="GatewayFaultLoggerExtensions"/> logging helper: a null logger is
/// tolerated silently (webapp handlers hold <c>ILogger?</c>), the <see cref="GatewayFault"/> overload logs at the
/// fault's own level, and the <c>(errors, dto)</c> overload — the value-aware <c>TapError</c> seam — logs one
/// Error entry carrying the machine id, the specific code name and the error list.
/// </summary>
public class GatewayFaultLoggerExtensionsTests
{
    private const int MachineId = 42;

    [Fact]
    public void FaultOverload_NullLogger_DoesNotThrow()
    {
        ILogger? logger = null;

        Should.NotThrow(() => logger.LogGatewayFault(new GatewayFault("boom", ResultValidation.Invalid)));
    }

    [Fact]
    public void ErrorsDtoOverload_NullLogger_DoesNotThrow()
    {
        ILogger? logger = null;

        Should.NotThrow(() => logger.LogGatewayFault(new[] { "boom" }, dto: null));
    }

    [Fact]
    public void FaultOverload_LogsAtFaultLevel()
    {
        var logger = new TestLogger<GatewayFaultLoggerExtensionsTests>();
        var fault = new GatewayFault("soft reject", ResultValidation.PartRejected, LogLevel.Warning);

        logger.LogGatewayFault(fault);

        logger.GetLogCount(LogLevel.Warning).ShouldBe(1);
        logger.GetLogCount(LogLevel.Error).ShouldBe(0);
        logger.HasMessage("soft reject").ShouldBeTrue();
        logger.HasMessage(ResultValidation.PartRejected.Name).ShouldBeTrue(); // "PieceRejected"
    }

    [Fact]
    public void FaultOverload_DefaultLevel_LogsError()
    {
        var logger = new TestLogger<GatewayFaultLoggerExtensionsTests>();

        logger.LogGatewayFault(new GatewayFault("hard failure", ResultValidation.CycleNotFound));

        logger.GetLogCount(LogLevel.Error).ShouldBe(1);
        logger.HasMessage("hard failure").ShouldBeTrue();
    }

    [Fact]
    public void ErrorsDtoOverload_LogsMachineIdCodeAndErrors()
    {
        var logger = new TestLogger<GatewayFaultLoggerExtensionsTests>();
        var dto = PlcFailureDiagnostics.BuildDiagnosticResponse(ResultValidation.InvalidMachine, MachineId);

        logger.LogGatewayFault(new[] { "first error", "second error" }, dto);

        logger.GetLogCount(LogLevel.Error).ShouldBe(1);
        logger.HasMessage(MachineId.ToString(System.Globalization.CultureInfo.InvariantCulture)).ShouldBeTrue();
        logger.HasMessage(ResultValidation.InvalidMachine.Name).ShouldBeTrue();
        logger.HasMessage("first error").ShouldBeTrue();
        logger.HasMessage("second error").ShouldBeTrue();
    }

    [Fact]
    public void ErrorsDtoOverload_NullDto_LogsNoneCodeWithoutThrowing()
    {
        var logger = new TestLogger<GatewayFaultLoggerExtensionsTests>();

        Should.NotThrow(() => logger.LogGatewayFault(new[] { "value-less failure" }, dto: null));

        logger.GetLogCount(LogLevel.Error).ShouldBe(1);
        logger.HasMessage("value-less failure").ShouldBeTrue();
    }
}
