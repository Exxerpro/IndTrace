// <copyright file="IBarCodeValidationService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities.BarCodes;

using IndTrace.Domain.Enum;
using IndTrace.Domain.Models;
using IndTrace.Domain.Routing;

/// <summary>
/// Defines the contract for barcode validation services.
/// </summary>
/// <remarks>
/// Implementations are shared across concurrently validating parts (Singleton on the PLC path), so they
/// MUST be stateless: every validation output flows through the <see cref="Validate"/> return value.
/// </remarks>
public interface IBarCodeValidationService
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
    /// E6-1 (#56): the context-derived legal-arrival set the arrival gate validates membership against. On
    /// linear / stay / degraded data it is the singleton <c>{ nextMachineId }</c>, so membership is byte-identical
    /// to the legacy <c>nextMachineId == machineId</c> equality. When left <c>default</c> (empty set) the gate
    /// falls back to the exact legacy equality, keeping non-production callers byte-identical.
    /// </param>
    /// <returns>A <see cref="ResultValidation"/> indicating the validation outcome.</returns>
    ResultValidation Validate(FlowStatus flowStatus, MachineType machineType, CycleStatus cycleStatus,
        PartStatus partStatus, int machineId, int nextMachineId, LegalNextMachines legalArrivalMachines = default);
}