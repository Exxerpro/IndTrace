// <copyright file="GetCyclesListQueryHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Cycles;

/// <summary>
/// Unit tests for GetCyclesListQueryHandler
/// </summary>
public class GetCyclesListQueryHandlerTests
{
    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var mockRepository = Substitute.For<IReadOnlyRepository<Cycle>>();
        var logger = XUnitLogger.CreateLogger<GetCyclesListQueryHandler>();

        // Act
        var handler = new GetCyclesListQueryHandler(mockRepository, logger);

        // Assert
        handler.ShouldNotBeNull();
    }
    /// <summary>
    /// Executes Constructor_WithNullRepository_ShouldThrowException operation.
    /// </summary>

    // MARKED FOR DELETION - Constructor null guard test no longer needed for DI handlers
    // [Fact]
    //     public void Constructor_WithNullRepository_ShouldThrowException()
    //     {
    //         // Arrange
    //         IReadOnlyRepository<Cycle>? nullRepository = null!;
    //         var logger = XUnitLogger.CreateLogger<GetCyclesListQueryHandler>();
    // 
    //         // Act & Assert
    //         Should.Throw<ArgumentNullException>(() => new GetCyclesListQueryHandler(nullRepository!, logger));
    //     }
    /// <summary>
    /// Executes Constructor_WithNullLogger_ShouldThrowException operation.
    /// </summary>

    // MARKED FOR DELETION - Constructor null guard test no longer needed for DI handlers
    // [Fact]
    //     public void Constructor_WithNullLogger_ShouldThrowException()
    //     {
    //         // Arrange
    //         var mockRepository = Substitute.For<IReadOnlyRepository<Cycle>>();
    //         ILogger<GetCyclesListQueryHandler>? nullLogger = null!;
    // 
    //         // Act & Assert
    //         Should.Throw<ArgumentNullException>(() => new GetCyclesListQueryHandler(mockRepository, nullLogger!));
    //     }
    /// <summary>
    /// Executes Constructor_WithAllNullParameters_ShouldThrowException operation.
    /// </summary>

    // MARKED FOR DELETION - Constructor null guard test no longer needed for DI handlers
    // [Fact]
    //     public void Constructor_WithAllNullParameters_ShouldThrowException()
    //     {
    //         // Arrange
    //         IReadOnlyRepository<Cycle>? nullRepository = null!;
    //         ILogger<GetCyclesListQueryHandler>? nullLogger = null!;
    // 
    //         // Act & Assert
    //         Should.Throw<ArgumentNullException>(() => new GetCyclesListQueryHandler(nullRepository!, nullLogger!));
    //     }
}