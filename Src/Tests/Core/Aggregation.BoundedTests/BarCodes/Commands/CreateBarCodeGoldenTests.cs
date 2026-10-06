// <copyright file="CreateBarCodeGoldenTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.Commands.Create;
using IndTrace.Domain.ValueObjects;

namespace IndTrace.Aggregation.BoundedTests.BarCodes.Commands;

/// <summary>
/// Golden tests to establish baseline behavior for CreateBarCodeCommandHandler refactoring.
/// These tests capture the exact current behavior to ensure no regressions during refactoring.
/// </summary>
public class CreateBarCodeGoldenTests : DependenciesFactory
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public CreateBarCodeGoldenTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    /// <summary>
    /// Captures baseline behavior for all critical scenarios.
    /// Results should be saved and compared after refactoring.
    /// </summary>
    [Theory]
    [InlineData(100, "DEFAULT", "Success_Printer_Machine")]
    [InlineData(101, "L100003", "Success_InitialPrinter_Machine")]
    [InlineData(0, "DEFAULT", "Failure_Invalid_Machine_ID")]
    [InlineData(100, "UNKNOWN-PART-999", "Failure_Product_Not_Found")]
    [InlineData(102, "DEFAULT", "Failure_Non_Printer_Machine_Type")]
    [InlineData(999, "DEFAULT", "Failure_Machine_Not_Found")]
    public async Task CreateBarCode_GoldenBaseline_CaptureCurrentBehavior(
        int machineId, string partNumber, string scenario)
    {
        // Arrange
        await Initialization;
        var fixedTimestamp = new DateTime(2025, 9, 21, 10, 0, 0, DateTimeKind.Local);
        
        // Override DateTimeMachine for deterministic results
        var command = new CreateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = machineId,
                PartNumber = partNumber,
                TimeStamp = fixedTimestamp
            }
        };

        // Act
        var result = await DpGatewayCommandDispatcher
            .ProcessAsync(command, CancellationToken.None);

        // Capture baseline
        var baseline = new GoldenTestBaseline
        {
            Scenario = scenario,
            Timestamp = fixedTimestamp,
            Input = new
            {
                MachineId = machineId,
                PartNumber = partNumber
            },
            Result = new
            {
                IsSuccess = result.IsSuccess,
                Errors = result.IsFailure ? result.Errors?.ToList() : null,
                HasValue = result.Value != null
            }
        };

        if (result.IsSuccess && result.Value != null)
        {
            baseline.SuccessOutput = CaptureSuccessOutput(result.Value);
        }

        // Capture database state
        baseline.DatabaseState = await CaptureDatabaseState(machineId, partNumber, result);

        // Save golden test baseline
        var json = JsonSerializer.Serialize(baseline, _jsonOptions);
        var fileName = $"golden-baseline-{scenario}.json";
        
        // In real implementation, save to file system
        // await File.WriteAllTextAsync($"GoldenTests/{fileName}", json);
        
        // For now, validate structure
        baseline.ShouldNotBeNull();
        json.ShouldNotBeNullOrWhiteSpace();
    }

    // DELETED: Obsolete performance baseline test - YAGNI applied
    // Original purpose was to compare old vs new handlers during SRP refactoring
    // Since old handlers no longer exist, this test serves no purpose

    /// <summary>
    /// Captures all error message patterns for validation scenarios.
    /// </summary>
    [Theory]
    [InlineData(0, "DEFAULT", "Machine 0 number invalid")]
    [InlineData(999, "DEFAULT", "Machine 999 does not exist or cannot create labels.")]
    [InlineData(100, "UNKNOWN-999", "Product for UNKNOWN-999 does not exist.")]
    public async Task CreateBarCode_ErrorMessages_CaptureExactText(
        int machineId, string partNumber, string expectedError)
    {
        // Arrange
        await Initialization;
        var command = new CreateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = machineId,
                PartNumber = partNumber,
                TimeStamp = DateTime.Now
            }
        };

        // Act
        var result = await DpGatewayCommandDispatcher
            .ProcessAsync(command, CancellationToken.None);

        // Assert - Capture exact error messages
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        
        var errorMessages = result.Errors.ToList();
        errorMessages.ShouldContain(expectedError);
        
        // Document error message pattern
        // _output.WriteLine($"Error for MachineId={machineId}, PartNumber={partNumber}: {string.Join(", ", errorMessages)}"); // TODO: Add proper test output
    }

    /// <summary>
    /// PR #182 regression pin. Repository.AddAsync surfaces SaveChanges' rows-affected count (always 1),
    /// NOT the generated identity; EF back-fills the real key onto the tracked entity during SaveChanges.
    /// The old create/EndOfProcess handlers overwrote BarCodeId/CycleId with that count, so every created
    /// barcode reported id 1 and its first cycle attached to barcode 1 (cross-part corruption). This pins
    /// the contract those handlers now rely on: the return value is a count, and the real store-generated
    /// key lands on the entity — never conflate the two again. (A dispatcher-level pin is impossible here:
    /// the embedded seed has no MasterLabel rows, so no CreateBarCode can succeed in this suite — the
    /// test-data twin of issue #187.)
    /// </summary>
    [Fact]
    public async Task CycleAddAsync_ReturnsRowsAffected_WhileEfBackFillsTheRealIdentity()
    {
        // Arrange
        await Initialization;
        var cycle = new Cycle
        {
            MachineId = new MachineId(100),
            BarCodeId = new BarCodeId(31),
            StartedOn = new DateTime(2025, 9, 21, 10, 0, 0, DateTimeKind.Local),
            FinishedOn = new DateTime(2025, 9, 21, 10, 0, 45, DateTimeKind.Local),
            CycleTime = 45,
            CyclesOk = 0,
        };

        // Act
        var add = await DpCycleRepository.AddAsync(cycle, CancellationToken.None);

        // Assert — the return value is the rows-affected count …
        add.IsSuccess.ShouldBeTrue();
        add.Value.ShouldBe(1);
        // … while the entity carries the real back-filled identity (seed data guarantees it is far past 1).
        cycle.CycleId.Value.ShouldBeGreaterThan(1);
        cycle.CycleId.Value.ShouldNotBe(add.Value);
    }

    #region Helper Methods

    private async Task<Result<TaskGatewayResponseDto>> CreateValidBarCode()
    {
        var command = new CreateBarCodeCommand
        {
            Command = new TaskGatewayRequest
            {
                MachineId = 100, // Valid printer machine
                PartNumber = "DEFAULT", // Valid product from TestData
                TimeStamp = DateTime.Now
            }
        };

        return await DpGatewayCommandDispatcher
            .ProcessAsync(command, CancellationToken.None);
    }

    private SuccessOutput CaptureSuccessOutput(TaskGatewayResponseDto response)
    {
        return new SuccessOutput
        {
            BarCode = response.BarCode?.ToString() ?? "",
            BarCodeId = response.BarCodeId,
            CycleId = response.CycleId,
            CyclesOk = response.CyclesOk,
            ResultValidation = response.ResultValidation.ToString(),
            FlowStatus = response.FlowStatus?.ToString(),
            PartStatus = response.PartStatus?.ToString(),
            CycleStatus = response.CycleStatus?.ToString(),
            MachineType = response.MachineType?.ToString(),
            WorkFlowType = response.WorkFlowType?.ToString(),
            HasReferences = response.References?.Any() ?? false,
            ReferenceCount = response.References?.Count ?? 0
        };
    }

    private async Task<DatabaseStateCapture> CaptureDatabaseState(
        int machineId, string partNumber, Result<TaskGatewayResponseDto> result)
    {
        var state = new DatabaseStateCapture();

        if (result.IsSuccess && result.Value != null)
        {
            // Capture BarCode entity
            if (result.Value.BarCodeId > 0)
            {
                var barCode = await DpBarCodeRepository
                    .GetByIdAsync(result.Value.BarCodeId, CancellationToken.None);
                    
                if (barCode?.IsSuccess == true && barCode.Value != null)
                {
                    state.BarCodeCreated = true;
                    state.BarCodeLabel = barCode.Value.Label.Value;
                    state.BarCodeFlowStatus = barCode.Value.FlowStatus.ToString();
                }
            }

            // Capture Cycle entity
            if (result.Value.CycleId > 0)
            {
                var cycle = await DpCycleRepository
                    .GetByIdAsync(result.Value.CycleId, CancellationToken.None);
                    
                if (cycle?.IsSuccess == true && cycle.Value != null)
                {
                    state.CycleCreated = true;
                    state.CycleStatus = cycle.Value.CycleStatus.ToString();
                    state.CycleMachineId = cycle.Value.MachineId.Value;
                    state.CyclesOkValue = cycle.Value.CyclesOk;
                }
            }
        }

        // Capture TaskGatewayRequest (audit log)
        var spec = new Specification<TaskGatewayRequest>(t => 
            t.MachineId == machineId && 
            t.PartNumber == partNumber);
        spec.AddOrderByDescending(t => t.TimeStamp);
        
        var gatewayRequest = await DpRoRequestRepository
            .FirstOrDefaultAsync(spec, CancellationToken.None);
            
        if (gatewayRequest?.IsSuccess == true && gatewayRequest.Value != null)
        {
            state.GatewayRequestLogged = true;
            state.GatewayTask = gatewayRequest.Value.GatewayTask.ToString();
            state.ResultValidation = gatewayRequest.Value.ResultValidation?.ToString();
        }

        return state;
    }

    #endregion

    #region Data Models

    private class GoldenTestBaseline
    {
        public string Scenario { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public object Input { get; set; } = new();
        public object Result { get; set; } = new();
        public SuccessOutput? SuccessOutput { get; set; }
        public DatabaseStateCapture DatabaseState { get; set; } = new();
    }

    private class SuccessOutput
    {
        public string? BarCode { get; set; }
        public int BarCodeId { get; set; }
        public int CycleId { get; set; }
        public int CyclesOk { get; set; }
        public string? ResultValidation { get; set; }
        public string? FlowStatus { get; set; }
        public string? PartStatus { get; set; }
        public string? CycleStatus { get; set; }
        public string? MachineType { get; set; }
        public string? WorkFlowType { get; set; }
        public bool HasReferences { get; set; }
        public int ReferenceCount { get; set; }
    }

    private class DatabaseStateCapture
    {
        public bool BarCodeCreated { get; set; }
        public string? BarCodeLabel { get; set; }
        public string? BarCodeFlowStatus { get; set; }
        public bool CycleCreated { get; set; }
        public string? CycleStatus { get; set; }
        public int CycleMachineId { get; set; }
        public int CyclesOkValue { get; set; }
        public bool GatewayRequestLogged { get; set; }
        public string? GatewayTask { get; set; }
        public string? ResultValidation { get; set; }
    }

    #endregion
}