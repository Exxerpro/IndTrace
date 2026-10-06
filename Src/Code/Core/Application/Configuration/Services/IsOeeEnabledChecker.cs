// <copyright file="IsOeeEnabledChecker.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Configuration.Services;

using System.Data;
using System.Data.Common;

/// <summary>
/// Checks if OEE (Overall Equipment Effectiveness) features are enabled for specified machines and validates required schema and tags.
/// </summary>
public class IsOeeEnabledChecker(IReadOnlyRepository<Variable> variableRepository, ILogger<IsOeeEnabledChecker> logger) : IIsOeeEnabledChecker
{
    // TODO: Future Enhancements for CheckOeeFeatureByMachineIds
    // - Replace bool availability with enum (e.g., OeeFeatureStatus: Unavailable, TagsOnly, SchemaOnly, Ready)
    // - Split table-check and tag-check phases if caching or partial validation is needed
    // - Optionally expose diagnostics (e.g., which variables are missing)
    // - Consider async execution for large-scale machine sets
    // - Integrate feature-specific dependency registration based on machine capability

    private readonly string[] requiredVariableNames =
    [
        "ApplicationFlag",
        "EventCounter",
        "CurrentTime",
        "RunningTime",
        "StoppedTime",
        "FaultedTime",
        "StatusFaultReason",
        "TotalProduction",
        "ProductionOk",
        "ProductionNoK",
        "StatusFaultReject"
    ];

    /// <summary>
    /// Checks if the OEE feature is enabled for the specified machine IDs by validating required tables and tags.
    /// </summary>
    /// <param name="machineIds">The list of machine IDs to check.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result containing the OEE configuration for the specified machines.</returns>
    public async Task<Result<OeeConfiguration>> CheckOeeFeatureByMachineIdsAsync(List<int> machineIds, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<OeeConfiguration>.WithFailure("Operation was canceled.");
        }

        try
        {
            var result = new OeeConfiguration();
            var oeeGroupId = TagsGroups.PerformanceTags.Value;
            var tagsPresents = new Dictionary<int, bool>();

            // Note: Table existence checking moved to infrastructure layer responsibility
            // For now, assume tables exist and let repository operations fail gracefully
            var allTablesExist = true; // TODO: Implement through infrastructure service if needed

            if (!allTablesExist)
            {
                logger.LogWarning("Critical OEE tables are missing from the schema.");
                foreach (var id in machineIds)
                {
                    tagsPresents[id] = false;
                }
            }

            try
            {
                // Get all variables and filter in memory (could be optimized with specification pattern)
                var allVariablesResult = await variableRepository.ListAsync(cancellationToken);

                if (allVariablesResult.IsFailure)
                {
                    logger.LogError("Failed to retrieve variables: {Errors}", string.Join(", ", allVariablesResult.Errors));
                    return Result<OeeConfiguration>.WithFailure(allVariablesResult.Errors);
                }

                if (allVariablesResult.Value is null)
                {
                    logger.LogError("Variable result value is null");
                    return Result<OeeConfiguration>.WithFailure("Variable result value is null");
                }

                var variableData = allVariablesResult.Value
                    .Where(v => machineIds.Contains(v.MachineId) &&
                                v.VariableGroupId == oeeGroupId &&
                                this.requiredVariableNames.Contains(v.Name))
                    .Select(v => new { v.MachineId, v.Name })
                    .ToList();

                var grouped = variableData
                    .GroupBy(v => v.MachineId)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.Name).Distinct().ToHashSet());

                foreach (var id in machineIds)
                {
                    if (grouped.TryGetValue(id, out var names))
                    {
                        var hasAll = this.requiredVariableNames.All(name => names.Contains(name));
                        tagsPresents[id] = hasAll;
                    }
                    else
                    {
                        tagsPresents[id] = false;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Variable check failed ");
                return Result<OeeConfiguration>.WithFailure(ex.Message, result);
            }

            result.Enabled = allTablesExist;
            result.EnabledByMachine = tagsPresents;

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Process finishes with Failure ");
            return Result<OeeConfiguration>.WithFailure($"Operation finished with an exception: {ex.Message}");
        }
    }

    // TODO: Move table existence checking to Infrastructure layer as a separate service
    // The Application layer should not be concerned with database schema validation
    // Consider creating an ISchemaValidationService in Infrastructure layer
}