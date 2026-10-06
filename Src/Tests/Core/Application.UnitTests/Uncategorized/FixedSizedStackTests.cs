// <copyright file="FixedSizedStackTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Uncategorized;

/// <summary>
/// Unit tests for FixedSizedStack
/// </summary>
public class FixedSizedStackTests
{
    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var instance = new FixedSizedStack<string>(50);

        // Assert
        instance.ShouldNotBeNull();
        instance.Limit.ShouldBe(50);
        instance.Count.ShouldBe(0);
    }

    /// <summary>
    /// Executes Constructor_WithDefaultParameters_ShouldCreateInstanceWithDefaultSize operation.
    /// </summary>

    [Fact]
    public void Constructor_WithDefaultParameters_ShouldCreateInstanceWithDefaultSize()
    {
        // Arrange & Act
        var instance = new FixedSizedStack<string>();

        // Assert
        instance.ShouldNotBeNull();
        instance.Limit.ShouldBe(FixedSizedStack<string>.DefaultSize);
        instance.Count.ShouldBe(0);
    }

    /// <summary>
    /// Executes Constructor_WithInvalidParameters_ShouldCreateInstanceGracefully operation.
    /// </summary>

    [Fact]
    public void Constructor_WithInvalidParameters_ShouldCreateInstanceGracefully()
    {
        // Arrange & Act - Test with zero or negative size
        var instance1 = new FixedSizedStack<string>(0);
        var instance2 = new FixedSizedStack<string>(-5);

        // Assert - Should create instances but with potentially problematic limits
        instance1.ShouldNotBeNull();
        instance1.Limit.ShouldBe(0);
        instance2.ShouldNotBeNull();
        instance2.Limit.ShouldBe(-5);
    }

    /// <summary>
    /// Executes Properties_WhenSet_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSet_ShouldReturnCorrectValues()
    {
        // Arrange
        var instance = new FixedSizedStack<string>(10);

        // Act
        instance.Limit = 25;

        // Assert
        instance.Limit.ShouldBe(25);
        instance.Count.ShouldBe(0);
    }

    /// <summary>
    /// Executes Push_WithManufacturingBarcodes_ShouldAddCorrectly operation.
    /// </summary>
    /// <param name="barcodes">The barcodes.</param>
    /// <param name="limit">The limit.</param>
    /// <param name="expectedCount">The expectedCount.</param>

    [Theory]
    [MemberData(nameof(ManufacturingBarcodeScenarios))]
    public void Push_WithManufacturingBarcodes_ShouldAddCorrectly(string[] barcodes, int limit, int expectedCount)
    {
        // Using parameters: barcodes, limit, expectedCount
        _ = barcodes; // xUnit1026 fix
        _ = limit; // xUnit1026 fix
        _ = expectedCount; // xUnit1026 fix
        // Using parameters: barcodes, limit, expectedCount
        _ = barcodes; // xUnit1026 fix
        _ = limit; // xUnit1026 fix
        _ = expectedCount; // xUnit1026 fix
        // Using parameters: barcodes, limit, expectedCount
        _ = barcodes; // xUnit1026 fix
        _ = limit; // xUnit1026 fix
        _ = expectedCount; // xUnit1026 fix
        // Using parameters: barcodes, limit, expectedCount
        _ = barcodes; // xUnit1026 fix
        _ = limit; // xUnit1026 fix
        _ = expectedCount; // xUnit1026 fix
        // Using parameters: barcodes, limit, expectedCount
        _ = barcodes; // xUnit1026 fix
        _ = limit; // xUnit1026 fix
        _ = expectedCount; // xUnit1026 fix
        // Arrange
        var instance = new FixedSizedStack<string>(limit);

        // Act
        foreach (var barcode in barcodes)
        {
            instance.Push(barcode);
        }

        // Assert
        instance.Count.ShouldBe(expectedCount);
    }

    /// <summary>
    /// Executes Push_WithNullItem_ShouldNotAddItem operation.
    /// </summary>

    [Fact]
    public void Push_WithNullItem_ShouldNotAddItem()
    {
        // Arrange
        var instance = new FixedSizedStack<string>(10);

        // Act
        instance.Push(null!);

        // Assert
        instance.Count.ShouldBe(0);
    }

    /// <summary>
    /// Executes Push_WithEmptyString_ShouldNotAddItem operation.
    /// </summary>

    [Fact]
    public void Push_WithEmptyString_ShouldNotAddItem()
    {
        // Arrange
        var instance = new FixedSizedStack<string>(10);

        // Act
        instance.Push("");
        instance.Push("  "); // This should add as it's not empty, just whitespace

        // Assert
        instance.Count.ShouldBe(1); // Only whitespace string should be added
    }

    /// <summary>
    /// Executes Push_WithDuplicateItems_ShouldNotAddDuplicates operation.
    /// </summary>

    [Fact]
    public void Push_WithDuplicateItems_ShouldNotAddDuplicates()
    {
        // Arrange - Ford F-150 production line scenario
        var instance = new FixedSizedStack<string>(10);

        // Act
        instance.Push("VIN:1FTFW1ET5DFC12345");
        instance.Push("VIN:1FTFW1ET5DFC12346");
        instance.Push("VIN:1FTFW1ET5DFC12345"); // Duplicate

        // Assert
        instance.Count.ShouldBe(2);
        instance.ToArray().ShouldContain("VIN:1FTFW1ET5DFC12345");
        instance.ToArray().ShouldContain("VIN:1FTFW1ET5DFC12346");
    }

    /// <summary>
    /// Executes Push_ExceedingLimit_ShouldRemoveOldestItems operation.
    /// </summary>

    [Fact]
    public void Push_ExceedingLimit_ShouldRemoveOldestItems()
    {
        // Arrange - iPhone PCB production line with limit of 3
        var instance = new FixedSizedStack<string>(3);

        // Act
        instance.Push("PCB:C02YG0VZJHD1");
        instance.Push("PCB:C02YG0VZJHD2");
        instance.Push("PCB:C02YG0VZJHD3");
        instance.Push("PCB:C02YG0VZJHD4"); // Should remove the oldest

        // Assert
        instance.Count.ShouldBe(3);
        instance.ToArray().ShouldNotContain("PCB:C02YG0VZJHD1"); // Oldest should be removed
        instance.ToArray().ShouldContain("PCB:C02YG0VZJHD2");
        instance.ToArray().ShouldContain("PCB:C02YG0VZJHD3");
        instance.ToArray().ShouldContain("PCB:C02YG0VZJHD4");
    }

    /// <summary>
    /// #128 (Chunk B): dedup is now backed by a HashSet mirror maintained in lockstep with the queue. This
    /// pins the eviction semantic that mirror must preserve: an item EVICTED as the oldest is pruned from the
    /// membership mirror too, so it can be pushed again afterwards (exactly as the former O(n) queue.Contains
    /// allowed). A desynced mirror would wrongly reject the re-push.
    /// </summary>

    [Fact]
    public void Push_ItemEvictedThenPushedAgain_ShouldBeAccepted()
    {
        // Arrange - limit 2 so pushing a third item evicts the oldest
        var instance = new FixedSizedStack<string>(2);

        // Act
        instance.Push("A");
        instance.Push("B");
        instance.Push("C"); // evicts "A" (oldest)

        instance.ToArray().ShouldNotContain("A"); // A was evicted
        instance.Count.ShouldBe(2);

        instance.Push("A"); // A is no longer resident, so it must be accepted again

        // Assert - A re-enters and now "B" (the current oldest) is evicted
        instance.Count.ShouldBe(2);
        instance.ToArray().ShouldContain("A");
        instance.ToArray().ShouldContain("C");
        instance.ToArray().ShouldNotContain("B");

        // Dedup still holds for a currently-resident item
        instance.Push("A");
        instance.Count.ShouldBe(2);
    }

    /// <summary>
    /// Executes Peek_WithItems_ShouldReturnOldestItem operation.
    /// </summary>

    [Fact]
    public void Peek_WithItems_ShouldReturnOldestItem()
    {
        // Arrange - Vaccine batch scenario
        var instance = new FixedSizedStack<string>(10);
        instance.Push("BATCH:LOT-PFZ-2024-001");
        instance.Push("BATCH:LOT-PFZ-2024-002");

        // Act
        var result = instance.Peek();

        // Assert
        result.ShouldBe("BATCH:LOT-PFZ-2024-001"); // First added should be oldest
        instance.Count.ShouldBe(2); // Peek should not remove item
    }

    /// <summary>
    /// Executes Peek_WithEmptyStack_ShouldReturnDefault operation.
    /// </summary>

    [Fact]
    public void Peek_WithEmptyStack_ShouldReturnDefault()
    {
        // Arrange
        var instance = new FixedSizedStack<string>(10);

        // Act
        var result = instance.Peek();

        // Assert - empty stack returns default value instead of throwing
        result.ShouldBeNull();
    }

    /// <summary>
    /// Executes ToEnumerable_WithItems_ShouldReturnInLifoOrder operation.
    /// </summary>

    [Fact]
    public void ToEnumerable_WithItems_ShouldReturnInLifoOrder()
    {
        // Arrange - CNC machining parts
        var instance = new FixedSizedStack<string>(10);
        instance.Push("PART:ENG-001");
        instance.Push("PART:ENG-002");
        instance.Push("PART:ENG-003");

        // Act
        var result = instance.ToEnumerable().ToList();

        // Assert
        result.Count.ShouldBe(3);
        result[0].ShouldBe("PART:ENG-003"); // Most recent first (LIFO)
        result[1].ShouldBe("PART:ENG-002");
        result[2].ShouldBe("PART:ENG-001");
    }

    /// <summary>
    /// Executes ToReadOnlyCollection_WithItems_ShouldReturnCorrectCollection operation.
    /// </summary>

    [Fact]
    public void ToReadOnlyCollection_WithItems_ShouldReturnCorrectCollection()
    {
        // Arrange
        var instance = new FixedSizedStack<int>(5);
        instance.Push(100);
        instance.Push(200);
        instance.Push(300);

        // Act
        var result = instance.ToReadOnlyCollection();

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
        result.ToList()[0].ShouldBe(300); // LIFO order
        result.ToList()[1].ShouldBe(200);
        result.ToList()[2].ShouldBe(100);
    }

    /// <summary>
    /// Executes ToArray_WithItems_ShouldReturnCorrectArray operation.
    /// </summary>

    [Fact]
    public void ToArray_WithItems_ShouldReturnCorrectArray()
    {
        // Arrange - Automotive assembly line stations
        var instance = new FixedSizedStack<string>(10);
        instance.Push("STATION:WELD-01");
        instance.Push("STATION:PAINT-02");
        instance.Push("STATION:ASSEMBLY-03");

        // Act
        var result = instance.ToArray();

        // Assert
        result.ShouldNotBeNull();
        result.Length.ShouldBe(3);
        result[0].ShouldBe("STATION:WELD-01"); // FIFO order in ToArray
        result[1].ShouldBe("STATION:PAINT-02");
        result[2].ShouldBe("STATION:ASSEMBLY-03");
    }

    /// <summary>
    /// Executes Count_AfterOperations_ShouldReturnCorrectCount operation.
    /// </summary>

    [Fact]
    public void Count_AfterOperations_ShouldReturnCorrectCount()
    {
        // Arrange
        var instance = new FixedSizedStack<string>(5);

        // Act & Assert
        instance.Count.ShouldBe(0);

        instance.Push("ITEM1");
        instance.Count.ShouldBe(1);

        instance.Push("ITEM2");
        instance.Push("ITEM3");
        instance.Count.ShouldBe(3);

        instance.Push("ITEM1"); // Duplicate, should not increase count
        instance.Count.ShouldBe(3);
    }

    /// <summary>
    /// Executes ThreadSafety_ConcurrentOperations_ShouldHandleCorrectly operation.
    /// </summary>

    [Fact]
    public async Task ThreadSafety_ConcurrentOperations_ShouldHandleCorrectly()
    {
        // Arrange - Simulate concurrent manufacturing line operations
        var instance = new FixedSizedStack<string>(100);
        const int threadCount = 10;
        const int operationsPerThread = 50;

        // Act
        var tasks = new List<Task>();
        for (int i = 0; i < threadCount; i++)
        {
            int threadId = i;
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < operationsPerThread; j++)
                {
                    instance.Push($"THREAD{threadId}-ITEM{j}");
                }

                return Task.FromResult(instance);
            }));
        }

        await Task.WhenAll(tasks.ToArray());

        // Assert
        instance.Count.ShouldBeLessThanOrEqualTo(100); // Should not exceed limit
        instance.Count.ShouldBeGreaterThan(0); // Should have some items
        instance.ToArray().ShouldAllBe(item => !string.IsNullOrEmpty(item));
    }

    public static IEnumerable<object[]> ManufacturingBarcodeScenarios =>
        new List<object[]>
        {
            // [barcodes array, limit, expected count]
            new object[]
            {
                new[] { "VIN:1FTFW1ET5DFC12345", "VIN:1FTFW1ET5DFC12346", "VIN:1FTFW1ET5DFC12347" },
                5, 3
            },
            new object[]
            {
                new[] { "PCB:C02YG0VZJHD1", "PCB:C02YG0VZJHD2", "PCB:C02YG0VZJHD3", "PCB:C02YG0VZJHD4", "PCB:C02YG0VZJHD5" },
                3, 3
            }, // Exceeds limit
            new object[]
            {
                new[] { "BATCH:LOT-001", "BATCH:LOT-001", "BATCH:LOT-002" },
                10, 2
            }, // Duplicates
            new object[]
            {
                new[] { "PART:A", "PART:B", "PART:C", "PART:D", "PART:E", "PART:F" },
                4, 4
            } // Exceeds limit, keeps last 4
        };
}