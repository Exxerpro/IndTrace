// <copyright file="GetOeeHistoryQuery.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Oee.Queries;

using IndTrace.Application.Constants;

/// <summary>
/// QueryAsync to retrieve OEE history for machines within a date range.
/// </summary>
public record GetOeeHistoryQuery
{
    /// <summary>
    /// Gets the machine identifier (optional, null means all machines).
    /// </summary>
    public int? MachineId { get; init; }

    /// <summary>
    /// Gets the start date for the history range.
    /// </summary>
    public DateTime StartDate { get; init; }

    /// <summary>
    /// Gets the end date for the history range.
    /// </summary>
    public DateTime EndDate { get; init; }

    /// <summary>
    /// Gets the minimum OEE performance level filter (optional).
    /// </summary>
    public OeePerformanceLevel? MinPerformanceLevel { get; init; }

    /// <summary>
    /// Gets the page number for pagination (1-based).
    /// </summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Gets the page size for pagination.
    /// </summary>
    public int PageSize { get; init; } = 50;
}

/// <summary>
/// Represents a single OEE history record.
/// </summary>
public record OeeHistoryRecord
{
    /// <summary>
    /// Gets the machine identifier.
    /// </summary>
    public int MachineId { get; init; }

    /// <summary>
    /// Gets the machine name.
    /// </summary>
    public string MachineName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the OEE metrics.
    /// </summary>
    public OeeMetrics Metrics { get; init; } = new(0, 0, 0);

    /// <summary>
    /// Gets the calculation timestamp.
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// Gets the shift information (if applicable).
    /// </summary>
    public string? Shift { get; init; }

    /// <summary>
    /// Gets the product being manufactured.
    /// </summary>
    public string? Product { get; init; }
}

/// <summary>
/// Response model for OEE history query.
/// </summary>
public record GetOeeHistoryResponse
{
    /// <summary>
    /// Gets the OEE history records.
    /// </summary>
    public IEnumerable<OeeHistoryRecord> Records { get; init; } = Enumerable.Empty<OeeHistoryRecord>();

    /// <summary>
    /// Gets the total count of records.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Gets the current page number.
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    /// Gets the page size.
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)this.TotalCount / this.PageSize);

    /// <summary>
    /// Gets a value indicating whether gets whether there is a next page.
    /// </summary>
    public bool HasNextPage => this.PageNumber < this.TotalPages;

    /// <summary>
    /// Gets a value indicating whether gets whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage => this.PageNumber > 1;
}

/// <summary>
/// Validator for the GetOeeHistoryQuery.
/// </summary>
public class GetOeeHistoryQueryValidator : AbstractValidator<GetOeeHistoryQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetOeeHistoryQueryValidator"/> class.
    /// Initializes a new instance of the GetOeeHistoryQueryValidator.
    /// </summary>
    public GetOeeHistoryQueryValidator()
    {
        this.RuleFor(x => x.MachineId)
            .GreaterThan(0)
            .When(x => x.MachineId.HasValue)
            .WithMessage(ValidationConstants.MachineIdRequired);

        this.RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage(ValidationConstants.StartDateRequired);

        this.RuleFor(x => x.EndDate)
            .NotEmpty()
            .WithMessage(ValidationConstants.EndDateRequired);

        this.RuleFor(x => x)
            .Must(x => x.EndDate >= x.StartDate)
            .WithMessage(ValidationConstants.EndDateMustBeAfterStartDate);

        this.RuleFor(x => x)
            .Must(x => x.EndDate.Subtract(x.StartDate).TotalDays <= 365)
            .WithMessage(ValidationConstants.DateRangeTooLarge);

        this.RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage(ValidationConstants.PageNumberInvalid);

        this.RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 1000)
            .WithMessage(ValidationConstants.PageSizeInvalid);
    }
}