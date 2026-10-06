// <copyright file="WorkFlowTypeEntityTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.WorkFlowsTests;

/// <summary>
/// Unit tests for WorkFlowTypeEntity - Manufacturing workflow type lookup entity support
/// </summary>
public class WorkFlowTypeEntityTests
{
    /// <summary>
    /// Executes WorkFlowTypeEntity_WhenDefaultConstructor_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void WorkFlowTypeEntity_WhenDefaultConstructor_ShouldCreateInstance()
    {
        // Arrange & Act
        var workFlowTypeEntity = new WorkFlowTypeEntity();

        // Assert
        workFlowTypeEntity.ShouldNotBeNull();
        workFlowTypeEntity.ShouldBeAssignableTo<EnumLookUpTable>();
        workFlowTypeEntity.Id.ShouldBe(0);
        workFlowTypeEntity.Name.ShouldBe(string.Empty);
        workFlowTypeEntity.DisplayName.ShouldBe(string.Empty);
    }
    /// <summary>
    /// Executes WorkFlowTypeEntity_WhenParameters_ShouldCreateInstanceWithValues operation.
    /// </summary>

    [Fact]
    public void WorkFlowTypeEntity_WhenParameters_ShouldCreateInstanceWithValues()
    {
        // Arrange
        var id = 1;
        var name = "Initial";
        var displayName = "Initial Workflow Stage";

        // Act
        var workFlowTypeEntity = new WorkFlowTypeEntity(id, name, displayName);

        // Assert
        workFlowTypeEntity.ShouldNotBeNull();
        workFlowTypeEntity.Id.ShouldBe(id);
        workFlowTypeEntity.Name.ShouldBe(name);
        workFlowTypeEntity.DisplayName.ShouldBe(displayName);
    }
    /// <summary>
    /// Executes WorkFlowTypeEntity_WhenManufacturingWorkflowTypes_ShouldCreateValidEntities operation.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="name">The name.</param>
    /// <param name="displayName">The displayName.</param>

    [Theory]
    [InlineData(1, "Initial", "Initial manufacturing stage")]
    [InlineData(2, "Serial", "Sequential manufacturing process")]
    [InlineData(4, "Lateral", "Lateral workflow branch")]
    [InlineData(8, "Diverter", "Manufacturing line diverter")]
    [InlineData(16, "Merger", "Manufacturing line merger")]
    [InlineData(32, "Final", "Final manufacturing stage")]
    public void WorkFlowTypeEntity_WhenManufacturingWorkflowTypes_ShouldCreateValidEntities(int id, string name, string displayName)
    {
        // Arrange & Act
        var workFlowTypeEntity = new WorkFlowTypeEntity(id, name, displayName);

        // Assert
        workFlowTypeEntity.ShouldNotBeNull();
        workFlowTypeEntity.Id.ShouldBe(id);
        workFlowTypeEntity.Name.ShouldBe(name);
        workFlowTypeEntity.DisplayName.ShouldBe(displayName);
    }
    /// <summary>
    /// Executes WorkFlowTypeEntityProperties_WhenSetThroughParameterizedConstructor_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void WorkFlowTypeEntityProperties_WhenSetThroughParameterizedConstructor_ShouldReturnCorrectValues()
    {
        // Arrange
        var id = 100;
        var name = "Custom_Workflow";
        var displayName = "Custom Manufacturing Workflow";

        // Act
        var workFlowTypeEntity = new WorkFlowTypeEntity(id, name, displayName);

        // Assert
        workFlowTypeEntity.Id.ShouldBe(id);
        workFlowTypeEntity.Name.ShouldBe(name);
        workFlowTypeEntity.DisplayName.ShouldBe(displayName);
    }
    /// <summary>
    /// Executes WorkFlowTypeEntityProperties_WhenSetAfterDefaultConstruction_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void WorkFlowTypeEntityProperties_WhenSetAfterDefaultConstruction_ShouldReturnCorrectValues()
    {
        // Arrange
        var workFlowTypeEntity = new WorkFlowTypeEntity();
        var id = 200;
        var name = "Modified_Workflow";
        var displayName = "Modified Manufacturing Workflow";

        // Act
        workFlowTypeEntity.Id = id;
        workFlowTypeEntity.Name = name;
        workFlowTypeEntity.DisplayName = displayName;

        // Assert
        workFlowTypeEntity.Id.ShouldBe(id);
        workFlowTypeEntity.Name.ShouldBe(name);
        workFlowTypeEntity.DisplayName.ShouldBe(displayName);
    }
    /// <summary>
    /// Executes WorkFlowTypeEntity_Properties_WithEdgeCaseValues_ShouldAcceptAllValues operation.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="name">The name.</param>
    /// <param name="displayName">The displayName.</param>

    [Theory]
    [InlineData(0, null, null)]
    [InlineData(-1, "", "")]
    [InlineData(int.MaxValue, "MaxValue", "Maximum Value Workflow")]
    [InlineData(int.MinValue, "MinValue", "Minimum Value Workflow")]
    public void WorkFlowTypeEntity_Properties_WithEdgeCaseValues_ShouldAcceptAllValues(int id, string? name, string? displayName)
    {
        // Arrange
        var workFlowTypeEntity = new WorkFlowTypeEntity();

        // Act
        workFlowTypeEntity.Id = id;
        workFlowTypeEntity.Name = name!;
        workFlowTypeEntity.DisplayName = displayName!;

        // Assert
        workFlowTypeEntity.Id.ShouldBe(id);
        workFlowTypeEntity.Name.ShouldBe(name);
        workFlowTypeEntity.DisplayName.ShouldBe(displayName);
    }
    /// <summary>
    /// Executes WorkFlowTypeEntity_DomainLogic_WithFordF150AssemblyWorkflow_ShouldCreateValidWorkflowTypeEntity operation.
    /// </summary>

    [Fact]
    public void WorkFlowTypeEntity_DomainLogic_WithFordF150AssemblyWorkflow_ShouldCreateValidWorkflowTypeEntity()
    {
        // Arrange - Ford F-150 Assembly Line Workflow Types
        var initialStage = new WorkFlowTypeEntity(1, "Ford_Initial", "Ford F-150 Initial Assembly Stage");
        var serialStage = new WorkFlowTypeEntity(2, "Ford_Serial", "Ford F-150 Sequential Assembly Process");
        var finalStage = new WorkFlowTypeEntity(32, "Ford_Final", "Ford F-150 Final Assembly Stage");

        // Assert - Manufacturing workflow progression
        initialStage.ShouldNotBeNull();
        initialStage.Id.ShouldBe(1);
        initialStage.Name.ShouldBe("Ford_Initial");
        initialStage.DisplayName.ShouldBe("Ford F-150 Initial Assembly Stage");

        serialStage.ShouldNotBeNull();
        serialStage.Id.ShouldBe(2);
        serialStage.Name.ShouldBe("Ford_Serial");
        serialStage.DisplayName.ShouldBe("Ford F-150 Sequential Assembly Process");

        finalStage.ShouldNotBeNull();
        finalStage.Id.ShouldBe(32);
        finalStage.Name.ShouldBe("Ford_Final");
        finalStage.DisplayName.ShouldBe("Ford F-150 Final Assembly Stage");
    }
    /// <summary>
    /// Executes WorkFlowTypeEntity_DomainLogic_WithTeslaModelSBatteryWorkflow_ShouldCreateValidWorkflowTypeEntity operation.
    /// </summary>

    [Fact]
    public void WorkFlowTypeEntity_DomainLogic_WithTeslaModelSBatteryWorkflow_ShouldCreateValidWorkflowTypeEntity()
    {
        // Arrange - Tesla Model S Battery Assembly Workflow
        var lateralStage = new WorkFlowTypeEntity(4, "Tesla_Lateral", "Tesla Battery Lateral Workflow");
        var diverterStage = new WorkFlowTypeEntity(8, "Tesla_Diverter", "Tesla Battery Line Diverter");
        var mergerStage = new WorkFlowTypeEntity(16, "Tesla_Merger", "Tesla Battery Line Merger");

        // Assert - Tesla battery manufacturing workflow
        lateralStage.ShouldNotBeNull();
        lateralStage.Id.ShouldBe(4);
        lateralStage.Name.ShouldBe("Tesla_Lateral");
        lateralStage.DisplayName.ShouldBe("Tesla Battery Lateral Workflow");

        diverterStage.ShouldNotBeNull();
        diverterStage.Id.ShouldBe(8);
        diverterStage.Name.ShouldBe("Tesla_Diverter");
        diverterStage.DisplayName.ShouldBe("Tesla Battery Line Diverter");

        mergerStage.ShouldNotBeNull();
        mergerStage.Id.ShouldBe(16);
        mergerStage.Name.ShouldBe("Tesla_Merger");
        mergerStage.DisplayName.ShouldBe("Tesla Battery Line Merger");
    }
    /// <summary>
    /// Executes WorkFlowTypeEntity_DomainLogic_WithBMWX5PaintShopWorkflow_ShouldCreateValidWorkflowTypeEntity operation.
    /// </summary>

    [Fact]
    public void WorkFlowTypeEntity_DomainLogic_WithBMWX5PaintShopWorkflow_ShouldCreateValidWorkflowTypeEntity()
    {
        // Arrange - BMW X5 Paint Shop Workflow
        var paintInitial = new WorkFlowTypeEntity(1, "BMW_Paint_Initial", "BMW X5 Paint Initial Stage");
        var paintProcess = new WorkFlowTypeEntity(2, "BMW_Paint_Process", "BMW X5 Paint Process Stage");
        var paintFinal = new WorkFlowTypeEntity(32, "BMW_Paint_Final", "BMW X5 Paint Final Stage");

        // Assert - BMW paint shop workflow stages
        paintInitial.Id.ShouldBe(1);
        paintInitial.Name.ShouldBe("BMW_Paint_Initial");
        paintInitial.DisplayName.ShouldBe("BMW X5 Paint Initial Stage");

        paintProcess.Id.ShouldBe(2);
        paintProcess.Name.ShouldBe("BMW_Paint_Process");
        paintProcess.DisplayName.ShouldBe("BMW X5 Paint Process Stage");

        paintFinal.Id.ShouldBe(32);
        paintFinal.Name.ShouldBe("BMW_Paint_Final");
        paintFinal.DisplayName.ShouldBe("BMW X5 Paint Final Stage");
    }
    /// <summary>
    /// Executes WorkFlowTypeEntity_DomainLogic_WithMercedesQualityControlWorkflow_ShouldCreateValidWorkflowTypeEntity operation.
    /// </summary>

    [Fact]
    public void WorkFlowTypeEntity_DomainLogic_WithMercedesQualityControlWorkflow_ShouldCreateValidWorkflowTypeEntity()
    {
        // Arrange - Mercedes Quality Control Workflow
        var qcDiverter = new WorkFlowTypeEntity(8, "Mercedes_QC_Diverter", "Mercedes QC Line Diverter");
        var qcMerger = new WorkFlowTypeEntity(16, "Mercedes_QC_Merger", "Mercedes QC Line Merger");

        // Assert - Quality control workflow diverter/merger
        qcDiverter.Id.ShouldBe(8);
        qcDiverter.Name.ShouldBe("Mercedes_QC_Diverter");
        qcDiverter.DisplayName.ShouldBe("Mercedes QC Line Diverter");

        qcMerger.Id.ShouldBe(16);
        qcMerger.Name.ShouldBe("Mercedes_QC_Merger");
        qcMerger.DisplayName.ShouldBe("Mercedes QC Line Merger");

        // Different workflow types should have different IDs
        qcDiverter.Id.ShouldNotBe(qcMerger.Id);
    }
    /// <summary>
    /// Executes WorkFlowTypeEntity_DomainLogic_WithAudiA4WeldingWorkflow_ShouldCreateValidWorkflowTypeEntity operation.
    /// </summary>

    [Fact]
    public void WorkFlowTypeEntity_DomainLogic_WithAudiA4WeldingWorkflow_ShouldCreateValidWorkflowTypeEntity()
    {
        // Arrange - Oemx A4 Welding Process Workflow
        var weldingLateral = new WorkFlowTypeEntity(4, "Audi_Welding_Lateral", "Oemx A4 Welding Lateral Process");

        // Assert - Welding lateral workflow
        weldingLateral.ShouldNotBeNull();
        weldingLateral.Id.ShouldBe(4);
        weldingLateral.Name.ShouldBe("Audi_Welding_Lateral");
        weldingLateral.DisplayName.ShouldBe("Oemx A4 Welding Lateral Process");
    }
    /// <summary>
    /// Executes WorkFlowTypeEntity_DomainLogic_WithIndustry40StampingWorkflow_ShouldCreateValidWorkflowTypeEntities operation.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="name">The name.</param>
    /// <param name="displayName">The displayName.</param>

    [Theory]
    [InlineData(1, "Stamping_Initial", "Metal stamping initial stage")]
    [InlineData(2, "Stamping_Process", "Metal stamping process stage")]
    [InlineData(4, "Stamping_Lateral", "Metal stamping lateral workflow")]
    [InlineData(8, "Stamping_Diverter", "Metal stamping line diverter")]
    [InlineData(16, "Stamping_Merger", "Metal stamping line merger")]
    [InlineData(32, "Stamping_Final", "Metal stamping final stage")]
    public void WorkFlowTypeEntity_DomainLogic_WithIndustry40StampingWorkflow_ShouldCreateValidWorkflowTypeEntities(int id, string name, string displayName)
    {
        // Arrange & Act
        var workflowType = new WorkFlowTypeEntity(id, name, displayName);

        // Assert - Industry 4.0 Manufacturing Workflow Support
        workflowType.ShouldNotBeNull();
        workflowType.Id.ShouldBe(id);
        workflowType.Name.ShouldBe(name);
        workflowType.DisplayName.ShouldBe(displayName);

        // Power-of-two ID validation for bitwise operations
        if (id > 0)
        {
            IsPowerOfTwo(id).ShouldBeTrue();
        }
    }
    /// <summary>
    /// Executes WorkFlowTypeEntity_DomainLogic_AsLookupEntity_ShouldSupportEntityFrameworkConfiguration operation.
    /// </summary>

    [Fact]
    public void WorkFlowTypeEntity_DomainLogic_AsLookupEntity_ShouldSupportEntityFrameworkConfiguration()
    {
        // Arrange - Based on EF Configuration (UserId, Name MaxLength 80, DisplayName MaxLength 80)
        var id = 999;
        var name = new string('N', 80); // Maximum allowed length
        var displayName = new string('D', 80); // Maximum allowed length

        // Act
        var workFlowTypeEntity = new WorkFlowTypeEntity(id, name, displayName);

        // Assert - Database constraints validation
        workFlowTypeEntity.ShouldNotBeNull();
        workFlowTypeEntity.Id.ShouldBe(id);
        workFlowTypeEntity.Name.ShouldBe(name);
        workFlowTypeEntity.Name.Length.ShouldBe(80);
        workFlowTypeEntity.DisplayName.ShouldBe(displayName);
        workFlowTypeEntity.DisplayName.Length.ShouldBe(80);
    }
    /// <summary>
    /// Executes WorkFlowTypeEntity_DomainLogic_WithComplexManufacturingPipeline_ShouldSupportWorkflowOrchestration operation.
    /// </summary>

    [Fact]
    public void WorkFlowTypeEntity_DomainLogic_WithComplexManufacturingPipeline_ShouldSupportWorkflowOrchestration()
    {
        // Arrange - Complex Manufacturing Pipeline (Automotive Assembly)
        var workflows = new[]
        {
            new WorkFlowTypeEntity(1, "Body_Initial", "Vehicle Body Initial Assembly"),
            new WorkFlowTypeEntity(2, "Body_Process", "Vehicle Body Process Assembly"),
            new WorkFlowTypeEntity(4, "Paint_Lateral", "Paint Shop Lateral Process"),
            new WorkFlowTypeEntity(8, "QC_Diverter", "Quality Control Diverter"),
            new WorkFlowTypeEntity(16, "Assembly_Merger", "Final Assembly Merger"),
            new WorkFlowTypeEntity(32, "Final_Inspection", "Final Inspection Stage")
        };

        // Act & Assert - Manufacturing pipeline workflow validation
        workflows.Length.ShouldBe(6);
        workflows.All(w => w != null).ShouldBeTrue();
        workflows.All(w => w.Id > 0).ShouldBeTrue();
        workflows.All(w => !string.IsNullOrEmpty(w.Name)).ShouldBeTrue();
        workflows.All(w => !string.IsNullOrEmpty(w.DisplayName)).ShouldBeTrue();

        // All IDs should be unique
        var uniqueIds = workflows.Select(w => w.Id).Distinct().Count();
        uniqueIds.ShouldBe(workflows.Length);

        // Combined workflow mask for orchestration
        var combinedWorkflowMask = workflows.Aggregate(0, (current, workflow) => current | workflow.Id);
        combinedWorkflowMask.ShouldBe(63); // 1 | 2 | 4 | 8 | 16 | 32 = 63
    }

    /// <summary>
    /// Helper method to verify if a number is a power of two
    /// </summary>
    /// <param name="num">Number to check</param>
    /// <returns>True if number is a power of two</returns>
    private static bool IsPowerOfTwo(int num)
    {
        return num > 0 && (num & (num - 1)) == 0;
    }
}