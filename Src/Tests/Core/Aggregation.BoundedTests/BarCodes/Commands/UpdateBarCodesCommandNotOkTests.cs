// <copyright file="UpdateBarCodesCommandNotOkTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.BarCodes.Commands;

/// <summary>
/// NotOk path tests for UpdateBarCodesCommand - covering the complete BarCode→Cycle aggregate.
/// Copied from UpdateBarCodesCommandTests and converted to NotOk scenarios.
/// </summary>
public class UpdateBarCodesCommandNotOkTests : DependenciesFactory
{
    public UpdateBarCodesCommandNotOkTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    /// <summary>
    /// Tests NotOk path scenarios for the complete BarCode→Cycle aggregate flow.
    /// Uses comprehensive test data from the same dataset with NOk status to test failure scenarios.
    /// Enhanced with more data points for better safety coverage.
    /// </summary>
    [Theory]
    [InlineData("L1AL100003232372504", 100, 7, 4, 4, "2023-08-27T03:41:07")]
    [InlineData("L1AL100003232372507", 100, 7, 7, 7, "2023-08-27T11:50:57")]
    [InlineData("L1AL100003232372510", 100, 7, 10, 10, "2023-08-27T12:19:35")]
    [InlineData("L1AL100003232372516", 300, 7, 16, 17, "2023-08-27T12:20:44")]
    [InlineData("L1AL100003232372518", 300, 7, 18, 21, "2023-08-27T12:24:15")]
    [InlineData("L1AL100003232372528", 300, 7, 28, 41, "2023-08-27T16:56:05")]
    [InlineData("L1AL100003232372531", 500, 7, 31, 48, "2023-08-27T10:03:02")]
    [InlineData("L1AL100003232372534", 500, 7, 34, 57, "2023-08-27T10:05:55")]
    [InlineData("L1AL100003232372537", 500, 7, 37, 66, "2023-08-27T10:08:44")]
    [InlineData("L1AL90164629232372562", 300, 9, 62, 109, "2023-08-27T12:20:44")]
    [InlineData("L1AL90164629232372564", 300, 9, 64, 113, "2023-08-27T12:24:15")]
    [InlineData("L1AL90164629232372576", 500, 9, 76, 138, "2023-08-27T10:03:02")]
    [InlineData("L1AL90164629232372579", 500, 9, 79, 147, "2023-08-27T10:08:44")]
    public async Task ShouldUpdateBarCode_WithUpdateCycleNotOk(string label,
        int machineId,
        int lenPartNumber,
        int barCodeId,
        int cycleId,
        DateTime startCycleStr)
    {
        await Initialization;

        // Arrange - Same pattern as working OK test, but with NotOk status

        // NO MOCKING: Use real DependenciesFactory dispatcher
        var timeCycle = new TimeSpan(0, 0, 45);
        var finCycle = startCycleStr.Add(timeCycle);

        // NO MOCKING: Use real DependenciesFactory members
        DpDateTimeMachine.SetDateTimeNow(finCycle);
        var end = 3 + lenPartNumber;
        var partNumber = label[3..end];

        var sut = new UpdateBarCodeCommandHandler(DpDateTimeMachine, DpBarCodeAggregateRepository, DpBarCodeDetailsLoader);

        var command = new UpdateBarCodeCommand();
        // KEY CHANGE: Use PartStatus.NOk instead of PartStatus.Ok
        command.WithData(TaskGatewayRequest.Create(machineId, label, partNumber, PartStatus.NOk));

        // Act
        var result = await sut.ProcessAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert - Test NotOk aggregate behavior
        result.Value.ShouldBeOfType<TaskGatewayResponseDto>();
        result.Value.ResultValidation.ShouldBe(ResultValidation.Valid);
        result.Value.CycleId.ShouldBe(cycleId);
        result.Value.BarCodeId.ShouldBe(barCodeId);

        //[Fix]
        //CLAUDE
        //Date: 20/09/2025
        //Reason: [Complete Aggregate Testing] - NotOk path for BarCode→Cycle aggregate
        // Tests the complete manufacturing flow with failure scenarios
        // Copied from working UpdateBarCodesCommandTests and converted to NotOk paths
    }

    /// <summary>
    /// Simple NotOk test following the established pattern.
    /// </summary>
    [Fact]
    public async Task UpdateBarCodeCommand_NotOk_ShouldProcessSuccessfully()
    {
        await Initialization;

        // Arrange - Simple NotOk scenario
        DpDateTimeMachine.SetDateTimeNow(new DateTimeOffset(2020, 06, 06, 06, 06, 06, 6, TimeSpan.Zero));

        var sut = new UpdateBarCodeCommandHandler(DpDateTimeMachine, DpBarCodeAggregateRepository, DpBarCodeDetailsLoader);
        var command = new UpdateBarCodeCommand();

        // Use existing barcode from test data with NotOk status
        command.WithData(TaskGatewayRequest.Create(100, "L1AL100003232372501", "L100003", PartStatus.NOk));

        // Act
        var result = await sut.ProcessAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue($"NotOk command should be processed successfully: {string.Join(", ", result.Errors ?? [])}");

        //[Fix]
        //CLAUDE
        //Date: 20/09/2025
        //Reason: [NotOk Path Coverage] - Simple NotOk test for basic validation
        // Ensures the aggregate handles NotOk scenarios correctly
    }

    /// <summary>
    /// Tests that NotOk updates fail when trying to update a cycle from a different machine.
    /// Critical business rule: A station cannot update cycles created by another station.
    /// Uses comprehensive test data from the same dataset for better coverage.
    /// </summary>
    [Theory]
    [InlineData("L1AL100003232372504", 500, 7, 4, 4, "2023-08-27T03:41:07")]    // Created on Machine 100, attempt from Machine 500
    [InlineData("L1AL100003232372507", 300, 7, 7, 7, "2023-08-27T11:50:57")]    // Created on Machine 100, attempt from Machine 300
    [InlineData("L1AL100003232372510", 500, 7, 10, 10, "2023-08-27T12:19:35")]  // Created on Machine 100, attempt from Machine 500
    [InlineData("L1AL100003232372516", 500, 7, 16, 17, "2023-08-27T12:20:44")]  // Created on Machine 300, attempt from Machine 500
    [InlineData("L1AL100003232372518", 100, 7, 18, 21, "2023-08-27T12:24:15")]  // Created on Machine 300, attempt from Machine 100
    [InlineData("L1AL100003232372528", 100, 7, 28, 41, "2023-08-27T16:56:05")]  // Created on Machine 300, attempt from Machine 100
    [InlineData("L1AL90164629232372562", 500, 9, 62, 109, "2023-08-27T12:20:44")] // Created on Machine 300, attempt from Machine 500
    [InlineData("L1AL90164629232372564", 100, 9, 64, 113, "2023-08-27T12:24:15")] // Created on Machine 300, attempt from Machine 100
    [InlineData("L1AL90164629232372576", 300, 9, 76, 138, "2023-08-27T10:03:02")] // Created on Machine 500, attempt from Machine 300
    [InlineData("L1AL90164629232372579", 100, 9, 79, 147, "2023-08-27T10:08:44")] // Created on Machine 500, attempt from Machine 100
    public async Task ShouldNotUpdate_BarCodeNotOk_WhenTryingToUpdateCycleFromDifferentMachine(string label,
        int attemptedMachineId, // Different machine trying to update
        int lenPartNumber,
        int barCodeId,
        int cycleId,
        DateTime startCycleStr)
    {
        await Initialization;

        DpLogger.LogInformation("Starting cross-machine NotOk update test for label {Label} from machine {MachineId}", label, attemptedMachineId);
        //logging barcode id and cycle ud
        DpLogger.LogInformation("?BarCodeId: {BarCodeId}, CycleId: {CycleId}", barCodeId, cycleId);
        // Arrange - Try to update from wrong machine (business rule violation)
        var timeCycle = new TimeSpan(0, 0, 45);
        var finCycle = startCycleStr.Add(timeCycle);
        DpDateTimeMachine.SetDateTimeNow(finCycle);
        var end = 3 + lenPartNumber;
        var partNumber = label[3..end];

        var sut = new UpdateBarCodeCommandHandler(DpDateTimeMachine, DpBarCodeAggregateRepository, DpBarCodeDetailsLoader);
        var command = new UpdateBarCodeCommand();

        // KEY: Use different machine ID than where cycle was created (business rule violation)
        command.WithData(TaskGatewayRequest.Create(attemptedMachineId, label, partNumber, PartStatus.NOk));

        // Act
        var result = await sut.ProcessAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert - Should FAIL due to cross-machine update violation
        result.ShouldNotBeNull();

        if (result.IsSuccess)
        {
            // If somehow successful, verify it has proper validation result
            result.Value.ShouldBeOfType<TaskGatewayResponseDto>();
            result.Value.ResultValidation.ShouldBe(ResultValidation.DestinationNotValid,
                "Cross-machine cycle update should be marked as DestinationNotValid");
        }
        else
        {
            // Expected: Should fail due to business rule violation
            result.IsFailure.ShouldBeTrue("Cross-machine cycle update should fail business validation");
            result.Errors.ShouldNotBeEmpty("Failed cross-machine update should have descriptive error messages");
        }

        //[Fix]
        //CLAUDE
        //Date: 20/09/2025
        //Reason: [Edge Case Coverage] - Critical business rule validation
        // Manufacturing safety rule: Station cannot update cycles created by different stations
        // Mirrors ShouldNotUpdate_BarCodes_InFinalStation_WhenFlow_IsNotComplete pattern for NotOk scenarios
    }

    /// <summary>
    /// Tests that NotOk updates fail when flow is not complete (similar to OK path edge case).
    /// Uses comprehensive test data from the same dataset matching the OK path patterns.
    /// </summary>
    [Theory]
    [InlineData("L1AL100003232372502", 500, 7, 2, 1, "2023-08-27 12:21:31.2170000")]
    [InlineData("L1AL100003232372504", 500, 7, 4, 4, "2023-08-27 12:21:31.2170000")]
    [InlineData("L1AL90164629232372554", 500, 7, 54, 92, "2023-08-27 12:21:31.2170000")]
    [InlineData("L1AL90164629232372557", 500, 7, 57, 155, "2023-08-27 12:21:31.2170000")]
    public async Task ShouldNotUpdate_BarCodesNotOk_InFinalStation_WhenFlow_IsNotComplete(string label,
        int machineId,
        int lenPartNumber,
        int barCodeId,
        int cycleId,
        DateTime startCycle)
    {
        await Initialization;
        DpLogger.LogInformation("Starting cross-machine NotOk update test for label {Label} from machine {MachineId}", label, machineId);
        //logging barcode id and cycle ud
        DpLogger.LogInformation("?BarCodeId: {BarCodeId}, CycleId: {CycleId}", barCodeId, cycleId);
        // Arrange - Try to update from wrong machine (business rule violation)
        // Arrange - Same pattern as OK path but for NotOk scenarios
        var timeCycle = new TimeSpan(0, 0, 45);
        var finCycle = startCycle.Add(timeCycle);
        DpDateTimeMachine.SetDateTimeNow(finCycle);
        var end = 3 + lenPartNumber;
        var partNumber = label[3..end];

        var sut = new UpdateBarCodeCommandHandler(DpDateTimeMachine, DpBarCodeAggregateRepository, DpBarCodeDetailsLoader);
        var command = new UpdateBarCodeCommand();
        command.WithData(TaskGatewayRequest.Create(machineId, label, partNumber, PartStatus.NOk));

        // Act
        var result = await sut.ProcessAsync(command, cancellationToken: TestContext.Current.CancellationToken);

        // Assert - Should fail or return DestinationNotValid for incomplete flow
        result.ShouldNotBeNull();

        if (result.IsSuccess)
        {
            result.Value.ShouldBeOfType<TaskGatewayResponseDto>();
            result.Value.ResultValidation.ShouldBe(ResultValidation.DestinationNotValid,
                "Incomplete flow should be marked as DestinationNotValid even for NotOk scenarios");
        }
        else
        {
            result.IsFailure.ShouldBeTrue("Incomplete flow should fail validation for NotOk scenarios");
            result.Errors.ShouldNotBeEmpty("Failed incomplete flow should have descriptive error messages");
        }

        //[Fix]
        //CLAUDE
        //Date: 20/09/2025
        //Reason: [Edge Case Completeness] - Mirror OK path edge cases for NotOk scenarios
        // Ensures NotOk path has same business rule validation as OK path
    }
}