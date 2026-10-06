// <copyright file="RegisterCleanerGoldenMasterTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.UnitTests.Cycles.Services;

using IndTrace.Application.Cycles.Services;
using Meziantou.Extensions.Logging.Xunit;

/// <summary>
/// Characterization (golden-master) tests for <see cref="RegisterCleaner.CleanRegisters"/> — the PERSISTED
/// register-producing path (#39 Task 6, design §2 / §5 / §8). <see cref="RegisterCleaner"/> reconstructs the
/// row that is actually saved (<c>new Register { ... }</c>); the live mutated PLC-tag instances are never
/// persisted directly. These tests pin the EXACT produced output byte-identical so the upcoming
/// mutate-in-place -&gt; reconstruction / immutability rework can be proven behaviour-preserving.
///
/// <para>
/// The contract pinned here is the reconstructed field set: the cleaner copies
/// <c>Name, Description, Value, CycleId, MachineId, RegisterId(=0), TimeStamp</c> AND now CARRIES the source
/// <c>VariableId</c> (#96) — it must satisfy the ENABLED <c>Registers.VariableId -&gt; Variables</c> FK on real
/// SQL, so hardcoding it to 0 raised SqlException 547 on QA45. Only <c>DataType, StatusValueId</c> remain
/// intentionally DROPPED (design §2 / §8) and land at their type defaults (<c>DataType = ""</c>,
/// <c>StatusValueId = 0</c>) on the persisted row, REGARDLESS of what the source live register carried.
/// DO NOT "fix" the two remaining drops — that is the current persisted-bytes contract.
/// </para>
/// </summary>
public class RegisterCleanerGoldenMasterTests
{
    private readonly RegisterCleaner _cleaner;
    private readonly DateTime _timestamp = new(2026, 6, 30, 8, 30, 15, DateTimeKind.Local);

    public RegisterCleanerGoldenMasterTests(ITestOutputHelper output)
    {
        _cleaner = new RegisterCleaner(XUnitLogger.CreateLogger<RegisterCleaner>(output));
    }

    /// <summary>
    /// Golden master: a fully-populated live PLC-tag register (carrying DataType, VariableId, StatusValueId,
    /// a stale RegisterId and stale Cycle/Machine ids) is reconstructed into the persisted row. Pins that the
    /// source <c>VariableId</c> is CARRIED (#96, to satisfy the enabled Registers.VariableId FK), that
    /// <c>DataType/StatusValueId</c> are DROPPED, and that the carried fields take the supplied values.
    /// </summary>
    [Fact]
    public void CleanRegisters_FullyPopulatedRegister_CarriesVariableId_DropsDataTypeStatusValueId()
    {
        // Arrange — a register shaped exactly like a live mutated PLC tag carrier.
        var registers = new Dictionary<string, Register>
        {
            ["TotalProduction"] = Register.CreateFixture(registerId: 987, name: "TotalProduction", description: "Total production counter", machineId: 11, variableId: 4242, cycleId: 7, value: "1500", dataType: "System.Int32", statusValueId: 1, timeStamp: new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Local)),
        };

        // Act
        var result = _cleaner.CleanRegisters(registers, cycleId: 555, machineId: 444, timestamp: _timestamp);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        var cleaned = result.Value.ToList();
        cleaned.Count.ShouldBe(1);

        var row = cleaned[0];

        // Seven CARRIED fields (the persisted-bytes contract).
        row.Name.ShouldBe("TotalProduction");
        row.Description.ShouldBe("Total production counter");
        row.Value.ShouldBe("1500");
        row.CycleId.Value.ShouldBe(555);
        row.MachineId.ShouldBe(444);
        row.RegisterId.ShouldBe(0);
        row.TimeStamp.ShouldBe(_timestamp);

        // VariableId is CARRIED from the source (#96) — the enabled Registers.VariableId FK requires it.
        row.VariableId.ShouldBe(4242);

        // The two remaining DROPPED fields — must land at type defaults, NOT the source values.
        row.DataType.ShouldBe(string.Empty);
        row.StatusValueId.ShouldBe(0);
    }

    /// <summary>
    /// Golden master: invisible-character / whitespace scrubbing on the three string fields is part of the
    /// produced bytes. Pins the exact <c>Trim()</c> + strip-<c>\n\r\t</c> output.
    /// </summary>
    [Fact]
    public void CleanRegisters_DirtyStrings_ScrubsToExactCanonicalForm()
    {
        var registers = new Dictionary<string, Register>
        {
            ["dirty"] = Register.CreateFixture(name: "  Temperature\n\r\t", description: "Temp\tsensor\nreading\r", value: "\t25.5\r\n"),
        };

        var result = _cleaner.CleanRegisters(registers, cycleId: 1, machineId: 1, timestamp: _timestamp);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        var row = result.Value.Single();

        row.Name.ShouldBe("Temperature");
        row.Description.ShouldBe("Tempsensorreading");
        row.Value.ShouldBe("25.5");
    }

    /// <summary>
    /// Golden master: ordering and multiplicity. Two registers in, two reconstructed rows out, each carrying
    /// the shared cycle/machine/timestamp metadata and RegisterId reset to 0.
    /// </summary>
    [Fact]
    public void CleanRegisters_MultipleRegisters_ReconstructsEachWithSharedMetadata()
    {
        var registers = new Dictionary<string, Register>
        {
            ["a"] = Register.CreateFixture(registerId: 1, name: "ProductionOk", variableId: 1, value: "1450", dataType: "System.Double", statusValueId: 1),
            ["b"] = Register.CreateFixture(registerId: 2, name: "ProductionNoK", variableId: 2, value: "50", dataType: "System.Double", statusValueId: 1),
        };

        var result = _cleaner.CleanRegisters(registers, cycleId: 99, machineId: 88, timestamp: _timestamp);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        var rows = result.Value.ToList();
        rows.Count.ShouldBe(2);

        var ok = rows.Single(r => r.Name == "ProductionOk");
        ok.Value.ShouldBe("1450");
        ok.CycleId.Value.ShouldBe(99);
        ok.MachineId.ShouldBe(88);
        ok.RegisterId.ShouldBe(0);
        ok.TimeStamp.ShouldBe(_timestamp);
        ok.DataType.ShouldBe(string.Empty);
        ok.VariableId.ShouldBe(1);
        ok.StatusValueId.ShouldBe(0);

        var nok = rows.Single(r => r.Name == "ProductionNoK");
        nok.Value.ShouldBe("50");
        nok.CycleId.Value.ShouldBe(99);
        nok.MachineId.ShouldBe(88);
        nok.RegisterId.ShouldBe(0);
        nok.DataType.ShouldBe(string.Empty);
        nok.VariableId.ShouldBe(2);
        nok.StatusValueId.ShouldBe(0);
    }
}
