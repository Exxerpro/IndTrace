// <copyright file="GetShiftsListQueryHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Shifts;

/// <summary>
/// Unit tests for GetShiftsListQueryHandler
/// </summary>
public class GetShiftsListQueryHandlerTests
{
    /// <summary>
    /// Executes Should_Return_Valid_Instance_When_Valid_Context_Provided operation.
    /// </summary>
    [Fact]
    public void Should_Return_Valid_Instance_When_Valid_Context_Provided()
    {
        // Arrange
        var mockRepository = Substitute.For<IRepository<Shift>>();
        var logger = XUnitLogger.CreateLogger<GetShiftsListQueryHandler>();

        // Act
        var handler = new GetShiftsListQueryHandler(mockRepository, logger);

        // Assert
        handler.ShouldNotBeNull();
    }
    /// <summary>
    /// Executes Should_Throw_Exception_When_Null_Context_Provided operation.
    /// </summary>

    // MARKED FOR DELETION - Constructor null guard test no longer needed for DI handlers
    // [Fact]
    // public void Should_Throw_Exception_When_Null_Context_Provided()
    // {
    //     // Arrange
    //     IRepository<Shift>? nullRepository = null!;
    //     var logger = XUnitLogger.CreateLogger<GetShiftsListQueryHandler>();
    // 
    //     // Act & Assert
    //     //[Fix] 
    //     //CLAUDE
    //     //Date: 22/08/2025 
    //     //Reason: Pattern 12 Fix - Constructor null guard tests no longer needed for DI handlers per null safety refactoring strategy
    //     Should.Throw<ArgumentNullException>(() => new GetShiftsListQueryHandler(nullRepository!, logger));
    // }
    /// <summary>
    /// Executes Constructor_WithNullLogger_ShouldThrowException operation.
    /// </summary>

    // MARKED FOR DELETION - Constructor null guard test no longer needed for DI handlers
    // [Fact]
    //     public void Constructor_WithNullLogger_ShouldThrowException()
    //     {
    //         // Arrange
    //         var mockRepository = Substitute.For<IRepository<Shift>>();
    //         ILogger<GetShiftsListQueryHandler>? nullLogger = null!;
    // 
    //         // Act & Assert
    //         Should.Throw<ArgumentNullException>(() => new GetShiftsListQueryHandler(mockRepository, nullLogger!));
    //     }
    /// <summary>
    /// Executes Constructor_WithAllNullParameters_ShouldThrowException operation.
    /// </summary>

    // MARKED FOR DELETION - Constructor null guard test no longer needed for DI handlers
    // [Fact]
    //     public void Constructor_WithAllNullParameters_ShouldThrowException()
    //     {
    //         // Arrange
    //         IRepository<Shift>? nullRepository = null!;
    //         ILogger<GetShiftsListQueryHandler>? nullLogger = null!;
    // 
    //         // Act & Assert
    //         Should.Throw<ArgumentNullException>(() => new GetShiftsListQueryHandler(nullRepository!, nullLogger!));
    //     }


    [Fact]
    public async Task Process_Should_Return_Failure_When_Cancelled()
    {
        var mockRepository = Substitute.For<IRepository<Shift>>();
        var logger = XUnitLogger.CreateLogger<GetShiftsListQueryHandler>();
        var handler = new GetShiftsListQueryHandler(mockRepository, logger);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await handler.ProcessAsync(new GetShiftsListQuery(), cts.Token);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Process_Should_Return_Failure_When_Request_Null()
    {
        var mockRepository = Substitute.For<IRepository<Shift>>();
        var logger = XUnitLogger.CreateLogger<GetShiftsListQueryHandler>();
        var handler = new GetShiftsListQueryHandler(mockRepository, logger);

        var result = await handler.ProcessAsync(null!, TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeEmpty();
    }
}

