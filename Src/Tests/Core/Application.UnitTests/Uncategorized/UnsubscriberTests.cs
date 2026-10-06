// <copyright file="UnsubscriberTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Lock = System.Threading.Lock;

namespace Application.UnitTests.Uncategorized;

/// <summary>
/// Unit tests for Unsubscriber
/// </summary>
public class UnsubscriberTests
{
    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var observers = new List<IObserver<StateChange>>();
        var observer = Substitute.For<IObserver<StateChange>>();
        var lockObj = new Lock();

        // Act
        var instance = new Unsubscriber<StateChange>(observers, observer, lockObj);

        // Assert
        instance.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Constructor_WithNullObservers_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_WithNullObservers_ShouldCreateInstance()
    {
        // Arrange
        List<IObserver<StateChange>>? observers = null!;
        var observer = Substitute.For<IObserver<StateChange>>();
        var lockObj = new Lock();

        // Act
        var instance = new Unsubscriber<StateChange>(observers!, observer, lockObj);

        // Assert
        instance.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Constructor_WithNullObserver_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_WithNullObserver_ShouldCreateInstance()
    {
        // Arrange
        var observers = new List<IObserver<StateChange>>();
        IObserver<StateChange>? observer = null!;
        var lockObj = new Lock();

        // Act
        var instance = new Unsubscriber<StateChange>(observers, observer!, lockObj);

        // Assert
        instance.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Constructor_WithNullLock_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_WithNullLock_ShouldCreateInstance()
    {
        // Arrange
        var observers = new List<IObserver<StateChange>>();
        var observer = Substitute.For<IObserver<StateChange>>();
        Lock? lockObj = null!;

        // Act
        var instance = new Unsubscriber<StateChange>(observers, observer, lockObj!);

        // Assert
        instance.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Dispose_WhenObserverExistsInList_ShouldRemoveObserver operation.
    /// </summary>

    [Fact]
    public void Dispose_WhenObserverExistsInList_ShouldRemoveObserver()
    {
        // Arrange
        var observers = new List<IObserver<StateChange>>();
        var observer = Substitute.For<IObserver<StateChange>>();
        var lockObj = new Lock();

        observers.Add(observer);
        observers.Add(Substitute.For<IObserver<StateChange>>());

        var unsubscriber = new Unsubscriber<StateChange>(observers, observer, lockObj);

        // Act
        unsubscriber.Dispose();

        // Assert
        observers.Count.ShouldBe(1);
        observers.ShouldNotContain(observer);
    }

    /// <summary>
    /// Executes Dispose_WhenObserverDoesNotExistInList_ShouldNotThrowException operation.
    /// </summary>

    [Fact]
    public void Dispose_WhenObserverDoesNotExistInList_ShouldNotThrowException()
    {
        // Arrange
        var observers = new List<IObserver<StateChange>>();
        var observer = Substitute.For<IObserver<StateChange>>();
        var lockObj = new Lock();

        // Observer is not added to the list
        observers.Add(Substitute.For<IObserver<StateChange>>());

        var unsubscriber = new Unsubscriber<StateChange>(observers, observer, lockObj);

        // Act & Assert
        Should.NotThrow(() => unsubscriber.Dispose());
        observers.Count.ShouldBe(1);
    }

    /// <summary>
    /// Executes Dispose_WhenListIsEmpty_ShouldNotThrowException operation.
    /// </summary>

    [Fact]
    public void Dispose_WhenListIsEmpty_ShouldNotThrowException()
    {
        // Arrange
        var observers = new List<IObserver<StateChange>>();
        var observer = Substitute.For<IObserver<StateChange>>();
        var lockObj = new Lock();

        var unsubscriber = new Unsubscriber<StateChange>(observers, observer, lockObj);

        // Act & Assert
        Should.NotThrow(() => unsubscriber.Dispose());
        observers.ShouldBeEmpty();
    }

    /// <summary>
    /// Executes Dispose_WhenCalledMultipleTimes_ShouldNotThrowException operation.
    /// </summary>

    [Fact]
    public void Dispose_WhenCalledMultipleTimes_ShouldNotThrowException()
    {
        // Arrange
        var observers = new List<IObserver<StateChange>>();
        var observer = Substitute.For<IObserver<StateChange>>();
        var lockObj = new Lock();

        observers.Add(observer);

        var unsubscriber = new Unsubscriber<StateChange>(observers, observer, lockObj);

        // Act & Assert
        Should.NotThrow(() => unsubscriber.Dispose());
        Should.NotThrow(() => unsubscriber.Dispose());
        Should.NotThrow(() => unsubscriber.Dispose());

        observers.ShouldBeEmpty();
    }

    /// <summary>
    /// Executes Dispose_WhenMultipleObserversExist_ShouldOnlyRemoveTargetObserver operation.
    /// </summary>

    [Fact]
    public void Dispose_WhenMultipleObserversExist_ShouldOnlyRemoveTargetObserver()
    {
        // Arrange
        var observers = new List<IObserver<StateChange>>();
        var observer1 = Substitute.For<IObserver<StateChange>>();
        var observer2 = Substitute.For<IObserver<StateChange>>();
        var observer3 = Substitute.For<IObserver<StateChange>>();
        var lockObj = new Lock();

        observers.Add(observer1);
        observers.Add(observer2);
        observers.Add(observer3);

        var unsubscriber = new Unsubscriber<StateChange>(observers, observer2, lockObj);

        // Act
        unsubscriber.Dispose();

        // Assert
        observers.Count.ShouldBe(2);
        observers.ShouldContain(observer1!);
        observers.ShouldNotContain(observer2);
        observers.ShouldContain(observer3!);
    }

    /// <summary>
    /// Executes Dispose_WhenObserverAppearsMultipleTimes_ShouldRemoveAllInstances operation.
    /// </summary>

    [Fact]
    public void Dispose_WhenObserverAppearsMultipleTimes_ShouldRemoveAllInstances()
    {
        // Arrange
        var observers = new List<IObserver<StateChange>>();
        var observer = Substitute.For<IObserver<StateChange>>();
        var lockObj = new Lock();

        observers.Add(observer);
        observers.Add(Substitute.For<IObserver<StateChange>>());
        observers.Add(observer);
        observers.Add(Substitute.For<IObserver<StateChange>>());
        observers.Add(observer);

        var unsubscriber = new Unsubscriber<StateChange>(observers, observer, lockObj);

        var logger = XUnitLogger.CreateLogger<UnsubscriberTests>();
        logger.LogInformation("Observers before Dispose:");
        foreach (var ob in observers)
        {
            logger.LogInformation(ob.GetHashCode().ToString());
        }

        // Act
        unsubscriber.Dispose();

        logger.LogInformation("Observers after Dispose:");
        foreach (var ob in observers)
        {
            logger.LogInformation(ob.GetHashCode().ToString());
        }

        // Assert
        observers.Count.ShouldBe(2);
        observers.ShouldNotContain(observer);
    }

    /// <summary>
    /// Executes Dispose_WhenListIsNull_ShouldNotThrowException operation.
    /// </summary>

    [Fact]
    public void Dispose_WhenListIsNull_ShouldNotThrowException()
    {
        // Arrange
        List<IObserver<StateChange>>? observers = null!;
        var observer = Substitute.For<IObserver<StateChange>>();
        var lockObj = new Lock();

        var unsubscriber = new Unsubscriber<StateChange>(observers!, observer, lockObj);

        // Act & Assert
        Should.NotThrow(() => unsubscriber.Dispose());
    }

    /// <summary>
    /// Executes Dispose_WhenObserverIsNull_ShouldNotThrowException operation.
    /// </summary>

    [Fact]
    public void Dispose_WhenObserverIsNull_ShouldNotThrowException()
    {
        // Arrange
        var observers = new List<IObserver<StateChange>>();
        IObserver<StateChange>? observer = null!;
        var lockObj = new Lock();

        observers.Add(Substitute.For<IObserver<StateChange>>());

        var unsubscriber = new Unsubscriber<StateChange>(observers, observer!, lockObj);

        // Act & Assert
        Should.NotThrow(() => unsubscriber.Dispose());
        observers.Count.ShouldBe(1);
    }

    /// <summary>
    /// Executes Dispose_WhenLockIsNull_ShouldNotThrowException operation.
    /// </summary>

    [Fact]
    public void Dispose_WhenLockIsNull_ShouldNotThrowException()
    {
        // Arrange
        var observers = new List<IObserver<StateChange>>();
        var observer = Substitute.For<IObserver<StateChange>>();
        Lock? lockObj = null!;

        observers.Add(observer);

        var unsubscriber = new Unsubscriber<StateChange>(observers, observer, lockObj!);

        // Act & Assert
        Should.NotThrow(() => unsubscriber.Dispose());
        observers.ShouldBeEmpty();
    }

    /// <summary>
    /// Executes Unsubscriber_ShouldImplementIDisposable operation.
    /// </summary>

    [Fact]
    public void Unsubscriber_ShouldImplementIDisposable()
    {
        // Arrange
        var observers = new List<IObserver<StateChange>>();
        var observer = Substitute.For<IObserver<StateChange>>();
        var lockObj = new Lock();

        // Act
        var unsubscriber = new Unsubscriber<StateChange>(observers, observer, lockObj);

        // Assert
        unsubscriber.ShouldBeAssignableTo<IDisposable>();
    }

    /// <summary>
    /// Executes Dispose_ShouldBeThreadSafe operation.
    /// </summary>

    [Fact]
    public async Task Dispose_ShouldBeThreadSafeAsync()
    {
        // Arrange
        var observers = new List<IObserver<StateChange>>();
        var observer = Substitute.For<IObserver<StateChange>>();
        var lockObj = new Lock();

        observers.Add(observer);

        var unsubscriber = new Unsubscriber<StateChange>(observers, observer, lockObj);

        // Act - Simulate concurrent disposal
        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() => unsubscriber.Dispose(), TestContext.Current.CancellationToken));
        }

        await Task.WhenAll(tasks.ToArray());

        // Assert
        observers.ShouldBeEmpty();
    }
}