// <copyright file="SimulatedControllerRxTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Devices.Tests;

using IndTrace.Devices.Plc;
using IndTrace.Domain.Enum;
using IndTrace.Domain.Models;
using Meziantou.Extensions.Logging.Xunit.v3;
using Microsoft.Reactive.Testing;
using Shouldly;

/// <summary>
/// Behavior of the community in-memory PLC controller: configuration validation like a real driver, OnChange
/// command notifications, and the part data round trip the gateway relies on.
/// </summary>
public class SimulatedControllerRxTests(ITestOutputHelper output)
{
    private SimulatedControllerRx CreateController(PlcDtoMutator? mutate = null)
    {
        var plc = PlcFixture.Create();
        mutate?.Invoke(plc);
        return new SimulatedControllerRx(XUnitLogger.CreateLogger<SimulatedControllerRx>(output), plc, new DateTimeMachine())
        {
            CommandSettleDelay = TimeSpan.Zero,
        };
    }

    private delegate void PlcDtoMutator(IndTrace.Application.Plcs.Queries.GetDetail.PlcDto plc);

    private static async Task<SimulatedControllerRx> ConnectedAsync(SimulatedControllerRx controller)
    {
        (await controller.SetUpAsync(TestContext.Current.CancellationToken)).ShouldBeTrue();
        (await controller.ValidateThatTheTagExistOnTheController(TestContext.Current.CancellationToken)).ShouldBeTrue();
        (await controller.ConnectAndCreateNotificationsAsync(TestContext.Current.CancellationToken)).ShouldBeTrue();
        return controller;
    }

    [Fact]
    public async Task SetUp_ValidConfiguration_ConfiguresReferencesAndRegisters()
    {
        using var controller = this.CreateController();

        var configured = await controller.SetUpAsync(TestContext.Current.CancellationToken);

        configured.ShouldBeTrue();
        controller.Configured.ShouldBeTrue();
        controller.References.Keys.ShouldBe(["Reference1"]);
        controller.Registers.Keys.ShouldBe(["Register1"]);
    }

    [Fact]
    public async Task SetUp_WithoutFourEventTags_FailsLikeARealDriver()
    {
        using var controller = this.CreateController(plc => plc.Variables.Remove("HeartBeat"));

        var configured = await controller.SetUpAsync(TestContext.Current.CancellationToken);

        configured.ShouldBeFalse();
        controller.Configured.ShouldBeFalse();
        (await controller.ConnectAndCreateNotificationsAsync(TestContext.Current.CancellationToken)).ShouldBeFalse();
    }

    [Fact]
    public async Task SetUp_WithoutRegisterTags_Fails()
    {
        using var controller = this.CreateController(plc => plc.Variables.Remove("Register1"));

        (await controller.SetUpAsync(TestContext.Current.CancellationToken)).ShouldBeFalse();
    }

    [Fact]
    public async Task SimulateCommand_WritesPartDataThenRaisesCommandChanged()
    {
        using var controller = await ConnectedAsync(this.CreateController());
        var raised = new List<(short Command, string BarCode)>();
        using var subscription = controller.CommandChanged.Subscribe(c => raised.Add((c.Command, c.BarCode)));

        await controller.SimulateCommandAsync(
            new SimulatedCommand { MachineId = 100, PartNumber = "PN-0001", BarCode = "SN0001", Command = 2, PartStatus = PartStatus.Ok.Value },
            TestContext.Current.CancellationToken);

        raised.ShouldBe([((short)2, "SN0001")]);
        var upload = await controller.UploadCommandDataFromController(TestContext.Current.CancellationToken);
        upload.IsSuccess.ShouldBeTrue();
        upload.Value.ShouldNotBeNull();
        upload.Value.Command.ShouldBe(2);
        upload.Value.MachineId.ShouldBe(100);
        upload.Value.BarCode.ShouldBe("SN0001");
        upload.Value.PartNumber.ShouldBe("PN-0001");
        controller.PartStatus.ShouldBe(PartStatus.Ok);
        (await controller.ReadStringTagAsync("BarCode", TestContext.Current.CancellationToken)).ShouldBe("SN0001");
    }

    [Fact]
    public async Task CommandChanged_FiresOnlyOnChange_AndOnReset()
    {
        using var controller = await ConnectedAsync(this.CreateController());
        var raised = new List<short>();
        using var subscription = controller.CommandChanged.Subscribe(c => raised.Add(c.Command));
        var command = new SimulatedCommand { MachineId = 100, PartNumber = "PN-0001", BarCode = "SN0001", Command = 2 };

        await controller.SimulateCommandAsync(command, TestContext.Current.CancellationToken);
        await controller.SimulateCommandAsync(command, TestContext.Current.CancellationToken);
        await controller.ResetCommandAsync(TestContext.Current.CancellationToken);
        await controller.ResetCommandAsync(TestContext.Current.CancellationToken);

        raised.ShouldBe([(short)2, (short)0]);
    }

    [Fact]
    public async Task PlcId_RoundTripsThroughTheTagImage()
    {
        using var controller = await ConnectedAsync(this.CreateController());

        (await controller.GetPlcIdAsync(TestContext.Current.CancellationToken)).ShouldBe(100);
        await controller.SetPlcIdAsync(7, TestContext.Current.CancellationToken);
        (await controller.GetPlcIdAsync(TestContext.Current.CancellationToken)).ShouldBe(7);
    }

    [Fact]
    public async Task ReadTag_UnknownTag_FailsInsteadOfFabricatingAValue()
    {
        using var controller = await ConnectedAsync(this.CreateController());

        await Should.ThrowAsync<KeyNotFoundException>(() => controller.ReadShortTagAsync("NoSuchTag", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task HeartBeat_TicksOnTheSchedulerAfterConnect_AndStopsOnDispose()
    {
        var scheduler = new TestScheduler();
        var controller = this.CreateController();
        controller.HeartBeatScheduler = scheduler;
        await ConnectedAsync(controller);
        await controller.ConnectAndCreateNotificationsAsync(TestContext.Current.CancellationToken); // reconnect must not double-wire
        var beats = new List<short>();
        using var subscription = controller.HeartBeatChanged.Subscribe(c => beats.Add(c.HeartBeat));

        scheduler.AdvanceBy(TimeSpan.FromSeconds(3).Ticks);
        controller.Dispose();
        scheduler.AdvanceBy(TimeSpan.FromSeconds(3).Ticks);

        beats.ShouldBe([(short)1, (short)2, (short)3]);
    }

    [Fact]
    public async Task ReadStartUp_RaisesTheStartupCommand()
    {
        using var controller = await ConnectedAsync(this.CreateController());
        var raised = new List<short>();
        using var subscription = controller.CommandChanged.Subscribe(c => raised.Add(c.Command));

        var command = await controller.ReadStartUp(TestContext.Current.CancellationToken);

        command.Command.ShouldBe((short)8);
        raised.ShouldBe([(short)8]);
    }

    [Fact]
    public async Task Monitor_ReflectsTheSimulatedPart()
    {
        using var controller = await ConnectedAsync(this.CreateController());
        await controller.SimulateCommandAsync(
            new SimulatedCommand { MachineId = 100, PartNumber = "PN-0001", BarCode = "SN0001", Command = 2 },
            TestContext.Current.CancellationToken);

        var monitor = await controller.GetPlcMonitorAsync(TestContext.Current.CancellationToken);

        monitor.PlcId.ShouldBe(100);
        monitor.PartNumber.ShouldBe("PN-0001");
        monitor.Label.ShouldBe("SN0001");
    }
}
