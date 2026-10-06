// <copyright file="RestExceptionTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Net;

namespace IndTrace.Agregation.Dependices.Middleware
{
    /// <summary>
    /// Unit tests for RestException
    /// </summary>
    public class RestExceptionTests
    {
        /// <summary>
        /// Executes Constructor_ShouldCreateInstance operation.
        /// </summary>
        [Fact]
        public void Constructor_ShouldCreateInstance()
        {
            // Arrange
            var code = HttpStatusCode.BadRequest;
            var message = "Test error message";

            // Act
            var exception = new RestException(code, message);

            // Assert
            exception.ShouldNotBeNull();
        }
        /// <summary>
        /// Executes Constructor_WithNullMessage_ShouldCreateInstance operation.
        /// </summary>

        [Fact]
        public void Constructor_WithNullMessage_ShouldCreateInstance()
        {
            // Arrange
            var code = HttpStatusCode.InternalServerError;
            object message = null!;

            // Act
            var exception = new RestException(code, message);

            // Assert
            exception.ShouldNotBeNull();
        }
        /// <summary>
        /// Executes Constructor_WithComplexObject_ShouldCreateInstance operation.
        /// </summary>

        [Fact]
        public void Constructor_WithComplexObject_ShouldCreateInstance()
        {
            // Arrange
            var code = HttpStatusCode.NotFound;
            var errorObject = new { Error = "Not found", Id = 123 };

            // Act
            var exception = new RestException(code, errorObject);

            // Assert
            exception.ShouldNotBeNull();
        }
        /// <summary>
        /// Executes Properties_WhenSet_ShouldReturnCorrectValues operation.
        /// </summary>

        [Fact]
        public void Properties_WhenSet_ShouldReturnCorrectValues()
        {
            // Arrange
            var code = HttpStatusCode.OK;
            var message = "Sample message";
            var instance = new RestException(code, message);

            // Act & Assert
            instance.Code.ShouldBe(code);
            instance.Message.ShouldBe(message);
        }
        /// <summary>
        /// Executes Methods_WhenCalled_ShouldReturnExpectedResults operation.
        /// </summary>

        [Fact]
        public void Methods_WhenCalled_ShouldReturnExpectedResults()
        {
            // Arrange
            var code = HttpStatusCode.Forbidden;
            var message = "Access denied";
            var instance = new RestException(code, message);

            // Act
            var actualCode = instance.Code;
            var actualMessage = instance.Message;

            // Assert
            actualCode.ShouldBe(code);
            actualMessage.ShouldBe(message);
        }
    }
}