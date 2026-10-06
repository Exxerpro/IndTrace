// <copyright file="ProductsViewState.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.UI.Models.Products;

using IndTrace.Application.ConfigStations.Queries.GetConfigStationList;
using IndTrace.Application.Models.Interfaces;
using IndTrace.Application.Products.Commands.Create;
using IndTrace.Application.Products.Events;
using IndTrace.Application.Products.Queries.GetProductDetail;
using IndTrace.Application.Products.Services;
using IndTrace.Application.Repositories;
using IndTrace.Application.RulesEngine.Dto;
using IndTrace.Application.WorkFlows.Dto;
using IndTrace.Domain.Models;
using MudBlazor;

/// <summary>
/// Manages the state and operations for product view including products, machines, and workflow interactions.
/// </summary>
public class ProductsViewState(ApplicationConfiguration applicationConfiguration)
{
    /// <summary>
    /// Gets the current view of products displayed in the interface.
    /// </summary>
    public List<ProductDefinition> ProductsView { get; private set; } = [];

    /// <summary>
    /// Gets the complete list of available products.
    /// </summary>
    public List<ProductDefinition> ProductsList { get; private set; } = [];

    /// <summary>
    /// Gets the collection of machine items associated with products.
    /// </summary>
    public List<ProductMachineItem> MachineItems { get; private set; } = [];

    /// <summary>
    /// Gets the mapping of machine IDs to machine names.
    /// </summary>
    public Dictionary<int, string> MachineNames { get; private set; } = new();

    private readonly ApplicationConfiguration applicationConfiguration = applicationConfiguration ?? throw new ArgumentNullException(nameof(applicationConfiguration));

    /// <summary>
    /// Adopts the machine-name mapping by taking ownership of a COPY of <paramref name="machineNames"/>.
    /// </summary>
    /// <remarks>
    /// #217: callers hand in the PROCESS-CACHED <c>ApplicationConfiguration.MachineNames</c> dictionary — one
    /// instance shared by every Blazor circuit for the cache lifetime. The previous
    /// <c>Clear()</c>-then-reassign implementation aliased that shared instance, so the SECOND call on a view
    /// state cleared the cached dictionary process-wide (blanking the Monitor dashboard and breaking circuits on
    /// unguarded indexer reads). Never <c>Clear()</c> a caller-supplied dictionary; copy it.
    /// </remarks>
    /// <param name="machineNames">The machine-id to machine-name mapping to adopt (copied, never aliased).</param>
    public void SetMachineNames(Dictionary<int, string> machineNames)
    {
        if (machineNames is null || machineNames.Count == 0)
        {
            return;
        }

        this.MachineNames = new Dictionary<int, string>(machineNames);
    }

    /// <summary>
    /// Handles the addition of a new product to the view state.
    /// </summary>
    /// <param name="newProduct">The new product to add.</param>
    public void NewProductAdded(ProductDto newProduct)
    {
        if (newProduct is null)
        {
            return;
        }

        if (this.ProductsList.Count == 0)
        {
            this.ProductsList.AddRange(this.ProductsView);
        }

        this.ProductsView.Clear();
        this.ProductsView.Add(new ProductDefinition("Catalog", false, string.Empty));

        var newProductDefinition = new ProductDefinition(newProduct.ProductName, false, string.Empty);
        this.ProductsView.Add(newProductDefinition);

        this.ProductsView.Add(new ProductDefinition("Discard", false, string.Empty));
    }

    /// <summary>
    /// Handles product selection and updates the view accordingly.
    /// </summary>
    /// <param name="selected">The selected product.</param>
    public void ProductSelected(ProductDto selected)
    {
        if (selected is null)
        {
            return;
        }

        this.ProductSelectedItem = selected;

        if (this.ProductsList.Count == 0)
        {
            this.ProductsList.AddRange(this.ProductsView);
        }

        this.ProductsView.Clear();
        this.ProductsView.Add(new ProductDefinition("Catalog", false, string.Empty));

        var selectedProduct = this.ProductsList.FirstOrDefault(p => p.Name == selected.ProductName);
        if (selectedProduct != null)
        {
            this.ProductsView.Add(selectedProduct);
        }

        this.ProductsView.Add(new ProductDefinition("Discard", false, string.Empty));
    }

    /// <summary>
    /// Gets or sets the currently selected product item.
    /// </summary>
    public ProductDto ProductSelectedItem { get; set; } = new();

    /// <summary>
    /// Generates a list of product definitions from product DTOs.
    /// </summary>
    /// <param name="products">The list of product DTOs to convert.</param>
    /// <returns>A list of product definitions.</returns>
    public static List<ProductDefinition> GenerateProductsList(List<ProductDto> products)
    {
        List<ProductDefinition> result = [new ProductDefinition("Catalog", false, string.Empty)];

        result.AddRange(products.Select(product =>
            new ProductDefinition(product.ProductName, false, string.Empty)));

        result.Add(new ProductDefinition("Discard", false, string.Empty));

        return result;
    }

    /// <summary>
    /// Generates product machine items from products and workflows.
    /// </summary>
    /// <param name="products">The list of products.</param>
    /// <param name="workflows">The workflows to process.</param>
    /// <param name="machineNames">The machine names mapping.</param>
    /// <returns>A collection of product machine items or null if machine names are not provided.</returns>
    public static IEnumerable<ProductMachineItem>? GenerateProductsItems(List<ProductDto> products, IEnumerable<WorkFlowDto> workflows, Dictionary<int, string>? machineNames)
    {
        if (machineNames is null || machineNames.Count == 0)
        {
            return null;
        }

        var productIdToNameMap = MapProductIdsToNames(products);

        // E11.4-5: derive one display tile per DISTINCT real-machine NODE in each product's route, NOT by
        // filtering edges on the legacy singular scalar (`NextMachineId != 0`). Under the C2 nodes+edges model a
        // WorkFlowDto row is an edge (LastMachineId -> NextMachineId); `0` is the start/end-of-line BOUNDARY
        // sentinel, never a real machine. The node set is every LastMachineId and NextMachineId across the
        // product's edges, EXCLUDING that `0` boundary — so the magic-0 sentinel produces no tile, the start
        // machine (only ever an edge target, e.g. 0->A) and the terminal machine (only ever an edge source,
        // e.g. C->0) both appear, and a fork/merge does not duplicate a shared downstream machine tile.
        var result = new List<ProductMachineItem>();
        var seenNodesByProduct = new Dictionary<int, HashSet<int>>();
        var orderByProduct = new Dictionary<int, int>();

        foreach (var workflow in workflows)
        {
            if (!seenNodesByProduct.TryGetValue(workflow.ProductId, out var seenNodes))
            {
                seenNodes = [];
                seenNodesByProduct[workflow.ProductId] = seenNodes;
            }

            var productName = GetProductName(workflow.ProductId, productIdToNameMap);

            foreach (var machineId in new[] { workflow.LastMachineId, workflow.NextMachineId })
            {
                // Skip the `0` boundary sentinel and any node already emitted for this product (fork/merge dedup).
                if (machineId == 0 || !seenNodes.Add(machineId))
                {
                    continue;
                }

                var order = orderByProduct.GetValueOrDefault(workflow.ProductId, 0);
                result.Add(CreateProductMachineItem(machineId, productName, order, machineNames));
                orderByProduct[workflow.ProductId] = order + 1;
            }
        }

        return result;
    }

    private static Dictionary<int, string> MapProductIdsToNames(List<ProductDto> products)
    {
        return products.ToDictionary(p => p.ProductId, p => p.ProductName);
    }

    private static string GetProductName(int productId, Dictionary<int, string> productIdToNameMap)
    {
        return productIdToNameMap.GetValueOrDefault(productId, "default");
    }

    private static ProductMachineItem CreateProductMachineItem(int machineId, string productName, int order, Dictionary<int, string> machineNames)
    {
        return new ProductMachineItem(
            $"Machine {machineNames.GetValueOrDefault(machineId)}",
            productName,
            order,
            machineId,
            machineNames.GetValueOrDefault(machineId),
            string.Empty);
    }

    /// <summary>
    /// Generates catalog machine items from workflows.
    /// </summary>
    /// <param name="workFlowList">The list of workflows.</param>
    /// <param name="machineNames">The machine names mapping.</param>
    /// <returns>A collection of catalog machine items.</returns>
    public static IEnumerable<ProductMachineItem> GenerateCatalogMachineItems(IEnumerable<WorkFlowDto> workFlowList, Dictionary<int, string>? machineNames)
    {
        var distinctMachineIds = GetDistinctMachineIds(machineNames);
        return CreateCatalogMachineItems(distinctMachineIds, machineNames);
    }

    private static HashSet<int> GetDistinctMachineIds(Dictionary<int, string>? machineNames)
    {
        return machineNames?
            .Keys
            .Where(id => id != 0)
            .ToHashSet() ?? [];
    }

    private static IEnumerable<ProductMachineItem> CreateCatalogMachineItems(HashSet<int> distinctMachineIds, Dictionary<int, string>? machineNames)
    {
        var result = new List<ProductMachineItem>();
        var order = 0;

        if (machineNames is null || machineNames.Count == 0)
        {
            return result;
        }

        foreach (var machineId in distinctMachineIds.OrderBy(id => id))
        {
            result.Add(new ProductMachineItem(
                $"Machine {machineNames.GetValueOrDefault(machineId)}",
                "Catalog",
                order,
                machineId,
                machineNames.GetValueOrDefault(machineId),
                "Catalog"));
            order++;
        }

        return result;
    }

    /// <summary>
    /// Initializes machine items with products and workflows data.
    /// </summary>
    /// <param name="products">The list of products.</param>
    /// <param name="workflows">The workflows to process.</param>
    /// <param name="machineNames">The machine names mapping.</param>
    public void InitializeMachineItems(List<ProductDto> products, IEnumerable<WorkFlowDto> workflows, Dictionary<int, string>? machineNames)
    {
        this.MachineItems.Clear();
        var productItems = GenerateProductsItems(products, workflows, machineNames);
        var machineCatalog = GenerateCatalogMachineItems(workflows, machineNames);
        if (productItems is not null)
        {
            this.MachineItems.AddRange(productItems);
        }

        if (machineCatalog is not null)
        {
            this.MachineItems.AddRange(machineCatalog);
        }

        // Enrich ProductMachineItem instances with full DTOs using your extension methods
        foreach (var item in this.MachineItems)
        {
            var product = this.applicationConfiguration.Products.FirstOrDefault(p => p.PartNumber == item.Status);

            var machine = this.applicationConfiguration.Machines.FirstOrDefault(p => p.MachineId == item.MachineId);

            if (product is not null)
            {
                item.SetProduct(product);
            }

            if (machine is not null)
            {
                item.SetMachine(machine);
            }
        }
    }

    /// <summary>
    /// Removes duplicate machine items from the state in place. Callers previously invoked the
    /// <see cref="ProductMachineItemExtensions.RemoveDuplicates"/> extension and discarded its
    /// result (it returns a new sequence and does not mutate the source), so no de-duplication
    /// actually happened. This applies the de-duplication to the backing list.
    /// </summary>
    public void RemoveDuplicateMachineItems()
    {
        var deduplicated = this.MachineItems.RemoveDuplicates().ToList();
        if (deduplicated.Count == this.MachineItems.Count)
        {
            return;
        }

        this.MachineItems.Clear();
        this.MachineItems.AddRange(deduplicated);
    }

    /// <summary>
    /// Initializes the products view with the provided products.
    /// </summary>
    /// <param name="products">The list of products to initialize.</param>
    public void InitializeProducts(List<ProductDto> products)
    {
        this.ProductsView.Clear();
        this.ProductsList.Clear();

        var generatedProducts = GenerateProductsList(products);
        if (products is not null)
        {
            this.ProductsView.AddRange(generatedProducts);
            this.ProductsList.AddRange(generatedProducts);
        }
    }

    /// <summary>
    /// Handles the drop operation for catalog items in the drag-and-drop interface.
    /// </summary>
    /// <param name="info">The drop information containing item and drop zone details.</param>
    public void HandleCatalogItemDrop(MudItemDropInfo<ProductMachineItem> info)
    {
        // Guards: info, item, and dropzone identifier must be present
        if (info is null || info.Item is null || info.DropzoneIdentifier is not { } dropzone)
        {
            return;
        }

        var existingItem = this.MachineItems.FirstOrDefault(item => item.Name == info.Item.Name && item.Status == dropzone);

        if (existingItem != null)
        {
            this.ReplaceExistingItem(info, existingItem);
        }
        else if (info.Item.Status != dropzone)
        {
            info.Item.Status = dropzone;
            this.AddNewCatalogItem(info);
        }
    }

    private void ReplaceExistingItem(MudItemDropInfo<ProductMachineItem> info, ProductMachineItem existingItem)
    {
        if (info.Item is null || info.DropzoneIdentifier is not { } dropzone)
        {
            return;
        }
        this.MachineItems.Remove(existingItem);

        if (!this.MachineNames.TryGetValue(info.Item.MachineId, out var machineName))
        {
            return;
        }
        var catalogItem = new ProductMachineItem(info.Item.Name, "Catalog", existingItem.IndexInZone, info.Item.MachineId, machineName, string.Empty);
        var productItem = new ProductMachineItem(info.Item.Name, dropzone, existingItem.IndexInZone, info.Item.MachineId, machineName, string.Empty);

        this.MachineItems.AddIfNotExist(catalogItem);
        this.MachineItems.AddIfNotExist(productItem);
    }

    /// <summary>
    /// Handles dropping a catalog item into the discard zone.
    /// </summary>
    /// <param name="info">The drop information containing item and drop zone details.</param>
    public void HandleCatalogItemDropIntoDiscard(MudItemDropInfo<ProductMachineItem> info)
    {
        if (info.Item is null)
        {
            return;
        }
        this.RemoveItemFromState(info.Item);

        if (!this.MachineNames.TryGetValue(info.Item.MachineId, out var machineName))
        {
            return;
        }

        var newItem = new ProductMachineItem(info.Item.Name, "Catalog", info.IndexInZone, info.Item.MachineId, machineName, string.Empty);
        this.MachineItems.InsertIfNotExist(info.IndexInZone, newItem);
    }

    private void AddNewCatalogItem(MudItemDropInfo<ProductMachineItem> info)
    {
        if (info.Item is null)
        {
            return;
        }
        if (!this.MachineNames.TryGetValue(info.Item.MachineId, out var machineName))
        {
            return;
        }

        var newItem = new ProductMachineItem(info.Item.Name, "Catalog", info.IndexInZone, info.Item.MachineId, machineName, string.Empty);
        this.MachineItems.InsertIfNotExist(info.IndexInZone, newItem);
    }

    /// <summary>
    /// Removes an item from the current state.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    public void RemoveItemFromState(ProductMachineItem item)
    {
        if (item is null)
        {
            return;
        }

        var itemToRemove = this.MachineItems.FirstOrDefault(x => x.Name == item.Name && x.Status == item.Status);
        if (itemToRemove != null)
        {
            this.MachineItems.Remove(itemToRemove);
        }
    }

    /// <summary>
    /// Adds a new machine to the specified product section.
    /// </summary>
    /// <param name="section">The product section to add the machine to.</param>
    public void AddMachine(ProductDefinition section)
    {
        if (section is null)
        {
            return;
        }

        this.MachineItems.Add(new ProductMachineItem(section.NewMachineName, section.Name, this.MachineItems.Count, 0, "None", string.Empty));
        section.NewMachineName = string.Empty;
        section.NewMachine = false;
    }

    /// <summary>
    /// Deletes a product section and updates the view accordingly.
    /// </summary>
    /// <param name="section">The section to delete.</param>
    public void DeleteSection(ProductDefinition section)
    {
        if (section is null)
        {
            return;
        }

        if (this.IsLastSection())
        {
            this.ClearViewAndItems();
        }
        else
        {
            this.UpdateMachineStatusesAfterSectionDeletion(section);
        }
    }

    private bool IsLastSection()
    {
        return this.ProductsView.Count == 1;
    }

    private void ClearViewAndItems()
    {
        this.MachineItems.Clear();
        this.ProductsView.Clear();
    }

    private void UpdateMachineStatusesAfterSectionDeletion(ProductDefinition section)
    {
        var newIndex = this.GetNewIndexAfterDeletion(section);
        this.ProductsView.Remove(section);
        this.UpdateMachineStatuses(section.Name, this.ProductsView[newIndex].Name);
    }

    private int GetNewIndexAfterDeletion(ProductDefinition section)
    {
        var newIndex = this.ProductsView.IndexOf(section) - 1;
        return newIndex < 0 ? 0 : newIndex;
    }

    private void UpdateMachineStatuses(string oldStatus, string newStatus)
    {
        var machines = this.MachineItems.Where(x => x.Status == oldStatus);
        foreach (var machineItem in machines)
        {
            machineItem.Status = newStatus;
        }
    }

    /// <summary>
    /// Adds a new product using the product service.
    /// </summary>
    /// <param name="productService">The product service to use for creation.</param>
    /// <param name="productDto">The product data to create.</param>
    /// <returns>A task representing the result of the product creation operation.</returns>
    public async Task<Result<ProductCreatedEvent>> AddProduct(IProductService productService, ProductDto productDto)
    {
        // #72 (P0-12): UI GAP. The AddProduct form (AddProduct.razor) captures only ProductName,
        // CustomerName and the machine list — it has NO fields for the barcode rule (RuleJson / Name /
        // Version) or the recipe cycle-time window. The previous code fabricated a fixed QA-line-4 barcode
        // rule and a product-1 recipe (CycleTimeMaximum 216000) and stamped them onto EVERY product,
        // regardless of the part actually being created. That is removed here: we do NOT invent master data.
        // Empty rule/recipe DTOs are sent so nothing false is persisted; the real barcode rule is authored
        // via the Rules editor and per-machine recipes via the recipe/routing editor. Until AddProduct.razor
        // grows the corresponding fields, this create path deliberately authors no rule/recipe content.
        var ruleDto = new RuleDto();
        var recipe = new RecipeDto();

        // Step 5: Create the ProductCreationDto
        var productCreationDto = new ProductCreationDto
        {
            Product = productDto,
            Machines = productDto.Machines,
            Rule = ruleDto,
            Recipe = recipe,

            // E11.4-4: carry the operator's authored node+edge route (if the route editor produced one) into the
            // create pipeline so the created product persists its RoutingNode + edge rows. Null => no route authored
            // (product created without routing, as before).
            Route = productDto.Route,
        };

        // Step 11: Execute the final monitorRequest using the service. #128 (Chunk B): thread a cancellation
        // token; no ambient token flows into this view-state helper, so pass CancellationToken.None explicitly.
        var result = await productService.ExecuteCreateProductCommand(productCreationDto, CancellationToken.None);

        return result;
    }

    /// <summary>
    /// Sets the selected customer for the current context.
    /// </summary>
    /// <param name="selectedCustomer">The name of the selected customer.</param>
    public void SetSelectedCustomer(string selectedCustomer)
    {
        this.SelectedCustomer = selectedCustomer;

        // var customer = Customers.FirstOrDefault(p => p.Name == _newProduct.CustomerName);

        // if (customer is null || customer.CustomerId <= 0)
        // {
        //    return;
        // }

        // _newProduct.CustomerId = customer.CustomerId;
    }

    /// <summary>
    /// Gets or sets the name of the currently selected customer.
    /// </summary>
    public string SelectedCustomer { get; set; } = string.Empty;
}