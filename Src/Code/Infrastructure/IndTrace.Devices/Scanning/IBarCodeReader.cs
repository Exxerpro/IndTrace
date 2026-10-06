// <copyright file="IBarCodeReader.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Devices.Scanning;

using System;

/// <summary>
/// Vendor-neutral abstraction over a barcode reader, exposing only the members the scan
/// consumers (background worker, final-report page) use. A reader driver supplies the
/// implementation; the community edition registers <see cref="NullBarCodeReader"/>.
/// </summary>
public interface IBarCodeReader
{
    /// <summary>
    /// Gets an observable that emits the reader each time a barcode is scanned.
    /// </summary>
    IObservable<IBarCodeReader> BarCode { get; }

    /// <summary>
    /// Gets a value indicating whether the reader is currently connected.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Gets the most recently decoded barcode string.
    /// </summary>
    string? Result { get; }

    /// <summary>
    /// Starts discovery/connection to the configured reader. Implementations must not throw.
    /// </summary>
    void Connect();
}
