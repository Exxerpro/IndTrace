// <copyright file="GetBarCodeListQueryValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Queries.GetBarCodeList;

/// <summary>
/// Validator for <see cref="GetBarCodesListQuery"/> that ensures barcode list queries have valid date range parameters.
/// </summary>
/// <remarks>
/// This validator enforces that barcode queries must specify both start and end dates, which is essential
/// for performance optimization and preventing unbounded queries against the production tracking database.
/// Date-based filtering is critical for managing large datasets in manufacturing environments.
/// </remarks>
public class GetBarCodeListQueryValidator : AbstractValidator<GetBarCodesListQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetBarCodeListQueryValidator"/> class
    /// and configures validation rules for barcode list queries.
    /// </summary>
    /// <param name="dateTimeMachine">The deterministic time source for the "not in the future" cutoff. Issue #85:
    /// the cutoff is read inside the rule (at validation time) via the injected clock, not frozen at
    /// construction as the previous <c>DateTime.Now.AddDays(1)</c> did.</param>
    public GetBarCodeListQueryValidator(IDateTimeMachine dateTimeMachine)
    {
        ArgumentNullException.ThrowIfNull(dateTimeMachine);

        // [Fix]
        // CLAUDE
        // Date: 20/08/2025
        // Reason: Added comprehensive date validation - validator was too simple, only checking NotNull but missing business rules for date ranges and valid date constraints
        this.RuleFor(v => v.StartDate)
            .NotNull()
            .WithMessage("Start date cannot be null.")
            .Must(date => date != default(DateTime))
            .WithMessage("Start date must be a valid date.")
            .Must(date => date < dateTimeMachine.Now.AddDays(1))
            .WithMessage("Start date cannot be in the future.");

        this.RuleFor(v => v.EndDate)
            .NotNull()
            .WithMessage("End date cannot be null.")
            .Must(date => date != default(DateTime))
            .WithMessage("End date must be a valid date.")
            .Must(date => date < dateTimeMachine.Now.AddDays(1))
            .WithMessage("End date cannot be in the future.");

        this.RuleFor(v => v)
            .Must(query => query.EndDate >= query.StartDate)
            .WithMessage("End date must be greater than or equal to start date.")
            .WithName("DateRange");

        this.RuleFor(v => v)
            .Must(query => (query.EndDate - query.StartDate).TotalDays <= 365)
            .WithMessage("Date range cannot exceed 365 days.")
            .WithName("DateRangeLimit");
    }
}