// <copyright file="EventsListVmTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.ViewModels;

/// <summary>
/// Unit tests for EventsListVm
/// </summary>
public class EventsListVmTests
{
    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var validRequests = new List<TaskGatewayRequest>(); // Provide valid requests
        var validResponses = new List<TaskGatewayResponseDto>(); // Provide valid responses

        // Act
        var instance = new EventsListVm(validRequests, validResponses);

        // Assert
        instance.ShouldNotBeNull();
    }
    /// <summary>
    /// Executes Constructor_WithInvalidParameters_ShouldThrowException operation.
    /// </summary>

    // MARKED FOR DELETION - Constructor null guard test no longer needed for Railway-Oriented Programming
    // [Fact]
    // public void Constructor_WithInvalidParameters_ShouldThrowException()
    // {
    //     // Arrange
    //     //[Fix] 
    //     //CLAUDE
    //     //Date: 20/08/2025 
    //     //Reason: Intentional null test for exception throwing - using null-forgiving operator to suppress CS8600 warnings
    //     IEnumerable<TaskGatewayRequest> invalidRequests = null!; // Invalid requests
    //     IEnumerable<TaskGatewayResponse> invalidResponses = null!; // Invalid responses
    // 
    //     // Act & Assert
    //     Should.Throw<ArgumentNullException>(() => new EventsListVm(invalidRequests!, invalidResponses!));
    // }
    /// <summary>
    /// Executes Properties_WhenSet_ShouldReturnCorrectValues operation.
    /// </summary>

    [Fact]
    public void Properties_WhenSet_ShouldReturnCorrectValues()
    {
        // Arrange - Manufacturing events with realistic data
        var manufacturingRequests = new List<TaskGatewayRequest>
        {
            new TaskGatewayRequest
            {
                MachineId = 10001,
                RequestTask = "Create BarCode for Ford F-150",
                BarCode = "VIN:1FTFW1ET5DFC12345",
                TimeStamp = DateTime.Parse("2024-01-15 08:30:00")
            },
            new TaskGatewayRequest
            {
                MachineId = 201,
                RequestTask = "PCB Inspection",
                BarCode = "PCB:C02YG0VZJHD4",
                TimeStamp = DateTime.Parse("2024-01-15 09:15:00")
            }
        };

        var manufacturingResponses = new List<TaskGatewayResponseDto>
        {
            new TaskGatewayResponseDto
            {
                MachineId = 10001,
                Name = "Ford F-150 BarCode Created Successfully",
                Label = "VIN:1FTFW1ET5DFC12345",
                TimeStamp = DateTime.Parse("2024-01-15 08:30:30"),
                ResultValidation = ResultValidation.Valid
            },
            new TaskGatewayResponseDto
            {
                MachineId = 201,
                Name = "iPhone PCB Inspection Completed",
                Label = "PCB:C02YG0VZJHD4",
                TimeStamp = DateTime.Parse("2024-01-15 09:15:45"),
                ResultValidation = ResultValidation.Valid
            }
        };

        var instance = new EventsListVm(manufacturingRequests, manufacturingResponses);

        // Act
        var requests = instance.Requests;
        var responses = instance.Responses;

        // Assert
        requests.ShouldBe(manufacturingRequests);
        responses.ShouldBe(manufacturingResponses);
        requests.Count().ShouldBe(2);
        responses.Count().ShouldBe(2);
    }
    /// <summary>
    /// Executes CreateEventsListVm_WithSuccessfulResults_ShouldReturnSuccessResult operation.
    /// </summary>

    [Fact]
    public void CreateEventsListVm_WithSuccessfulResults_ShouldReturnSuccessResult()
    {
        // Arrange - Ford F-150 production events
        var requests = new List<TaskGatewayRequest>
        {
            new TaskGatewayRequest { MachineId = 10001, RequestTask = "Create BarCode", TimeStamp = DateTime.Now },
            new TaskGatewayRequest { MachineId = 10002, RequestTask = "Read BarCode", TimeStamp = DateTime.Now.AddMinutes(-1) }
        };
        // #32 C2: CreateEventsListVm reads persisted entities and up-projects them onto wire DTOs; the slim entity
        // carries only the 17 persisted columns (no wire-only Name), so the fed rows use entity-valid props and the
        // carried-through responses are asserted by their persisted identity (MachineId) after the DTO projection.
        var responses = new List<TaskGatewayResponse>
        {
            new TaskGatewayResponse { MachineId = 10001, TimeStamp = DateTime.Now.AddSeconds(-30) },
            new TaskGatewayResponse { MachineId = 10002, TimeStamp = DateTime.Now.AddMinutes(-1) }
        };

        var requestResult = Result<IEnumerable<TaskGatewayRequest>>.Success(requests);
        var responseResult = Result<IEnumerable<TaskGatewayResponse>>.Success(responses);

        // Act
        var result = EventsListVm.CreateEventsListVm(requestResult, responseResult);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Requests.ShouldBe(requests);
        result.Value.Responses.Count().ShouldBe(responses.Count);
        result.Value.Responses.Select(r => r.MachineId).ShouldBe(responses.Select(r => r.MachineId));
    }
    /// <summary>
    /// Executes CreateEventsListVm_WithFailedResults_ShouldReturnFailureResult operation.
    /// </summary>

    [Fact]
    public void CreateEventsListVm_WithFailedResults_ShouldReturnFailureResult()
    {
        // Arrange
        var requestResult = Result<IEnumerable<TaskGatewayRequest>>.WithFailure("Failed to retrieve requests");
        var responseResult = Result<IEnumerable<TaskGatewayResponse>>.WithFailure("Failed to retrieve responses");

        // Act
        var result = EventsListVm.CreateEventsListVm(requestResult, responseResult);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
        result.Errors.ShouldContain("Failed to retrieve requests");
        result.Errors.ShouldContain("Failed to retrieve responses");
    }
    /// <summary>
    /// Executes Constructor_WithEmptyCollections_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_WithEmptyCollections_ShouldCreateInstance()
    {
        // Arrange
        var emptyRequests = new List<TaskGatewayRequest>();
        var emptyResponses = new List<TaskGatewayResponseDto>();

        // Act
        var instance = new EventsListVm(emptyRequests, emptyResponses);

        // Assert
        instance.ShouldNotBeNull();
        instance.Requests.ShouldBe(emptyRequests);
        instance.Responses.ShouldBe(emptyResponses);
        instance.Requests.ShouldBeEmpty();
        instance.Responses.ShouldBeEmpty();
    }
    /// <summary>
    /// Executes Constructor_WithDifferentCollectionSizes_ShouldCreateInstance operation.
    /// </summary>
    /// <param name="requestCount">The requestCount.</param>
    /// <param name="responseCount">The responseCount.</param>
    /// <param name="scenario">The scenario.</param>

    [Theory]
    [InlineData(1, 1, "Single request and response")]
    [InlineData(5, 3, "Multiple requests, fewer responses")]
    [InlineData(2, 8, "Fewer requests, multiple responses")]
    [InlineData(10, 10, "Equal numbers of requests and responses")]
    public void Constructor_WithDifferentCollectionSizes_ShouldCreateInstance(int requestCount, int responseCount, string scenario)
    {
        // Using parameters: requestCount, responseCount, scenario
        _ = requestCount; // xUnit1026 fix
        _ = responseCount; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: requestCount, responseCount, scenario
        _ = requestCount; // xUnit1026 fix
        _ = responseCount; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: requestCount, responseCount, scenario
        _ = requestCount; // xUnit1026 fix
        _ = responseCount; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: requestCount, responseCount, scenario
        _ = requestCount; // xUnit1026 fix
        _ = responseCount; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Using parameters: requestCount, responseCount, scenario
        _ = requestCount; // xUnit1026 fix
        _ = responseCount; // xUnit1026 fix
        _ = scenario; // xUnit1026 fix
        // Arrange
        var requests = Enumerable.Range(1, requestCount)
            .Select(i => new TaskGatewayRequest
            {
                MachineId = i,
                RequestTask = $"Manufacturing Task {i}",
                TimeStamp = DateTime.Now.AddMinutes(-i)
            })
            .ToList();

        var responses = Enumerable.Range(1, responseCount)
            .Select(i => new TaskGatewayResponseDto
            {
                MachineId = i,
                Name = $"Manufacturing Response {i}",
                TimeStamp = DateTime.Now.AddMinutes(-i)
            })
            .ToList();

        // Act
        var instance = new EventsListVm(requests, responses);

        // Assert
        instance.ShouldNotBeNull();
        instance.Requests.Count().ShouldBe(requestCount);
        instance.Responses.Count().ShouldBe(responseCount);
    }
}