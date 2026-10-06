// <copyright file="GetConfigAppDetailQueryHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.ConfigApp.Queries;
/// <summary>
/// Represents the GetConfigAppDetailQueryHandlerTests.
/// </summary>

public class GetConfigAppDetailQueryHandlerTests : DependenciesFactory
{
    // Removed: DpLogger - using Meziantou logging instead
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="base(outputHelper">The base(outputHelper.</param>

    public GetConfigAppDetailQueryHandlerTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    /// <summary>
    /// Executes GetConfigAppDetail operation.
    /// </summary>
    /// <returns>The result of GetConfigAppDetail.</returns>

    [Fact]
    public async Task GetConfigAppDetail()
    {
        await Initialization;

        // Arrange

        var repository = DpConfigAppRepository;
        var logger = XUnitLogger.CreateLogger<GetConfigAppsDetailQueryHandler>();

        var sut = new GetConfigAppsDetailQueryHandler(repository, logger);

        // Act
        var result = await sut.ProcessAsync(new GetConfigAppsDetailQuery { Id = 1 }, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<ConfigAppDto>();
        result.Value.ConfigAppId.ShouldBe("IndTrace L1A");
    }
}