// <copyright file="IndTraceUserServiceTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.UserService;

namespace Application.UnitTests.Infrastructure;

/// <summary>
/// Unit tests for IndTraceUserService
/// </summary>
public class IndTraceUserServiceTests
{
    /// <summary>
    /// Executes Constructor_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Act
        var instance = new IndTraceUserService();

        // Assert
        instance.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes CurrentUserId_ShouldReturnValidGuidString operation.
    /// </summary>
    /// <returns>The result of CurrentUserId_ShouldReturnValidGuidString.</returns>

    [Fact]
    public async Task CurrentUserId_ShouldReturnValidGuidString()
    {
        // Arrange
        var service = new IndTraceUserService();

        // Act
        var userId = await service.CurrentUserId;

        // Assert
        userId.ShouldNotBeNull();
        userId.ShouldNotBeEmpty();
        Guid.TryParse(userId, out var guid).ShouldBeTrue();
        guid.ShouldNotBe(Guid.Empty);
    }

    /// <summary>
    /// Executes CurrentUserId_WhenCalledMultipleTimes_ShouldReturnDifferentGuids operation.
    /// </summary>
    /// <returns>The result of CurrentUserId_WhenCalledMultipleTimes_ShouldReturnDifferentGuids.</returns>

    [Fact]
    public async Task CurrentUserId_WhenCalledMultipleTimes_ShouldReturnDifferentGuids()
    {
        // Arrange
        var indTraceUserService1 = new IndTraceUserService();
        var indTraceUserService2 = new IndTraceUserService();
        // Act
        var userId1 = await indTraceUserService1.CurrentUserId;

        //for that we need to create a new instance of the service or fire an event
        //who make the user change or the service resolve another user

        var userId2 = await indTraceUserService2.CurrentUserId;

        //this Test is failing because the IndTraceUserService is just a stub
        //that returns a new Guid each time CurrentUserId is accessed.
        //since we are not creating a new service the user guid have the same value
        //but it has value on changing the behavior this will be a stub anyway ??
        //Creating another instance the test passess
        //[TODO]
        //[DESIGN]
        //Link the service to entity framework context to get the current user from there
        //or link it to the http context to get the current user from there
        //or another way to get the current user
        //[ABR]
        // 22/8/2025

        // /Assert
        userId1.ShouldNotBe(userId2);
        Guid.TryParse(userId1, out var guid1).ShouldBeTrue();
        Guid.TryParse(userId2, out var guid2).ShouldBeTrue();

        guid1.ShouldNotBe(guid2);
    }

    /// <summary>
    /// Executes CurrentUserName_ShouldReturnAdmin operation.
    /// </summary>
    /// <returns>The result of CurrentUserName_ShouldReturnAdmin.</returns>

    [Fact]
    public async Task CurrentUserName_ShouldReturnAdmin()
    {
        // Arrange
        var service = new IndTraceUserService();

        // Act
        var userName = await service.CurrentUserName;

        // Assert
        userName.ShouldBe("Admin");
    }

    /// <summary>
    /// Executes CurrentUserName_WhenCalledMultipleTimes_ShouldReturnSameValue operation.
    /// </summary>
    /// <returns>The result of CurrentUserName_WhenCalledMultipleTimes_ShouldReturnSameValue.</returns>

    [Fact]
    public async Task CurrentUserName_WhenCalledMultipleTimes_ShouldReturnSameValue()
    {
        // Arrange
        var service = new IndTraceUserService();

        // Act
        var userName1 = await service.CurrentUserName;
        var userName2 = await service.CurrentUserName;

        // Assert
        userName1.ShouldBe(userName2);
        userName1.ShouldBe("Admin");
    }

    /// <summary>
    /// Executes IsAuthenticated_ShouldReturnTrue operation.
    /// </summary>
    /// <returns>The result of IsAuthenticated_ShouldReturnTrue.</returns>

    [Fact]
    public async Task IsAuthenticated_ShouldReturnTrue()
    {
        // Arrange
        var service = new IndTraceUserService();

        // Act
        var isAuthenticated = await service.IsAuthenticated;

        // Assert
        isAuthenticated.ShouldBeTrue();
    }

    /// <summary>
    /// Executes IsAuthenticated_WhenCalledMultipleTimes_ShouldReturnSameValue operation.
    /// </summary>
    /// <returns>The result of IsAuthenticated_WhenCalledMultipleTimes_ShouldReturnSameValue.</returns>

    [Fact]
    public async Task IsAuthenticated_WhenCalledMultipleTimes_ShouldReturnSameValue()
    {
        // Arrange
        var service = new IndTraceUserService();

        // Act
        var isAuthenticated1 = await service.IsAuthenticated;
        var isAuthenticated2 = await service.IsAuthenticated;

        // Assert
        isAuthenticated1.ShouldBe(isAuthenticated2);
        isAuthenticated1.ShouldBeTrue();
    }

    /// <summary>
    /// Executes AllProperties_WhenAccessedConcurrently_ShouldReturnConsistentValues operation.
    /// </summary>
    /// <returns>The result of AllProperties_WhenAccessedConcurrently_ShouldReturnConsistentValues.</returns>

    [Fact]
    public async Task AllProperties_WhenAccessedConcurrently_ShouldReturnConsistentValues()
    {
        // Arrange
        var service = new IndTraceUserService();

        // Act
        var tasks = new List<Task>
        {
            service.CurrentUserId,
            service.CurrentUserName,
            service.IsAuthenticated,
            service.CurrentUserId,
            service.CurrentUserName,
            service.IsAuthenticated
        };

        await Task.WhenAll(tasks);

        // Assert
        var userId1 = await service.CurrentUserId;
        var userId2 = await service.CurrentUserId;
        var userName = await service.CurrentUserName;
        var isAuthenticated = await service.IsAuthenticated;

        userId1.ShouldNotBeNull();
        userId2.ShouldNotBeNull();
        userName.ShouldBe("Admin");
        isAuthenticated.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Service_ShouldImplementIIndTraceUserService operation.
    /// </summary>

    [Fact]
    public void Service_ShouldImplementIIndTraceUserService()
    {
        // Arrange & Act
        var service = new IndTraceUserService();

        // Assert
        service.ShouldBeAssignableTo<IIndTraceUserService>();
    }

    /// <summary>
    /// Executes CurrentUserId_ShouldBeValidGuidFormat operation.
    /// </summary>
    /// <returns>The result of CurrentUserId_ShouldBeValidGuidFormat.</returns>

    [Fact]
    public async Task CurrentUserId_ShouldBeValidGuidFormat()
    {
        // Arrange
        var service = new IndTraceUserService();

        // Act
        var userId = await service.CurrentUserId;

        // Assert
        userId.ShouldMatch(@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$");
    }

    /// <summary>
    /// Executes CurrentUserName_ShouldNotBeEmpty operation.
    /// </summary>
    /// <returns>The result of CurrentUserName_ShouldNotBeEmpty.</returns>

    [Fact]
    public async Task CurrentUserName_ShouldNotBeEmpty()
    {
        // Arrange
        var service = new IndTraceUserService();

        // Act
        var userName = await service.CurrentUserName;

        // Assert
        userName.ShouldNotBeNullOrEmpty();
        userName.ShouldNotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// Executes IsAuthenticated_ShouldBeBoolean operation.
    /// </summary>
    /// <returns>The result of IsAuthenticated_ShouldBeBoolean.</returns>

    [Fact]
    public async Task IsAuthenticated_ShouldBeBoolean()
    {
        // Arrange
        var service = new IndTraceUserService();

        // Act
        var isAuthenticated = await service.IsAuthenticated;

        // Assert
        isAuthenticated.ShouldBeOfType<bool>();
    }
}