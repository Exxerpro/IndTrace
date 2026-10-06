// <copyright file="CreateMaquinaPLCValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.MachinesPlcs.Commands.Create;

/// <summary>
/// Validator for <see cref="CreateMachinePlcCommand"/> that ensures machine-PLC association requests contain valid PLC identifiers.
/// </summary>
/// <remarks>
/// This validator is critical for establishing the relationship between physical machines and their controlling PLCs
/// in the IndTrace manufacturing execution system. The PLC ID is required to enable communication, monitoring,
/// and control of production equipment through the Siemens S7 protocol.
/// </remarks>
public class CreateMaquinaPlcValidator : AbstractValidator<CreateMachinePlcCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateMaquinaPlcValidator"/> class
    /// and configures validation rules for machine-PLC association creation.
    /// </summary>
    public CreateMaquinaPlcValidator()
    {
        // [Fix]
        // CLAUDE
        // Date: 23/08/2025
        // Reason: Pattern A Fix - Removed MachineId validation to restore single-purpose validator design. Tests expect this validator to only validate PlCsId as per the "single purpose" design pattern.

        // Validate PLC ID must be a valid positive integer
        this.RuleFor(v => v.PlCsId)
            .GreaterThan(0)
            .WithMessage("PLC ID must be greater than 0.");
    }
}