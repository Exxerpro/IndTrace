// <copyright file="GetBarCodeListQueryReportsValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Queries.GetReportsList.GetList;

/// <summary>
/// Represents the GetBarCodeListQueryReportsValidator.
/// </summary>
public class GetBarCodeListQueryReportsValidator : AbstractValidator<GetReportsListQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetBarCodeListQueryReportsValidator"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="dateTimeMachine">The deterministic time source for the "not in the future" cutoff (issue #85):
    /// evaluated inside the rule at validation time, never frozen at construction.</param>
    public GetBarCodeListQueryReportsValidator(IDateTimeMachine dateTimeMachine)
    {
        ArgumentNullException.ThrowIfNull(dateTimeMachine);

        // [Fix]
        // CLAUDE
        // Date: 20/08/2025
        // Reason: Enhanced validation for report list queries with comprehensive date and filter validation rules
        // [Fix]
        // CLAUDE
        // Date: 21/08/2025
        // Reason: Fixed DateTime validation - replaced NotNull() with GreaterThan(DateTime.MinValue) since DateTime is non-nullable struct

        // Date validation
        this.RuleFor(v => v.StartDate)
            .GreaterThan(DateTime.MinValue)
            .WithMessage("Start date cannot be empty.")
            .Must(date => date < dateTimeMachine.Now.AddDays(1))
            .WithMessage("Start date cannot be in the future.");

        this.RuleFor(v => v.EndDate)
            .GreaterThan(DateTime.MinValue)
            .WithMessage("End date cannot be empty.")
            .Must(date => date < dateTimeMachine.Now.AddDays(1))
            .WithMessage("End date cannot be in the future.");

        // Date range validation
        this.RuleFor(v => v)
            .Must(query => query.EndDate >= query.StartDate)
            .WithMessage("End date must be greater than or equal to start date.")
            .WithName("DateRange");

        // Conditional validations for filters
        this.When(v => v.FilterByLine, () =>
        {
            this.RuleFor(v => v.Line)
                .NotNull()
                .NotEmpty()
                .Length(1, 50)
                .WithMessage("Line must be between 1 and 50 characters when filter is enabled.");
        });

        this.When(v => v.FilterByRegister, () =>
        {
            this.RuleFor(v => v.RegisterSearch)
                .NotNull()
                .NotEmpty()
                .Length(1, 100)
                .WithMessage("Register search must be between 1 and 100 characters when filter is enabled.");
        });

        this.When(v => v.FilterByCustomer, () =>
        {
            this.RuleFor(v => v.CustomerSearch)
                .NotNull()
                .NotEmpty()
                .Length(1, 100)
                .WithMessage("Customer search must be between 1 and 100 characters when filter is enabled.");
        });

        this.When(v => v.FilterByProduct, () =>
        {
            this.RuleFor(v => v.Model)
                .NotNull()
                .NotEmpty()
                .Length(1, 100)
                .WithMessage("Model must be between 1 and 100 characters when filter is enabled.");
        });

        this.When(v => v.FilterByShift, () =>
        {
            this.RuleFor(v => v.Shift)
                .InclusiveBetween(1, 3)
                .WithMessage("Shift must be between 1 and 3 when filter is enabled.");
        });

        this.When(v => v.FilterByState, () =>
        {
            this.RuleFor(v => v.State)
                .NotNull()
                .NotEmpty()
                .Length(1, 50)
                .WithMessage("State must be between 1 and 50 characters when filter is enabled.");
        });
    }
}