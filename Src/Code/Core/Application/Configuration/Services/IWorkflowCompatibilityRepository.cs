// <copyright file="IWorkflowCompatibilityRepository.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Configuration.Services;

public interface IWorkflowCompatibilityRepository
{
    IReadOnlyList<MachineProductMap> GetAll();

    IEnumerable<MachineProductMap> GetByCustomer(int customerId);

    IEnumerable<MachineProductMap> GetByProduct(int productId);

    IEnumerable<MachineProductMap> GetByMachine(int machineId);

    bool IsCompatible(int machineId, int productId);

    Task RefreshAsync();
}