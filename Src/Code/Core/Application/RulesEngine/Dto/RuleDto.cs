// <copyright file="RuleDto.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.RulesEngine.Dto;

/// <summary>
/// Represents the RuleDto.
/// </summary>
public class RuleDto
{
    private string ruleJson = string.Empty;
    private string name = string.Empty;
    private string description = string.Empty;

    /// <summary>
    /// Gets or sets the RuleId.
    /// </summary>
    public int RuleId { get; set; }

    public string RuleJson
    {
        get => this.ruleJson;
        set => this.ruleJson = value ?? string.Empty;
    }

    public string Name
    {
        get => this.name;
        set => this.name = value ?? string.Empty;
    }

    public string Description
    {
        get => this.description;
        set => this.description = value ?? string.Empty;
    }

    /// <summary>
    /// Gets or sets the Version.
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets the IsActive.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Executes ToDto operation.
    /// </summary>
    /// <param name="src">The src.</param>
    /// <returns>The result of ToDto.</returns>
    public static IndQuestResults.Result<RuleDto> ToDto(Rule src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<RuleDto>.WithFailure("Rule source cannot be null");
        }

        return IndQuestResults.Result<RuleDto>.Success(new RuleDto
        {
            RuleId = src.RuleId,
            RuleJson = src.RuleJson ?? string.Empty,
            Name = src.Name ?? string.Empty,
            Description = src.Description ?? string.Empty,
            Version = src.Version,
            IsActive = src.IsActive,
        });
    }

    /// <summary>
    /// Executes ToEntity operation.
    /// </summary>
    /// <param name="src">The src.</param>
    /// <returns>The result of ToEntity.</returns>
    public static IndQuestResults.Result<Rule> ToEntity(RuleDto src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<Rule>.WithFailure("RuleDto source cannot be null");
        }

        return IndQuestResults.Result<Rule>.Success(new Rule
        {
            RuleId = src.RuleId,
            RuleJson = src.RuleJson,
            Name = src.Name,
            Description = src.Description,
            Version = src.Version,
            IsActive = src.IsActive,
        });
    }
}