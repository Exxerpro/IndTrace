// <copyright file="NullSmartEnumWriteSentinelTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Microsoft.Data.Sqlite;

namespace IndTrace.Aggregation.BoundedTests.Repository;

/// <summary>
/// #117 (F4) regression tests for the smart-enum EF value converters. Pre-fix every affected site wrote a
/// NULL smart-enum property as <c>0</c> — but <c>0</c> is a REAL recorded member (<c>None</c>) on every one
/// of these enums, so a missing value was indistinguishable from a deliberately recorded None. The fix
/// writes each enum's <c>Invalid</c> sentinel instead (<c>-1</c> for most, <c>8</c> for
/// <see cref="FlowStatus"/>, <see cref="int.MinValue"/> for <see cref="ActiveStatus"/>), and the read
/// direction maps that sentinel back to the <c>Invalid</c> member (the Products/Variables branchy read
/// gains an exact-sentinel guard so <see cref="int.MinValue"/> no longer collapses to Inactive).
/// </summary>
public sealed class NullSmartEnumWriteSentinelTests
{
    /// <summary>
    /// Minimal SQLite DDL for the FlowTransitionLog table: exactly the columns the EF model maps for
    /// <see cref="FlowTransitionLog"/>, so raw column bytes can be asserted on a relational provider.
    /// </summary>
    private const string FlowTransitionLogTableDdl = """
        CREATE TABLE "FlowTransitionLog" (
            "FlowTransitionLogId" INTEGER NOT NULL CONSTRAINT "PK_FlowTransitionLog" PRIMARY KEY AUTOINCREMENT,
            "From" INTEGER NOT NULL,
            "To" INTEGER NOT NULL,
            "FromCycleStatus" INTEGER NOT NULL,
            "Trigger" INTEGER NOT NULL,
            "Path" INTEGER NOT NULL,
            "MachineId" INTEGER NOT NULL,
            "BarCodeId" INTEGER NOT NULL,
            "CycleId" INTEGER NOT NULL,
            "ResultValidation" INTEGER NOT NULL,
            "TimeStamp" TEXT NOT NULL
        );
        """;

    /// <summary>
    /// Minimal SQLite DDL for the Variables table (same shape as AddRangeBulkAsyncRelationalTests).
    /// </summary>
    private const string VariablesTableDdl = """
        CREATE TABLE "Variables" (
            "VariableId" INTEGER NOT NULL CONSTRAINT "PK_Variables" PRIMARY KEY AUTOINCREMENT,
            "MachineId" INTEGER NOT NULL,
            "PlcId" INTEGER NOT NULL,
            "Name" TEXT NOT NULL,
            "Description" TEXT NOT NULL,
            "Alias" TEXT NOT NULL,
            "Address" TEXT NOT NULL,
            "NetType" TEXT NOT NULL,
            "Length" INTEGER NOT NULL,
            "IsActive" INTEGER NOT NULL,
            "Direction" INTEGER NOT NULL,
            "VariableGroupId" INTEGER NOT NULL,
            "CreatedBy" TEXT NOT NULL,
            "ModifiedBy" TEXT NOT NULL,
            "CreatedOn" TEXT NOT NULL,
            "ModifiedOn" TEXT NULL
        );
        """;

    /// <summary>
    /// One converter site under test: the mapped entity/property, the int the write side must emit for a
    /// NULL model value (the enum's Invalid sentinel), and the Invalid member the read side must
    /// materialize that sentinel back to.
    /// </summary>
    private sealed record ConverterSite(Type EntityType, string PropertyName, int Sentinel, EnumModel Invalid);

    /// <summary>
    /// All 18 F4 sites (every smart-enum HasConversion whose write lambda previously fell back to 0).
    /// </summary>
    private static readonly ConverterSite[] Sites =
    [
        new(typeof(BarCode), nameof(BarCode.PartStatus), PartStatus.Invalid.Value, PartStatus.Invalid),
        new(typeof(BarCode), nameof(BarCode.FlowStatus), FlowStatus.Invalid.Value, FlowStatus.Invalid),
        new(typeof(TaskGatewayResponse), nameof(TaskGatewayResponse.ResultValidation), ResultValidation.Invalid.Value, ResultValidation.Invalid),
        new(typeof(TaskGatewayResponse), nameof(TaskGatewayResponse.CycleStatus), CycleStatus.Invalid.Value, CycleStatus.Invalid),
        new(typeof(TaskGatewayResponse), nameof(TaskGatewayResponse.FlowStatus), FlowStatus.Invalid.Value, FlowStatus.Invalid),
        new(typeof(TaskGatewayResponse), nameof(TaskGatewayResponse.PartStatus), PartStatus.Invalid.Value, PartStatus.Invalid),
        new(typeof(Cycle), nameof(Cycle.CycleStatus), CycleStatus.Invalid.Value, CycleStatus.Invalid),
        new(typeof(Cycle), nameof(Cycle.PartStatus), PartStatus.Invalid.Value, PartStatus.Invalid),
        new(typeof(FlowTransitionLog), nameof(FlowTransitionLog.From), FlowStatus.Invalid.Value, FlowStatus.Invalid),
        new(typeof(FlowTransitionLog), nameof(FlowTransitionLog.To), FlowStatus.Invalid.Value, FlowStatus.Invalid),
        new(typeof(FlowTransitionLog), nameof(FlowTransitionLog.FromCycleStatus), CycleStatus.Invalid.Value, CycleStatus.Invalid),
        new(typeof(FlowTransitionLog), nameof(FlowTransitionLog.Trigger), GatewayTask.Invalid.Value, GatewayTask.Invalid),
        new(typeof(FlowTransitionLog), nameof(FlowTransitionLog.ResultValidation), ResultValidation.Invalid.Value, ResultValidation.Invalid),
        new(typeof(Machine), nameof(Machine.WorkFlowType), WorkFlowType.Invalid.Value, WorkFlowType.Invalid),
        new(typeof(Plc), nameof(Plc.Enabled), ActiveStatus.Invalid.Value, ActiveStatus.Invalid),
        new(typeof(MachinePlc), nameof(MachinePlc.IsActive), ActiveStatus.Invalid.Value, ActiveStatus.Invalid),
        new(typeof(Product), nameof(Product.IsActive), ActiveStatus.Invalid.Value, ActiveStatus.Invalid),
        new(typeof(Variable), nameof(Variable.IsActive), ActiveStatus.Invalid.Value, ActiveStatus.Invalid),
    ];

    /// <summary>
    /// THE F4 defect, model-wide: for every affected site the RAW write-direction conversion expression
    /// (the exact lambda EF composes into its pipelines, before any object-level null sanitizing) must map
    /// a NULL model value to the enum's Invalid sentinel — never to 0/None. Pre-fix every site returned 0.
    /// </summary>
    [Fact]
    public void WriteConversion_OfNullSmartEnum_EmitsInvalidSentinelNotZero()
    {
        using var context = NewInMemoryContext();

        foreach (var site in Sites)
        {
            var converter = FindConverter(context, site);

            var written = converter.ConvertToProviderExpression.Compile().DynamicInvoke(new object?[] { null });

            written.ShouldNotBeNull($"{site.EntityType.Name}.{site.PropertyName}: write(null) returned null");
            Convert.ToInt32(written, CultureInfo.InvariantCulture).ShouldBe(
                site.Sentinel,
                $"{site.EntityType.Name}.{site.PropertyName}: write(null) must emit the Invalid sentinel {site.Sentinel}, not 0/None");
        }
    }

    /// <summary>
    /// F4 round-trip guard, model-wide: the read-direction conversion must materialize each enum's stored
    /// Invalid sentinel back to the Invalid member. Pre-fix the Products/Variables branchy read collapsed
    /// the ActiveStatus sentinel (<see cref="int.MinValue"/> &lt; 0) onto Inactive — a REAL recorded state.
    /// </summary>
    [Fact]
    public void ReadConversion_OfInvalidSentinel_MaterializesInvalidMember()
    {
        using var context = NewInMemoryContext();

        foreach (var site in Sites)
        {
            var converter = FindConverter(context, site);

            var materialized = converter.ConvertFromProviderExpression.Compile().DynamicInvoke(site.Sentinel);

            materialized.ShouldNotBeNull($"{site.EntityType.Name}.{site.PropertyName}: read({site.Sentinel}) returned null");
            materialized.ShouldBe(
                site.Invalid,
                $"{site.EntityType.Name}.{site.PropertyName}: stored sentinel {site.Sentinel} must round-trip to {site.Invalid.Name}");
        }
    }

    /// <summary>
    /// Read-direction NON-regression for the Products/Variables branchy normalization: every int that can
    /// exist in production data today (0, 1, -1, and arbitrary positive/negative "legacy active" values)
    /// must keep materializing exactly as before the F4 fix — only the NEW sentinel value changes reading.
    /// </summary>
    [Fact]
    public void ReadConversion_BranchyActiveStatusSites_PreserveLegacyMapping()
    {
        using var context = NewInMemoryContext();

        foreach (var site in Sites.Where(s => s.EntityType == typeof(Product) || s.EntityType == typeof(Variable)))
        {
            var converter = FindConverter(context, site);
            var read = converter.ConvertFromProviderExpression.Compile();

            read.DynamicInvoke(1).ShouldBe(ActiveStatus.Active);
            read.DynamicInvoke(7).ShouldBe(ActiveStatus.Active);    // legacy positive "active"
            read.DynamicInvoke(0).ShouldBe(ActiveStatus.None);
            read.DynamicInvoke(-1).ShouldBe(ActiveStatus.Inactive);
            read.DynamicInvoke(-7).ShouldBe(ActiveStatus.Inactive); // legacy negative
        }
    }

    /// <summary>
    /// Pins the EF SAVE-path behavior for a NULL smart-enum on a RELATIONAL provider: EF's update pipeline
    /// null-sanitizes BEFORE the converter lambda (the object-level <c>ConvertToProvider</c> short-circuits
    /// null), so the row fails the NOT NULL column constraint LOUDLY — a missing value can never silently
    /// persist as anything (neither 0/None nor the sentinel) through <c>SaveChangesAsync</c>. The raw
    /// write-lambda seam (query parameters/constants and any composed use of the expression) is what the
    /// F4 sentinel fix hardens; this test guards against that save path ever fail-opening.
    /// </summary>
    [Fact]
    public async Task NullEnums_OnEfSavePath_FailLoudlyAndPersistNothing()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        await using var connection = await OpenConnectionAsync(FlowTransitionLogTableDdl, ct);

        var log = new FlowTransitionLog
        {
            MachineId = 9402,
            BarCodeId = 1,
            CycleId = 1,
            TimeStamp = new DateTime(2026, 7, 14, 0, 0, 0, DateTimeKind.Utc),
        };
        SetNull(log, nameof(FlowTransitionLog.From));

        // Act — the REAL EF save pipeline must refuse the row, not quietly coin a value.
        await using (var writeContext = NewSqliteContext(connection))
        {
            writeContext.Set<FlowTransitionLog>().Add(log);
            await Should.ThrowAsync<DbUpdateException>(async () => await writeContext.SaveChangesAsync(ct));
        }

        // Assert — nothing was persisted.
        await using var rawCommand = connection.CreateCommand();
        rawCommand.CommandText = "SELECT COUNT(*) FROM \"FlowTransitionLog\"";
        var count = await rawCommand.ExecuteScalarAsync(ct);
        Convert.ToInt32(count, CultureInfo.InvariantCulture).ShouldBe(0);
    }

    /// <summary>
    /// F4 end-to-end sentinel round-trip on a RELATIONAL provider: <see cref="FlowTransitionLog"/> rows
    /// recorded with the Invalid members must store the exact sentinel ints in the raw columns
    /// (FlowStatus = 8, CycleStatus/GatewayTask = -1) and materialize back as the Invalid members.
    /// </summary>
    [Fact]
    public async Task InvalidEnums_PersistedThroughEfPipeline_StoreSentinelBytesAndRoundTrip()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        await using var connection = await OpenConnectionAsync(FlowTransitionLogTableDdl, ct);

        var log = new FlowTransitionLog
        {
            From = FlowStatus.Invalid,
            To = FlowStatus.Invalid,
            FromCycleStatus = CycleStatus.Invalid,
            Trigger = GatewayTask.Invalid,
            ResultValidation = ResultValidation.Invalid,
            MachineId = 9402,
            BarCodeId = 1,
            CycleId = 1,
            TimeStamp = new DateTime(2026, 7, 14, 0, 0, 0, DateTimeKind.Utc),
        };

        // Act — write through the REAL EF save pipeline (converters applied by EF, not by the test).
        await using (var writeContext = NewSqliteContext(connection))
        {
            writeContext.Set<FlowTransitionLog>().Add(log);
            await writeContext.SaveChangesAsync(ct);
        }

        // Assert — raw column bytes hold the Invalid sentinels, never 0/None.
        await using (var rawCommand = connection.CreateCommand())
        {
            rawCommand.CommandText =
                "SELECT \"From\", \"FromCycleStatus\", \"Trigger\" FROM \"FlowTransitionLog\" WHERE \"MachineId\" = 9402";
            await using var reader = await rawCommand.ExecuteReaderAsync(ct);
            (await reader.ReadAsync(ct)).ShouldBeTrue();
            reader.GetInt32(0).ShouldBe(FlowStatus.Invalid.Value);      // 8, not 0
            reader.GetInt32(1).ShouldBe(CycleStatus.Invalid.Value);     // -1, not 0
            reader.GetInt32(2).ShouldBe(GatewayTask.Invalid.Value);     // -1, not 0
        }

        // Assert — EF materializes the sentinels back as the Invalid members.
        await using var readContext = NewSqliteContext(connection);
        var persisted = await readContext.Set<FlowTransitionLog>().AsNoTracking().SingleAsync(l => l.MachineId == 9402, ct);
        persisted.From.ShouldBe(FlowStatus.Invalid);
        persisted.FromCycleStatus.ShouldBe(CycleStatus.Invalid);
        persisted.Trigger.ShouldBe(GatewayTask.Invalid);
    }

    /// <summary>
    /// F4 end-to-end for an <see cref="ActiveStatus"/> site with the branchy read (Variables): a recorded
    /// Invalid must store <see cref="int.MinValue"/> in the raw column and round-trip back as Invalid.
    /// Pre-fix the branchy read had no sentinel arm, so the stored sentinel (&lt; 0) materialized as
    /// Inactive — a REAL recorded state.
    /// </summary>
    [Fact]
    public async Task InvalidActiveStatus_PersistedThroughEfPipeline_StoresMinValueAndRoundTripsInvalid()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        await using var connection = await OpenConnectionAsync(VariablesTableDdl, ct);

        var created = Variable.Create(
            machineId: 9403,
            plcId: 1,
            name: "F4InvalidIsActive",
            description: "#117 F4 ActiveStatus sentinel round-trip",
            alias: "F4InvalidIsActive",
            address: "DB100.F4",
            netType: "Int",
            length: 2,
            isActive: ActiveStatus.Invalid,
            direction: 1,
            variableGroupId: 1);
        created.IsSuccess.ShouldBeTrue(created.Error);
        created.Value.ShouldNotBeNull();
        var variable = created.Value;

        // Act
        await using (var writeContext = NewSqliteContext(connection))
        {
            writeContext.Set<Variable>().Add(variable);
            await writeContext.SaveChangesAsync(ct);
        }

        // Assert — raw column holds the out-of-band sentinel, not 0/None and not -1/Inactive.
        await using (var rawCommand = connection.CreateCommand())
        {
            rawCommand.CommandText = "SELECT \"IsActive\" FROM \"Variables\" WHERE \"Name\" = 'F4InvalidIsActive'";
            var rawValue = await rawCommand.ExecuteScalarAsync(ct);
            rawValue.ShouldNotBeNull();
            Convert.ToInt32(rawValue, CultureInfo.InvariantCulture).ShouldBe(ActiveStatus.Invalid.Value);
        }

        // Assert — the branchy read maps the sentinel to Invalid (NOT Inactive, despite int.MinValue < 0).
        await using var readContext = NewSqliteContext(connection);
        var persisted = await readContext.Set<Variable>().AsNoTracking().SingleAsync(v => v.Name == "F4InvalidIsActive", ct);
        persisted.IsActive.ShouldBe(ActiveStatus.Invalid);
    }

    private static IndTraceDbContext NewInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<IndTraceDbContext>()
            .UseInMemoryDatabase($"f4-null-sentinel-{Guid.NewGuid()}")
            .Options;
        var context = new IndTraceDbContext(options);
        context.SetTestingInterfaces(new TesterUserService(), new DateTimeMachine());
        return context;
    }

    private static IndTraceDbContext NewSqliteContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<IndTraceDbContext>()
            .UseSqlite(connection)
            .Options;
        var context = new IndTraceDbContext(options);
        context.SetTestingInterfaces(new TesterUserService(), new DateTimeMachine());
        return context;
    }

    private static async Task<SqliteConnection> OpenConnectionAsync(string ddl, CancellationToken ct)
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();
        command.CommandText = ddl;
        await command.ExecuteNonQueryAsync(ct);

        return connection;
    }

    private static Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter FindConverter(
        IndTraceDbContext context,
        ConverterSite site)
    {
        var model = context.Model;
        model.ShouldNotBeNull();
        var entityType = model.FindEntityType(site.EntityType);
        entityType.ShouldNotBeNull($"{site.EntityType.Name} is not mapped");
        var property = entityType.FindProperty(site.PropertyName);
        property.ShouldNotBeNull($"{site.EntityType.Name}.{site.PropertyName} is not mapped");
        var converter = property.GetValueConverter();
        converter.ShouldNotBeNull($"{site.EntityType.Name}.{site.PropertyName} has no value converter");
        return converter;
    }

    /// <summary>
    /// Forces a smart-enum property to NULL via reflection — the runtime state NRT forbids assigning in
    /// code but which an uninitialized/materialized instance can still carry. The null branch of every F4
    /// write lambda exists exactly for this state.
    /// </summary>
    private static void SetNull(object entity, string propertyName)
    {
        var property = entity.GetType().GetProperty(propertyName);
        property.ShouldNotBeNull();
        property.SetValue(entity, null);
    }
}
