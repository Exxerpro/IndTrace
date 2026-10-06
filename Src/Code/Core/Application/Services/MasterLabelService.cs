// <copyright file="MasterLabelService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Services;

/// <summary>
/// Service implementation for master label operations.
/// </summary>
public class MasterLabelService : IMasterLabelService
{
    private readonly IReadOnlyRepository<MasterLabel> masterLabelRepository;

    // [Fix]
    // CLAUDE
    // Date: 25/08/2025
    // Reason: [DI SIMPLIFICATION] - Remove HybridCache dependency as DI handles caching at repository level
    public MasterLabelService(IReadOnlyRepository<MasterLabel> masterLabelRepository)
    {
        this.masterLabelRepository = masterLabelRepository;
    }

    /// <summary>
    /// Gets a list of master label codes by part number.
    /// Caching is handled at the repository level by DI.
    /// </summary>
    /// <param name="partNumber">The part number to search for.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A list of master label codes, or a failure result if not found.</returns>
    public async Task<Result<List<string>>> GetMasterLabelByPartNumberAsync(string partNumber, CancellationToken cancellationToken)
    {
        try
        {
            // [Fix]
            // CLAUDE
            // Date: 25/08/2025
            // Reason: [DI SIMPLIFICATION] - Direct repository usage, caching handled by DI at repository level
            var spec = new Specification<MasterLabel>(b => b.MasterLabelCode.Contains(partNumber));
            var masterLabels = await this.masterLabelRepository.ListAsync(spec, cancellationToken).ConfigureAwait(false);

            if (masterLabels.IsFailure || masterLabels.Value is null || !masterLabels.Value.Any())
            {
                return Result<List<string>>.WithFailure("No master labels found matching the criteria");
            }

            var result = masterLabels.Value.Select(ml => ml.MasterLabelCode).ToList();
            return Result<List<string>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<List<string>>.WithFailure($"Error retrieving master labels: {ex.Message}");
        }
    }
}