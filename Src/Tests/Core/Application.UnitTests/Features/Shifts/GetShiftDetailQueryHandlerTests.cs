// <copyright file="GetShiftDetailQueryHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Shifts;

/// <summary>
/// Unit tests for GetShiftDetailQueryHandler
/// </summary>
public class GetShiftDetailQueryHandlerTests
{
    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var mockRepository = Substitute.For<IRepository<Shift>>();
        var logger = XUnitLogger.CreateLogger<GetShiftDetailQueryHandler>();

        // Act
        var handler = new GetShiftDetailQueryHandler(mockRepository, logger);

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
    //         IRepository<Shift>? nullRepository = null!;
    //         var logger = XUnitLogger.CreateLogger<GetShiftDetailQueryHandler>();
    // 
    //         // Act & Assert
    //         Should.Throw<ArgumentNullException>(() => new GetShiftDetailQueryHandler(nullRepository!, logger));
    //     }
    /// <summary>
    /// Executes Constructor_WithNullLogger_ShouldThrowException operation.
    /// </summary>

    // MARKED FOR DELETION - Constructor null guard test no longer needed for DI handlers
    // [Fact]
    //     public void Constructor_WithNullLogger_ShouldThrowException()
    //     {
    //         // Arrange
    //         var mockRepository = Substitute.For<IRepository<Shift>>();
    //         ILogger<GetShiftDetailQueryHandler>? nullLogger = null!;
    // 
    //         // Act & Assert
    //         Should.Throw<ArgumentNullException>(() => new GetShiftDetailQueryHandler(mockRepository, nullLogger!));
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
    //         ILogger<GetShiftDetailQueryHandler>? nullLogger = null!;
    //
    //         // Act & Assert
    //         Should.Throw<ArgumentNullException>(() => new GetShiftDetailQueryHandler(nullRepository!, nullLogger!));
    //     }

    /// <summary>
    /// Issue #118 (Chunk A): the handler must resolve the shift through a keyed specification
    /// (FirstOrDefaultAsync) whose criteria filters by ShiftId — not by scanning the full table.
    /// </summary>
    /// <returns>The result of Process_WithValidShiftId_ShouldReturnShiftResolvedBySpecification.</returns>
    [Fact]
    public async Task Process_WithValidShiftId_ShouldReturnShiftResolvedBySpecification()
    {
        // Arrange
        var repository = Substitute.For<IRepository<Shift>>();
        var handler = new GetShiftDetailQueryHandler(repository, XUnitLogger.CreateLogger<GetShiftDetailQueryHandler>());
        var dateTimeMachine = Substitute.For<IDateTimeMachine>();
        var targetShift = new Shift(dateTimeMachine) { ShiftId = new ShiftId(7), ShiftType = "First" };
        var otherShift = new Shift(dateTimeMachine) { ShiftId = new ShiftId(8), ShiftType = "Second" };
        var shifts = new List<Shift> { otherShift, targetShift };

        repository.FirstOrDefaultAsync(Arg.Any<ISpecification<Shift>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var match = shifts.FirstOrDefault(callInfo.Arg<ISpecification<Shift>>().Criteria.Compile());
                return match is not null
                    ? Result<Shift?>.Success(match)
                    : Result<Shift?>.WithFailure("No matching entity found.");
            });

        // Act
        var result = await handler.ProcessAsync(new GetShiftDetailQuery { ShiftId = 7 }, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShiftId.ShouldBe(7);
        result.Value.ShiftName.ShouldBe("First");
    }

    /// <summary>
    /// Issue #118 (Chunk A): the repository's "No matching entity found." sentinel must surface as the
    /// handler's original not-found failure message.
    /// </summary>
    /// <returns>The result of Process_WhenShiftNotFound_ShouldReturnNotFoundFailure.</returns>
    [Fact]
    public async Task Process_WhenShiftNotFound_ShouldReturnNotFoundFailure()
    {
        // Arrange
        var repository = Substitute.For<IRepository<Shift>>();
        var handler = new GetShiftDetailQueryHandler(repository, XUnitLogger.CreateLogger<GetShiftDetailQueryHandler>());

        repository.FirstOrDefaultAsync(Arg.Any<ISpecification<Shift>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Shift?>.WithFailure("No matching entity found."));

        // Act
        var result = await handler.ProcessAsync(new GetShiftDetailQuery { ShiftId = 42 }, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Shift not found 42");
    }

    /// <summary>
    /// Issue #118 (Chunk A): a genuine repository failure (anything but the not-found sentinel) must be
    /// propagated verbatim, preserving the old repository-failure/not-found distinction.
    /// </summary>
    /// <returns>The result of Process_WhenRepositoryFails_ShouldPropagateFailure.</returns>
    [Fact]
    public async Task Process_WhenRepositoryFails_ShouldPropagateFailure()
    {
        // Arrange
        var repository = Substitute.For<IRepository<Shift>>();
        var handler = new GetShiftDetailQueryHandler(repository, XUnitLogger.CreateLogger<GetShiftDetailQueryHandler>());

        repository.FirstOrDefaultAsync(Arg.Any<ISpecification<Shift>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Shift?>.WithFailure("Database connection timeout"));

        // Act
        var result = await handler.ProcessAsync(new GetShiftDetailQuery { ShiftId = 7 }, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Database connection timeout");
    }
}
