// <copyright file="AdvisoryDestination.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.ValueObjects;

namespace IndTrace.Domain.Routing;

/// <summary>
/// AD-1 role value-type (#56/#94): the closed, three-case classification of the singular §7 PLC
/// "next machine" hint as a <b>transparent discriminated union</b>. It is the typed shape of the one
/// <c>NextMachineId</c> wire scalar — advice the read path emits, <b>never</b> a command: IndTrace tracks and
/// validates, it does not control, so this value never gates an arrival. The
/// <see cref="LegalNextMachines"/> set remains the arrival-validation yardstick.
/// </summary>
/// <remarks>
/// <para>
/// A closed hierarchy (external subclassing is blocked by the private constructor) with exactly three cases,
/// mirroring the three §7 wire states the singular scalar can carry:
/// <list type="bullet">
/// <item><description><see cref="Downstream"/> — one resolved single successor (a positive wire id).</description></item>
/// <item><description><see cref="EndOfLine"/> — the terminal node (wire <c>0</c>, "leave the line").</description></item>
/// <item><description>
/// <see cref="MultipleNext"/> — more than one legal successor exists, so the single scalar names none
/// (wire <c>-1</c>, the diverter sentinel). This case is <b>nullary</b>: it deliberately carries NO machine.
/// </description></item>
/// </list>
/// </para>
/// <para>
/// <b>Why <see cref="MultipleNext"/> is not "unknown".</b> It means PLURAL-and-known, not absent: the legal
/// successors are genuinely known and live in the co-existing <see cref="LegalNextMachines"/> role. AD-1 keeps
/// the two roles split (the surgical fix E6-1 made) — the singular hint here says only "the scalar cannot name
/// a single next", while the set says "here is the full legal successor set". Re-carrying the successors on
/// this case would re-merge the two roles E6-1 deliberately separated, so this case is intentionally empty.
/// </para>
/// <para>
/// <see cref="ToWireScalar"/> and <see cref="Match{T}"/> are polymorphic (abstract, overridden per case) rather
/// than a <c>switch</c> over the base type: the projection back to the wire is TOTAL and exhaustive with no
/// throwing dead branch, honouring warnings-as-errors. The inbound <see cref="FromWireScalar"/> parse is
/// fallible (the scalar arrives from the untrusted §7 wire / persisted int) and returns a
/// <see cref="Result{T}"/> rather than throwing, per the railway-oriented house rule.
/// </para>
/// </remarks>
public abstract record AdvisoryDestination
{
    /// <summary>
    /// The §7 wire scalar for the terminal node: <c>0</c> means "end of line / leave the line".
    /// </summary>
    public const int WireEndOfLine = 0;

    /// <summary>
    /// The §7 wire scalar reserved for a multi-successor diverter (<c>-1</c>): the single scalar names no
    /// successor because more than one legal next exists. Mirrors the read path's diverter sentinel.
    /// </summary>
    public const int WireMultipleNext = -1;

    // Private constructor closes the hierarchy: the only subtypes are the three nested cases below.
    private AdvisoryDestination()
    {
    }

    /// <summary>
    /// Projects this destination back onto the singular §7 wire scalar. TOTAL and exhaustive:
    /// <see cref="Downstream"/> emits its machine's <see cref="MachineId.Value"/>, <see cref="EndOfLine"/>
    /// emits <see cref="WireEndOfLine"/> (<c>0</c>), and <see cref="MultipleNext"/> emits
    /// <see cref="WireMultipleNext"/> (<c>-1</c>).
    /// </summary>
    /// <returns>The §7 wire scalar for this case.</returns>
    public abstract int ToWireScalar();

    /// <summary>
    /// Exhaustively pattern-matches this destination to a value, providing one handler per case. Never throws
    /// on the match itself (the closed hierarchy guarantees exactly one handler runs); a <see langword="null"/>
    /// handler is a caller programming error and is rejected up front.
    /// </summary>
    /// <typeparam name="T">The result type each handler produces.</typeparam>
    /// <param name="onDownstream">Handler for a single resolved successor, receiving its <see cref="MachineId"/>.</param>
    /// <param name="onEndOfLine">Handler for the terminal end-of-line case.</param>
    /// <param name="onMultipleNext">Handler for the plural-and-known (diverter) case.</param>
    /// <returns>The value produced by the handler matching this case.</returns>
    public abstract T Match<T>(Func<MachineId, T> onDownstream, Func<T> onEndOfLine, Func<T> onMultipleNext);

    /// <summary>
    /// Parses a singular §7 wire scalar into its typed destination: a positive value is a
    /// <see cref="Downstream"/> machine id, <see cref="WireEndOfLine"/> (<c>0</c>) is <see cref="EndOfLine"/>,
    /// and <see cref="WireMultipleNext"/> (<c>-1</c>) is <see cref="MultipleNext"/>. This is the exact inverse
    /// of <see cref="ToWireScalar"/> over the three in-contract values.
    /// </summary>
    /// <param name="scalar">The §7 wire scalar (untrusted external input).</param>
    /// <returns>
    /// A success <see cref="Result{T}"/> carrying the destination for an in-contract scalar
    /// (<c>&gt; 0</c>, <c>0</c>, or <c>-1</c>); a failure for an out-of-contract scalar (<c>&lt; -1</c>), which
    /// names no §7 state. Never throws.
    /// </returns>
    public static Result<AdvisoryDestination> FromWireScalar(int scalar) => scalar switch
    {
        > 0 => Result<AdvisoryDestination>.Success(new Downstream(new MachineId(scalar))),
        WireEndOfLine => Result<AdvisoryDestination>.Success(EndOfLine.Instance),
        WireMultipleNext => Result<AdvisoryDestination>.Success(MultipleNext.Instance),
        _ => Result<AdvisoryDestination>.WithFailure(
            $"A next-machine wire scalar of {scalar} is out of contract; only a positive machine id, " +
            $"{WireEndOfLine} (end of line) or {WireMultipleNext} (multiple successors) are legal §7 values."),
    };

    /// <summary>
    /// The case for exactly one resolved single successor — the §7 scalar names a real machine.
    /// </summary>
    /// <param name="Machine">The single legal successor machine.</param>
    public sealed record Downstream(MachineId Machine) : AdvisoryDestination
    {
        /// <inheritdoc/>
        public override int ToWireScalar() => this.Machine.Value;

        /// <inheritdoc/>
        public override T Match<T>(Func<MachineId, T> onDownstream, Func<T> onEndOfLine, Func<T> onMultipleNext)
        {
            ArgumentNullException.ThrowIfNull(onDownstream);
            return onDownstream(this.Machine);
        }
    }

    /// <summary>
    /// The terminal case: the part has reached the end of the line and legally leaves it (§7 wire <c>0</c>).
    /// Nullary — it carries no machine.
    /// </summary>
    public sealed record EndOfLine : AdvisoryDestination
    {
        /// <summary>
        /// A shared, allocation-free instance of the nullary <see cref="EndOfLine"/> case.
        /// </summary>
        public static readonly EndOfLine Instance = new();

        /// <inheritdoc/>
        public override int ToWireScalar() => WireEndOfLine;

        /// <inheritdoc/>
        public override T Match<T>(Func<MachineId, T> onDownstream, Func<T> onEndOfLine, Func<T> onMultipleNext)
        {
            ArgumentNullException.ThrowIfNull(onEndOfLine);
            return onEndOfLine();
        }
    }

    /// <summary>
    /// The plural case: more than one legal successor exists, so the single §7 scalar names none (wire
    /// <c>-1</c>). Nullary — the known successors live in <see cref="LegalNextMachines"/>, not here. This is
    /// PLURAL-and-known, <b>not</b> "unknown": AD-1 keeps the singular-hint and legal-set roles split.
    /// </summary>
    public sealed record MultipleNext : AdvisoryDestination
    {
        /// <summary>
        /// A shared, allocation-free instance of the nullary <see cref="MultipleNext"/> case.
        /// </summary>
        public static readonly MultipleNext Instance = new();

        /// <inheritdoc/>
        public override int ToWireScalar() => WireMultipleNext;

        /// <inheritdoc/>
        public override T Match<T>(Func<MachineId, T> onDownstream, Func<T> onEndOfLine, Func<T> onMultipleNext)
        {
            ArgumentNullException.ThrowIfNull(onMultipleNext);
            return onMultipleNext();
        }
    }
}
