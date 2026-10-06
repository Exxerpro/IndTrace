// <copyright file="GetConfigStationListQueryValidatorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.ConfigStations;

/// <summary>
/// Unit tests for GetConfigStationListQueryValidator
/// </summary>
public class GetConfigStationListQueryValidatorTests
{
    private readonly GetConfigStationListQueryValidator _validator = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public GetConfigStationListQueryValidatorTests()
    {
        _validator = new GetConfigStationListQueryValidator();
    }

    /// <summary>
    /// Executes Constructor_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Arrange & Act
        var validator = new GetConfigStationListQueryValidator();

        // Assert
        validator.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Validate_WithValidQuery_ShouldPass operation.
    /// </summary>

    [Fact]
    public void Validate_WithValidQuery_ShouldPass()
    {
        // Arrange
        var query = CreateValidQuery();

        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes Validate_WithShortPartNumber_ShouldFail operation.
    /// </summary>
    /// <param name="partNumber">The partNumber.</param>

    [Theory]
    [InlineData("")]          // Too short
    [InlineData("A")]         // Too short
    [InlineData("AB")]        // Too short
    [InlineData("ABC")]       // Too short (3 chars)
    public void Validate_WithShortPartNumber_ShouldFail(string partNumber)
    {
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Arrange
        var query = CreateValidQuery();
        query.PartNumber = partNumber;

        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PartNumber);
    }

    /// <summary>
    /// Executes Validate_WithLongPartNumber_ShouldFail operation.
    /// </summary>
    /// <param name="partNumber">The partNumber.</param>

    [Theory]
    [InlineData("ABCDEFGHIJ")]     // Too long (10 chars)
    [InlineData("ABCDEFGHIJK")]    // Too long (11 chars)
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZ")] // Much too long
    public void Validate_WithLongPartNumber_ShouldFail(string partNumber)
    {
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Arrange
        var query = CreateValidQuery();
        query.PartNumber = partNumber;

        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PartNumber);
    }

    /// <summary>
    /// Executes Validate_WithValidPartNumber_ShouldPass operation.
    /// </summary>
    /// <param name="partNumber">The partNumber.</param>

    [Theory]
    [InlineData("ABCD")]       // Exactly 4 chars (minimum)
    [InlineData("ABCDE")]      // 5 chars
    [InlineData("ABCDEF")]     // 6 chars
    [InlineData("ABCDEFG")]    // 7 chars
    [InlineData("ABCDEFGH")]   // 8 chars
    [InlineData("ABCDEFGHI")]  // Exactly 9 chars (maximum)
    public void Validate_WithValidPartNumber_ShouldPass(string partNumber)
    {
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Arrange
        var query = CreateValidQuery();
        query.PartNumber = partNumber;

        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes Validate_WithMinimumLengthPartNumber_ShouldPass operation.
    /// </summary>

    [Fact]
    public void Validate_WithMinimumLengthPartNumber_ShouldPass()
    {
        // Arrange
        var query = CreateValidQuery();
        query.PartNumber = "PART"; // Exactly 4 characters (minimum)

        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
        query.PartNumber.Length.ShouldBe(4);
    }

    /// <summary>
    /// Executes Validate_WithMaximumLengthPartNumber_ShouldPass operation.
    /// </summary>

    [Fact]
    public void Validate_WithMaximumLengthPartNumber_ShouldPass()
    {
        // Arrange
        var query = CreateValidQuery();
        query.PartNumber = "PARTNUM01"; // Exactly 9 characters (maximum)

        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
        query.PartNumber.Length.ShouldBe(9);
    }

    /// <summary>
    /// Executes Validate_WithNullPartNumber_ShouldPass operation.
    /// </summary>

    [Fact]
    public void Validate_WithNullPartNumber_ShouldPass()
    {
        // Arrange - PartNumber is nullable, so null should be valid
        var query = CreateValidQuery();
        query.PartNumber = null!;

        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        // Act
        var result = _validator.TestValidate(query);

        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Fixed test assertion - null PartNumber should pass validation (nullable field), test name indicates ShouldPass
        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes Validate_WithVariousValidFormats_ShouldPass operation.
    /// </summary>
    /// <param name="partNumber">The partNumber.</param>

    [Theory]
    [InlineData("1234")]       // All numbers
    [InlineData("PART")]       // All letters UperCase
    [InlineData("P123")]       // Mixed alphanumeric
    [InlineData("P_RT")]       // With underscore
    [InlineData("P-RT")]       // With hyphen
    [InlineData("P.RT")]       // With period
    [InlineData("part")]       // Lowercase
    [InlineData("Part")]       // CammelCase
    public void Validate_WithVariousValidFormats_ShouldPass(string partNumber)
    {
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Arrange
        var query = CreateValidQuery();
        query.PartNumber = partNumber;

        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.IsValid.ShouldBeTrue($"PartNumber '{partNumber}' should be valid");
        result.Errors.ShouldBeEmpty();
        query.PartNumber.Length.ShouldBeGreaterThanOrEqualTo(4);
        query.PartNumber.Length.ShouldBeLessThanOrEqualTo(9);
    }

    /// <summary>
    /// Executes Validate_WithBoundaryPartNumbers_ShouldPass operation.
    /// </summary>
    /// <param name="partNumber">The partNumber.</param>

    [Theory]
    [InlineData("COMPONENT")] // 9 chars - at maximum boundary
    [InlineData("PART_NUM")]  // 8 chars - within range
    [InlineData("PART123")]   // 7 chars - within range
    [InlineData("PARTNO")]    // 6 chars - within range
    [InlineData("PARTS")]     // 5 chars - within range
    [InlineData("PART")]      // 4 chars - at minimum boundary
    public void Validate_WithBoundaryPartNumbers_ShouldPass(string partNumber)
    {
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Using parameters: partNumber
        _ = partNumber; // xUnit1026 fix
        // Arrange
        var query = CreateValidQuery();
        query.PartNumber = partNumber;

        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.IsValid.ShouldBeTrue($"PartNumber '{partNumber}' (length {partNumber.Length}) should be valid");
        result.Errors.ShouldBeEmpty();
    }

    /// <summary>
    /// Executes ValidateAsync_WithValidQuery_ShouldPass operation.
    /// </summary>
    /// <returns>The result of ValidateAsync_WithValidQuery_ShouldPass.</returns>

    [Fact]
    public async Task ValidateAsync_WithValidQuery_ShouldPass()
    {
        // Arrange
        var query = CreateValidQuery();

        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        // Act
        var result = await _validator.TestValidateAsync(query, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes ValidateAsync_WithInvalidQuery_ShouldFail operation.
    /// </summary>
    /// <returns>The result of ValidateAsync_WithInvalidQuery_ShouldFail.</returns>

    [Fact]
    public async Task ValidateAsync_WithInvalidQuery_ShouldFail()
    {
        // Arrange
        var query = new GetConfigStationListQuery
        {
            PartNumber = "AB" // Too short - should fail
        };

        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        // Act
        var result = await _validator.TestValidateAsync(query, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PartNumber);
    }

    /// <summary>
    /// Executes ValidateAsync_WithCancellationToken_ShouldRespectCancellation operation.
    /// </summary>
    /// <returns>The result of ValidateAsync_WithCancellationToken_ShouldRespectCancellation.</returns>

    [Fact]
    public async Task ValidateAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var query = CreateValidQuery();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await _validator.ValidateAsync(query, cts.Token));
    }

    /// <summary>
    /// Executes Validate_WithMemberDataValidPartNumbers_ShouldPass operation.
    /// </summary>
    /// <param name="partNumber">The partNumber.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [MemberData(nameof(GetValidPartNumberTestCases))]
    public void Validate_WithMemberDataValidPartNumbers_ShouldPass(string partNumber, string description)
    {
        // Using parameters: partNumber, description
        _ = partNumber; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: partNumber, description
        _ = partNumber; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: partNumber, description
        _ = partNumber; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: partNumber, description
        _ = partNumber; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: partNumber, description
        _ = partNumber; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Arrange
        var query = CreateValidQuery();
        query.PartNumber = partNumber;

        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        // Act
        var result = _validator.TestValidate(query);

        // Assert
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        result.ShouldNotHaveAnyValidationErrors();
        if (partNumber != null)
        {
            partNumber.Length.ShouldBeInRange(4, 9);
        }
    }

    /// <summary>
    /// Executes GetValidPartNumberTestCases operation.
    /// </summary>
    /// <returns>The result of GetValidPartNumberTestCases.</returns>

    public static IEnumerable<object[]> GetValidPartNumberTestCases()
    {
        //  yield return new object[] { null, "Null value (optional field)" };
        yield return new object[] { "PART", "Minimum length - 4 characters" };
        yield return new object[] { "PART1", "5 characters" };
        yield return new object[] { "PART12", "6 characters" };
        yield return new object[] { "PART123", "7 characters" };
        yield return new object[] { "PART1234", "8 characters" };
        yield return new object[] { "PART12345", "Maximum length - 9 characters" };
        yield return new object[] { "1234", "All numeric" };
        yield return new object[] { "P_RT", "With underscore" };
        yield return new object[] { "P-RT", "With hyphen" };
        yield return new object[] { "part", "Lowercase" };
    }

    /// <summary>
    /// Executes Validate_WithMemberDataInvalidPartNumbers_ShouldFail operation.
    /// </summary>
    /// <param name="partNumber">The partNumber.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [MemberData(nameof(GetInvalidPartNumberTestCases))]
    public void Validate_WithMemberDataInvalidPartNumbers_ShouldFail(string partNumber, string description)
    {
        // Using parameters: partNumber, description
        _ = partNumber; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: partNumber, description
        _ = partNumber; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: partNumber, description
        _ = partNumber; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: partNumber, description
        _ = partNumber; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: partNumber, description
        _ = partNumber; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Arrange
        var query = CreateValidQuery();
        query.PartNumber = partNumber;

        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        // Act
        var result = _validator.TestValidate(query);

        // Assert
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        result.ShouldHaveValidationErrorFor(x => x.PartNumber);
    }

    /// <summary>
    /// Executes GetInvalidPartNumberTestCases operation.
    /// </summary>
    /// <returns>The result of GetInvalidPartNumberTestCases.</returns>

    public static IEnumerable<object[]> GetInvalidPartNumberTestCases()
    {
        yield return new object[] { "", "Empty string" };
        yield return new object[] { "A", "Too short - 1 character" };
        yield return new object[] { "AB", "Too short - 2 characters" };
        yield return new object[] { "ABC", "Too short - 3 characters" };
        yield return new object[] { "ABCDEFGHIJ", "Too long - 10 characters" };
        yield return new object[] { "ABCDEFGHIJK", "Too long - 11 characters" };
        yield return new object[] { "VERYLONGPARTNUMBER", "Much too long" };
    }

    /// <summary>
    /// Executes Validate_WithVariousLengths_ShouldReturnExpectedResult operation.
    /// </summary>
    /// <param name="length">The length.</param>
    /// <param name="expectedValid">The expectedValid.</param>

    [Theory]
    [InlineData(4, true)]     // Minimum valid length
    [InlineData(5, true)]     // Within range
    [InlineData(6, true)]     // Within range
    [InlineData(7, true)]     // Within range
    [InlineData(8, true)]     // Within range
    [InlineData(9, true)]     // Maximum valid length
    [InlineData(3, false)]    // Too short
    [InlineData(10, false)]   // Too long
    public void Validate_WithVariousLengths_ShouldReturnExpectedResult(int length, bool expectedValid)
    {
        // Using parameters: length, expectedValid
        _ = length; // xUnit1026 fix
        _ = expectedValid; // xUnit1026 fix
        // Using parameters: length, expectedValid
        _ = length; // xUnit1026 fix
        _ = expectedValid; // xUnit1026 fix
        // Using parameters: length, expectedValid
        _ = length; // xUnit1026 fix
        _ = expectedValid; // xUnit1026 fix
        // Using parameters: length, expectedValid
        _ = length; // xUnit1026 fix
        _ = expectedValid; // xUnit1026 fix
        // Using parameters: length, expectedValid
        _ = length; // xUnit1026 fix
        _ = expectedValid; // xUnit1026 fix
        // Arrange
        var query = CreateValidQuery();
        query.PartNumber = new string('A', length);

        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        // Act
        var result = _validator.TestValidate(query);

        // Assert
        //[Fix] 
        //CLAUDE
        //Date: 21/08/2025 
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        if (expectedValid)
        {
            result.ShouldNotHaveAnyValidationErrors();
        }
        else
        {
            result.ShouldHaveValidationErrorFor(x => x.PartNumber);
        }
    }

    /// <summary>
    /// Creates a valid GetConfigStationListQuery for testing purposes
    /// </summary>
    private static GetConfigStationListQuery CreateValidQuery()
    {
        return new GetConfigStationListQuery
        {
            PartNumber = "PART123" // Valid 7-character PartNumber
        };
    }
}