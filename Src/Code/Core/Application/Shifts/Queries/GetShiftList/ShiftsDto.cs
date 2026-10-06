// <copyright file="ShiftsDto.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Shifts.Queries.GetShiftList;

/// <summary>
/// Represents the ShiftsDto.
/// </summary>
public class ShiftsDto
{
    /// <summary>
    /// Gets or sets the ShiftId.
    /// </summary>
    public int ShiftId { get; set; }

    /// <summary>
    /// Gets or sets the StartBy.
    /// </summary>
    public DateTime StartBy { get; set; }

    /// <summary>
    /// Gets or sets the Duration.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets the MaxDuration.
    /// </summary>
    public TimeSpan MaxDuration { get; set; } = new(16, 0, 0);

    /// <summary>
    /// Gets or sets the MinDuration.
    /// </summary>
    public TimeSpan MinDuration { get; set; } = new(16, 0, 0);

    /// <summary>
    /// Gets or sets the NormalDuration.
    /// </summary>
    public TimeSpan NormalDuration { get; set; } = new(8, 30, 0);

    /// <summary>
    /// Executes ToDto operation.
    /// </summary>
    /// <param name="src">The src.</param>
    /// <returns>The result of ToDto.</returns>
    public static IndQuestResults.Result<ShiftsDto> ToDto(Shift src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<ShiftsDto>.WithFailure("Shift source cannot be null");
        }

        return IndQuestResults.Result<ShiftsDto>.Success(new ShiftsDto
        {
            ShiftId = src.ShiftId.Value,
            StartBy = src.StartBy,
            Duration = src.Duration,
            MaxDuration = src.MaxDuration,
            MinDuration = src.MinDuration,
            NormalDuration = src.NormalDuration,
        });
    }

    /// <summary>
    /// Executes ToEntity operation.
    /// </summary>
    /// <param name="src">The src.</param>
    /// <returns>The result of ToEntity.</returns>
    public static IndQuestResults.Result<Shift> ToEntity(ShiftsDto src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<Shift>.WithFailure("ShiftsDto source cannot be null");
        }

        return IndQuestResults.Result<Shift>.Success(new Shift(new DateTimeMachine())
        {
            ShiftId = new ShiftId(src.ShiftId),
            StartBy = src.StartBy,
            Duration = src.Duration,
            MaxDuration = src.MaxDuration,
            MinDuration = src.MinDuration,
            NormalDuration = src.NormalDuration,
        });
    }

    /// <summary>
    /// Executes ToDtoList operation.
    /// </summary>
    /// <param name="src">The src.</param>
    /// <returns>The result of ToDtoList.</returns>
    public static IndQuestResults.Result<List<ShiftsDto>> ToDtoList(IEnumerable<Shift> src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<List<ShiftsDto>>.WithFailure("Shift collection cannot be null");
        }

        var list = src.Select(s => new ShiftsDto
        {
            ShiftId = s.ShiftId.Value,
            StartBy = s.StartBy,
            Duration = s.Duration,
            MaxDuration = s.MaxDuration,
            MinDuration = s.MinDuration,
            NormalDuration = s.NormalDuration,
        }).ToList();
        return IndQuestResults.Result<List<ShiftsDto>>.Success(list);
    }
}