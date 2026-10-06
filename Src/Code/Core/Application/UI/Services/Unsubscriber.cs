// <copyright file="Unsubscriber.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.UI.Services;

/// <summary>
/// Provides a mechanism to unsubscribe an observer from a list of observers in a thread-safe manner.
/// </summary>
/// <typeparam name="TStateChange">The type of state change being observed.</typeparam>
public class Unsubscriber<TStateChange> : IDisposable
{
    private readonly List<IObserver<TStateChange>> observers;
    private readonly IObserver<TStateChange> observer;
    private readonly Lock @lock;

    /// <summary>
    /// Initializes a new instance of the <see cref="Unsubscriber{TStateChange}"/> class.
    /// </summary>
    /// <param name="observers">The list of observers to manage.</param>
    /// <param name="observer">The specific observer to unsubscribe.</param>
    /// <param name="syncRoot">The lock object for thread synchronization.</param>
    public Unsubscriber(List<IObserver<TStateChange>> observers, IObserver<TStateChange> observer, Lock syncRoot)
    {
        this.observers = observers;
        this.observer = observer;
        this.@lock = syncRoot;
    }

    /// <summary>
    /// Removes the observer from the observers list in a thread-safe manner.
    /// </summary>
    public void Dispose()
    {
        // [Fix]
        // COPILOT
        // Date: 2024-06-10
        // Reason: .NET best practice - Remove all instances of observer from the list in one call for robust unsubscribe logic
        if (this.@lock != null)
        {
            lock (this.@lock)
            {
                this.observers?.RemoveAll(x => x == this.observer);
            }
        }
        else
        {
            // Handle null lock case - still perform operation but without synchronization
            this.observers?.RemoveAll(x => x == this.observer);
        }
    }

}