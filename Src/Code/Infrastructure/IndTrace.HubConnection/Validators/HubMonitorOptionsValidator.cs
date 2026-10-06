// <copyright file="HubMonitorOptionsValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.HubConnection.Validators;

using IndTrace.Application.Models.Services;
using Microsoft.Extensions.Options;

/// <summary>
/// Validates HubMonitorOptions for correctness.
/// </summary>
public sealed class HubMonitorOptionsValidator : IValidateOptions<HubMonitorOptions>
{
    public ValidateOptionsResult Validate(string? name, HubMonitorOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Url))
            return ValidateOptionsResult.Fail("HubMonitorOptions: Url is required.");

        if (!Uri.TryCreate(options.Url, UriKind.Absolute, out _))
            return ValidateOptionsResult.Fail("HubMonitorOptions: Url must be a valid absolute URI.");

        if (options.RetryTime <= 0)
            return ValidateOptionsResult.Fail("HubMonitorOptions: RetryTime must be greater than 0.");

        return ValidateOptionsResult.Success;
    }
}

