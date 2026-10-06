// <copyright file="CommunityDefaultsTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Devices.Tests;

using IndTrace.Devices.Plc;
using IndTrace.Devices.Scanning;
using IndTrace.Domain.Models;
using Meziantou.Extensions.Logging.Xunit.v3;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

/// <summary>
/// The community defaults: a simulation-only controller factory that refuses real PLCs, a reader that never
/// scans, and registrations that never override an installed driver.
/// </summary>
public class CommunityDefaultsTests(ITestOutputHelper output)
{
    [Fact]
    public void Factory_SimulationEnabledPlc_CreatesSimulatedController()
    {
        var factory = new SimulatedPlcControllerFactory();

        var created = factory.Create(PlcFixture.Create(enableSimulation: true), XUnitLogger.CreateLogger<CommunityDefaultsTests>(output), new DateTimeMachine());

        created.IsSuccess.ShouldBeTrue();
        created.Value.ShouldBeOfType<SimulatedControllerRx>().Dispose();
    }

    [Fact]
    public void Factory_RealPlc_IsRefusedLoudly()
    {
        var factory = new SimulatedPlcControllerFactory();

        var created = factory.Create(PlcFixture.Create(enableSimulation: false), XUnitLogger.CreateLogger<CommunityDefaultsTests>(output), new DateTimeMachine());

        created.IsFailure.ShouldBeTrue();
        created.Error.ShouldContain("No PLC driver is installed");
    }

    [Fact]
    public void Factory_NeverClassifiesExceptionsAsUnreachable()
    {
        new SimulatedPlcControllerFactory().IsControllerUnreachable(new InvalidOperationException()).ShouldBeFalse();
    }

    [Fact]
    public void NullReader_NeverConnectsOrScans()
    {
        var reader = new NullBarCodeReader();
        var scans = 0;
        using var subscription = reader.BarCode.Subscribe(_ => scans++);

        reader.Connect();

        reader.IsConnected.ShouldBeFalse();
        reader.Result.ShouldBeNull();
        scans.ShouldBe(0);
    }

    [Fact]
    public void AddCommunityDeviceDefaults_RegistersDefaults_WhenNoDriverIsInstalled()
    {
        using var provider = new ServiceCollection().AddCommunityDeviceDefaults().BuildServiceProvider();

        provider.GetRequiredService<IPlcControllerFactory>().ShouldBeOfType<SimulatedPlcControllerFactory>();
        provider.GetRequiredService<IBarCodeReader>().ShouldBeOfType<NullBarCodeReader>();
    }

    [Fact]
    public void AddCommunityDeviceDefaults_NeverOverridesAnInstalledDriver()
    {
        var driverFactory = new DriverFactoryDouble();
        var services = new ServiceCollection();
        services.AddSingleton<IPlcControllerFactory>(driverFactory);

        using var provider = services.AddCommunityDeviceDefaults().BuildServiceProvider();

        provider.GetRequiredService<IPlcControllerFactory>().ShouldBeSameAs(driverFactory);
    }

    private sealed class DriverFactoryDouble : IPlcControllerFactory
    {
        public IndQuestResults.Result<IIndTraceControllerRx> Create(
            IndTrace.Application.Plcs.Queries.GetDetail.PlcDto plc,
            Microsoft.Extensions.Logging.ILogger logger,
            DateTimeMachine dateTimeMachine) =>
            IndQuestResults.Result<IIndTraceControllerRx>.WithFailure("double");

        public bool IsControllerUnreachable(Exception exception) => true;
    }
}
