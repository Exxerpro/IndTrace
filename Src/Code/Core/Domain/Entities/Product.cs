// <copyright file="Product.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

using System.ComponentModel.DataAnnotations.Schema;
using IndTrace.Domain.Enum;
using IndTrace.Domain.Interfaces;
using IndTrace.Domain.Models;
using IndTrace.Domain.ValueObjects;

/// <summary>
/// Represents a product entity with identification, versioning, and customer-related information.
/// </summary>
/// <remarks>
/// DECISION (#26, Story 26.A1 — finishes the Product de-anemization): every scalar invariant field is now
/// <c>private set</c>, so construction and mutation are the entity's own responsibility, exactly like the shipped
/// <c>BarCode</c>/<c>Cycle</c>/<c>Recipe</c>/<c>KpiOee</c> template. Construction routes through the guarded
/// <see cref="Create"/> factory (production), the <see cref="CreateEmpty"/>/<see cref="CreateWithId"/> placeholder
/// seams (the few non-persisted carrier slots), or the <see cref="CreateFixture"/> unguarded-hydration seam
/// (test-data builders and trusted internal projections). The single legitimate post-construction mutation path —
/// the partial product update — is the guarded <see cref="ApplyUpdate"/> method returning <see cref="Result{T}"/>.
/// Exceptions, per the config precedent: <see cref="ProductId"/> (the database-assigned identity) keeps a public
/// setter — mirroring <c>KpiOee.KpiOeeId</c>/<c>Recipe.RecipeId</c>/<c>Cycle.CycleId</c> — and the navigation
/// properties (<see cref="Line"/>/<see cref="Customer"/>) keep public setters for EF Core fix-up. This supersedes
/// the earlier Story 2.1 decision (which had kept the value setters public for the update handler and the fixture
/// bed); the update handler now routes through <see cref="ApplyUpdate"/> and the fixtures through the seams.
/// </remarks>
public class Product : AuditableEntity, IAggregateRoot
{
    // #95 Phase 2 Slice B: the aggregate's loaded/staged Recipe members. Held as explicit CLR collections
    // mirroring BarCode's loaded/staged sets — deliberately [NotMapped] so they never enter the EF model
    // (no entity-shape/navigation change; ProductAggregateRepository manages them). Populated only by
    // AttachLoadedRecipes / StageRecipeAppend / StageRecipeRemoval.
    [NotMapped]
    private readonly List<Recipe> loadedRecipes = [];

    [NotMapped]
    private readonly List<Recipe> pendingRecipeAppends = [];

    [NotMapped]
    private readonly List<Recipe> pendingRecipeRemovals = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="Product"/> class. Story 26.A1 (#26): the parameterless
    /// constructor stays <c>public</c> (mirroring the <c>BarCode</c> precedent's implicit public constructor and
    /// satisfying the <c>where T : new()</c> generic constraint used by the CRUD test helpers) — but it produces
    /// only an empty placeholder (all string scalars <see cref="string.Empty"/>, all ids/version <c>0</c>, default
    /// navs), so it is NOT a meaningful construction path. Story 26.A4 (#26): the three identity/description strings
    /// (<see cref="PartNumber"/>/<see cref="ProductName"/>/<see cref="Description"/>) initialize to
    /// <see cref="string.Empty"/> — matching the other three scalars and the shipped <c>BarCode</c> template —
    /// eliminating the pre-existing null-forgiving assignments (issue #42 null-forgiving ban) with no observable change
    /// to any pinned wire byte (no §7/golden-master path sources a scalar from an empty placeholder). The
    /// de-anemization is enforced at the value <b>setters</b> (all <c>private set</c>): a bare <c>new Product()</c>
    /// cannot be populated except through <see cref="Create"/>, the creation seams, or <see cref="ApplyUpdate"/>.
    /// </summary>
    public Product()
    {
        this.PartNumber = string.Empty;
        this.ProductName = string.Empty;
        this.CustomerPartNumber = string.Empty;
        this.AliasPartNumber = string.Empty;
        this.Description = string.Empty;
        this.CustomerName = string.Empty;

        // Story #81: the Line/Customer navigations are NO LONGER eagerly newed here. Constructing an empty
        // Line()/Customer() phantom made a bare `new Product()` carry two throw-away child entities that EF could
        // attempt to graph-insert (an FK-fight / spurious-insert hazard). An absent related entity is now modeled
        // as a null reference (BarCode/Recipe/Cycle "absent reference, not empty" discipline), consistent with
        // ProductsConfiguration ignoring both navs. Callers set a real Line/Customer via EF fix-up or explicitly.
    }

    /// <summary>
    /// Gets or sets the unique identifier for the product. Story 26.A1 (#26): the identity keeps a <c>public</c>
    /// setter — it is assigned by the database (identity column) and by the creation/test seams — mirroring the
    /// <c>KpiOee.KpiOeeId</c>/<c>Recipe.RecipeId</c>/<c>Cycle.CycleId</c> precedent. Story 35.D2 Cluster 4 (#35):
    /// retyped from a bare <see cref="int"/> to the strongly-typed <see cref="ValueObjects.ProductId"/> struct (the
    /// entity's own primary key), mapped to the SAME unchanged <c>int</c> identity column via a value-preserving EF
    /// converter. This makes the modeled inbound FKs (<c>Rule.ProductId</c>, <c>ProductSpec.ProductId</c>,
    /// <c>BarCode.ProductId</c>) type-compatible with the converted principal key. Narrows to the raw <c>int</c> via
    /// <c>.Value</c> at every DTO / view-model / structured-log / LINQ-to-SQL boundary.
    /// </summary>
    public ProductId ProductId { get; set; }

    /// <summary>
    /// Gets the part number of the product. Story 26.A1 (#26): restricted to <c>private set</c> so the identity
    /// invariant established by <see cref="Create"/> cannot be violated post-construction; the partial-update
    /// path now routes through the guarded <see cref="ApplyUpdate"/> method. EF Core materializes it through the
    /// property mapping, so the setter must remain settable (not get-only).
    /// </summary>
    public string PartNumber { get; private set; }

    /// <summary>
    /// Gets the name of the product. Story 26.A1 (#26): <c>private set</c> (see <see cref="PartNumber"/>).
    /// </summary>
    public string ProductName { get; private set; }

    /// <summary>
    /// Gets the tri-state activation status of the product
    /// (<see cref="ActiveStatus.Inactive"/>, <see cref="ActiveStatus.None"/>, or <see cref="ActiveStatus.Active"/>).
    /// Persisted as an <c>int</c> column via a read-normalizing EF value converter (any positive stored value
    /// materializes as <see cref="ActiveStatus.Active"/>, honoring the historical "positive = active" contract);
    /// defaults to <see cref="ActiveStatus.None"/> (0). Story 26.A1 (#26): <c>private set</c> (see
    /// <see cref="PartNumber"/>).
    /// </summary>
    public ActiveStatus IsActive { get; private set; } = ActiveStatus.None;

    /// <summary>
    /// Gets the version of the product. Story 26.A1 (#26): <c>private set</c> (see <see cref="PartNumber"/>).
    /// </summary>
    public int Version { get; private set; }

    /// <summary>
    /// Gets the customer part number associated with the product. Story 26.A1 (#26): <c>private set</c>.
    /// </summary>
    public string CustomerPartNumber { get; private set; }

    /// <summary>
    /// Gets the alias part number for the product. Story 26.A1 (#26): <c>private set</c>.
    /// </summary>
    public string AliasPartNumber { get; private set; }

    /// <summary>
    /// Gets the description of the product. Story 26.A1 (#26): <c>private set</c>.
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// Gets the rule identifier associated with the product. Story 26.A1 (#26): <c>private set</c>.
    /// </summary>
    public int RuleId { get; private set; }

    /// <summary>
    /// Gets the customer identifier associated with the product. Story 26.A1 (#26): <c>private set</c>.
    /// </summary>
    public int CustomerId { get; private set; }

    /// <summary>
    /// Gets the line identifier associated with the product. Story 26.A1 (#26): <c>private set</c>.
    /// </summary>
    public int LineId { get; private set; }

    /// <summary>
    /// Gets or sets the line entity associated with the product, or <see langword="null"/> when no line is
    /// attached. Story #81: modeled as a NULLABLE reference (absent, not an empty phantom) so a bare
    /// <c>new Product()</c> no longer carries a throw-away <c>Line</c> for EF to graph-insert. Kept a public
    /// setter for EF Core fix-up; <c>ProductsConfiguration</c> ignores this navigation. The scalar
    /// <see cref="LineId"/> remains the system of record.
    /// </summary>
    public Line? Line { get; set; }

    /// <summary>
    /// Gets or sets the customer entity associated with the product, or <see langword="null"/> when no customer
    /// is attached. Story #81: modeled as a NULLABLE reference (absent, not an empty phantom); see
    /// <see cref="Line"/>. The scalar <see cref="CustomerId"/> remains the system of record.
    /// </summary>
    public Customer? Customer { get; set; }

    /// <summary>
    /// Gets the customer name associated with the product. Story 26.A1 (#26): <c>private set</c>.
    /// </summary>
    public string CustomerName { get; private set; }

    /// <summary>
    /// Story 2.1 (#26) PUBLIC creation seam. Constructs a product, enforcing the product's identity
    /// invariant: <see cref="PartNumber"/> and <see cref="ProductName"/> must not be <see langword="null"/>
    /// (the anemic parameterless constructor leaves them null until populated). Both violations are
    /// aggregated rather than fail-fast. Empty strings are intentionally accepted — the legacy
    /// <c>ProductFactory</c> contract coalesces missing input to <see cref="string.Empty"/> and the
    /// behavioral test bed relies on that — so this guard is calibrated to the universally-safe invariant
    /// only. Identity (<see cref="ProductId"/>), audit, and navigation properties
    /// (<see cref="Customer"/>/<see cref="Line"/>) are NOT taken here; callers set them after construction
    /// (mirroring how <c>Recipe.Create</c> leaves <c>RecipeId</c> to the persistence layer).
    /// </summary>
    /// <param name="partNumber">The part number; must not be <see langword="null"/>.</param>
    /// <param name="productName">The product name; must not be <see langword="null"/>.</param>
    /// <param name="isActive">The tri-state activation status (already a validated value object; #25).</param>
    /// <param name="version">The product version.</param>
    /// <param name="customerPartNumber">The customer part number.</param>
    /// <param name="aliasPartNumber">The alias part number.</param>
    /// <param name="description">The product description.</param>
    /// <param name="customerId">The associated customer identifier.</param>
    /// <param name="customerName">The associated customer name.</param>
    /// <param name="lineId">The associated line identifier.</param>
    /// <param name="ruleId">The associated rule identifier.</param>
    /// <returns>A success result carrying the product, or a failure result aggregating the violated invariants.</returns>
    public static Result<Product> Create(
        string? partNumber,
        string? productName,
        ActiveStatus isActive,
        int version,
        string customerPartNumber,
        string aliasPartNumber,
        string description,
        int customerId,
        string customerName,
        int lineId,
        int ruleId)
    {
        var errors = new List<string>();

        if (partNumber is null)
        {
            errors.Add("PartNumber cannot be null.");
        }

        if (productName is null)
        {
            errors.Add("ProductName cannot be null.");
        }

        if (errors.Count > 0)
        {
            return Result<Product>.WithFailure(errors);
        }

        // Both identity strings are non-null here (validated above); this explicit guard re-narrows them
        // for the compiler without resorting to the banned null-forgiving operator.
        if (partNumber is null || productName is null)
        {
            return Result<Product>.WithFailure("PartNumber and ProductName are required.");
        }

        return Result<Product>.Success(new Product
        {
            PartNumber = partNumber,
            ProductName = productName,
            IsActive = isActive,
            Version = version,
            CustomerPartNumber = customerPartNumber,
            AliasPartNumber = aliasPartNumber,
            Description = description,
            CustomerId = customerId,
            CustomerName = customerName,
            LineId = lineId,
            RuleId = ruleId,
        });
    }

    /// <summary>
    /// Story 26.A1 (#26) INTERNAL SEAM. Constructs an empty placeholder product (all string scalars
    /// <see cref="string.Empty"/> after Story 26.A4, all ids/version <c>0</c>, default navs). Serves the
    /// few non-persisted carrier slots (<c>BarCodeResult</c>/<c>CyclesDto</c>/<c>ProductSpec</c> reset defaults)
    /// that are always overwritten before use. Exposed to the granted assemblies via <c>InternalsVisibleTo</c> —
    /// NOT part of the public surface; production external construction must use <see cref="Create"/>. Delegates to
    /// the parameterless constructor, so it carries exactly its placeholder state.
    /// </summary>
    /// <returns>An empty placeholder product.</returns>
    internal static Product CreateEmpty() => new();

    /// <summary>
    /// Story 26.A1 (#26) INTERNAL SEAM. Constructs an id-only carrier product — byte-identical to the former
    /// <c>new Product { ProductId = productId }</c> literal — for callers that need only the identity (for
    /// example <c>RecipeOrchestrator</c>, which reads only <see cref="ProductId"/> and never persists the
    /// product in this shape). Exposed via <c>InternalsVisibleTo</c>; production external construction must use
    /// <see cref="Create"/>.
    /// </summary>
    /// <param name="productId">The product identifier to seed.</param>
    /// <returns>An id-only carrier product.</returns>
    internal static Product CreateWithId(int productId) => new() { ProductId = new ProductId(productId) };

    /// <summary>
    /// Story 26.A1 (#26) POST-CREATION MUTATION seam. Applies a partial product update, coalescing each supplied
    /// value with the current one (a <see langword="null"/> argument leaves the corresponding field unchanged) and
    /// re-asserting the <see cref="Create"/> identity invariant (<see cref="PartNumber"/>/<see cref="ProductName"/>
    /// non-null) after coalescing. This is the single guarded write path that replaces the raw property
    /// assignments the update handler previously performed once the value setters became <c>private set</c>; it is
    /// byte-equal to those assignments for the callers' coalesced inputs. Identity (<see cref="ProductId"/>) is not
    /// part of the update (it is the lookup key) and audit stamping remains the persistence layer's responsibility.
    /// </summary>
    /// <param name="partNumber">The new part number, or <see langword="null"/> to keep the current value.</param>
    /// <param name="productName">The new product name, or <see langword="null"/> to keep the current value.</param>
    /// <param name="isActive">The new activation status, or <see langword="null"/> to keep the current value.</param>
    /// <param name="version">The new version, or <see langword="null"/> to keep the current value.</param>
    /// <param name="customerPartNumber">The new customer part number, or <see langword="null"/> to keep the current value.</param>
    /// <param name="aliasPartNumber">The new alias part number, or <see langword="null"/> to keep the current value.</param>
    /// <param name="description">The new description, or <see langword="null"/> to keep the current value.</param>
    /// <returns>A success result carrying this product, or a failure aggregating the violated invariants.</returns>
    public Result<Product> ApplyUpdate(
        string? partNumber,
        string? productName,
        ActiveStatus? isActive,
        int? version,
        string? customerPartNumber,
        string? aliasPartNumber,
        string? description)
    {
        var newPartNumber = partNumber ?? this.PartNumber;
        var newProductName = productName ?? this.ProductName;

        var errors = new List<string>();

        if (newPartNumber is null)
        {
            errors.Add("PartNumber cannot be null.");
        }

        if (newProductName is null)
        {
            errors.Add("ProductName cannot be null.");
        }

        if (errors.Count > 0)
        {
            return Result<Product>.WithFailure(errors);
        }

        // Re-narrow for the compiler without the banned null-forgiving operator (both validated non-null above).
        if (newPartNumber is null || newProductName is null)
        {
            return Result<Product>.WithFailure("PartNumber and ProductName are required.");
        }

        this.PartNumber = newPartNumber;
        this.ProductName = newProductName;
        this.AliasPartNumber = aliasPartNumber ?? this.AliasPartNumber;
        this.CustomerPartNumber = customerPartNumber ?? this.CustomerPartNumber;
        this.Description = description ?? this.Description;
        this.IsActive = isActive ?? this.IsActive;
        this.Version = version ?? this.Version;

        return Result<Product>.Success(this);
    }

    /// <summary>
    /// Story 26.A1 (#26) POST-CREATION MUTATION seam. Associates a rule with this product by setting
    /// <see cref="RuleId"/>. This is the single guarded write that replaces the former raw
    /// <c>product.RuleId = ...</c> assignment (in <c>RuleOrchestrator</c>) once the value setters became
    /// <c>private set</c>; it is byte-equal to that assignment. <see cref="RuleId"/> is an association identifier
    /// with no additional invariant, so this always succeeds — it returns <see cref="Result{T}"/> for consistency
    /// with the entity's guarded-method contract.
    /// </summary>
    /// <param name="ruleId">The rule identifier to associate with this product.</param>
    /// <returns>A success result carrying this product.</returns>
    public Result<Product> AssignRule(int ruleId)
    {
        this.RuleId = ruleId;
        return Result<Product>.Success(this);
    }

    /// <summary>
    /// Story 2.1 (#26) INTERNAL SEAM. Seeds a product directly from arbitrary, legacy, trusted-persisted, or even
    /// invalid values WITHOUT applying the <see cref="Create"/> guards. Serves test-data builders and trusted
    /// internal read-projections (values already sourced from persisted products). Every parameter is optional so
    /// callers seed only the fields they need; unset scalars default to <c>0</c>/<see cref="string.Empty"/> and
    /// <see cref="IsActive"/> defaults to <see cref="ActiveStatus.None"/>. Setting the fields here is legal even
    /// though they are <c>private set</c> because this factory lives INSIDE <c>IndTrace.Domain</c>. Exposed to the
    /// granted assemblies via <c>InternalsVisibleTo</c> — NOT part of the public surface; production external
    /// construction must use <see cref="Create"/>. Mirrors <c>Recipe.CreateFixture</c>/<c>Register.CreateFixture</c>.
    /// </summary>
    /// <param name="productId">The product identifier to seed.</param>
    /// <param name="partNumber">The part number to seed, unvalidated.</param>
    /// <param name="productName">The product name to seed, unvalidated.</param>
    /// <param name="isActive">The activation status to seed, or <see langword="null"/> for <see cref="ActiveStatus.None"/>.</param>
    /// <param name="version">The version to seed, unvalidated.</param>
    /// <param name="customerPartNumber">The customer part number to seed.</param>
    /// <param name="aliasPartNumber">The alias part number to seed.</param>
    /// <param name="description">The description to seed.</param>
    /// <param name="customerId">The customer identifier to seed.</param>
    /// <param name="customerName">The customer name to seed.</param>
    /// <param name="lineId">The line identifier to seed.</param>
    /// <param name="ruleId">The rule identifier to seed.</param>
    /// <returns>A product seeded with the supplied values.</returns>
    internal static Product CreateFixture(
        int productId = 0,
        string partNumber = "",
        string productName = "",
        ActiveStatus? isActive = null,
        int version = 0,
        string customerPartNumber = "",
        string aliasPartNumber = "",
        string description = "",
        int customerId = 0,
        string customerName = "",
        int lineId = 0,
        int ruleId = 0) =>
        new()
        {
            ProductId = new ProductId(productId),
            PartNumber = partNumber,
            ProductName = productName,
            IsActive = isActive ?? ActiveStatus.None,
            Version = version,
            CustomerPartNumber = customerPartNumber,
            AliasPartNumber = aliasPartNumber,
            Description = description,
            CustomerId = customerId,
            CustomerName = customerName,
            LineId = lineId,
            RuleId = ruleId,
        };

    /// <summary>
    /// Gets the Recipe member rows the aggregate repository materialised on <c>LoadAsync</c> (#95 Phase 2
    /// Slice B). Read-only exposure of the loaded fold — populated ONLY by <see cref="AttachLoadedRecipes"/>;
    /// the repository never persists this set directly. Empty until a load attaches it.
    /// </summary>
    [NotMapped]
    public IReadOnlyList<Recipe> LoadedRecipes => this.loadedRecipes;

    /// <summary>
    /// Gets the Recipe rows staged for insertion by <see cref="StageRecipeAppend"/> — the insert set the
    /// aggregate repository persists inside its single explicit transaction (#95 Phase 2 Slice B). Cleared
    /// by the repository after a durable commit (<see cref="ClearStagedRecipeChanges"/>).
    /// </summary>
    [NotMapped]
    public IReadOnlyList<Recipe> PendingRecipeAppends => this.pendingRecipeAppends;

    /// <summary>
    /// Gets the Recipe rows staged for removal by <see cref="StageRecipeRemoval"/> — the delete set the
    /// aggregate repository persists inside its single explicit transaction (#95 Phase 2 Slice B). Cleared
    /// by the repository after a durable commit (<see cref="ClearStagedRecipeChanges"/>).
    /// </summary>
    [NotMapped]
    public IReadOnlyList<Recipe> PendingRecipeRemovals => this.pendingRecipeRemovals;

    /// <summary>
    /// #95 Phase 2 Slice B hydration seam (the <c>BarCode.AttachLoadedCycles</c> precedent). Attaches the
    /// Recipe member set the aggregate repository loaded so callers can inspect the aggregate's persisted
    /// recipes. Additive and idempotent — it REPLACES any prior loaded set and does NOT mutate any recipe.
    /// NEVER throws: a <see langword="null"/> collection is treated as an empty set.
    /// </summary>
    /// <param name="recipes">The loaded recipe rows for this product; <see langword="null"/> is treated as empty.</param>
    public void AttachLoadedRecipes(IEnumerable<Recipe>? recipes)
    {
        this.loadedRecipes.Clear();
        if (recipes is not null)
        {
            this.loadedRecipes.AddRange(recipes);
        }
    }

    /// <summary>
    /// #95 Phase 2 Slice B staging seam. Stages a recipe for insertion through the aggregate's single
    /// transactional save. Fails (staging NOTHING) when the recipe is <see langword="null"/> or belongs to a
    /// different product — an aggregate only ever stages its own members. Never throws.
    /// </summary>
    /// <param name="recipe">The recipe to append (may be <see langword="null"/> — refused as a failure); its <see cref="Recipe.ProductId"/> must match this product.</param>
    /// <returns>A success <see cref="Result"/>, or a failure carrying the violated invariant.</returns>
    public Result StageRecipeAppend(Recipe? recipe)
    {
        if (recipe is null)
        {
            return Result.WithFailure($"Product {this.ProductId.Value}: cannot stage a null recipe for append.");
        }

        if (recipe.ProductId != this.ProductId.Value)
        {
            return Result.WithFailure(
                $"Product {this.ProductId.Value}: recipe (machine {recipe.MachineId}) targets product {recipe.ProductId}; it must match this aggregate's product.");
        }

        this.pendingRecipeAppends.Add(recipe);
        return Result.Success();
    }

    /// <summary>
    /// #95 Phase 2 Slice B staging seam. Stages a persisted recipe for removal through the aggregate's single
    /// transactional save (the compensating-delete path of the create-product saga). Fails (staging NOTHING)
    /// when the recipe is <see langword="null"/>, belongs to a different product, or carries no persisted
    /// identity (<see cref="Recipe.RecipeId"/> &lt;= 0 — there is no row to remove). Never throws.
    /// </summary>
    /// <param name="recipe">The persisted recipe to remove (may be <see langword="null"/> — refused as a failure); its <see cref="Recipe.ProductId"/> must match this product.</param>
    /// <returns>A success <see cref="Result"/>, or a failure carrying the violated invariant.</returns>
    public Result StageRecipeRemoval(Recipe? recipe)
    {
        if (recipe is null)
        {
            return Result.WithFailure($"Product {this.ProductId.Value}: cannot stage a null recipe for removal.");
        }

        if (recipe.ProductId != this.ProductId.Value)
        {
            return Result.WithFailure(
                $"Product {this.ProductId.Value}: recipe {recipe.RecipeId} targets product {recipe.ProductId}; it must match this aggregate's product.");
        }

        if (recipe.RecipeId <= 0)
        {
            return Result.WithFailure(
                $"Product {this.ProductId.Value}: recipe for machine {recipe.MachineId} has no persisted identity; only persisted recipes can be staged for removal.");
        }

        this.pendingRecipeRemovals.Add(recipe);
        return Result.Success();
    }

    /// <summary>
    /// #95 Phase 2 Slice B clear-after-save seam. Empties both staged sets
    /// (<see cref="PendingRecipeAppends"/> / <see cref="PendingRecipeRemovals"/>) — called by the aggregate
    /// repository strictly AFTER a durable commit so a subsequent save cannot double-apply the same staged
    /// changes. The loaded fold (<see cref="LoadedRecipes"/>) is intentionally left untouched: it reflects the
    /// load-time snapshot and is refreshed by the next <c>LoadAsync</c>, not by a save.
    /// </summary>
    public void ClearStagedRecipeChanges()
    {
        this.pendingRecipeAppends.Clear();
        this.pendingRecipeRemovals.Clear();
    }
}
