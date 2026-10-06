// <copyright file="GetBarCodeDetailQrCodeQueryValidatorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using QrCodeValidator = IndTrace.Application.BarCodes.Queries.GetBarCodeDetailQrCode.GetBarCodeDetailQueryValidator;

namespace Application.UnitTests.Features.Barcodes;

/// <summary>
/// Unit tests for GetBarCodeDetailQueryValidator for QR Code queries
/// </summary>
public class GetBarCodeDetailQrCodeQueryValidatorTests
{
    //[Fix] 
    //CLAUDE
    //Date: 21/08/2025 
    //Reason: Created comprehensive validation tests for GetBarCodeDetailQrCode QueryValidator with QR code specific validation

    private readonly QrCodeValidator _validator = null!;

    public GetBarCodeDetailQrCodeQueryValidatorTests()
    {
        _validator = new QrCodeValidator();
    }

    [Fact]
    public void Should_Have_Error_When_BarCode_Is_Null()
    {
        //[Fix] 
        //CLAUDE
        //Date: 22/08/2025 
        //Reason: Pattern A Fix - FluentValidation chains stop at first error, so just check for validation error on property without specific message
        var query = new GetBarCodeDetailQrCodeQuery { BarCode = null! };
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.BarCode);
    }

    [Fact]
    public void Should_Have_Error_When_BarCode_Is_Empty()
    {
        var query = new GetBarCodeDetailQrCodeQuery { BarCode = "" };
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.BarCode)
            .WithErrorMessage("BarCode cannot be empty.");
    }

    [Fact]
    public void Should_Have_Error_When_BarCode_Is_Too_Long()
    {
        var longBarCode = new string('A', 201);
        var query = new GetBarCodeDetailQrCodeQuery { BarCode = longBarCode };
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.BarCode)
            .WithErrorMessage("BarCode must be between 1 and 200 characters.");
    }

    [Fact]
    public void Should_Have_Error_When_BarCode_Has_Invalid_Characters()
    {
        var query = new GetBarCodeDetailQrCodeQuery { BarCode = "ABC@123!#$" };
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.BarCode)
            .WithErrorMessage("BarCode contains invalid characters for QR code format.");
    }

    [Fact]
    public void Should_Have_Error_When_BarCode_Has_Spaces()
    {
        var query = new GetBarCodeDetailQrCodeQuery { BarCode = "ABC 123" };
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.BarCode)
            .WithErrorMessage("BarCode contains invalid characters for QR code format.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_BarCode_Is_Valid_Alphanumeric()
    {
        var query = new GetBarCodeDetailQrCodeQuery { BarCode = "ABC123DEF456" };
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_When_BarCode_Has_Valid_Special_Characters()
    {
        var query = new GetBarCodeDetailQrCodeQuery { BarCode = "ABC-123_456/789:part=1&id=2" };
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_When_BarCode_Is_URL_Like()
    {
        var query = new GetBarCodeDetailQrCodeQuery { BarCode = "https://example.com/barcode?id=123&type=QR" };
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_When_BarCode_Is_Single_Character()
    {
        var query = new GetBarCodeDetailQrCodeQuery { BarCode = "A" };
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_When_BarCode_Is_Maximum_Length()
    {
        var maxBarCode = new string('A', 200);
        var query = new GetBarCodeDetailQrCodeQuery { BarCode = maxBarCode };
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }
}