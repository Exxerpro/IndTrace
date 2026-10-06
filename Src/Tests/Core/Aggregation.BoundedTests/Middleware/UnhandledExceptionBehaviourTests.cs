// <copyright file="UnhandledExceptionBehaviourTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.Middleware
{
    /// <summary>
    /// Unit tests for UnhandledExceptionBehaviour
    /// </summary>
    public class UnhandledExceptionBehaviourTests
    {
        /// <summary>
        /// Executes Constructor_ShouldCreateInstance operation.
        /// </summary>
        [Fact]
        public void Constructor_ShouldCreateInstance()
        {
            // Arrange
            var logger = XUnitLogger.CreateLogger<UnhandledExceptionBehaviour<object, Result<int>>>();

            // Act
            var behavior = new UnhandledExceptionBehaviour<object, Result<int>>(logger);

            // Assert
            behavior.ShouldNotBeNull();
        }

        /// <summary>
        /// Executes Handle_ShouldProcessRequest operation.
        /// </summary>
        /// <returns>The result of Handle_ShouldProcessRequest.</returns>

        [Fact]
        public async Task Handle_ShouldProcessRequest()
        {
            // Arrange
            var logger = XUnitLogger.CreateLogger<UnhandledExceptionBehaviour<object, Result<int>>>();
            var behavior = new UnhandledExceptionBehaviour<object, Result<int>>(logger);
            var request = new object();

            //[Fix]
            //CLAUDE
            //Date: 22/08/2025
            //Reason: Pattern - Setup next delegate to return expected Result<int>
            var expectedResult = Result<int>.Success(42);
            RequestFunctionalHandlerDelegate<Result<int>> next = () => Task.FromResult(expectedResult);

            // Act
            var result = await behavior.HandleAsync(request, next, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe(expectedResult);
        }

        /// <summary>
        /// Executes Handle_ShouldLogException operation.
        /// </summary>
        /// <returns>The result of Handle_ShouldLogException.</returns>

        [Fact]
        public async Task Handle_ShouldLogException()
        {
            // Arrange
            var logger = XUnitLogger.CreateLogger<UnhandledExceptionBehaviour<object, Result<int>>>();
            var behavior = new UnhandledExceptionBehaviour<object, Result<int>>(logger);
            var request = new object();

            // Create a delegate that throws an exception to test error handling
            RequestFunctionalHandlerDelegate<Result<int>> next = () => throw new InvalidOperationException("Test exception");

            // Act
            var result = await behavior.HandleAsync(request, next, TestContext.Current.CancellationToken);

            // Assert
            // Verify exception was caught and converted to Result.Failure
            result.ShouldNotBeNull();
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldNotBeNull();
            result.Errors.ShouldNotBeEmpty();
        }
    }
}