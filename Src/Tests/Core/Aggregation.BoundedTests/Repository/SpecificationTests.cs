// <copyright file="SpecificationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

//[Fix]
//CLAUDE
//Date: 27/08/2025
//Reason: Converted to comprehensive aggregation tests using real repositories and cross-boundary testing

using IndTrace.Domain.ValueObjects;

namespace IndTrace.Aggregation.BoundedTests.Repository;

//[Fix]
//CLAUDE
//Date: 27/08/2025
//Reason: [Pattern 1] - Added ITestOutputHelper parameter to constructor and passed to base class

/// <summary>
/// Aggregation tests for the Specification pattern using real repositories and entities.
/// </summary>
/// <remarks>
/// Initializes a new instance of the class.
/// </remarks>
public class SpecificationTests(ITestOutputHelper outputHelper) : DependenciesFactory(outputHelper)
{
    /// <summary>
    /// Executes Constructor_WithValidCriteria_ShouldCreateSpecification operation.
    /// </summary>
    [Fact]
    public void Constructor_WithValidCriteria_ShouldCreateSpecification()
    {
        // Arrange
        Expression<Func<BarCode, bool>> criteria = e => e.BarCodeId.Value > 0;

        // Act
        var specification = new Specification<BarCode>(criteria);

        // Assert
        specification.ShouldNotBeNull();
        specification.Criteria.ShouldBe(criteria);
    }

    /// <summary>
    /// Executes Constructor_WithNullCriteria_ShouldCreateSpecification operation.
    /// </summary>
    [Fact]
    public void Constructor_WithNullCriteria_ShouldCreateSpecification()
    {
        // Arrange
        Expression<Func<BarCode, bool>>? criteria = null!;

        // Act
        var specification = new Specification<BarCode>(criteria!);

        // Assert
        specification.ShouldNotBeNull();
        specification.Criteria.ShouldBe(criteria);
    }

    /// <summary>
    /// Executes AddInclude_WithExpression_ShouldAddInclude operation.
    /// </summary>
    [Fact]
    public void AddInclude_WithExpression_ShouldAddInclude()
    {
        // Arrange
        var specification = new Specification<BarCode>(e => e.BarCodeId.Value > 0);
        Expression<Func<BarCode, object>> include = e => e.Label;

        // Act
        var result = specification.AddInclude(include);

        // Assert
        result.ShouldBe(specification);
        specification.Includes.Count.ShouldBe(1);
        specification.Includes[0].ShouldBe(include);
    }

    /// <summary>
    /// Executes AddInclude_WithString_ShouldAddIncludeString operation.
    /// </summary>
    [Fact]
    public void AddInclude_WithString_ShouldAddIncludeString()
    {
        // Arrange
        var specification = new Specification<BarCode>(e => e.BarCodeId.Value > 0);
        var includeString = "Machine";

        // Act
        var result = specification.AddInclude(includeString);

        // Assert
        result.ShouldBe(specification);
        specification.IncludeStrings.Count.ShouldBe(1);
        specification.IncludeStrings[0].ShouldBe(includeString);
    }

    /// <summary>
    /// Executes AddOrderBy_ShouldSetOrderByExpression operation.
    /// </summary>
    [Fact]
    public void AddOrderBy_ShouldSetOrderByExpression()
    {
        // Arrange
        var specification = new Specification<BarCode>(e => e.BarCodeId.Value > 0);
        Expression<Func<BarCode, object>> orderBy = e => e.Label;

        // Act
        var result = specification.AddOrderBy(orderBy);

        // Assert
        result.ShouldBe(specification);
        specification.OrderBy.ShouldBe(orderBy);
    }

    /// <summary>
    /// Executes AddOrderByDescending_ShouldSetOrderByDescendingExpression operation.
    /// </summary>
    [Fact]
    public void AddOrderByDescending_ShouldSetOrderByDescendingExpression()
    {
        // Arrange
        var specification = new Specification<BarCode>(e => e.BarCodeId.Value > 0);
        Expression<Func<BarCode, object>> orderByDesc = e => e.Label;

        // Act
        var result = specification.AddOrderByDescending(orderByDesc);

        // Assert
        result.ShouldBe(specification);
        specification.OrderByDescending.ShouldBe(orderByDesc);
    }

    /// <summary>
    /// Executes ApplyPaging_ShouldSetSkipAndTake operation.
    /// </summary>
    [Fact]
    public void ApplyPaging_ShouldSetSkipAndTake()
    {
        // Arrange
        var specification = new Specification<BarCode>(e => e.BarCodeId.Value > 0);
        var skip = 10;
        var take = 20;

        // Act
        var result = specification.ApplyPaging(skip, take);

        // Assert
        result.ShouldBe(specification);
        specification.Skip.ShouldBe(skip);
        specification.Take.ShouldBe(take);
    }

    /// <summary>
    /// Executes ApplyNoTracking_ShouldSetIsTrackingToFalse operation.
    /// </summary>
    [Fact]
    public void ApplyNoTracking_ShouldSetIsTrackingToFalse()
    {
        // Arrange
        var specification = new Specification<BarCode>(e => e.BarCodeId.Value > 0);

        // Act
        var result = specification.ApplyNoTracking();

        // Assert
        result.ShouldBe(specification);
        specification.IsTracking.ShouldBeFalse();
    }

    /// <summary>
    /// Executes Key_ShouldGenerateUniqueKey operation.
    /// </summary>
    [Fact]
    public void Key_ShouldGenerateUniqueKey()
    {
        // Arrange
        var specification = new Specification<BarCode>(e => e.BarCodeId.Value > 0);

        // Act
        var key = specification.Key;

        // Assert
        key.ShouldNotBeNullOrEmpty();
        key.ShouldContain("Type:BarCode");
        key.ShouldContain("Criteria:");
    }

    /// <summary>
    /// Executes Key_WithIncludes_ShouldIncludeInKey operation.
    /// </summary>
    [Fact]
    public void Key_WithIncludes_ShouldIncludeInKey()
    {
        // Arrange
        var specification = new Specification<BarCode>(e => e.BarCodeId.Value > 0)
            .AddInclude(e => e.Label);

        var includeString = "Machine";
        specification.AddInclude(includeString);

        // Act
        var key = specification.Key;

        // Assert
        key.ShouldContain("Includes:");
        key.ShouldContain("IncludeStrings:");
    }

    /// <summary>
    /// Executes Key_WithOrderBy_ShouldIncludeInKey operation.
    /// </summary>
    [Fact]
    public void Key_WithOrderBy_ShouldIncludeInKey()
    {
        // Arrange
        var specification = new Specification<BarCode>(e => e.BarCodeId.Value > 0)
            .AddOrderBy(e => e.Label.Value);

        // Act
        var key = specification.Key;

        // Assert
        key.ShouldContain("OrderBy:");
    }

    /// <summary>
    /// Executes Key_WithOrderByDescending_ShouldIncludeInKey operation.
    /// </summary>
    [Fact]
    public void Key_WithOrderByDescending_ShouldIncludeInKey()
    {
        // Arrange
        var specification = new Specification<BarCode>(e => e.BarCodeId.Value > 0)
            .AddOrderByDescending(e => e.Label);

        // Act
        var key = specification.Key;

        // Assert
        key.ShouldContain("OrderByDescending:");
    }

    /// <summary>
    /// Executes Key_WithPaging_ShouldIncludeInKey operation.
    /// </summary>
    [Fact]
    public void Key_WithPaging_ShouldIncludeInKey()
    {
        // Arrange
        var specification = new Specification<BarCode>(e => e.BarCodeId.Value > 0)
            .ApplyPaging(10, 20);

        // Act
        var key = specification.Key;

        // Assert
        key.ShouldContain("Skip:10");
        key.ShouldContain("Take:20");
    }

    /// <summary>
    /// Executes Key_WithNoTracking_ShouldIncludeInKey operation.
    /// </summary>
    [Fact]
    public void Key_WithNoTracking_ShouldIncludeInKey()
    {
        // Arrange
        var specification = new Specification<BarCode>(e => e.BarCodeId.Value > 0)
            .ApplyNoTracking();

        // Act
        var key = specification.Key;

        // Assert
        key.ShouldContain("NoTracking");
    }

    /// <summary>
    /// Executes Key_ShouldBeConsistentForSameSpecification operation.
    /// </summary>
    [Fact]
    public void Key_ShouldBeConsistentForSameSpecification()
    {
        // Arrange
        var specification1 = new Specification<BarCode>(e => e.BarCodeId.Value > 0)
            .AddInclude(e => e.Label)
            .AddOrderBy(e => e.BarCodeId)
            .ApplyPaging(10, 20);

        var specification2 = new Specification<BarCode>(e => e.BarCodeId.Value > 0)
            .AddInclude(e => e.Label)
            .AddOrderBy(e => e.BarCodeId)
            .ApplyPaging(10, 20);

        // Act
        var key1 = specification1.Key;
        var key2 = specification2.Key;

        // Assert
        key1.ShouldBe(key2);
    }

    /// <summary>
    /// Executes Key_ShouldBeDifferentForDifferentSpecifications operation.
    /// </summary>
    [Fact]
    public void Key_ShouldBeDifferentForDifferentSpecifications()
    {
        // Arrange
        var specification1 = new Specification<BarCode>(e => e.BarCodeId.Value > 0);
        var specification2 = new Specification<BarCode>(e => e.BarCodeId.Value < 0);

        // Act
        var key1 = specification1.Key;
        var key2 = specification2.Key;

        // Assert
        key1.ShouldNotBe(key2);
    }

    /// <summary>
    /// Executes MultipleIncludes_ShouldBeAddedCorrectly operation.
    /// </summary>
    [Fact]
    public void MultipleIncludes_ShouldBeAddedCorrectly()
    {
        // Arrange
        var specification = new Specification<BarCode>(e => e.BarCodeId.Value > 0);

        // Act
        specification.AddInclude(e => e.Label)
                    .AddInclude(e => e.MachineId);

        var includeString1 = "Machine";
        var includeString2 = "Product";
        specification.AddInclude(includeString1)
                    .AddInclude(includeString2);

        // Assert
        specification.Includes.Count.ShouldBe(2);
        specification.IncludeStrings.Count.ShouldBe(2);
    }

    /// <summary>
    /// Executes FluentInterface_ShouldWorkCorrectly operation.
    /// </summary>
    [Fact]
    public void FluentInterface_ShouldWorkCorrectly()
    {
        // Arrange & Act
        var specification = new Specification<BarCode>(e => e.BarCodeId.Value > 0)
            .AddInclude(e => e.Label)
            .AddOrderBy(e => e.BarCodeId)
            .AddOrderByDescending(e => e.Label)
            .ApplyPaging(10, 20)
            .ApplyNoTracking();

        var includeString = "Machine";
        specification.AddInclude(includeString);

        // Assert
        specification.Includes.Count.ShouldBe(1);
        specification.IncludeStrings.Count.ShouldBe(1);
        specification.OrderBy.ShouldNotBeNull();
        specification.OrderByDescending.ShouldNotBeNull();
        specification.Skip.ShouldBe(10);
        specification.Take.ShouldBe(20);
        specification.IsTracking.ShouldBeFalse();
    }

    /// <summary>
    /// Executes Key_ShouldBeCached operation.
    /// </summary>
    [Fact]
    public void Key_ShouldBeCached()
    {
        // Arrange
        var specification = new Specification<BarCode>(e => e.BarCodeId.Value > 0);

        // Act
        var key1 = specification.Key;
        var key2 = specification.Key;

        // Assert
        key1.ShouldBe(key2);
        // The key should be cached, so calling it twice should return the same reference
        ReferenceEquals(key1, key2).ShouldBeTrue();
    }

    /// <summary>
    /// Tests Specification pattern with real BarCode repository - complex criteria with paging.
    /// </summary>
    [Fact]
    public async Task BarCodeRepository_WithComplexSpecification_ShouldFilterAndPageCorrectly()
    {
        // Arrange

        // Create test data first
        var testBarCodes = new List<BarCode>();
        for (int i = 1; i <= 15; i++)
        {
            // Story 6.4: status setters are private set; reach each pair via the builder (assertions unchanged).
            var barCode = new BarCodeBuilder()
                .AtState(
                    i % 2 == 0 ? FlowStatus.Finished : FlowStatus.InProcess,
                    i % 3 == 0 ? PartStatus.NOk : PartStatus.Ok)
                .With(b => { b.Label = BarCodeLabel.FromPersisted($"BC{i:D3}"); b.MachineId = new MachineId(i % 3 + 1); }) // Distributes across machines 1, 2, 3
                .Build();
            testBarCodes.Add(barCode);
        }

        // Add test data to repository
        foreach (var barCode in testBarCodes)
        {
            await DpBarCodeRepository.AddAsync(barCode, TestContext.Current.CancellationToken);
        }
        await DpBarCodeRepository.CommitAsync(TestContext.Current.CancellationToken);

        // Act & Assert - Test complex specification with filtering, ordering, and paging
        var specification = new Specification<BarCode>(bc => bc.MachineId.Value <= 2 && bc.FlowStatus == FlowStatus.Finished)
            .AddOrderBy(bc => bc.Label.Value)
            .ApplyPaging(1, 3); // Skip first, take 3

        var result = await DpBarCodeRepository.ListAsync(specification, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldNotBeNull();
        var barCodes = result.Value.ToList();
        barCodes.Count.ShouldBeLessThanOrEqualTo(3);
        barCodes.ShouldAllBe(bc => bc.MachineId.Value <= 2 && bc.FlowStatus == FlowStatus.Finished);
    }

    /// <summary>
    /// Tests Specification pattern with real Machine repository - includes and complex filtering.
    /// </summary>
    [Fact]
    public async Task MachineRepository_WithSpecification_ShouldFilterAndPage()
    {
        // Arrange

        // Use existing machines from test data or find available ones
        var allMachines = await DpRoMachineRepository.ListAsync(TestContext.Current.CancellationToken);
        allMachines.IsSuccess.ShouldBeTrue();

        allMachines.Value.ShouldNotBeNull();
        if (allMachines.Value.Any())
        {
            // Act - Test specification with includes (if supported by the entity)
            var specification = new Specification<Machine>(m => m.EnableAppTraceability == 1 || m.EnableBypassTraceability != 1)
                .AddOrderBy(m => m.Name)
                .ApplyPaging(0, 5);

            var result = await DpRoMachineRepository.ListAsync(specification, TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value.Count().ShouldBeLessThanOrEqualTo(5);
            result.Value.ShouldAllBe(m => m.EnableAppTraceability == 1 || m.EnableBypassTraceability != 1);
        }
    }

    /// <summary>
    /// Tests Specification pattern across multiple repositories - aggregation test.
    /// </summary>
    [Fact]
    public async Task CrossRepository_WithSpecifications_ShouldMaintainDataIntegrity()
    {
        // Arrange

        // Get a machine to work with
        var machineResult = await DpRoMachineRepository.FirstOrDefaultAsync(TestContext.Current.CancellationToken);
        machineResult.IsSuccess.ShouldBeTrue();
        machineResult.Value.ShouldNotBeNull();
        var machine = machineResult.Value;

        // Create a BarCode for this machine
        var barCode = new BarCodeBuilder()
            .InProcess(PartStatus.Ok)
            .With(b =>
            {
                b.BarCodeId = new BarCodeId(0);
                b.ProductId = new ProductId(0);
                b.Label = BarCodeLabel.FromPersisted("AGGREGATION_TEST_001");
                b.MachineId = machine.MachineId;
            })
            .Build();

        await DpBarCodeRepository.AddAsync(barCode, TestContext.Current.CancellationToken);
        await DpBarCodeRepository.CommitAsync(TestContext.Current.CancellationToken);

        // Act - Use specifications to query across repositories
        var machineSpec = new Specification<Machine>(m => m.MachineId == machine.MachineId);
        var barCodeSpec = new Specification<BarCode>(bc => bc.MachineId == machine.MachineId && bc.Label.Value.Contains("AGGREGATION"));

        var machineQueryResult = await DpRoMachineRepository.ListAsync(machineSpec, TestContext.Current.CancellationToken);
        var barCodeQueryResult = await DpBarCodeRepository.ListAsync(barCodeSpec, TestContext.Current.CancellationToken);

        // Assert - Verify cross-repository data integrity
        machineQueryResult.IsSuccess.ShouldBeTrue();
        barCodeQueryResult.IsSuccess.ShouldBeTrue();

        machineQueryResult.Value.ShouldNotBeNull();
        barCodeQueryResult.Value.ShouldNotBeNull();
        var foundMachine = machineQueryResult.Value.Single();
        var foundBarCode = barCodeQueryResult.Value.Single();

        foundMachine.MachineId.ShouldBe(foundBarCode.MachineId);
        foundBarCode.Label.Value.ShouldBe("AGGREGATION_TEST_001");
    }

    /// <summary>
    /// Tests Specification pattern with complex AND/OR logic using real repositories.
    /// </summary>
    [Fact]
    public async Task Repository_WithCombinedSpecifications_ShouldApplyComplexLogic()
    {
        // Arrange

        // Create test BarCodes with different statuses
        var testBarCodes = new[]
        {
            new BarCodeBuilder().Finished(PartStatus.Ok).With(b => { b.BarCodeId = new BarCodeId(0); b.ProductId = new ProductId(0); b.Label = BarCodeLabel.FromPersisted("AGG_SPEC_FIN_001"); b.MachineId = new MachineId(500); }).Build(),
            new BarCodeBuilder().Finished(PartStatus.Ok).With(b => { b.BarCodeId = new BarCodeId(0); b.ProductId = new ProductId(0); b.Label = BarCodeLabel.FromPersisted("AGG_SPEC_FIN_002"); b.MachineId = new MachineId(500); }).Build(),
            new BarCodeBuilder().InProcess(PartStatus.Ok).With(b => { b.BarCodeId = new BarCodeId(0); b.ProductId = new ProductId(0); b.Label = BarCodeLabel.FromPersisted("AGG_SPEC_INP_003"); b.MachineId = new MachineId(500); }).Build()
        };

        foreach (var barCode in testBarCodes)
        {
            await DpBarCodeRepository.AddAsync(barCode, TestContext.Current.CancellationToken);
        }
        await DpBarCodeRepository.CommitAsync(TestContext.Current.CancellationToken);

        // Act - Combine specifications with AND logic
        var finishedFlowSpec = new Specification<BarCode>(bc => bc.FlowStatus == FlowStatus.Finished);
        var machine500Spec = new Specification<BarCode>(bc => bc.MachineId == new MachineId(500));
        var labelPrefixSpec = new Specification<BarCode>(bc => bc.Label.Value.StartsWith("AGG_SPEC_"));
        var combinedSpec = finishedFlowSpec.And(machine500Spec).And(labelPrefixSpec);

        var result = await DpBarCodeRepository.ListAsync(combinedSpec, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        var barCodes = result.Value.ToList();
        barCodes.Count.ShouldBe(2);
        barCodes.ShouldAllBe(bc => bc.FlowStatus == FlowStatus.Finished && bc.MachineId == new MachineId(500) && bc.Label.Value.StartsWith("AGG_SPEC_"));
    }

    /// <summary>
    /// Tests Specification caching behavior with real repository calls.
    /// </summary>
    [Fact]
    public async Task Repository_WithSameSpecification_ShouldUseCaching()
    {
        // Arrange

        var spec1 = new Specification<BarCode>(bc => bc.FlowStatus == FlowStatus.Finished)
            .AddOrderBy(bc => bc.Label.Value);

        var spec2 = new Specification<BarCode>(bc => bc.FlowStatus == FlowStatus.Finished)
            .AddOrderBy(bc => bc.Label.Value);

        // Act - Call repository with equivalent specifications
        var result1 = await DpBarCodeRepository.ListAsync(spec1, TestContext.Current.CancellationToken);
        var result2 = await DpBarCodeRepository.ListAsync(spec2, TestContext.Current.CancellationToken);

        // Assert - Keys should be identical for caching
        spec1.Key.ShouldBe(spec2.Key);
        result1.IsSuccess.ShouldBeTrue();
        result2.IsSuccess.ShouldBeTrue();

        // Results should be equivalent (same data)
        result1.Value.ShouldNotBeNull();
        result2.Value.ShouldNotBeNull();
        if (result1.Value.Any() && result2.Value.Any())
        {
            result1.Value.Count().ShouldBe(result2.Value.Count());
        }
    }
}