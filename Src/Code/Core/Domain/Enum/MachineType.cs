// <copyright file="MachineType.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Enum;

/// <summary>
/// Represents the type of machine in the system, such as Printer, Initial, ProcessAsync, Final, etc.
/// </summary>
public class MachineType : EnumModel
{
    /// <summary>
    /// Represents an invalid machine type.
    /// </summary>
    public static readonly MachineType Invalid
        = new(-1, "Invalid Value");

    /// <summary>
    /// Represents no machine type.
    /// </summary>
    public static readonly MachineType None
        = new(0, "None");

    /// <summary>
    /// Represents a printer machine type.
    /// </summary>
    public static readonly MachineType Printer
        = new(1, "Printer");

    /// <summary>
    /// Represents an initial machine type.
    /// </summary>
    public static readonly MachineType Initial
        = new(2, "Initial");

    /// <summary>
    /// Represents an initial printer machine type.
    /// </summary>
    public static readonly MachineType InitialPrinter
        = new(4, "InitialPrinter");

    /// <summary>
    /// Represents a process machine type.
    /// </summary>
    public static readonly MachineType Process
        = new(8, "Process");

    /// <summary>
    /// Represents a final machine type.
    /// </summary>
    public static readonly MachineType Final
        = new(16, "Final");

    /// <summary>
    /// Represents an inspection machine type.
    /// </summary>
    public static readonly MachineType Inspection
        = new(32, "Inspection");

    /// <summary>
    /// Represents a dashboard machine type.
    /// </summary>
    public static readonly MachineType DashBoard
        = new(64, "DashBoard");

    /// <summary>
    /// Represents a disposable machine type for testing enum converter issues.
    /// </summary>
    /// <remarks>
    /// #98 (Option B): a test-only converter sentinel — value 1024 has no matching row in the QA45
    /// <c>MachineType</c> lookup seed (1..64) and is never a real, persisted machine type. Kept for the converter
    /// test; documented on the #98 allow-list in <c>EnumLookupSeedParityOnRealSqlTests</c>.
    /// </remarks>
    public static readonly MachineType Disposable
        = new(1024, "Disposable");

    /// <summary>
    /// Initializes a new instance of the <see cref="MachineType"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public MachineType()
    {
    }

    private MachineType(int value, string name, string? displayName = null)
        : base(value, name, displayName ?? string.Empty)
    {
    }

    public static implicit operator int(MachineType enumerator) => enumerator.Value;

    public static implicit operator string(MachineType enumerator) => enumerator.Value.ToString();

    public static implicit operator MachineType(int value) => FromValue<MachineType>(value);
}
