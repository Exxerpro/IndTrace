// <copyright file="IWorkflowOrchestrator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Routing.Authoring;

namespace IndTrace.Application.Products.Services.Interfaces;

/// <summary>
/// Workflow creation and relationship orchestration service.
/// Application service - manages workflow generation from machine assignments and persistence.
/// </summary>
public interface IWorkflowOrchestrator
{
    /// <summary>
    /// Authors and persists a product's routing from the node+edge <see cref="AuthoringRoute"/> (E11.4-2) — the
    /// order-preserving, fork-capable authoring path that replaces the flat <c>IEnumerable&lt;int&gt;</c> +
    /// ascending-machine-id guess. A DIVERTER (a node with more than one outgoing edge) is representable, and
    /// authored order/edge multiplicity survive to storage. Runs the same pre-write gate (authoring config gate +
    /// F3 magic-0 sentinel + #58 rule guard) and persists atomically through the <c>ProductRouting</c> aggregate.
    /// </summary>
    /// <param name="product">The product being authored.</param>
    /// <param name="route">The authored node+edge route (its <c>ProductId</c> must match the product).</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>The persisted clean interior edges on success, or a failure carrying the reason.</returns>
    Task<Result<IEnumerable<WorkFlow>>> CreateAndPersistWorkflowsAsync(Product product, AuthoringRoute route, CancellationToken cancellationToken);

    /// <summary>
    /// Replaces a product's entire routing (whole-route replace) from the node+edge <see cref="AuthoringRoute"/>
    /// (E11.4-2). Validate-before-destroy holds identically: the authored shape is fully validated (fork-aware
    /// gate) BEFORE anything is deleted, so an invalid edit never destroys the product's existing routing.
    /// </summary>
    /// <param name="product">The product whose routing is being edited.</param>
    /// <param name="route">The new authored node+edge route (its <c>ProductId</c> must match the product).</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>The persisted clean interior edges on success, or a failure carrying the reason.</returns>
    Task<Result<IEnumerable<WorkFlow>>> ReplaceProductRoutingAsync(Product product, AuthoringRoute route, CancellationToken cancellationToken);

    /// <summary>
    /// Compensating delete (F5, #113) for routing rows authored by
    /// <see cref="CreateAndPersistWorkflowsAsync(Product, AuthoringRoute, CancellationToken)"/> when a LATER
    /// product-creation step failed: removes ALL <c>RoutingNodeRow</c> + <c>WorkFlow</c> edge rows persisted for
    /// the product, atomically, through the <c>ProductRouting</c> aggregate. Must run BEFORE the compensating
    /// product delete (the routing rows reference the product), and is a no-op success when the product has no
    /// routing rows. NOT gated behind the authoring config gate — compensation of a failed create must always be
    /// able to clean up what the create itself just authored.
    /// </summary>
    /// <param name="productId">The id of the product whose authored routing rows are removed.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Success when no routing remains for the product; a failure carrying the reason.</returns>
    Task<Result> DeleteProductRoutingAsync(int productId, CancellationToken cancellationToken);
    /// <summary>
    /// Creates and persists workflows for a product based on machine assignments.
    /// Generates workflow chain from machine collection using business rules.
    /// </summary>
    /// <param name="product">Product to create workflows for</param>
    /// <param name="machineIds">Collection of machine IDs for workflow creation</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>Collection of created and persisted workflows</returns>
    /// <remarks>
    /// Workflow Creation Logic:
    /// - Sorts machines in ascending order for predictable workflow chain
    /// - Creates circular workflow: 0 → Machine1 → Machine2 → ... → 0
    /// - Assigns the configured routing rule number (default 2005, configurable via RoutingAuthoring:RoutingRuleId) to all workflows
    /// - Sets ProductId and RuleId relationships
    /// - Persists workflows using bulk operations
    ///
    /// Example: For machines [5, 3, 7] with the default routing rule number:
    /// - 0 → 3 (RuleId: 2005)
    /// - 3 → 5 (RuleId: 2005)
    /// - 5 → 7 (RuleId: 2005)
    /// - 7 → 0 (RuleId: 2005)
    /// </remarks>
    Task<Result<IEnumerable<WorkFlow>>> CreateAndPersistWorkflowsAsync(Product product, IEnumerable<int> machineIds, CancellationToken cancellationToken);

    /// <summary>
    /// Replaces a product's entire routing (whole-route replace) with the C2-clean shape derived from a new
    /// ordered machine sequence. The new sequence is fully validated BEFORE anything is deleted
    /// (validate-before-destroy), so an invalid edit — e.g. a duplicate-machine cycle — never destroys the
    /// product's existing routing.
    /// </summary>
    /// <param name="product">The product whose routing is being edited.</param>
    /// <param name="machineIds">The new ordered machine ids (ordered ascending + filtered to &gt; 0 here).</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>The persisted clean interior edges on success, or a failure carrying the reason.</returns>
    /// <remarks>
    /// Algorithm: validate (gate + magic-0 sentinel + <c>BuildRouting</c>) → snapshot the product's existing
    /// nodes + edges → delete them → insert the new draft's nodes + clean edges. On an insert failure the call
    /// best-effort deletes what it just inserted AND best-effort restores the snapshot, then returns loud.
    /// There is no shared unit-of-work across the two repositories (cross-entity atomicity is a known, accepted
    /// gap deferred to the planned aggregate-domain refactor), so the snapshot restore runs under
    /// <see cref="CancellationToken.None"/> as best-effort only. Concurrent same-product edits are last-writer-wins.
    /// </remarks>
    Task<Result<IEnumerable<WorkFlow>>> ReplaceProductRoutingAsync(Product product, IEnumerable<int> machineIds, CancellationToken cancellationToken);

    /// <summary>
    /// Generates workflow DTOs from machine collection without persistence.
    /// Pure workflow generation logic for validation or preview.
    /// </summary>
    /// <param name="machineIds">Collection of machine IDs</param>
    /// <returns>Generated workflow DTOs ready for entity conversion</returns>
    /// <remarks>
    /// Uses the same workflow generation algorithm as CreateProductCommand.CreateWorkFlowDtos.
    /// This method extracts the workflow generation logic for reusability.
    /// </remarks>
    Result<IEnumerable<WorkFlowDto>> GenerateWorkflowDtos(IEnumerable<int> machineIds);

    /// <summary>
    /// Converts workflow DTOs to entities and establishes relationships.
    /// Handles DTO to entity conversion with error collection.
    /// </summary>
    /// <param name="workflowDtos">Workflow DTOs to convert</param>
    /// <param name="product">Product to associate workflows with</param>
    /// <returns>Collection of workflow entities with relationships established</returns>
    Task<Result<IEnumerable<WorkFlow>>> ConvertAndLinkWorkflowsAsync(IEnumerable<WorkFlowDto> workflowDtos, Product product);

    /// <summary>
    /// Validates machine assignments for workflow creation.
    /// Ensures all machines are valid and available for product processing.
    /// </summary>
    /// <param name="machineIds">Machine IDs to validate</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>Validation result for machine assignments</returns>
    Task<Result> ValidateMachineAssignmentsAsync(IEnumerable<int> machineIds, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves existing workflows for a product.
    /// Used for workflow updates or relationship verification.
    /// </summary>
    /// <param name="productId">Product identifier</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>Existing workflows for the product</returns>
    Task<Result<IEnumerable<WorkFlow>>> GetWorkflowsForProductAsync(int productId, CancellationToken cancellationToken);
}