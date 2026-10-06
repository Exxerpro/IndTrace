// <copyright file="GetBarCodeDetailMonitorQueryValidatorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using MonitorValidator = IndTrace.Application.BarCodes.Queries.GetBarCodeDetailMonitor.GetBarCodeDetailQueryValidator;

namespace Application.UnitTests.Features.Barcodes;

/// <summary>
/// Unit tests for GetBarCodeDetailQueryValidator for Monitor queries
/// </summary>
public class GetBarCodeDetailMonitorQueryValidatorTests
{
    //[Fix] 
    //CLAUDE
    //Date: 21/08/2025 
    //Reason: Created comprehensive validation tests for GetBarCodeDetailMonitor QueryValidator with either/or validation logic

    private readonly MonitorValidator _validator = null!;

    public GetBarCodeDetailMonitorQueryValidatorTests()
    {
        _validator = new MonitorValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Both_BarCode_And_BarCodeId_Are_Empty()
    {
        var query = new GetBarCodeDetailMonitorQuery();
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("Either BarCode or BarCodeId must be provided.");
    }

    [Fact]
    public void Should_Have_Error_When_BarCode_Is_Whitespace_And_BarCodeId_Is_Zero()
    {
        var query = new GetBarCodeDetailMonitorQuery
        {
            BarCode = "   ",
            BarCodeId = 0
        };
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("Either BarCode or BarCodeId must be provided.");
    }

    [Fact]
    public void Should_Have_Error_When_BarCode_Is_Too_Long()
    {
        var longBarCode = new string('A', 101);
        var query = new GetBarCodeDetailMonitorQuery { BarCode = longBarCode };
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.BarCode)
            .WithErrorMessage("BarCode must be between 1 and 100 characters when provided.");
    }

    [Fact]
    public void Should_Have_Error_When_BarCode_Has_Invalid_Characters()
    {
        var query = new GetBarCodeDetailMonitorQuery { BarCode = "ABC@123!" };
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.BarCode)
            .WithErrorMessage("BarCode can only contain alphanumeric characters, hyphens, underscores, and forward slashes.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Valid_BarCode_Is_Provided()
    {
        var query = new GetBarCodeDetailMonitorQuery { BarCode = "ABC-123_456/789" };
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_When_Valid_BarCodeId_Is_Provided()
    {
        var query = new GetBarCodeDetailMonitorQuery { BarCodeId = 123 };
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_When_Both_Valid_BarCode_And_BarCodeId_Are_Provided()
    {
        var query = new GetBarCodeDetailMonitorQuery
        {
            BarCode = "ABC-123",
            BarCodeId = 456
        };
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_When_BarCode_Is_Single_Character()
    {
        var query = new GetBarCodeDetailMonitorQuery { BarCode = "A" };
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_When_BarCode_Is_Maximum_Length()
    {
        var maxBarCode = new string('A', 100);
        var query = new GetBarCodeDetailMonitorQuery { BarCode = maxBarCode };
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }
}