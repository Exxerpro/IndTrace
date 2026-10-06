// <copyright file="GatewayPipelineCompositionTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Application.UnitTests.TestDoubles;
using IndTrace.Application.StateMachine;

namespace Application.UnitTests.StateMachineRouting;

/// <summary>
/// Issue #176 acceptance smoke — proves the IndQuestResults 1.7.0 pipeline verbs
/// (<c>RequireValue</c> → <c>EnsureOrFault</c> → value-aware <c>TapError</c>, namespace
/// <c>IndQuestResults.Operations</c>) compose with the new <see cref="GatewayFailureFactory"/> and the
/// <see cref="GatewayFaultLoggerExtensions"/> terminal: a load-carried error flows through the chain as a
/// value-CARRYING failure whose DTO holds the specific negative code, and the chain ends with ONE
/// <c>.TapError(logger.LogGatewayFault)</c> call.
/// </summary>
public class GatewayPipelineCompositionTests
{
    private const int MachineId = 7;

    [Fact]
    public void FailurePath_ComposesRequireValueEnsureOrFaultAndValueAwareTapError()
    {
        var routing = new StateMachineRoutingOptions();
        var logger = new TestLogger<GatewayPipelineCompositionTests>();
        var faultySnapshot = new BarCodeSnapshot
        {
            MachineId = MachineId,
            Error = "Cycle not Found",
            ResultValidation = ResultValidation.CycleNotFound,
        };

        var result = Result<TaskGatewayResponseDto?>
            .Success(BarCodeResultProjection.ToResponse(faultySnapshot))
            .RequireValue("Failed to retrieve barcode information")
            .EnsureOrFault(
                dto => dto.Error.Length == 0,
                dto => GatewayFailureFactory.Fail(routing, dto.Error, dto, ResultValidation.CycleNotFound))
            .TapError(logger.LogGatewayFault);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Cycle not Found");
        var dto = result.Value.ShouldNotBeNull(); // EnsureOrFault let the factory carry the diagnostic value
        dto.ResultValidation.ShouldBe(ResultValidation.CycleNotFound);
        dto.References.ContainsKey(nameof(TaskGatewayResponseDto.ResultValidation)).ShouldBeTrue();

        // The value-aware TapError terminal fired exactly once with the machine id + specific code.
        logger.GetLogCount(LogLevel.Error).ShouldBe(1);
        logger.HasMessage(MachineId.ToString(System.Globalization.CultureInfo.InvariantCulture)).ShouldBeTrue();
        logger.HasMessage(ResultValidation.CycleNotFound.Name).ShouldBeTrue();
    }

    [Fact]
    public void SuccessPath_FlowsThroughWithoutFiringTheTerminal()
    {
        var routing = new StateMachineRoutingOptions();
        var logger = new TestLogger<GatewayPipelineCompositionTests>();
        var cleanSnapshot = new BarCodeSnapshot { MachineId = MachineId };

        var result = Result<TaskGatewayResponseDto?>
            .Success(BarCodeResultProjection.ToResponse(cleanSnapshot))
            .RequireValue("Failed to retrieve barcode information")
            .EnsureOrFault(
                dto => dto.Error.Length == 0,
                dto => GatewayFailureFactory.Fail(routing, dto.Error, dto, ResultValidation.CycleNotFound))
            .TapError(logger.LogGatewayFault);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().MachineId.ShouldBe(MachineId);
        logger.GetLogCount(LogLevel.Error).ShouldBe(0);
    }
}
