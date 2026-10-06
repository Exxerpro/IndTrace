// <copyright file="GetSettingsListQueryHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.Settings.Queries;
/// <summary>
/// Represents the GetSettingsListQueryHandlerTests.
/// </summary>

public class GetSettingsListQueryHandlerTests : DependenciesFactory
{
    public GetSettingsListQueryHandlerTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    /// <summary>
    /// Executes GetSettingsTest operation.
    /// </summary>
    /// <returns>The result of GetSettingsTest.</returns>

    [Fact]
    public async Task GetSettingsTest()
    {
        await Initialization;

        var logger = XUnitLogger.CreateLogger<GetSettingsListQueryHandler>();

        // #95 Slice C: the query handler now takes the read-only Setting repository.
        var sut = new GetSettingsListQueryHandler(DpRoSettingRepository, logger);

        var result = await sut.ProcessAsync(new GetSettingsListQuery(), TestContext.Current.CancellationToken);
        result.Value.ShouldNotBeNull();

        result.Value.ShouldBeOfType<SettingsListVm>();
        result.Value.Settings.Count.ShouldBe(1);
    }
}