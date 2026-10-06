// <copyright file="KpiOee.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

using IndTrace.Domain.Constants;
using IndTrace.Domain.Interfaces;
using IndTrace.Domain.ValueObjects;

/// <summary>
/// Represents OEE (Overall Equipment Effectiveness) KPI values and their association with an OEE register.
/// </summary>
public class KpiOee : IEntityRoot
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KpiOee"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public KpiOee()
    {
        // Story 26.B1: the metric properties are now non-nullable value objects. Seed them with a zero-valued VO
        // (via the total FromPersisted seam) so the public parameterless constructor stays usable and the CS8618
        // non-null guarantee holds without a `required` gate or a banned `null!`. This mirrors the pre-retype
        // default(double) == 0.0 semantics — Create/CreateFixture/EF overwrite these on any real instance.
        this.Oee = Ratio.FromPersisted(0.0);
        this.Availability = Ratio.FromPersisted(0.0);
        this.Performance = PerformanceRatio.FromPersisted(0.0);
        this.Quality = Ratio.FromPersisted(0.0);
    }

    /// <summary>
    /// Returns a string representation of the KPI OEE entry.
    /// </summary>
    /// <returns>A string containing the KPI OEE ID, OEE percentage, and timestamp.</returns>
    // [Fix]
    // CLAUDE
    // Date: 23/08/2025
    // Reason: Added ToString() implementation for better debugging and logging experience
    public override string ToString() => $"KPI {this.KpiOeeId}: OEE {this.Oee.Value:P1} at {this.TimeStamp:yyyy-MM-dd HH:mm}";

    /// <summary>
    /// Gets or sets the unique identifier for the KPI OEE entry. Identity stays publicly settable (assigned by
    /// the database / test seams), mirroring the Recipe and Cycle precedent.
    /// </summary>
    public int KpiOeeId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the associated OEE register. This FK stays publicly settable, mirroring
    /// the Recipe and Cycle precedent.
    /// </summary>
    public int OeeRegisterId { get; set; }

    /// <summary>
    /// Gets the OEE value as a <see cref="Ratio"/> (<c>[0, 1]</c>). Story 2.5: the setter is <c>private set</c> so
    /// the valid-range invariant originates ONLY from <see cref="Create"/> (the guarded factory), the
    /// <see cref="CreateFixture"/> test-data/seam factory, or EF Core materialization. Story 26.B1 retyped this
    /// off raw <c>double</c> so the <c>[0, 1]</c> bound is enforced by the type; the underlying <c>double</c> is
    /// exposed via <see cref="Ratio.Value"/>. Mapped to the unchanged <c>decimal(12,4)</c> column via an EF value
    /// converter (no schema change).
    /// </summary>
    public Ratio Oee { get; private set; }

    /// <summary>
    /// Gets the availability value as a <see cref="Ratio"/> (<c>[0, 1]</c>). Story 2.5: the setter is
    /// <c>private set</c> (see <see cref="Oee"/>). Story 26.B1 retyped it off raw <c>double</c>.
    /// </summary>
    public Ratio Availability { get; private set; }

    /// <summary>
    /// Gets the performance value as a <see cref="PerformanceRatio"/> (<c>[0, 1.5]</c>, allowing over-performance
    /// up to 150%). Story 2.5: the setter is <c>private set</c> (see <see cref="Oee"/>). Story 26.B1 retyped it
    /// off raw <c>double</c>.
    /// </summary>
    public PerformanceRatio Performance { get; private set; }

    /// <summary>
    /// Gets the quality value as a <see cref="Ratio"/> (<c>[0, 1]</c>). Story 2.5: the setter is <c>private set</c>
    /// (see <see cref="Oee"/>). Story 26.B1 retyped it off raw <c>double</c>.
    /// </summary>
    public Ratio Quality { get; private set; }

    /// <summary>
    /// Gets the timestamp for the KPI OEE entry. Story 2.5: the setter is <c>private set</c> (see
    /// <see cref="Oee"/>).
    /// </summary>
    public DateTime TimeStamp { get; private set; }

    /// <summary>
    /// Gets or sets the associated OEE register entity. The navigation property stays publicly settable for EF
    /// Core fix-up and is nullable — mirroring the shipped <c>Product.Line</c>/<c>Product.Customer</c> EF-navigation
    /// pattern (#81) so no phantom-nav placeholder or <c>null!</c> is needed; EF populates it on load.
    /// </summary>
    public OeeRegister? OeeRegister { get; set; }

    /// <summary>
    /// Story 2.5 PUBLIC creation seam. Constructs a validated KPI OEE entry, enforcing that every metric is a
    /// finite number within its valid range: <paramref name="oee"/>, <paramref name="availability"/> and
    /// <paramref name="quality"/> in <c>[0, 1]</c>, and <paramref name="performance"/> in <c>[0, 1.5]</c>
    /// (performance allows overperformance up to 150%, matching <c>OeeRegister.ToKpiOee</c>'s clamp ranges).
    /// Identity/FK inputs (<paramref name="oeeRegisterId"/>) and <paramref name="timeStamp"/> are NOT
    /// range-guarded and are stored exactly as supplied. The database-assigned <c>KpiOeeId</c> is not a
    /// parameter; set it post-construction where needed.
    /// </summary>
    /// <param name="oeeRegisterId">The identifier of the associated OEE register.</param>
    /// <param name="oee">The OEE value. Must be finite and within <c>[0, 1]</c>.</param>
    /// <param name="availability">The availability value. Must be finite and within <c>[0, 1]</c>.</param>
    /// <param name="performance">The performance value. Must be finite and within <c>[0, 1.5]</c>.</param>
    /// <param name="quality">The quality value. Must be finite and within <c>[0, 1]</c>.</param>
    /// <param name="timeStamp">The timestamp for the KPI OEE entry.</param>
    /// <returns>A success result carrying the KPI OEE entry, or a failure result aggregating the violated invariants.</returns>
    public static Result<KpiOee> Create(
        int oeeRegisterId,
        double oee,
        double availability,
        double performance,
        double quality,
        DateTime timeStamp)
    {
        var errors = new List<string>();

        ValidateMetric(nameof(oee), oee, OeeConstants.PerformanceThresholds.MinOeeValue, OeeConstants.PerformanceThresholds.MaxOeeValue, errors);
        ValidateMetric(nameof(availability), availability, OeeConstants.MetricThresholds.MinAvailability, OeeConstants.MetricThresholds.MaxAvailability, errors);
        ValidateMetric(nameof(performance), performance, OeeConstants.MetricThresholds.MinPerformance, OeeConstants.MetricThresholds.MaxAllowablePerformance, errors);
        ValidateMetric(nameof(quality), quality, OeeConstants.MetricThresholds.MinQuality, OeeConstants.MetricThresholds.MaxQuality, errors);

        if (errors.Count > 0)
        {
            return Result<KpiOee>.WithFailure(errors);
        }

        // Validation above already guaranteed each metric is finite and within its bound, so the total
        // FromPersisted seam is the correct (non-throwing) way to lift the sanitized doubles into their VOs.
        return Result<KpiOee>.Success(new KpiOee
        {
            OeeRegisterId = oeeRegisterId,
            Oee = Ratio.FromPersisted(oee),
            Availability = Ratio.FromPersisted(availability),
            Performance = PerformanceRatio.FromPersisted(performance),
            Quality = Ratio.FromPersisted(quality),
            TimeStamp = timeStamp,
        });
    }

    /// <summary>
    /// Story 2.5 INTERNAL SEAM. Internal unguarded construction for callers that have already sanitized inputs
    /// (<c>OeeRegister.ToKpiOee</c>) and for test-data builders; NOT public — production external construction
    /// must use <see cref="Create"/>. Setting the metric fields here is legal even after Story 2.5 restricts
    /// the setters to <c>private set</c> because this factory lives INSIDE <c>IndTrace.Domain</c>. Exposed to
    /// the granted assemblies via <c>InternalsVisibleTo</c>.
    /// </summary>
    /// <param name="kpiOeeId">The KPI OEE identifier to seed.</param>
    /// <param name="oeeRegisterId">The associated OEE register identifier to seed.</param>
    /// <param name="oee">The OEE value to seed, unvalidated.</param>
    /// <param name="availability">The availability value to seed, unvalidated.</param>
    /// <param name="performance">The performance value to seed, unvalidated.</param>
    /// <param name="quality">The quality value to seed, unvalidated.</param>
    /// <param name="timeStamp">The timestamp to seed.</param>
    /// <returns>A KPI OEE entry seeded with the supplied values.</returns>
    internal static KpiOee CreateFixture(
        int kpiOeeId,
        int oeeRegisterId,
        double oee,
        double availability,
        double performance,
        double quality,
        DateTime timeStamp) =>
        new()
        {
            KpiOeeId = kpiOeeId,
            OeeRegisterId = oeeRegisterId,

            // Unguarded seam: FromPersisted is total, so it HOLDS arbitrary/legacy values (e.g. an OEE above 1.0
            // or a performance above 1.5) exactly as supplied, preserving the pre-retype CreateFixture behavior.
            Oee = Ratio.FromPersisted(oee),
            Availability = Ratio.FromPersisted(availability),
            Performance = PerformanceRatio.FromPersisted(performance),
            Quality = Ratio.FromPersisted(quality),
            TimeStamp = timeStamp,
        };

    /// <summary>
    /// Classifies this entry's stored <see cref="Oee"/> into its <see cref="OeePerformanceLevel"/> band via the
    /// single banding source, <see cref="OeeMetrics.ClassifyLevel"/>. This bands the STORED OEE directly (no
    /// Availability x Performance x Quality recomposition), so it is unaffected by performance values above 1.0
    /// and by decimal-versus-double rounding. Exposed as a method (not a property) so it does not add to the
    /// entity's mapped property surface.
    /// </summary>
    /// <returns>The performance level band for the stored OEE value.</returns>
    public OeePerformanceLevel ClassifyOeeLevel()
    {
        // #91 guard: Oee is a total Ratio (FromPersisted/EF can hold a corrupt non-finite double). A NaN or
        // Infinity `double` throws OverflowException on the `(decimal)` cast, which would crash banding on a
        // corrupt row. A non-finite OEE is not a real performance figure, so fail SAFE to the worst band
        // (Poor) rather than throw. The finite happy path is unchanged (byte-identical banding).
        var value = this.Oee.Value;
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            return OeePerformanceLevel.Poor;
        }

        return OeeMetrics.ClassifyLevel((decimal)value);
    }

    /// <summary>
    /// Adds a validation error when <paramref name="value"/> is non-finite or outside the inclusive range.
    /// </summary>
    private static void ValidateMetric(string name, double value, double min, double max, List<string> errors)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            errors.Add($"{name} must be a finite number");
            return;
        }

        if (value < min || value > max)
        {
            errors.Add($"{name} must be between {min} and {max}");
        }
    }
}
