// <copyright file="GetMachineConfigQueryHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Machines.Queries.GetMachinesConfig;
using IndTrace.Application.Machines.Queries.GetMachinesConfig.DataLoaders;
using IndTrace.Application.Machines.Queries.GetMachinesConfig.Assemblers;

namespace Application.UnitTests.Features.Machines;

/// <summary>
/// Unit tests for GetMachineConfigQueryHandler using SRP-compliant services
/// </summary>
public class GetMachineConfigQueryHandlerTests
{
    private readonly IMachineConfigDataLoader _dataLoaderSub = null!;
    private readonly IMachineConfigAssembler _assemblerSub = null!;
    private readonly ILogger<GetMachineConfigQueryHandler> _logger = null!;
    private readonly GetMachineConfigQueryHandler _handler = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public GetMachineConfigQueryHandlerTests()
    {
        _dataLoaderSub = Substitute.For<IMachineConfigDataLoader>();
        _assemblerSub = Substitute.For<IMachineConfigAssembler>();
        _logger = XUnitLogger.CreateLogger<GetMachineConfigQueryHandler>();

        _handler = new GetMachineConfigQueryHandler(
            _dataLoaderSub,
            _assemblerSub,
            _logger);
    }

    /// <summary>
    /// Executes Constructor_ShouldCreateValidInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_ShouldCreateValidInstance()
    {
        // Arrange & Act
        var handler = new GetMachineConfigQueryHandler(
            _dataLoaderSub,
            _assemblerSub,
            _logger);

        // Assert
        handler.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Process_WithValidPartNumber_ShouldReturnSuccessWithConfiguration operation.
    /// </summary>
    /// <returns>The result of Process_WithValidPartNumber_ShouldReturnSuccessWithConfiguration.</returns>

    [Fact]
    public async Task Process_WithValidPartNumber_ShouldReturnSuccessWithConfiguration()
    {
        // Arrange
        var partNumber = "TEST123";
        var query = new GetMachineConfigQuery { PartNumber = partNumber };

        var expectedVm = new MachineConfigVm
        {
            Machines = new List<MachineConfigDto>(),
            Count = 1
        };

        var mockProduct = Product.CreateFixture(productId: 1, partNumber: partNumber);
        var mockContext = new MachineConfigContext(
            mockProduct,
            new List<WorkFlow>(),
            new List<Machine>(),
            new List<MachinePlc>(),
            new List<Plc>(),
            new List<Variable>());

        _dataLoaderSub.LoadByPartNumberAsync(partNumber, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<MachineConfigContext>.Success(mockContext)));
        
        _assemblerSub.AssembleConfiguration(Arg.Any<MachineConfigContext>())
            .Returns(Result<MachineConfigVm>.Success(expectedVm));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count.ShouldBe(1);
    }

    /// <summary>
    /// Executes Process_WithNonExistentPartNumber_ShouldReturnFailure operation.
    /// </summary>
    /// <returns>The result of Process_WithNonExistentPartNumber_ShouldReturnFailure.</returns>

    [Fact]
    public async Task Process_WithNonExistentPartNumber_ShouldReturnFailure()
    {
        // Arrange
        var partNumber = "NONEXISTENT";
        var query = new GetMachineConfigQuery { PartNumber = partNumber };

        _dataLoaderSub.LoadByPartNumberAsync(partNumber, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<MachineConfigContext>.WithFailure($"Product with PartNumber {partNumber} not found")));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain($"Product with PartNumber {partNumber} not found");
    }

    /// <summary>
    /// Executes Process_WhenDataLoaderFails_ShouldReturnFailure operation.
    /// </summary>
    /// <returns>The result of Process_WhenDataLoaderFails_ShouldReturnFailure.</returns>

    [Fact]
    public async Task Process_WhenDataLoaderFails_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetMachineConfigQuery { PartNumber = "TEST123" };

        _dataLoaderSub.LoadByPartNumberAsync("TEST123", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<MachineConfigContext>.WithFailure("Data loader error")));

        // Act
        var result = await _handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Data loader error");
    }

}