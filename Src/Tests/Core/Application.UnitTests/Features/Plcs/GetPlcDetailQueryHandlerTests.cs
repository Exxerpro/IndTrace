// <copyright file="GetPlcDetailQueryHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Plcs.Queries.GetDetail.DataLoaders;
using IndTrace.Application.Plcs.Queries.GetDetail.Assemblers;

namespace Application.UnitTests.Features.Plcs;

/// <summary>
/// Unit tests for GetPlcDetailQueryHandler
/// </summary>
public class GetPlcDetailQueryHandlerTests
{
    private readonly IPlcDetailDataLoader _dataLoader = Substitute.For<IPlcDetailDataLoader>();
    private readonly IPlcDetailAssembler _assembler = Substitute.For<IPlcDetailAssembler>();
    private readonly ILogger<GetPlcDetailQueryHandler> _logger = XUnitLogger.CreateLogger<GetPlcDetailQueryHandler>();
    
    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var handler = new GetPlcDetailQueryHandler(
            _dataLoader,
            _assembler,
            _logger);

        // Assert
        handler.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Constructor_WithNullPlcRepository_ShouldThrowException operation.
    /// </summary>

    // MARKED FOR DELETION - Constructor null guard test no longer needed for DI handlers
    // [Fact]
    //     public void Constructor_WithNullPlcRepository_ShouldThrowException()
    //     {
    //         // Arrange
    //         IRepository<Plc>? nullRepository = null!;
    //
    //         // Act & Assert
    //         Should.Throw<ArgumentNullException>(() => new GetPlcDetailQueryHandler(
    //             nullRepository!,
    //             _machinePlcRepository,
    //             _machineRepository,
    //             _variableRepository,
    //             _variablesGroupRepository,
    //             _logger));
    //     }
    /// <summary>
    /// Executes Constructor_WithNullLogger_ShouldThrowException operation.
    /// </summary>

    // MARKED FOR DELETION - Constructor null guard test no longer needed for DI handlers
    // [Fact]
    //     public void Constructor_WithNullLogger_ShouldThrowException()
    //     {
    //         // Arrange
    //         ILogger<GetPlcDetailQueryHandler>? nullLogger = null!;
    //
    //         // Act & Assert
    //         Should.Throw<ArgumentNullException>(() => new GetPlcDetailQueryHandler(
    //             _plcRepository,
    //             _machinePlcRepository,
    //             _machineRepository,
    //             _variableRepository,
    //             _variablesGroupRepository,
    //             nullLogger!));
    //     }
    /// <summary>
    /// Executes Process_WithValidQuery_ShouldReturnSuccess operation.
    /// </summary>
    /// <returns>The result of Process_WithValidQuery_ShouldReturnSuccess.</returns>

    [Fact]
    public async Task Process_WithValidQuery_ShouldReturnSuccess()
    {
        // Arrange
        var handler = new GetPlcDetailQueryHandler(
            _dataLoader,
            _assembler,
            _logger);

        var query = new GetPlcDetailQuery { Id = 100 };
        var plc = Plc.CreateFixture(plcId: 100, machineId: 0, enabled: ActiveStatus.None, name: "Test PLC", ipAddress: "192.168.1.100", plcType: string.Empty, plcBrand: string.Empty, options: string.Empty, commLibrary: string.Empty, brandOwner: string.Empty);
        var context = new PlcDetailContext(
            plc, 
            new List<MachinePlc>(), 
            new List<Machine>(), 
            new List<Variable>(), 
            new List<VariablesGroup>());
        var expectedDto = new PlcDto { PlcId = 100, Name = "Test PLC", IpAddress = "192.168.1.100" };

        _dataLoader.LoadByPlcIdAsync(query.Id, Arg.Any<CancellationToken>())
            .Returns(Result<PlcDetailContext>.Success(context));
        _assembler.AssembleDetail(context)
            .Returns(Result<PlcDto>.Success(expectedDto));

        // Act
        var result = await handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.PlcId.ShouldBe(100);
    }

    /// <summary>
    /// Executes Process_WhenPlcNotFound_ShouldReturnFailure operation.
    /// </summary>
    /// <returns>The result of Process_WhenPlcNotFound_ShouldReturnFailure.</returns>

    [Fact]
    public async Task Process_WhenPlcNotFound_ShouldReturnFailure()
    {
        // Arrange
        var handler = new GetPlcDetailQueryHandler(
            _dataLoader,
            _assembler,
            _logger);

        var query = new GetPlcDetailQuery { Id = 100 };

        _dataLoader.LoadByPlcIdAsync(query.Id, Arg.Any<CancellationToken>())
            .Returns(Result<PlcDetailContext>.WithFailure("PLC not found"));

        // Act
        var result = await handler.ProcessAsync(query, TestContext.Current.CancellationToken);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 23/08/2025
        //Reason: Pattern A Fix - Fixed test logic order, moved result.Value assertion to proper Assert section
        result.IsSuccess.ShouldBeFalse();
        result.Value.ShouldBeNull();
    }
}