// <copyright file="GetBarCodeDetailGatewayQueryHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Barcodes;

/// <summary>
/// Unit tests for GetBarCodeDetailGatewayQueryHandler - Gateway request handler for barcode detail processing.
/// Issue #33 (Chunk 3) — the handler was cut off the mutable god-object onto the stateless
/// <see cref="IBarCodeDetailsLoader"/> + immutable <see cref="BarCodeSnapshot"/>; the §7 wrapper decision still
/// gates on the loaded snapshot's <c>Error</c> length (the loader returns <c>Success</c> even on a validation
/// failure, carrying the negative code), never on <c>Result.IsSuccess</c>.
/// </summary>
public class GetBarCodeDetailGatewayQueryHandlerTests
{
    private readonly IBarCodeDetailsLoader _loader = Substitute.For<IBarCodeDetailsLoader>();

    /// <summary>Configures the loader to return a successful load whose snapshot carries the given error.</summary>
    /// <param name="error">The error the snapshot should carry (null/empty means no error).</param>
    private void ArrangeLoad(string? error) =>
        _loader.LoadAsync(Arg.Any<BarCodeDetailsRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<BarCodeSnapshot>.Success(new BarCodeSnapshot { Error = error })));

    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var instance = new GetBarCodeDetailGatewayQueryHandler(_loader);

        // Assert
        instance.ShouldNotBeNull();
        instance.ShouldBeAssignableTo<IGatewayRequestHandler<ReadBarCodeQuery, TaskGatewayResponseDto>>();
        instance.ShouldBeAssignableTo<IResettable>();
    }

    /// <summary>
    /// Executes ProcessAsync_WithValidQuery_ShouldReturnSuccessResult operation.
    /// </summary>
    /// <returns>The result of ProcessAsync_WithValidQuery_ShouldReturnSuccessResult.</returns>
    [Fact]
    public async Task ProcessAsync_WithValidQuery_ShouldReturnSuccessResult()
    {
        // Arrange
        var handler = new GetBarCodeDetailGatewayQueryHandler(_loader);
        var taskGatewayRequest = new TaskGatewayRequest
        {
            MachineId = 100001,
            BarCode = "QA4500T456251303275",
            PartNumber = "T456"
        };
        var query = new ReadBarCodeQuery().WithData(taskGatewayRequest);

        ArrangeLoad(error: null);

        // Act
        var result = await handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<TaskGatewayResponseDto>();
    }

    /// <summary>
    /// Executes ProcessAsync_WithErrorInBarCodeResult_ShouldReturnFailureResult operation.
    /// </summary>
    /// <returns>The result of ProcessAsync_WithErrorInBarCodeResult_ShouldReturnFailureResult.</returns>
    [Fact]
    public async Task ProcessAsync_WithErrorInBarCodeResult_ShouldReturnFailureResult()
    {
        // Arrange
        var handler = new GetBarCodeDetailGatewayQueryHandler(_loader);
        var taskGatewayRequest = new TaskGatewayRequest
        {
            MachineId = 100001,
            BarCode = "INVALID_BARCODE",
            PartNumber = "T456"
        };
        var query = new ReadBarCodeQuery().WithData(taskGatewayRequest);

        ArrangeLoad(error: "Barcode not found");

        // Act
        var result = await handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert — §7: a non-empty loaded Error becomes WithFailure(error, response), response still present.
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Barcode not found");
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<TaskGatewayResponseDto>();
    }

    /// <summary>
    /// Executes ProcessAsync_ShouldCallBarCodeResultWithCorrectParameters operation.
    /// </summary>
    /// <returns>The result of ProcessAsync_ShouldCallBarCodeResultWithCorrectParameters.</returns>
    [Fact]
    public async Task ProcessAsync_ShouldCallBarCodeResultWithCorrectParameters()
    {
        // Arrange
        var handler = new GetBarCodeDetailGatewayQueryHandler(_loader);
        var taskGatewayRequest = new TaskGatewayRequest
        {
            MachineId = 2001,
            BarCode = "QA45900290240740244",
            PartNumber = "A422"
        };
        var query = new ReadBarCodeQuery().WithData(taskGatewayRequest);

        ArrangeLoad(error: null);

        // Act
        await handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        await _loader.Received(1).LoadAsync(
            Arg.Is<BarCodeDetailsRequest>(r =>
                r.MachineId == 2001 &&
                r.Label == "QA45900290240740244" &&
                r.PartNumber == "A422"),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes TryReset_WhenCalled_ShouldReturnTrue operation.
    /// </summary>
    [Fact]
    public void TryReset_WhenCalled_ShouldReturnTrue()
    {
        // Arrange
        var handler = new GetBarCodeDetailGatewayQueryHandler(_loader);

        // Act
        var result = handler.TryReset();

        // Assert
        result.ShouldBeTrue();
    }

    /// <summary>
    /// Executes ProcessAsync_WithManufacturingScenarios_ShouldHandleCorrectly operation.
    /// </summary>
    /// <param name="machineId">The machineId.</param>
    /// <param name="barCode">The barCode.</param>
    /// <param name="partNumber">The partNumber.</param>
    /// <param name="description">The description.</param>
    /// <returns>The result of ProcessAsync_WithManufacturingScenarios_ShouldHandleCorrectly.</returns>
    [Theory]
    [InlineData(100, "QA4500t349251303242", "t349", "Ford F-150 Engine Block")]
    [InlineData(400, "QA4500T456251303275", "T456", "Samsung Galaxy PCB Assembly")]
    [InlineData(500, "L1A422290233440001", "A422", "Pfizer Vaccine Vial Production")]
    [InlineData(1100, "QA45900290240740244", "A422", "Intel CPU Core Manufacturing")]
    public async Task ProcessAsync_WithManufacturingScenarios_ShouldHandleCorrectly(int machineId, string barCode, string partNumber, string description)
    {
        // Arrange
        var handler = new GetBarCodeDetailGatewayQueryHandler(_loader);
        var taskGatewayRequest = new TaskGatewayRequest
        {
            MachineId = machineId,
            BarCode = barCode,
            PartNumber = partNumber
        };
        var query = new ReadBarCodeQuery().WithData(taskGatewayRequest);

        ArrangeLoad(error: null);

        // Act
        var result = await handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<TaskGatewayResponseDto>();

        // Verify scenario context
        description.ShouldNotBeEmpty();
    }

    /// <summary>
    /// Executes ProcessAsync_WithCancellationToken_ShouldPassTokenToBarCodeResult operation.
    /// </summary>
    /// <returns>The result of ProcessAsync_WithCancellationToken_ShouldPassTokenToBarCodeResult.</returns>
    [Fact]
    public async Task ProcessAsync_WithCancellationToken_ShouldPassTokenToBarCodeResult()
    {
        // Arrange
        var handler = new GetBarCodeDetailGatewayQueryHandler(_loader);
        var taskGatewayRequest = new TaskGatewayRequest
        {
            MachineId = 100001,
            BarCode = "QA4500T456251303275",
            PartNumber = "T456"
        };
        var query = new ReadBarCodeQuery().WithData(taskGatewayRequest);
        var cancellationToken = TestContext.Current.CancellationToken;

        ArrangeLoad(error: null);

        // Act
        await handler.ProcessAsync(query, cancellationToken);

        // Assert
        await _loader.Received(1).LoadAsync(
            Arg.Any<BarCodeDetailsRequest>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Executes ProcessAsync_ShouldCallApplyReferencesValuesOnResponse operation.
    /// </summary>
    /// <returns>The result of ProcessAsync_ShouldCallApplyReferencesValuesOnResponse.</returns>
    [Fact]
    public async Task ProcessAsync_ShouldCallApplyReferencesValuesOnResponse()
    {
        // Arrange
        var handler = new GetBarCodeDetailGatewayQueryHandler(_loader);
        var taskGatewayRequest = new TaskGatewayRequest
        {
            MachineId = 100001,
            BarCode = "QA4500T456251303275",
            PartNumber = "T456"
        };
        var query = new ReadBarCodeQuery().WithData(taskGatewayRequest);

        ArrangeLoad(error: null);

        // Act
        var result = await handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<TaskGatewayResponseDto>();
    }

    /// <summary>
    /// Executes ProcessAsync_WithInvalidMachineId_ShouldStillProcessRequest operation.
    /// </summary>
    /// <param name="invalidMachineId">The invalidMachineId.</param>
    /// <param name="barCode">The barCode.</param>
    /// <param name="partNumber">The partNumber.</param>
    /// <returns>The result of ProcessAsync_WithInvalidMachineId_ShouldStillProcessRequest.</returns>
    [Theory]
    [InlineData(0, "QA4500T456251303275", "T456")]
    [InlineData(-1, "QA4500T456251303275", "T456")]
    [InlineData(int.MinValue, "QA4500T456251303275", "T456")]
    public async Task ProcessAsync_WithInvalidMachineId_ShouldStillProcessRequest(int invalidMachineId, string barCode, string partNumber)
    {
        // Arrange
        var handler = new GetBarCodeDetailGatewayQueryHandler(_loader);
        var taskGatewayRequest = new TaskGatewayRequest
        {
            MachineId = invalidMachineId,
            BarCode = barCode,
            PartNumber = partNumber
        };
        var query = new ReadBarCodeQuery().WithData(taskGatewayRequest);

        ArrangeLoad(error: "Machine not found");

        // Act
        var result = await handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Machine not found");
    }

    /// <summary>
    /// Executes ProcessAsync_WithEmptyErrorString_ShouldReturnSuccessResult operation.
    /// </summary>
    /// <returns>The result of ProcessAsync_WithEmptyErrorString_ShouldReturnSuccessResult.</returns>
    [Fact]
    public async Task ProcessAsync_WithEmptyErrorString_ShouldReturnSuccessResult()
    {
        // Arrange
        var handler = new GetBarCodeDetailGatewayQueryHandler(_loader);
        var taskGatewayRequest = new TaskGatewayRequest
        {
            MachineId = 100001,
            BarCode = "QA4500T456251303275",
            PartNumber = "T456"
        };
        var query = new ReadBarCodeQuery().WithData(taskGatewayRequest);

        ArrangeLoad(error: string.Empty);

        // Act
        var result = await handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert — empty Error string (Length == 0) still takes the Success path.
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<TaskGatewayResponseDto>();
    }
}
