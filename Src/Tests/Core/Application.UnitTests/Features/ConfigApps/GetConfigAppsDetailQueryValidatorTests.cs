// <copyright file="GetConfigAppsDetailQueryValidatorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.ConfigApps;

/// <summary>
/// Unit tests for GetConfigAppsDetailQueryValidator
/// </summary>
public class GetConfigAppsDetailQueryValidatorTests
{
    private readonly GetConfigAppsDetailQueryValidator _validator = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public GetConfigAppsDetailQueryValidatorTests()
    {
        _validator = new GetConfigAppsDetailQueryValidator();
    }

    /// <summary>
    /// Executes Constructor_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Arrange & Act
        var validator = new GetConfigAppsDetailQueryValidator();

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
    /// Executes Validate_WithInvalidId_ShouldFail operation.
    /// </summary>
    /// <param name="id">The id.</param>

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    public void Validate_WithInvalidId_ShouldFail(int id)
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

        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    /// <summary>
    /// Executes Validate_WithValidId_ShouldPass operation.
    /// </summary>
    /// <param name="id">The id.</param>

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(999)]
    [InlineData(1000)]
    [InlineData(int.MaxValue)]
    public void Validate_WithValidId_ShouldPass(int id)
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
    /// Executes Validate_WithMinimumValidId_ShouldPass operation.
    /// </summary>

    [Fact]
    public void Validate_WithMinimumValidId_ShouldPass()
    {
        // Arrange
        var query = CreateValidQuery();
        query.Id = 1; // Minimum valid value

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
    /// Executes Validate_WithMaximumValidId_ShouldPass operation.
    /// </summary>

    [Fact]
    public void Validate_WithMaximumValidId_ShouldPass()
    {
        // Arrange
        var query = CreateValidQuery();
        query.Id = int.MaxValue; // Maximum valid value

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
    /// Executes Validate_WithZeroId_ShouldFail operation.
    /// </summary>

    [Fact]
    public void Validate_WithZeroId_ShouldFail()
    {
        // Arrange
        var query = CreateValidQuery();
        query.Id = 0; // Zero should fail

        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    /// <summary>
    /// Executes Validate_WithNegativeOneId_ShouldFail operation.
    /// </summary>

    [Fact]
    public void Validate_WithNegativeOneId_ShouldFail()
    {
        // Arrange
        var query = CreateValidQuery();
        query.Id = -1; // Negative one should fail

        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
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
        var query = new GetConfigAppsDetailQuery
        {
            Id = 0 // Zero should fail
        };

        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        // Act
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
    public void Validate_WithMemberDataValidIds_ShouldPass(int id, string description)
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
    /// Executes GetValidIdTestCases operation.
    /// </summary>
    /// <returns>The result of GetValidIdTestCases.</returns>

    public static IEnumerable<object[]> GetValidIdTestCases()
    {
        yield return new object[] { 1, "Minimum valid value" };
        yield return new object[] { 2, "Small positive value" };
        yield return new object[] { 42, "Typical positive value" };
        yield return new object[] { 100, "Hundred" };
        yield return new object[] { 999, "Three-digit value" };
        yield return new object[] { 1000, "Four-digit value" };
        yield return new object[] { 99999, "Five-digit value" };
        yield return new object[] { int.MaxValue, "Maximum int value" };
    }

    /// <summary>
    /// Executes Validate_WithMemberDataInvalidIds_ShouldFail operation.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [MemberData(nameof(GetInvalidIdTestCases))]
    public void Validate_WithMemberDataInvalidIds_ShouldFail(int id, string description)
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

        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        // Act
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
        yield return new object[] { 0, "Zero value" };
        yield return new object[] { -1, "Negative one" };
        yield return new object[] { -2, "Small negative value" };
        yield return new object[] { -42, "Typical negative value" };
        yield return new object[] { -100, "Negative hundred" };
        yield return new object[] { -999, "Large negative value" };
        yield return new object[] { int.MinValue, "Minimum int value" };
    }

    /// <summary>
    /// Executes Validate_WithBoundaryValues_ShouldWorkCorrectly operation.
    /// </summary>

    [Fact]
    public void Validate_WithBoundaryValues_ShouldWorkCorrectly()
    {
        // Arrange & Test boundary values
        var validQuery = CreateValidQuery();
        validQuery.Id = 1;
        var invalidQuery = CreateValidQuery();
        invalidQuery.Id = 0;

        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        // Act
        var validResult = _validator.TestValidate(validQuery);
        var invalidResult = _validator.TestValidate(invalidQuery);

        // Assert
        validResult.ShouldNotHaveAnyValidationErrors();
        invalidResult.ShouldHaveValidationErrorFor(x => x.Id);
    }

    /// <summary>
    /// Executes Validate_WithVariousIdValues_ShouldReturnExpectedResult operation.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="expectedValid">The expectedValid.</param>

    [Theory]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(100, true)]
    [InlineData(int.MaxValue, true)]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    [InlineData(-100, false)]
    [InlineData(int.MinValue, false)]
    public void Validate_WithVariousIdValues_ShouldReturnExpectedResult(int id, bool expectedValid)
    {
        // Using parameters: id, expectedValid
        _ = id; // xUnit1026 fix
        _ = expectedValid; // xUnit1026 fix
        // Using parameters: id, expectedValid
        _ = id; // xUnit1026 fix
        _ = expectedValid; // xUnit1026 fix
        // Using parameters: id, expectedValid
        _ = id; // xUnit1026 fix
        _ = expectedValid; // xUnit1026 fix
        // Using parameters: id, expectedValid
        _ = id; // xUnit1026 fix
        _ = expectedValid; // xUnit1026 fix
        // Using parameters: id, expectedValid
        _ = id; // xUnit1026 fix
        _ = expectedValid; // xUnit1026 fix
        // Arrange
        var query = CreateValidQuery();
        query.Id = id;

        //[Fix]
        //CLAUDE
        //Date: 21/08/2025
        //Reason: Updated to use FluentValidation.TestHelper pattern for modern validation testing
        // Act
        var result = _validator.TestValidate(query);

        // Assert
        if (expectedValid)
        {
            result.ShouldNotHaveAnyValidationErrors();
        }
        else
        {
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }

    /// <summary>
    /// Creates a valid GetConfigAppsDetailQuery for testing purposes
    /// </summary>
    private static GetConfigAppsDetailQuery CreateValidQuery()
    {
        return new GetConfigAppsDetailQuery
        {
            Id = 1 // Valid positive UserId
        };
    }
}