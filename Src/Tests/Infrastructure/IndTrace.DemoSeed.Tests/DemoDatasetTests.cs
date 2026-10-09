// <copyright file="DemoDatasetTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.DemoSeed.Tests;

using IndTrace.Application.Plcs.Queries.GetDetail;
using IndTrace.Devices.Plc;
using IndTrace.Domain.Entities;
using IndTrace.Domain.Enum;
using IndTrace.Domain.Models;
using Meziantou.Extensions.Logging.Xunit.v3;
using Shouldly;

/// <summary>
/// The demo dataset must run as a line out of the box: every PLC is accepted by the simulated controller, every
/// route is linear and stored without legacy zero-endpoint rows, and the walkthrough's assumptions hold.
/// </summary>
public class DemoDatasetTests(ITestOutputHelper output)
{
    private static DemoDataset Dataset()
    {
        var dataset = DemoDataset.Create(new DateTimeMachine());
        dataset.IsSuccess.ShouldBeTrue(string.Join("; ", dataset.Errors));
        dataset.Value.ShouldNotBeNull();
        return dataset.Value;
    }

    [Fact]
    public void EveryStation_HasOneEnabledPlcWithTheSameId()
    {
        var dataset = Dataset();

        foreach (var machine in dataset.Machines)
        {
            var plc = dataset.Plcs.Single(p => p.PlcId == machine.MachineId.Value);
            plc.Enabled.ShouldBe(ActiveStatus.Active);
            plc.MachineId.ShouldBe(machine.MachineId.Value);
            dataset.MachinePlcs.ShouldContain(link => link.MachineId == machine.MachineId && link.PlcId == plc.PlcId);
        }
    }

    [Fact]
    public async Task EveryPlc_IsAcceptedByTheSimulatedController()
    {
        var dataset = Dataset();
        var groups = dataset.VariablesGroups.ToDictionary(g => g.VariableGroupName);

        foreach (var plc in dataset.Plcs)
        {
            var dto = new PlcDto
            {
                PlcId = plc.PlcId,
                MachineId = plc.MachineId,
                Name = plc.Name,
                IpAddress = plc.IpAddress,
                EnableSimulation = true,
                Variables = dataset.Variables.Where(v => v.PlcId == plc.PlcId && v.IsActive == ActiveStatus.Active).ToDictionary(v => v.Name),
                VariablesGroups = groups,
            };
            using var controller = new SimulatedControllerRx(XUnitLogger.CreateLogger<SimulatedControllerRx>(output), dto, new DateTimeMachine());

            (await controller.SetUpAsync(TestContext.Current.CancellationToken)).ShouldBeTrue($"PLC {plc.PlcId}");
            dto.Variables.Values.Count(v => v.VariableGroupId == TagsGroups.EventTags).ShouldBe(DemoDataset.EventTagNames.Count);
        }
    }

    [Fact]
    public void TagGroupCatalog_CoversEveryGroupTheTagsUse()
    {
        var dataset = Dataset();
        var catalog = dataset.VariablesGroups.Select(g => g.VariableGroupId).ToHashSet();

        dataset.Variables.ShouldAllBe(v => catalog.Contains(v.VariableGroupId));
    }

    [Fact]
    public void Routes_AreStoredWithoutZeroEndpointRows()
    {
        Dataset().WorkFlows.ShouldAllBe(w => w.LastMachineId.Value > 0 && w.NextMachineId.Value > 0);
    }

    [Fact]
    public void EveryProduct_HasALinearRouteWithOneFirstAndOneLastStation()
    {
        var dataset = Dataset();

        foreach (var product in dataset.Products)
        {
            var nodes = dataset.RoutingNodes.Where(n => n.ProductId == product.ProductId.Value).ToList();
            var edges = dataset.WorkFlows.Where(w => w.ProductId == product.ProductId.Value).ToList();

            nodes.Count.ShouldBeGreaterThan(1);
            edges.Count.ShouldBe(nodes.Count - 1);
            nodes.Count(n => (n.RoleValue & WorkFlowType.Initial.Value) != 0).ShouldBe(1);
            nodes.Count(n => (n.RoleValue & WorkFlowType.Final.Value) != 0).ShouldBe(1);
            dataset.Recipes.Count(r => r.ProductId == product.ProductId.Value).ShouldBe(nodes.Count);
            dataset.Rules.ShouldContain(r => r.ProductId == product.ProductId && r.MachineId.Value == 100 && r.IsActive);
            dataset.MasterLabels.ShouldContain(m => m.MasterLabelCode.Contains(product.PartNumber));
        }
    }

    [Fact]
    public void RouteAuthoringRule_Exists_ButIsNeverUsedForLabels()
    {
        var dataset = Dataset();
        var routingRuleId = new IndTrace.Application.Configuration.RoutingAuthoringOptions().RoutingRuleId;

        var rule = dataset.Rules.Single(r => r.RuleId == routingRuleId);

        rule.IsActive.ShouldBeFalse();
        dataset.WorkFlows.ShouldAllBe(w => w.RuleId == routingRuleId);
    }

    [Fact]
    public void DemoProduct_RunsStationsOneToFiveInOrder()
    {
        var dataset = Dataset();
        var product = dataset.Products.Single(p => p.PartNumber == DemoDataset.DemoPartNumber);

        dataset.WorkFlows
            .Where(w => w.ProductId == product.ProductId.Value)
            .Select(w => (w.LastMachineId.Value, w.NextMachineId.Value))
            .ShouldBe([(100, 200), (200, 300), (300, 400), (400, 500)]);
    }

    [Fact]
    public void PartNumbers_AreUnique_AndEachCustomerHasAtMostOneProduct()
    {
        var dataset = Dataset();

        dataset.Products.Select(p => p.PartNumber).ShouldBeUnique();
        dataset.Products.Select(p => p.CustomerId).ShouldBeUnique();
        var free = dataset.Customers.Single(c => c.Name == DemoDataset.CustomerWithoutProduct);
        dataset.Products.ShouldNotContain(p => p.CustomerId == free.CustomerId);
    }

    [Fact]
    public void EveryAuditedRow_IsStamped()
    {
        Dataset().All().OfType<AuditableEntity>().ShouldAllBe(row => row.CreatedBy == DemoDataset.AuditUser && row.CreatedOn != null);
    }
}
