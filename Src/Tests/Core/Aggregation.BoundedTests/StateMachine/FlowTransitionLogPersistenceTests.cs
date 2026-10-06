// <copyright file="FlowTransitionLogPersistenceTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.StateMachine;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IndTrace.Application.StateMachine;
using IndTrace.Domain.Entities;
using IndTrace.Domain.StateMachine;
using IndTrace.Domain.ValueObjects;
using IndTrace.Persistence.Interfaces;
using IndTrace.Persistence.Services;
using Microsoft.Extensions.Options;

/// <summary>
/// Story 3.5 — InMemory (NO mocks for the DB) tests proving the EF-backed <see cref="FlowTransitionLogSink"/>
/// and the <see cref="LoggingItemStateMachineDecorator"/> append exactly one additive <see cref="FlowTransitionLog"/>
/// row per fire (success AND rejection) with the correct Path/From/To/Trigger/MachineId/ResultValidation, and
/// that the row is retrievable for diagnostics (AC6).
/// </summary>
public class FlowTransitionLogPersistenceTests
{
    private static readonly DateTime FixedNow = new(2026, 6, 19, 8, 0, 0, DateTimeKind.Local);

    private static Recipe ValidRecipe() => Recipe.Create(0, 0, 0, 216000, 3, 5, 1).Value.ShouldNotBeNull();

    private static TransitionContext FullContext(MachineType machineType, CycleStatus cycleStatus, PartStatus partStatus) =>
        new(machineType, cycleStatus, partStatus, 100, ValidRecipe(), BarCodeFound: true, MachineFound: true, ProductFound: true, RuleFound: true, RecipeFound: true, ShiftValid: true);

    // A factory backed by a SHARED named InMemory database. Each call hands out a FRESH context (so the sink's
    // `using` can dispose it safely, exactly like the production factory), while all contexts share the same
    // store — so a row appended via one context is readable via another. This mirrors real EF behavior.
    private sealed class SingleContextFactory : IIndTraceDbContextFactory, IDisposable
    {
        private readonly string databaseName = Guid.NewGuid().ToString();

        public SingleContextFactory()
        {
            using var seed = this.NewContext();
            seed.Database.EnsureCreated();
        }

        private IndTraceDbContext NewContext()
        {
            var options = new DbContextOptionsBuilder<IndTraceDbContext>()
                .UseInMemoryDatabase(this.databaseName)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
                .Options;
            var ctx = new IndTraceDbContext(options);
            ctx.SetTestingInterfaces(new TesterUserService(), new DateTimeMachine());
            return ctx;
        }

        // A fresh context for the test's READ assertions (caller disposes via `using`).
        public IndTraceDbContext OpenReadContext() => this.NewContext();

        public Microsoft.EntityFrameworkCore.DbContext CreateEfDbContext() => this.NewContext();

        public Task<IIndTraceDbContext> CreateDbContextAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IIndTraceDbContext>(this.NewContext());

        public IIndTraceDbContext CreateDbContext() => this.NewContext();

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        public void Dispose()
        {
        }
    }

    private static FlowTransitionLogSink Sink(SingleContextFactory factory) =>
        new(factory, XUnitLogger.CreateLogger<FlowTransitionLogSink>());

    private static LoggingItemStateMachineDecorator Decorator(IFlowTransitionLogSink sink, TransitionPath path) =>
        new(
            new ItemStateMachine(),
            sink,
            new FixedPath(path),
            new DateTimeMachine(),
            XUnitLogger.CreateLogger<LoggingItemStateMachineDecorator>(),
            Options.Create(new StateMachineRoutingOptions()));

    private sealed class FixedPath(TransitionPath path) : ITransitionPathContext
    {
        public TransitionPath Current => path;

        public void Set(TransitionPath p)
        {
        }
    }

    /// <summary>
    /// The EF sink persists one FlowTransitionLog row to a real (InMemory) context and it is retrievable (AC6).
    /// </summary>
    [Fact]
    public void Sink_Append_PersistsOneRetrievableRow()
    {
        using var factory = new SingleContextFactory();
        var sink = Sink(factory);
        var row = new FlowTransitionLog
        {
            From = FlowStatus.Created,
            To = FlowStatus.InProcess,
            Trigger = GatewayTask.CreateCycleAsync,
            Path = TransitionPath.Plc,
            MachineId = 100,
            BarCodeId = 7,
            ResultValidation = ResultValidation.Valid,
            TimeStamp = FixedNow,
        };

        var result = sink.Append(row);

        result.IsSuccess.ShouldBeTrue();
        using var read = factory.OpenReadContext();
        var stored = read.FlowTransitionLogs.Where(r => r.MachineId == 100).ToList();
        stored.Count.ShouldBe(1);
        stored[0].From.Value.ShouldBe(FlowStatus.Created.Value);
        stored[0].To.Value.ShouldBe(FlowStatus.InProcess.Value);
        stored[0].Trigger.Value.ShouldBe(GatewayTask.CreateCycleAsync.Value);
        stored[0].Path.ShouldBe(TransitionPath.Plc);
    }

    /// <summary>
    /// AC2/AC4 — a successful PLC fire through the decorator appends exactly one persisted row with Path=Plc.
    /// </summary>
    [Fact]
    public void Decorator_OverEfSink_SuccessfulPlcFire_PersistsOneRow_WithPlcPath()
    {
        using var factory = new SingleContextFactory();
        var decorator = Decorator(Sink(factory), TransitionPath.Plc);
        var barcode = new BarCodeBuilder().AtState(FlowStatus.Created, PartStatus.None)
            .With(b => { b.BarCodeId = new BarCodeId(21); b.MachineId = new MachineId(55); }).Build();

        var result = decorator.Fire(barcode, GatewayTask.CreateCycleAsync, FullContext(MachineType.Process, CycleStatus.Started, PartStatus.Ok));

        result.IsSuccess.ShouldBeTrue();
        using var read = factory.OpenReadContext();
        var rows = read.FlowTransitionLogs.Where(r => r.BarCodeId == 21).ToList();
        rows.Count.ShouldBe(1);
        rows[0].From.Value.ShouldBe(FlowStatus.Created.Value);
        rows[0].To.Value.ShouldBe(FlowStatus.InProcess.Value);
        rows[0].Path.ShouldBe(TransitionPath.Plc);
        rows[0].MachineId.ShouldBe(55);
    }

    /// <summary>
    /// AC3 — a rejected webapp fire through the decorator still appends one persisted row with To=From and a negative code.
    /// </summary>
    [Fact]
    public void Decorator_OverEfSink_RejectedWebappFire_PersistsOneRow_WithNoAdvance()
    {
        using var factory = new SingleContextFactory();
        var decorator = Decorator(Sink(factory), TransitionPath.Webapp);
        var barcode = new BarCodeBuilder().AtState(FlowStatus.Rejected, PartStatus.None)
            .With(b => { b.BarCodeId = new BarCodeId(22); b.MachineId = new MachineId(56); }).Build();

        // (Rejected, RejectPartAsync) is illegal.
        var result = decorator.Fire(barcode, GatewayTask.RejectPartAsync, FullContext(MachineType.Process, CycleStatus.Started, PartStatus.None));

        result.IsSuccess.ShouldBeFalse();
        using var read = factory.OpenReadContext();
        var rows = read.FlowTransitionLogs.Where(r => r.BarCodeId == 22).ToList();
        rows.Count.ShouldBe(1);
        rows[0].From.Value.ShouldBe(FlowStatus.Rejected.Value);
        rows[0].To.Value.ShouldBe(FlowStatus.Rejected.Value); // no advance
        rows[0].Path.ShouldBe(TransitionPath.Webapp);
        rows[0].ResultValidation.Value.ShouldBeLessThan(0);
    }
}
