// <copyright file="IBarCodeDetailsLoader.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Services;

/// <summary>
/// Stateless loader that runs the bar code detail fetch pipeline and returns a fresh immutable
/// <see cref="BarCodeSnapshot"/> per call. Issue #33 (Chunk 2) — the replacement for the mutable god-object
/// <c>IBarCodeResult.GetBarCodeDetails</c> protocol, mirroring the shipped
/// <see cref="IndTrace.Application.Cycles.Services.Interfaces.IBarCodeInfoProvider.GetCycleUpdateLoadStateAsync"/>
/// precedent. Because it holds no per-call state and hands back only an immutable snapshot, it is safe at any DI
/// lifetime and structurally removes the captive-dependency concurrency defect.
/// </summary>
public interface IBarCodeDetailsLoader
{
    /// <summary>
    /// Loads an immutable <see cref="BarCodeSnapshot"/> for the given request.
    /// </summary>
    /// <remarks>
    /// A <b>validation</b> failure (e.g. machine-not-found, workflow-not-valid) returns
    /// <see cref="Result{T}.Success(T)"/> with the snapshot carrying the specific negative
    /// <see cref="BarCodeSnapshot.ResultValidation"/> code — the §7 PLC contract requires that code to survive to
    /// the projection. Only a null or exception load becomes a <see cref="Result{T}.WithFailure(string)"/>.
    /// </remarks>
    /// <param name="request">The bar code details request (machine, label, part number).</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A task producing the load result: the snapshot on success, a failure for null/exception loads.</returns>
    Task<Result<BarCodeSnapshot>> LoadAsync(BarCodeDetailsRequest request, CancellationToken cancellationToken);
}
