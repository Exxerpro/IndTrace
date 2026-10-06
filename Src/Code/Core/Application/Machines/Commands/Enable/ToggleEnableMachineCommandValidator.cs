// <copyright file="ToggleEnableMachineCommandValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Machines.Commands.Enable;

/// <summary>
/// Represents the ToggleEnableMachineCommandValidator.
/// </summary>
public class ToggleEnableMachineCommandValidator : AbstractValidator<ToggleEnableMachineCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ToggleEnableMachineCommandValidator"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public ToggleEnableMachineCommandValidator()
    {
        this.RuleFor(x => x.MachineId)
            .GreaterThan(0)
            .WithMessage("MachineId must be greater than 0.");

        // Validate that Enable and Disable have inverse values
        this.RuleFor(x => x)
            .Must(this.HaveInverseEnableAndDisable)
            .WithMessage("Enable and Disable properties must have inverse values.");
    }

    /// <summary>
    /// Validates that the Enable and Disable properties have inverse values.
    /// </summary>
    /// <param name="command">The monitorRequest containing the properties.</param>
    /// <returns>True if Enable and Disable are inverses, otherwise false.</returns>
    private bool HaveInverseEnableAndDisable(ToggleEnableMachineCommand command)
    {
        return command.Enable != command.Disable;
    }
}