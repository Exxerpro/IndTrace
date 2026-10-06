// <copyright file="WorkFlowDtoTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.WorkFlows;

/// <summary>
/// Unit tests for WorkFlowDto
/// </summary>
public class WorkFlowDtoTests
{
    /// <summary>
    /// Executes Constructor_WithDefaultConstructor_ShouldCreateInstanceWithDefaultValues operation.
    /// </summary>
    [Fact]
    public void Constructor_WithDefaultConstructor_ShouldCreateInstanceWithDefaultValues()
    {
        // Act
        var workFlowDto = new WorkFlowDto();

        // Assert
        workFlowDto.ShouldNotBeNull();
        workFlowDto.WorkFlowId.ShouldBe(0);
        workFlowDto.ProductId.ShouldBe(0);
        workFlowDto.NextMachineId.ShouldBe(0);
        workFlowDto.LastMachineId.ShouldBe(0);
        workFlowDto.RuleId.ShouldBe(0);
        workFlowDto.Machine.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Properties_WhenSet_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSet_ShouldReturnCorrectValues()
    {
        // Arrange
        var machines = new List<MachineDto>
        {
            new() { MachineId = 100001, Name = "CNC Mill A" },
            new() { MachineId = 100002, Name = "CNC Mill B" }
        };

        // Act (#225: init-only properties - construction via object initializer)
        var workFlowDto = new WorkFlowDto
        {
            WorkFlowId = 12345,
            ProductId = 67890,
            NextMachineId = 100002,
            LastMachineId = 100001,
            RuleId = 2005,
            Machine = machines,
        };

        // Assert
        workFlowDto.WorkFlowId.ShouldBe(12345);
        workFlowDto.ProductId.ShouldBe(67890);
        workFlowDto.NextMachineId.ShouldBe(100002);
        workFlowDto.LastMachineId.ShouldBe(100001);
        workFlowDto.RuleId.ShouldBe(2005);
        workFlowDto.Machine.ShouldBeSameAs(machines);
        workFlowDto.Machine.Count.ShouldBe(2);
    }

    /// <summary>
    /// Executes Machine_Collection_ShouldBeInitializedAndMutable operation.
    /// </summary>

    [Fact]
    public void Machine_Collection_ShouldBeInitializedAndMutable()
    {
        // Arrange (#225: the property is init-only; the list instance itself stays a List)
        var machine = new MachineDto { MachineId = 5001, Name = "Assembly Station" };
        var workFlowDto = new WorkFlowDto { Machine = [] };

        // Act
        workFlowDto.Machine.Add(machine);

        // Assert
        workFlowDto.Machine.Count.ShouldBe(1);
        workFlowDto.Machine[0].ShouldBeSameAs(machine);
        workFlowDto.Machine[0].MachineId.ShouldBe(5001);
        workFlowDto.Machine[0].Name.ShouldBe("Assembly Station");
    }

    // ToDto Static Method Tests
    /// <summary>
    /// Executes ToDto_WithNullWorkFlow_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToDto_WithNullWorkFlow_ShouldReturnFailureResult()
    {
        // Arrange
        WorkFlow? nullWorkFlow = null!;

        // Act
        var result = WorkFlowDto.ToDto(nullWorkFlow!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("Parameter 'src' cannot be null");
    }

    /// <summary>
    /// Executes ToDto_WithValidWorkFlow_ShouldMapAllProperties operation.
    /// </summary>

    [Fact]
    public void ToDto_WithValidWorkFlow_ShouldMapAllProperties()
    {
        // Arrange
        var machines = new List<Machine>
        {
            new() { MachineId = new MachineId(3001), Name = "Injection Molding A" },
            new() { MachineId = new MachineId(3002), Name = "Quality Inspection B" }
        };

        var workFlow = new WorkFlow
        {
            WorkFlowId = 98765,
            ProductId = 54321,
            NextMachineId = new MachineId(3002),
            LastMachineId = new MachineId(3001),
            RuleId = 3000,
            Machine = machines
        };

        // Act
        var dtoWrapper = WorkFlowDto.ToDto(workFlow);

        // Assert
        dtoWrapper.IsSuccess.ShouldBeTrue();
        dtoWrapper.Value.ShouldNotBeNull();
        var dto = dtoWrapper.Value;
        dto.ShouldNotBeNull();
        dto.WorkFlowId.ShouldBe(98765);
        dto.ProductId.ShouldBe(54321);
        dto.NextMachineId.ShouldBe(3002);
        dto.LastMachineId.ShouldBe(3001);
        dto.RuleId.ShouldBe(3000);

        // #225 detachment regression: the DTO must NOT alias the entity's live machine list
        // (the cached ApplicationConfiguration serves this DTO by reference for 60 minutes).
        dto.Machine.ShouldNotBeSameAs(machines);
        dto.Machine.Count.ShouldBe(2);

        // Mutating the entity's list after mapping must not change the DTO's view.
        machines.Add(new Machine { MachineId = new MachineId(3003), Name = "Late Arrival" });
        dto.Machine.Count.ShouldBe(2);
    }

    /// <summary>
    /// #225 regression: element-level detachment — mutating a source entity after mapping must not
    /// be visible through the DTO's machine collection.
    /// </summary>
    [Fact]
    public void ToDto_AfterEntityMutation_ShouldKeepDetachedMachineView()
    {
        // Arrange
        var machines = new List<Machine>
        {
            new() { MachineId = new MachineId(3101), Name = "Original Station" },
        };

        var workFlow = new WorkFlow
        {
            WorkFlowId = 1,
            ProductId = 2,
            NextMachineId = new MachineId(3101),
            LastMachineId = new MachineId(0),
            Machine = machines,
        };

        // Act
        var dtoWrapper = WorkFlowDto.ToDto(workFlow);
        dtoWrapper.IsSuccess.ShouldBeTrue();
        var dto = dtoWrapper.Value.ShouldNotBeNull();

        machines[0].Name = "MUTATED AFTER MAPPING";
        machines.Add(new Machine { MachineId = new MachineId(3102), Name = "Late Arrival" });

        // Assert - the DTO's view is a detached snapshot taken at mapping time
        dto.Machine.Count.ShouldBe(1);
        dto.Machine[0].Name.ShouldBe("Original Station");
    }

    /// <summary>
    /// Executes ToDto_WithMinimalWorkFlow_ShouldMapBasicProperties operation.
    /// </summary>

    [Fact]
    public void ToDto_WithMinimalWorkFlow_ShouldMapBasicProperties()
    {
        // Arrange
        var workFlow = new WorkFlow
        {
            WorkFlowId = 1,
            ProductId = 5080,
            NextMachineId = new MachineId(2001),
            LastMachineId = new MachineId(2000)
        };

        // Act
        var dtoWrapper = WorkFlowDto.ToDto(workFlow);

        // Assert
        dtoWrapper.IsSuccess.ShouldBeTrue();
        dtoWrapper.Value.ShouldNotBeNull();
        var dto = dtoWrapper.Value;
        dto.ShouldNotBeNull();
        dto.WorkFlowId.ShouldBe(1);
        dto.ProductId.ShouldBe(5080);
        dto.NextMachineId.ShouldBe(2001);
        dto.LastMachineId.ShouldBe(2000);
        dto.RuleId.ShouldBe(0);
        dto.Machine.ShouldNotBeNull().ShouldBeEmpty(); // From entity = []
    }

    /// <summary>
    /// Executes ToDto_WithNullCollections_ShouldHandleGracefully operation.
    /// </summary>

    [Fact]
    public void ToDto_WithDefaultCollections_ShouldMapToEmptyList()
    {
        // Arrange (#225: WorkFlow.Machine defaults to an empty list; the DTO maps it to a
        // detached empty list instead of aliasing / propagating the entity collection)
        var workFlow = new WorkFlow
        {
            WorkFlowId = 123,
            ProductId = 456,
        };

        // Act
        var dtoWrapper = WorkFlowDto.ToDto(workFlow);

        // Assert
        dtoWrapper.IsSuccess.ShouldBeTrue();
        dtoWrapper.Value.ShouldNotBeNull();
        var dto = dtoWrapper.Value;
        dto.ShouldNotBeNull();
        dto.WorkFlowId.ShouldBe(123);
        dto.ProductId.ShouldBe(456);
        dto.Machine.ShouldNotBeNull();
        dto.Machine.ShouldBeEmpty();
        dto.Machine.ShouldNotBeSameAs(workFlow.Machine);
    }

    // ToEntity Static Method Tests
    /// <summary>
    /// Executes ToEntity_WithNullWorkFlowDto_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithNullWorkFlowDto_ShouldReturnFailureResult()
    {
        // Arrange
        WorkFlowDto? nullDto = null!;

        // Act
        var result = WorkFlowDto.ToEntity(nullDto!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldContain("Parameter 'src' cannot be null");
    }

    /// <summary>
    /// Executes ToEntity_WithValidWorkFlowDto_ShouldMapAllProperties operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithValidWorkFlowDto_ShouldMapAllProperties()
    {
        // Arrange
        var machines = new List<MachineDto>
        {
            new() { MachineId = 4001, Name = "Welding Station A" },
            new() { MachineId = 4002, Name = "Paint Booth B" }
        };

        var dto = new WorkFlowDto
        {
            WorkFlowId = 11111,
            ProductId = 22222,
            NextMachineId = 4002,
            LastMachineId = 4001,
            RuleId = 4000,
            Machine = machines
        };

        // Act
        var entityWrapper = WorkFlowDto.ToEntity(dto);

        // Assert
        entityWrapper.IsSuccess.ShouldBeTrue();
        entityWrapper.Value.ShouldNotBeNull();
        var entity = entityWrapper.Value;
        entity.ShouldNotBeNull();
        entity.WorkFlowId.ShouldBe(11111);
        entity.ProductId.ShouldBe(22222);
        entity.NextMachineId.Value.ShouldBe(4002);
        entity.LastMachineId.Value.ShouldBe(4001);
        entity.RuleId.ShouldBe(4000);

        // #225 detachment: the entity rebuilds fresh Machine instances from the DTO list.
        entity.Machine.ShouldNotBeSameAs(machines);
        entity.Machine.Count.ShouldBe(2);
        entity.Machine[0].MachineId.Value.ShouldBe(4001);
        entity.Machine[0].Name.ShouldBe("Welding Station A");
        entity.Machine[1].MachineId.Value.ShouldBe(4002);
        entity.Machine[1].Name.ShouldBe("Paint Booth B");
    }

    /// <summary>
    /// Executes ToEntity_WithMinimalWorkFlowDto_ShouldMapBasicProperties operation.
    /// </summary>

    [Fact]
    public void ToEntity_WithMinimalWorkFlowDto_ShouldMapBasicProperties()
    {
        // Arrange
        var dto = new WorkFlowDto
        {
            WorkFlowId = 999,
            ProductId = 888,
            NextMachineId = 777,
            LastMachineId = 666
        };

        // Act
        var entityWrapper = WorkFlowDto.ToEntity(dto);

        // Assert
        entityWrapper.IsSuccess.ShouldBeTrue();
        entityWrapper.Value.ShouldNotBeNull();
        var entity = entityWrapper.Value;
        entity.ShouldNotBeNull();
        entity.WorkFlowId.ShouldBe(999);
        entity.ProductId.ShouldBe(888);
        entity.NextMachineId.Value.ShouldBe(777);
        entity.LastMachineId.Value.ShouldBe(666);
        entity.RuleId.ShouldBe(0);
        entity.Machine.ShouldNotBeNull(); // DTO initializes empty list
        entity.Machine.ShouldBeEmpty();
    }

    // Round-trip Conversion Tests
    /// <summary>
    /// Executes ToDto_ThenToEntity_ShouldPreserveAllProperties operation.
    /// </summary>

    [Fact]
    public void ToDto_ThenToEntity_ShouldPreserveAllProperties()
    {
        // Arrange
        var originalWorkFlow = new WorkFlow
        {
            WorkFlowId = 55555,
            ProductId = 44444,
            NextMachineId = new MachineId(6002),
            LastMachineId = new MachineId(6001),
            RuleId = 5000,
            Machine =
            [
                new() { MachineId = new MachineId(6001), Name = "Lathe Station" },
                new() { MachineId = new MachineId(6002), Name = "Finishing Station" }
            ]
        };

        // Act
        var dtoWrapper = WorkFlowDto.ToDto(originalWorkFlow);
        dtoWrapper.IsSuccess.ShouldBeTrue();
        dtoWrapper.Value.ShouldNotBeNull();
        var dto = dtoWrapper.Value;

        var convertedEntityWrapper = WorkFlowDto.ToEntity(dto);
        convertedEntityWrapper.IsSuccess.ShouldBeTrue();
        convertedEntityWrapper.Value.ShouldNotBeNull();
        var convertedEntity = convertedEntityWrapper.Value;
        convertedEntity.ShouldNotBeNull();

        // Assert
        convertedEntity.WorkFlowId.ShouldBe(originalWorkFlow.WorkFlowId);
        convertedEntity.ProductId.ShouldBe(originalWorkFlow.ProductId);
        convertedEntity.NextMachineId.ShouldBe(originalWorkFlow.NextMachineId);
        convertedEntity.LastMachineId.ShouldBe(originalWorkFlow.LastMachineId);
        convertedEntity.RuleId.ShouldBe(originalWorkFlow.RuleId);

        // #225 detachment regression: the round-tripped entity rebuilds its own machine list
        // instead of aliasing the original entity's collection through the DTO.
        convertedEntity.Machine.ShouldNotBeSameAs(originalWorkFlow.Machine);
        convertedEntity.Machine.Count.ShouldBe(originalWorkFlow.Machine.Count);
    }

    // Industrial Scenario Tests
    /// <summary>
    /// Executes WorkFlowDto_WithAutomotiveManufacturingScenario_ShouldHandleComplexWorkflow operation.
    /// </summary>

    [Fact]
    public void WorkFlowDto_WithAutomotiveManufacturingScenario_ShouldHandleComplexWorkflow()
    {
        // Arrange & Act - Automotive assembly line workflow (#225: object-initializer construction)
        var workFlowDto = new WorkFlowDto
        {
            WorkFlowId = 1001,
            ProductId = 501234, // Engine block product
            LastMachineId = 0, // Start of workflow
            NextMachineId = 7001, // First CNC machine
            RuleId = 2005,
            Machine =
            [
                new() { MachineId = 7001, Name = "CNC Rough Milling Station", MachineType = 8 }, // Process
                new() { MachineId = 7002, Name = "CNC Precision Boring Machine", MachineType = 8 }, // Process
                new() { MachineId = 7003, Name = "Surface Honing Station", MachineType = 8 }, // Process
                new() { MachineId = 7004, Name = "Quality Inspection CMM", MachineType = 32 } // Inspection
            ],
        };

        // Assert - Verify automotive workflow
        workFlowDto.WorkFlowId.ShouldBe(1001);
        workFlowDto.ProductId.ShouldBe(501234);
        workFlowDto.Machine.Count.ShouldBe(4);
        workFlowDto.Machine[0].Name.ShouldBe("CNC Rough Milling Station");
        workFlowDto.Machine[3].MachineType.Value.ShouldBe(32); // Inspection type
        workFlowDto.RuleId.ShouldBe(2005);
    }

    /// <summary>
    /// Executes WorkFlowDto_WithElectronicsManufacturingScenario_ShouldHandleComplexWorkflow operation.
    /// </summary>

    [Fact]
    public void WorkFlowDto_WithElectronicsManufacturingScenario_ShouldHandleComplexWorkflow()
    {
        // Arrange & Act - Electronics PCB assembly workflow (#225: object-initializer construction)
        var workFlowDto = new WorkFlowDto
        {
            WorkFlowId = 2001,
            ProductId = 700456, // PCB product
            LastMachineId = 0, // Start of workflow
            NextMachineId = 8001, // First SMT machine
            RuleId = 2005,
            Machine =
            [
                new() { MachineId = 8001, Name = "SMT Pick & Place Line 1", MachineType = 8 }, // Process
                new() { MachineId = 8002, Name = "Reflow Oven Station", MachineType = 8 }, // Process
                new() { MachineId = 8003, Name = "ICT Testing Station", MachineType = 32 }, // Inspection
                new() { MachineId = 8004, Name = "Final Assembly & Packaging", MachineType = 16 } // Final
            ],
        };

        // Assert - Verify electronics workflow
        workFlowDto.WorkFlowId.ShouldBe(2001);
        workFlowDto.ProductId.ShouldBe(700456);
        workFlowDto.Machine.Count.ShouldBe(4);
        workFlowDto.Machine[0].Name.ShouldBe("SMT Pick & Place Line 1");
        workFlowDto.Machine[1].MachineType.Value.ShouldBe(8); // Process type
        workFlowDto.RuleId.ShouldBe(2005);
    }

    /// <summary>
    /// Executes WorkFlowDto_WithLinearWorkflow_ShouldMaintainSequentialOrder operation.
    /// </summary>

    [Fact]
    public void WorkFlowDto_WithLinearWorkflow_ShouldMaintainSequentialOrder()
    {
        // Arrange & Act - Linear production workflow (#225: object-initializer construction)
        var workFlowDto = new WorkFlowDto
        {
            WorkFlowId = 3001,
            ProductId = 12345,
            LastMachineId = 9001,
            NextMachineId = 9002,
            RuleId = 2005,
            Machine =
            [
                new() { MachineId = 9001, Name = "Input Station", MachineType = 2 }, // Initial
                new() { MachineId = 9002, Name = "Process Station A", MachineType = 8 }, // Process
                new() { MachineId = 9003, Name = "Process Station B", MachineType = 8 }, // Process
                new() { MachineId = 9004, Name = "Output Station", MachineType = 16 } // Final
            ],
        };

        // Assert - Verify linear workflow
        workFlowDto.Machine.Count.ShouldBe(4);
        workFlowDto.Machine.ShouldAllBe(m => m.MachineType > 0);
        workFlowDto.Machine.OrderBy(m => m.MachineId).First().Name.ShouldBe("Input Station");
        workFlowDto.Machine.OrderBy(m => m.MachineId).Last().Name.ShouldBe("Output Station");
    }
}
