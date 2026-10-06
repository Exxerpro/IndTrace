// <copyright file="TagsGroupsEnumTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.TagsTests;

/// <summary>
/// Unit tests for TagsGroupsEnum - Enumeration model for PLC tag group classification in manufacturing systems.
/// Tests static properties, implicit conversions, bitwise operations, and manufacturing tag management scenarios.
/// </summary>
public class TagsGroupsEnumTests
{
    /// <summary>
    /// Executes TagsGroupsEnum_Constructor_WhenCreated_ShouldCreateInstanceWithDefaultValues operation.
    /// </summary>
    [Fact]
    public void TagsGroupsEnum_Constructor_WhenCreated_ShouldCreateInstanceWithDefaultValues()
    {
        // Arrange & Act
        var instance = new TagsGroups();

        // Assert
        instance.ShouldNotBeNull();
        instance.ShouldBeAssignableTo<EnumModel>();
        // IndQuestEnums: the parameterless constructor yields the invalid sentinel
        // (Value -1 / "Invalid Value") rather than the former in-tree 0/null defaults.
        instance.Value.ShouldBe(EnumModel.InvalidState);
        instance.Name.ShouldBe(EnumModel.InvalidName);
    }
    /// <summary>
    /// Executes StaticProperties_WhenAccessed_WhenQueried_ShouldReturnExpectedData operation.
    /// </summary>
    /// <param name="propertyName">The propertyName.</param>
    /// <param name="expectedValue">The expectedValue.</param>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData("None", 0)]
    [InlineData("EventTags", 1)]
    [InlineData("ReadOnlyTags", 2)]
    [InlineData("WriteOnlyTags", 4)]
    [InlineData("WriteAndReadTags", 8)]
    [InlineData("ReadCyclicTags", 16)]
    [InlineData("WriteCyclicTags", 32)]
    [InlineData("HeartbeatTags", 64)]
    [InlineData("RegisterTags", 128)]
    [InlineData("ReferenceTags", 256)]
    [InlineData("CommandTags", 512)]
    [InlineData("PerformanceTags", 1024)]
    public void StaticProperties_WhenAccessed_WhenQueried_ShouldReturnExpectedData(string propertyName, int expectedValue)
    {
        // Arrange & Act
        var property = typeof(TagsGroups).GetField(propertyName,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        var instance = property?.GetValue(null) as TagsGroups;

        // Assert
        instance.ShouldNotBeNull();
        instance.Value.ShouldBe(expectedValue);
        instance.Name.ShouldBe(propertyName);
    }
    /// <summary>
    /// Executes ImplicitConversion_FromInt_ShouldReturnCorrectTagsGroupsEnum operation.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="expectedName">The expectedName.</param>

    [Theory]
    [InlineData(0, "None")]
    [InlineData(1, "EventTags")]
    [InlineData(2, "ReadOnlyTags")]
    [InlineData(4, "WriteOnlyTags")]
    [InlineData(8, "WriteAndReadTags")]
    [InlineData(16, "ReadCyclicTags")]
    [InlineData(32, "WriteCyclicTags")]
    [InlineData(64, "HeartbeatTags")]
    [InlineData(128, "RegisterTags")]
    [InlineData(256, "ReferenceTags")]
    [InlineData(512, "CommandTags")]
    [InlineData(1024, "PerformanceTags")]
    public void ImplicitConversion_FromInt_ShouldReturnCorrectTagsGroupsEnum(int value, string expectedName)
    {
        // Arrange & Act
        TagsGroups result = value;

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBe(value);
        result.Name.ShouldBe(expectedName);
    }
    /// <summary>
    /// Executes ImplicitConversion_ToInt_ShouldReturnCorrectValue operation.
    /// </summary>
    /// <param name="propertyName">The propertyName.</param>
    /// <param name="expectedValue">The expectedValue.</param>

    [Theory]
    [InlineData("EventTags", 1)]
    [InlineData("ReadOnlyTags", 2)]
    [InlineData("WriteOnlyTags", 4)]
    [InlineData("HeartbeatTags", 64)]
    [InlineData("CommandTags", 512)]
    [InlineData("PerformanceTags", 1024)]
    public void ImplicitConversion_ToInt_ShouldReturnCorrectValue(string propertyName, int expectedValue)
    {
        // Arrange
        var property = typeof(TagsGroups).GetField(propertyName,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        var tagsGroup = property?.GetValue(null) as TagsGroups;

        // Act
        int value = tagsGroup!;

        // Assert
        value.ShouldBe(expectedValue);
    }
    /// <summary>
    /// Executes ImplicitConversion_ToString_ShouldReturnStringValue operation.
    /// </summary>
    /// <param name="propertyName">The propertyName.</param>
    /// <param name="expectedStringValue">The expectedStringValue.</param>

    [Theory]
    [InlineData("EventTags", "1")]
    [InlineData("ReadOnlyTags", "2")]
    [InlineData("WriteOnlyTags", "4")]
    [InlineData("HeartbeatTags", "64")]
    [InlineData("CommandTags", "512")]
    [InlineData("PerformanceTags", "1024")]
    public void ImplicitConversion_ToString_ShouldReturnStringValue(string propertyName, string expectedStringValue)
    {
        // Arrange
        var property = typeof(TagsGroups).GetField(propertyName,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        var tagsGroup = property?.GetValue(null) as TagsGroups;

        // Act
        string stringValue = tagsGroup!;

        // Assert
        stringValue.ShouldBe(expectedStringValue);
    }
    /// <summary>
    /// Executes ManufacturingTagScenarios_WithDifferentProducts_ShouldHandleCorrectly operation.
    /// </summary>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData("Ford F-150 PLC tag monitoring")]
    [InlineData("Tesla Model S production tags")]
    [InlineData("BMW X5 assembly line tags")]
    [InlineData("Mercedes quality control tags")]
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters

    // Justification: Scenario parameter provides test context documentation

    // Approved By: CLAUDE on 27/08/2025

    public void ManufacturingTagScenarios_WithDifferentProducts_ShouldHandleCorrectly(string scenario)

#pragma warning restore xUnit1026
    {
        // Arrange
        var eventTags = TagsGroups.EventTags;
        var readOnlyTags = TagsGroups.ReadOnlyTags;
        var commandTags = TagsGroups.CommandTags;

        // Act & Assert
        eventTags.Value.ShouldBe(1);
        readOnlyTags.Value.ShouldBe(2);
        commandTags.Value.ShouldBe(512);

        // Manufacturing tag group progression
        eventTags.ShouldNotBe(readOnlyTags);
        readOnlyTags.ShouldNotBe(commandTags);
        eventTags.ShouldNotBe(commandTags);
    }
    /// <summary>
    /// Executes TagsGroupsEnumBusinessRules_WhenValidatingTagGroups_ShouldFollowPowerOfTwoPattern operation.
    /// </summary>

    [Fact]
    public void TagsGroupsEnumBusinessRules_WhenValidatingTagGroups_ShouldFollowPowerOfTwoPattern()
    {
        // Arrange & Act
        var none = TagsGroups.None;
        var eventTags = TagsGroups.EventTags;
        var readOnlyTags = TagsGroups.ReadOnlyTags;
        var writeOnlyTags = TagsGroups.WriteOnlyTags;
        var heartbeatTags = TagsGroups.HeartbeatTags;
        var commandTags = TagsGroups.CommandTags;
        var performanceTags = TagsGroups.PerformanceTags;

        // Assert - Power-of-two values for bitwise operations
        none.Value.ShouldBe(0);           // Special case
        eventTags.Value.ShouldBe(1);      // 2^0
        readOnlyTags.Value.ShouldBe(2);   // 2^1
        writeOnlyTags.Value.ShouldBe(4);  // 2^2
        heartbeatTags.Value.ShouldBe(64); // 2^6
        commandTags.Value.ShouldBe(512);  // 2^9
        performanceTags.Value.ShouldBe(1024); // 2^10

        // All non-zero values should be power of two for bitwise operations
        bool IsPowerOfTwo(int num) => num > 0 && (num & (num - 1)) == 0;
        IsPowerOfTwo(eventTags.Value).ShouldBeTrue();
        IsPowerOfTwo(readOnlyTags.Value).ShouldBeTrue();
        IsPowerOfTwo(writeOnlyTags.Value).ShouldBeTrue();
        IsPowerOfTwo(heartbeatTags.Value).ShouldBeTrue();
        IsPowerOfTwo(commandTags.Value).ShouldBeTrue();
        IsPowerOfTwo(performanceTags.Value).ShouldBeTrue();
    }
    /// <summary>
    /// Executes TagGroupTypes_WithManufacturingScenarios_ShouldIndicateCorrectOperation operation.
    /// </summary>
    /// <param name="tagValue">The tagValue.</param>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(64)]
    [InlineData(512)]
    [InlineData(1024)]
    public void TagGroupTypes_WithManufacturingScenarios_ShouldIndicateCorrectOperation(int tagValue)
    {
        // Arrange & Act
        TagsGroups tagGroup = tagValue;

        // Assert
        tagGroup.Value.ShouldBe(tagValue);

        // Validate power-of-two pattern for bitwise operations (except 0)
        if (tagValue > 0)
        {
            var isPowerOfTwo = (tagValue & (tagValue - 1)) == 0;
            isPowerOfTwo.ShouldBeTrue();
        }
    }
    /// <summary>
    /// Executes ComplexManufacturingTagCombination_WithMultipleGroups_ShouldSupportBitwiseOperations operation.
    /// </summary>

    [Fact]
    public void ComplexManufacturingTagCombination_WithMultipleGroups_ShouldSupportBitwiseOperations()
    {
        // Arrange
        var eventTags = TagsGroups.EventTags;
        var readOnlyTags = TagsGroups.ReadOnlyTags;
        var commandTags = TagsGroups.CommandTags;
        var performanceTags = TagsGroups.PerformanceTags;

        // Act - Simulate complex tag group combination with bitwise operations
        int combinedTags = eventTags.Value | readOnlyTags.Value | commandTags.Value | performanceTags.Value;

        // Assert
        combinedTags.ShouldBe(1539); // 1 + 2 + 512 + 1024 = 1539

        // Verify individual components can be extracted
        (combinedTags & eventTags.Value).ShouldBe(eventTags.Value);
        (combinedTags & readOnlyTags.Value).ShouldBe(readOnlyTags.Value);
        (combinedTags & commandTags.Value).ShouldBe(commandTags.Value);
        (combinedTags & performanceTags.Value).ShouldBe(performanceTags.Value);
    }
    /// <summary>
    /// Executes TagGroupTransitions_BetweenTypes_ShouldMaintainPLCIntegrity operation.
    /// </summary>

    [Theory]
    [InlineData("EventTags", "ReadOnlyTags", "Event to monitoring transition")]
    [InlineData("WriteOnlyTags", "WriteAndReadTags", "Write-only to bidirectional")]
    [InlineData("HeartbeatTags", "PerformanceTags", "Heartbeat to performance monitoring")]
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
    // Justification: Scenario parameter provides test context documentation
    // Approved By: CLAUDE on 27/08/2025
    public void TagGroupTransitions_BetweenTypes_ShouldMaintainPLCIntegrity(
        string fromGroup, string toGroup, string scenario)
#pragma warning restore xUnit1026
    {
        // Arrange
        var fromProperty = typeof(TagsGroups).GetField(fromGroup,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        var toProperty = typeof(TagsGroups).GetField(toGroup,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

        var fromTagGroup = fromProperty?.GetValue(null) as TagsGroups;
        var toTagGroup = toProperty?.GetValue(null) as TagsGroups;

        // Act & Assert
        fromTagGroup.ShouldNotBeNull();
        toTagGroup.ShouldNotBeNull();
        fromTagGroup.ShouldNotBe(toTagGroup);

        // Both should be valid PLC tag groups
        fromTagGroup.Value.ShouldBeGreaterThanOrEqualTo(0);
        toTagGroup.Value.ShouldBeGreaterThanOrEqualTo(0);
    }
    /// <summary>
    /// Executes PLCTagGroupClassification_ShouldEnableEffectiveCommunication operation.
    /// </summary>

    [Fact]
    public void PLCTagGroupClassification_ShouldEnableEffectiveCommunication()
    {
        // Arrange
        var readOnlyTags = TagsGroups.ReadOnlyTags;
        var writeOnlyTags = TagsGroups.WriteOnlyTags;
        var writeAndReadTags = TagsGroups.WriteAndReadTags;
        var cyclicTags = TagsGroups.ReadCyclicTags;

        // Act & Assert - PLC communication patterns
        readOnlyTags.Value.ShouldBe(2);
        writeOnlyTags.Value.ShouldBe(4);
        writeAndReadTags.Value.ShouldBe(8);
        cyclicTags.Value.ShouldBe(16);

        // Communication direction classification
        readOnlyTags.Name.ShouldContain("Read");
        writeOnlyTags.Name.ShouldContain("Write");
        writeAndReadTags.Name.ShouldContain("WriteAndRead");
        cyclicTags.Name.ShouldContain("Cyclic");

        // All tag groups should be unique for proper classification
        readOnlyTags.ShouldNotBe(writeOnlyTags);
        writeOnlyTags.ShouldNotBe(writeAndReadTags);
        writeAndReadTags.ShouldNotBe(cyclicTags);
    }
    /// <summary>
    /// Executes ImplicitConversion_FromNullableInt_ShouldHandleCorrectly operation.
    /// </summary>
    /// <param name="nullableValue">The nullableValue.</param>
    /// <param name="expectedValue">The expectedValue.</param>

    [Theory]
    [InlineData(null, 0)]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(1024, 1024)]
    public void ImplicitConversion_FromNullableInt_ShouldHandleCorrectly(int? nullableValue, int expectedValue)
    {
        // Arrange & Act
        TagsGroups result = nullableValue;

        // Assert
        result.Value.ShouldBe(expectedValue);
    }
    /// <summary>
    /// Executes IndustrialAutomationSupport_ShouldEnableAdvancedPLCOperations operation.
    /// </summary>

    [Fact]
    public void IndustrialAutomationSupport_ShouldEnableAdvancedPLCOperations()
    {
        // Arrange
        var eventTags = TagsGroups.EventTags;
        var registerTags = TagsGroups.RegisterTags;
        var referenceTags = TagsGroups.ReferenceTags;
        var performanceTags = TagsGroups.PerformanceTags;

        // Act & Assert - Industrial automation tag management
        eventTags.Value.ShouldBe(1);
        registerTags.Value.ShouldBe(128);
        referenceTags.Value.ShouldBe(256);
        performanceTags.Value.ShouldBe(1024);

        // Advanced PLC operations support
        eventTags.Name.ShouldBe("EventTags");
        registerTags.Name.ShouldBe("RegisterTags");
        referenceTags.Name.ShouldBe("ReferenceTags");
        performanceTags.Name.ShouldBe("PerformanceTags");

        // All should support bitwise operations for complex combinations
        var complexOperation = eventTags.Value | registerTags.Value | referenceTags.Value | performanceTags.Value;
        complexOperation.ShouldBe(1409); // 1 + 128 + 256 + 1024
    }
    /// <summary>
    /// Executes EdgeCaseHandling_WithVariousValues_ShouldStoreCorrectly operation.
    /// </summary>
    /// <param name="edgeValue">The edgeValue.</param>
    /// <param name="scenario">The scenario.</param>
    /// <param name="expectedValue">The expectedValue.</param>

    [Theory]
    [InlineData(2048, "Custom high-value tag group", -1)] // Unmatched values resolve to the invalid sentinel (-1)
    [InlineData(-1, "Invalid tag group", -1)] // Unmatched values resolve to the invalid sentinel (-1)
    [InlineData(0, "None tag group", 0)]
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
    // Justification: Scenario parameter provides test context documentation
    // Approved By: CLAUDE on 27/08/2025
    public void EdgeCaseHandling_WithVariousValues_ShouldStoreCorrectly(int edgeValue, string scenario, int expectedValue)
#pragma warning restore xUnit1026
    {
        // Arrange & Act
        TagsGroups tagGroup = edgeValue;

        // Assert
        tagGroup.Value.ShouldBe(expectedValue);
    }
    /// <summary>
    /// Executes ManufacturingTagWorkflow_ShouldSupportCompleteProductionLineOperations operation.
    /// </summary>

    [Fact]
    public void ManufacturingTagWorkflow_ShouldSupportCompleteProductionLineOperations()
    {
        // Arrange
        var eventTags = TagsGroups.EventTags;        // Production events
        var readOnlyTags = TagsGroups.ReadOnlyTags;  // Sensor monitoring
        var commandTags = TagsGroups.CommandTags;    // Control commands
        var performanceTags = TagsGroups.PerformanceTags; // OEE monitoring

        // Act & Assert - Complete manufacturing workflow
        eventTags.Value.ShouldBe(1);
        readOnlyTags.Value.ShouldBe(2);
        commandTags.Value.ShouldBe(512);
        performanceTags.Value.ShouldBe(1024);

        // Manufacturing workflow tag coordination
        var productionWorkflow = eventTags.Value | readOnlyTags.Value | commandTags.Value | performanceTags.Value;
        productionWorkflow.ShouldBe(1539); // Complete production line tag set

        // Each tag group should serve a distinct purpose
        eventTags.ShouldNotBe(readOnlyTags);
        readOnlyTags.ShouldNotBe(commandTags);
        commandTags.ShouldNotBe(performanceTags);

        // All should support Industry 4.0 operations
        eventTags.Value.ShouldBePositive();
        readOnlyTags.Value.ShouldBePositive();
        commandTags.Value.ShouldBePositive();
        performanceTags.Value.ShouldBePositive();
    }
}
