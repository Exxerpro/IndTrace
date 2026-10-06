// <copyright file="GetSettingDetailQueryValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Settings.Queries.GetSettingDetail;

/// <summary>
/// Represents the GetSettingDetailQueryValidator.
/// </summary>
public class GetSettingDetailQueryValidator : AbstractValidator<GetSettingDetailQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetSettingDetailQueryValidator"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public GetSettingDetailQueryValidator()
    {
        this.RuleFor(v => v.SettingId)
            .GreaterThan(0)
            .WithMessage("SettingId must be greater than 0.");
    }
}