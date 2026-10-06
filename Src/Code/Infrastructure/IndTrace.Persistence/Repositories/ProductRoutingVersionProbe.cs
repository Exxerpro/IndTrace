// <copyright file="ProductRoutingVersionProbe.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Buffers.Binary;
using IndTrace.Application.BarCodes.Services;
using IndTrace.Domain.Entities;
using IndTrace.Persistence.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IndTrace.Persistence.Repositories;

/// <summary>
/// #224: EF-backed <see cref="IProductRoutingVersionProbe"/>. Operation-scoped like
/// <see cref="ProductRoutingRepository"/> — each call obtains one pooled <see cref="IIndTraceDbContext"/> from
/// the factory, uses it, and disposes it inside the call, so it is safe to invoke from any lifetime.
/// </summary>
/// <remarks>
/// <para>
/// The probe reads ONLY the <c>RowVersion</c> column of the product's <c>RoutingNodes</c> and <c>WorkFlows</c>
/// rows and computes the maximum CLIENT-SIDE (as big-endian <see cref="ulong"/>s). This is deliberate:
/// a server-side <c>Max</c>/<c>OrderBy</c> over <c>byte[]</c> does not translate on every provider (the EF
/// InMemory provider used by the Aggregation test bed cannot), while a two-column projection works everywhere.
/// A product's routing is tens of rows, so the client-side fold is trivial next to the reads the probe guards.
/// </para>
/// <para>
/// On providers without real rowversions (EF InMemory leaves the arrays empty) every row folds to 0, so the
/// probe returns a constant 0 — cached entries stamped 0 always match, preserving the pre-#224 single-process
/// cache behaviour in those test beds.
/// </para>
/// </remarks>
public sealed class ProductRoutingVersionProbe : IProductRoutingVersionProbe
{
    private const string ContextInactive = "Database context is not active.";

    private readonly IIndTraceDbContextFactory contextFactory;
    private readonly ILogger<ProductRoutingVersionProbe> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductRoutingVersionProbe"/> class.
    /// </summary>
    /// <param name="contextFactory">The pooled database context factory (one context per probe).</param>
    /// <param name="logger">The logger.</param>
    public ProductRoutingVersionProbe(IIndTraceDbContextFactory contextFactory, ILogger<ProductRoutingVersionProbe> logger)
    {
        this.contextFactory = contextFactory;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ulong>> GetVersionAsync(int productId, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<ulong>.WithFailure("Operation was canceled.");
        }

        if (this.contextFactory is null)
        {
            return Result<ulong>.WithFailure("The database context factory is not available.");
        }

        try
        {
            await using var ctx = await this.contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
            if (ctx is null)
            {
                this.logger.LogError("ProductRoutingVersionProbe: database context is null.");
                return Result<ulong>.WithFailure(ContextInactive);
            }

            // Project ONLY the RowVersion column (provider-safe on SQL Server AND EF InMemory); fold client-side.
            var nodeVersions = await ctx.Set<RoutingNodeRow>()
                .Where(n => n.ProductId == productId)
                .Select(n => n.RowVersion)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var edgeVersions = await ctx.Set<WorkFlow>()
                .Where(e => e.ProductId == productId)
                .Select(e => e.RowVersion)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var version = 0UL;
            foreach (var rowVersion in nodeVersions)
            {
                version = Math.Max(version, ToVersion(rowVersion));
            }

            foreach (var rowVersion in edgeVersions)
            {
                version = Math.Max(version, ToVersion(rowVersion));
            }

            return Result<ulong>.Success(version);
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex, "ProductRoutingVersionProbe: error probing the routing version for product {ProductId}.", productId);
            return Result<ulong>.WithFailure(ex.Message);
        }
    }

    /// <summary>
    /// Interprets a SQL Server <c>rowversion</c> byte array as a big-endian <see cref="ulong"/> — the natural
    /// reading of the engine's monotonically increasing 8-byte stamp. A <see langword="null"/>, empty or
    /// short (fewer than 8 bytes) array — e.g. an unmaterialised row or a provider without real rowversions —
    /// folds to 0; a longer array reads its first 8 bytes.
    /// </summary>
    /// <param name="rowVersion">The raw rowversion bytes, or <see langword="null"/>.</param>
    /// <returns>The big-endian <see cref="ulong"/> value, or 0 when the array carries no full stamp.</returns>
    public static ulong ToVersion(byte[]? rowVersion) =>
        rowVersion is { Length: >= 8 }
            ? BinaryPrimitives.ReadUInt64BigEndian(rowVersion.AsSpan(0, 8))
            : 0UL;
}
