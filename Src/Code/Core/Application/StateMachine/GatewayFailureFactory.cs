// <copyright file="GatewayFailureFactory.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.StateMachine;

using IndTrace.Application.BarCodes.Services;

/// <summary>
/// Issue #176 — the single shared factory for PLC-gateway failure <c>Result&lt;TaskGatewayResponseDto&gt;</c>
/// values, centralizing what the routed handlers hand-roll today. It is a thin COMPOSITION layer over the Story
/// 3.3 primitives (<see cref="PlcFailureDiagnostics.Promote"/>, <see cref="PlcFailureDiagnostics.BuildDiagnosticResponse"/>,
/// <see cref="PlcFailureDiagnostics.Classify"/>) and adds no classification logic of its own.
/// </summary>
/// <remarks>
/// <para>
/// Two carriage modes exist, mirroring the two shipped handler behaviors:
/// </para>
/// <para>
/// <b>Flag-gated</b> (<see cref="Fail(StateMachineRoutingOptions, string, BarCodeSnapshot, ResultValidation)"/> /
/// <see cref="FailWithoutProjection(StateMachineRoutingOptions, string, int, ResultValidation, IDictionary{string, Register})"/>)
/// — the <c>CreateCyclesCommandHandler</c> spec: when <see cref="StateMachineRoutingOptions.SpecificDiagnostics"/>
/// is OFF the failure is value-LESS, so the transport's legacy generic <c>-1</c> collapse re-applies (the shipped
/// zero-redeploy rollback, Story 3.3 AC8); when ON the failure CARRIES a diagnostic DTO whose
/// <c>ResultValidation</c> and <c>References["ResultValidation"]</c> register hold the specific negative code.
/// </para>
/// <para>
/// <b>Always-carry</b> (<see cref="FailAlways(string, int, ResultValidation, IDictionary{string, Register})"/> /
/// <see cref="BuildFailureDto"/>) — the <c>UpdateCyclesCommandHandler.BuildFailureDto</c> spec: the DTO is ALWAYS
/// carried and the flag is intentionally not consulted (ratified as-built pin from the #175 characterization); the
/// code is the caller's explicit one when known, otherwise the first failure message is classified through
/// <see cref="PlcFailureDiagnostics.Classify"/>.
/// </para>
/// </remarks>
public static class GatewayFailureFactory
{
    /// <summary>
    /// Flag-gated failure that projects the <paramref name="snapshot"/> onto the §7 DTO (via
    /// <see cref="BarCodeResultProjection.ToResponse"/>) and promotes it with the specific negative
    /// <paramref name="code"/> when <c>SpecificDiagnostics</c> is ON; value-less failure when OFF.
    /// </summary>
    /// <param name="routing">The routing options carrying the <c>SpecificDiagnostics</c> flag.</param>
    /// <param name="error">The failure message.</param>
    /// <param name="snapshot">The immutable load snapshot to project into the diagnostic value.</param>
    /// <param name="code">The specific negative <see cref="ResultValidation"/> to publish.</param>
    /// <returns>The flag-gated failure result.</returns>
    public static Result<TaskGatewayResponseDto> Fail(
        StateMachineRoutingOptions routing,
        string error,
        BarCodeSnapshot snapshot,
        ResultValidation code)
    {
        ArgumentNullException.ThrowIfNull(routing);
        ArgumentNullException.ThrowIfNull(snapshot);

        if (!routing.SpecificDiagnostics)
        {
            return Result<TaskGatewayResponseDto>.WithFailure(error);
        }

        return Fail(routing, error, BarCodeResultProjection.ToResponse(snapshot), code);
    }

    /// <summary>
    /// Flag-gated failure that promotes an ALREADY-built §7 projection with the specific negative
    /// <paramref name="code"/> (via <see cref="PlcFailureDiagnostics.Promote"/>) when <c>SpecificDiagnostics</c>
    /// is ON; value-less failure when OFF.
    /// </summary>
    /// <param name="routing">The routing options carrying the <c>SpecificDiagnostics</c> flag.</param>
    /// <param name="error">The failure message.</param>
    /// <param name="projection">The already-projected §7 response to promote.</param>
    /// <param name="code">The specific negative <see cref="ResultValidation"/> to publish.</param>
    /// <returns>The flag-gated failure result.</returns>
    public static Result<TaskGatewayResponseDto> Fail(
        StateMachineRoutingOptions routing,
        string error,
        TaskGatewayResponseDto projection,
        ResultValidation code)
    {
        ArgumentNullException.ThrowIfNull(routing);
        ArgumentNullException.ThrowIfNull(projection);

        if (!routing.SpecificDiagnostics)
        {
            return Result<TaskGatewayResponseDto>.WithFailure(error);
        }

        var dto = PlcFailureDiagnostics.Promote(projection, code);
        return Result<TaskGatewayResponseDto>.WithFailure(error, dto);
    }

    /// <summary>
    /// Flag-gated <see cref="GatewayFault"/> overload of
    /// <see cref="Fail(StateMachineRoutingOptions, string, BarCodeSnapshot, ResultValidation)"/>: a pipeline step
    /// returns the fault and the terminal maps it here.
    /// </summary>
    /// <param name="routing">The routing options carrying the <c>SpecificDiagnostics</c> flag.</param>
    /// <param name="fault">The fault carrying the message and the specific code.</param>
    /// <param name="snapshot">The immutable load snapshot to project into the diagnostic value.</param>
    /// <returns>The flag-gated failure result.</returns>
    public static Result<TaskGatewayResponseDto> Fail(
        StateMachineRoutingOptions routing,
        GatewayFault fault,
        BarCodeSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(fault);

        return Fail(routing, fault.Message, snapshot, fault.Code);
    }

    /// <summary>
    /// Flag-gated failure for branches with NO barcode projection to promote (e.g. the load itself failed):
    /// builds a bare diagnostic value via <see cref="PlcFailureDiagnostics.BuildDiagnosticResponse"/> when
    /// <c>SpecificDiagnostics</c> is ON; value-less failure when OFF.
    /// </summary>
    /// <param name="routing">The routing options carrying the <c>SpecificDiagnostics</c> flag.</param>
    /// <param name="error">The failure message.</param>
    /// <param name="machineId">The originating machine id for the diagnostic value.</param>
    /// <param name="code">The specific negative <see cref="ResultValidation"/> to publish.</param>
    /// <param name="references">An optional References dictionary to carry through onto the diagnostic value.</param>
    /// <returns>The flag-gated failure result.</returns>
    public static Result<TaskGatewayResponseDto> FailWithoutProjection(
        StateMachineRoutingOptions routing,
        string error,
        int machineId,
        ResultValidation code,
        IDictionary<string, Register>? references = null)
    {
        ArgumentNullException.ThrowIfNull(routing);

        if (!routing.SpecificDiagnostics)
        {
            return Result<TaskGatewayResponseDto>.WithFailure(error);
        }

        var dto = PlcFailureDiagnostics.BuildDiagnosticResponse(code, machineId, references);
        return Result<TaskGatewayResponseDto>.WithFailure(error, dto);
    }

    /// <summary>
    /// Flag-gated <see cref="GatewayFault"/> overload of
    /// <see cref="FailWithoutProjection(StateMachineRoutingOptions, string, int, ResultValidation, IDictionary{string, Register})"/>.
    /// </summary>
    /// <param name="routing">The routing options carrying the <c>SpecificDiagnostics</c> flag.</param>
    /// <param name="fault">The fault carrying the message and the specific code.</param>
    /// <param name="machineId">The originating machine id for the diagnostic value.</param>
    /// <param name="references">An optional References dictionary to carry through onto the diagnostic value.</param>
    /// <returns>The flag-gated failure result.</returns>
    public static Result<TaskGatewayResponseDto> FailWithoutProjection(
        StateMachineRoutingOptions routing,
        GatewayFault fault,
        int machineId,
        IDictionary<string, Register>? references = null)
    {
        ArgumentNullException.ThrowIfNull(fault);

        return FailWithoutProjection(routing, fault.Message, machineId, fault.Code, references);
    }

    /// <summary>
    /// Always-carry failure (the <c>UpdateCyclesCommandHandler</c> mode — intentionally IGNORES the
    /// <c>SpecificDiagnostics</c> flag; ratified as-built pin): the returned failure ALWAYS carries a diagnostic
    /// DTO whose code is <paramref name="explicitCode"/> when supplied, otherwise
    /// <see cref="PlcFailureDiagnostics.Classify"/> of the <paramref name="error"/> message.
    /// </summary>
    /// <param name="error">The failure message (also the classification input when no explicit code is given).</param>
    /// <param name="machineId">The originating machine id for the diagnostic value.</param>
    /// <param name="explicitCode">The precise code when the caller already knows it (e.g. the station validator's own).</param>
    /// <param name="references">An optional References dictionary to carry through onto the diagnostic value.</param>
    /// <returns>The value-carrying failure result.</returns>
    public static Result<TaskGatewayResponseDto> FailAlways(
        string error,
        int machineId,
        ResultValidation? explicitCode = null,
        IDictionary<string, Register>? references = null)
    {
        var dto = BuildFailureDto(error, machineId, explicitCode, references);
        return Result<TaskGatewayResponseDto>.WithFailure(error, dto);
    }

    /// <summary>
    /// Always-carry failure preserving a MULTI-error collection on the result (the
    /// <c>WithFailure(loadResult.Errors, dto)</c> shape): the first error is classified when no explicit code is
    /// supplied, and every error survives on <c>Result.Errors</c>.
    /// </summary>
    /// <param name="errors">The failure messages; the FIRST is the classification input.</param>
    /// <param name="machineId">The originating machine id for the diagnostic value.</param>
    /// <param name="explicitCode">The precise code when the caller already knows it.</param>
    /// <param name="references">An optional References dictionary to carry through onto the diagnostic value.</param>
    /// <returns>The value-carrying failure result.</returns>
    public static Result<TaskGatewayResponseDto> FailAlways(
        IReadOnlyList<string> errors,
        int machineId,
        ResultValidation? explicitCode = null,
        IDictionary<string, Register>? references = null)
    {
        ArgumentNullException.ThrowIfNull(errors);

        var dto = BuildFailureDto(errors.FirstOrDefault(), machineId, explicitCode, references);
        return Result<TaskGatewayResponseDto>.WithFailure(errors, dto);
    }

    /// <summary>
    /// Always-carry <see cref="GatewayFault"/> overload: the fault's own code is explicit and therefore always
    /// wins over message classification.
    /// </summary>
    /// <param name="fault">The fault carrying the message and the specific code.</param>
    /// <param name="machineId">The originating machine id for the diagnostic value.</param>
    /// <param name="references">An optional References dictionary to carry through onto the diagnostic value.</param>
    /// <returns>The value-carrying failure result.</returns>
    public static Result<TaskGatewayResponseDto> FailAlways(
        GatewayFault fault,
        int machineId,
        IDictionary<string, Register>? references = null)
    {
        ArgumentNullException.ThrowIfNull(fault);

        return FailAlways(fault.Message, machineId, fault.Code, references);
    }

    /// <summary>
    /// Pure diagnostic-DTO builder mirroring <c>UpdateCyclesCommandHandler.BuildFailureDto</c> verbatim: selects
    /// <paramref name="explicitCode"/> when supplied, otherwise classifies <paramref name="firstError"/> via
    /// <see cref="PlcFailureDiagnostics.Classify"/>, then builds the value through
    /// <see cref="PlcFailureDiagnostics.BuildDiagnosticResponse"/> (which stamps the
    /// <c>References["ResultValidation"]</c> register that survives the publish path).
    /// </summary>
    /// <param name="firstError">The first failure message (classification input when no explicit code is given).</param>
    /// <param name="machineId">The originating machine id for the diagnostic value.</param>
    /// <param name="explicitCode">The precise code when the caller already knows it.</param>
    /// <param name="references">An optional References dictionary to reuse on the diagnostic value.</param>
    /// <returns>The diagnostic §7 response carrying the specific negative code.</returns>
    public static TaskGatewayResponseDto BuildFailureDto(
        string? firstError,
        int machineId,
        ResultValidation? explicitCode = null,
        IDictionary<string, Register>? references = null)
    {
        var code = explicitCode ?? PlcFailureDiagnostics.Classify(firstError);
        return PlcFailureDiagnostics.BuildDiagnosticResponse(code, machineId, references);
    }
}
