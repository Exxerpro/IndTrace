// <copyright file="DemoDataset.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.DemoSeed;

/// <summary>
/// A small, fictitious production line that runs out of the box on the community edition: nine stations, one
/// simulated PLC per station with the tags the gateway requires, customers, products, routes, label rules and
/// recipes. Every name and number is invented. Built through the domain's public factories, so it follows the same
/// rules as data entered through the Monitor.
/// </summary>
public sealed class DemoDataset
{
    /// <summary>The audit user stamped on every demo row.</summary>
    public const string AuditUser = "Demo";

    /// <summary>The part number the walkthrough runs through the line (route WS100 to WS500).</summary>
    public const string DemoPartNumber = "L100003";

    /// <summary>A customer with no product yet, left free for the walkthrough that adds one.</summary>
    public const string CustomerWithoutProduct = "Apex Lighting";

    /// <summary>The tags every PLC fires events on. The gateway requires exactly four.</summary>
    public static readonly IReadOnlyList<string> EventTagNames = ["Command", "HeartBeat", "PlcId", "CommandFeedback"];

    // Label rule: "WS" + "100" + part number + year (2 digits) + Julian day + 4-digit counter.
    private const string LabelRuleJson =
        """{"ruleId": "DEMO","ruleFunction": ["lineIdentifier", "fixedPart", "partNumber", "lastTwoYearDigits", "julianDay", "autoIncrement"],"components": {"lineIdentifier": {"action": "string","origin": "fixed","value": "WS"},"fixedPart": {"action": "string","origin": "fixed","value": "100"},"partNumber": {"action": "string","origin": "program","lengthMin": 6,"lengthMax": 9},"lastTwoYearDigits": {"action": "lastTwoYearDigits","origin": "program"},"julianDay": {"action": "julianDay","origin": "program"},"autoIncrement": {"action": "numeric","origin": "program","length": 4,"incremental": true}}}""";

    // The rule number route authoring stamps on every edge it saves; it refuses to save unless that rule exists.
    private static readonly int RoutingRuleId = new RoutingAuthoringOptions().RoutingRuleId;

    private const int LineId = 1;
    private const int LabelStation = 100;

    private static readonly IReadOnlyList<StationSpec> StationSpecs =
    [
        new(100, "Label printing", MachineType.InitialPrinter, WorkFlowType.Initial, [("LabelGrade", "System.Int16")]),
        new(200, "Screw driving", MachineType.Process, WorkFlowType.Serial, [("Torque", "System.Single"), ("Angle", "System.Single")]),
        new(300, "Leak test", MachineType.Process, WorkFlowType.Serial, [("LeakRate", "System.Single"), ("TestPressure", "System.Single")]),
        new(400, "Vision inspection", MachineType.Process, WorkFlowType.Serial, [("VisionScore", "System.Single")]),
        new(500, "End-of-line test", MachineType.Final, WorkFlowType.Final, [("TestVoltage", "System.Single"), ("TestCurrent", "System.Single")]),
        new(600, "Ultrasonic welding", MachineType.Process, WorkFlowType.Serial, [("WeldEnergy", "System.Single")]),
        new(700, "Lens assembly", MachineType.Process, WorkFlowType.Serial, [("LensGap", "System.Single")]),
        new(800, "Light output test", MachineType.Process, WorkFlowType.Serial, [("Luminance", "System.Single")]),
        new(900, "Packing", MachineType.Final, WorkFlowType.Final, [("BoxCount", "System.Int16")]),
    ];

    // Products.CustomerId is unique, so each customer has at most one product.
    private static readonly IReadOnlyList<string> CustomerNames =
    [
        "Northgate Auto", "Orion Components", "Harbor Drive Systems", "Summit Electric", "Lumen Optics", "Granite Axle",
        CustomerWithoutProduct,
    ];

    private static readonly IReadOnlyList<ProductSpec> ProductSpecs =
    [
        new("L100001", "Door control module", "NGA-4471", "DCM-4471", "Driver door control module", 1, [100, 300, 500]),
        new("L100002", "Brake light assembly", "ORC-2210", "BLA-2210", "Centre high-mounted brake light", 2, [100, 200, 500]),
        new(DemoPartNumber, "Rear lamp module", "HDS-3305", "RLM-3305", "Rear combination lamp module", 3, [100, 200, 300, 400, 500]),
        new("L100004", "Wiper motor housing", "SUM-7720", "WMH-7720", "Front wiper motor housing", 4, [100, 600, 700, 800, 900]),
        new("L100005", "Fog lamp lens", "LUM-5108", "FLL-5108", "Front fog lamp lens", 5, [100, 700, 800, 900]),
        new("L100006", "Sensor bracket", "GRA-6612", "SBR-6612", "Parking sensor bracket", 6, [100, 200, 300, 900]),
    ];

    private static readonly IReadOnlyList<TagsGroups> VariableGroupCatalog =
    [
        TagsGroups.EventTags, TagsGroups.ReadOnlyTags, TagsGroups.WriteOnlyTags, TagsGroups.WriteAndReadTags,
        TagsGroups.ReadCyclicTags, TagsGroups.WriteCyclicTags, TagsGroups.HeartbeatTags, TagsGroups.RegisterTags,
        TagsGroups.ReferenceTags, TagsGroups.CommandTags, TagsGroups.PerformanceTags,
    ];

    private DemoDataset()
    {
    }

    /// <summary>Gets the production line.</summary>
    public IReadOnlyList<Line> Lines { get; private init; } = [];

    /// <summary>Gets the stations (machines).</summary>
    public IReadOnlyList<Machine> Machines { get; private init; } = [];

    /// <summary>Gets one simulated, enabled PLC per station; each PLC id equals its station's machine id.</summary>
    public IReadOnlyList<Plc> Plcs { get; private init; } = [];

    /// <summary>Gets the station-to-PLC links.</summary>
    public IReadOnlyList<MachinePlc> MachinePlcs { get; private init; } = [];

    /// <summary>Gets the tag group catalog.</summary>
    public IReadOnlyList<VariablesGroup> VariablesGroups { get; private init; } = [];

    /// <summary>Gets the PLC tags.</summary>
    public IReadOnlyList<Variable> Variables { get; private init; } = [];

    /// <summary>Gets the customers.</summary>
    public IReadOnlyList<Customer> Customers { get; private init; } = [];

    /// <summary>Gets the products.</summary>
    public IReadOnlyList<Product> Products { get; private init; } = [];

    /// <summary>
    /// Gets the label rules, one per product at the label printing station, plus the inactive rule whose number
    /// route authoring stamps on saved edges.
    /// </summary>
    public IReadOnlyList<Rule> Rules { get; private init; } = [];

    /// <summary>
    /// Gets one master label per product. Barcode creation refuses a part number that no master label contains;
    /// the counter of new labels continues from these.
    /// </summary>
    public IReadOnlyList<MasterLabel> MasterLabels { get; private init; } = [];

    /// <summary>Gets the recipes, one per product and station on its route.</summary>
    public IReadOnlyList<Recipe> Recipes { get; private init; } = [];

    /// <summary>Gets the route edges (station to next station). No zero-endpoint rows.</summary>
    public IReadOnlyList<WorkFlow> WorkFlows { get; private init; } = [];

    /// <summary>Gets the route nodes and their roles (first, serial, last).</summary>
    public IReadOnlyList<RoutingNodeRow> RoutingNodes { get; private init; } = [];

    /// <summary>Gets the three daily shifts.</summary>
    public IReadOnlyList<Shift> Shifts { get; private init; } = [];

    /// <summary>Gets the application configuration row.</summary>
    public IReadOnlyList<ConfigApp> ConfigApps { get; private init; } = [];

    /// <summary>
    /// Builds the demo dataset.
    /// </summary>
    /// <param name="clock">The time source for audit timestamps and shift start times.</param>
    /// <returns>The dataset, or the first domain rule a demo row broke.</returns>
    public static Result<DemoDataset> Create(IDateTimeMachine clock)
    {
        if (clock is null)
        {
            return Result<DemoDataset>.WithFailure("A time source is required to build the demo dataset.");
        }

        var now = clock.Now;
        var plcs = new List<Plc>();
        var machinePlcs = new List<MachinePlc>();
        var variables = new List<Variable>();
        foreach (var station in StationSpecs)
        {
            var plc = Plc.Create(
                machineId: station.MachineId,
                enabled: ActiveStatus.Active,
                name: $"PLC WS{station.MachineId}",
                ipAddress: $"10.0.0.{station.MachineId / 100}",
                plcType: "Simulated",
                plcBrand: "Simulated",
                options: "[]",
                commLibrary: "Simulated",
                brandOwner: "IndTrace");
            if (plc.IsFailure || plc.Value is null)
            {
                return Result<DemoDataset>.WithFailure(plc.Errors);
            }

            plc.Value.PlcId = station.MachineId;
            plcs.Add(plc.Value);

            var link = MachinePlc.Create(station.MachineId, station.MachineId, ActiveStatus.Active);
            if (link.IsFailure || link.Value is null)
            {
                return Result<DemoDataset>.WithFailure(link.Errors);
            }

            machinePlcs.Add(link.Value);

            var tags = StationTags(station, variables.Count + 1);
            if (tags.IsFailure || tags.Value is null)
            {
                return Result<DemoDataset>.WithFailure(tags.Errors);
            }

            variables.AddRange(tags.Value);
        }

        var customers = CustomerNames
            .Select((name, index) => new Customer { CustomerId = index + 1, Name = name, IsActive = true })
            .ToList();

        var products = new List<Product>();
        var masterLabels = new List<MasterLabel>();
        var rules = new List<Rule>();
        var recipes = new List<Recipe>();
        var workFlows = new List<WorkFlow>();
        var routingNodes = new List<RoutingNodeRow>();
        foreach (var (spec, index) in ProductSpecs.Select((spec, index) => (spec, index)))
        {
            var productId = index + 1;
            var customer = customers[spec.CustomerId - 1];
            var product = Product.Create(
                partNumber: spec.PartNumber,
                productName: spec.Name,
                isActive: ActiveStatus.Active,
                version: 1,
                customerPartNumber: spec.CustomerPartNumber,
                aliasPartNumber: spec.AliasPartNumber,
                description: spec.Description,
                customerId: customer.CustomerId,
                customerName: customer.Name,
                lineId: LineId,
                ruleId: 0);
            if (product.IsFailure || product.Value is null)
            {
                return Result<DemoDataset>.WithFailure(product.Errors);
            }

            product.Value.ProductId = new ProductId(productId);
            products.Add(product.Value);

            // Same shape the label rule mints: station prefix, part number, year, Julian day, counter 0000.
            masterLabels.Add(new MasterLabel
            {
                MasterLabelId = productId,
                MasterLabelCode = $"WS{LabelStation}{spec.PartNumber}{now:yy}{now.DayOfYear:000}0000",
                Description = $"First label of {spec.PartNumber}",
            });

            rules.Add(new Rule
            {
                RuleId = productId,
                Name = $"WS{LabelStation}_{spec.PartNumber}",
                Description = $"Label rule for {spec.PartNumber}",
                RuleJson = LabelRuleJson,
                Version = 1,
                IsActive = true,
                MachineId = new MachineId(LabelStation),
                ProductId = new ProductId(productId),
            });

            foreach (var machineId in spec.Route)
            {
                // Cycle times are whole seconds and the minimum is exclusive, so 0 accepts any simulated cycle.
                var recipe = Recipe.Create(productId, machineId, cycleTimeMinimum: 0, cycleTimeMaximum: 600, maxCyclesOk: 3, maxCyclesNOk: 5, retry: 1);
                if (recipe.IsFailure || recipe.Value is null)
                {
                    return Result<DemoDataset>.WithFailure(recipe.Errors);
                }

                recipe.Value.RecipeId = recipes.Count + 1;
                recipes.Add(recipe.Value);
            }

            var route = Route(productId, spec.Route, workFlows.Count + 1, routingNodes.Count + 1);
            if (route.IsFailure || route.Value.Edges is null || route.Value.Nodes is null)
            {
                return Result<DemoDataset>.WithFailure(route.Errors);
            }

            workFlows.AddRange(route.Value.Edges);
            routingNodes.AddRange(route.Value.Nodes);
        }

        // Inactive, so barcode creation (which reads active rules only) never picks it; route authoring only needs
        // it to exist. The foreign keys need a real station and product.
        rules.Add(new Rule
        {
            RuleId = RoutingRuleId,
            Name = "ROUTING_DEFAULT",
            Description = "Rule number stamped on authored route edges (RoutingAuthoring:RoutingRuleId)",
            Version = 1,
            IsActive = false,
            MachineId = new MachineId(LabelStation),
            ProductId = products[0].ProductId,
        });

        var dataset = new DemoDataset
        {
            Lines = [new Line { LineId = LineId, Name = "Assembly Line 1", Description = "Demo assembly line", IsActive = true }],
            Machines = StationSpecs.Select(Station).ToList(),
            Plcs = plcs,
            MachinePlcs = machinePlcs,
            VariablesGroups = VariableGroupCatalog
                .Select(group => new VariablesGroup { VariableGroupId = group.Value, VariableGroupName = group.Name })
                .ToList(),
            Variables = variables,
            Customers = customers,
            Products = products,
            MasterLabels = masterLabels,
            Rules = rules,
            Recipes = recipes,
            WorkFlows = workFlows,
            RoutingNodes = routingNodes,
            Shifts = Shifts3(clock),
            ConfigApps =
            [
                new ConfigApp
                {
                    AppId = 1,
                    ConfigAppId = "IndTrace Demo",
                    MachineId = LabelStation,
                    PlcId = LabelStation,
                    Pc = "1",
                    Client = "Demo",
                    Factory = "Demo plant",
                    Line = "Assembly Line 1",
                    Project = "IndTrace",
                    Version = "1",
                },
            ],
        };

        foreach (var row in dataset.All().OfType<AuditableEntity>())
        {
            row.CreatedBy = AuditUser;
            row.CreatedOn = now;
            row.ModifiedBy = AuditUser;
            row.ModifiedOn = now;
        }

        return Result<DemoDataset>.Success(dataset);
    }

    /// <summary>
    /// Enumerates every row of the dataset, in an order that satisfies the foreign keys.
    /// </summary>
    /// <returns>All rows.</returns>
    public IEnumerable<object> All() =>
        this.Lines.Cast<object>()
            .Concat(this.Machines)
            .Concat(this.Plcs)
            .Concat(this.MachinePlcs)
            .Concat(this.VariablesGroups)
            .Concat(this.Variables)
            .Concat(this.Customers)
            .Concat(this.Products)
            .Concat(this.MasterLabels)
            .Concat(this.Rules)
            .Concat(this.Recipes)
            .Concat(this.WorkFlows)
            .Concat(this.RoutingNodes)
            .Concat(this.Shifts)
            .Concat(this.ConfigApps);

    private static Machine Station(StationSpec spec) => new()
    {
        MachineId = new MachineId(spec.MachineId),
        Name = $"WS{spec.MachineId}",
        Description = spec.Description,
        Location = "Assembly Line 1",
        MachineType = spec.MachineType,
        WorkFlowType = spec.WorkFlowType,
        EnableAppTraceability = 1,
        EnableBypassTraceability = 0,
        Retry = 1,
        RuleId = 0,
    };

    private static Result<List<Variable>> StationTags(StationSpec station, int firstVariableId)
    {
        var tags = new List<(string Name, string NetType, int Length, TagsGroups Group)>();
        tags.AddRange(EventTagNames.Select(name => (name, "System.Int16", 1, TagsGroups.EventTags)));
        tags.Add(("BarCode", "System.String", 30, TagsGroups.ReadOnlyTags));
        tags.Add(("PartNumber", "System.String", 30, TagsGroups.ReadOnlyTags));
        tags.Add(("PartStatusPlc", "System.Int16", 1, TagsGroups.ReadOnlyTags));
        tags.Add(("CycleStatusPlc", "System.Int16", 1, TagsGroups.ReadOnlyTags));
        tags.Add(("PartNumberReference", "System.String", 30, TagsGroups.ReferenceTags));
        tags.Add(("CycleTime", "System.Single", 1, TagsGroups.RegisterTags));
        tags.AddRange(station.Registers.Select(register => (register.Name, register.NetType, 1, TagsGroups.RegisterTags)));

        var variables = new List<Variable>();
        var offset = 0;
        foreach (var tag in tags)
        {
            var address = $"DB1.DBX{offset}";
            var created = Variable.Create(
                machineId: station.MachineId,
                plcId: station.MachineId,
                name: tag.Name,
                description: tag.Name,
                alias: address,
                address: address,
                netType: tag.NetType,
                length: tag.Length,
                isActive: ActiveStatus.Active,
                direction: 1,
                variableGroupId: tag.Group.Value);
            if (created.IsFailure || created.Value is null)
            {
                return Result<List<Variable>>.WithFailure(created.Errors);
            }

            created.Value.VariableId = firstVariableId + variables.Count;
            variables.Add(created.Value);
            offset += tag.NetType == "System.String" ? tag.Length + 2 : 4;
        }

        return Result<List<Variable>>.Success(variables);
    }

    private static Result<(List<WorkFlow> Edges, List<RoutingNodeRow> Nodes)> Route(
        int productId,
        IReadOnlyList<int> stations,
        int firstWorkFlowId,
        int firstNodeId)
    {
        var edges = stations
            .Zip(stations.Skip(1), (from, to) => new WorkFlow
            {
                ProductId = productId,
                LastMachineId = new MachineId(from),
                NextMachineId = new MachineId(to),
                RuleId = RoutingRuleId,
            })
            .ToList();
        for (var i = 0; i < edges.Count; i++)
        {
            edges[i].WorkFlowId = firstWorkFlowId + i;
        }

        // The node roles come from the domain's single role oracle. It reads the route boundary from the legacy
        // zero-endpoint rows, which exist here only in memory and are never stored.
        var withBoundary = new List<WorkFlow> { new() { LastMachineId = new MachineId(0), NextMachineId = new MachineId(stations[0]) } };
        withBoundary.AddRange(edges);
        withBoundary.Add(new WorkFlow { LastMachineId = new MachineId(stations[^1]), NextMachineId = new MachineId(0) });
        var nodes = RoutingNodeBackfill.BuildNodes(productId, withBoundary);
        if (nodes.IsFailure || nodes.Value is null)
        {
            return Result<(List<WorkFlow>, List<RoutingNodeRow>)>.WithFailure(nodes.Errors);
        }

        var nodeRows = nodes.Value.ToList();
        for (var i = 0; i < nodeRows.Count; i++)
        {
            nodeRows[i].RoutingNodeId = firstNodeId + i;
        }

        return Result<(List<WorkFlow>, List<RoutingNodeRow>)>.Success((edges, nodeRows));
    }

    private static List<Shift> Shifts3(IDateTimeMachine clock) =>
    [
        Shift(clock, 1, ShiftType.First, TimeSpan.FromHours(7)),
        Shift(clock, 2, ShiftType.Second, TimeSpan.FromHours(15)),
        Shift(clock, 3, ShiftType.Third, TimeSpan.FromHours(23)),
    ];

    private static Shift Shift(IDateTimeMachine clock, int id, ShiftType type, TimeSpan start) => new(clock)
    {
        ShiftId = new ShiftId(id),
        ShiftType = type.Name,
        StartBy = clock.Today + start,
        Duration = TimeSpan.FromHours(8),
    };

    private sealed record StationSpec(
        int MachineId,
        string Description,
        MachineType MachineType,
        WorkFlowType WorkFlowType,
        IReadOnlyList<(string Name, string NetType)> Registers);

    private sealed record ProductSpec(
        string PartNumber,
        string Name,
        string CustomerPartNumber,
        string AliasPartNumber,
        string Description,
        int CustomerId,
        IReadOnlyList<int> Route);
}
