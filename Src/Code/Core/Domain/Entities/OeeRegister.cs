// <copyright file="OeeRegister.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

using IndTrace.Domain.Interfaces;
using IndTrace.Domain.Models;

/// <summary>
/// Represents an OEE (Overall Equipment Effectiveness) register, including production metrics, PLC signals, and KPI calculations.
/// </summary>
public class OeeRegister : IEntityRoot
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OeeRegister"/> class.
    /// </summary>
    public OeeRegister()
    {
    }

    /// <summary>
    /// Returns a string representation of the OEE register.
    /// </summary>
    /// <returns>A string containing the OEE register ID, machine ID, OEE percentage, and timestamp.</returns>
    // [Fix]
    // CLAUDE
    // Date: 23/08/2025
    // Reason: Added ToString() implementation for better debugging and logging experience
    public override string ToString() => $"OEE {this.OeeRegisterId} (Machine {this.MachineId}): {this.Oee:P1} at {this.TimeStamp:yyyy-MM-dd HH:mm}";

    /// <summary>
    /// Gets or sets the unique identifier for the OEE register.
    /// </summary>
    public int OeeRegisterId { get; set; }

    /// <summary>
    /// Gets or sets the machine identifier associated with the OEE register.
    /// </summary>
    public int MachineId { get; set; }

    /// <summary>
    /// Gets or sets the PLC identifier associated with the OEE register.
    /// </summary>
    public int PlcId { get; set; }

    /// <summary>
    /// Gets or sets the timestamp for the OEE register.
    /// </summary>
    public DateTime TimeStamp { get; set; }

    /// <summary>
    /// Gets or sets the application flag from the PLC.
    /// </summary>
    public int ApplicationFlag { get; set; }

    /// <summary>
    /// Gets or sets the event counter from the PLC.
    /// </summary>
    public int EventCounter { get; set; }

    /// <summary>
    /// Gets or sets the current time from the PLC.
    /// </summary>
    public int CurrentTime { get; set; }

    /// <summary>
    /// Gets or sets the running time from the PLC.
    /// </summary>
    public int RunningTime { get; set; }

    /// <summary>
    /// Gets or sets the stopped time from the PLC.
    /// </summary>
    public int StoppedTime { get; set; }

    /// <summary>
    /// Gets or sets the faulted time from the PLC.
    /// </summary>
    public int FaultedTime { get; set; }

    /// <summary>
    /// Gets or sets the status fault reason from the PLC.
    /// </summary>
    public int StatusFaultReason { get; set; }

    /// <summary>
    /// Gets or sets the product identifier associated with the OEE register.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets the total production value.
    /// </summary>
    public double TotalProduction { get; set; }

    /// <summary>
    /// Gets or sets the standard cycle time.
    /// </summary>
    public double StandardCycleTime { get; set; }

    /// <summary>
    /// Gets or sets the actual cycle time.
    /// </summary>
    public double ActualCycleTime { get; set; }

    /// <summary>
    /// Gets or sets the planned production time.
    /// </summary>
    public double PlanedProductionTime { get; set; }

    /// <summary>
    /// Gets or sets the reject event counter.
    /// </summary>
    public int RejectEventCounter { get; set; }

    /// <summary>
    /// Gets or sets the status reject value.
    /// </summary>
    public int StatusReject { get; set; }

    /// <summary>
    /// Gets or sets the quantity of rejected units.
    /// </summary>
    public double RejectQuantityUnits { get; set; }

    /// <summary>
    /// Gets or sets the quantity of OK production units.
    /// </summary>
    public double ProductionOk { get; set; }

    /// <summary>
    /// Gets or sets the quantity of NOK production units.
    /// </summary>
    public double ProductionNoK { get; set; }

    /// <summary>
    /// Gets or sets the associated KPI OEE entity. Nullable EF navigation (EF populates it on load), mirroring the
    /// shipped <c>Product.Line</c>/<c>Product.Customer</c> pattern (#81) — no <c>null!</c> placeholder.
    /// </summary>
    public KpiOee? KpiOee { get; set; }

    /// <summary>
    /// Gets or sets the OEE value (not mapped to the database).
    /// </summary>
    public double Oee { get; set; }

    /// <summary>
    /// Gets or sets the availability value (not mapped to the database).
    /// </summary>
    public double Availability { get; set; }

    /// <summary>
    /// Gets or sets the performance value (not mapped to the database).
    /// </summary>
    public double Performance { get; set; }

    /// <summary>
    /// Gets or sets the quality value (not mapped to the database).
    /// </summary>
    public double Quality { get; set; }

    /// <summary>
    /// Creates a KpiOee object from the current OEE register with sanitized metrics.
    /// </summary>
    /// <param name="register">The OEE register to convert.</param>
    /// <param name="dateTimeMachine">The date time machine to use for timestamp operations. Defaults to new DateTimeMachine() if null.</param>
    /// <returns>A new KpiOee object with sanitized metrics.</returns>
    public static KpiOee ToKpiOee(OeeRegister register, IDateTimeMachine? dateTimeMachine = null)
    {
        ArgumentNullException.ThrowIfNull(register);

        var dtm = dateTimeMachine ?? new DateTimeMachine();

        // Sanitize values before Math.Round to prevent NaN/Infinity propagation
        var quality = ClampMetric(register.Quality, 0.0, 1.0);
        var availability = ClampMetric(register.Availability, 0.0, 1.0);
        var performance = ClampMetric(register.Performance, 0.0, 1.5);
        var oee = ClampMetric(register.Oee, 0.0, 1.0);

        return KpiOee.CreateFixture(
            0,
            0,
            Math.Round(oee, 6),
            Math.Round(availability, 6),
            Math.Round(performance, 6),
            Math.Round(quality, 6),
            dtm.Now);
    }

    /// <summary>
    /// Calculates OEE metrics and returns a result containing the updated register and any warnings or errors.
    /// The <paramref name="data"/> sample is treated as an immutable raw PLC reading (#126 F1.1): all
    /// sanitation happens on local copies and only the returned <paramref name="register"/> carries the
    /// sanitized/computed values. Impossible readings — negative production counters, negative time
    /// components, or OK + NOK counts exceeding a positive reported total — produce a failure Result
    /// (#126 F1.2, PO decision) instead of a silently clamped success.
    /// </summary>
    /// <param name="register">The OEE register to update.</param>
    /// <param name="data">The performance data to use for calculation. Never mutated.</param>
    /// <returns>A <see cref="Result{OeeRegister}"/> containing the updated register and any warnings or errors.</returns>
    public static Result<OeeRegister> CalculateOee(OeeRegister register, PerformanceData data)
    {
        var warnings = new List<string>();
        var errors = new List<string>();

        if (register == null || data == null)
        {
            return Result<OeeRegister>.WithFailure("Inputs cannot be null");
        }

        // #126 F1.1 — local sanitized copies; the raw PLC sample is persisted downstream and must survive.
        var totalProduction = data.TotalProduction;
        var productionOk = data.ProductionOk;
        var productionNoK = data.ProductionNoK;
        var currentTime = data.CurrentTime;
        var runningTime = data.RunningTime;
        var stoppedTime = data.StoppedTime;
        var faultedTime = data.FaultedTime;

        // #126 F1.2 — negative counters/times are impossible PLC readings: reject the sample (error),
        // but keep computing on zero-clamped locals so the failure still carries a bounded register.
        if (totalProduction < 0)
        {
            errors.Add("TotalProduction was negative; the PLC sample is invalid.");
            totalProduction = 0;
        }

        if (productionOk < 0)
        {
            errors.Add("ProductionOk was negative; the PLC sample is invalid.");
            productionOk = 0;
        }

        if (productionNoK < 0)
        {
            errors.Add("ProductionNoK was negative; the PLC sample is invalid.");
            productionNoK = 0;
        }

        // A positive reported total contradicted by its own counts is an inconsistency error; a zero
        // total with counts stays the tolerated warning case. NOTE (#126 review, LOW 6): with a zero
        // total the OK/NOK counts are clamped to zero by the range clamps just below, BEFORE the
        // TotalProduction fallback runs — so the fallback always re-derives 0 and a zero total is never
        // actually resurrected from the counts (Quality then takes its denominator fallback of 1.0).
        if (totalProduction > 0 && productionOk + productionNoK > totalProduction)
        {
            errors.Add("ProductionOk + ProductionNoK exceeded TotalProduction; production counters are inconsistent.");
        }

        if (productionOk > totalProduction)
        {
            if (totalProduction <= 0)
            {
                warnings.Add("ProductionOk was outside valid range and was clamped.");
            }

            productionOk = Math.Max(0, Math.Min(productionOk, totalProduction));
        }

        if (productionNoK > totalProduction - productionOk)
        {
            if (totalProduction <= 0)
            {
                warnings.Add("ProductionNoK was outside valid range and was clamped.");
            }

            productionNoK = Math.Max(0, Math.Min(productionNoK, totalProduction - productionOk));
        }

        if (currentTime < 0)
        {
            errors.Add("CurrentTime was negative; the PLC sample is invalid.");
            currentTime = 0;
        }

        if (runningTime < 0)
        {
            errors.Add("RunningTime was negative; the PLC sample is invalid.");
            runningTime = 0;
        }

        if (stoppedTime < 0)
        {
            errors.Add("StoppedTime was negative; the PLC sample is invalid.");
            stoppedTime = 0;
        }

        if (faultedTime < 0)
        {
            errors.Add("FaultedTime was negative; the PLC sample is invalid.");
            faultedTime = 0;
        }

        // #126 F1.3 — explicit flag instead of sniffing warning message text further down.
        var plannedTimeWasNegative = false;
        if (register.PlanedProductionTime < 0)
        {
            warnings.Add("PlannedProductionTime was negative and clamped to zero.");
            plannedTimeWasNegative = true;
            register.PlanedProductionTime = Math.Max(0, register.PlanedProductionTime);
        }

        if (register.ActualCycleTime < 0)
        {
            warnings.Add("ActualCycleTime was negative and clamped to zero.");
            register.ActualCycleTime = Math.Max(0, register.ActualCycleTime);
        }

        if (register.StandardCycleTime < 0)
        {
            warnings.Add("StandardCycleTime was negative and clamped to zero.");
            register.StandardCycleTime = Math.Max(0, register.StandardCycleTime);
        }

        // Note: Individual clamping already handled above with warnings

        // Clamp PlanedProductionTime to CurrentTime
        if (register.PlanedProductionTime > currentTime)
        {
            warnings.Add($"PlannedProductionTime ({register.PlanedProductionTime}) was greater than CurrentTime ({currentTime}) and was clamped.");
            register.PlanedProductionTime = currentTime;
        }

        // Scale down FaultedTime + StoppedTime if they exceed CurrentTime
        // Use long arithmetic to prevent integer overflow in the comparison
        if (((long)faultedTime + stoppedTime) > currentTime && currentTime > 0)
        {
            double totalTime = (double)faultedTime + stoppedTime;
            if (totalTime > 0)
            {
                double scale = currentTime / totalTime;

                // Prevent overflow by using double arithmetic and clamping
                double scaledFaulted = faultedTime * scale;
                double scaledStopped = stoppedTime * scale;

                faultedTime = (int)Math.Min(Math.Round(scaledFaulted), currentTime);
                stoppedTime = (int)Math.Min(Math.Round(scaledStopped), currentTime);
                warnings.Add("FaultedTime + StoppedTime exceeded CurrentTime and were proportionally scaled down.");
            }
            else
            {
                // Handle edge case where both are zero but somehow triggered the condition
                faultedTime = 0;
                stoppedTime = 0;
            }
        }

        // Transfer shared fields (initial transfer - may be updated by fallback logic below)
        register.TotalProduction = totalProduction;
        register.CurrentTime = currentTime;
        register.RunningTime = runningTime;
        register.StoppedTime = stoppedTime;
        register.FaultedTime = faultedTime;
        register.ApplicationFlag = data.ApplicationFlag;
        register.EventCounter = data.EventCounter;
        register.StatusFaultReason = data.StatusFaultReason;

        // Fallback PlanedProductionTime (only if it was originally zero, not clamped from negative —
        // #126 F1.3: steered by the explicit flag, not by warning message text)
        if (register.PlanedProductionTime <= 0 && !plannedTimeWasNegative)
        {
            register.PlanedProductionTime = runningTime + stoppedTime + faultedTime;
            warnings.Add("PlanedProductionTime was zero and computed from time aggregates.");
        }

        // Fallback StandardCycleTime
        if (register.StandardCycleTime <= 0)
        {
            register.StandardCycleTime = register.ActualCycleTime > 0 ? register.ActualCycleTime : 1.0;
            warnings.Add("StandardCycleTime was zero and computed from ActualCycleTime or set to 1.0.");
        }

        // Fallback RunningTime
        if (runningTime <= 0)
        {
            runningTime = currentTime - stoppedTime;
            warnings.Add("RunningTime was zero and computed from CurrentTime - StoppedTime.");

            // Check if computed value is negative and clamp
            if (runningTime < 0)
            {
                runningTime = 0;
                warnings.Add("Computed RunningTime was negative and clamped to zero.");
            }

            // Synchronize the register with the computed fallback (the raw sample keeps its zero).
            register.RunningTime = runningTime;
        }

        // Fallback TotalProduction. NOTE (#126 review, LOW 6): by this point productionOk/productionNoK
        // have already been clamped to at most totalProduction, so when totalProduction <= 0 both counts
        // are 0 and this "fallback" always computes 0 — the historical warning text is preserved verbatim,
        // but a zero total is never actually recovered from the counts.
        if (totalProduction <= 0)
        {
            totalProduction = productionOk + productionNoK;
            warnings.Add("TotalProduction was zero and computed from OK + NOK counts.");

            // Synchronize the register with the fallback total (mirrors the RunningTime fallback above).
            register.TotalProduction = totalProduction;
        }

        // Quality
        register.Quality = SafeRatio(productionOk, totalProduction, 0.0, 1.0, 1.0);

        // Availability
        register.Availability = SafeRatio(runningTime, register.PlanedProductionTime, 0.0, 1.0, 0.0);

        // Performance — #126 F1.4 (PO decision): with no observable running time the ratio is
        // unknowable, so report 0% with a warning instead of the flattering 100% fallback.
        double perfNumerator = register.StandardCycleTime * totalProduction;
        if (runningTime <= 0)
        {
            warnings.Add("RunningTime was zero or negative; Performance reported as 0.");
            register.Performance = 0.0;
        }
        else
        {
            register.Performance = SafeRatio(perfNumerator, runningTime, 0.0, 1.5, 1.0);
        }

        // OEE
        double rawOee = register.Availability * register.Performance * register.Quality;
        register.Oee = ClampMetric(rawOee, 0.0, 1.0);

        // MVP estimation: treat each warning as a proxy for missing/estimated data.
        // Compute a simple missing data ratio and a derived confidence score.
        var missingDataRatio = warnings.Count switch
        {
            0 => 0.0,
            <= 2 => 0.1,
            <= 5 => 0.25,
            <= 10 => 0.5,
            _ => 0.75,
        };
        var confidence = Math.Max(0.1, 1.0 - missingDataRatio); // guarantee a floor of 0.1

        return (warnings.Count, errors.Count) switch
        {
            ( > 0, > 0) => Result<OeeRegister>.CombineErrors(errors, warnings, register),
            ( > 0, 0) => Result<OeeRegister>.WithWarnings(warnings, register, confidence, missingDataRatio),
            (0, > 0) => Result<OeeRegister>.WithFailure(errors, register),
            _ => Result<OeeRegister>.Success(register),
        };
    }

    /// <summary>
    /// Clamps a metric value between the specified minimum and maximum.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <param name="min">The minimum allowed value.</param>
    /// <param name="max">The maximum allowed value.</param>
    /// <returns>The clamped value.</returns>
    public static double ClampMetric(double value, double min, double max)
    {
        // Handle special values first
        if (double.IsNaN(value))
        {
            return min; // Default to minimum for NaN
        }

        if (double.IsPositiveInfinity(value))
        {
            return max;
        }

        if (double.IsNegativeInfinity(value))
        {
            return min;
        }

        // Normal clamping logic
        return value switch
        {
            var v when v < min => min,
            var v when v > max => max,
            _ => value,
        };
    }

    /// <summary>
    /// Safely calculates a ratio, clamping the result and providing a fallback if the denominator is zero or negative.
    /// </summary>
    /// <param name="numerator">The numerator value.</param>
    /// <param name="denominator">The denominator value.</param>
    /// <param name="min">The minimum allowed value.</param>
    /// <param name="max">The maximum allowed value.</param>
    /// <param name="fallback">The fallback value if the denominator is zero or negative.</param>
    /// <returns>The calculated and clamped ratio, or the fallback value.</returns>
    public static double SafeRatio(double numerator, double denominator, double min, double max, double fallback)
    {
        // Handle NaN or Infinity inputs immediately
        if (double.IsNaN(numerator) || double.IsNaN(denominator) ||
            double.IsInfinity(numerator) || double.IsInfinity(denominator))
        {
            return fallback;
        }

        if (denominator <= 0)
        {
            return fallback;
        }

        var value = numerator / denominator;

        // Check for NaN or Infinity results from division
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            return fallback;
        }

        return ClampMetric(value, min, max);
    }
}