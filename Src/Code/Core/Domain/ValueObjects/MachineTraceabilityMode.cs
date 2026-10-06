// <copyright file="MachineTraceabilityMode.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.ValueObjects;

using IndTrace.Domain.Models;

/// <summary>
/// Immutable value object modelling a machine's two-flag traceability gate (security-by-design).
/// <para>
/// The gate is encoded as a pair of ints — application-traceability and bypass-traceability — and the
/// machine is <see cref="IsEnabled">disabled only on the exact combination (App=0, Bypass=1)</see>; every
/// other combination stays enabled (fail-safe). This is an intentional safety gate (PO-ratified), NOT
/// primitive obsession: it must wrap BOTH ints and must never be collapsed to a single boolean. The two
/// ints remain the persisted system of record on <c>Machine</c>; this value object is the typed home of
/// the gate rule and the two canonical transitions (<see cref="Enabled"/>/<see cref="Disabled"/>).
/// </para>
/// </summary>
public sealed class MachineTraceabilityMode : ValueObject
{
    /// <summary>
    /// The canonical "enabled" mode written by <c>Machine.Enable()</c>: App=1, Bypass=0.
    /// </summary>
    public static readonly MachineTraceabilityMode Enabled = new(1, 0);

    /// <summary>
    /// The canonical "disabled" mode written by <c>Machine.Disable()</c>: App=0, Bypass=1 — the single
    /// combination that the fail-safe gate treats as disabled.
    /// </summary>
    public static readonly MachineTraceabilityMode Disabled = new(0, 1);

    private MachineTraceabilityMode(int appTraceability, int bypassTraceability)
    {
        this.AppTraceability = appTraceability;
        this.BypassTraceability = bypassTraceability;
    }

    /// <summary>
    /// Gets the application-traceability flag value.
    /// </summary>
    public int AppTraceability { get; }

    /// <summary>
    /// Gets the bypass-traceability flag value.
    /// </summary>
    public int BypassTraceability { get; }

    /// <summary>
    /// Gets a value indicating whether the machine is enabled under this mode. Disabled only on the exact
    /// combination (App=0, Bypass=1); all other combinations — including (1,1) and (0,0) and any
    /// non-canonical value — remain enabled by design (fail-safe).
    /// </summary>
    public bool IsEnabled => !(this.AppTraceability == 0 && this.BypassTraceability == 1);

    /// <summary>
    /// Wraps a raw flag pair as a traceability mode. Deliberately non-validating: the gate is fail-safe for
    /// every integer input, and the stored flags may carry any value, so this never rejects.
    /// </summary>
    /// <param name="appTraceability">The application-traceability flag value.</param>
    /// <param name="bypassTraceability">The bypass-traceability flag value.</param>
    /// <returns>A mode over the supplied flag pair.</returns>
    public static MachineTraceabilityMode FromFlags(int appTraceability, int bypassTraceability) =>
        new(appTraceability, bypassTraceability);

    /// <inheritdoc/>
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.AppTraceability;
        yield return this.BypassTraceability;
    }
}
