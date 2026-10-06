// <copyright file="IIndTraceControllerRx.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Devices.Plc;

using IndTrace.Application.Performance.Request.Command.Create;

/// <summary>
/// Defines the vendor-neutral contract for a reactive PLC controller. The gateway depends only on this
/// contract; a PLC driver (e.g. the Siemens S7 driver) supplies the implementation through
/// <see cref="IPlcControllerFactory"/>.
/// </summary>
public interface IIndTraceControllerRx
{
    // TODO: [IMPORTANT]  PASS A CANCELATION TOKEN TO ALL ASYNC TASK

    /// <summary>
    /// Gets the PLC details configuration.
    /// </summary>
    PlcDto PlcDetails { get; }

    /// <summary>
    /// Gets a value indicating whether OEE (Overall Equipment Effectiveness) is enabled.
    /// </summary>
    bool IsOeeEnabled { get; }

    /// <summary>
    /// Gets the current barcode value.
    /// </summary>
    string BarCode { get; }

    /// <summary>
    /// Gets the current command value.
    /// </summary>
    short Command { get; }

    /// <summary>
    /// Gets an observable that emits when the command value changes.
    /// </summary>
    IObservable<IIndTraceControllerRx> CommandChanged { get; }

    /// <summary>
    /// Gets an observable that emits when the heartbeat value changes.
    /// </summary>
    IObservable<IIndTraceControllerRx> HeartBeatChanged { get; }

    /// <summary>
    /// Gets a value indicating whether the controller is connected.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Gets a value indicating whether the controller is initialized.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Gets the machine identifier.
    /// </summary>
    int MachineId { get; }

    /// <summary>
    /// Gets the part number.
    /// </summary>
    string PartNumber { get; }

    /// <summary>
    /// Gets a value indicating whether the controller is configured.
    /// </summary>
    bool Configured { get; }

    /// <summary>
    /// Gets the current heartbeat value.
    /// </summary>
    short HeartBeat { get; }

    /// <summary>
    /// Gets the current part status.
    /// </summary>
    PartStatus PartStatus { get; }

    /// <summary>
    /// Gets the current cycle status.
    /// </summary>
    CycleStatus CycleStatus { get; }

    /// <summary>
    /// Gets the controller name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the PLC identifier.
    /// </summary>
    int PlcId { get; }

    /// <summary>
    /// Gets the number of successful cycles.
    /// </summary>
    int CyclesOk { get; }

    /// <summary>
    /// Gets or sets the references dictionary.
    /// </summary>
    IDictionary<string, Register> References { get; set; }

    /// <summary>
    /// Gets or sets the registers dictionary.
    /// </summary>
    IDictionary<string, Register> Registers { get; set; }

    /// <summary>
    /// Disposes of the controller resources.
    /// </summary>
    void Dispose();

    /// <summary>
    /// Returns a string representation of the controller.
    /// </summary>
    /// <returns>A string that represents the controller.</returns>
    string ToString();

    /// <summary>
    /// Gets or sets a value indicating whether retry is enabled.
    /// </summary>
    bool Retry { get; set; }

    /// <summary>
    /// Downloads references in bulk asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result> DownloadReferencesBulkAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Validates that the specified tag exists on the controller.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation, returning true if the tag exists.</returns>
    Task<bool> ValidateThatTheTagExistOnTheController(CancellationToken cancellationToken);

    /// <summary>
    /// Gets the PLC monitor information asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation, containing the PLC monitor information.</returns>
    Task<ControllerMonitor> GetPlcMonitorAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Sets the feedback value asynchronously.
    /// </summary>
    /// <param name="value">The feedback value to set.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation, returning the result as a string.</returns>
    Task<string> SetFeedBackAsync(short value, CancellationToken cancellationToken);

    /// <summary>
    /// Sets the PLC ID asynchronously.
    /// </summary>
    /// <param name="value">The PLC ID value to set.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation, returning the result as an integer.</returns>
    Task<int> SetPlcIdAsync(short value, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the PLC ID asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation, returning the PLC ID.</returns>
    Task<int> GetPlcIdAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Reads a short tag value asynchronously.
    /// </summary>
    /// <param name="tagName">The name of the tag to read.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation, returning the short value.</returns>
    Task<short> ReadShortTagAsync(string tagName, CancellationToken cancellationToken);

    /// <summary>
    /// Reads a string tag value asynchronously.
    /// </summary>
    /// <param name="tagName">The name of the tag to read.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation, returning the string value.</returns>
    Task<string> ReadStringTagAsync(string tagName, CancellationToken cancellationToken);

    /// <summary>
    /// Resets the command asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation, returning the result as a string.</returns>
    Task<string> ResetCommandAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Simulates a command asynchronously.
    /// </summary>
    /// <param name="command">The simulated command to execute.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation, returning the result as a string.</returns>
    Task<string> SimulateCommandAsync(SimulatedCommand command, CancellationToken cancellationToken);

    /// <summary>
    /// Sets the barcode value asynchronously.
    /// </summary>
    /// <param name="value">The barcode value to set.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation, returning the result as a string.</returns>
    Task<string> SetBarCodeAsync(string value, CancellationToken cancellationToken);

    /// <summary>
    /// Sets up the controller asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation, returning true if successful.</returns>
    Task<bool> SetUpAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Reads the startup configuration asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation, containing the startup command.</returns>
    Task<SimulatedCommand> ReadStartUp(CancellationToken cancellationToken);

    /// <summary>
    /// Connects to the controller and creates notifications asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation, returning true if successful.</returns>
    Task<bool> ConnectAndCreateNotificationsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Reads registers in bulk asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result> ReadRegistersBulkAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Uploads command data from the controller asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation, containing the data from PLC.</returns>
    Task<Result<DataFromPlc>> UploadCommandDataFromController(CancellationToken cancellationToken);

    /// <summary>
    /// Reads performance data from the PLC asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation, containing the performance data command.</returns>
    Task<Result<PerformanceDataCommand>> ReadPerformanceDataFromPlcAsync(CancellationToken cancellationToken);
}

