// <copyright file="BarCodeExecutorTimeSnapshotTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Barcodes;

/// <summary>
/// Regression tests for issue #126 F2.1 — torn multi-component time reads: the barcode executors
/// read <c>Year</c> and <c>DayOfYear</c> as two separate provider calls, so a label built across a
/// new-year rollover could stamp the OLD year with the NEW day-of-year (e.g. "25" + "001").
/// Both executors must derive every time component of one label from a single snapshot.
/// </summary>
public class BarCodeExecutorTimeSnapshotTests
{
    private const string YearJulianRuleJson = """
        {
        "ruleId": "SNAP",
        "ruleFunction": ["lastTwoYearDigits", "julianDay"],
        "components": {
            "lastTwoYearDigits": {
                "action": "lastTwoYearDigits",
                "origin": "program"
            },
            "julianDay": {
                "action": "julianDay",
                "origin": "program"
            }
          }
        }
        """;

    /// <summary>
    /// #126 F2.1 — <see cref="CreateBarCodeExecutor"/>: with a clock that rolls into the next year
    /// between consecutive reads, the label must still be internally consistent ("25" + "365"),
    /// never a year/julian-day hybrid of two different instants ("25" + "001").
    /// </summary>
    [Fact]
    public void CreateBarCodeExecutor_YearRollover_ProducesConsistentLabel()
    {
        // Arrange - every provider read advances the clock by one day, starting Dec 31 2025 (UTC).
        var machine = new DateTimeMachine(new SteppingTimeProvider(
            new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero), TimeSpan.FromDays(1)));
        var rule = new Rule { RuleJson = YearJulianRuleJson, IsActive = true };
        var executor = new CreateBarCodeExecutor(machine);

        // Act
        var result = executor.ApplyRuleCreateBarCode(rule, "PART", 1);

        // Assert - one snapshot: year 25 with julian day 365 (2025-12-31), never "25001".
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("25365");
    }

    /// <summary>
    /// #126 F2.1 — <see cref="CreateBarCodeDictionaryExecutor"/>: same rollover consistency for the
    /// dictionary-dispatched component actions.
    /// </summary>
    [Fact]
    public void CreateBarCodeDictionaryExecutor_YearRollover_ProducesConsistentLabel()
    {
        // Arrange
        var machine = new DateTimeMachine(new SteppingTimeProvider(
            new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero), TimeSpan.FromDays(1)));
        var executor = new CreateBarCodeDictionaryExecutor(machine);
        executor.ParseRuleFromJson(YearJulianRuleJson).ShouldNotBeNull();
        executor.InitializeComponentActions().IsSuccess.ShouldBeTrue();

        // Act
        var result = executor.ApplyRuleCreateBarCode("PART", 1);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("25365");
    }

    /// <summary>
    /// Deterministic provider whose clock advances by a fixed step on EVERY read — makes a torn
    /// multi-read observable as two different instants within one label build.
    /// </summary>
    private sealed class SteppingTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset start;
        private readonly TimeSpan step;
        private int reads;

        /// <summary>
        /// Initializes a new instance of the <see cref="SteppingTimeProvider"/> class.
        /// </summary>
        /// <param name="start">The instant returned by the first read.</param>
        /// <param name="step">The amount the clock advances on each subsequent read.</param>
        public SteppingTimeProvider(DateTimeOffset start, TimeSpan step)
        {
            this.start = start;
            this.step = step;
        }

        /// <summary>
        /// Gets the time zone (pinned to UTC so component values are machine-independent).
        /// </summary>
        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;

        /// <summary>
        /// Returns the current instant and advances the clock by one step for the next read.
        /// </summary>
        /// <returns>The instant for this read.</returns>
        public override DateTimeOffset GetUtcNow() => this.start + (this.step * this.reads++);
    }
}
