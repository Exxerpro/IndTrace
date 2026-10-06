// <copyright file="GetBarCodeDetailGatewayQueryHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Queries.GetBarCodeGatewayDetail;

/// <summary>
/// Represents the GetBarCodeDetailGatewayQueryHandler.
/// </summary>
/// <remarks>
/// Issue #33 (Chunk 3) — cut off the mutable god-object <c>IBarCodeResult.GetBarCodeDetails</c> onto the stateless
/// <see cref="IBarCodeDetailsLoader"/> + immutable <see cref="BarCodeSnapshot"/> + pure
/// <see cref="BarCodeResultProjection"/>. The §7 PLC surface is preserved byte-identically: the loader returns
/// <c>Success(snapshot)</c> even on a validation failure (the snapshot carries the specific negative
/// <c>ResultValidation</c> code), so the <c>WithFailure</c>-vs-<c>Success</c> decision keeps gating on the
/// snapshot's <c>Error</c> length, exactly as the god-object path did — NOT on <c>Result.IsSuccess</c>.
/// </remarks>
public class GetBarCodeDetailGatewayQueryHandler(IBarCodeDetailsLoader barCodeDetailsLoader) :
    IGatewayRequestHandler<ReadBarCodeQuery, TaskGatewayResponseDto>, IResettable
{
    /// <inheritdoc/>
    public async Task<Result<TaskGatewayResponseDto>> ProcessAsync(ReadBarCodeQuery cmd, CancellationToken cancellationToken)
    {
        var request = cmd.Command;

        BarCodeDetailsRequest barCodeDetailsRequest =
            new BarCodeDetailsRequest(
                request.MachineId,
                request.BarCode,
                request.PartNumber);

        var loadResult = await barCodeDetailsLoader.LoadAsync(
            barCodeDetailsRequest,
            cancellationToken).ConfigureAwait(false);

        // Only a null/exception load (never a validation failure) yields a failed Result — surface it as such.
        if (loadResult.IsFailure || loadResult.Value is null)
        {
            return Result<TaskGatewayResponseDto>.WithFailure(loadResult.Errors);
        }

        var snapshot = loadResult.Value;
        var result = BarCodeResultProjection.ToResponse(snapshot);

        // #32 C2: stamp the §7 References via the pure ReferenceStamper (replaces the retired instance method).
        var stamped = ReferenceStamper.Apply(result);
        if (stamped.Value is not null)
        {
            result = stamped.Value;
        }

        // §7 byte-identity: keep the EXACT wrapper decision the god-object path made — a non-empty loaded Error
        // (carrying the negative ResultValidation) becomes WithFailure(error, response), else Success(response).
        if (snapshot.Error is not null && snapshot.Error.Length > 0)
        {
            return Result<TaskGatewayResponseDto>.WithFailure(snapshot.Error, result);
        }

        // [Fix]
        // CLAUDE
        // Date: 22/08/2025
        // Reason: [CLUSTER A - MASSIVE FIX] - Return Result<T>.Success() for Railway-Oriented Programming pattern
        return Result<TaskGatewayResponseDto>.Success(result);
    }

    /// <inheritdoc/>
    public bool TryReset()
    {
        return true;
    }
}