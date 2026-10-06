// <copyright file="AppDetailsFactory.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Configuration.Services;

using IndTrace.Application.MachinesPlcs.Queries.GetMachinesList;

/// <summary>
/// Factory service responsible for creating comprehensive application configuration objects.
/// </summary>
/// <remarks>
/// This factory aggregates data from multiple repositories to build a complete ApplicationConfiguration
/// object containing PLCs, machines, products, workflows, customers, and OEE configuration.
/// It handles complex data relationships and mappings required for the IndTrace application.
/// </remarks>
public class AppDetailsFactory(
    IRepository<ConfigApp> configAppRepository,
    IRepository<Plc> plcRepository,
    IReadOnlyRepository<MachinePlc> machinePlcRepository,
    IReadOnlyRepository<WorkFlow> workflowRepository,
    IRepository<Machine> machineRepository,
    IRepository<Variable> variableRepository,
    IRepository<Customer> customerRepository,
    IRepository<VariablesGroup> variablesGroupRepository,
    IRepository<Product> productRepository,
    IIsOeeEnabledChecker isOeeEnabledChecker,
    ILogger<AppDetailsFactory> logger,
    IDateTimeMachine dateTimeMachine)
{
    /// <summary>
    /// Creates a complete application configuration object by aggregating data from multiple repositories.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>
    /// A successful Result carrying the comprehensive ApplicationConfiguration, or a failed Result
    /// propagating the repository errors.
    /// </returns>
    /// <remarks>
    /// This method loads and correlates data from:
    /// - PLCs and their associated machines
    /// - Machine-Product compatibility mappings
    /// - Customer and product relationships
    /// - Workflow configurations
    /// - OEE settings and capabilities
    /// The method uses specifications to filter active/enabled entities.
    /// #116 fail-loud: any repository FAILURE makes the whole operation a failed Result — previously each
    /// failure only logged and fell through with empty collections, so a total DB outage produced an
    /// empty-but-successful configuration that was then cached and served as a valid (empty) plant.
    /// A repository SUCCESS with genuinely zero rows remains legal (empty-but-valid config for a fresh
    /// install) — only failures become failures.
    /// </remarks>
    public async Task<Result<ApplicationConfiguration>> CreateAppDetailsAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<ApplicationConfiguration>.WithFailure("Operation cancelled.");
        }

        logger.LogInformation("Creating AppDetails");

        // Specification for PLCs with Enabled == Active.
        // Compare against the ActiveStatus static member (not the int literal) so the EF value
        // converter participates in SQL translation, emitting WHERE [Enabled] = 1.
        var plcSpecification = new Specification<Plc>(plc => plc.Enabled == ActiveStatus.Active);

        // Specification for Machine PLCs with IsActive == Active.
        // Compare against the ActiveStatus static member (not the int literal) so the EF value
        // converter participates in SQL translation, emitting WHERE [IsActive] = 1.
        var machinePlcSpecification = new Specification<MachinePlc>(machinePlc => machinePlc.IsActive == ActiveStatus.Active);

        // Specification loading the ConfigApp rows newest-first (highest AppId wins).
        // #116: this intentionally uses ListAsync instead of FirstOrDefaultAsync — the repository's
        // FirstOrDefaultAsync fails with a not-found sentinel on an empty table, which is
        // indistinguishable from a real DB failure. ListAsync keeps the two apart: an empty table is a
        // SUCCESS with zero rows (legal fresh-install state), a failure is a genuine outage.
        var configAppSpec = new Specification<ConfigApp>(_ => true)
            .AddOrderByDescending(c => c.AppId);

        var configAppList = await configAppRepository.
            ListAsync(configAppSpec, cancellationToken).ConfigureAwait(false);
        if (configAppList.IsFailure)
        {
            logger.LogError("Error getting ConfigApp: {@Errors}", configAppList.Errors);
            return Result<ApplicationConfiguration>.WithFailure(configAppList.Errors);
        }

        var configApp = configAppList.Value?.FirstOrDefault();

        var plcList = await plcRepository.
            ListAsync(plcSpecification, cancellationToken).ConfigureAwait(false);

        if (plcList.IsFailure)
        {
            logger.LogError("Error getting plcList: {@Errors}", plcList.Errors);
            return Result<ApplicationConfiguration>.WithFailure(plcList.Errors);
        }

        var machinePlcList = await machinePlcRepository.
            ListAsync(machinePlcSpecification, cancellationToken).ConfigureAwait(false);
        if (machinePlcList.IsFailure)
        {
            logger.LogError("Error getting machinePlcList: {@Errors}", machinePlcList.Errors);
            return Result<ApplicationConfiguration>.WithFailure(machinePlcList.Errors);
        }

        var oeeConfigurationResult = await isOeeEnabledChecker.CheckOeeFeatureByMachineIdsAsync(
            machinePlcList.Value?.Select(mp => mp.MachineId.Value).ToList() ?? [], cancellationToken).ConfigureAwait(false);

        if (oeeConfigurationResult.IsFailure)
        {
            logger.LogError("Error getting OeeConfigurationResult: {@Errors}", oeeConfigurationResult.Errors);
            return Result<ApplicationConfiguration>.WithFailure(oeeConfigurationResult.Errors);
        }

        var customersList = await customerRepository.
             ListAsync(cancellationToken).ConfigureAwait(false);
        if (customersList.IsFailure)
        {
            logger.LogError("Error getting customersList: {@Errors}", customersList.Errors);
            return Result<ApplicationConfiguration>.WithFailure(customersList.Errors);
        }

        var machineList = await machineRepository.
            ListAsync(cancellationToken).ConfigureAwait(false);
        if (machineList.IsFailure)
        {
            logger.LogError("Error getting machineList: {@Errors}", machineList.Errors);
            return Result<ApplicationConfiguration>.WithFailure(machineList.Errors);
        }

        Dictionary<int, string> machinesNames = [];

        if (machineList.Value is not null)
        {
            // #128: dedup first-wins on MachineId so a duplicate id cannot throw and kill the whole config build.
            machinesNames = machineList.Value
                .GroupBy(m => m.MachineId.Value)
                .ToDictionary(g => g.Key, g => g.First().Name);
        }

        // Add a default entry for "None" with ID 0
        machinesNames.TryAdd(0, "None");

        var workflowList = await workflowRepository.
                ListAsync(cancellationToken).ConfigureAwait(false);
        if (workflowList.IsFailure)
        {
            logger.LogError("Error getting workflowList: {@Errors}", workflowList.Errors);
            return Result<ApplicationConfiguration>.WithFailure(workflowList.Errors);
        }

        var productList = await productRepository.
            ListAsync(cancellationToken).ConfigureAwait(false);
        if (productList.IsFailure)
        {
            logger.LogError("Error getting productList: {@Errors}", productList.Errors);
            return Result<ApplicationConfiguration>.WithFailure(productList.Errors);
        }

        var variablesList = await variableRepository.
            ListAsync(cancellationToken).ConfigureAwait(false);
        if (variablesList.IsFailure)
        {
            logger.LogError("Error getting variablesList: {@Errors}", variablesList.Errors);
            return Result<ApplicationConfiguration>.WithFailure(variablesList.Errors);
        }

        var variablesGroupList = await variablesGroupRepository.
            ListAsync(cancellationToken).ConfigureAwait(false);
        if (variablesGroupList.IsFailure)
        {
            logger.LogError("Error getting variablesGroupList: {@Errors}", variablesGroupList.Errors);
            return Result<ApplicationConfiguration>.WithFailure(variablesGroupList.Errors);
        }

        var plcDtos = this.MapPlcListToPlcDtos(
            plcList.Value ?? [],
            machinePlcList.Value ?? [],
            machineList.Value ?? [],
            variablesList.Value ?? [],
            variablesGroupList.Value ?? [],
            oeeConfigurationResult.Value ?? new OeeConfiguration());

        // #118: materialize the ProductDto projection exactly once. The previous lazy Select chain was
        // re-enumerated by GetActiveCustomer (once per customer), GetMonitorPrinterProductMapping and the
        // Products assignment, re-running ProductDto.ToDto O(customers x products) times. A single list
        // also stamps every product DTO with one load timestamp instead of a per-enumeration timestamp.
        var productDtos = productList.Value?
            .Select(p => ProductDto.ToDto(p, dateTimeMachine.Now))
            .Where(r => r.IsSuccess)
            .Select(r => r.Value)
            .OfType<ProductDto>()
            .ToList() ?? [];

        // #222: materialize every remaining DTO projection exactly once (extending the #118 productDtos
        // precedent above). These lazy Select chains were stored inside the ApplicationConfiguration that
        // CacheManager serves BY REFERENCE for 60 minutes, so every enumeration by every consumer re-ran
        // the DTO mapping, yielded fresh DTO instances each time, and pinned the source entity lists alive
        // for the whole cache lifetime. Single-shot lists keep instance identity stable across enumerations
        // with identical order and content.
        var customerDtos = customersList.Value?
            .Select(c => CustomerDto.ToDto(c))
            .Where(r => r.IsSuccess)
            .Select(r => r.Value)
            .OfType<CustomerDto>()
            .ToList() ?? [];

        var machineDtos = machineList.Value?
            .Select(m => MachineDto.ToDto(m))
            .Where(r => r.IsSuccess)
            .Select(r => r.Value)
            .OfType<MachineDto>()
            .ToList() ?? [];

        var workFlowDtos = workflowList.Value?
            .Select(w => WorkFlowDto.ToDto(w))
            .Where(r => r.IsSuccess)
            .Select(r => r.Value)
            .OfType<WorkFlowDto>()
            .ToList() ?? [];

        var machinePlcDtos = machinePlcList.Value?
            .Select(mp => MachinePlcDto.ToDto(mp))
            .Where(r => r.IsSuccess)
            .Select(r => r.Value)
            .OfType<MachinePlcDto>()
            .ToList() ?? [];

        var activeCustomer = this.GetActiveCustomer(customerDtos, productDtos);

        // Create a dictionary of machines names and a dictionary of products names for the UI to hande special cases
        var monitorPrinters = this.GetMonitorPrinterProductMapping(
            machineDtos,
            productDtos,
            activeCustomer?.Value ?? []);

        List<MachineProductMap> machineProductMap = [];

        // All loads succeeded by this point (#116 fail-loud early returns above); only success-with-null
        // anomalies skip the compatibility join.
        if (workflowList.Value is not null && productList.Value is not null && customersList.Value is not null && machineList.Value is not null)
        {
            machineProductMap = (
                from w in workflowList.Value
                join p in productList.Value on w.ProductId equals p.ProductId.Value
                join c in customersList.Value on p.CustomerId equals c.CustomerId
                from mId in new[] { w.LastMachineId, w.NextMachineId }
                join m in machineList.Value on mId equals m.MachineId into machines
                from m in machines.DefaultIfEmpty()
                where mId.Value != 0
                select new MachineProductMap
                {
                    MachineId = m?.MachineId.Value ?? mId.Value,
                    MachineName = m?.Name ?? "Unknown",
                    ProductId = p.ProductId.Value,
                    PartNumber = p.PartNumber,
                    CustomerId = c.CustomerId,
                    CustomerName = c.Name,
                    WorkFlowId = w.WorkFlowId,
                    LastMachineId = w.LastMachineId.Value,
                    NextMachineId = w.NextMachineId.Value,
                }).Distinct().ToList();
        }
        else
        {
            logger.LogWarning("Skipping MachineProductCompatibility population due to data load failure.");
        }

        return Result<ApplicationConfiguration>.Success(new ApplicationConfiguration
        {
            WorkFlows = workFlowDtos,
            MachinePlcs = machinePlcDtos,
            Machines = machineDtos,
            Products = productDtos,
            Customers = customerDtos,
            ActiveCustomer = activeCustomer?.Value ?? [],
            MachineProductCompatibility = machineProductMap,
            Plcs = plcDtos,
            MachineNames = machinesNames,
            ConfigApp = configApp is not null ? (ConfigAppDto.ToDto(configApp).Value ?? new ConfigAppDto()) : new ConfigAppDto(),
            LineConfiguration = monitorPrinters,
            OeeConfiguration = oeeConfigurationResult.Value ?? new OeeConfiguration(),
        });
    }

    /// <summary>
    /// Maps a collection of PLC entities to PLC DTOs with their associated machines, variables, and OEE configuration.
    /// </summary>
    /// <param name="plcList">The collection of PLC entities to map.</param>
    /// <param name="machinePlcList">The machine-PLC relationship entities.</param>
    /// <param name="machineList">The collection of machine entities.</param>
    /// <param name="variablesList">The collection of variable entities.</param>
    /// <param name="variablesGroupList">The collection of variable group entities.</param>
    /// <param name="oeeConfiguration">The OEE configuration settings.</param>
    /// <returns>A list of PlcDto objects with complete mapping data.</returns>
    public List<PlcDto> MapPlcListToPlcDtos(
        IEnumerable<Plc> plcList,
        IEnumerable<MachinePlc> machinePlcList,
        IEnumerable<Machine> machineList,
        IEnumerable<Variable> variablesList,
        IEnumerable<VariablesGroup> variablesGroupList,
        OeeConfiguration oeeConfiguration)
    {
        return plcList.Select(plc => this.MapPlcToPlcDto(plc, machinePlcList, machineList, variablesList, variablesGroupList, oeeConfiguration)).ToList();
    }

    /// <summary>
    /// Maps a single PLC entity to a PLC DTO with its associated machines, variables, and configuration.
    /// </summary>
    /// <param name="plc">The PLC entity to map.</param>
    /// <param name="machinePlcList">The machine-PLC relationship entities.</param>
    /// <param name="machineList">The collection of machine entities.</param>
    /// <param name="variablesList">The collection of variable entities.</param>
    /// <param name="variablesGroupList">The collection of variable group entities.</param>
    /// <param name="oeeConfiguration">The OEE configuration settings.</param>
    /// <returns>A fully mapped PlcDto object.</returns>
    /// <remarks>
    /// #222 KNOWN LEAK (documented, not yet fixed): <see cref="PlcDto.Machines"/> (<c>IEnumerable&lt;Machine&gt;</c>),
    /// <see cref="PlcDto.Variables"/> (<c>Dictionary&lt;string, Variable&gt;</c>) and <see cref="PlcDto.VariablesGroups"/>
    /// hold RAW EF ENTITY references, and the produced PlcDto lives inside the ApplicationConfiguration that
    /// CacheManager serves BY REFERENCE to every consumer for 60 minutes. The collections themselves are
    /// materialized here (ToList/ToDictionary), but their elements are the same live entity instances shared
    /// across all consumers — consumers MUST NOT mutate them (precedent: the #217 IndTraceControllerRx incident,
    /// where mutating a cached Variable leaked local state plant-wide). <c>WorkFlowDto.Machine</c> was detached
    /// to <c>List&lt;MachineDto&gt;</c> in #225 (Slice 1), leaving PlcDto as the remaining raw-entity carrier in
    /// the cached config. Retyping these properties to detached DTOs is the tracked follow-up (#225 Slice 2);
    /// the PLC gateway consumes these maps (§7-adjacent), so the property types are deliberately unchanged here.
    /// </remarks>
    private PlcDto MapPlcToPlcDto(
        Plc plc,
        IEnumerable<MachinePlc> machinePlcList,
        IEnumerable<Machine> machineList,
        IEnumerable<Variable> variablesList,
        IEnumerable<VariablesGroup> variablesGroupList,
        OeeConfiguration oeeConfiguration)
    {
        var plcDtoResult = PlcDto.ToDto(plc);
        if (plcDtoResult.IsSuccess && plcDtoResult.Value is not null)
        {
            var plcDto = plcDtoResult.Value;
            plcDto.HasOeeEnabled = oeeConfiguration.Enabled &&
                                   oeeConfiguration.EnabledByMachine.GetValueOrDefault(plc.MachineId, false);

            plcDto.Machines = this.GetMachinesForPlc(plc.PlcId, machinePlcList, machineList);
            var plcVariables = this.GetActiveVariablesForPlc(plc.PlcId, variablesList);

            plcDto.Variables = this.MapVariablesToDictionary(plcVariables);
            plcDto.VariablesGroups = this.MapVariablesGroupsToDictionary(variablesGroupList);

            // #39: the register maps are now built through the guarded Register.Create factory and surface a
            // Result. A construction failure is mapped onto the pre-existing "conversion failed" fallback
            // (an empty PlcDto), preserving the method's failure semantics without changing its signature.
            var perfomancesResult = this.MapVariablesToRegisters(plcVariables, TagsGroups.PerformanceTags.Value);
            var registersResult = this.MapVariablesToRegisters(plcVariables, TagsGroups.RegisterTags.Value);
            var referencesResult = this.MapVariablesToRegisters(plcVariables, TagsGroups.ReferenceTags.Value);

            if (perfomancesResult.IsFailure || perfomancesResult.Value is null ||
                registersResult.IsFailure || registersResult.Value is null ||
                referencesResult.IsFailure || referencesResult.Value is null)
            {
                return new PlcDto();
            }

            plcDto.Perfomances = perfomancesResult.Value;
            plcDto.Registers = registersResult.Value;
            plcDto.References = referencesResult.Value;

            return plcDto;
        }
        else
        {
            // Return a default PlcDto if conversion fails
            return new PlcDto();
        }
    }

    /// <summary>
    /// Retrieves all machines associated with a specific PLC.
    /// </summary>
    /// <param name="plcId">The ID of the PLC to find machines for.</param>
    /// <param name="machinePlcList">The machine-PLC relationship entities.</param>
    /// <param name="machineList">The collection of all machines.</param>
    /// <returns>A collection of machines associated with the specified PLC.</returns>
    private IEnumerable<Machine> GetMachinesForPlc(
        int plcId,
        IEnumerable<MachinePlc> machinePlcList,
        IEnumerable<Machine> machineList)
    {
        var machineIds = machinePlcList.Where(mp => mp.PlcId == plcId).Select(mp => mp.MachineId).ToList();
        return machineList.Where(m => machineIds.Contains(m.MachineId)).ToList();
    }

    /// <summary>
    /// Retrieves all active variables for a specific PLC.
    /// </summary>
    /// <param name="plcId">The ID of the PLC to find variables for.</param>
    /// <param name="variablesList">The collection of all variables.</param>
    /// <returns>A collection of active variables for the specified PLC.</returns>
    private IEnumerable<Variable> GetActiveVariablesForPlc(
        int plcId,
        IEnumerable<Variable> variablesList)
    {
        return variablesList.Where(v => v.PlcId == plcId && v.IsActive.Value == ActiveStatus.Active.Value).ToList();
    }

    /// <summary>
    /// Maps variables to a dictionary with variable names as keys.
    /// </summary>
    /// <param name="plcVariables">The collection of variables to map.</param>
    /// <returns>A dictionary mapping variable names to Variable objects.</returns>
    /// <remarks>
    /// If multiple variables have the same name, the first one is used.
    /// </remarks>
    private Dictionary<string, Variable> MapVariablesToDictionary(
        IEnumerable<Variable> plcVariables)
    {
        return plcVariables.GroupBy(v => v.Name).ToDictionary(g => g.Key, g => g.First());
    }

    /// <summary>
    /// Maps variables groups to a dictionary with group names as keys.
    /// </summary>
    /// <param name="variablesGroupList">The collection of variable groups to map.</param>
    /// <returns>A dictionary mapping group names to VariablesGroup objects.</returns>
    private Dictionary<string, VariablesGroup> MapVariablesGroupsToDictionary(
        IEnumerable<VariablesGroup> variablesGroupList)
    {
        // #128: dedup first-wins (matching MapVariablesToDictionary) so a duplicate VariableGroupName no longer
        // throws ArgumentException and kills the whole configuration build.
        return variablesGroupList.GroupBy(vg => vg.VariableGroupName).ToDictionary(g => g.Key, g => g.First());
    }

    /// <summary>
    /// Maps variables to registers based on their variable group ID.
    /// </summary>
    /// <param name="plcVariables">The collection of variables to map.</param>
    /// <param name="variableGroupId">The ID of the variable group to filter by.</param>
    /// <returns>A Result carrying a dictionary mapping variable names to Register objects, or a failure.</returns>
    private Result<Dictionary<string, Register>> MapVariablesToRegisters(
     IEnumerable<Variable> plcVariables, int variableGroupId)
    {
        // #39: route each register through the guarded Register.Create factory. RegisterId (surrogate identity)
        // is mapped from VariableId via the factory's registerId parameter (the #39 lockdown made RegisterId
        // immutable, so it can no longer be assigned after construction). The StringComparer.Ordinal keyed
        // dictionary and grouped iteration order are preserved, so the produced map is byte-identical for valid
        // (non-null) variable data.
        var registers = new Dictionary<string, Register>(StringComparer.Ordinal);
        foreach (var g in plcVariables.Where(v => v.VariableGroupId == variableGroupId).GroupBy(v => v.Name))
        {
            var variable = g.First();
            var registerResult = Register.Create(
                name: variable.Name,
                description: variable.Description,
                machineId: variable.MachineId,
                variableId: variable.VariableId,
                cycleId: 0,

                // #43: Variable.Value dropped (always empty in DB, overwritten downstream); string.Empty is byte-equivalent.
                value: string.Empty,
                dataType: variable.NetType,
                statusValueId: 0,
                timeStamp: default,
                registerId: variable.VariableId);

            if (registerResult.IsFailure || registerResult.Value is null)
            {
                return Result<Dictionary<string, Register>>.WithFailure(registerResult.Error);
            }

            var register = registerResult.Value;
            registers.Add(g.Key, register);
        }

        return Result<Dictionary<string, Register>>.Success(registers);
    }

    /// <summary>
    /// Determines which customers are currently active based on their running products.
    /// </summary>
    /// <param name="customer">The collection of all customers.</param>
    /// <param name="products">The collection of all products.</param>
    /// <returns>A Result containing the collection of active customers.</returns>
    /// <remarks>
    /// A customer is considered active if they have at least one product with IsActive = 1.
    /// </remarks>
    public Result<IEnumerable<CustomerDto>> GetActiveCustomer(
        IEnumerable<CustomerDto> customer,
        IEnumerable<ProductDto> products)
    {
        var activeCustomers = customer
            .Select(c =>
            {
                // Match products to customer
                var hasRunningProduct = products.Any(p =>
                    p.CustomerId == c.CustomerId &&
                    p.IsActive == ActiveStatus.Active.Value);

                return new CustomerDto
                {
                    CustomerId = c.CustomerId,
                    Name = c.Name,
                    IsActive = c.IsActive,
                    HasProductRunningOnLine = hasRunningProduct,
                };
            })
            .Where(c => c.HasProductRunningOnLine)

            // #222: materialize the projection once. The lazy chain ended up inside the 60-minute cached
            // ApplicationConfiguration, re-building fresh CustomerDto instances on every enumeration.
            .ToList();

        return Result<IEnumerable<CustomerDto>>.Success(activeCustomers);
    }

    /// <summary>
    /// Builds a monitor mapping object linking initial printers to their associated products and customers.
    /// Ensures runtime null-safety and structured output for monitoring apps.
    /// </summary>
    /// <param name="machines">A non-null list of machines to evaluate.</param>
    /// <param name="products">A non-null list of products to associate with printers.</param>
    /// <returns>A populated <see cref="MainLineConfiguration"/> object.</returns>
    /// <exception cref="ArgumentNullException">Thrown if machines or products are null at runtime.</exception>
    private MainLineConfiguration GetMonitorPrinterProductMapping(
        IEnumerable<MachineDto> machines,
        IEnumerable<ProductDto> products,
        IEnumerable<CustomerDto> activeCustomers)
    {
        if (machines is null || products is null || activeCustomers is null)
        {
            logger.LogWarning("Machine list or product list is null. Returning empty monitor mapping.");
            return new MainLineConfiguration();
        }

        var monitorMapping = new MainLineConfiguration();

        var machineList = machines.ToList(); // Materialize early
        var productList = products.ToList();

        var initialPrinters = machineList
            .Where(m => m.MachineType == MachineType.InitialPrinter)
            .ToList();

        monitorMapping.ActiveCustomer = activeCustomers;
        monitorMapping.HasMultipleInitialPrinters = initialPrinters.Count > 1;
        monitorMapping.InitialPrinterIds = initialPrinters.Select(p => p.MachineId).ToList();

        monitorMapping.DictClientsProducts = productList
            .GroupBy(p => p.PartNumber)
            .ToDictionary(
                g => g.Key,
                g => g.Select(p => p.CustomerName)
                      .Distinct()
                      .ToList());

        monitorMapping.HasMultipleClients = monitorMapping.DictClientsProducts.Count > 1;

        monitorMapping.Associations = (from printer in initialPrinters
                                       from product in productList
                                       select new PrinterProductAssociation
                                       {
                                           MachineId = printer.MachineId,
                                           MachineName = printer.Name,
                                           MachineType = printer.MachineType,
                                           ProductId = product.ProductId,
                                           PartNumber = product.PartNumber,
                                           CustomerName = product.CustomerName,
                                       }).ToList();

        return monitorMapping;
    }
}