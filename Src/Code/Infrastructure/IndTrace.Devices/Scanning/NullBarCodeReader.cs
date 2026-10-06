// <copyright file="NullBarCodeReader.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Devices.Scanning;

/// <summary>
/// The community-edition <see cref="IBarCodeReader"/>: no reader hardware is attached, so it never connects
/// and never emits a scan. Pages that consume scans stay usable through manual barcode entry.
/// </summary>
public sealed class NullBarCodeReader : IBarCodeReader
{
    /// <inheritdoc/>
    public IObservable<IBarCodeReader> BarCode { get; } = Observable.Never<IBarCodeReader>();

    /// <inheritdoc/>
    public bool IsConnected => false;

    /// <inheritdoc/>
    public string? Result => null;

    /// <inheritdoc/>
    public void Connect()
    {
    }
}
