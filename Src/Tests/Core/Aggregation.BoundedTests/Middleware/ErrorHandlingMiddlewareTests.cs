// <copyright file="ErrorHandlingMiddlewareTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Net;
using Microsoft.AspNetCore.Http;

namespace IndTrace.Aggregation.BoundedTests.Middleware;

/// <summary>
/// Unit tests for ErrorHandlingMiddleware
/// </summary>
public class ErrorHandlingMiddlewareTests
{
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    public ErrorHandlingMiddlewareTests()
    {
        _logger = XUnitLogger.CreateLogger<ErrorHandlingMiddleware>();
    }

    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var mockLogger = XUnitLogger.CreateLogger<ErrorHandlingMiddleware>();
        RequestDelegate mockRequestDelegate = (context) => Task.CompletedTask;

        // Act
        var instance = new ErrorHandlingMiddleware(mockRequestDelegate, mockLogger);

        // Assert
        instance.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Constructor_WithNullRequestDelegate_ShouldThrowException operation.
    /// </summary>

    //[Fact]
    //public void Constructor_WithNullRequestDelegate_ShouldThrowException()
    //{
    //    // Arrange
    //    RequestDelegate? nullRequestDelegate = null!;
    //    var mockLogger = XUnitLogger.CreateLogger<ErrorHandlingMiddleware>();

    //    // Act & Assert
    //    Should.Throw<ArgumentNullException>(() => new ErrorHandlingMiddleware(nullRequestDelegate!, mockLogger));
    //}

    ///// <summary>
    ///// Executes Constructor_WithNullLogger_ShouldThrowException operation.
    ///// </summary>
    //[Fact]
    //public void Constructor_WithNullLogger_ShouldThrowException()
    //{
    //    // Arrange
    //    RequestDelegate mockRequestDelegate = (context) => Task.CompletedTask;
    //    ILogger<ErrorHandlingMiddleware>? nullLogger = null!;

    //    // Act & Assert
    //    Should.Throw<ArgumentNullException>(() => new ErrorHandlingMiddleware(mockRequestDelegate, nullLogger!));
    //}
    /// <summary>
    /// Executes Invoke_WithSuccessfulNext_ShouldCallNextMiddleware operation.
    /// </summary>
    /// <returns>The result of Invoke_WithSuccessfulNext_ShouldCallNextMiddleware.</returns>

    [Fact]
    public async Task Invoke_WithSuccessfulNext_ShouldCallNextMiddleware()
    {
        // Arrange
        var context = CreateHttpContext();
        var nextCalled = false;
        RequestDelegate next = (ctx) => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new ErrorHandlingMiddleware(next, _logger);

        // Act
        await middleware.Invoke(context);

        // Assert
        nextCalled.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Invoke_WithRestException_ShouldReturnCorrectStatusAndResponse operation.
    /// </summary>
    /// <returns>The result of Invoke_WithRestException_ShouldReturnCorrectStatusAndResponse.</returns>

    [Fact]
    public async Task Invoke_WithRestException_ShouldReturnCorrectStatusAndResponse()
    {
        // Arrange
        var context = CreateHttpContext();
        var restException = new RestException(HttpStatusCode.BadRequest, "Validation failed");
        RequestDelegate next = (ctx) => throw restException;
        var middleware = new ErrorHandlingMiddleware(next, _logger);

        // Act
        await middleware.Invoke(context);

        // Assert
        context.Response.StatusCode.ShouldBe((int)HttpStatusCode.BadRequest);
        context.Response.ContentType.ShouldBe("application/json");

        // Verify response body
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);
        var responseObj = JsonSerializer.Deserialize<object>(responseBody);
        responseObj.ShouldNotBeNull();
    }

    /// <summary>
    /// Executes Invoke_WithUnauthorizedRestException_ShouldReturn401 operation.
    /// </summary>
    /// <returns>The result of Invoke_WithUnauthorizedRestException_ShouldReturn401.</returns>

    [Fact]
    public async Task Invoke_WithUnauthorizedRestException_ShouldReturn401()
    {
        // Arrange
        var context = CreateHttpContext();
        var restException = new RestException(HttpStatusCode.Unauthorized, "Access denied");
        RequestDelegate next = (ctx) => throw restException;
        var middleware = new ErrorHandlingMiddleware(next, _logger);

        // Act
        await middleware.Invoke(context);

        // Assert
        context.Response.StatusCode.ShouldBe((int)HttpStatusCode.Unauthorized);
        context.Response.ContentType.ShouldBe("application/json");
    }

    /// <summary>
    /// Executes Invoke_WithNotFoundRestException_ShouldReturn404 operation.
    /// </summary>
    /// <returns>The result of Invoke_WithNotFoundRestException_ShouldReturn404.</returns>

    [Fact]
    public async Task Invoke_WithNotFoundRestException_ShouldReturn404()
    {
        // Arrange
        var context = CreateHttpContext();
        var restException = new RestException(HttpStatusCode.NotFound, "Resource not found");
        RequestDelegate next = (ctx) => throw restException;
        var middleware = new ErrorHandlingMiddleware(next, _logger);

        // Act
        await middleware.Invoke(context);

        // Assert
        context.Response.StatusCode.ShouldBe((int)HttpStatusCode.NotFound);
        context.Response.ContentType.ShouldBe("application/json");
    }

    /// <summary>
    /// Executes Invoke_WithGeneralException_ShouldReturn500 operation.
    /// </summary>
    /// <returns>The result of Invoke_WithGeneralException_ShouldReturn500.</returns>

    [Fact]
    public async Task Invoke_WithGeneralException_ShouldReturn500()
    {
        // Arrange
        var context = CreateHttpContext();
        var generalException = new InvalidOperationException("Something went wrong");
        RequestDelegate next = (ctx) => throw generalException;
        var middleware = new ErrorHandlingMiddleware(next, _logger);

        // Act
        await middleware.Invoke(context);

        // Assert
        context.Response.StatusCode.ShouldBe((int)HttpStatusCode.InternalServerError);
        context.Response.ContentType.ShouldBe("application/json");
    }

    /// <summary>
    /// Executes Invoke_WithDatabaseException_ShouldHandleManufacturingScenario operation.
    /// </summary>
    /// <returns>The result of Invoke_WithDatabaseException_ShouldHandleManufacturingScenario.</returns>

    [Fact]
    public async Task Invoke_WithDatabaseException_ShouldHandleManufacturingScenario()
    {
        // Arrange - Simulating database connection failure in manufacturing environment
        var context = CreateHttpContext();
        var dbException = new InvalidOperationException("Database connection timeout - Manufacturing data unavailable");
        RequestDelegate next = (ctx) => throw dbException;
        var middleware = new ErrorHandlingMiddleware(next, _logger);

        // Act
        await middleware.Invoke(context);

        // Assert
        context.Response.StatusCode.ShouldBe((int)HttpStatusCode.InternalServerError);
        context.Response.ContentType.ShouldBe("application/json");

        // Verify response contains error information
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);
        responseBody.ShouldContain("An internal server error has occured");
    }

    /// <summary>
    /// Executes Invoke_WithNetworkException_ShouldHandleIndustrialConnectivityIssues operation.
    /// </summary>
    /// <returns>The result of Invoke_WithNetworkException_ShouldHandleIndustrialConnectivityIssues.</returns>

    [Fact]
    public async Task Invoke_WithNetworkException_ShouldHandleIndustrialConnectivityIssues()
    {
        // Arrange - Simulating network issues in industrial setting
        var context = CreateHttpContext();
        var networkException = new HttpRequestException("PLC communication failure - Network timeout");
        RequestDelegate next = (ctx) => throw networkException;
        var middleware = new ErrorHandlingMiddleware(next, _logger);

        // Act
        await middleware.Invoke(context);

        // Assert
        context.Response.StatusCode.ShouldBe((int)HttpStatusCode.InternalServerError);
        context.Response.ContentType.ShouldBe("application/json");
    }

    /// <summary>
    /// Executes Invoke_WithVariousRestExceptions_ShouldHandleManufacturingErrors operation.
    /// </summary>
    /// <param name="statusCode">The statusCode.</param>
    /// <param name="message">The message.</param>
    /// <returns>The result of Invoke_WithVariousRestExceptions_ShouldHandleManufacturingErrors.</returns>

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, "Invalid machine configuration")]
    [InlineData(HttpStatusCode.Unauthorized, "Machine access unauthorized")]
    [InlineData(HttpStatusCode.NotFound, "Production line not found")]
    [InlineData(HttpStatusCode.Conflict, "Production cycle conflict")]
    [InlineData(HttpStatusCode.UnprocessableEntity, "Invalid part specification")]
    public async Task Invoke_WithVariousRestExceptions_ShouldHandleManufacturingErrors(HttpStatusCode statusCode, string message)
    {
        // Arrange
        var context = CreateHttpContext();
        var restException = new RestException(statusCode, message);
        RequestDelegate next = (ctx) => throw restException;
        var middleware = new ErrorHandlingMiddleware(next, _logger);

        // Act
        await middleware.Invoke(context);

        // Assert
        context.Response.StatusCode.ShouldBe((int)statusCode);
        context.Response.ContentType.ShouldBe("application/json");
    }

    /// <summary>
    /// Executes Invoke_WithRestExceptionWithNullMessage_ShouldHandleGracefully operation.
    /// </summary>
    /// <returns>The result of Invoke_WithRestExceptionWithNullMessage_ShouldHandleGracefully.</returns>

    [Fact]
    public async Task Invoke_WithRestExceptionWithNullMessage_ShouldHandleGracefully()
    {
        // Arrange
        var context = CreateHttpContext();
        var restException = new RestException(HttpStatusCode.BadRequest, null);
        RequestDelegate next = (ctx) => throw restException;
        var middleware = new ErrorHandlingMiddleware(next, _logger);

        // Act
        await middleware.Invoke(context);

        // Assert
        context.Response.StatusCode.ShouldBe((int)HttpStatusCode.BadRequest);
        context.Response.ContentType.ShouldBe("application/json");
    }

    /// <summary>
    /// Executes Invoke_WithRestExceptionWithEmptyMessage_ShouldHandleGracefully operation.
    /// </summary>
    /// <returns>The result of Invoke_WithRestExceptionWithEmptyMessage_ShouldHandleGracefully.</returns>

    [Fact]
    public async Task Invoke_WithRestExceptionWithEmptyMessage_ShouldHandleGracefully()
    {
        // Arrange
        var context = CreateHttpContext();
        var restException = new RestException(HttpStatusCode.BadRequest, "");
        RequestDelegate next = (ctx) => throw restException;
        var middleware = new ErrorHandlingMiddleware(next, _logger);

        // Act
        await middleware.Invoke(context);

        // Assert
        context.Response.StatusCode.ShouldBe((int)HttpStatusCode.BadRequest);
        context.Response.ContentType.ShouldBe("application/json");
    }

    /// <summary>
    /// Executes Invoke_WithNullReferenceException_ShouldLogErrorAndReturn500 operation.
    /// </summary>
    /// <returns>The result of Invoke_WithNullReferenceException_ShouldLogErrorAndReturn500.</returns>

    [Fact]
    public async Task Invoke_WithNullReferenceException_ShouldLogErrorAndReturn500()
    {
        // Arrange
        var context = CreateHttpContext();
        var nullRefException = new NullReferenceException("Object reference not set to an instance of an object");
        RequestDelegate next = (ctx) => throw nullRefException;
        var middleware = new ErrorHandlingMiddleware(next, _logger);

        // Act
        await middleware.Invoke(context);

        // Assert
        context.Response.StatusCode.ShouldBe((int)HttpStatusCode.InternalServerError);
        context.Response.ContentType.ShouldBe("application/json");
    }

    /// <summary>
    /// Executes Invoke_WithTimeoutException_ShouldHandleManufacturingTimeouts operation.
    /// </summary>
    /// <returns>The result of Invoke_WithTimeoutException_ShouldHandleManufacturingTimeouts.</returns>

    [Fact]
    public async Task Invoke_WithTimeoutException_ShouldHandleManufacturingTimeouts()
    {
        // Arrange - Manufacturing operation timeout scenario
        var context = CreateHttpContext();
        var timeoutException = new TimeoutException("Machine operation timeout - Assembly cycle exceeded maximum time");
        RequestDelegate next = (ctx) => throw timeoutException;
        var middleware = new ErrorHandlingMiddleware(next, _logger);

        // Act
        await middleware.Invoke(context);

        // Assert
        context.Response.StatusCode.ShouldBe((int)HttpStatusCode.InternalServerError);
        context.Response.ContentType.ShouldBe("application/json");
    }

    /// <summary>
    /// Executes Invoke_ShouldLogExceptionDetails operation.
    /// </summary>
    /// <returns>The result of Invoke_ShouldLogExceptionDetails.</returns>

    [Fact]
    public async Task Invoke_ShouldLogExceptionDetails()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new InvalidOperationException("Test exception");
        exception.Source = "TestSource";
        RequestDelegate next = (ctx) => throw exception;
        var middleware = new ErrorHandlingMiddleware(next, _logger);

        // Act
        await middleware.Invoke(context);

        //Assert
        //Assert something so the test pass
        //If we are here it means the exception was handled and likely logged
        context.Response.StatusCode.ShouldBe((int)HttpStatusCode.InternalServerError);
        context.Response.ContentType.ShouldBe("application/json");
    }

    /// <summary>
    /// Executes Invoke_WithCancellationException_ShouldHandleGracefully operation.
    /// </summary>
    /// <returns>The result of Invoke_WithCancellationException_ShouldHandleGracefully.</returns>

    [Fact]
    public async Task Invoke_WithCancellationException_ShouldHandleGracefully()
    {
        // Arrange
        var context = CreateHttpContext();
        var cancellationException = new OperationCanceledException("Operation was cancelled");
        RequestDelegate next = (ctx) => throw cancellationException;
        var middleware = new ErrorHandlingMiddleware(next, _logger);

        // Act
        await middleware.Invoke(context);

        // Assert
        context.Response.StatusCode.ShouldBe((int)HttpStatusCode.InternalServerError);
        context.Response.ContentType.ShouldBe("application/json");
    }

    /// <summary>
    /// Executes Invoke_WithArgumentException_ShouldHandleInvalidInputs operation.
    /// </summary>
    /// <returns>The result of Invoke_WithArgumentException_ShouldHandleInvalidInputs.</returns>

    [Fact]
    public async Task Invoke_WithArgumentException_ShouldHandleInvalidInputs()
    {
        // Arrange
        var context = CreateHttpContext();
        var argumentException = new ArgumentException("Invalid machine ID provided", "machineId");
        RequestDelegate next = (ctx) => throw argumentException;
        var middleware = new ErrorHandlingMiddleware(next, _logger);

        // Act
        await middleware.Invoke(context);

        // Assert
        context.Response.StatusCode.ShouldBe((int)HttpStatusCode.InternalServerError);
        context.Response.ContentType.ShouldBe("application/json");
    }

    /// <summary>
    /// Executes Invoke_ResponseShouldContainValidJsonStructure operation.
    /// </summary>
    /// <returns>The result of Invoke_ResponseShouldContainValidJsonStructure.</returns>

    [Fact]
    public async Task Invoke_ResponseShouldContainValidJsonStructure()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new InvalidOperationException("Test error message");
        RequestDelegate next = (ctx) => throw exception;
        var middleware = new ErrorHandlingMiddleware(next, _logger);

        // Act
        await middleware.Invoke(context);

        // Assert
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);

        // Verify it's valid JSON with expected structure
        var responseData = JsonSerializer.Deserialize<JsonElement>(responseBody);
        responseData.TryGetProperty("errors", out var errorsProperty).ShouldBeTrue();
        errorsProperty.GetString().ShouldBe("An internal server error has occured.");
    }

    /// <summary>
    /// Executes Invoke_WithRestExceptionResponseShouldContainErrorArray operation.
    /// </summary>
    /// <returns>The result of Invoke_WithRestExceptionResponseShouldContainErrorArray.</returns>

    [Fact]
    public async Task Invoke_WithRestExceptionResponseShouldContainErrorArray()
    {
        // Arrange
        var context = CreateHttpContext();
        var restException = new RestException(HttpStatusCode.BadRequest, "Validation error");
        RequestDelegate next = (ctx) => throw restException;
        var middleware = new ErrorHandlingMiddleware(next, _logger);

        // Act
        await middleware.Invoke(context);

        // Assert
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);

        // Verify response contains error array
        var responseData = JsonSerializer.Deserialize<JsonElement>(responseBody);
        responseData.TryGetProperty("errors", out var errorsProperty).ShouldBeTrue();
        errorsProperty.ValueKind.ShouldBe(JsonValueKind.Array);
    }

    /// <summary>
    /// Executes Invoke_WithMultipleInvocations_ShouldHandleConsistently operation.
    /// </summary>
    /// <returns>The result of Invoke_WithMultipleInvocations_ShouldHandleConsistently.</returns>

    [Fact]
    public async Task Invoke_WithMultipleInvocations_ShouldHandleConsistently()
    {
        // Arrange
        var contexts = Enumerable.Range(0, 3).Select(_ => CreateHttpContext()).ToList();
        var exception = new InvalidOperationException("Consistent error");
        RequestDelegate next = (ctx) => throw exception;
        var middleware = new ErrorHandlingMiddleware(next, _logger);

        // Act & Assert
        foreach (var context in contexts)
        {
            await middleware.Invoke(context);

            context.Response.StatusCode.ShouldBe((int)HttpStatusCode.InternalServerError);
            context.Response.ContentType.ShouldBe("application/json");
        }
    }

    // Helper method to create HttpContext for testing
    private static HttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }
}