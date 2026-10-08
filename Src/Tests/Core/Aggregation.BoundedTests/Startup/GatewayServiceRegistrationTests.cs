// <copyright file="GatewayServiceRegistrationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.Startup;

using IndTrace.Communications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

/// <summary>
/// The gateway host builds its container with <c>ValidateOnBuild</c> and <c>ValidateScopes</c>, so one missing
/// registration stops the whole gateway at start-up. These tests build the real gateway composition under the same
/// options, without starting it, so such a gap fails here instead of on the plant floor.
/// </summary>
public sealed class GatewayServiceRegistrationTests
{
    private const string ConnectionString = "Server=localhost;Database=IndTraceData;Integrated Security=true";

    [Fact]
    public void AddGatewayHostServices_CommunityEdition_BuildsUnderHostValidation()
    {
        // Arrange
        var builder = CreateValidatingBuilder();

        // Act
        builder.Services.AddGatewayHostServices(builder.Configuration, ConnectionString, (_, _) => { }, _ => { });
        var build = Record.Exception(() => builder.Build().Dispose());

        // Assert
        build.ShouldBeNull();
    }

    [Fact]
    public void AddGatewayHostServices_RegistersOneClockBehindTheInterfaceOnly()
    {
        // Arrange — every gateway consumer depends on IDateTimeMachine (#242), so the host registers a single
        // clock behind the interface and never the concrete DateTimeMachine.
        var builder = CreateValidatingBuilder();
        builder.Services.AddGatewayHostServices(builder.Configuration, ConnectionString, (_, _) => { }, _ => { });

        // Act
        using var host = builder.Build();

        // Assert
        host.Services.GetService<DateTimeMachine>().ShouldBeNull();
        host.Services.GetRequiredService<IDateTimeMachine>()
            .ShouldBeSameAs(host.Services.GetRequiredService<IDateTimeMachine>());
    }

    private static HostApplicationBuilder CreateValidatingBuilder()
    {
        var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:IndTraceDbContext"] = ConnectionString,
            ["HubMonitorOptions:Url"] = "http://localhost:5200/eventmonitor",
        });
        ((IHostApplicationBuilder)builder).ConfigureContainer(
            new DefaultServiceProviderFactory(new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true,
            }));
        return builder;
    }
}
