// <copyright file="BarCodeValidationServiceStatelessnessTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.ValidationTests
{
    /// <summary>
    /// Regression tests for issue #115 finding 6: <see cref="BarCodeValidationService"/> is registered as a
    /// Singleton on the PLC path, so two parts validating simultaneously share ONE instance. The service
    /// previously kept a mutable <c>Result</c> property written on every call — interleaved validations
    /// overwrote each other's failure (cross-part corruption). The service is now stateless; these tests pin
    /// that every observable output flows through the return value and is per-call independent.
    /// </summary>
    public class BarCodeValidationServiceStatelessnessTests
    {
        /// <summary>
        /// Interleaved validations of two different parts on ONE service instance: the first part's
        /// verdict must be unaffected by the second part's call, and vice versa.
        /// </summary>
        [Fact]
        public void Validate_InterleavedCallsOnOneInstance_VerdictsAreIndependent()
        {
            // Arrange — ONE instance, as the Singleton registration produces.
            var service = new BarCodeValidationService();

            // Act — part A reports an illegal arrival (machine 1, expected next 2); part B then validates
            // on the SAME instance with a not-OK part before part A's handler consumes its verdict.
            var firstVerdict = service.Validate(
                Domain.Enum.FlowStatus.InProcess, MachineType.Process, CycleStatus.Started,
                PartStatus.Ok, machineId: 1, nextMachineId: 2);
            var secondVerdict = service.Validate(
                Domain.Enum.FlowStatus.InProcess, MachineType.Process, CycleStatus.Started,
                PartStatus.NOk, machineId: 3, nextMachineId: 3);

            // Assert — each part observes its own verdict.
            firstVerdict.ShouldBe(ResultValidation.DestinationNotValid);
            secondVerdict.ShouldBe(ResultValidation.PartNotValid);
        }

        /// <summary>
        /// Many parts validating in parallel on ONE service instance each get the verdict their own
        /// inputs dictate — no cross-part interference through the shared instance.
        /// </summary>
        [Fact]
        public void Validate_ParallelCallsOnOneInstance_EachPartGetsItsOwnVerdict()
        {
            // Arrange — ONE instance and three input shapes with three distinct expected verdicts.
            var service = new BarCodeValidationService();
            var expected = new ResultValidation[300];
            var actual = new ResultValidation[300];

            // Act — concurrent PLC traffic: valid / illegal-arrival / part-not-valid interleaved.
            Parallel.For(0, 300, i =>
            {
                switch (i % 3)
                {
                    case 0:
                        expected[i] = ResultValidation.Valid;
                        actual[i] = service.Validate(
                            Domain.Enum.FlowStatus.InProcess, MachineType.Process, CycleStatus.Started,
                            PartStatus.Ok, machineId: 5, nextMachineId: 5);
                        break;
                    case 1:
                        expected[i] = ResultValidation.DestinationNotValid;
                        actual[i] = service.Validate(
                            Domain.Enum.FlowStatus.InProcess, MachineType.Process, CycleStatus.Started,
                            PartStatus.Ok, machineId: 1, nextMachineId: 2);
                        break;
                    default:
                        expected[i] = ResultValidation.PartNotValid;
                        actual[i] = service.Validate(
                            Domain.Enum.FlowStatus.InProcess, MachineType.Process, CycleStatus.Started,
                            PartStatus.NOk, machineId: 3, nextMachineId: 3);
                        break;
                }
            });

            // Assert — every call returned exactly the verdict its own inputs dictate.
            for (var i = 0; i < actual.Length; i++)
            {
                actual[i].ShouldBe(expected[i]);
            }
        }
    }
}
