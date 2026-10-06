// <copyright file="BarCodeValidationServiceTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Meziantou.Extensions.Logging.Xunit.v3;
using Microsoft.Extensions.Logging;

namespace IndTrace.Domain.UnitTests.ValidationTests
{
    /// <summary>
    /// Represents the BarCodeValidationServiceTests.
    /// </summary>
    public class BarCodeValidationServiceTests(ITestOutputHelper output)
    {
        private readonly IBarCodeValidationService _service = new BarCodeValidationService();
        /// <summary>
        /// Executes Validate_ShouldThrowArgumentNullException_WhenFlowStatusIsNull operation.
        /// </summary>

        [Fact]
        public void Validate_ShouldThrowArgumentNullException_WhenFlowStatusIsNull()
        {
            output.WriteLine("Test started: Validate_ShouldThrowArgumentNullException_WhenFlowStatusIsNull");

            var act = () => _service.Validate(null!, MachineType.Printer, CycleStatus.Started, PartStatus.Ok, 1, 1);

            output.WriteLine($"Inputs: flowStatus=null, machineType=Printer, cycleStatus=Started, partStatus=Ok, machineId=1, nextMachineId=100");

            var exception = Should.Throw<ArgumentNullException>(act);
            exception.ParamName.ShouldBe("flowStatus");

            output.WriteLine("Test completed: Validate_ShouldThrowArgumentNullException_WhenFlowStatusIsNull");
        }

        /// <summary>
        /// Executes Validate_ShouldThrowArgumentNullException_WhenMachineTypeIsNull operation.
        /// </summary>

        [Fact]
        public void Validate_ShouldThrowArgumentNullException_WhenMachineTypeIsNull()
        {
            output.WriteLine("Test started: Validate_ShouldThrowArgumentNullException_WhenMachineTypeIsNull");

            var act = () => _service.Validate(Domain.Enum.FlowStatus.Created, null!, CycleStatus.Started, PartStatus.Ok, 1, 1);

            output.WriteLine($"Inputs: flowStatus=Created, machineType=null, cycleStatus=Started, partStatus=Ok, machineId=1, nextMachineId=100");

            var exception = Should.Throw<ArgumentNullException>(act);
            exception.ParamName.ShouldBe("machineType");

            output.WriteLine("Test completed: Validate_ShouldThrowArgumentNullException_WhenMachineTypeIsNull");
        }

        /// <summary>
        /// Executes Validate_ShouldThrowArgumentNullException_WhenCycleStatusIsNull operation.
        /// </summary>

        [Fact]
        public void Validate_ShouldThrowArgumentNullException_WhenCycleStatusIsNull()
        {
            output.WriteLine("Test started: Validate_ShouldThrowArgumentNullException_WhenCycleStatusIsNull");

            var act = () => _service.Validate(Domain.Enum.FlowStatus.Created, MachineType.Printer, null!, PartStatus.Ok, 1, 1);

            output.WriteLine($"Inputs: flowStatus=Created, machineType=Printer, cycleStatus=null, partStatus=Ok, machineId=1, nextMachineId=100");

            var exception = Should.Throw<ArgumentNullException>(act);
            exception.ParamName.ShouldBe("cycleStatus");

            output.WriteLine("Test completed: Validate_ShouldThrowArgumentNullException_WhenCycleStatusIsNull");
        }

        /// <summary>
        /// Executes Validate_ShouldReturnPartNotValid_WhenPartStatusNotOkAndCycleStatusNotFinishedNok operation.
        /// </summary>

        [Theory]
        [MemberData(nameof(PartNotValidTestData))]
        public void Validate_ShouldReturnPartNotValid_WhenPartStatusNotOkAndCycleStatusNotFinishedNok(PartStatus partStatus,
            CycleStatus cycleStatus)
        {
            // Arrange
            output.WriteLine($"Test started: Validate_ShouldReturnPartNotValid with PartStatus={partStatus}, CycleStatus={cycleStatus}");

            // Act
            var result = _service.Validate(Domain.Enum.FlowStatus.InProcess, MachineType.Printer, cycleStatus, partStatus, 1, 1);

            // Log values
            output.WriteLine($"Inputs: flowStatus=InProcess, machineType=Printer, cycleStatus={cycleStatus}, partStatus={partStatus}, machineId=1, nextMachineId=100");
            output.WriteLine($"Result: {result}");

            // Assert
            result.ShouldBe(ResultValidation.PartNotValid);

            result.ShouldNotBe(ResultValidation.Valid);
            result.Equals(ResultValidation.Valid).ShouldBeFalse();

            output.WriteLine("Test completed: Validate_ShouldReturnPartNotValid");
        }

        /// <summary>
        /// Executes Validate_ShouldReturnDestinationNotValid_WhenNextMachineIdDoesNotMatchMachineId operation.
        /// </summary>

        [Fact]
        public void Validate_ShouldReturnDestinationNotValid_WhenNextMachineIdDoesNotMatchMachineId()
        {
            // Arrange
            output.WriteLine("Test started: Validate_ShouldReturnDestinationNotValid_WhenNextMachineIdDoesNotMatchMachineId");

            // Act
            var result = _service.Validate(Domain.Enum.FlowStatus.InProcess, MachineType.Printer, CycleStatus.Started, PartStatus.Ok, 1, 2);

            // Log values
            output.WriteLine($"Inputs: flowStatus=InProcess, machineType=Printer, cycleStatus=Started, partStatus=Ok, machineId=1, nextMachineId=2");
            output.WriteLine($"Result: {result}");

            // Assert
            result.ShouldBe(ResultValidation.DestinationNotValid);
            result.ShouldNotBe(ResultValidation.Valid);

            output.WriteLine("Test completed: Validate_ShouldReturnDestinationNotValid");
        }

        /// <summary>
        /// Executes Validate_ShouldReturnExpectedResult_ForValidCases operation.
        /// </summary>

        [Theory]
        [ClassData(typeof(ValidCasesData))]
        public void Validate_ShouldReturnExpectedResult_ForValidCases(Domain.Enum.FlowStatus flowStatus, MachineType machineType,
            CycleStatus cycleStatus, ResultValidation expectedResult)
        {
            // Arrange
            output.WriteLine($"Test started: Validate_ShouldReturnExpectedResult with FlowStatus={flowStatus}, MachineType={machineType}, CycleStatus={cycleStatus}");

            // Act
            var result = _service.Validate(flowStatus, machineType, cycleStatus, PartStatus.Ok, 1, 1);

            // Log values
            output.WriteLine($"Inputs: flowStatus={flowStatus}, machineType={machineType}, cycleStatus={cycleStatus}, partStatus=Ok, machineId=1, nextMachineId=100");
            output.WriteLine($"Result: {result}, Expected: {expectedResult}");

            // Assert
            result.ShouldBe(expectedResult);

            output.WriteLine("Test completed: Validate_ShouldReturnExpectedResult");
        }

        // TODO: 🚨 Screaming Test – Validation Required
        // ⚠️ This test fails by design and MUST NOT be skipped or suppressed.
        //
        // Context:
        // - Behavior change was requested by the client.
        // - This test previously passed, but now fails due to changes in domain logic and mappings.
        // - Legacy seed values, part numbers, and test data no longer align with updated operational identifiers.
        // - Confirmation of these mappings took over a month due to communication delays.
        // - Operational database validation is currently blocked — existing seed data is outdated or untraceable.
        //
        // Action Required:
        // ✅ Confirm expected behavior with the operations team in the field.
        // ✅ Update or regenerate seed values and part numbers to reflect current domain usage.
        // ✅ Manually verify new behavior OR update test logic only after explicit confirmation.
        //
        // This test serves as a guardrail against silent regressions in domain assumptions.
        /// <summary>
        /// Executes Validate_ShouldReturnWorkFlowNotValid_ForInvalidCases operation.
        /// </summary>

        [Theory]
        [ClassData(typeof(InvalidCasesData))]
        public void Validate_ShouldReturnWorkFlowNotValid_ForInvalidCases(string caseName, Domain.Enum.FlowStatus flowStatus, MachineType machineType,
            CycleStatus cycleStatus, ResultValidation expectedResult)
        {
            var logger = XUnitLogger.CreateLogger<BarCodeValidationServiceTests>();
            //Logg the scen of the test

            logger.LogInformation("Starting test {CaseName}: Validate_ShouldReturnWorkFlowNotValid with FlowStatus={FlowStatus}, MachineType={MachineType}, CycleStatus={CycleStatus}",
                caseName, flowStatus, machineType, cycleStatus);

            // Arrange
            output.WriteLine($"Test started {caseName}: Validate_ShouldReturnWorkFlowNotValid with FlowStatus={flowStatus}, MachineType={machineType}, CycleStatus={cycleStatus}");

            // Act
            var result = _service.Validate(flowStatus, machineType, cycleStatus, PartStatus.Ok, 100, 100);

            // Log values
            output.WriteLine($"Inputs: flowStatus={flowStatus}, machineType={machineType}, cycleStatus={cycleStatus}, partStatus=Ok, machineId=100, nextMachineId=10000");
            output.WriteLine($"Result: {result}, Expected: {expectedResult}");

            // Assert
            result.ShouldBe(expectedResult);

            output.WriteLine("Test completed: Validate_ShouldReturnWorkFlowNotValid");
        }

        /// <summary>
        /// Executes Validate_ShouldSetResultToInvalid_WhenNoCaseMatches operation.
        /// </summary>

        [Fact]
        public void Validate_ShouldSetResultToInvalid_WhenNoCaseMatches()
        {
            // Arrange
            output.WriteLine("Test started: Validate_ShouldSetResultToInvalid_WhenNoCaseMatches");

            // Act
            var result = _service.Validate(Domain.Enum.FlowStatus.Created, MachineType.Printer, CycleStatus.FinishedOk, PartStatus.Ok, 1, 1);

            // Log values
            output.WriteLine($"Inputs: flowStatus=Created, machineType=Printer, cycleStatus=FinishedOk, partStatus=Ok, machineId=1, nextMachineId=100");
            output.WriteLine($"Result: {result}");

            // Assert
            result.ShouldBe(ResultValidation.Invalid);
            result.ShouldNotBe(ResultValidation.Valid);

            output.WriteLine("Test completed: Validate_ShouldSetResultToInvalid");
        }

        /// <summary>
        /// Executes PartNotValidTestData operation.
        /// </summary>
        /// <returns>The result of PartNotValidTestData.</returns>

        public static IEnumerable<object[]> PartNotValidTestData()
        {
            yield return new object[] { PartStatus.NOk, CycleStatus.FinishedOk };
            yield return new object[] { PartStatus.NOk, CycleStatus.Started };
            // Add more cases if needed
        }
    }
}