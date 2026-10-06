// <copyright file="CreatePerformanceDataCommandHandlerGatewayTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Performance;

/// <summary>
/// #126 adversarial review C13: the PO decision "(2026-07-15) invalid PLC inputs FAIL the OEE
/// calculation" must hold on the live gateway path. When <see cref="OeeRegister.CalculateOee"/>
/// fails, the only gateway consumer of this handler must NOT persist the garbage-derived register
/// and must return the failure Result (the gateway's fire-and-forget dispatch layer logs a failed
/// Result loudly and skips — a failure can never crash the message loop).
/// </summary>
public class CreatePerformanceDataCommandHandlerGatewayTests
{
    private readonly IRepository<OeeRegister> _oeeRegisterRepository;
    private readonly IRepository<KpiOee> _kpiOeeRepository;
    private readonly CreatePerformanceDataCommandHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePerformanceDataCommandHandlerGatewayTests"/> class.
    /// </summary>
    public CreatePerformanceDataCommandHandlerGatewayTests()
    {
        _oeeRegisterRepository = Substitute.For<IRepository<OeeRegister>>();
        _kpiOeeRepository = Substitute.For<IRepository<KpiOee>>();
        _handler = new CreatePerformanceDataCommandHandler(
            XUnitLogger.CreateLogger<CreatePerformanceDataCommandHandler>(),
            _oeeRegisterRepository,
            _kpiOeeRepository);
    }

    /// <summary>
    /// An impossible PLC sample (negative TotalProduction, #126 F1.2) fails the OEE calculation:
    /// the handler must return a failure and write NOTHING to either repository.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task Process_WhenOeeCalculationFails_DoesNotPersistAndReturnsFailure()
    {
        // Arrange - a PLC sample CalculateOee rejects per the PO decision.
        var request = new PerformanceDataCommand(new TaskGatewayRequest
        {
            MachineId = 100,
            BarCode = "TEST123",
            PartNumber = "PART001",
        })
        {
            MachineId = 100,
            TotalProduction = -5, // impossible reading -> CalculateOee failure (#126 F1.2)
            CurrentTime = 100,
            RunningTime = 80,
        };

        // Act
        var result = await _handler.ProcessAsync(request, TestContext.Current.CancellationToken);

        // Assert - the failure propagates and no garbage-derived row is persisted.
        result.IsFailure.ShouldBeTrue();
        await _oeeRegisterRepository.DidNotReceive().AddAsync(Arg.Any<OeeRegister>(), Arg.Any<CancellationToken>());
        await _kpiOeeRepository.DidNotReceive().AddAsync(Arg.Any<KpiOee>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// A merely-degraded sample (warnings only, e.g. zero counters needing fallbacks) keeps the
    /// pre-fix behavior: the register is persisted and the handler succeeds.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task Process_WhenOeeCalculationOnlyWarns_StillPersists()
    {
        // Arrange - all-zero counters produce fallback WARNINGS, not errors.
        var request = new PerformanceDataCommand(new TaskGatewayRequest
        {
            MachineId = 100,
            BarCode = "TEST123",
            PartNumber = "PART001",
        })
        {
            MachineId = 100,
        };

        _oeeRegisterRepository.AddAsync(Arg.Any<OeeRegister>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(1));
        _kpiOeeRepository.AddAsync(Arg.Any<KpiOee>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(1));

        // Act
        var result = await _handler.ProcessAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await _oeeRegisterRepository.Received(1).AddAsync(Arg.Any<OeeRegister>(), Arg.Any<CancellationToken>());
        await _kpiOeeRepository.Received(1).AddAsync(Arg.Any<KpiOee>(), Arg.Any<CancellationToken>());
    }
}
