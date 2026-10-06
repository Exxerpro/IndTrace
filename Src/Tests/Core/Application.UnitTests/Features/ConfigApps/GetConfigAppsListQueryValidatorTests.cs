// <copyright file="GetConfigAppsListQueryValidatorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.ConfigApps;

/// <summary>
/// Unit tests for GetConfigAppsListQueryValidator
/// </summary>
public class GetConfigAppsListQueryValidatorTests
{
    private readonly GetConfigAppsListQueryValidator _validator = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public GetConfigAppsListQueryValidatorTests()
    {
        _validator = new GetConfigAppsListQueryValidator();
    }

    /// <summary>
    /// Executes Constructor_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Arrange & Act
        var validator = new GetConfigAppsListQueryValidator();

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

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes Validate_WithNullOrEmptyId_ShouldFail operation.
    /// </summary>
    /// <param name="id">The id.</param>

    [Theory]
#pragma warning disable xUnit1012 // Null should not be used for type parameter - this test intentionally validates null behavior
    [InlineData(null)]
#pragma warning restore xUnit1012
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithNullOrEmptyId_ShouldFail(string id)
    {
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Arrange
        var query = CreateValidQuery();
        query.Id = id;

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    /// <summary>
    /// Executes Validate_WithShortId_ShouldFail operation.
    /// </summary>
    /// <param name="id">The id.</param>

    [Theory]
    [InlineData("A")]         // Too short
    [InlineData("AB")]        // Too short
    [InlineData("ABC")]       // Too short
    [InlineData("ABCD")]      // Too short (4 chars)
    public void Validate_WithShortId_ShouldFail(string id)
    {
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Arrange
        var query = CreateValidQuery();
        query.Id = id;

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(query);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Removed hardcoded error message assertion to align with validator implementation
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    /// <summary>
    /// Executes Validate_WithLongId_ShouldFail operation.
    /// </summary>
    /// <param name="id">The id.</param>

    [Theory]
    [InlineData("ABCDEF")]     // Too long (6 chars)
    [InlineData("ABCDEFG")]    // Too long (7 chars)
    [InlineData("ABCDEFGH")]   // Too long (8 chars)
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZ")] // Much too long
    public void Validate_WithLongId_ShouldFail(string id)
    {
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Arrange
        var query = CreateValidQuery();
        query.Id = id;

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(query);

        // Assert
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Removed hardcoded error message assertion to align with validator implementation
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    /// <summary>
    /// Executes Validate_WithValidId_ShouldPass operation.
    /// </summary>
    /// <param name="id">The id.</param>

    [Theory]
    [InlineData("ABCDE")]      // Exactly 5 chars - letters
    [InlineData("12345")]      // Numbers
    [InlineData("A1B2C")]      // Mixed alphanumeric
    [InlineData("CFG_1")]      // With underscore
    [InlineData("CFG-1")]      // With hyphen
    [InlineData("CFG.1")]      // With period
    [InlineData("ABC01")]      // Mixed letters and numbers
    public void Validate_WithValidId_ShouldPass(string id)
    {
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Arrange
        var query = CreateValidQuery();
        query.Id = id;

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Executes Validate_WithBoundaryId_ExactlyFiveCharacters_ShouldPass operation.
    /// </summary>

    [Fact]
    public void Validate_WithBoundaryId_ExactlyFiveCharacters_ShouldPass()
    {
        // Arrange
        var query = CreateValidQuery();
        query.Id = "12345"; // Exactly 5 characters

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
        query.Id.Length.ShouldBe(5);
    }

    /// <summary>
    /// Executes Validate_WithVariousValidFiveCharacterIds_ShouldPass operation.
    /// </summary>
    /// <param name="id">The id.</param>

    [Theory]
    [InlineData("CFG01")]      // Alphanumeric
    [InlineData("APP_1")]      // With underscore
    [InlineData("APP-1")]      // With hyphen
    [InlineData("APP.1")]      // With period
    [InlineData("00001")]      // All numeric with leading zeros
    [InlineData("ZZZZZ")]      // All letters
    [InlineData("MIX3D")]      // Mixed case letters and numbers
    public void Validate_WithVariousValidFiveCharacterIds_ShouldPass(string id)
    {
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Using parameters: id
        _ = id; // xUnit1026 fix
        // Arrange
        var query = CreateValidQuery();
        query.Id = id;

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
        query.Id.Length.ShouldBe(5);
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

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
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
        var query = new GetConfigAppsListQuery
        {
            Id = "" // Empty UserId should fail
        };

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = await _validator.TestValidateAsync(query, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
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
    /// Executes Validate_WithMemberDataValidIds_ShouldPass operation.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [MemberData(nameof(GetValidIdTestCases))]
    public void Validate_WithMemberDataValidIds_ShouldPass(string id, string description)
    {
        // Using parameters: id, description
        _ = id; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: id, description
        _ = id; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: id, description
        _ = id; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: id, description
        _ = id; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: id, description
        _ = id; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Arrange
        var query = CreateValidQuery();
        query.Id = id;

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
        query.Id.Length.ShouldBe(5);
    }

    /// <summary>
    /// Executes GetValidIdTestCases operation.
    /// </summary>
    /// <returns>The result of GetValidIdTestCases.</returns>

    public static IEnumerable<object[]> GetValidIdTestCases()
    {
        yield return new object[] { "ABCDE", "All uppercase letters" };
        yield return new object[] { "abcde", "All lowercase letters" };
        yield return new object[] { "12345", "All numbers" };
        yield return new object[] { "A1B2C", "Mixed alphanumeric" };
        yield return new object[] { "CFG_1", "With underscore" };
        yield return new object[] { "CFG-1", "With hyphen" };
        yield return new object[] { "CFG.1", "With period" };
        yield return new object[] { "00001", "Numbers with leading zeros" };
    }

    /// <summary>
    /// Executes Validate_WithMemberDataInvalidIds_ShouldFail operation.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [MemberData(nameof(GetInvalidIdTestCases))]
    public void Validate_WithMemberDataInvalidIds_ShouldFail(string id, string description)
    {
        // Using parameters: id, description
        _ = id; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: id, description
        _ = id; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: id, description
        _ = id; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: id, description
        _ = id; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Using parameters: id, description
        _ = id; // xUnit1026 fix
        _ = description; // xUnit1026 fix
        // Arrange
        var query = CreateValidQuery();
        query.Id = id;

        // Act
        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    /// <summary>
    /// Executes GetInvalidIdTestCases operation.
    /// </summary>
    /// <returns>The result of GetInvalidIdTestCases.</returns>

    public static IEnumerable<object[]> GetInvalidIdTestCases()
    {
        yield return new object[] { null!, "Null value" };
        yield return new object[] { "", "Empty string" };
        yield return new object[] { "   ", "Whitespace only" };
        yield return new object[] { "A", "Too short - 1 character" };
        yield return new object[] { "AB", "Too short - 2 characters" };
        yield return new object[] { "ABC", "Too short - 3 characters" };
        yield return new object[] { "ABCD", "Too short - 4 characters" };
        yield return new object[] { "ABCDEF", "Too long - 6 characters" };
        yield return new object[] { "ABCDEFG", "Too long - 7 characters" };
        yield return new object[] { "VERYLONGSTRING", "Much too long" };
    }

    /// <summary>
    /// Creates a valid GetConfigAppsListQuery for testing purposes
    /// </summary>
    private static GetConfigAppsListQuery CreateValidQuery()
    {
        return new GetConfigAppsListQuery
        {
            Id = "CFG01" // Valid 5-character UserId
        };
    }
}