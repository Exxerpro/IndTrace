// <copyright file="FixedSizedStackTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.CommonTests;
/// <summary>
/// Represents the FixedSizedStackTests.
/// </summary>

[TestCaseOrderer(typeof(TestCaseOrderer))] // Correct Assembly Name  
#pragma warning disable CS9113 // Parameter is unread
// Justification: ITestOutputHelper parameter required by xUnit test class pattern
// Approved By: CLAUDE on 27/08/2025
public class FixedSizedStackTests(ITestOutputHelper output)
#pragma warning restore CS9113
{
    /// <summary>
    /// Executes Enqueue_Should_Add_String_With_Timestamp operation.
    /// </summary>
    [Fact]
    public void Enqueue_Should_Add_String_With_Timestamp()
    {
        var stack = new FixedSizedStack<string>(5);
        var message = $"Message at {DateTime.Now.ToLocalTime():O}";
        stack.Push(message);

        stack.Count.ShouldBe(1);
        stack.Peek().ShouldBe(message);
    }
    /// <summary>
    /// Executes Enqueue_Should_Not_Add_Duplicate_Messages operation.
    /// </summary>

    [Fact]
    public void Enqueue_Should_Not_Add_Duplicate_Messages()
    {
        var stack = new FixedSizedStack<string>(5);
        var message = $"Duplicate message at {DateTime.Now.ToLocalTime():O}";
        stack.Push(message);
        stack.Push(message); // Attempt to add the same message again
        stack.Push(message); // Attempt to add the same message again
        stack.Push(message); // Attempt to add the same message again
        stack.Push(message); // Attempt to add the same message again

        stack.Count.ShouldBe(1); // Only one instance should be added
        stack.Peek().ShouldBe(message);
    }
    /// <summary>
    /// Executes Enqueue_Should_Handle_Special_Characters_In_Messages operation.
    /// </summary>

    [Fact]
    public void Enqueue_Should_Handle_Special_Characters_In_Messages()
    {
        var stack = new FixedSizedStack<string>(5);
        var specialCharMessage = "Special characters: !@#$%^&*()_+-={}|[]\\:\";'<>?,./`~";
        stack.Push(specialCharMessage);

        stack.Count.ShouldBe(1);
        stack.Peek().ShouldBe(specialCharMessage);
    }
    /// <summary>
    /// Executes Enqueue_Should_Handle_Large_Messages operation.
    /// </summary>

    [Fact]
    public void Enqueue_Should_Handle_Large_Messages()
    {
        var stack = new FixedSizedStack<string>(5);
        var largeMessage = new string('A', 1000); // Message with 1000 'A' characters
        stack.Push(largeMessage);

        stack.Count.ShouldBe(1);
        stack.Peek().ShouldBe(largeMessage);
    }
    /// <summary>
    /// Executes Enqueue_Should_Respect_Limit_And_Remove_Oldest_Item operation.
    /// </summary>

    [Fact]
    public void Enqueue_Should_Respect_Limit_And_Remove_Oldest_Item()
    {
        var stack = new FixedSizedStack<string>(3);
        stack.Push("Message 1");
        stack.Push("Message 2");
        stack.Push("Message 3");
        stack.Push("Message 4"); // This should cause "Message 1" to be removed

        stack.Count.ShouldBe(3);
        stack.ToArray().ShouldNotContain("Message 1");
        stack.ToArray().ShouldBe(new[] { "Message 2", "Message 3", "Message 4" });
    }
    /// <summary>
    /// Executes Enqueue_Should_NotAdd_DuplicatedLiteralStrings operation.
    /// </summary>

    [Fact]
    public void Enqueue_Should_NotAdd_DuplicatedLiteralStrings()
    {
        var stack = new FixedSizedStack<string>(3);
        stack.Push("Message 1");
        stack.Push("Message 1");
        stack.Push("Message 1");
        stack.Push("Message 1"); // This should cause "Message 1" to be removed

        stack.Count.ShouldBe(1);
    }
    /// <summary>
    /// Executes Enqueue_Should_NotAdd_DuplicatedStrings operation.
    /// </summary>

    [Fact]
    public void Enqueue_Should_NotAdd_DuplicatedStrings()
    {
        var stack = new FixedSizedStack<string>(3);
        var message = "Message 1";
        stack.Push(message);
        stack.Push(message);
        stack.Push(message);
        stack.Push(message); // This should cause "Message 1" to be removed

        stack.Count.ShouldBe(1);
    }
    /// <summary>
    /// Executes Enqueue_Should_Respect_Limit_And_Remove_Oldest_Item_AfterAdding_Many_Items operation.
    /// </summary>

    [Fact]
    public void Enqueue_Should_Respect_Limit_And_Remove_Oldest_Item_AfterAdding_Many_Items()
    {
        var stack = new FixedSizedStack<string>(3);
        stack.Push("Message 1");
        stack.Push("Message 2");
        stack.Push("Message 3");
        stack.Push("Message 4");
        stack.Push("Message 5");
        stack.Push("Message 6");
        stack.Push("Message 7");
        stack.Push("Message 8");
        stack.Push("Message 9");
        stack.Push("Message 10");
        stack.Push("Message 11");
        stack.Push("Message 12");
        stack.Push("Message 12");
        stack.Push("Message 12");

        stack.Count.ShouldBe(3);
        stack.ToArray().ShouldNotContain("Message 1");

        var messages = stack.ToArray();
        messages.ShouldBe(new[] { "Message 10", "Message 11", "Message 12" });
    }
    /// <summary>
    /// Executes Enqueue_Should_Not_Add_Duplicated_Items_NotMatter_TheOrder operation.
    /// </summary>

    [Fact]
    public void Enqueue_Should_Not_Add_Duplicated_Items_NotMatter_TheOrder()
    {
        var stack = new FixedSizedStack<string>(20);
        stack.Push("Message 1");
        stack.Push("Message 2");
        stack.Push("Message 3");
        stack.Push("Message 2");
        stack.Push("Message 4");
        stack.Push("Message 2");
        stack.Push("Message 1");
        stack.Push("Message 4");
        stack.Push("Message 1");
        stack.Push("Message 2");
        stack.Push("Message 5");
        stack.Push("Message 7");
        stack.Push("Message 6");
        stack.Push("Message 6");

        var expectedMessages = new[] { "Message 1", "Message 2", "Message 3", "Message 4", "Message 5", "Message 7", "Message 6" };

        stack.Count.ShouldBe(7);

        var messages = stack.ToArray();
        messages.ShouldBe(expectedMessages);
    }
    /// <summary>
    /// Executes Enqueue_Should_Not_Add_Empty_Messages operation.
    /// </summary>

    [Fact]
    public void Enqueue_Should_Not_Add_Empty_Messages()
    {
        var stack = new FixedSizedStack<string>(5);
        stack.Push(""); // Push an empty message
        stack.Count.ShouldBe(0); // No items should be added
    }
    /// <summary>
    /// Executes Enqueue_Should_Not_Add_Null_Object operation.
    /// </summary>

    [Fact]
    public void Enqueue_Should_Not_Add_Null_Object()
    {
        var stack = new FixedSizedStack<Dictionary<int, string>>(5);

        Dictionary<int, string> nullDictionary = [];

        nullDictionary = null!;
        stack.Push(nullDictionary!); // Push a null object
        stack.Count.ShouldBe(0); // No items should be added
    }
    /// <summary>
    /// Executes Enqueue_Should_Add_Two_Complex_Object operation.
    /// </summary>

    [Fact]
    public void Enqueue_Should_Add_Two_Complex_Object()
    {
        var stack = new FixedSizedStack<List<FixedSizedStack<string>>>(5);

        var stack2 = new FixedSizedStack<string>(3);
        stack2.Push("Message 1");

        var stack3 = new FixedSizedStack<string>(3);
        stack3.Push("Message 1");

        List<FixedSizedStack<string>> listStack = [];
        List<FixedSizedStack<string>> listStack2 = [stack2, stack3];

        stack.Push(listStack);
        stack.Push(listStack2);

        stack.Count.ShouldBe(2);
    }
    /// <summary>
    /// Executes Enqueue_Should_Not_Add_Two_Complex_Equal_Object operation.
    /// </summary>

    [Fact]
    public void Enqueue_Should_Not_Add_Two_Complex_Equal_Object()
    {
        var stack = new FixedSizedStack<List<FixedSizedStack<string>>>(5);

        List<FixedSizedStack<string>> listStack = [];

        stack.Push(listStack);
        stack.Push(listStack);

        stack.Count.ShouldBe(1);
    }
    /// <summary>
    /// Executes Enqueue_Should_Add_Two_Complex_Equal_Object operation.
    /// </summary>

    [Fact]
    public void Enqueue_Should_Add_Two_Complex_Equal_Object()
    {
        var stack = new FixedSizedStack<List<FixedSizedStack<string>>>(5);

        List<FixedSizedStack<string>> listStack = [];

        stack.Push(listStack);
        stack.Push(listStack);

        stack.Count.ShouldBe(1);
    }
    /// <summary>
    /// Executes Peek_Should_Not_Remove_The_Item operation.
    /// </summary>

    [Fact]
    public void Peek_Should_Not_Remove_The_Item()
    {
        var stack = new FixedSizedStack<string>(3);
        var message = $"Message at {DateTime.Now.ToLocalTime():O}";
        stack.Push(message);

        var peekedMessage = stack.Peek();
        stack.Count.ShouldBe(1);
        peekedMessage.ShouldBe(message);
        stack.Count.ShouldBe(1);
    }
    /// <summary>
    /// Executes GetEnumerator_Should_Return_Items_In_Lifo_Order operation.
    /// </summary>

    [Fact]
    public void GetEnumerator_Should_Return_Items_In_Lifo_Order()
    {
        var stack = new FixedSizedStack<string>(3);
        stack.Push("Message 1");
        stack.Push("Message 2");
        stack.Push("Message 3");

        using var enumerator = stack.ToEnumerable().GetEnumerator(); // Get the enumerator directly

        enumerator.MoveNext().ShouldBeTrue(); // Assert that there is a first item
        enumerator.Current.ShouldBe("Message 3"); // Last pushed item should be first in LIFO order

        enumerator.MoveNext().ShouldBeTrue(); // Assert that there is a second item
        enumerator.Current.ShouldBe("Message 2"); // Second last pushed item should be second

        enumerator.MoveNext().ShouldBeTrue(); // Assert that there is a third item
        enumerator.Current.ShouldBe("Message 1"); // First pushed item should be last

        enumerator.MoveNext().ShouldBeFalse(); // Assert that there are no more items
    }
    /// <summary>
    /// Executes GetEnumerator_Should_Return_Items_In_Lifo_Order_Respecting_The_FIFO_Queing operation.
    /// </summary>

    [Fact]
    public void GetEnumerator_Should_Return_Items_In_Lifo_Order_Respecting_The_FIFO_Queing()
    {
        var stack = new FixedSizedStack<string>(3);
        stack.Push("Message 1");
        stack.Push("Message 2");
        stack.Push("Message 3");
        stack.Push("Message 4");
        stack.Push("Message 5");

        stack.Count.ShouldBe(3); // Ensure there are 3 items

        using var enumerator = stack.ToEnumerable().GetEnumerator(); // Get the enumerator directly

        enumerator.MoveNext().ShouldBeTrue(); // Assert that there is a first item
        enumerator.Current.ShouldBe("Message 5"); // Last pushed item should be first in LIFO order

        enumerator.MoveNext().ShouldBeTrue(); // Assert that there is a second item
        enumerator.Current.ShouldBe("Message 4"); // Second last pushed item should be second

        enumerator.MoveNext().ShouldBeTrue(); // Assert that there is a third item
        enumerator.Current.ShouldBe("Message 3"); // First pushed item should be last

        enumerator.MoveNext().ShouldBeFalse(); // Assert that there are no more items
    }
    /// <summary>
    /// Executes ToReadOnlyCollection_Should_Return_Items_In_Lifo_Order operation.
    /// </summary>

    [Fact]
    public void ToReadOnlyCollection_Should_Return_Items_In_Lifo_Order()
    {
        var stack = new FixedSizedStack<string>(3);
        stack.Push("Message 1");
        stack.Push("Message 2");
        stack.Push("Message 3");

        IReadOnlyCollection<string> lifoCollection = stack.ToReadOnlyCollection();

        lifoCollection.Count.ShouldBe(3); // Ensure there are 3 items
        lifoCollection.ElementAt(0).ShouldBe("Message 3"); // Last pushed item should be first in LIFO order
        lifoCollection.ElementAt(1).ShouldBe("Message 2"); // Second last pushed item should be second
        lifoCollection.ElementAt(2).ShouldBe("Message 1"); // First pushed item should be last
    }
    /// <summary>
    /// Executes ToReadOnlyCollection_Should_Return_Items_In_Lifo_Order_Respecting_The_FIFO_Queing operation.
    /// </summary>

    [Fact]
    public void ToReadOnlyCollection_Should_Return_Items_In_Lifo_Order_Respecting_The_FIFO_Queing()
    {
        var stack = new FixedSizedStack<string>(3);
        stack.Push("Message 1");
        stack.Push("Message 2");
        stack.Push("Message 3");
        stack.Push("Message 4");
        stack.Push("Message 5");

        stack.Count.ShouldBe(3); // Ensure there are 3 items

        using var enumerator = stack.ToEnumerable().GetEnumerator(); // Get the enumerator directly

        enumerator.MoveNext().ShouldBeTrue(); // Assert that there is a first item
        enumerator.Current.ShouldBe("Message 5"); // Last pushed item should be first in LIFO order

        enumerator.MoveNext().ShouldBeTrue(); // Assert that there is a second item
        enumerator.Current.ShouldBe("Message 4"); // Second last pushed item should be second

        enumerator.MoveNext().ShouldBeTrue(); // Assert that there is a third item
        enumerator.Current.ShouldBe("Message 3"); // First pushed item should be last

        enumerator.MoveNext().ShouldBeFalse(); // Assert that there are no more items
    }
    /// <summary>
    /// Executes GetLifoEnumerator_Should_Return_Items_In_Lifo_Order operation.
    /// </summary>

    [Fact]
    public void GetLifoEnumerator_Should_Return_Items_In_Lifo_Order()
    {
        var stack = new FixedSizedStack<string>(3);
        stack.Push("Message 1");
        stack.Push("Message 2");
        stack.Push("Message 3");

        var lifoItems = stack.ToEnumerable().ToList();

        lifoItems.Count.ShouldBe(3); // Ensure there are 3 items
        lifoItems[0].ShouldBe("Message 3"); // Last pushed item should be first
        lifoItems[1].ShouldBe("Message 2"); // Second last pushed item should be second
        lifoItems[2].ShouldBe("Message 1"); // First pushed item should be last
    }
}