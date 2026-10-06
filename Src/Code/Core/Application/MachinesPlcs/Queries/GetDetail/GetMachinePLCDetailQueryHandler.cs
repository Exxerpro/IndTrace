// <copyright file="GetMachinePLCDetailQueryHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.MachinesPlcs.Queries.GetDetail;

/// <summary>
/// Handles the retrieval of detailed machine-PLC relationship information.
/// Provides comprehensive data about specific machine-PLC associations and their configurations.
/// </summary>
public class GetMachinePlcDetailQueryHandler : IMonitorRequestHandler<GetMachinePlcDetailQuery, MachinePlcDetailVm>
{
    // The repository signals "FirstOrDefault matched nothing" as a failure carrying this sentinel
    // (Repository.cs / ReadOnlyRepository.cs). It is the ONE failure that means "empty", not
    // "infrastructure error" — any other failure is a genuine repository error and is propagated.
    private const string RepositoryNotFoundSentinel = RepositoryFailures.NotFoundSentinel;

    private readonly IReadOnlyRepository<MachinePlc> repository;
    private readonly ILogger<GetMachinePlcDetailQueryHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetMachinePlcDetailQueryHandler"/> class.
    /// </summary>
    /// <param name="repository">Read-only repository for accessing machine-PLC relationship data (#95 Slice C: MachinePlc is a Machine aggregate member; reads stay free).</param>
    /// <param name="logger">Logger for recording operations and errors.</param>
    public GetMachinePlcDetailQueryHandler(IReadOnlyRepository<MachinePlc> repository, ILogger<GetMachinePlcDetailQueryHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    /// <summary>
    /// Processes the machine-PLC detail query and returns comprehensive relationship information.
    /// </summary>
    /// <param name="request">The query containing the machine and PLC IDs to retrieve details for.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A result containing the detailed machine-PLC relationship view model.</returns>
    public async Task<Result<MachinePlcDetailVm>> ProcessAsync(GetMachinePlcDetailQuery request, CancellationToken cancellationToken)
    {
        var specification = new Specification<MachinePlc>(p => p.PlcId == request.PlcId && p.MachineId == new MachineId(request.MachineId));
        var getResult = await this.repository.FirstOrDefaultAsync(specification, cancellationToken).ConfigureAwait(false);
        if (getResult.IsFailure && !IsNotFound(getResult.Errors))
        {
            this.logger.LogError("Failed to retrieve MachinePlcs: {Errors}", string.Join(", ", getResult.Errors ?? []));
            return Result<MachinePlcDetailVm>.WithFailure(getResult.Errors);
        }

        var machinePlc = getResult.Value;
        if (machinePlc == null)
        {
            this.logger.LogError("MachinePlc not found: PlcId={PlcId}, MachineId={MachineId}", request.PlcId, request.MachineId);
            return Result<MachinePlcDetailVm>.WithFailure($"MachinePlc with PlcId {request.PlcId} and MachineId {request.MachineId} not found");
        }

        var vm = MachinePlcDetailVm.ToDto(machinePlc);
        if (!vm.IsSuccess)
        {
            return Result<MachinePlcDetailVm>.WithFailure(vm.Errors);
        }

        if (vm.Value is null)
        {
            return Result<MachinePlcDetailVm>.WithFailure("DTO conversion returned null value");
        }

        return Result<MachinePlcDetailVm>.Success(vm.Value);
    }

    /// <summary>
    /// Distinguishes the repository's "no rows matched" sentinel failure from a genuine infrastructure failure.
    /// </summary>
    /// <param name="errors">The errors carried by the failed repository result.</param>
    /// <returns>True when the failure only signals that no entity matched the specification.</returns>
    private static bool IsNotFound(IEnumerable<string>? errors) =>
        errors is not null && errors.Any(e => e is not null && e.Contains(RepositoryNotFoundSentinel, StringComparison.Ordinal));
}