// <copyright file="OeeDependencyRegistration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using FluentValidation;
using IndTrace.Application.Mediator;
using IndTrace.Application.Oee.Commands;
using IndTrace.Application.Oee.Queries;
using IndTrace.Application.Oee.Services;
using IndTrace.Application.Repository;
using IndTrace.Domain.Interfaces;
using IndTrace.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace IndTrace.Dependencies.Services;

/// <summary>
/// Registers all OEE-related dependencies in the dependency injection container.
/// </summary>
public static class OeeDependencyRegistration
{
    /// <summary>
    /// Adds OEE services to the dependency injection container.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <returns>Service collection for chaining.</returns>
    public static IServiceCollection AddOeeServices(this IServiceCollection services)
    {
        // Domain Services
        // services.AddScoped<IOeeCalculationService, OeeCalculationService>(); // TODO: Implement OeeCalculationService

        // Command Handlers
        services.AddScoped<ICommandHandler<CalculateOeeCommand, OeeMetrics>, CalculateOeeCommandHandler>();

        // QueryAsync Handlers
        services.AddScoped<IPerformanceQueryHandler<GetOeeHistoryQuery, GetOeeHistoryResponse>, GetOeeHistoryPerformanceQueryHandler>();

        // Validators
        services.AddScoped<IValidator<CalculateOeeCommand>, CalculateOeeCommandValidator>();
        services.AddScoped<IValidator<GetOeeHistoryQuery>, GetOeeHistoryQueryValidator>();

        // Application Services
        services.AddScoped<IOeeService, OeeService>();

        return services;
    }

    /// <summary>
    /// Adds OEE repository services to the dependency injection container.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <returns>Service collection for chaining.</returns>
    /// <remarks>
    /// Repository implementation should be provided by the infrastructure layer
    /// This is now handled in the main application startup.
    /// </remarks>
    public static IServiceCollection AddOeeRepositories(this IServiceCollection services)
    {
        // Repository implementation is registered in the application layer
        // to avoid circular dependencies between Dependencies and DataStore
        return services;
    }

    /// <summary>
    /// Validates that all required OEE services are registered.
    /// </summary>
    /// <param name="serviceProvider">Service provider to validate.</param>
    /// <returns>Validation result with any missing services.</returns>
    public static OeeServicesValidationResult ValidateOeeServices(this IServiceProvider serviceProvider)
    {
        var missingServices = new List<string>();

        try
        {
            // Check domain services
            var oeeCalculationService = serviceProvider.GetService<IOeeCalculationService>();
            if (oeeCalculationService == null)
                missingServices.Add(nameof(IOeeCalculationService));

            // Check command handlers
            var calculateOeeHandler = serviceProvider.GetService<ICommandHandler<CalculateOeeCommand, OeeMetrics>>();
            if (calculateOeeHandler == null)
                missingServices.Add($"IPerformanceCommandHandler<{nameof(CalculateOeeCommand)}, {nameof(OeeMetrics)}>");

            // Check query handlers
            var getHistoryHandler = serviceProvider.GetService<IPerformanceQueryHandler<GetOeeHistoryQuery, GetOeeHistoryResponse>>();
            if (getHistoryHandler == null)
                missingServices.Add($"IPerformanceQueryHandler<{nameof(GetOeeHistoryQuery)}, {nameof(GetOeeHistoryResponse)}>");

            // Check validators
            var calculateOeeValidator = serviceProvider.GetService<IValidator<CalculateOeeCommand>>();
            if (calculateOeeValidator == null)
                missingServices.Add($"IValidator<{nameof(CalculateOeeCommand)}>");

            var getHistoryValidator = serviceProvider.GetService<IValidator<GetOeeHistoryQuery>>();
            if (getHistoryValidator == null)
                missingServices.Add($"IValidator<{nameof(GetOeeHistoryQuery)}>");

            // Check application services
            var oeeService = serviceProvider.GetService<IOeeService>();
            if (oeeService == null)
                missingServices.Add(nameof(IOeeService));

            // Check repository (optional - might be registered elsewhere)
            var oeeRepository = serviceProvider.GetService<IOeeRepository>();
            if (oeeRepository == null)
                missingServices.Add($"{nameof(IOeeRepository)} (Warning: Should be registered in Infrastructure layer)");

            return new OeeServicesValidationResult
            {
                IsValid = missingServices.Count == 0,
                MissingServices = missingServices,
                ValidationPerformed = DateTime.UtcNow,
            };
        }
        catch (Exception ex)
        {
            return new OeeServicesValidationResult
            {
                IsValid = false,
                MissingServices = new[] { $"Validation failed: {ex.Message}" },
                ValidationPerformed = DateTime.UtcNow,
            };
        }
    }
}