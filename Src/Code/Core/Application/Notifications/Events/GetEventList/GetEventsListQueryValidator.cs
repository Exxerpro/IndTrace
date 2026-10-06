// <copyright file="GetEventsListQueryValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Notifications.Events.GetEventList;

/// <summary>
/// Represents the GetEventsListQueryValidator.
/// </summary>
public class GetEventsListQueryValidator : AbstractValidator<GetEventsListQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetEventsListQueryValidator"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="dateTimeMachine">The deterministic time source for the "not in the future" cutoff (issue #85):
    /// evaluated inside the rule at validation time, never frozen at construction.</param>
    public GetEventsListQueryValidator(IDateTimeMachine dateTimeMachine)
    {
        ArgumentNullException.ThrowIfNull(dateTimeMachine);

        // [Fix]
        // CLAUDE
        // Date: 20/08/2025
        // Reason: Fixed duplicate validation rules and syntax errors - validator had duplicate NotNull rules and incorrect error message for EndDate. Improved pagination validation constraints.

        // Validate date range
        this.RuleFor(v => v.StartDate)
            .NotNull()
            .WithMessage("Start date is required.")
            .GreaterThan(DateTime.Parse("2020-01-01"))
            .WithMessage("Start date must be after 2020-01-01.")
            .Must(date => date < dateTimeMachine.Now.AddDays(1))
            .WithMessage("Start date cannot be in the future.");

        this.RuleFor(v => v.EndDate)
            .NotNull()
            .WithMessage("End date is required.")
            .GreaterThan(DateTime.Parse("2020-01-01"))
            .WithMessage("End date must be after 2020-01-01.")
            .Must(date => date < dateTimeMachine.Now.AddDays(1))
            .WithMessage("End date cannot be in the future.");

        this.RuleFor(v => v.StartDate)
            .LessThanOrEqualTo(v => v.EndDate)
            .WithMessage("Start date must be less than or equal to end date.");

        // Validate pagination parameters
        this.RuleFor(v => v.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0.")
            .LessThan(10_000_000)
            .WithMessage("Page number must be less than 10,000,000.");

        this.RuleFor(v => v.PageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than 0.")
            .LessThan(1000)
            .WithMessage("Page size must be less than 1000.");

        // Date range constraint
        this.RuleFor(v => v)
            .Must(query => (query.EndDate - query.StartDate).TotalDays <= 365)
            .WithMessage("Date range cannot exceed 365 days.")
            .WithName("DateRangeLimit");
    }
}