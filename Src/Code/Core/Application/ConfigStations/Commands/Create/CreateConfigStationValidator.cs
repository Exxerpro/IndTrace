// <copyright file="CreateConfigStationValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.ConfigStations.Commands.Create;

/// <summary>
/// Represents the CreateConfigStationValidator.
/// </summary>
public class CreateConfigStationValidator : AbstractValidator<CreateConfigStationCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateConfigStationValidator"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public CreateConfigStationValidator()
    {
        this.RuleFor(v => v.ConfigId).NotEmpty().Length(10);
        this.RuleFor(v => v.MachineId).GreaterThan(0);
        this.RuleFor(v => v.Pc).GreaterThan(0);
        this.RuleFor(v => v.Client).NotEmpty().MaximumLength(100);
        this.RuleFor(v => v.Factorie).NotEmpty().MaximumLength(100);
        this.RuleFor(v => v.Line).NotEmpty().MaximumLength(100);
        this.RuleFor(v => v.Project).NotEmpty().MaximumLength(100);
        this.RuleFor(v => v.Version).NotEmpty().MaximumLength(50);
    }
}