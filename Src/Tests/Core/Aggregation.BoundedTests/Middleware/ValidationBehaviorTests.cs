// <copyright file="ValidationBehaviorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.Middleware;

/// <summary>
/// Unit tests for ValidationBehavior - Testing the pipeline behavior with real validators
/// </summary>
public class ValidationBehaviorTests : DependenciesFactory
{
    public ValidationBehaviorTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    // Simple test validator that always passes validation
    public class AlwaysValidValidator<T> : AbstractValidator<T>
    {
        public AlwaysValidValidator()
        {
            // No validation rules = always valid
        }
    }

    // Simple test validator that always fails validation
    public class AlwaysInvalidValidator<T> : AbstractValidator<T>
    {
        public AlwaysInvalidValidator()
        {
            RuleFor(x => x).Must(x => true == false).WithMessage("Test validation failure");
        }
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldPassValidationAndProceedToNext()
    {
        await Initialization;

        // Arrange - Use real validator that passes validation

        var validator = new AlwaysValidValidator<GetAppDetailsMonitorRequest>();
        var validators = new List<IValidator<GetAppDetailsMonitorRequest>> { validator };

        var logger = XUnitLogger.CreateLogger<ValidationBehavior<GetAppDetailsMonitorRequest, ApplicationConfiguration>>();

        var behavior = new ValidationBehavior<GetAppDetailsMonitorRequest, ApplicationConfiguration>(validators, logger);

        var request = new GetAppDetailsMonitorRequest(false);
        var expectedResponse = new ApplicationConfiguration();

        RequestFunctionalHandlerDelegate<ApplicationConfiguration> next = () => Task.FromResult(expectedResponse);

        // Act
        var result = await behavior.HandleAsync(request, next, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBe(expectedResponse);
    }

    [Fact]
    public async Task Handle_WithMultipleValidators_ShouldRunAllValidators()
    {
        await Initialization;

        // Arrange - Use multiple real validators that pass validation

        var validator1 = new AlwaysValidValidator<GetAppDetailsMonitorRequest>();
        var validator2 = new AlwaysValidValidator<GetAppDetailsMonitorRequest>();
        var validators = new List<IValidator<GetAppDetailsMonitorRequest>> { validator1, validator2 };
        var logger = XUnitLogger.CreateLogger<ValidationBehavior<GetAppDetailsMonitorRequest, ApplicationConfiguration>>();
        var behavior = new ValidationBehavior<GetAppDetailsMonitorRequest, ApplicationConfiguration>(validators, logger);

        var request = new GetAppDetailsMonitorRequest(true);
        var expectedResponse = new ApplicationConfiguration();

        RequestFunctionalHandlerDelegate<ApplicationConfiguration> next = () => Task.FromResult(expectedResponse);

        // Act
        var result = await behavior.HandleAsync(request, next, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBe(expectedResponse);
    }

    [Fact]
    public async Task Handle_WithFailingValidator_ShouldThrowValidationException()
    {
        await Initialization;

        // Arrange - Use real validator that fails validation

        var validator = new AlwaysInvalidValidator<GetAppDetailsMonitorRequest>();
        var validators = new List<IValidator<GetAppDetailsMonitorRequest>> { validator };
        var logger = XUnitLogger.CreateLogger<ValidationBehavior<GetAppDetailsMonitorRequest, ApplicationConfiguration>>();
        var behavior = new ValidationBehavior<GetAppDetailsMonitorRequest, ApplicationConfiguration>(validators, logger);

        var request = new GetAppDetailsMonitorRequest(false);

        var handlerInvoked = false;
        RequestFunctionalHandlerDelegate<ApplicationConfiguration> next = () =>
        {
            handlerInvoked = true;
            return Task.FromResult(new ApplicationConfiguration());
        };

        // Act & Assert - a genuinely non-Result response fails CLOSED on invalid input: the behavior
        // short-circuits with a ValidationException and the handler is NEVER invoked (no fail-open).
        await Should.ThrowAsync<FluentValidation.ValidationException>(async () =>
            await behavior.HandleAsync(request, next, TestContext.Current.CancellationToken));

        handlerInvoked.ShouldBeFalse();
    }
}