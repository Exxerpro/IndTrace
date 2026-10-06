// <copyright file="FixedSizedStack.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Models.Helpers;

using System.Collections.Concurrent;

/// <summary>
/// Represents a fixed-size stack with FIFO behavior and thread safety.
/// </summary>
public class FixedSizedStack<T>(int size)
{
    private readonly ConcurrentQueue<T> queue = new(); // Use ConcurrentQueue for FIFO behavior
    private readonly object syncLock = new(); // Use an object for locking

    // #128 (Chunk B): O(1) membership mirror of the queue's contents, maintained in lockstep with the queue
    // under syncLock (add on push, remove on eviction) so the per-Push dedup check is O(1) instead of the
    // former O(n) queue.Contains scan. Only ever touched inside the lock, so it stays consistent with the queue.
    private readonly HashSet<T> membership = [];

    /// <summary>
    /// The default size of the stack.
    /// </summary>
    public const int DefaultSize = 80;

    /// <summary>
    /// Gets or sets the maximum number of items allowed in the stack.
    /// </summary>
    public int Limit { get; set; } = size;

    /// <summary>
    /// Initializes a new instance of the <see cref="FixedSizedStack{T}"/> class with the default size.
    /// </summary>
    public FixedSizedStack()
        : this(DefaultSize)
    {
    }

    /// <summary>
    /// Adds an item to the stack if it does not already exist.
    /// </summary>
    /// <param name="item">The item to add.</param>


    /// <summary>
    /// Executes Push operation.
    /// </summary>
    /// <param name="item">The item.</param>
    public void Push(T item)
    {
        if (item is null)
        {
            return;
        }

        if (item is string str && string.IsNullOrEmpty(str))
        {
            return;
        }

        lock (this.syncLock) // Lock to ensure thread safety
        {
            // Check if the item already exists in the queue (O(1) via the membership mirror)
            if (this.membership.Contains(item))
            {
                return; // Do not push the item if it already exists in the queue
            }

            this.queue.Enqueue(item); // Add the new item to the queue
            this.membership.Add(item); // Mirror the addition for O(1) dedup
            while (this.queue.Count > this.Limit) // If the limit is exceeded
            {
                if (this.queue.TryDequeue(out var removed)) // Remove the oldest item
                {
                    this.membership.Remove(removed); // Keep the mirror in lockstep so it can be pushed again later
                }
                else
                {
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Returns the oldest item in the stack without removing it.
    /// </summary>
    /// <returns>The oldest item in the stack.</returns>
    public T? Peek()
    {
        if (this.queue.TryPeek(out T? result) && result is not null)
        {
            return result; // Return the oldest item without removing it
        }
        else
        {
            return default;
        }
    }

    /// <summary>
    /// Returns the items in the stack in LIFO order as an enumerable.
    /// </summary>
    /// <returns>An enumerable of items in LIFO order.</returns>
    public IEnumerable<T> ToEnumerable()
    {
        return this.queue.Reverse();
    }

    /// <summary>
    /// Converts the stack to an <see cref="IReadOnlyCollection{T}"/> in LIFO order.
    /// </summary>
    /// <returns>An <see cref="IReadOnlyCollection{T}"/> containing items in LIFO order.</returns>
    public IReadOnlyCollection<T> ToReadOnlyCollection()
    {
        return this.queue.Reverse().ToArray(); // Materialize the IEnumerable to an array, which implements IReadOnlyCollection
    }

    /// <summary>
    /// Gets the number of items in the stack.
    /// </summary>
    public int Count => this.queue.Count;

    /// <summary>
    /// Converts the stack to an array.
    /// </summary>
    /// <returns>An array containing the items in the stack.</returns>
    public T[] ToArray() => this.queue.ToArray();
}