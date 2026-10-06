// <copyright file="DistinctRegisterService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Registers.Services;

/// <summary>
/// Represents the DistinctRegisterService.
/// </summary>
public class DistinctRegisterService(
    IRepository<DistinctRegister> distinctRegisterRepository,
    IReadOnlyRepository<Register> registerRepository) : IDistinctRegisterService
{
    /// <inheritdoc/>
    public async Task<Result> UpdateDistinctRegistersAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result.WithFailure(["Operation was canceled."]);
        }

        var targetResult = await this.GetDistinctRegistersAsync(cancellationToken).ConfigureAwait(false);
        if (targetResult.IsFailure || targetResult.Value is null)
        {
            return Result.WithFailure(targetResult.Errors ?? ["Failed to compute the distinct register catalog."]);
        }

        var currentResult = await distinctRegisterRepository.ListAsync(cancellationToken).ConfigureAwait(false);
        if (currentResult.IsFailure || currentResult.Value is null)
        {
            return Result.WithFailure(currentResult.Errors ?? ["Failed to load the persisted distinct register catalog."]);
        }

        // #119 (F4): incremental diff-sync on the value key {Name, VariableId, MachineId}. The
        // pre-fix clear-then-rebuild issued one auto-committing DELETE per row followed by one
        // auto-committing INSERT per row, so concurrent Metrics readers observed an EMPTY or
        // partial catalog mid-rebuild. Diffing touches only rows that actually changed, so readers
        // always see a valid (at worst slightly stale) catalog without needing a transaction seam.
        var target = targetResult.Value.DistinctBy(GetKey).ToList();
        var targetKeys = target.Select(GetKey).ToHashSet();
        var current = currentResult.Value.ToList();
        var currentKeys = current.Select(GetKey).ToHashSet();

        // #119 (F4): every write Result is honored; failures are collected per key and reported.
        var errors = new List<string>();

        foreach (var stale in current.Where(row => !targetKeys.Contains(GetKey(row))))
        {
            var deleteResult = await distinctRegisterRepository.DeleteAsync(stale, cancellationToken).ConfigureAwait(false);
            if (deleteResult.IsFailure)
            {
                errors.Add($"Failed to delete distinct register {Describe(stale)}: {string.Join("; ", deleteResult.Errors ?? [])}");
            }
        }

        foreach (var missing in target.Where(row => !currentKeys.Contains(GetKey(row))))
        {
            var distinctRegister = new DistinctRegister
            {
                Name = missing.Name,
                VariableId = missing.VariableId,
                MachineId = missing.MachineId,
            };

            var addResult = await distinctRegisterRepository.AddAsync(distinctRegister, cancellationToken).ConfigureAwait(false);
            if (addResult.IsFailure)
            {
                errors.Add($"Failed to add distinct register {Describe(missing)}: {string.Join("; ", addResult.Errors ?? [])}");
            }
        }

        // Each AddAsync/DeleteAsync auto-commits (repositories are stateless-per-operation and
        // CommitAsync is a documented no-op), so no explicit commit is required here.
        return errors.Count > 0 ? Result.WithFailure(errors) : Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result<IEnumerable<DistinctRegister>>> GetDistinctRegistersAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<IEnumerable<DistinctRegister>>.WithFailure(["Operation was canceled."]);
        }

        var spec = new Specification<Register>(r => true)
            .ApplyNoTracking();

        var distinctValues = await registerRepository.AsQueryableAsync(spec, cancellationToken).ConfigureAwait(false);
        if (distinctValues.IsFailure || distinctValues.Value is null)
        {
            return Result<IEnumerable<DistinctRegister>>.WithFailure(distinctValues.Errors ?? ["Failed to lease a Registers queryable."]);
        }

        // #117 (F1): the lease owns the pooled context backing the queryable; hold it until ToList()
        // materializes below, then return the context to the pool.
        await using var lease = distinctValues.Value;

        // AsQueryableAsync defers database access to enumeration, so a dead database surfaces here
        // as an exception rather than a failed Result at the repository boundary. #119 (F4): contain
        // it into a FAILURE Result — pre-fix it degraded to a silent empty catalog, which the
        // Metrics page could not distinguish from "no registers exist".
        try
        {
            var distinctRegister = lease.Query.Select(r => new DistinctRegister
            {
                Name = r.Name,
                VariableId = r.VariableId,
                MachineId = r.MachineId,
            })
                .Distinct();

            return Result<IEnumerable<DistinctRegister>>.Success(distinctRegister.ToList());
        }
        catch (OperationCanceledException)
        {
            return Result<IEnumerable<DistinctRegister>>.WithFailure(["Operation was canceled."]);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<DistinctRegister>>.WithFailure([$"Failed to enumerate the distinct register catalog: {ex.Message}"]);
        }
    }

    /// <summary>
    /// Value-based composite key for a catalog row: {Name, VariableId, MachineId}.
    /// </summary>
    /// <param name="row">The catalog row.</param>
    /// <returns>The composite key tuple.</returns>
    private static (string Name, int VariableId, int MachineId) GetKey(DistinctRegister row) =>
        (row.Name, row.VariableId, row.MachineId);

    /// <summary>
    /// Human-readable key rendering used in write-failure messages.
    /// </summary>
    /// <param name="row">The catalog row.</param>
    /// <returns>The rendered key.</returns>
    private static string Describe(DistinctRegister row) =>
        $"{{Name: {row.Name}, VariableId: {row.VariableId}, MachineId: {row.MachineId}}}";
}
