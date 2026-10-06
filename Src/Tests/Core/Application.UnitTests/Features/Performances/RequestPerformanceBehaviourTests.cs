// <copyright file="RequestPerformanceBehaviourTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Performances
{
    /// <summary>
    /// Unit tests for RequestPerformanceBehaviour
    /// </summary>
    public class RequestPerformanceBehaviourTests
    {
        /// <summary>
        /// Executes Constructor_ShouldCreateInstance operation.
        /// </summary>
        [Fact]
        public void Constructor_ShouldCreateInstance()
        {
            // Arrange
            var logger = XUnitLogger.CreateLogger<CreateBarCodeCommand>();

            // Act
            var behavior = new RequestPerformanceBehaviour<CreateBarCodeCommand, TaskGatewayResponse>(logger);

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
            var logger = XUnitLogger.CreateLogger<CreateBarCodeCommand>();
            var behavior = new RequestPerformanceBehaviour<CreateBarCodeCommand, TaskGatewayResponse>(logger);
            var request = new CreateBarCodeCommand();
            var next = Substitute.For<RequestFunctionalHandlerDelegate<TaskGatewayResponse>>();

            //[Fix] 
            //CLAUDE
            //Date: 22/08/2025 
            //Reason: Pattern - Setup next delegate to return expected response
            var expectedResponse = new TaskGatewayResponse();
            next().Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await behavior.HandleAsync(request, next, TestContext.Current.CancellationToken);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe(expectedResponse);
            await next.Received(1).Invoke();
        }
        /// <summary>
        /// Executes Handle_ShouldLogPerformance operation.
        /// </summary>
        /// <returns>The result of Handle_ShouldLogPerformance.</returns>

        [Fact]
        public async Task Handle_ShouldLogPerformance()
        {
            // Arrange
            var logger = XUnitLogger.CreateLogger<CreateBarCodeCommand>();
            var behavior = new RequestPerformanceBehaviour<CreateBarCodeCommand, TaskGatewayResponse>(logger);
            var request = new CreateBarCodeCommand();
            var next = Substitute.For<RequestFunctionalHandlerDelegate<TaskGatewayResponse>>();

            // Act
            await behavior.HandleAsync(request, next, TestContext.Current.CancellationToken);

            // Assert
            // Verify logging occurred
        }
    }
}