// <copyright file="BarCodeValidationService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities.BarCodes
{
    using IndTrace.Domain.Enum;
    using IndTrace.Domain.Models;
    using IndTrace.Domain.Routing;
    using IndTrace.Domain.ValueObjects;

    /// <summary>
    /// Provides validation logic for barcode operations based on flow status, machine type, and cycle status.
    /// </summary>
    /// <remarks>
    /// Issue #115 finding 6: this service is registered as a Singleton on the PLC path, so it is shared by
    /// concurrently validating parts. It MUST stay stateless — every output flows through the return value;
    /// never add per-call instance state.
    /// </remarks>
    public class BarCodeValidationService : IBarCodeValidationService
    {
        /// <summary>
        /// Validates the barcode operation based on the provided statuses and machine information.
        /// </summary>
        /// <param name="flowStatus">The flow status of the part.</param>
        /// <param name="machineType">The type of machine involved.</param>
        /// <param name="cycleStatus">The status of the cycle.</param>
        /// <param name="partStatus">The status of the part.</param>
        /// <param name="machineId">The current machine ID.</param>
        /// <param name="nextMachineId">The expected next machine ID.</param>
        /// <param name="legalArrivalMachines">
        /// E6-1 (#56): the context-derived legal-arrival set. A reported arrival is legal iff the requesting
        /// <paramref name="machineId"/> is a MEMBER of this set. On linear / stay / degraded data it is the
        /// singleton <c>{ nextMachineId }</c>, so membership is byte-identical to the legacy
        /// <c>nextMachineId == machineId</c>; the set expands only on a genuine multi-successor diverter. When
        /// left <c>default</c> (empty set) the gate falls back to the exact legacy equality.
        /// </param>
        /// <returns>A <see cref="ResultValidation"/> indicating the validation outcome.</returns>
        public ResultValidation Validate(FlowStatus flowStatus, MachineType machineType, CycleStatus cycleStatus,
            PartStatus partStatus, int machineId, int nextMachineId, LegalNextMachines legalArrivalMachines = default)
        {
            ArgumentNullException.ThrowIfNull(flowStatus);
            ArgumentNullException.ThrowIfNull(machineType);
            ArgumentNullException.ThrowIfNull(cycleStatus);

            if (!Equals(partStatus, PartStatus.Ok) && !Equals(cycleStatus, CycleStatus.FinishedNok))
            {
                return ResultValidation.PartNotValid;
            }

            // E6-1 (#56): the arrival gate. A reported arrival is legal iff the requesting machineId is a MEMBER of
            // the context-derived legal-arrival set — not equal to a single computed next. On linear / stay /
            // degraded data that set is the singleton { nextMachineId }, so membership is byte-identical to the
            // legacy `nextMachineId != machineId`; the set expands only on a genuine diverter advance. When no set
            // was supplied (empty default) fall back to the exact legacy equality so nothing changes.
            var arrivalIsLegal = legalArrivalMachines.Count > 0
                ? legalArrivalMachines.Contains(new MachineId(machineId))
                : nextMachineId == machineId;
            if (!arrivalIsLegal)
            {
                return ResultValidation.DestinationNotValid;
            }

            if (Equals(flowStatus, FlowStatus.Rejected))
            {
                return ResultValidation.PartRejected;
            }

            var key = (FlowStatus: flowStatus, MachineType: machineType, CycleStatus: cycleStatus);

            return key switch
            {
                // Valid Cases
                // When the flow is created, the machine is a printer, and the cycle has started.
                var (fs, mt, cs) when Equals(fs, FlowStatus.Created) && Equals(mt, MachineType.Printer) &&
                                      Equals(cs, CycleStatus.Started) => ResultValidation.Valid,

                // When the flow is created, the machine is an initial printer, and the cycle has started.
                var (fs, mt, cs) when Equals(fs, FlowStatus.Created) && Equals(mt, MachineType.InitialPrinter) &&
                                      Equals(cs, CycleStatus.Started) => ResultValidation.Valid,

                // When the flow is created and the machine is set to initial.
                var (fs, mt, _) when Equals(fs, FlowStatus.Created) && Equals(mt, MachineType.Initial)
                                            => ResultValidation.Valid,

                // When the flow is created and the machine is set to initial.
                var (fs, mt, _) when Equals(fs, FlowStatus.InProcess) && Equals(mt, MachineType.InitialPrinter)
                                            => ResultValidation.Valid,

                // When the flow is in process and the machine is in the process state.
                var (fs, mt, _) when Equals(fs, FlowStatus.InProcess) && Equals(mt, MachineType.Process)
                                            => ResultValidation.Valid,

                // When the flow is in process, the machine is in the final state, and the cycle hasn't started.
                var (fs, mt, cs) when Equals(fs, FlowStatus.InProcess) && Equals(mt, MachineType.Final) &&
                                      Equals(cs, CycleStatus.NotStarted) => ResultValidation.Valid,

                // When the flow is in process, the machine is in the final state, and the cycle finished successfully.
                var (fs, mt, cs) when Equals(fs, FlowStatus.InProcess) && Equals(mt, MachineType.Final) &&
                                      Equals(cs, CycleStatus.FinishedOk) => ResultValidation.Valid,

                // When the flow is in process, the machine is in the final state, and the cycle finished nOK.
                var (fs, mt, cs) when Equals(fs, FlowStatus.InProcess) && Equals(mt, MachineType.Final) &&
                                      Equals(cs, CycleStatus.FinishedNok) => ResultValidation.Valid,

                // When the flow is in process, the machine is a dashboard, and the cycle hasn't started.
                var (fs, mt, cs) when Equals(fs, FlowStatus.InProcess) && Equals(mt, MachineType.DashBoard) &&
                                      Equals(cs, CycleStatus.NotStarted) => ResultValidation.Valid,

                // When the flow is in process, the machine is a dashboard, and the cycle finished successfully.
                var (fs, mt, cs) when Equals(fs, FlowStatus.InProcess) && Equals(mt, MachineType.DashBoard) &&
                                      Equals(cs, CycleStatus.FinishedOk) => ResultValidation.Valid,

                // When the flow is in process, the machine is a printer, and the cycle has started.
                var (fs, mt, cs) when Equals(fs, FlowStatus.InProcess) && Equals(mt, MachineType.Printer) &&
                                      Equals(cs, CycleStatus.Started) => ResultValidation.Valid,

                // When the flow is in process, the machine is a dashboard, and the cycle has started.
                var (fs, mt, cs) when Equals(fs, FlowStatus.InProcess) && Equals(mt, MachineType.Final) &&
                                      Equals(cs, CycleStatus.Started) => ResultValidation.Valid,

                // Invalid Cases
                // When the flow is in process and the machine is in the final state (regardless of cycle status).
                var (fs, mt, _) when Equals(fs, FlowStatus.InProcess) && Equals(mt, MachineType.Final)
                                        => ResultValidation.WorkFlowNotValid,

                // When the flow has finished, the machine is a dashboard, and the cycle finished successfully.
                var (fs, mt, cs) when Equals(fs, FlowStatus.Finished) && Equals(mt, MachineType.DashBoard) &&
                                      Equals(cs, CycleStatus.FinishedOk) => ResultValidation.WorkFlowNotValid,

                // Default case when none of the above conditions are met.
                _ => ResultValidation.Invalid,
            };
        }
    }
}