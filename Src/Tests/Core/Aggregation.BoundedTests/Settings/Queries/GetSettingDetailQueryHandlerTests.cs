// <copyright file="GetSettingDetailQueryHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.Settings.Queries;
/// <summary>
/// Represents the GetSettingDetailQueryHandlerTests.
/// </summary>

public class GetSettingDetailQueryHandlerTests : DependenciesFactory
{
    public GetSettingDetailQueryHandlerTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    /// <summary>
    /// Executes GetSettingDetail operation.
    /// </summary>
    /// <returns>The result of GetSettingDetail.</returns>

    [Fact]
    public async Task GetSettingDetail()
    {
        await Initialization;

        // #95 Slice C: the query handler now takes the read-only Setting repository.
        var repository = DpRoSettingRepository;
        var logger = XUnitLogger.CreateLogger<GetSettingDetailQueryHandler>();

        var sut = new GetSettingDetailQueryHandler(repository, logger);

        var result = await sut.ProcessAsync(new GetSettingDetailQuery { SettingId = 1 }, cancellationToken: TestContext.Current.CancellationToken);
        result.Value.ShouldNotBeNull();

        result.Value.ShouldBeOfType<SettingDetailVm>();
        result.Value.SettingId.ShouldBe(1);
    }
}