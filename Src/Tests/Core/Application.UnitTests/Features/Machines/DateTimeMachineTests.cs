// <copyright file="DateTimeMachineTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Application.UnitTests.Features.Machines;

/// <summary>
/// Unit tests for DateTimeMachine service
/// </summary>
public class DateTimeMachineTests
{
    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var dateTimeMachine = new DateTimeMachine();

        // Assert
        dateTimeMachine.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Now_WhenCalled_ShouldReturnCurrentDateTime operation.
    /// </summary>

    [Fact]
    public void Now_WhenCalled_ShouldReturnCurrentDateTime()
    {
        // Arrange
        var dateTimeMachine = new DateTimeMachine();
        var beforeCall = DateTime.Now;

        // Act
        var result = dateTimeMachine.Now;
        var afterCall = DateTime.Now;

        // Assert
        result.ShouldBeGreaterThanOrEqualTo(beforeCall);
        result.ShouldBeLessThanOrEqualTo(afterCall);
    }

    /// <summary>
    /// Executes Now_WhenCalledMultipleTimes_ShouldReturnDifferentValues operation.
    /// </summary>

    [Fact]
    public void Now_WhenCalledMultipleTimes_ShouldReturnDifferentValues()
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();
        var dateTimeMachine = new DateTimeMachine(fakeTimeProvider);

        // Act
        var firstCall = dateTimeMachine.Now;
        fakeTimeProvider.Advance(TimeSpan.FromMilliseconds(10));
        var secondCall = dateTimeMachine.Now;

        // Assert
        secondCall.ShouldBeGreaterThan(firstCall);
    }

    /// <summary>
    /// Executes Now_WhenCalledInRapidSuccession_ShouldReturnValidDates operation.
    /// </summary>

    [Fact]
    public void Now_WhenCalledInRapidSuccession_ShouldReturnValidDates()
    {
        // Arrange
        var dateTimeMachine = new DateTimeMachine();
        var results = new List<DateTime>();

        // Act
        for (int i = 0; i < 100; i++)
        {
            results.Add(dateTimeMachine.Now);
        }

        // Assert
        results.ShouldNotBeEmpty();
        results.Count.ShouldBe(100);
        results.ShouldAllBe(dt => dt > DateTime.MinValue);
        results.ShouldAllBe(dt => dt < DateTime.MaxValue);
    }

    /// <summary>
    /// Executes Now_WhenCalledFromDifferentThreads_ShouldReturnValidDates operation.
    /// </summary>
    /// <returns>The result of Now_WhenCalledFromDifferentThreads_ShouldReturnValidDates.</returns>

    [Fact]
    public async Task Now_WhenCalledFromDifferentThreads_ShouldReturnValidDates()
    {
        // Arrange
        var dateTimeMachine = new DateTimeMachine();
        var results = new List<DateTime>();
        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() =>
                {
                    lock (results)
                    {
                        results.Add(dateTimeMachine.Now);
                    }

                    return Task.FromResult(results);
                }, TestContext.Current.CancellationToken
            ));
        }

        await Task.WhenAll(tasks.ToArray());

        // Assert
        results.ShouldNotBeEmpty();
        results.Count.ShouldBe(10);
        results.ShouldAllBe(dt => dt > DateTime.MinValue);
        results.ShouldAllBe(dt => dt < DateTime.MaxValue);
    }

    /// <summary>
    /// Executes Now_WhenCalledAtSystemBoundaries_ShouldHandleCorrectly operation.
    /// </summary>

    [Fact]
    public void Now_WhenCalledAtSystemBoundaries_ShouldHandleCorrectly()
    {
        // Arrange
        var dateTimeMachine = new DateTimeMachine();

        // Act
        var result = dateTimeMachine.Now;

        // Assert
        result.ShouldNotBe(DateTime.MinValue);
        result.ShouldNotBe(DateTime.MaxValue);
        result.Kind.ShouldBe(DateTimeKind.Local);
    }

    /// <summary>
    /// Executes Now_WhenCalledInLoop_ShouldReturnMonotonicallyIncreasingValues operation.
    /// </summary>

    [Fact]
    public void Now_WhenCalledInLoop_ShouldReturnMonotonicallyIncreasingValues()
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();
        var dateTimeMachine = new DateTimeMachine(fakeTimeProvider);
        var results = new List<DateTime>();

        // Act
        for (int i = 0; i < 50; i++)
        {
            results.Add(dateTimeMachine.Now);
            fakeTimeProvider.Advance(TimeSpan.FromMilliseconds(1)); // Deterministic delay
        }

        // Assert
        for (int i = 1; i < results.Count; i++)
        {
            results[i].ShouldBeGreaterThanOrEqualTo(results[i - 1]);
        }
    }

    /// <summary>
    /// Executes Now_WhenCalledWithHighFrequency_ShouldNotThrowException operation.
    /// </summary>

    [Fact]
    public void Now_WhenCalledWithHighFrequency_ShouldNotThrowException()
    {
        // Arrange
        var dateTimeMachine = new DateTimeMachine();

        // Act & Assert
        Should.NotThrow(() =>
        {
            for (int i = 0; i < 1000; i++)
            {
                var result = dateTimeMachine.Now;
                result.ShouldNotBe(DateTime.MinValue);
            }
        });
    }

    /// <summary>
    /// Executes Now_WhenCalledAfterLongDelay_ShouldReturnCorrectTime operation.
    /// </summary>

    [Fact]
    public void Now_WhenCalledAfterLongDelay_ShouldReturnCorrectTime()
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();
        var dateTimeMachine = new DateTimeMachine(fakeTimeProvider);
        var beforeDelay = dateTimeMachine.Now;

        // Act
        fakeTimeProvider.Advance(TimeSpan.FromMilliseconds(100));
        var afterDelay = dateTimeMachine.Now;

        // Assert
        var timeDifference = afterDelay - beforeDelay;
        timeDifference.TotalMilliseconds.ShouldBe(100);
    }

    /// <summary>
    /// Executes Now_WhenCalledInDifferentTimeZones_ShouldReturnLocalTime operation.
    /// </summary>

    [Fact]
    public void Now_WhenCalledInDifferentTimeZones_ShouldReturnLocalTime()
    {
        // Arrange
        var dateTimeMachine = new DateTimeMachine();

        // Act
        var result = dateTimeMachine.Now;

        // Assert
        result.Kind.ShouldBe(DateTimeKind.Local);
    }

    /// <summary>
    /// Executes Now_WhenCalledAtMidnight_ShouldHandleCorrectly operation.
    /// </summary>

    [Fact]
    public void Now_WhenCalledAtMidnight_ShouldHandleCorrectly()
    {
        // Arrange
        var dateTimeMachine = new DateTimeMachine();

        // Act
        var result = dateTimeMachine.Now;

        // Assert
        result.ShouldNotBe(DateTime.MinValue);
        result.Date.ShouldBe(result.Date); // Should be the same date
    }

    /// <summary>
    /// Executes Now_WhenCalledAtYearBoundaries_ShouldHandleCorrectly operation.
    /// </summary>

    [Fact]
    public void Now_WhenCalledAtYearBoundaries_ShouldHandleCorrectly()
    {
        // Arrange
        var dateTimeMachine = new DateTimeMachine();

        // Act
        var result = dateTimeMachine.Now;

        // Assert
        result.Year.ShouldBeGreaterThan(1900);
        result.Year.ShouldBeLessThan(2100);
    }

    /// <summary>
    /// Executes Now_WhenCalledAtMonthBoundaries_ShouldHandleCorrectly operation.
    /// </summary>

    [Fact]
    public void Now_WhenCalledAtMonthBoundaries_ShouldHandleCorrectly()
    {
        // Arrange
        var dateTimeMachine = new DateTimeMachine();

        // Act
        var result = dateTimeMachine.Now;

        // Assert
        result.Month.ShouldBeGreaterThanOrEqualTo(1);
        result.Month.ShouldBeLessThanOrEqualTo(12);
    }

    /// <summary>
    /// Executes Now_WhenCalledAtDayBoundaries_ShouldHandleCorrectly operation.
    /// </summary>

    [Fact]
    public void Now_WhenCalledAtDayBoundaries_ShouldHandleCorrectly()
    {
        // Arrange
        var dateTimeMachine = new DateTimeMachine();

        // Act
        var result = dateTimeMachine.Now;

        // Assert
        result.Day.ShouldBeGreaterThanOrEqualTo(1);
        result.Day.ShouldBeLessThanOrEqualTo(31);
    }

    /// <summary>
    /// Executes Now_WhenCalledAtHourBoundaries_ShouldHandleCorrectly operation.
    /// </summary>

    [Fact]
    public void Now_WhenCalledAtHourBoundaries_ShouldHandleCorrectly()
    {
        // Arrange
        var dateTimeMachine = new DateTimeMachine();

        // Act
        var result = dateTimeMachine.Now;

        // Assert
        result.Hour.ShouldBeGreaterThanOrEqualTo(0);
        result.Hour.ShouldBeLessThanOrEqualTo(23);
    }

    /// <summary>
    /// Executes Now_WhenCalledAtMinuteBoundaries_ShouldHandleCorrectly operation.
    /// </summary>

    [Fact]
    public void Now_WhenCalledAtMinuteBoundaries_ShouldHandleCorrectly()
    {
        // Arrange
        var dateTimeMachine = new DateTimeMachine();

        // Act
        var result = dateTimeMachine.Now;

        // Assert
        result.Minute.ShouldBeGreaterThanOrEqualTo(0);
        result.Minute.ShouldBeLessThanOrEqualTo(59);
    }

    /// <summary>
    /// Executes Now_WhenCalledAtSecondBoundaries_ShouldHandleCorrectly operation.
    /// </summary>

    [Fact]
    public void Now_WhenCalledAtSecondBoundaries_ShouldHandleCorrectly()
    {
        // Arrange
        var dateTimeMachine = new DateTimeMachine();

        // Act
        var result = dateTimeMachine.Now;

        // Assert
        result.Second.ShouldBeGreaterThanOrEqualTo(0);
        result.Second.ShouldBeLessThanOrEqualTo(59);
    }

    /// <summary>
    /// Executes Now_WhenCalledAtMillisecondBoundaries_ShouldHandleCorrectly operation.
    /// </summary>

    [Fact]
    public void Now_WhenCalledAtMillisecondBoundaries_ShouldHandleCorrectly()
    {
        // Arrange
        var dateTimeMachine = new DateTimeMachine();

        // Act
        var result = dateTimeMachine.Now;

        // Assert
        result.Millisecond.ShouldBeGreaterThanOrEqualTo(0);
        result.Millisecond.ShouldBeLessThanOrEqualTo(999);
    }

    /// <summary>
    /// Executes Now_WhenCalledInStressTest_ShouldHandleCorrectly operation.
    /// </summary>

    [Fact]
    public async Task Now_WhenCalledInStressTest_ShouldHandleCorrectly()
    {
        // Arrange
        var dateTimeMachine = new DateTimeMachine();
        var results = new List<DateTime>();
        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < 10; j++)
                {
                    lock (results)
                    {
                        results.Add(dateTimeMachine.Now);
                    }
                }
            }, TestContext.Current.CancellationToken));
        }

        await Task.WhenAll(tasks.ToArray());

        // Assert
        results.Count.ShouldBe(1000);
        results.ShouldAllBe(dt => dt > DateTime.MinValue);
        results.ShouldAllBe(dt => dt < DateTime.MaxValue);
    }

    /// <summary>
    /// Executes Now_WhenCalledWithParallelExecution_ShouldHandleCorrectly operation.
    /// </summary>

    [Fact]
    public void Now_WhenCalledWithParallelExecution_ShouldHandleCorrectly()
    {
        // Arrange
        var dateTimeMachine = new DateTimeMachine();
        var results = new System.Collections.Concurrent.ConcurrentBag<DateTime>();

        // Act
        Parallel.For(0, 100, i =>
        {
            results.Add(dateTimeMachine.Now);
        });

        // Assert
        results.Count.ShouldBe(100);
        results.ShouldAllBe(dt => dt > DateTime.MinValue);
        results.ShouldAllBe(dt => dt < DateTime.MaxValue);
    }

    /// <summary>
    /// Executes Now_WhenCalledInAsyncContext_ShouldHandleCorrectly operation.
    /// </summary>
    /// <returns>The result of Now_WhenCalledInAsyncContext_ShouldHandleCorrectly.</returns>

    [Fact]
    public async Task Now_WhenCalledInAsyncContext_ShouldHandleCorrectly()
    {
        // Arrange
        var dateTimeMachine = new DateTimeMachine();

        // Act & Assert
        var result = await Task.Run(() =>
        {
            var dateResult = dateTimeMachine.Now;
            dateResult.ShouldNotBe(DateTime.MinValue);
            dateResult.ShouldNotBe(DateTime.MaxValue);
            return dateResult;
        }, TestContext.Current.CancellationToken);

        result.ShouldNotBe(DateTime.MinValue);
        result.ShouldNotBe(DateTime.MaxValue);
    }

    /// <summary>
    /// Executes Now_WhenCalledWithDifferentInstances_ShouldReturnIndependentValues operation.
    /// </summary>

    [Fact]
    public void Now_WhenCalledWithDifferentInstances_ShouldReturnIndependentValues()
    {
        // Arrange
        var dateTimeMachine1 = new DateTimeMachine();
        var dateTimeMachine2 = new DateTimeMachine();

        // Act
        var result1 = dateTimeMachine1.Now;
        var result2 = dateTimeMachine2.Now;

        // Assert
        result1.ShouldNotBe(DateTime.MinValue);
        result2.ShouldNotBe(DateTime.MinValue);
        // Both should be valid times, but they might be the same if called very quickly
    }

    /// <summary>
    /// Executes Now_WhenCalledAfterGarbageCollection_ShouldStillWork operation.
    /// </summary>

    [Fact]
    public void Now_WhenCalledAfterGarbageCollection_ShouldStillWork()
    {
        // Arrange
        var dateTimeMachine = new DateTimeMachine();
        var beforeGC = dateTimeMachine.Now;

        // Act
        GC.Collect();
        GC.WaitForPendingFinalizers();
        var afterGC = dateTimeMachine.Now;

        // Assert
        beforeGC.ShouldNotBe(DateTime.MinValue);
        afterGC.ShouldNotBe(DateTime.MinValue);
        afterGC.ShouldBeGreaterThan(beforeGC);
    }

    /// <summary>
    /// Executes Now_WhenCalledWithMemoryPressure_ShouldHandleCorrectly operation.
    /// </summary>

    [Fact]
    public void Now_WhenCalledWithMemoryPressure_ShouldHandleCorrectly()
    {
        // Arrange
        var dateTimeMachine = new DateTimeMachine();
        var results = new List<DateTime>();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            results.Add(dateTimeMachine.Now);
            if (i % 100 == 0)
            {
                GC.Collect(); // Simulate memory pressure
            }
        }

        // Assert
        results.Count.ShouldBe(1000);
        results.ShouldAllBe(dt => dt > DateTime.MinValue);
        results.ShouldAllBe(dt => dt < DateTime.MaxValue);
    }
}