// <copyright file="UpdateProductCommandTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.Products.Commands;
/// <summary>
/// Represents the UpdateProductCommandTests.
/// </summary>

public class UpdateProductCommandTests : DependenciesFactory
{
    //[Fix]
    //CLAUDE
    //Date: 09/09/2025
    //Reason: [Constructor Pattern] - Added ITestContextAccessor parameter to match DependenciesFactory signature
    public UpdateProductCommandTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    /// <summary>
    /// Executes ShouldSendRequestAsync operation.
    /// </summary>
    /// <returns>The result of ShouldSendRequestAsync.</returns>

    [Fact]
    public async Task ShouldSendRequestAsync()
    {
        await Initialization;

        // Arrange

        var dispatcher = DpMonitorRequestDispatcher;

        // Set deterministic time for test
        DpDateTimeMachine.SetDateTimeNow(new DateTimeOffset(2020, 06, 06, 06, 06, 06, 6, TimeSpan.Zero));

        //This test are failing because  the handler are not beein registered

        //[Fix]
        //CLAUDE
        //Date: 09/09/2025
        //Reason: [Validation Failure] - Fixed empty string validation issues
        // Using realistic values based on existing ProductId 508 data
        var request = new UpdateProductCommand()
        {
            AliasNoParte = "Housing CHMSL X1 Updated",
            CustomerPartNumber = "Housing CHMSL X1 Updated",
            Description = "Housing CHMSL X1 Updated",
            IsActive = 1,
            NoParte = "L100003",
            ProductId = 508,
            ProductName = "L100003 Updated",
            Version = 2 // Increment version for update
        };

        // Act
        var result = await dispatcher.ProcessAsync(request, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<ProductDto>();
    }

    /// <summary>
    /// Executes ShouldExecuteRequestHandler operation.
    /// </summary>
    /// <returns>The result of ShouldExecuteRequestHandler.</returns>

    [Fact]
    public async Task ShouldExecuteRequestHandler()
    {
        await Initialization;

        // Arrange

        var repository = DpProductRepository;
        var dispatcher = DpMonitorRequestDispatcher;
        var logger = XUnitLogger.CreateLogger<UpdateProductCommandHandler>();

        // Set deterministic time for test
        DpDateTimeMachine.SetDateTimeNow(new DateTimeOffset(2020, 06, 06, 06, 06, 06, 6, TimeSpan.Zero));

        var sut = new UpdateProductCommandHandler(repository, dispatcher, logger, DpDateTimeMachine);

        var request = new UpdateProductCommand()
        {
            AliasNoParte = "Ram Performance Exhaust Updated",
            CustomerPartNumber = "R150752-Updated",
            Description = "Updated Ram Performance Exhaust System",
            IsActive = (int?)Active.Inactive,
            NoParte = "R150752",
            ProductId = 634, // Use existing ProductId
            ProductName = "Ram Performance Exhaust Updated",
            Version = 2
        };

        // Act
        var result = await sut.ProcessAsync(request, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<ProductDto>();
        result.Value.AliasPartNumber.ShouldBe(request.AliasNoParte);
        result.Value.CustomerPartNumber.ShouldBe(request.CustomerPartNumber);
        result.Value.IsActive.ShouldBeEquivalentTo(request.IsActive);
        result.Value.PartNumber.ShouldBe(request.NoParte);
        result.Value.ProductId.ShouldBeEquivalentTo(request.ProductId);
        result.Value.ProductName.ShouldBe(request.ProductName);
        result.Value.Version.ShouldBeEquivalentTo(request.Version);
    }
}