// <copyright file="GetConfigAppsListQueryHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.ConfigApplication.Queries.GetConfigAppsList;

namespace IndTrace.Aggregation.BoundedTests.ConfigApp.Queries;
/// <summary>
/// Represents the GetConfigAppsListQueryHandlerTests.
/// </summary>

public class GetConfigAppsListQueryHandlerTests : DependenciesFactory
{
    // Removed: DpLogger - using Meziantou logging instead
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="base(outputHelper">The base(outputHelper.</param>

    public GetConfigAppsListQueryHandlerTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    /// <summary>
    /// Executes GetConfigAppsTest operation.
    /// </summary>
    /// <returns>The result of GetConfigAppsTest.</returns>

    [Fact]
    public async Task GetConfigAppsTest()
    {
        await Initialization;

        //[Fix]
        //CLAUDE
        //Date: 05/09/2025
        //Reason: [Pattern: Migration Task] - Remove mock repositories from Aggregation tests
        //        Use real DpConfigAppRepository instead of mock, flexible assertion for hybrid loader
        var logger = XUnitLogger.CreateLogger<GetConfigAppsListQueryHandler>();
        var sut = new GetConfigAppsListQueryHandler(DpConfigAppRepository, logger, DpIDateTimeMachine);

        var result = await sut.ProcessAsync(new GetConfigAppsListQuery(), TestContext.Current.CancellationToken);
        result.Value.ShouldNotBeNull();

        var spec = new Specification<Domain.Entities.ConfigApp>(b => true);
        var total = await DpConfigAppRepository.CountAsync(spec, TestContext.Current.CancellationToken);

        total.ShouldNotBeNull();
        result.Value.ShouldBeOfType<ConfigAppsListVm>();
        result.Value.ConfigApp.Count.ShouldBeGreaterThanOrEqualTo(total.Value);
    }
}