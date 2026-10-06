// <copyright file="CreateShiftValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Shifts.Commands.Create;

/// <summary>
/// Represents the CreateShiftValidator.
/// </summary>
public class CreateShiftValidator : AbstractValidator<CreateShiftCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateShiftValidator"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public CreateShiftValidator()
    {
        // [Fix]
        // CLAUDE
        // Date: 22/08/2025
        // Reason: [CLUSTER SHIFT DURATION FIX] - Fixed Must() validation lambda to properly access the shift object for MinDuration/MaxDuration

        // Validate date/time properties
        this.RuleFor(shift => shift.StartBy)
            .GreaterThan(DateTime.MinValue)
            .WithMessage("Start time is required.");

        this.RuleFor(shift => shift.Duration)
            .Cascade(CascadeMode.Stop)
            .GreaterThan(TimeSpan.Zero).WithMessage("Duration is required.")
            .Must((shift, duration) =>
            {
                // [Fix]
                // CLAUDE
                // Date: 23/08/2025
                // Reason: [CLUSTER A SHIFT DURATION FIX] - Fixed Must() validation rule. The lambda parameters were incorrectly named - first parameter is the command object, second is the duration value
                return duration >= shift.MinDuration && duration <= shift.MaxDuration;
            })
            .WithMessage((shift, duration) => $"Duration must be between {shift.MinDuration} and {shift.MaxDuration}.");

        // Validate IDs must be valid positive integers
        this.RuleFor(shift => shift.MachineId)
            .GreaterThan(0)
            .WithMessage("Machine ID must be greater than 0.");

        // Validate production metrics
        this.RuleFor(shift => shift.ShiftProduction)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Shift production must be 0 or greater.");

        this.RuleFor(shift => shift.CyclesOk)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Cycles OK must be 0 or greater.");

        // [Fix]
        // CLAUDE
        // Date: 23/08/2025
        // Reason: [CRITICAL ENUMMODEL FIX] - ShiftType is EnumModel not standard enum. Must use EnumModel.Exists<ShiftType>() instead of .IsInEnum() for proper validation
        // Validate ShiftType EnumModel (not standard enum)
        this.RuleFor(shift => shift.ShiftType)
            .Must(shiftType => EnumModel.Exists<ShiftType>(shiftType.Value))
            .WithMessage("Shift type must be a valid shift type.");
    }
}
