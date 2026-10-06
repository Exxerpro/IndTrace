// <copyright file="AggregateLoadOptions.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Abstractions.Aggregates;

/// <summary>
/// Describes how much of an aggregate <see cref="IAggregateRepository{TRoot}.LoadAsync"/> materialises.
/// <c>ProductRouting</c> always loads <see cref="Full"/> (the route is tiny); the #40 <c>BarCode</c>
/// aggregate loads a machine-windowed subset via <see cref="ForMachineWindow"/>.
/// </summary>
public sealed record AggregateLoadOptions
{
    private AggregateLoadOptions(IReadOnlyList<int>? machineWindow) => this.MachineWindow = machineWindow;

    /// <summary>
    /// Gets the option that loads the entire aggregate (used by <c>ProductRouting</c> — the whole route).
    /// </summary>
    public static AggregateLoadOptions Full { get; } = new(machineWindow: null);

    /// <summary>
    /// Gets the machine window to load, or <see langword="null"/> for a full load.
    /// </summary>
    public IReadOnlyList<int>? MachineWindow { get; }

    /// <summary>
    /// Creates an option that loads only the aggregate members touching the given machines
    /// (the #40 BarCode windowed load).
    /// </summary>
    /// <param name="machineIds">The machine ids that bound the window.</param>
    /// <returns>A windowed <see cref="AggregateLoadOptions"/>.</returns>
    public static AggregateLoadOptions ForMachineWindow(IReadOnlyList<int> machineIds) => new(machineIds);
}
