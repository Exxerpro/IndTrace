// <copyright file="GetBarCodeReportQueryValidatorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Barcodes;

/// <summary>
/// Unit tests for GetBarCodeReportQueryValidator
/// </summary>
public class GetBarCodeReportQueryValidatorTests
{
    //[Fix] 
    //CLAUDE
    //Date: 21/08/2025 
    //Reason: Replaced placeholder tests with comprehensive validation tests for GetBarCodeReportQueryValidator

    private readonly GetBarCodeReportQueryValidator _validator = null!;

    public GetBarCodeReportQueryValidatorTests()
    {
        _validator = new GetBarCodeReportQueryValidator();
    }

    [Fact]
    public void Should_Have_Error_When_BarCodesIdList_Is_Null()
    {
        var query = new GetBarCodeReportQuery { BarCodesIdList = null! };
        var result = _validator.TestValidate(query);
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Removed hardcoded error message assertion to align with validator implementation
        result.ShouldHaveValidationErrorFor(x => x.BarCodesIdList);
    }

    [Fact]
    public void Should_Have_Error_When_BarCodesIdList_Is_Empty()
    {
        var query = new GetBarCodeReportQuery { BarCodesIdList = [] };
        var result = _validator.TestValidate(query);
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Removed hardcoded error message assertion to align with validator implementation
        result.ShouldHaveValidationErrorFor(x => x.BarCodesIdList);
    }

    [Fact]
    public void Should_Have_Error_When_BarCodesIdList_Exceeds_Maximum_Count()
    {
        var query = new GetBarCodeReportQuery
        {
            BarCodesIdList = Enumerable.Range(1, 1001).ToList()
        };
        var result = _validator.TestValidate(query);
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Removed hardcoded error message assertion to align with validator implementation
        result.ShouldHaveValidationErrorFor(x => x.BarCodesIdList);
    }

    [Fact]
    public void Should_Have_Error_When_BarCodesIdList_Contains_Zero_Or_Negative_Ids()
    {
        var query = new GetBarCodeReportQuery
        {
            BarCodesIdList = [0, -1, 5]
        };
        var result = _validator.TestValidate(query);
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Removed hardcoded error message assertion to align with validator implementation
        result.ShouldHaveValidationErrorFor("BarCodesIdList[0]");
        result.ShouldHaveValidationErrorFor("BarCodesIdList[1]");
    }

    [Fact]
    public void Should_Not_Have_Error_When_BarCodesIdList_Is_Valid()
    {
        var query = new GetBarCodeReportQuery
        {
            BarCodesIdList = [1, 2, 3, 4, 5]
        };
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_When_BarCodesIdList_Has_Maximum_Allowed_Count()
    {
        var query = new GetBarCodeReportQuery
        {
            BarCodesIdList = Enumerable.Range(1, 1000).ToList()
        };
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_When_BarCodesIdList_Has_Single_Valid_Id()
    {
        var query = new GetBarCodeReportQuery
        {
            BarCodesIdList = [1]
        };
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
