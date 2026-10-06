// <copyright file="CalculateOeeCommand.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Oee.Commands;

using IndTrace.Application.Constants;

/// <summary>
/// Command to calculate OEE metrics based on operational data.
/// </summary>
public record CalculateOeeCommand
{
    /// <summary>
    /// Gets the machine identifier.
    /// </summary>
    public int MachineId { get; init; }

    /// <summary>
    /// Gets the total scheduled production time in minutes.
    /// </summary>
    public double TotalTimeMinutes { get; init; }

    /// <summary>
    /// Gets the total downtime in minutes.
    /// </summary>
    public double DowntimeMinutes { get; init; }

    /// <summary>
    /// Gets the ideal cycle time per unit in seconds.
    /// </summary>
    public double IdealCycleTimeSeconds { get; init; }

    /// <summary>
    /// Gets the total units produced.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Gets the number of defective units.
    /// </summary>
    public int DefectCount { get; init; }

    /// <summary>
    /// Gets the timestamp for the calculation.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Validator for the CalculateOeeCommand.
/// </summary>
public class CalculateOeeCommandValidator : AbstractValidator<CalculateOeeCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CalculateOeeCommandValidator"/> class.
    /// Initializes a new instance of the CalculateOeeCommandValidator.
    /// </summary>
    public CalculateOeeCommandValidator()
    {
        this.RuleFor(x => x.MachineId)
            .GreaterThan(0)
            .WithMessage(ValidationConstants.MachineIdRequired);

        this.RuleFor(x => x.TotalTimeMinutes)
            .GreaterThan(0)
            .WithMessage(ValidationConstants.TotalTimeRequired);

        this.RuleFor(x => x.DowntimeMinutes)
            .GreaterThanOrEqualTo(0)
            .WithMessage(ValidationConstants.DowntimeNonNegative);

        this.RuleFor(x => x.IdealCycleTimeSeconds)
            .GreaterThan(0)
            .WithMessage(ValidationConstants.IdealCycleTimeRequired);

        this.RuleFor(x => x.TotalCount)
            .GreaterThanOrEqualTo(0)
            .WithMessage(ValidationConstants.TotalCountNonNegative);

        this.RuleFor(x => x.DefectCount)
            .GreaterThanOrEqualTo(0)
            .WithMessage(ValidationConstants.DefectCountNonNegative);

        this.RuleFor(x => x)
            .Must(x => x.DowntimeMinutes <= x.TotalTimeMinutes)
            .WithMessage(ValidationConstants.DowntimeExceedsTotalTime);

        this.RuleFor(x => x)
            .Must(x => x.DefectCount <= x.TotalCount)
            .WithMessage(ValidationConstants.DefectCountExceedsTotalCount);

        this.RuleFor(x => x.Timestamp)
            .NotEmpty()
            .WithMessage(ValidationConstants.TimestampRequired);
    }
}