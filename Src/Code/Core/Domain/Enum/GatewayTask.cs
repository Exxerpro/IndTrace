// <copyright file="GatewayTask.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Enum;

/// <summary>
/// Represents a gateway task in the system, such as creating barcodes, reading barcodes, creating cycles,
/// updating cycles, and process-related tasks.
/// <para>
/// Story 4.3 — SINGLE SOURCE OF TRUTH. This is the ONLY <c>GatewayTask</c> definition in the solution; the
/// former <c>IndTrace.Simulator.Validation.GatewayTask</c> C# <c>enum</c> was deleted and the Simulator now
/// references this Domain smart-enum. The PLC alphabet (<c>4 … 256</c>) is FROZEN (hardware contract,
/// analysis §7). Off-PLC-bus members are grouped in the regions below.
/// </para>
/// </summary>
public class GatewayTask : EnumModel
{
    /// <summary>
    /// Represents an invalid gateway task.
    /// </summary>
    public static readonly GatewayTask Invalid
        = new(-1, "Invalid Value");

    /// <summary>
    /// Represents no gateway task.
    /// </summary>
    public static readonly GatewayTask None
        = new(0, "None");

    // ============================================================================
    // SIMULATOR-ONLY region (OFF the PLC bus). Story 4.3 — merged in from the deleted
    // IndTrace.Simulator.Validation.GatewayTask enum. Values 1/2 are NOT part of the PLC
    // command alphabet; they are read-only info probes used only by the Simulator.
    // ============================================================================

    /// <summary>
    /// Simulator-only (off the PLC bus): read application information asynchronously.
    /// </summary>
    public static readonly GatewayTask GetReadAppInfoAsync
        = new(1, "GetReadAppInfoAsync");

    /// <summary>
    /// Simulator-only (off the PLC bus): read stations information asynchronously.
    /// </summary>
    public static readonly GatewayTask GetReadStationsInfoAsync
        = new(2, "GetReadStationsInfoAsync");

    // ============================================================================
    // PLC-BUS alphabet (4 … 256). FROZEN hardware contract (analysis §7) — DO NOT renumber.
    // ============================================================================

    /// <summary>
    /// Represents a task to create a barcode asynchronously.
    /// </summary>
    public static readonly GatewayTask CreateBarCodeAsync
        = new(4, "CreateBarCodeAsync");

    /// <summary>
    /// Represents a task to read a barcode asynchronously.
    /// </summary>
    public static readonly GatewayTask ReadBarCodeAsync
        = new(8, "ReadBarCodeAsync");

    /// <summary>
    /// Represents a task to create a cycle asynchronously.
    /// </summary>
    public static readonly GatewayTask CreateCycleAsync
        = new(16, "CreateCycleAsync");

    /// <summary>
    /// Represents a task to update a cycle as OK asynchronously.
    /// </summary>
    public static readonly GatewayTask UpdateCycleOkAsync
        = new(32, "UpdateCycleOkAsync");

    /// <summary>
    /// Represents a task to update a cycle as Not OK asynchronously.
    /// </summary>
    public static readonly GatewayTask UpdateCycleNotOkAsync
        = new(64, "UpdateCycleNotOkAsync");

    /// <summary>
    /// Represents a task to end the process asynchronously.
    /// </summary>
    public static readonly GatewayTask EndOfProcessAsync
        = new(128, "EndOfProcessAsync", "RejectPartAsyncMonitor");

    /// <summary>
    /// Represents a task to reject a part asynchronously. Value 256 is the PLC-bus reject (FROZEN).
    /// </summary>
    public static readonly GatewayTask RejectPartAsync
        = new(256, "RejectPartAsync", "RejectPartAsyncMonitor");

    // ============================================================================
    // OFF-PLC-BUS monitor/domain triggers. NOT serialized to the PLC.
    // ============================================================================

    /// <summary>
    /// Represents a task to restore a part asynchronously (monitor/webapp path).
    /// <para>
    /// Story 4.3 — RENUMBERED 512 → 1024 to remove the cross-type "mirror" collision with the old Simulator
    /// enum (where 512 meant <c>RejectPartAsync</c>). Restore is monitor-path and was always OFF the PLC bus,
    /// so the renumber changes no hardware contract. Value 1024 was reserved for this by Story 4.1.
    /// </para>
    /// </summary>
    public static readonly GatewayTask RestorePartAsync
        = new(1024, "RestorePartAsync");

    /// <summary>
    /// Story 4.1 — domain-internal trigger that marks an item <see cref="FlowStatus.Invalid"/> when the
    /// completeness gate is ON. Value 2048 is OFF the PLC bus (the PLC alphabet ends at RejectPartAsync = 256);
    /// it is never serialized to the PLC. Story 4.3 will unify the trigger enum.
    /// </summary>
    public static readonly GatewayTask MarkInvalid
        = new(2048, "MarkInvalid");

    /// <summary>
    /// Story 4.1 — domain-internal trigger that sets <see cref="PartStatus.Scrap"/> when the completeness gate
    /// is ON. Value 4096 is OFF the PLC bus; it is never serialized to the PLC. Story 4.3 will unify the
    /// trigger enum.
    /// </summary>
    public static readonly GatewayTask MarkScrap
        = new(4096, "MarkScrap");

    /// <summary>
    /// Story 4.1 — domain-internal trigger that sets <see cref="CycleStatus.Canceled"/> on a Started cycle when
    /// the completeness gate is ON. Value 8192 is OFF the PLC bus; it is never serialized to the PLC.
    /// Story 4.3 will unify the trigger enum.
    /// </summary>
    public static readonly GatewayTask Cancel
        = new(8192, "Cancel");

    /// <summary>
    /// Initializes a new instance of the <see cref="GatewayTask"/> class.
    /// </summary>
    public GatewayTask()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GatewayTask"/> class with specified values.
    /// </summary>
    /// <param name="value">The integer value.</param>
    /// <param name="name">The name.</param>
    /// <param name="displayName">The display name.</param>
    private GatewayTask(int value, string name, string? displayName = null)
        : base(value, name, displayName ?? string.Empty)
    {
    }

    /// <summary>
    /// Implicitly converts a GatewayTask to its integer value.
    /// </summary>
    /// <param name="enumerator">The enumerator to convert.</param>
    public static implicit operator int(GatewayTask enumerator) => enumerator.Value;

    /// <summary>
    /// Implicitly converts a GatewayTask to a nullable integer value.
    /// </summary>
    /// <param name="enumerator">The enumerator to convert.</param>
    public static implicit operator int?(GatewayTask enumerator) => enumerator.Value;

    /// <summary>
    /// Implicitly converts a GatewayTask to its string representation.
    /// </summary>
    /// <param name="enumerator">The enumerator to convert.</param>
    public static implicit operator string(GatewayTask enumerator) => enumerator.Value.ToString();

    /// <summary>
    /// Implicitly converts an integer value to a GatewayTask.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator GatewayTask(int value) => FromValue<GatewayTask>(value);

    /// <summary>
    /// Implicitly converts a nullable integer value to a GatewayTask.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator GatewayTask(int? value) => FromValue<GatewayTask>(value ?? 0);
}
