// <copyright file="ConfigStationDetailVmTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.ConfigStations;

/// <summary>
/// Comprehensive unit tests for ConfigStationDetailVm - Manufacturing station configuration view model
/// </summary>
public class ConfigStationDetailVmTests
{
    /// <summary>
    /// Executes Should_CreateInstance_When_DefaultConstructorCalled operation.
    /// </summary>
    [Fact]
    public void Should_CreateInstance_When_DefaultConstructorCalled()
    {
        // Act
        var viewModel = new ConfigStationDetailVm();

        // Assert
        viewModel.ShouldNotBeNull();
        viewModel.WorkFlow.ShouldNotBeNull().ShouldBeEmpty();
        viewModel.Maquinas.ShouldNotBeNull().ShouldBeEmpty();
        viewModel.PlCs.ShouldNotBeNull().ShouldBeEmpty();
        viewModel.MaquinasPlCs.ShouldNotBeNull().ShouldBeEmpty();
        viewModel.VariablesGroups.ShouldNotBeNull().ShouldBeEmpty();
        viewModel.Variables.ShouldNotBeNull().ShouldBeEmpty();
    }

    /// <summary>
    /// Executes Should_SetWorkFlowProperty_When_ValidWorkFlowCollectionProvided operation.
    /// </summary>

    [Fact]
    public void Should_SetWorkFlowProperty_When_ValidWorkFlowCollectionProvided()
    {
        // Arrange
        var viewModel = new ConfigStationDetailVm();
        var workflows = new List<WorkFlow>
        {
            new() { WorkFlowId = 1, ProductId = 5080, NextMachineId = new MachineId(201), LastMachineId = new MachineId(200), RuleId = 1 },
            new() { WorkFlowId = 2, ProductId = 5081, NextMachineId = new MachineId(301), LastMachineId = new MachineId(300), RuleId = 2 },
            new() { WorkFlowId = 3, ProductId = 5082, NextMachineId = new MachineId(401), LastMachineId = new MachineId(400), RuleId = 3 }
        };

        // Act
        viewModel.WorkFlow = workflows;

        // Assert
        viewModel.WorkFlow.ShouldNotBeNull();
        viewModel.WorkFlow.Count().ShouldBe(3);
        viewModel.WorkFlow.First().WorkFlowId.ShouldBe(1);
        viewModel.WorkFlow.Last().WorkFlowId.ShouldBe(3);
    }

    /// <summary>
    /// Executes Should_ConvertToDto_When_ValidConfigAppProvided operation.
    /// </summary>

    [Fact]
    public void Should_ConvertToDto_When_ValidConfigAppProvided()
    {
        // Arrange
        var configApp = new ConfigApp
        {
            ConfigAppId = 1.ToString(),
            Factory = "Ford Manufacturing Config",
            Client = "F150_Engine_Line_Config",
            Project = "Configuration for Ford F-150 engine assembly line"
        };

        // Act
        var resultWrapper = ConfigStationDetailVm.ToDto(configApp);

        // Assert
        resultWrapper.IsSuccess.ShouldBeTrue();
        resultWrapper.Value.ShouldNotBeNull();
        var result = resultWrapper.Value;
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.ShouldBeOfType<ConfigStationDetailVm>();
    }

    /// <summary>
    /// Returns failure result when ToDto is called with a null source.
    /// </summary>

    [Fact]
    public void Should_ReturnFailureResult_When_NullConfigAppProvidedToToDto()
    {
        // Act
        var result = ConfigStationDetailVm.ToDto(null!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldNotBeEmpty();
    }

    /// <summary>
    /// Executes Should_ConvertToEntity_When_ValidViewModelProvided operation.
    /// </summary>

    [Fact]
    public void Should_ConvertToEntity_When_ValidViewModelProvided()
    {
        // Arrange
        var viewModel = new ConfigStationDetailVm();

        // Act
        var result = ConfigStationDetailVm.ToEntity(viewModel);

        // Assert
        result.Value.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.ShouldBeOfType<Result<ConfigApp>>();
    }

    /// <summary>
    /// Returns failure result when ToEntity is called with a null source.
    /// </summary>

    [Fact]
    public void Should_ReturnFailureResult_When_NullViewModelProvidedToToEntity()
    {
        // Act
        var result = ConfigStationDetailVm.ToEntity(null!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldNotBeEmpty();
    }
}