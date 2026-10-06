// <copyright file="BadRequestExceptionTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.Middleware;

/// <summary>
/// Unit tests for BadRequestException
/// </summary>
public class BadRequestExceptionTests
{
    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        string message = "Test error message";

        // Act
        var instance = new BadRequestException(message);

        // Assert
        instance.ShouldNotBeNull();
        instance.Message.ShouldBe(message);
    }
    /// <summary>
    /// Executes Constructor_WithInvalidParameters_ShouldThrowException operation.
    /// </summary>

    [Fact]
    public void Constructor_WithInvalidParameters_ShouldThrowException()
    {
        // Arrange
        string message = "Invalid request error";

        // Act & Assert
        var instance = new BadRequestException(message);
        instance.ShouldNotBeNull();
        instance.Message.ShouldBe(message);
    }
    /// <summary>
    /// Executes Properties_WhenSet_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSet_ShouldReturnCorrectValues()
    {
        // Arrange
        string message = "Property test message";
        var instance = new BadRequestException(message);

        // Act & Assert
        instance.Message.ShouldBe(message);
        instance.ShouldBeAssignableTo<Exception>();
    }
    /// <summary>
    /// Executes Methods_WhenCalled_ShouldReturnExpectedResults operation.
    /// </summary>

    [Fact]
    public void Methods_WhenCalled_ShouldReturnExpectedResults()
    {
        // Arrange
        string message = "Method test message";
        var instance = new BadRequestException(message);

        // Act
        var toString = instance.ToString();

        // Assert
        toString.ShouldContain(message!);
        instance.ShouldNotBeNull();
    }
}