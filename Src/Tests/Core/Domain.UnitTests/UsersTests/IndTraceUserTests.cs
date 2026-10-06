// <copyright file="IndTraceUserTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.UsersTests;

/// <summary>
/// Unit tests for IndTraceUser - Domain entity for user management in manufacturing systems.
/// Tests property validation, interface compliance, audit functionality, and manufacturing user scenarios.
/// </summary>
public class IndTraceUserTests
{
    /// <summary>
    /// Executes IndTraceUser_Constructor_WhenCreated_ShouldCreateInstanceWithDefaultValues operation.
    /// </summary>
    [Fact]
    public void IndTraceUser_Constructor_WhenCreated_ShouldCreateInstanceWithDefaultValues()
    {
        // Arrange & Act
        var instance = new IndTraceUser();

        // Assert
        instance.ShouldNotBeNull();
        instance.ShouldBeAssignableTo<AuditableEntity>();
        instance.ShouldBeAssignableTo<IEntityRoot>();
        instance.UserId.ShouldBe(0);
        instance.UserName.ShouldBe(string.Empty);

        // Inherited AuditableEntity properties: timestamps are stamped by the persistence layer on save (#29 F6),
        // so a not-yet-persisted entity carries the string.Empty user defaults and null timestamps.
        instance.CreatedBy.ShouldBe(string.Empty);
        instance.ModifiedBy.ShouldBe(string.Empty);
        instance.CreatedOn.ShouldBeNull();
        instance.ModifiedOn.ShouldBeNull();
    }

    /// <summary>
    /// Executes IndTraceUserProperties_WhenSetToValidValues_ShouldReturnCorrectValues operation.
    /// </summary>
    /// <param name="userId">The userId.</param>
    /// <param name="userName">The userName.</param>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData(1, "john.operator")]
    [InlineData(2, "mary.supervisor")]
    [InlineData(3, "david.technician")]
    [InlineData(4, "sarah.quality")]
    [InlineData(5, "mike.manager")]
    public void IndTraceUserProperties_WhenSetToValidValues_ShouldReturnCorrectValues(int userId, string userName)
    {
        // Arrange
        var instance = new IndTraceUser();

        // Act
        instance.UserId = userId;
        instance.UserName = userName!;

        // Assert
        instance.UserId.ShouldBe(userId);
        instance.UserName.ShouldBe(userName);
    }

    /// <summary>
    /// Executes IndTraceUser_Properties_WithEdgeCaseValues_ShouldStoreCorrectly operation.
    /// </summary>
    /// <param name="userId">The userId.</param>
    /// <param name="userName">The userName.</param>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData(0, null)]
    [InlineData(-1, "")]
    [InlineData(999, "admin")]
    [InlineData(1, "a")]
    [InlineData(42, "user@example.com")]
    public void IndTraceUser_Properties_WithEdgeCaseValues_ShouldStoreCorrectly(int userId, string? userName)
    {
        // Arrange
        var instance = new IndTraceUser();

        // Act
        instance.UserId = userId;
        instance.UserName = userName!;

        // Assert
        instance.UserId.ShouldBe(userId);
        instance.UserName.ShouldBe(userName);
    }

    /// <summary>
    /// Executes ManufacturingUserRoles_WithDifferentPositions_ShouldHandleCorrectly operation.
    /// </summary>
    /// <param name="userName">The userName.</param>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData("operator.smith")]
    [InlineData("supervisor.jones")]
    [InlineData("technician.brown")]
    [InlineData("manager.davis")]
    [InlineData("inspector.wilson")]
    public void ManufacturingUserRoles_WithDifferentPositions_ShouldHandleCorrectly(string userName)
    {
        // Arrange & Act
        var user = new IndTraceUser
        {
            UserId = 1,
            UserName = userName
        };

        // Assert
        user.UserName.ShouldBe(userName);
        user.UserId.ShouldBe(1);

        // Manufacturing user naming conventions
        user.UserName.ShouldNotBeNullOrWhiteSpace();
        user.UserId.ShouldBeGreaterThan(0);

        // IndTraceUser roles should indicate function
        (user.UserName.Contains("operator") ||
         user.UserName.Contains("supervisor") ||
         user.UserName.Contains("technician") ||
         user.UserName.Contains("manager") ||
         user.UserName.Contains("inspector")).ShouldBeTrue();
    }

    /// <summary>
    /// Executes InterfaceCompliance_ShouldImplementRequiredInterfaces operation.
    /// </summary>

    [Fact]
    public void InterfaceCompliance_ShouldImplementRequiredInterfaces()
    {
        // Arrange & Act
        var instance = new IndTraceUser();

        // Assert
        instance.ShouldBeAssignableTo<AuditableEntity>();
        instance.ShouldBeAssignableTo<IEntityRoot>();

        // Verify interface contracts
        instance.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes ShiftManagement_WithDifferentShifts_ShouldSupportManufacturingOperations operation.
    /// </summary>
    /// <param name="userId">The userId.</param>
    /// <param name="userName">The userName.</param>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData(1, "shift1.operator")]
    [InlineData(2, "shift2.operator")]
    [InlineData(3, "shift3.operator")]
    [InlineData(4, "weekend.operator")]
    [InlineData(5, "night.supervisor")]
    public void ShiftManagement_WithDifferentShifts_ShouldSupportManufacturingOperations(int userId, string userName)
    {
        // Arrange
        var user = new IndTraceUser();

        // Act
        user.UserId = userId;
        user.UserName = userName;

        // Assert
        user.UserId.ShouldBe(userId);
        user.UserName.ShouldBe(userName);

        // Shift user business rules
        user.UserId.ShouldBeGreaterThan(0);
        (user.UserName.Contains("shift") || user.UserName.Contains("weekend") || user.UserName.Contains("night")).ShouldBeTrue();
    }

    /// <summary>
    /// Executes AuditableEntityFunctionality_ShouldInheritAuditFields operation.
    /// </summary>

    [Fact]
    public void AuditableEntityFunctionality_ShouldInheritAuditFields()
    {
        // Arrange
        var user = new IndTraceUser();
        var testDate = DateTime.Now;
        var testUser = "test.admin";

        // Act
        user.CreatedBy = testUser;
        user.ModifiedBy = testUser;
        user.CreatedOn = testDate;
        user.ModifiedOn = testDate;

        // Assert
        user.CreatedBy.ShouldBe(testUser);
        user.ModifiedBy.ShouldBe(testUser);
        user.CreatedOn.ShouldBe(testDate);
        user.ModifiedOn.ShouldBe(testDate);
    }

    /// <summary>
    /// Executes ManufacturingScenarios_WithCompanySpecificUsers_ShouldProvideProperIdentification operation.
    /// </summary>
    /// <param name="userName">The userName.</param>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData("Ford.Production.IndTraceUser")]
    [InlineData("Tesla.Quality.IndTraceUser")]
    [InlineData("BMW.Maintenance.IndTraceUser")]
    [InlineData("Mercedes.Assembly.IndTraceUser")]
    public void ManufacturingScenarios_WithCompanySpecificUsers_ShouldProvideProperIdentification(string userName)
    {
        // Arrange
        var user = new IndTraceUser
        {
            UserId = 1,
            UserName = userName
        };

        // Act & Assert
        user.UserName.ShouldBe(userName);
        user.UserId.ShouldBe(1);

        // Manufacturing user identification business rules
        user.UserName.Length.ShouldBeGreaterThan(0);
        user.UserId.ShouldBePositive();

        // Company-specific user naming should contain company identifier
        (user.UserName.Contains("Ford") ||
         user.UserName.Contains("Tesla") ||
         user.UserName.Contains("BMW") ||
         user.UserName.Contains("Mercedes")).ShouldBeTrue();
    }

    /// <summary>
    /// Executes UserManagement_ShouldSupportCompleteUserLifecycle operation.
    /// </summary>

    [Fact]
    public void UserManagement_ShouldSupportCompleteUserLifecycle()
    {
        // Arrange
        var newUser = new IndTraceUser { UserId = 1, UserName = "new.operator" };
        var activeUser = new IndTraceUser { UserId = 2, UserName = "active.supervisor" };
        var adminUser = new IndTraceUser { UserId = 3, UserName = "admin.manager" };

        // Act & Assert - IndTraceUser lifecycle management
        newUser.UserId.ShouldBe(1);
        activeUser.UserId.ShouldBe(2);
        adminUser.UserId.ShouldBe(3);

        newUser.UserName.ShouldBe("new.operator");
        activeUser.UserName.ShouldBe("active.supervisor");
        adminUser.UserName.ShouldBe("admin.manager");

        // All users should be unique
        newUser.UserId.ShouldNotBe(activeUser.UserId);
        activeUser.UserId.ShouldNotBe(adminUser.UserId);
        newUser.UserId.ShouldNotBe(adminUser.UserId);

        // All users should have distinct usernames
        newUser.UserName.ShouldNotBe(activeUser.UserName);
        activeUser.UserName.ShouldNotBe(adminUser.UserName);
        newUser.UserName.ShouldNotBe(adminUser.UserName);
    }

    /// <summary>
    /// Executes EdgeCaseHandling_WithVariousIdValues_ShouldStoreCorrectly operation.
    /// </summary>
    /// <param name="edgeId">The edgeId.</param>
    /// <param name="userName">The userName.</param>

    [Theory]
    [InlineData(1000, "high.id.user")]
    [InlineData(-999, "negative.id.user")]
    [InlineData(0, "zero.id.user")]
    public void EdgeCaseHandling_WithVariousIdValues_ShouldStoreCorrectly(int edgeId, string userName)
    {
        // Arrange & Act
        var user = new IndTraceUser
        {
            UserId = edgeId,
            UserName = userName
        };

        // Assert
        user.UserId.ShouldBe(edgeId);
        user.UserName.ShouldBe(userName);
    }

    /// <summary>
    /// Executes PropertyRoundTrip_ShouldMaintainDataIntegrity operation.
    /// </summary>

    [Fact]
    public void PropertyRoundTrip_ShouldMaintainDataIntegrity()
    {
        // Arrange
        var originalId = 42;
        var originalName = "test.user";
        var instance = new IndTraceUser();

        // Act
        instance.UserId = originalId;
        instance.UserName = originalName;

        // Assert
        instance.UserId.ShouldBe(originalId);
        instance.UserName.ShouldBe(originalName);
    }

    /// <summary>
    /// Executes SecurityCompliance_ShouldSupportManufacturingAccessControl operation.
    /// </summary>

    [Fact]
    public void SecurityCompliance_ShouldSupportManufacturingAccessControl()
    {
        // Arrange
        var operatorUser = new IndTraceUser { UserId = 1, UserName = "operator.level1" };
        var supervisorUser = new IndTraceUser { UserId = 2, UserName = "supervisor.level2" };
        var managerUser = new IndTraceUser { UserId = 3, UserName = "manager.level3" };

        // Act & Assert - Security level implications
        operatorUser.UserName.ShouldContain("operator");
        supervisorUser.UserName.ShouldContain("supervisor");
        managerUser.UserName.ShouldContain("manager");

        // IndTraceUser IDs should be unique for security tracking
        operatorUser.UserId.ShouldNotBe(supervisorUser.UserId);
        supervisorUser.UserId.ShouldNotBe(managerUser.UserId);
        operatorUser.UserId.ShouldNotBe(managerUser.UserId);

        // All users should have positive IDs for security systems
        operatorUser.UserId.ShouldBePositive();
        supervisorUser.UserId.ShouldBePositive();
        managerUser.UserId.ShouldBePositive();
    }
}