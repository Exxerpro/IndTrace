// <copyright file="UpdateCycleCommandAdapter.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Cycles.Commands.UpdateCycles;

/// <summary>
/// Adapter to make legacy command objects compatible with new interfaces.
/// </summary>
public class UpdateCycleCommandAdapter : IUpdateCycleCommand
{
    private readonly TaskGatewayRequest _legacyCommand;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateCycleCommandAdapter"/> class from OK command.
    /// </summary>
    /// <param name="command">The OK command.</param>
    public UpdateCycleCommandAdapter(UpdateCyclesOkCommand command)
    {
        _legacyCommand = command?.Command ?? throw new ArgumentNullException(nameof(command));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateCycleCommandAdapter"/> class from NOT OK command.
    /// </summary>
    /// <param name="command">The NOT OK command.</param>
    public UpdateCycleCommandAdapter(UpdateCyclesNotOkCommand command)
    {
        _legacyCommand = command?.Command ?? throw new ArgumentNullException(nameof(command));
    }

    /// <inheritdoc/>
    public int MachineId => _legacyCommand.MachineId;

    /// <inheritdoc/>
    public string BarCode => _legacyCommand.BarCode;

    /// <inheritdoc/>
    public string PartNumber => _legacyCommand.PartNumber;

    /// <inheritdoc/>
    public PartStatus PartStatus => _legacyCommand.PartStatus;

    /// <inheritdoc/>
    public CycleStatus CycleStatus => _legacyCommand.CycleStatus;

    /// <inheritdoc/>
    public IDictionary<string, Register> Registers => _legacyCommand.Registers;
}