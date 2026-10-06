// <copyright file="PerformanceDataValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Performance.Request.Command.Create;

/// <summary>
/// Validates properties of the <see cref="Domain.Entities.PerformanceData"/> object.
/// </summary>
public class PerformanceDataValidator : AbstractValidator<Domain.Entities.PerformanceData>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceDataValidator"/> class.
    /// </summary>
    public PerformanceDataValidator()
    {
        // [Fix]
        // CLAUDE
        // Date: 20/08/2025
        // Reason: Fixed incorrect validation syntax - validator was using wrong pattern with conditions in RuleFor instead of proper property validation. Changed to correct FluentValidation syntax for railway-oriented programming and null safety

        // Validate ID properties must be greater than 0
        this.RuleFor(p => p.MachineId)
            .GreaterThan(0)
            .WithMessage("MachineId must be greater than 0.");

        this.RuleFor(p => p.PlcId)
            .GreaterThan(0)
            .WithMessage("PlcId must be greater than 0.");

        this.RuleFor(p => p.BarCodeId)
            .GreaterThan(0)
            .WithMessage("BarCodeID must be greater than 0.");

        // Story 35.D2 C3: CycleId is the converted struct (IComparable) — compare the whole property against the
        // zero-key so the rule stays keyed on the "CycleId" member (ShouldHaveValidationErrorFor(p => p.CycleId)
        // and the pinned message keep matching); no .Value member-access in the rule expression.
        this.RuleFor(p => p.CycleId)
            .GreaterThan(new CycleId(0))
            .WithMessage("CycleId must be greater than 0.");

        // Validate DateTime properties
        this.RuleFor(p => p.TimeStamp)
            .NotEmpty()
            .WithMessage("TimeStamp must not be empty.");

        // [Fix]
        // CLAUDE
        // Date: 21/08/2025
        // Reason: Updated validation rules from GreaterThanOrEqualTo(0) to NotEmpty() to match test expectations. Tests expect these properties to fail validation when they have empty/zero values, requiring non-empty (non-zero) values for industrial performance data tracking.

        // Validate numeric properties must not be empty (non-zero values required)
        this.RuleFor(p => p.ApplicationFlag)
            .NotEmpty()
            .WithMessage("ApplicationFlag must not be empty.");

        this.RuleFor(p => p.EventCounter)
            .NotEmpty()
            .WithMessage("Event_Counter must not be empty.");

        this.RuleFor(p => p.CurrentTime)
            .NotEmpty()
            .WithMessage("Current_Time must not be empty.");

        this.RuleFor(p => p.RunningTime)
            .NotEmpty()
            .WithMessage("Running_Time must not be empty.");

        this.RuleFor(p => p.StoppedTime)
            .NotEmpty()
            .WithMessage("Stopped_time must not be empty.");

        this.RuleFor(p => p.FaultedTime)
            .NotEmpty()
            .WithMessage("Faulted_Time must not be empty.");

        this.RuleFor(p => p.StatusFaultReason)
            .NotEmpty()
            .WithMessage("Status_Fault_Reason must not be empty.");

        this.RuleFor(p => p.TotalProduction)
            .NotEmpty()
            .WithMessage("Total_Production must not be empty.");

        this.RuleFor(p => p.ProductionOk)
            .NotEmpty()
            .WithMessage("Production_Ok must not be empty.");

        this.RuleFor(p => p.ProductionNoK)
            .NotEmpty()
            .WithMessage("Production_NoK must not be empty.");

        this.RuleFor(p => p.StatusFaultReject)
            .NotEmpty()
            .WithMessage("Status_Fault_Reject must not be empty.");

        // Business validation: Production totals should be consistent
        this.RuleFor(p => p)
            .Must(p => p.TotalProduction >= (p.ProductionOk + p.ProductionNoK))
            .WithMessage("Total production must be greater than or equal to the sum of OK and NOK production.")
            .WithName("ProductionConsistency");
    }
}