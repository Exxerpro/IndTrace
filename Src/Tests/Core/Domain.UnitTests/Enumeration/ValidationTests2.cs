// <copyright file="ValidationTests2.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Reflection;
using Meziantou.Extensions.Logging.Xunit.v3;
using Microsoft.Extensions.Logging;

namespace IndTrace.Domain.UnitTests.Enumeration;
/// <summary>
/// Represents the ValidationTests.
/// </summary>

public class ValidationTests2
{
    /// <summary>
    /// Executes ResultValidation_Should_Not_Have_Duplicate_Ids operation.
    /// </summary>
    [Fact]
    public void ResultValidation_Should_Not_Have_Duplicate_Ids()
    {
        // Get all the ResultValidation fields
        var resultValidationFields = typeof(ResultValidation)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.FieldType == typeof(ResultValidation))
            .Select(f => (ResultValidation)f.GetValue(null)!);

        // Extract Ids
        var ids = resultValidationFields.Select(rv => rv.Value).ToList();

        var logger = XUnitLogger.CreateLogger<ValidationTests2>();

        foreach (var res in resultValidationFields)
        {
            logger.LogInformation(" res {name} value {Value}", res.Name, res.Value);
        }

        logger.LogInformation("Ids: {Ids}", string.Join(", ", ids));
        // Check for duplicates
        ids.Distinct().Count().ShouldBe(ids.Count(), "because Names should be uniqueunique in ResultValidation");
    }

    /// <summary>
    /// Executes ResultValidation_Should_Not_Have_Duplicate_Names operation.
    /// </summary>

    [Fact]
    public void ResultValidation_Should_Not_Have_Duplicate_Names()
    {
        // Get all the ResultValidation fields
        var resultValidationFields = typeof(ResultValidation)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.FieldType == typeof(ResultValidation))
            .Select(f => (ResultValidation)f.GetValue(null)!);

        // Extract Names
        var names = resultValidationFields.Select(rv => rv.Name).ToList();

        // Check for duplicates
        names.Distinct().Count().ShouldBe(names.Count(), "because Names should be unique in ResultValidation");
    }

    /// <summary>
    /// Executes GatewayTask_Should_Not_Have_Duplicate_Ids operation.
    /// </summary>

    [Fact]
    public void GatewayTask_Should_Not_Have_Duplicate_Ids()
    {
        // Get all the GatewayTask fields
        var gatewayTaskFields = typeof(GatewayTask)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.FieldType == typeof(GatewayTask))
            .Select(f => (GatewayTask)f.GetValue(null)!);

        // Extract Ids
        var ids = gatewayTaskFields.Select(gt => gt.Value).ToList();

        // Check for duplicates
        ids.Distinct().Count().ShouldBe(ids.Count(), "because Names should be unique in GatewayTask");
    }

    /// <summary>
    /// Executes GatewayTask_Should_Not_Have_Duplicate_Names operation.
    /// </summary>

    [Fact]
    public void GatewayTask_Should_Not_Have_Duplicate_Names()
    {
        // Get all the GatewayTask fields
        var gatewayTaskFields = typeof(GatewayTask)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.FieldType == typeof(GatewayTask))
            .Select(f => (GatewayTask)f.GetValue(null)!);

        // Extract Names
        var names = gatewayTaskFields.Select(gt => gt.Name).ToList();

        // Check for duplicates
        names.Distinct().Count().ShouldBe(names.Count(), "because Names should be unique in GatewayTask");
    }

    /// <summary>
    /// Executes CycleStatus_Should_Not_Have_Duplicate_Names operation.
    /// </summary>

    [Fact]
    public void CycleStatus_Should_Not_Have_Duplicate_Names()
    {
        var cycleStatusFields = typeof(CycleStatus)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.FieldType == typeof(CycleStatus))
            .Select(f => (CycleStatus)f.GetValue(null)!);

        var names = cycleStatusFields.Select(cs => cs.Name).ToList();

        names.Distinct().Count().ShouldBe(names.Count(), "because Names should be unique in CycleStatus");
    }

    /// <summary>
    /// Executes FlowStatus_Should_Not_Have_Duplicate_Names operation.
    /// </summary>

    [Fact]
    public void FlowStatus_Should_Not_Have_Duplicate_Names()
    {
        var flowStatusFields = typeof(FlowStatus)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.FieldType == typeof(FlowStatus))
            .Select(f => (FlowStatus)f.GetValue(null)!);

        var names = flowStatusFields.Select(fs => fs.Name).ToList();

        names.Distinct().Count().ShouldBe(names.Count(), "because Names should be unique in FlowStatus");
    }

    /// <summary>
    /// Executes MachineType_Should_Not_Have_Duplicate_Names operation.
    /// </summary>

    [Fact]
    public void MachineType_Should_Not_Have_Duplicate_Names()
    {
        var machineTypeFields = typeof(MachineType)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.FieldType == typeof(MachineType))
            .Select(f => (MachineType)f.GetValue(null)!);

        var names = machineTypeFields.Select(mt => mt.Name).ToList();

        names.Distinct().Count().ShouldBe(names.Count(), "because Names should be unique in MachineType");
    }

    /// <summary>
    /// Executes PartStatus_Should_Not_Have_Duplicate_Names operation.
    /// </summary>

    [Fact]
    public void PartStatus_Should_Not_Have_Duplicate_Names()
    {
        var partStatusFields = typeof(PartStatus)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.FieldType == typeof(PartStatus))
            .Select(f => (PartStatus)f.GetValue(null)!);

        var names = partStatusFields.Select(ps => ps.Name).ToList();

        names.Distinct().Count().ShouldBe(names.Count(), "because Names should be unique in PartStatus");
    }

    /// <summary>
    /// Executes ShiftType_Should_Not_Have_Duplicate_Names operation.
    /// </summary>

    [Fact]
    public void ShiftType_Should_Not_Have_Duplicate_Names()
    {
        var shiftTypeFields = typeof(ShiftType)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.FieldType == typeof(ShiftType))
            .Select(f => (ShiftType)f.GetValue(null)!);

        var names = shiftTypeFields.Select(st => st.Name).ToList();

        names.Distinct().Count().ShouldBe(names.Count(), "because Names should be unique in ShiftType");
    }

    /// <summary>
    /// Executes WorkFlowType_Should_Not_Have_Duplicate_Names operation.
    /// </summary>

    [Fact]
    public void WorkFlowType_Should_Not_Have_Duplicate_Names()
    {
        var workFlowTypeFields = typeof(WorkFlowType)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.FieldType == typeof(WorkFlowType))
            .Select(f => (WorkFlowType)f.GetValue(null)!);

        var names = workFlowTypeFields.Select(wt => wt.Name).ToList();

        names.Distinct().Count().ShouldBe(names.Count(), "because Names should be unique in WorkFlowType");
    }

    /// <summary>
    /// Executes TagsGroupsEnum_Should_Not_Have_Duplicate_Names operation.
    /// </summary>

    [Fact]
    public void TagsGroupsEnum_Should_Not_Have_Duplicate_Names()
    {
        var tagsGroupsEnumFields = typeof(TagsGroups)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.FieldType == typeof(TagsGroups))
            .Select(f => (TagsGroups)f.GetValue(null)!);

        var names = tagsGroupsEnumFields.Select(tg => tg.Name).ToList();

        names.Distinct().Count().ShouldBe(names.Count(), "because Names should be unique in TagsGroupsEnum");
    }

    /// <summary>
    /// Executes CycleStatus_Should_Not_Have_Duplicate_Values operation.
    /// </summary>

    [Fact]
    public void CycleStatus_Should_Not_Have_Duplicate_Values()
    {
        var cycleStatusFields = typeof(CycleStatus)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.FieldType == typeof(CycleStatus))
            .Select(f => (CycleStatus)f.GetValue(null)!);

        var values = cycleStatusFields.Select(cs => cs.Value).ToList();

        values.Distinct().Count().ShouldBe(values.Count(), "because Names should be unique CycleStatus");
    }

    /// <summary>
    /// Executes FlowStatus_Should_Not_Have_Duplicate_Values operation.
    /// </summary>

    [Fact]
    public void FlowStatus_Should_Not_Have_Duplicate_Values()
    {
        var flowStatusFields = typeof(FlowStatus)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.FieldType == typeof(FlowStatus))
            .Select(f => (FlowStatus)f.GetValue(null)!);

        var values = flowStatusFields.Select(fs => fs.Value).ToList();

        values.Distinct().Count().ShouldBe(values.Count(), "because Names should be uniquee in FlowStatus");
    }

    /// <summary>
    /// Executes MachineType_Should_Not_Have_Duplicate_Values operation.
    /// </summary>

    [Fact]
    public void MachineType_Should_Not_Have_Duplicate_Values()
    {
        var machineTypeFields = typeof(MachineType)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.FieldType == typeof(MachineType))
            .Select(f => (MachineType)f.GetValue(null)!);

        var values = machineTypeFields.Select(mt => mt.Value).ToList();

        values.Distinct().Count().ShouldBe(values.Count(), "because Names should be unique in MachineType");
    }

    /// <summary>
    /// Executes PartStatus_Should_Not_Have_Duplicate_Values operation.
    /// </summary>
    [Fact]
    public void PartStatus_Should_Not_Have_Duplicate_Values()
    {
        var partStatusFields = typeof(PartStatus)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.FieldType == typeof(PartStatus))
            .Select(f => (PartStatus)f.GetValue(null)!);

        var values = partStatusFields.Select(ps => ps.Value).ToList();

        values.Distinct().Count().ShouldBe(values.Count(), "because Names should be unique in PartStatus");
    }

    /// <summary>
    /// Executes ShiftType_Should_Not_Have_Duplicate_Values operation.
    /// </summary>
    [Fact]
    public void ShiftType_Should_Not_Have_Duplicate_Values()
    {
        var shiftTypeFields = typeof(ShiftType)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.FieldType == typeof(ShiftType))
            .Select(f => (ShiftType)f.GetValue(null)!);

        var values = shiftTypeFields.Select(st => st.Value).ToList();

        values.Distinct().Count().ShouldBe(values.Count(), "because Names should be unique in ShiftType");
    }

    /// <summary>
    /// Executes WorkFlowType_Should_Not_Have_Duplicate_Values operation.
    /// </summary>

    [Fact]
    public void WorkFlowType_Should_Not_Have_Duplicate_Values()
    {
        var workFlowTypeFields = typeof(WorkFlowType)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.FieldType == typeof(WorkFlowType))
            .Select(f => (WorkFlowType)f.GetValue(null)!);

        var values = workFlowTypeFields.Select(wt => wt.Value).ToList();

        values.Distinct().Count().ShouldBe(values.Count(), "because Names should be unique in WorkFlowType");
    }

    /// <summary>
    /// Executes TagsGroupsEnum_Should_Not_Have_Duplicate_Values operation.
    /// </summary>
    [Fact]
    public void TagsGroupsEnum_Should_Not_Have_Duplicate_Values()
    {
        var tagsGroupsEnumFields = typeof(TagsGroups)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.FieldType == typeof(TagsGroups))
            .Select(f => (TagsGroups)f.GetValue(null)!);

        var values = tagsGroupsEnumFields.Select(tg => tg.Value).ToList();

        values.Distinct().Count().ShouldBe(values.Count(), "because Names should be unique in TagsGroupsEnum");
    }
}