// <copyright file="GetTagDetailQueryHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.Variables.Queries;
/// <summary>
/// Represents the GetTagDetailQueryHandlerTests.
/// </summary>

public class GetTagDetailQueryHandlerTests : DependenciesFactory
{
    public GetTagDetailQueryHandlerTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    /// <summary>
    /// Executes GetVariableDetail operation.
    /// </summary>
    /// <returns>The result of GetVariableDetail.</returns>

    [Fact]
    public async Task GetVariableDetail()
    {
        await Initialization;

        // Arrange

        var repository = DpVariablesRepository;
        var logger = XUnitLogger.CreateLogger<GetVariableDetailQueryHandler>();

        var sut = new GetVariableDetailQueryHandler(repository, logger);

        // Act
        var result = await sut.ProcessAsync(new GetVariableDetailQuery { Id = 4 }, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<VariableDetailVm>();
        result.Value.Address.ShouldBe("DB257.W14");
    }

    /// <summary>
    /// Executes GetVariableDetails operation.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="address">The address.</param>
    /// <param name="netType">The netType.</param>
    /// <returns>The result of GetVariableDetails.</returns>

    [Theory]
    [InlineData(1, "DB257.W0", "System.Int16")]
    [InlineData(2, "DB257.W2", "System.Int16")]
    [InlineData(3, "DB257.W12", "System.Int16")]
    public async Task GetVariableDetails(int id, string address, string netType)
    {
        await Initialization;

        //[Fix]
        //CLAUDE
        //Date: 05/09/2025
        //Reason: [Pattern: Migration Task] - Replace mock repository causing NullReferenceException
        //        Mock returned null instead of Result<Variable>, causing NRE in query handler
        //        Aggregation.BoundedTests must use real repositories, not mocks
        var logger = XUnitLogger.CreateLogger<GetVariableDetailQueryHandler>();

        var sut = new GetVariableDetailQueryHandler(DpVariablesRepository, logger);

        var result = await sut.ProcessAsync(new GetVariableDetailQuery { Id = id }, cancellationToken: TestContext.Current.CancellationToken);

        // Assert that the result is successful and contains valid data
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<VariableDetailVm>();

        // Variable details should match expected values from test data
        result.Value.Address.ShouldBe(address);
        result.Value.NetType.ShouldBe(netType);
    }
}