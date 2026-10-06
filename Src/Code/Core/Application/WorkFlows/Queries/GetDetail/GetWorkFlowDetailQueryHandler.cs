// <copyright file="GetWorkFlowDetailQueryHandler.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.WorkFlows.Queries.GetDetail;

/// <summary>
/// Represents the GetWorkFlowDetailQueryHandler.
/// </summary>
public class GetWorkFlowDetailQueryHandler : IMonitorRequestHandler<GetWorkFlowDetailQuery, List<WorkFlowDetailVm>>
{
    private readonly IRepository<Product> productRepository;

    // #95 Phase 2 Slice D (policy C, writes-only): this query only LISTS workflow edges, so it consumes the
    // read-only surface. WorkFlow writes are aggregate-scoped through IAggregateRepository<ProductRouting>.
    private readonly IReadOnlyRepository<WorkFlow> workFlowRepository;
    private readonly ILogger<GetWorkFlowDetailQueryHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetWorkFlowDetailQueryHandler"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public GetWorkFlowDetailQueryHandler(
        IRepository<Product> productRepository,
        IReadOnlyRepository<WorkFlow> workFlowRepository,
        ILogger<GetWorkFlowDetailQueryHandler> logger)
    {
        this.productRepository = productRepository;
        this.workFlowRepository = workFlowRepository;
        this.logger = logger;
    }

    /// <summary>
    /// Executes ProcessAsync operation.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <returns>The result of ProcessAsync.</returns>
    public async Task<Result<List<WorkFlowDetailVm>>> ProcessAsync(GetWorkFlowDetailQuery request, CancellationToken cancellationToken)
    {
        // First get the product by part number through a keyed specification (issue #118).
        // Collation fix: the spec keeps the SQL-side `==` filter (a tiny collation-insensitive superset
        // on real SQL — Modern_Spanish_CI_AS matches case/trailing-space insensitively — and the exact
        // matches on InMemory); ORDINAL equality is re-applied client-side so the replaced in-memory
        // FirstOrDefault semantics are preserved byte-for-byte.
        var productSpecification = new Specification<Product>(m => m.PartNumber == request.NoParte);
        var productResult = await this.productRepository.ListAsync(productSpecification, cancellationToken).ConfigureAwait(false);
        if (productResult.IsFailure)
        {
            this.logger.LogError("Failed to retrieve Products: {Errors}", string.Join(", ", productResult.Errors ?? []));
            return Result<List<WorkFlowDetailVm>>.WithFailure(productResult.Errors);
        }

        var product = productResult.Value?.FirstOrDefault(m => string.Equals(m.PartNumber, request.NoParte, StringComparison.Ordinal));
        if (product == null)
        {
            this.logger.LogError("Product not found with PartNumber: {PartNumber}", request.NoParte);
            return Result<List<WorkFlowDetailVm>>.WithFailure($"Product with PartNumber {request.NoParte} not found");
        }

        // Get the WorkFlows for the product through a filtered specification (issue #118).
        var productId = product.ProductId.Value;
        var workFlowSpecification = new Specification<WorkFlow>(f => f.ProductId == productId);
        var workFlowsResult = await this.workFlowRepository.ListAsync(workFlowSpecification, cancellationToken).ConfigureAwait(false);
        if (!workFlowsResult.IsSuccess)
        {
            this.logger.LogError("Failed to retrieve WorkFlows: {Errors}", string.Join(", ", workFlowsResult.Errors ?? []));
            return Result<List<WorkFlowDetailVm>>.WithFailure(workFlowsResult.Errors);
        }

        var workFlows = workFlowsResult.Value?.ToList() ?? [];
        var vmResult = WorkFlowDetailVm.ToDtoList(workFlows);
        if (vmResult.IsFailure || vmResult.Value is null)
        {
            return Result<List<WorkFlowDetailVm>>.WithFailure(vmResult.Errors);
        }

        return Result<List<WorkFlowDetailVm>>.Success(vmResult.Value.ToList());
    }
}