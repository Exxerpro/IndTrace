// <copyright file="FixedSizedStackSequentialTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.CommonTests;
/// <summary>
/// Represents the FixedSizedStackSequentialTests.
/// </summary>

[TestCaseOrderer(typeof(TestCaseOrderer))] // Correct Assembly Name
public class FixedSizedStackSequentialTests
{
    private readonly Subject<Func<Task>> _testSubject = new Subject<Func<Task>>();
    private readonly ITestOutputHelper _output;

    private static int _testCount = 0;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="output">The output.</param>

    public FixedSizedStackSequentialTests(ITestOutputHelper output)
    {
        _output = output;

        // Ensure tests are executed sequentially
        _testSubject
            .SelectMany(testFunc => Observable.FromAsync(testFunc))
            .Subscribe(result =>
                _output.WriteLine($"Test executed with result: {result}"));
    }
    /// <summary>
    /// Executes AEnqueue_Should_Add_Item_To_Stack operation.
    /// </summary>
    /// <returns>The result of AEnqueue_Should_Add_Item_To_Stack.</returns>

    [Fact, TestPriority(1)]
    public async Task AEnqueue_Should_Add_Item_To_Stack()
    {
        await EnqueueTest(() =>
        {
            _testCount++;
            _output.WriteLine($"Test number {_testCount.ToString()} executed at {DateTime.Now.ToLocalTime():O}");

            var stack = new FixedSizedStack<int>(5);
            stack.Push(1);

            stack.Count.ShouldBe(1);
            stack.Peek().ShouldBe(1);
        });
    }
    /// <summary>
    /// Executes BEnqueue_Should_Remove_Excess_Items_When_Limit_Is_Reached operation.
    /// </summary>
    /// <returns>The result of BEnqueue_Should_Remove_Excess_Items_When_Limit_Is_Reached.</returns>

    [Fact, TestPriority(2)]
    public async Task BEnqueue_Should_Remove_Excess_Items_When_Limit_Is_Reached()
    {
        await EnqueueTest(() =>
        {
            _testCount++;
            _output.WriteLine($"Test number {_testCount.ToString()} executed at {DateTime.Now.ToLocalTime():O}");

            var stack = new FixedSizedStack<int>(3);
            stack.Push(1);
            stack.Push(2);
            stack.Push(3);
            stack.Push(4);
            stack.Push(5);
            stack.Push(6);

            stack.Count.ShouldBe(3);
            stack.ToArray().ShouldBe(new[] { 4, 5, 6 });
        });
    }
    /// <summary>
    /// Executes CEnqueue_Should_Maintain_Thread_Safety operation.
    /// </summary>
    /// <returns>The result of CEnqueue_Should_Maintain_Thread_Safety.</returns>

    [Fact, TestPriority(3)]
    public async Task CEnqueue_Should_Maintain_Thread_Safety()
    {
        await EnqueueTest(() =>
        {
            _testCount++;
            _output.WriteLine($"Test number {_testCount.ToString()} executed at {DateTime.Now.ToLocalTime():O}");

            var stack = new FixedSizedStack<int>(5);
            Parallel.For(0, 10, i => stack.Push(i));

            stack.Count.ShouldBeLessThanOrEqualTo(5);
        });
    }
    /// <summary>
    /// Executes DFixedSizedStack_Should_Initialize_With_Default_Size operation.
    /// </summary>
    /// <returns>The result of DFixedSizedStack_Should_Initialize_With_Default_Size.</returns>

    [Fact, TestPriority(4)]
    public async Task DFixedSizedStack_Should_Initialize_With_Default_Size()
    {
        await EnqueueTest(() =>
        {
            _testCount++;
            _output.WriteLine($"Test number {_testCount.ToString()} executed at {DateTime.Now.ToLocalTime():O}");
            var stack = new FixedSizedStack<int>(); // No size provided, should use default size

            stack.Limit.ShouldBe(FixedSizedStack<int>.DefaultSize);
        });
    }
    /// <summary>
    /// Executes EPeek_Should_Return_Top_Item_Without_Removing_It operation.
    /// </summary>
    /// <returns>The result of EPeek_Should_Return_Top_Item_Without_Removing_It.</returns>

    [Fact, TestPriority(5)]
    public async Task EPeek_Should_Return_Top_Item_Without_Removing_It()
    {
        await EnqueueTest(() =>
        {
            _testCount++;
            _output.WriteLine($"Test number {_testCount.ToString()} executed at {DateTime.Now.ToLocalTime():O}");
            var stack = new FixedSizedStack<int>(3);
            stack.Push(1);
            stack.Push(2);
            stack.Push(3);

            var topItem = stack.Peek();
            topItem.ShouldBe(1);
            stack.Count.ShouldBe(3);
        });
    }
    /// <summary>
    /// Executes Peek_Should_Return_Null_WhenStack_Is_Empty operation.
    /// </summary>
    /// <returns>The result of Peek_Should_Return_Null_WhenStack_Is_Empty.</returns>

    [Fact, TestPriority(6)]
    public async Task Peek_Should_Return_Null_WhenStack_Is_Empty()
    {
        await EnqueueTest(() =>
        {
            _testCount++;
            _output.WriteLine($"Test number {_testCount.ToString()} executed at {DateTime.Now.ToLocalTime():O}");

            var stack = new FixedSizedStack<int>(3);
            var result = stack.Peek();

            result.ShouldBe(default(int)); // Should return default value (0 for int) when empty
        });
    }
    /// <summary>
    /// Executes GEnqueue_Should_Remove_Excess_Items_When_Limit_Is_Reached_Test1 operation.
    /// </summary>
    /// <returns>The result of GEnqueue_Should_Remove_Excess_Items_When_Limit_Is_Reached_Test1.</returns>

    [Fact, TestPriority(7)]
    public async Task GEnqueue_Should_Remove_Excess_Items_When_Limit_Is_Reached_Test1()
    {
        await EnqueueTest(() =>
        {
            _testCount++;
            _output.WriteLine($"Test number {_testCount.ToString()} executed at {DateTime.Now.ToLocalTime():O}");

            var stack = new FixedSizedStack<int>(3);
            stack.Push(1);
            stack.Push(2);
            stack.Push(3);
            stack.Push(4); // This should remove the first item (1)

            stack.Count.ShouldBe(3);
            stack.ToArray().ShouldNotContain(1);
            stack.ToArray().ShouldBe(new[] { 2, 3, 4 });
        });
    }
    /// <summary>
    /// Executes HPeek_Should_Return_Top_Item_Without_Removing_It_After_Reaching_Limit operation.
    /// </summary>
    /// <returns>The result of HPeek_Should_Return_Top_Item_Without_Removing_It_After_Reaching_Limit.</returns>

    [Fact, TestPriority(8)]
    public async Task HPeek_Should_Return_Top_Item_Without_Removing_It_After_Reaching_Limit()
    {
        await EnqueueTest(() =>
        {
            _testCount++;
            _output.WriteLine($"Test number {_testCount.ToString()} executed at {DateTime.Now.ToLocalTime():O}");

            var stack = new FixedSizedStack<int>(5);
            stack.Push(1);
            stack.Push(2);
            stack.Push(3);
            stack.Push(4);
            stack.Push(5);
            stack.Push(6);

            var topItem = stack.Peek();
            topItem.ShouldBe(2); // Oldest item should be 1
            stack.Count.ShouldBe(5); // Ensure the item wasn't removed
        });
    }

    private async Task EnqueueTest(Action testAction)
    {
        var stopwatch = Stopwatch.StartNew();
        _testSubject.OnNext(() =>
        {
            testAction();
            return Task.CompletedTask;
        });

        await Task.Delay(2000); // Delay to ensure sequential execution
        stopwatch.Stop();
        _output.WriteLine($"Test executed in {stopwatch.ElapsedMilliseconds} ms");
        _output.WriteLine($"Test number {_testCount.ToString()} executed at {DateTime.Now.ToLocalTime():O}");
    }
}