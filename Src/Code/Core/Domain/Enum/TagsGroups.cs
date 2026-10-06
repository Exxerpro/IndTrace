// <copyright file="TagsGroups.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Enum;

/// <summary>
/// Represents groups of tags for classification in the system.
/// </summary>
public class TagsGroups : EnumModel
{
    /// <summary>
    /// Gets the 'None' tag group.
    /// </summary>
    public static readonly TagsGroups None
        = new(0, "None");

    /// <summary>
    /// Gets the 'EventTags' tag group.
    /// </summary>
    public static readonly TagsGroups EventTags
        = new(1, "EventTags");

    /// <summary>
    /// Gets the 'ReadOnlyTags' tag group.
    /// </summary>
    public static readonly TagsGroups ReadOnlyTags
        = new(2, "ReadOnlyTags");

    /// <summary>
    /// Gets the 'WriteOnlyTags' tag group.
    /// </summary>
    public static readonly TagsGroups WriteOnlyTags
        = new(4, "WriteOnlyTags");

    /// <summary>
    /// Gets the 'WriteAndReadTags' tag group.
    /// </summary>
    public static readonly TagsGroups WriteAndReadTags
        = new(8, "WriteAndReadTags");

    /// <summary>
    /// Gets the 'ReadCyclicTags' tag group.
    /// </summary>
    public static readonly TagsGroups ReadCyclicTags
        = new(16, "ReadCyclicTags");

    /// <summary>
    /// Gets the 'WriteCyclicTags' tag group.
    /// </summary>
    public static readonly TagsGroups WriteCyclicTags
        = new(32, "WriteCyclicTags");

    /// <summary>
    /// Gets the 'HeartbeatTags' tag group.
    /// </summary>
    public static readonly TagsGroups HeartbeatTags
        = new(64, "HeartbeatTags");

    /// <summary>
    /// Gets the 'RegisterTags' tag group.
    /// </summary>
    public static readonly TagsGroups RegisterTags
        = new(128, "RegisterTags");

    /// <summary>
    /// Gets the 'ReferenceTags' tag group.
    /// </summary>
    public static readonly TagsGroups ReferenceTags
        = new(256, "ReferenceTags");

    /// <summary>
    /// Gets the 'CommandTags' tag group.
    /// </summary>
    public static readonly TagsGroups CommandTags
        = new(512, "CommandTags");

    /// <summary>
    /// Gets the 'PerformanceTags' tag group.
    /// </summary>
    public static readonly TagsGroups PerformanceTags
        = new(1024, "PerformanceTags");

    /// <summary>
    /// Initializes a new instance of the <see cref="TagsGroups"/> class.
    /// </summary>
    public TagsGroups()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TagsGroups"/> class with specified values.
    /// </summary>
    /// <param name="value">The integer value.</param>
    /// <param name="name">The name.</param>
    /// <param name="displayName">The display name.</param>
    private TagsGroups(int value, string name, string? displayName = null)
        : base(value, name, displayName ?? string.Empty)
    {
    }

    /// <summary>
    /// Implicitly converts a TagsGroupsEnum to its integer value.
    /// </summary>
    /// <param name="enumerator">The enumerator to convert.</param>
    public static implicit operator int(TagsGroups enumerator) => enumerator.Value;

    /// <summary>
    /// Implicitly converts a TagsGroupsEnum to a nullable integer value.
    /// </summary>
    /// <param name="enumerator">The enumerator to convert.</param>
    public static implicit operator int?(TagsGroups enumerator) => enumerator.Value;

    /// <summary>
    /// Implicitly converts a TagsGroupsEnum to its string representation.
    /// </summary>
    /// <param name="enumerator">The enumerator to convert.</param>
    public static implicit operator string(TagsGroups enumerator) => enumerator.Value.ToString();

    /// <summary>
    /// Implicitly converts an integer value to a TagsGroupsEnum.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator TagsGroups(int value) => FromValue<TagsGroups>(value);

    /// <summary>
    /// Implicitly converts a nullable integer value to a TagsGroupsEnum.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator TagsGroups(int? value) => FromValue<TagsGroups>(value ?? 0);
}
