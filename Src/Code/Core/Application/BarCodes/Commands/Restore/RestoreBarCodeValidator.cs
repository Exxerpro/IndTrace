// <copyright file="RestoreBarCodeValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Commands.Restore;

internal class RestoreBarCodeValidator : AbstractValidator<RestoreBarCodeCommand>
{
    public RestoreBarCodeValidator()
    {
        this.RuleFor(v => v.Label)
            .NotNull();
    }
}