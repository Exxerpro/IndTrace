// <copyright file="PlcsInformationLifecycleTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.UI.Models;
using IndTrace.Components.Area.DashBoard;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace IndTrace.Monitor.Tests;

/// <summary>
/// Regression tests for issue #124 (F15 residuals) on <see cref="PlcsInformation"/>: a Refresh
/// click must probe each PLC exactly ONCE (a redundant first loop used to probe every PLC twice
/// per click), and the #90 disposal work (event unsubscribe + timer dispose) must keep the
/// component safe to render and tear down.
/// </summary>
public sealed class PlcsInformationLifecycleTests : IDisposable
{
    private readonly BunitContext context;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlcsInformationLifecycleTests"/> class,
    /// registering the MudBlazor services the component's chrome needs and putting JS interop in
    /// loose mode (the refresh button is plain markup; no real JS is exercised).
    /// </summary>
    public PlcsInformationLifecycleTests()
    {
        this.context = new BunitContext();
        this.context.Services.AddLogging();
        this.context.Services.AddMudServices();
        this.context.JSInterop.Mode = JSRuntimeMode.Loose;
    }

    /// <summary>
    /// A Refresh click probes each PLC exactly once. <see cref="ControllerMonitor.RefreshConnection"/>
    /// sets <see cref="ControllerMonitor.TimeStamp"/>, which raises <c>UpdateReceived</c> once per
    /// probe — so the event count IS the probe count. Before the #124 fix this was 2 per click.
    /// </summary>
    [Fact]
    public void RefreshClick_ProbesEachPlcExactlyOnce()
    {
        // Arrange
        var plc = new ControllerMonitor(1, 100) { Name = "PLC-1", IpAddress = "10.0.0.1" };
        var probeCount = 0;
        plc.UpdateReceived += (_, _) => probeCount++;
        var connections = new Dictionary<int, ControllerMonitor> { [1] = plc };

        var component = this.context.Render<PlcsInformation>(p => p
            .Add(x => x.PlcConnections, connections));

        // Act — click the "Refresh status" button.
        var refreshButton = component
            .FindAll("button")
            .First(b => b.TextContent.Contains("Refresh status", StringComparison.Ordinal));
        refreshButton.Click();

        // Assert — exactly one probe per PLC per click (the redundant duplicate loop is gone).
        probeCount.ShouldBe(1);
    }

    /// <summary>
    /// Rendering and disposing the component must not throw: Dispose unsubscribes the PLC events
    /// and disposes the refresh timer (the #90 remediation this chunk must keep intact).
    /// </summary>
    [Fact]
    public void RenderAndDispose_DoesNotThrow()
    {
        // Arrange
        var plc = new ControllerMonitor(2, 200) { Name = "PLC-2", IpAddress = "10.0.0.2" };
        var connections = new Dictionary<int, ControllerMonitor> { [2] = plc };

        this.context.Render<PlcsInformation>(p => p
            .Add(x => x.PlcConnections, connections));

        // Act & Assert — disposing the bUnit context tears down the renderer, which runs
        // PlcsInformation.Dispose on the rendered component.
        Should.NotThrow(() => this.context.Dispose());
    }

    /// <inheritdoc/>
    public void Dispose() => this.context.Dispose();
}
