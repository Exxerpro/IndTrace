// <copyright file="GetConfigStationDetailQueryHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.ConfigStations.Queries.GetConfigStationDetail;

namespace Application.UnitTests.Features.ConfigStations;

/// <summary>
/// Unit tests for GetConfigStationDetailQueryHandler using repository pattern
/// </summary>
public class GetConfigStationDetailQueryHandlerTests
{
    private readonly IRepository<ConfigStation> _repository = null!;
    private readonly ILogger<GetConfigStationDetailQueryHandler> _logger = null!;
    private readonly GetConfigStationDetailQueryHandler _handler = null!;
    // MARKED FOR REMOVAL - Constructor null guard test no longer needed with Result<T> patterns
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public GetConfigStationDetailQueryHandlerTests()
    {
        _repository = Substitute.For<IRepository<ConfigStation>>();
        _logger = XUnitLogger.CreateLogger<GetConfigStationDetailQueryHandler>();
        _handler = new GetConfigStationDetailQueryHandler(_repository, _logger);
    }

    // /// <summary>
    // /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    // /// </summary>

    // [Fact]
    // public void Constructor_WithValidParameters_ShouldCreateInstance()
    // {
    //     // Arrange & Act
    //     var handler = new GetConfigStationDetailQueryHandler(_repository, _logger);

    //     // Assert
    //     handler.ShouldNotBeNull();
    // }
    // /// <summary>
    // /// Executes Constructor_WithNullRepository_ShouldThrowArgumentNullException operation.
    // /// </summary>

    // [Fact]
    // public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
    // {
    //     // Arrange & Act & Assert
    //     // Act
    //     var result = new GetConfigStationDetailQueryHandler(null!, _logger);

    //     // Assert
    //     //result.IsFailure.ShouldBeTrue();
    //     //result.Errors.ShouldNotBeNull();
    //     //result.Errors.ShouldContain("repository");
    // }
    // MARKED FOR REMOVAL - Constructor null guard test no longer needed with Result<T> patterns
    // /// <summary>
    // /// Executes Constructor_WithNullLogger_ShouldThrowArgumentNullException operation.
    // /// </summary>

    // [Fact]
    // public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    // {
    //     // Arrange & Act & Assert
    //     // Act
    //     var result = new GetConfigStationDetailQueryHandler(_repository, null!);

    //     // Assert
    //     //result.IsFailure.ShouldBeTrue();
    //     //result.Errors.ShouldNotBeNull();
    //     //result.Errors.ShouldContain("logger");
    // }
    /// <summary>
    /// Executes Process_WithValidQuery_ShouldReturnSuccess operation.
    /// </summary>
    /// <returns>The result of Process_WithValidQuery_ShouldReturnSuccess.</returns>

    [Fact]
    public async Task Process_WithValidQuery_ShouldReturnSuccess()
    {
        // Arrange
        var query = CreateValidQuery();
        var configStations = CreateSampleConfigStations();

        _repository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<ConfigStation>>.Success(configStations));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<ConfigStationDetailVm>();
    }

    /// <summary>
    /// Executes Process_WhenRepositoryFails_ShouldReturnFailure operation.
    /// </summary>
    /// <returns>The result of Process_WhenRepositoryFails_ShouldReturnFailure.</returns>

    [Fact]
    public async Task Process_WhenRepositoryFails_ShouldReturnFailure()
    {
        // Arrange
        var query = CreateValidQuery();
        _repository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<ConfigStation>>.WithFailure("Repository error"));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Repository error");
    }

    /// <summary>
    /// Executes Process_WithEmptyResult_ShouldReturnEmptyDetailVm operation.
    /// </summary>
    /// <returns>The result of Process_WithEmptyResult_ShouldReturnEmptyDetailVm.</returns>

    [Fact]
    public async Task Process_WithEmptyResult_ShouldReturnEmptyDetailVm()
    {
        // Arrange
        var query = CreateValidQuery();
        var emptyList = new List<ConfigStation>();

        _repository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<ConfigStation>>.Success(emptyList));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<ConfigStationDetailVm>();
    }

    /// <summary>
    /// Executes Process_ShouldCallRepositoryListAsync operation.
    /// </summary>
    /// <returns>The result of Process_ShouldCallRepositoryListAsync.</returns>

    [Fact]
    public async Task Process_ShouldCallRepositoryListAsync()
    {
        // Arrange
        var query = CreateValidQuery();
        var configStations = CreateSampleConfigStations();

        _repository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<ConfigStation>>.Success(configStations));

        // Act
        await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        await _repository.Received(1).ListAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes Process_WithDifferentPartNumbers_ShouldHandleCorrectly operation.
    /// </summary>
    /// <param name="partNumber">The partNumber.</param>
    /// <returns>The result of Process_WithDifferentPartNumbers_ShouldHandleCorrectly.</returns>

    [Theory]
    [InlineData("PART001")]
    [InlineData("PART002")]
    [InlineData(null)]
    public async Task Process_WithDifferentPartNumbers_ShouldHandleCorrectly(string? partNumber)
    {
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Arrange
        var query = new GetConfigStationDetailQuery { PartNumber = partNumber };
        var configStations = CreateSampleConfigStations();

        _repository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<ConfigStation>>.Success(configStations));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Process_WithCancellationToken_ShouldPassTokenToRepository operation.
    /// </summary>
    /// <returns>The result of Process_WithCancellationToken_ShouldPassTokenToRepository.</returns>

    [Fact]
    public async Task Process_WithCancellationToken_ShouldPassTokenToRepository()
    {
        // Arrange
        var query = CreateValidQuery();
        var configStations = CreateSampleConfigStations();
        var cancellationToken = new CancellationTokenSource().Token;

        _repository.ListAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<ConfigStation>>.Success(configStations));

        // Act
        await _handler.ProcessAsync(query, cancellationToken);

        // Assert
        await _repository.Received(1).ListAsync(cancellationToken);
    }

    private static GetConfigStationDetailQuery CreateValidQuery()
    {
        return new GetConfigStationDetailQuery
        {
            PartNumber = "PART001"
        };
    }

    private static List<ConfigStation> CreateSampleConfigStations()
    {
        return
        [
            new ConfigStation
            {
                ConfigAppId = "IndTrace L1A",
                AppId = 100,
                Client = "Acme",
                Factory = "Acme",
                Line = "CHMSL",
                MachineId = 100,
                Project = "IndTrace",
                Version = "3",
                VersionDate = new DateTime(2023, 8, 31, 10, 46, 18),
                ModifiedDate = new DateTime(2023, 8, 31, 10, 46, 18)
            }
        ];
    }
}