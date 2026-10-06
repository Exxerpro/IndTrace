// <copyright file="IRuleOrchestrator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Routing.Authoring;

namespace IndTrace.Application.Products.Services.Interfaces;

/// <summary>
/// Rule creation and linking orchestration service.
/// Application service - manages rule creation from DTOs and product association.
/// </summary>
public interface IRuleOrchestrator
{
    /// <summary>
    /// Creates and persists a rule for a product with machine association.
    /// Handles rule creation, machine assignment, and product linking.
    /// </summary>
    /// <param name="ruleDto">Rule data transfer object containing rule configuration</param>
    /// <param name="product">Product to associate the rule with</param>
    /// <param name="workflows">Workflows containing machine assignments for rule association</param>
    /// <param name="route">The authored node+edge route (may be null); its INITIAL machine is used when no workflows are present.</param>
    /// <param name="authoredMachineIds">The command's authored machine ids (legacy create-without-route source); the first &gt; 0 is used when neither workflows nor a route yield a machine.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>Created and persisted rule entity</returns>
    /// <remarks>
    /// Rule Creation Logic:
    /// - Converts RuleDto to Rule entity
    /// - Ensures RuleId = 0 for EF to assign new ID
    /// - Sets ProductId relationship to associate with product
    /// - Resolves MachineId from workflows (LastMachineId where > 0); when none, from the route's initial machine
    /// - Persists rule to database
    /// - Updates product.RuleId with created rule ID
    /// - Updates product entity to maintain relationship
    ///
    /// Machine Assignment Logic (resolution cascade):
    /// - Searches workflows for LastMachineId > 0 (Distinct, FirstOrDefault) — original handler semantic, unchanged
    /// - When no workflow yields a machine, uses the authored route's initial machine (E11.4 create-with-route path)
    /// - When neither yields one, uses the first authored machine id > 0 (legacy create-without-route path)
    /// - FAILS LOUD when none yields a machine id &gt; 0, never persisting a rule with MachineId 0
    /// </remarks>
    Task<Result<Rule>> CreateAndLinkRuleAsync(RuleDto ruleDto, Product product, IEnumerable<WorkFlow> workflows, AuthoringRoute? route, IReadOnlyList<int> authoredMachineIds, CancellationToken cancellationToken);

    /// <summary>
    /// Compensating delete for a rule committed by <see cref="CreateAndLinkRuleAsync"/> when a LATER product-creation
    /// step failed. The rule row carries a <c>ProductId</c> FK to Product with <c>OnDelete(Restrict)</c>, so it must be
    /// removed before the product delete or that delete is blocked by the constraint and the orphan is never cleaned up.
    /// </summary>
    /// <param name="rule">The committed rule to remove.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>Success when the rule is removed; a failure carrying the reason.</returns>
    Task<Result> DeleteRuleAsync(Rule rule, CancellationToken cancellationToken);

    /// <summary>
    /// Converts RuleDto to Rule entity without persistence.
    /// Pure DTO to entity conversion for validation or preview.
    /// </summary>
    /// <param name="ruleDto">Rule DTO to convert</param>
    /// <returns>Rule entity converted from DTO</returns>
    Result<Rule> ConvertRuleDtoToEntity(RuleDto ruleDto);

    /// <summary>
    /// Determines machine assignment for rule from workflow collection.
    /// Extracts machine ID from workflows using business logic.
    /// </summary>
    /// <param name="workflows">Workflows containing machine assignments</param>
    /// <returns>Machine ID for rule association, or 0 if none found</returns>
    /// <remarks>
    /// Machine Selection Logic:
    /// - Filters workflows where LastMachineId > 0
    /// - Applies Distinct() to remove duplicates
    /// - Returns FirstOrDefault() for single machine selection
    /// - This preserves the original handler's machine assignment logic
    /// </remarks>
    int DetermineMachineIdFromWorkflows(IEnumerable<WorkFlow> workflows);

    /// <summary>
    /// Determines the rule's machine from an authored route: its INITIAL machine (the node flagged
    /// <see cref="WorkFlowType.Initial"/>, or structurally the node that is never any edge's target).
    /// Returns 0 when the route is null/empty or no entry machine can be identified.
    /// </summary>
    /// <param name="route">The authored route, or null when none was authored.</param>
    /// <returns>The route's initial machine id (&gt; 0), or 0 when it cannot be resolved.</returns>
    int DetermineInitialMachineIdFromRoute(AuthoringRoute? route);

    /// <summary>
    /// Determines the rule's machine from the command's authored machine ids (the legacy create-without-route
    /// source): the first real (&gt; 0) machine. Returns 0 when the list is null/empty or holds no positive id.
    /// </summary>
    /// <param name="authoredMachineIds">The command's authored machine ids, or null/empty when none were supplied.</param>
    /// <returns>The first authored machine id (&gt; 0), or 0 when none can be resolved.</returns>
    int DetermineMachineIdFromAuthoredMachines(IReadOnlyList<int>? authoredMachineIds);

    /// <summary>
    /// Updates product entity with rule relationship AND persists the assignment (F4, #113). The product passed
    /// in is a DETACHED instance already committed by the create pipeline's persist step, so the rule id is
    /// written back through the product repository — a failed write is a failure of this operation (fail-loud),
    /// never a silent success over an unpersisted assignment.
    /// </summary>
    /// <param name="product">Product to update with rule relationship</param>
    /// <param name="rule">Rule to associate with product</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    /// <returns>Updated product with rule relationship established and persisted</returns>
    Task<Result<Product>> UpdateProductWithRuleAsync(Product product, Rule rule, CancellationToken cancellationToken);

    /// <summary>
    /// Validates rule configuration for product association.
    /// Ensures rule data is valid for the specific product type.
    /// </summary>
    /// <param name="ruleDto">Rule configuration to validate</param>
    /// <param name="product">Product the rule will be associated with</param>
    /// <returns>Validation result for rule-product compatibility</returns>
    Task<Result> ValidateRuleForProductAsync(RuleDto ruleDto, Product product);
}