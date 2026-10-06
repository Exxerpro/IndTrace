// <copyright file="GetRegistroDetailQueryValidatorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Queries;

/// <summary>
/// Unit tests for GetRegistroDetailQueryValidator
/// </summary>
public class GetRegistroDetailQueryValidatorTests
{
    private readonly GetRegistroDetailQueryValidator _validator = null!;
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>

    public GetRegistroDetailQueryValidatorTests()
    {
        _validator = new GetRegistroDetailQueryValidator();
    }
    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var instance = new GetRegistroDetailQueryValidator();

        // Assert
        instance.ShouldNotBeNull();
        instance.ShouldBeAssignableTo<AbstractValidator<GetRegisterDetailQuery>>();
    }
    /// <summary>
    /// Executes Validate_WithValidRegisterId_ShouldReturnSuccess operation.
    /// </summary>

    [Fact]
    public void Validate_WithValidRegisterId_ShouldReturnSuccess()
    {
        // Arrange
        var query = new GetRegisterDetailQuery { RegisterId = 1 };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }
    /// <summary>
    /// Executes Validate_WithInvalidRegisterId_ShouldReturnValidationError operation.
    /// </summary>
    /// <param name="invalidRegisterId">The invalidRegisterId.</param>

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    public void Validate_WithInvalidRegisterId_ShouldReturnValidationError(int invalidRegisterId)
    {
        // Using parameters: invalidRegisterId
        _ = invalidRegisterId; // xUnit1026 fix
        // Using parameters: invalidRegisterId
        _ = invalidRegisterId; // xUnit1026 fix
        // Using parameters: invalidRegisterId
        _ = invalidRegisterId; // xUnit1026 fix
        // Using parameters: invalidRegisterId
        _ = invalidRegisterId; // xUnit1026 fix
        // Using parameters: invalidRegisterId
        _ = invalidRegisterId; // xUnit1026 fix
        // Arrange
        var query = new GetRegisterDetailQuery { RegisterId = invalidRegisterId };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(GetRegisterDetailQuery.RegisterId));
    }
    /// <summary>
    /// Executes Validate_WithValidManufacturingRegisterIds_ShouldReturnSuccess operation.
    /// </summary>
    /// <param name="registerId">The registerId.</param>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData(1, "First register")]
    [InlineData(100, "Standard register")]
    [InlineData(1000, "High volume register")]
    [InlineData(9999, "Large scale register")]
    [InlineData(int.MaxValue, "Maximum value register")]
    public void Validate_WithValidManufacturingRegisterIds_ShouldReturnSuccess(int registerId, string scenario)
    {
        // Using parameters: registerId, scenario
        _ = registerId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: registerId, scenario
        _ = registerId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: registerId, scenario
        _ = registerId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: registerId, scenario
        _ = registerId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: registerId, scenario
        _ = registerId; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Arrange
        var query = new GetRegisterDetailQuery { RegisterId = registerId };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }
    /// <summary>
    /// Executes Validate_WithTypicalProductionLineRegisterId_ShouldPass operation.
    /// </summary>

    [Fact]
    public void Validate_WithTypicalProductionLineRegisterId_ShouldPass()
    {
        // Arrange - Ford F-150 production line register ID
        var query = new GetRegisterDetailQuery { RegisterId = 45123 };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }
    /// <summary>
    /// Executes Validate_WithElectronicsManufacturingRegisterId_ShouldPass operation.
    /// </summary>

    [Fact]
    public void Validate_WithElectronicsManufacturingRegisterId_ShouldPass()
    {
        // Arrange - SMT line register ID
        var query = new GetRegisterDetailQuery { RegisterId = 1001 };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }
    /// <summary>
    /// Executes Validate_PropertyRoundTrip_ShouldMaintainValue operation.
    /// </summary>

    [Fact]
    public void Validate_PropertyRoundTrip_ShouldMaintainValue()
    {
        // Arrange
        var expectedRegisterId = 2550;
        var query = new GetRegisterDetailQuery { RegisterId = expectedRegisterId };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue();
        query.RegisterId.ShouldBe(expectedRegisterId);
    }
    /// <summary>
    /// Executes Validator_Rules_ShouldBeConfiguredCorrectly operation.
    /// </summary>

    [Fact]
    public void Validator_Rules_ShouldBeConfiguredCorrectly()
    {
        // Arrange
        var query = new GetRegisterDetailQuery { RegisterId = 0 };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.Errors.ShouldHaveSingleItem();
        var error = result.Errors.First();
        error.PropertyName.ShouldBe(nameof(GetRegisterDetailQuery.RegisterId));
        error.ErrorMessage.ShouldContain("greater than");
    }
    /// <summary>
    /// Executes Validate_EdgeCaseBoundaryValues_ShouldHandleCorrectly operation.
    /// </summary>

    [Fact]
    public void Validate_EdgeCaseBoundaryValues_ShouldHandleCorrectly()
    {
        // Arrange & Act & Assert - Testing boundary conditions
        var validQuery = new GetRegisterDetailQuery { RegisterId = 1 };
        var validResult = _validator.Validate(validQuery);
        validResult.IsValid.ShouldBeTrue();

        var invalidQuery = new GetRegisterDetailQuery { RegisterId = 0 };
        var invalidResult = _validator.Validate(invalidQuery);
        invalidResult.IsValid.ShouldBeFalse();
    }
}
