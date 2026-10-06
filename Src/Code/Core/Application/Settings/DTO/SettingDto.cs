// <copyright file="SettingDto.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Settings.DTO;

/// <summary>
/// Represents the SettingDto.
/// </summary>
public class SettingDto
{
    /// <summary>
    /// Gets or sets the SettingId.
    /// </summary>
    public int SettingId { get; set; }

    /// <summary>
    /// Gets or sets the MachineId.
    /// </summary>
    public int MachineId { get; set; }

    /// <summary>
    /// Gets or sets the configuration.
    /// </summary>
    public string Config { get; set; } = string.Empty;

    /// <summary>
    /// Executes ToDto operation.
    /// </summary>
    /// <param name="src">The src.</param>
    /// <returns>The result of ToDto.</returns>
    public static IndQuestResults.Result<SettingDto> ToDto(Domain.Entities.Setting src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<SettingDto>.WithFailure("Setting source cannot be null");
        }

        return IndQuestResults.Result<SettingDto>.Success(new SettingDto
        {
            SettingId = src.SettingId,
            MachineId = src.MachineId.Value,
            Config = src.Config,
        });
    }

    /// <summary>
    /// Executes ToEntity operation.
    /// </summary>
    /// <param name="src">The src.</param>
    /// <returns>The result of ToEntity.</returns>
    public static IndQuestResults.Result<Domain.Entities.Setting> ToEntity(SettingDto src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<Domain.Entities.Setting>.WithFailure("SettingDto source cannot be null");
        }

        return IndQuestResults.Result<Domain.Entities.Setting>.Success(new Domain.Entities.Setting
        {
            SettingId = src.SettingId,
            MachineId = new MachineId(src.MachineId),
            Config = src.Config,
        });
    }

    /// <summary>
    /// Executes ToDtoList operation.
    /// </summary>
    /// <param name="src">The src.</param>
    /// <returns>The result of ToDtoList.</returns>
    public static IndQuestResults.Result<List<SettingDto>> ToDtoList(IEnumerable<Domain.Entities.Setting> src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<List<SettingDto>>.WithFailure("Setting collection cannot be null");
        }

        var list = src.Select(s => new SettingDto
        {
            SettingId = s.SettingId,
            MachineId = s.MachineId.Value,
            Config = s.Config,
        }).ToList();
        return IndQuestResults.Result<List<SettingDto>>.Success(list);
    }
}