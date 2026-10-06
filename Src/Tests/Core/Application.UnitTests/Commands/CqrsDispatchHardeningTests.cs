// <copyright file="CqrsDispatchHardeningTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Commands;

/// <summary>
/// Regression tests for GitHub issue #78 (P1): CQRS dispatch and validation must fail CLOSED.
/// <list type="bullet">
/// <item>A validation failure short-circuits the pipeline — the handler is never invoked and a
/// railway <see cref="Result"/>/<see cref="Result{T}"/> failure is returned.</item>
/// <item>A <see cref="Result{T}"/> handler failure/exception survives the MonitorRequestDispatcher
/// catch path (no <see cref="InvalidCastException"/> replacing the real failure).</item>
/// <item>A throwing gateway handler yields a <see cref="Result"/> failure instead of escaping into
/// the PLC worker loop, and a null request is guarded.</item>
/// </list>
/// </summary>
public class CqrsDispatchHardeningTests
{
    /// <summary>A minimal payload for a Result-returning request.</summary>
    public sealed class TestPayload
    {
        /// <summary>Gets or sets a marker value.</summary>
        public int Value { get; set; }
    }

    /// <summary>A request whose response is <see cref="Result{TestPayload}"/>.</summary>
    public sealed class ResultOfTRequest : IRequest<Result<TestPayload>>;

    /// <summary>A request whose response is the non-generic <see cref="Result"/>.</summary>
    public sealed class ResultRequest : IRequest<Result>;

    /// <summary>A Monitor request whose handler throws, to exercise the dispatcher catch path.</summary>
    public sealed class ThrowingMonitorRequest : IMonitorRequest<TestPayload>;

    /// <summary>A validator that always fails, to force the short-circuit branch.</summary>
    public sealed class AlwaysFailingValidator<T> : AbstractValidator<T>
    {
        /// <summary>Initializes a new instance of the <see cref="AlwaysFailingValidator{T}"/> class.</summary>
        public AlwaysFailingValidator()
        {
            this.RuleFor(x => x).Must(_ => false).WithMessage("forced validation failure");
        }
    }

    [Fact]
    public async Task ValidationBehavior_WithFailingValidator_ResultOfT_ShortCircuitsHandler_ReturnsFailure()
    {
        // Arrange
        var validators = new List<IValidator<ResultOfTRequest>> { new AlwaysFailingValidator<ResultOfTRequest>() };
        var logger = XUnitLogger.CreateLogger<ValidationBehavior<ResultOfTRequest, Result<TestPayload>>>();
        var behavior = new ValidationBehavior<ResultOfTRequest, Result<TestPayload>>(validators, logger);

        var handlerInvoked = false;
        RequestFunctionalHandlerDelegate<Result<TestPayload>> next = () =>
        {
            handlerInvoked = true;
            return Task.FromResult(Result<TestPayload>.Success(new TestPayload { Value = 42 }));
        };

        // Act
        var result = await behavior.HandleAsync(new ResultOfTRequest(), next, TestContext.Current.CancellationToken);

        // Assert - fail closed: handler NOT invoked, railway failure carrying the validator message.
        handlerInvoked.ShouldBeFalse();
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("forced validation failure");
    }

    [Fact]
    public async Task ValidationBehavior_WithFailingValidator_NonGenericResult_ShortCircuitsHandler_ReturnsFailure()
    {
        // Arrange
        var validators = new List<IValidator<ResultRequest>> { new AlwaysFailingValidator<ResultRequest>() };
        var logger = XUnitLogger.CreateLogger<ValidationBehavior<ResultRequest, Result>>();
        var behavior = new ValidationBehavior<ResultRequest, Result>(validators, logger);

        var handlerInvoked = false;
        RequestFunctionalHandlerDelegate<Result> next = () =>
        {
            handlerInvoked = true;
            return Task.FromResult(Result.Success());
        };

        // Act
        var result = await behavior.HandleAsync(new ResultRequest(), next, TestContext.Current.CancellationToken);

        // Assert
        handlerInvoked.ShouldBeFalse();
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("forced validation failure");
    }

    [Fact]
    public async Task MonitorRequestDispatcher_WhenHandlerThrows_ReturnsResultOfTFailure_NoInvalidCastException()
    {
        // Arrange - a handler whose ProcessAsync throws. Before the fix the dispatcher catch did
        // (TResponse)(object)Result.WithFailure(...) which threw InvalidCastException for Result<T>,
        // replacing the real failure with a crash.
        var services = new ServiceCollection();
        var handler = Substitute.For<IMonitorRequestHandler<ThrowingMonitorRequest, TestPayload>>();
        handler.ProcessAsync(Arg.Any<ThrowingMonitorRequest>(), Arg.Any<CancellationToken>())
            .Returns<Task<Result<TestPayload>>>(_ => throw new InvalidOperationException("handler boom"));
        services.AddSingleton(handler);
        var provider = services.BuildServiceProvider();

        var logger = XUnitLogger.CreateLogger<MonitorRequestDispatcher>();
        var dispatcher = new MonitorRequestDispatcher(provider, logger);

        // Act - must NOT throw; must surface a typed Result<TestPayload> failure.
        var result = await dispatcher.ProcessAsync(new ThrowingMonitorRequest(), TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeOfType<Result<TestPayload>>();
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task GatewayCommandDispatcher_WhenHandlerThrows_ReturnsFailure_DoesNotEscape()
    {
        // Arrange - CreateBarCodeCommand handler throws (the live trigger is a duplicate reference
        // name ArgumentException). It must become a Result failure, not escape to the worker loop.
        var services = new ServiceCollection();
        var handler = Substitute.For<IGatewayRequestHandler<CreateBarCodeCommand, TaskGatewayResponseDto>>();
        handler.ProcessAsync(Arg.Any<CreateBarCodeCommand>(), Arg.Any<CancellationToken>())
            .Returns<Task<Result<TaskGatewayResponseDto>>>(_ => throw new ArgumentException("duplicate reference name"));
        services.AddSingleton(handler);
        var logger = XUnitLogger.CreateLogger<GatewayCommandDispatcher>();
        var provider = services.BuildServiceProvider();
        var dispatcher = new GatewayCommandDispatcher(provider.GetRequiredService<IServiceScopeFactory>(), logger);

        // Act - must NOT throw.
        var result = await dispatcher.ProcessAsync(new CreateBarCodeCommand(), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task GatewayCommandDispatcher_WithNullRequest_ReturnsFailure_NotNullReferenceException()
    {
        // Arrange
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var logger = XUnitLogger.CreateLogger<GatewayCommandDispatcher>();
        var dispatcher = new GatewayCommandDispatcher(scopeFactory, logger);

        // Act - a null request must be guarded, not NRE at request.GetType().
        var result = await dispatcher.ProcessAsync<TaskGatewayResponseDto>(null!, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("Gateway request was null.");
    }
}
