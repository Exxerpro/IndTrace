// <copyright file="GetMachinePlcDetailQueryHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Machines;

/// <summary>
/// Unit tests for GetMachinePlcDetailQueryHandler
/// </summary>
public class GetMachinePlcDetailQueryHandlerTests
{
    private readonly IReadOnlyRepository<MachinePlc> _repository = Substitute.For<IReadOnlyRepository<MachinePlc>>();
    private readonly ILogger<GetMachinePlcDetailQueryHandler> _logger = XUnitLogger.CreateLogger<GetMachinePlcDetailQueryHandler>();
    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var handler = new GetMachinePlcDetailQueryHandler(_repository, _logger);

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
    //         IReadOnlyRepository<MachinePlc>? nullRepository = null!;
    // 
    //         // Act & Assert
    //         Should.Throw<ArgumentNullException>(() => new GetMachinePlcDetailQueryHandler(nullRepository!, _logger));
    //     }
    /// <summary>
    /// Executes Constructor_WithNullLogger_ShouldThrowException operation.
    /// </summary>

    // MARKED FOR DELETION - Constructor null guard test no longer needed for DI handlers
    // [Fact]
    //     public void Constructor_WithNullLogger_ShouldThrowException()
    //     {
    //         // Arrange
    //         ILogger<GetMachinePlcDetailQueryHandler>? nullLogger = null!;
    // 
    //         // Act & Assert
    //         Should.Throw<ArgumentNullException>(() => new GetMachinePlcDetailQueryHandler(_repository, nullLogger!));
    //     }
    /// <summary>
    /// Executes Process_WithValidQuery_ShouldReturnSuccess operation.
    /// </summary>
    /// <returns>The result of Process_WithValidQuery_ShouldReturnSuccess.</returns>

    [Fact]
    public async Task Process_WithValidQuery_ShouldReturnSuccess()
    {
        // Arrange
        var handler = new GetMachinePlcDetailQueryHandler(_repository, _logger);
        var query = new GetMachinePlcDetailQuery { MachineId = 10000, PlcId = 200 };
        var machinePlc = MachinePlc.CreateFixture(10000, 200, ActiveStatus.Active);
        var machinePlcList = new List<MachinePlc> { machinePlc };

        // Issue #118 (Chunk A): the handler resolves the row through a composite-key specification.
        // The stub compiles the captured criteria against sample data, mirroring the SQL-side filter.
        _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<MachinePlc>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var match = machinePlcList.FirstOrDefault(callInfo.Arg<ISpecification<MachinePlc>>().Criteria.Compile());
                return match is not null
                    ? Result<MachinePlc?>.Success(match)
                    : Result<MachinePlc?>.WithFailure("No matching entity found.");
            });

        // Act
        var result = await handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
    }
    /// <summary>
    /// Executes Process_WhenEntityNotFound_ShouldReturnFailure operation.
    /// </summary>
    /// <returns>The result of Process_WhenEntityNotFound_ShouldReturnFailure.</returns>

    [Fact]
    public async Task Process_WhenEntityNotFound_ShouldReturnFailure()
    {
        // Arrange
        var handler = new GetMachinePlcDetailQueryHandler(_repository, _logger);
        var query = new GetMachinePlcDetailQuery { MachineId = 10000, PlcId = 200 };

        // Issue #118 (Chunk A): the real repository signals "no rows matched" with the sentinel failure.
        _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<MachinePlc>>(), Arg.Any<CancellationToken>())
            .Returns(Result<MachinePlc?>.WithFailure("No matching entity found."));

        // Act
        var result = await handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain($"MachinePlc with PlcId {query.PlcId} and MachineId {query.MachineId} not found");
    }
    /// <summary>
    /// Executes Process_WhenRepositoryFails_ShouldReturnFailure operation.
    /// </summary>
    /// <returns>The result of Process_WhenRepositoryFails_ShouldReturnFailure.</returns>

    [Fact]
    public async Task Process_WhenRepositoryFails_ShouldReturnFailure()
    {
        // Arrange
        var handler = new GetMachinePlcDetailQueryHandler(_repository, _logger);
        var query = new GetMachinePlcDetailQuery { MachineId = 10000, PlcId = 200 };

        // Issue #118 (Chunk A): a non-sentinel failure is a genuine repository error and must propagate.
        _repository.FirstOrDefaultAsync(Arg.Any<ISpecification<MachinePlc>>(), Arg.Any<CancellationToken>())
            .Returns(Result<MachinePlc?>.WithFailure("Repository error"));

        // Act
        var result = await handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Repository error");
    }
}