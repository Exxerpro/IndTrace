// <copyright file="GenericCrudCommand.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Generic.Commands.Delete;
using IndTrace.Application.Generic.Commands.List;

namespace IndTrace.Application.Generic.CRUD;

/// <summary>
/// Ultra-generic helper class for I²TDD (Interface-based Test-Driven Development) testing.
/// Supports ANY entity type and command type combination through pure reflection-based approach.
/// This helper enables testing generic CRUD contracts without concrete type dependencies.
/// </summary>
/// <typeparam name="TEntity">Any entity type that meets the constraints.</typeparam>
/// <typeparam name="TCommand">Any command type that meets the constraints.</typeparam>
public static class GenericCrudCommand<TEntity, TCommand>
    where TEntity : class, new()
    where TCommand : class, new()
{
    /// <summary>
    /// Creates a generic handler instance for any CRUD operation type.
    /// Uses reflection to instantiate handlers without concrete type dependencies.
    /// </summary>
    /// <typeparam name="THandler">The handler type (Create, List, Update, Delete, Detail).</typeparam>
    /// <param name="repository">The repository instance for the entity.</param>
    /// <returns>The instantiated handler instance.</returns>
    public static THandler CreateGenericHandler<THandler>(object repository)
        where THandler : class
    {
        var handlerType = typeof(THandler);

        // Handle generic handler types that need type parameters
        if (handlerType.IsGenericTypeDefinition)
        {
            handlerType = handlerType.MakeGenericType(typeof(TCommand), typeof(TEntity));
        }

        var handler = Activator.CreateInstance(handlerType, repository)
            ?? throw new InvalidOperationException($"Could not create an instance of handler type '{handlerType.Name}'.");
        return (THandler)handler;
    }

    /// <summary>
    /// Creates a test command instance with default or specified property values.
    /// Supports any command type through reflection-based property setting.
    /// </summary>
    /// <param name="propertyValues">Optional dictionary of property names and values to set.</param>
    /// <returns>The configured command instance.</returns>
    public static TCommand CreateTestCommand(Dictionary<string, object>? propertyValues = null, ILogger? logger = default)
    {
        var command = new TCommand();

        // Set common default values for known properties
        SetDefaultPropertyValues(command);

        if (propertyValues is not null)
        {
            foreach (var (propertyName, value) in propertyValues)
            {
                logger?.LogInformation("Setting property {PropertyName} to value {Value}", propertyName, value);
                var property = typeof(TCommand).GetProperty(propertyName);
                property?.SetValue(command, value);
            }
        }

        return command;
    }

    /// <summary>
    /// Invokes any handler method generically using reflection.
    /// Supports ProcessAsync, CreateAsync, ListProcessAsync, UpdateAsync, etc.
    /// </summary>
    /// <param name="handler">The handler instance.</param>
    /// <param name="methodName">The method name to invoke.</param>
    /// <param name="command">The command parameter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The task result from the method invocation.</returns>
    public static async Task<object> InvokeHandlerMethodAsync(
        object handler,
        string methodName,
        object command,
        CancellationToken cancellationToken)
    {
        var handlerType = handler.GetType();
        var method = handlerType.GetMethod(methodName);

        if (method == null)
        {
            throw new InvalidOperationException($"Method '{methodName}' not found on handler type '{handlerType.Name}'");
        }

        var task = method.Invoke(handler, new[] { command, cancellationToken }) as Task
            ?? throw new InvalidOperationException($"Handler method '{methodName}' did not return a Task.");
        await task;

        // Get the result from the Task<T>
        var resultProperty = task.GetType().GetProperty("Result")
            ?? throw new InvalidOperationException($"Handler method '{methodName}' task does not expose a 'Result' property.");
        return resultProperty.GetValue(task)
            ?? throw new InvalidOperationException($"Handler method '{methodName}' produced a null result.");
    }

    /// <summary>
    /// Extracts the result value from a Result&lt;T&gt; object using reflection.
    /// Handles both successful and failed results generically.
    /// </summary>
    /// <param name="result">The Result&lt;T&gt; object to extract from.</param>
    /// <returns>A tuple containing (isSuccess, value, errors).</returns>
    public static (bool IsSuccess, object? Value, string[]? Errors) ExtractResult(object result)
    {
        var resultType = result.GetType();

        var isSuccessProperty = resultType.GetProperty("IsSuccess");
        var valueProperty = resultType.GetProperty("Value");
        var errorsProperty = resultType.GetProperty("Errors") ?? resultType.GetProperty("ErrorMessages");

        var isSuccess = (bool)(isSuccessProperty?.GetValue(result) ?? false);
        var value = valueProperty?.GetValue(result);
        var errors = errorsProperty?.GetValue(result) as string[];

        return (isSuccess, value, errors);
    }

    /// <summary>
    /// Validates that a handler implements expected interface contracts.
    /// Useful for ensuring generic handlers maintain consistent contracts.
    /// </summary>
    /// <param name="handler">The handler instance to validate.</param>
    /// <param name="expectedInterfaces">The interfaces the handler should implement.</param>
    /// <returns>True if all interfaces are implemented, false otherwise.</returns>
    public static bool ValidateHandlerInterfaces(object handler, params Type[] expectedInterfaces)
    {
        var handlerType = handler.GetType();

        return expectedInterfaces.All(expectedInterface =>
        {
            // Handle generic interface matching
            if (expectedInterface.IsGenericTypeDefinition)
            {
                return handlerType.GetInterfaces()
                    .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == expectedInterface);
            }

            return expectedInterface.IsAssignableFrom(handlerType);
        });
    }

    /// <summary>
    /// Creates test data for pagination scenarios.
    /// Useful for testing List operations with various pagination parameters.
    /// </summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="includes">Optional includes for eager loading.</param>
    /// <returns>Property values dictionary for pagination commands.</returns>
    public static Dictionary<string, object> CreatePaginationData(
        int page = 1,
        int pageSize = 20,
        string[]? includes = null)
    {
        return new Dictionary<string, object>
        {
            ["Page"] = page,
            ["PageSize"] = pageSize,
            ["Includes"] = includes ?? Array.Empty<string>()
        };
    }

    /// <summary>
    /// Creates test data for entity identification scenarios.
    /// Useful for testing Detail, Update, and Delete operations.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <returns>Property values dictionary for ID-based commands.</returns>
    public static Dictionary<string, object> CreateEntityIdData(int id)
    {
        return new Dictionary<string, object>
        {
            ["RegisterId"] = id
        };
    }

    /// <summary>
    /// Counts items in any IEnumerable result using reflection.
    /// Handles generic collection results without type constraints.
    /// </summary>
    /// <param name="enumerableResult">The enumerable result to count.</param>
    /// <returns>The number of items in the collection.</returns>
    public static int CountEnumerableResult(object? enumerableResult)
    {
        if (enumerableResult is System.Collections.IEnumerable enumerable)
        {
            return enumerable.Cast<object>().Count();
        }

        return 0;
    }

    /// <summary>
    /// Sets default property values for common command properties.
    /// Handles pagination, identification, and other common scenarios.
    /// </summary>
    /// <param name="command">The command instance to configure.</param>
    private static void SetDefaultPropertyValues(object command)
    {
        var commandType = command.GetType();

        // Set default pagination values
        commandType.GetProperty("Page")?.SetValue(command, 1);
        commandType.GetProperty("PageSize")?.SetValue(command, 20);
        commandType.GetProperty("Includes")?.SetValue(command, Array.Empty<string>());

        // Set default ID for identification-based commands
        // this will induce a buggy behavio0r if the command does not have an RegisterId property
        // of ith t
        //        commandType.GetProperty("RegisterId")?.SetValue(command, 1);
    }
}

/// <summary>
/// Non-generic static helper methods for cross-entity-type operations.
/// Supports I²TDD testing scenarios that work across multiple entity types.
/// </summary>
public static class GenericCrudTestHelper
{
    /// <summary>
    /// Creates a generic handler for any entity type using pure reflection.
    /// Enables testing handlers for multiple entity types in parameterized tests.
    /// </summary>
    /// <param name="handlerGenericType">The generic handler type definition (e.g., typeof(ListQueryHandler&lt;,&gt;)).</param>
    /// <param name="commandType">The command type.</param>
    /// <param name="entityType">The entity type.</param>
    /// <param name="repository">The repository instance.</param>
    /// <returns>The instantiated handler instance.</returns>
    public static object CreateHandlerForEntityType(
        Type handlerGenericType,
        Type commandType,
        Type entityType,
        object repository)
    {
        var concreteHandlerType = handlerGenericType.MakeGenericType(commandType, entityType);
        return Activator.CreateInstance(concreteHandlerType, repository)
            ?? throw new InvalidOperationException($"Could not create an instance of handler type '{concreteHandlerType.Name}'.");
    }

    /// <summary>
    /// Creates a command for any entity type using pure reflection.
    /// Enables creating commands for multiple entity types in parameterized tests.
    /// </summary>
    /// <param name="commandGenericType">The generic command type definition.</param>
    /// <param name="entityType">The entity type.</param>
    /// <param name="propertyValues">Optional property values to set.</param>
    /// <returns>The instantiated command instance.</returns>
    public static object CreateCommandForEntityType(
        Type commandGenericType,
        Type entityType,
        Dictionary<string, object>? propertyValues = null)
    {
        var concreteCommandType = commandGenericType.MakeGenericType(entityType);
        var command = Activator.CreateInstance(concreteCommandType)
            ?? throw new InvalidOperationException($"Could not create an instance of command type '{concreteCommandType.Name}'.");

        if (propertyValues != null)
        {
            foreach (var (propertyName, value) in propertyValues)
            {
                var property = concreteCommandType.GetProperty(propertyName);
                property?.SetValue(command, value);
            }
        }

        return command;
    }

    /// <summary>
    /// Validates that multiple entity types can work with the same generic pattern.
    /// Essential for I²TDD testing that ensures generic contracts work across entity types.
    /// </summary>
    /// <param name="entityTypes">The entity types to validate.</param>
    /// <param name="handlerGenericType">The generic handler type to test.</param>
    /// <param name="repositoryFactory">Factory function to create repository for each entity type.</param>
    /// <returns>True if all entity types work with the pattern, false otherwise.</returns>
    public static bool ValidateGenericPattern(
        Type[] entityTypes,
        Type handlerGenericType,
        Func<Type, object> repositoryFactory)
    {
        return entityTypes.All(entityType =>
        {
            try
            {
                // Use the production TestGenericListCommand for validation
                var repository = repositoryFactory(entityType);
                var testCommandType = typeof(TestGenericListCommand<>).MakeGenericType(entityType);
                var handler = CreateHandlerForEntityType(handlerGenericType, testCommandType, entityType, repository);
                return handler != null;
            }
            catch
            {
                return false;
            }
        });
    }
}

// REMOVED: TestGenericListCommand<T> class moved to test infrastructure
// The test helper class in IndTrace.Aggregation.BoundedTests.Generic.Helpers should be used instead
// This eliminates naming conflicts and consolidates test utilities in their proper location