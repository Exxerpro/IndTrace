// <copyright file="MachineUpdateCommandTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Machines;

/// <summary>
/// Unit tests for MachineUpdateCommand
/// </summary>
public class MachineUpdateCommandTests
{
    // MARKED FOR REMOVAL - Constructor null guard test no longer needed with Result<T> patterns
    // /// <summary>
    // /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    // /// </summary>
    // [Fact]
    // public void Constructor_WithValidParameters_ShouldCreateInstance()
    // {
    //     // Arrange
    //     // TODO: Add constructor parameters

    //     // Act
    //     var instance = new MachineUpdateCommand();

    //     // Assert
    //     instance.ShouldNotBeNull();
    // }
    // /// <summary>
    // /// Executes Constructor_WithInvalidParameters_ShouldThrowException operation.
    // /// </summary>

    // [Fact]
    // public void Constructor_WithInvalidParameters_ShouldThrowException()
    // {
    //     // Arrange
    //     // TODO: Add invalid parameters

    //     // Act & Assert
    //     // TODO: Add exception assertion
    // }
    /// <summary>
    /// Executes Properties_WhenSet_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSet_ShouldReturnCorrectValues()
    {
        // Arrange
        var instance = new MachineUpdateCommand();

        // Act & Assert
        // TODO: Test property setters and getters
    }
    /// <summary>
    /// Executes Methods_WhenCalled_ShouldReturnExpectedResults operation.
    /// </summary>

    [Fact]
    public void Methods_WhenCalled_ShouldReturnExpectedResults()
    {
        // Arrange
        var instance = new MachineUpdateCommand();

        // Act
        // TODO: Call methods

        // Assert
        // TODO: Verify results
    }
}

/// <summary>
/// Unit tests for <see cref="MachineUpdateCommandHandler"/> (#113 F8/F3): the handler must load STRICTLY
/// by id, guard renames against name collisions fail-closed, and never misread an infrastructure failure
/// as the repository's not-found sentinel.
/// </summary>
public class MachineUpdateCommandHandlerTests
{
    private readonly IRepository<Machine> _repository;
    private readonly MachineUpdateCommandHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="MachineUpdateCommandHandlerTests"/> class.
    /// </summary>
    public MachineUpdateCommandHandlerTests()
    {
        _repository = Substitute.For<IRepository<Machine>>();
        var logger = XUnitLogger.CreateLogger<MachineUpdateCommandHandler>();
        _handler = new MachineUpdateCommandHandler(_repository, logger, Substitute.For<IMonitorRequestDispatcher>());
    }

    /// <summary>
    /// Mirrors the real repository contract for FirstOrDefaultAsync(spec): the specification criteria is
    /// evaluated against the sample data, a match returns success and no match returns the repository's
    /// "No matching entity found." sentinel failure (Repository.cs).
    /// </summary>
    private static Result<Machine?> FindFirst(IEnumerable<Machine> source, ISpecification<Machine> spec)
    {
        var match = source.FirstOrDefault(spec.Criteria.Compile());
        return match is not null
            ? Result<Machine?>.Success(match)
            : Result<Machine?>.WithFailure("No matching entity found.");
    }

    /// <summary>
    /// Mirrors the real repository contract for CountAsync(spec) over the sample data.
    /// </summary>
    private static Result<int> CountOf(IEnumerable<Machine> source, ISpecification<Machine> spec) =>
        Result<int>.Success(source.Count(spec.Criteria.Compile()));

    /// <summary>
    /// #113 F8: renaming machine 5 to machine 7's existing name must FAIL (name collision) and must
    /// never save — with the old name-OR-id specification the handler would load machine 7 and silently
    /// overwrite it.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task Should_FailAndNeverSave_When_RenamingToAnotherMachinesName()
    {
        // Arrange — machine 7 first in the sample so the old name-OR-id spec resolves the WRONG machine.
        var machine7 = new Machine { MachineId = new MachineId(7), Name = "Press-7", Location = "Line B" };
        var machine5 = new Machine { MachineId = new MachineId(5), Name = "Press-5", Location = "Line A" };
        var sample = new[] { machine7, machine5 };

        _repository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => FindFirst(sample, callInfo.Arg<ISpecification<Machine>>()));
        _repository.CountAsync(Arg.Any<ISpecification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => CountOf(sample, callInfo.Arg<ISpecification<Machine>>()));
        _repository.UpdateAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var command = new MachineUpdateCommand { MachineId = 5, Name = "Press-7" };

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert — fail-closed: the rename is refused and NOTHING is saved.
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("already in use", StringComparison.OrdinalIgnoreCase));
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>());

        // And machine 7 was never touched.
        machine7.Name.ShouldBe("Press-7");
        machine5.Name.ShouldBe("Press-5");
    }

    /// <summary>
    /// #113 F8: a rename to a free name loads the machine STRICTLY by id and saves the update.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task Should_UpdateMachineLoadedById_When_RenameHasNoCollision()
    {
        // Arrange
        var machine7 = new Machine { MachineId = new MachineId(7), Name = "Press-7", Location = "Line B" };
        var machine5 = new Machine { MachineId = new MachineId(5), Name = "Press-5", Location = "Line A" };
        var sample = new[] { machine7, machine5 };

        _repository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => FindFirst(sample, callInfo.Arg<ISpecification<Machine>>()));
        _repository.CountAsync(Arg.Any<ISpecification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => CountOf(sample, callInfo.Arg<ISpecification<Machine>>()));
        _repository.UpdateAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var command = new MachineUpdateCommand { MachineId = 5, Name = "Press-5B", Location = "Line A2" };

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert — machine 5 (loaded by id) was updated; machine 7 untouched.
        result.IsSuccess.ShouldBeTrue();
        machine5.Name.ShouldBe("Press-5B");
        machine5.Location.ShouldBe("Line A2");
        machine7.Name.ShouldBe("Press-7");
        await _repository.Received(1).UpdateAsync(
            Arg.Is<Machine>(m => m.MachineId.Value == 5),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// #113 F8: the name-collision check is fail-closed — when the uniqueness count itself fails the
    /// rename is refused rather than trusted.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task Should_RefuseRename_When_NameCollisionCheckFails()
    {
        // Arrange
        var machine5 = new Machine { MachineId = new MachineId(5), Name = "Press-5", Location = "Line A" };

        _repository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => FindFirst(new[] { machine5 }, callInfo.Arg<ISpecification<Machine>>()));
        _repository.CountAsync(Arg.Any<ISpecification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.WithFailure("Database connection failed"));

        var command = new MachineUpdateCommand { MachineId = 5, Name = "Press-5B" };

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("Could not verify", StringComparison.OrdinalIgnoreCase));
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// #113 F3: an infrastructure failure whose message happens to contain the word "entity" must NOT be
    /// misread as not-found — the handler must propagate the infrastructure error, not the "does not
    /// exist" business message.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task Should_PropagateInfraError_When_LookupFailsWithErrorContainingWordEntity()
    {
        // Arrange
        _repository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.WithFailure(new[] { "The entity framework provider timed out" }));

        var command = new MachineUpdateCommand { MachineId = 5, Name = "Press-5B" };

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("The entity framework provider timed out");
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// #113 F3: the repository's genuine "No matching entity found" sentinel still takes the not-found
    /// path and surfaces the handler's "does not exist" business message.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task Should_ReturnNotFoundMessage_When_LookupFailsWithNotFoundSentinel()
    {
        // Arrange
        _repository.FirstOrDefaultAsync(Arg.Any<Specification<Machine>>(), Arg.Any<CancellationToken>())
            .Returns(Result<Machine?>.WithFailure("No matching entity found."));

        var command = new MachineUpdateCommand { MachineId = 5, Name = "Press-5B" };

        // Act
        var result = await _handler.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Machine 5 does not exist please provide a valid MachineId");
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Machine>(), Arg.Any<CancellationToken>());
    }
}