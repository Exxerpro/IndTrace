// <copyright file="GetBarCodeReportQueryTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Queries;

/// <summary>
/// Unit tests for GetBarCodeReportQuery
/// </summary>
public class GetBarCodeReportQueryTests
{
    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var barCodesIdList = new List<int> { 1, 2, 3 };

        // Act
        var instance = new GetBarCodeReportQuery
        {
            BarCodesIdList = barCodesIdList
        };

        // Assert
        instance.ShouldNotBeNull();
        instance.BarCodesIdList.ShouldBe(barCodesIdList);
    }

    ///// <summary>
    ///// Executes Constructor_WithInvalidParameters_ShouldThrowException operation.
    ///// </summary>

    //[Fact]
    //public void Constructor_WithInvalidParameters_ShouldThrowException()
    //{
    //    // Arrange
    //    // Invalid parameter: null BarCodesIdList

    //    // Act & Assert
    //    Should.Throw<ArgumentNullException>(() =>
    //    {
    //        var instance = new GetBarCodeReportQuery
    //        {
    //            BarCodesIdList = null
    //        };
    //    });
    //}
    /// <summary>
    /// Executes Properties_WhenSet_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSet_ShouldReturnCorrectValues()
    {
        // Arrange
        var barCodesIdList = new List<int> { 4, 5, 6 };
        var instance = new GetBarCodeReportQuery
        {
            BarCodesIdList = barCodesIdList
        };

        // Act & Assert
        instance.BarCodesIdList.ShouldBe(barCodesIdList);
    }

    /// <summary>
    /// Executes BarCodesIdList_WithEmptyList_ShouldAcceptEmptyCollection operation.
    /// </summary>

    [Fact]
    public void BarCodesIdList_WithEmptyList_ShouldAcceptEmptyCollection()
    {
        // Arrange
        var emptyList = new List<int>();
        var instance = new GetBarCodeReportQuery();

        // Act
        instance.BarCodesIdList = emptyList;

        // Assert
        instance.BarCodesIdList.ShouldNotBeNull();
        instance.BarCodesIdList.ShouldBeEmpty();
    }

    /// <summary>
    /// Executes BarCodesIdList_WithManufacturingScenarios_ShouldStoreCorrectly operation.
    /// </summary>

    [Fact]
    public void BarCodesIdList_WithManufacturingScenarios_ShouldStoreCorrectly()
    {
        // Test multiple scenarios
        var testCases = new[]
        {
            new { BarCodeIds = new int[] { 1001, 1002, 1003 }, Scenario = "Production Line A BarCodes" },
            new { BarCodeIds = new int[] { 2001, 2002, 2003, 2004 }, Scenario = "Quality Control BarCodes" },
            new { BarCodeIds = new int[] { 3001 }, Scenario = "Single Assembly BarCode" },
            new { BarCodeIds = new int[] { 4001, 4002, 4003, 4004, 4005, 4006 }, Scenario = "Packaging Line BarCodes" }
        };

        foreach (var testCase in testCases)
        {
            TestManufacturingScenario(testCase.BarCodeIds, testCase.Scenario);
        }
    }

    private void TestManufacturingScenario(int[] barCodeIds, string scenario)
    {
        // Arrange
        var barCodesList = barCodeIds.ToList();
        var instance = new GetBarCodeReportQuery();

        // Act
        instance.BarCodesIdList = barCodesList;

        // Assert
        instance.BarCodesIdList.ShouldBe(barCodesList);
        instance.BarCodesIdList.Count.ShouldBe(barCodeIds.Length);
        foreach (var id in barCodeIds)
        {
            instance.BarCodesIdList.ShouldContain(id);
        }
    }

    /// <summary>
    /// Executes BarCodesIdList_WithDuplicateIds_ShouldAllowDuplicates operation.
    /// </summary>

    [Fact]
    public void BarCodesIdList_WithDuplicateIds_ShouldAllowDuplicates()
    {
        // Arrange
        var duplicateList = new List<int> { 1001, 1001, 1002, 1002, 1003 };
        var instance = new GetBarCodeReportQuery();

        // Act
        instance.BarCodesIdList = duplicateList;

        // Assert
        instance.BarCodesIdList.ShouldBe(duplicateList);
        instance.BarCodesIdList.Count.ShouldBe(5);
        instance.BarCodesIdList.Count(x => x == 1001).ShouldBe(2);
    }

    /// <summary>
    /// Executes BarCodesIdList_WithLargeList_ShouldHandleBigCollections operation.
    /// </summary>

    [Fact]
    public void BarCodesIdList_WithLargeList_ShouldHandleBigCollections()
    {
        // Arrange
        var largeList = Enumerable.Range(1, 10000).ToList();
        var instance = new GetBarCodeReportQuery();

        // Act
        instance.BarCodesIdList = largeList;

        // Assert
        instance.BarCodesIdList.ShouldBe(largeList);
        instance.BarCodesIdList.Count.ShouldBe(10000);
        instance.BarCodesIdList.First().ShouldBe(1);
        instance.BarCodesIdList.Last().ShouldBe(10000);
    }

    /// <summary>
    /// Executes BarCodesIdList_WithNegativeIds_ShouldAcceptNegativeValues operation.
    /// </summary>

    [Fact]
    public void BarCodesIdList_WithNegativeIds_ShouldAcceptNegativeValues()
    {
        // Arrange
        var negativeList = new List<int> { -1, -100, -999, 0, 1 };
        var instance = new GetBarCodeReportQuery();

        // Act
        instance.BarCodesIdList = negativeList;

        // Assert
        instance.BarCodesIdList.ShouldBe(negativeList);
        instance.BarCodesIdList.ShouldContain(-1);
        instance.BarCodesIdList.ShouldContain(0);
        instance.BarCodesIdList.ShouldContain(1);
    }

    /// <summary>
    /// Executes BarCodesIdList_WithMaxIntegerValues_ShouldHandleEdgeCases operation.
    /// </summary>

    [Fact]
    public void BarCodesIdList_WithMaxIntegerValues_ShouldHandleEdgeCases()
    {
        // Arrange
        var edgeCaseList = new List<int> { int.MinValue, int.MaxValue, 0 };
        var instance = new GetBarCodeReportQuery();

        // Act
        instance.BarCodesIdList = edgeCaseList;

        // Assert
        instance.BarCodesIdList.ShouldBe(edgeCaseList);
        instance.BarCodesIdList.ShouldContain(int.MinValue);
        instance.BarCodesIdList.ShouldContain(int.MaxValue);
        instance.BarCodesIdList.ShouldContain(0);
    }

    /// <summary>
    /// Executes QueryObject_ShouldImplementIMonitorRequest operation.
    /// </summary>

    [Fact]
    public void QueryObject_ShouldImplementIMonitorRequest()
    {
        // Arrange & Act
        var instance = new GetBarCodeReportQuery();

        // Assert
        instance.ShouldBeAssignableTo<IMonitorRequest<List<BarCodeReportVm>>>();
    }

    /// <summary>
    /// Executes PropertyAssignment_WithMultipleAssignments_ShouldOverwritePrevious operation.
    /// </summary>

    [Fact]
    public void PropertyAssignment_WithMultipleAssignments_ShouldOverwritePrevious()
    {
        // Arrange
        var firstList = new List<int> { 1, 2, 3 };
        var secondList = new List<int> { 4, 5, 6 };
        var instance = new GetBarCodeReportQuery();

        // Act
        instance.BarCodesIdList = firstList;
        var firstResult = instance.BarCodesIdList;

        instance.BarCodesIdList = secondList;
        var secondResult = instance.BarCodesIdList;

        // Assert
        firstResult.ShouldBe(firstList);
        secondResult.ShouldBe(secondList);
        instance.BarCodesIdList.ShouldBe(secondList);
        instance.BarCodesIdList.ShouldNotBe(firstList);
    }

    /// <summary>
    /// Executes BarCodesIdList_WithAutomotiveManufacturingIds_ShouldHandleRealWorldScenarios operation.
    /// </summary>

    [Fact]
    public void BarCodesIdList_WithAutomotiveManufacturingIds_ShouldHandleRealWorldScenarios()
    {
        // Arrange - Simulating automotive part barcodes for Ford F-150 assembly
        var automotiveBarCodes = new List<int>
        {
            100001, // Engine Block
            100002, // Transmission
            100003, // Chassis Frame
            100004, // Dashboard Assembly
            100005, // Brake System
            100006  // Electrical Harness
        };
        var instance = new GetBarCodeReportQuery();

        // Act
        instance.BarCodesIdList = automotiveBarCodes;

        // Assert
        instance.BarCodesIdList.ShouldBe(automotiveBarCodes);
        instance.BarCodesIdList.Count.ShouldBe(6);
        instance.BarCodesIdList.All(id => id >= 100000).ShouldBeTrue();
    }

    /// <summary>
    /// Executes BarCodesIdList_WithElectronicsManufacturingIds_ShouldHandleTechScenarios operation.
    /// </summary>

    [Fact]
    public void BarCodesIdList_WithElectronicsManufacturingIds_ShouldHandleTechScenarios()
    {
        // Arrange - Simulating electronics manufacturing barcodes for Samsung smartphone
        var electronicsBarCodes = new List<int>
        {
            200001, // PCB Board
            200002, // Display Assembly
            200003, // Battery Pack
            200004, // Camera Module
            200005, // Speaker Assembly
            200006, // SIM Card Tray
            200007  // Protective Case
        };
        var instance = new GetBarCodeReportQuery();

        // Act
        instance.BarCodesIdList = electronicsBarCodes;

        // Assert
        instance.BarCodesIdList.ShouldBe(electronicsBarCodes);
        instance.BarCodesIdList.Count.ShouldBe(7);
        instance.BarCodesIdList.All(id => id >= 200000 && id < 300000).ShouldBeTrue();
    }
}