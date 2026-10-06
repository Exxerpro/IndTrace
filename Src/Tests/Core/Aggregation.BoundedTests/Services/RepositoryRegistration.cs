// <copyright file="RepositoryRegistration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.Services;

/// <summary>
/// Provides extension methods for registering repository services with the dependency injection container.
/// </summary>
public static class RepositoryRegistration
{
    /// <summary>
    /// Registers a repository implementation for the specified interface with the given service lifetime.
    /// Intended for repositories that support both read and write operations.
    /// </summary>
    /// <typeparam name="TInterface">The repository interface type. Must implement <see cref="IRepository{T}"/>.</typeparam>
    /// <typeparam name="TImplementation">The concrete implementation type of the repository.</typeparam>
    /// <typeparam name="T">The entity type for the repository.</typeparam>
    /// <param name="services">The service collection to add the registration to.</param>
    /// <param name="lifetime">The desired service lifetime (Singleton, Scoped, or Transient).</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddRepository<TInterface, TImplementation, T>
        (this IServiceCollection services, ServiceLifetime lifetime)
        where TInterface : class, IRepository<T>
        where TImplementation : class, TInterface
        where T : class, IndTrace.Domain.Interfaces.IPersistable
    {
        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                services.AddSingleton<TInterface, TImplementation>();
                break;

            case ServiceLifetime.Scoped:
                services.AddScoped<TInterface, TImplementation>();

                //.AddScoped<IRepository<OeeRegister>>(sp =>
                // {
                //     var factory = sp.GetRequiredService<IIndTraceDbContextFactory>();
                //     var logger = sp.GetRequiredService<ILogger<Repository<OeeRegister>>>();
                //     return new Repository<OeeRegister>(factory, logger);
                // })
                break;

            case ServiceLifetime.Transient:
                services.AddTransient<TInterface, TImplementation>();

                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null!);
        }
        return services;
    }

    /// <summary>
    /// Registers a read-only repository implementation for the specified interface with the given service lifetime.
    /// Intended for repositories that only support read operations.
    /// </summary>
    /// <typeparam name="TInterface">The read-only repository interface type. Must implement <see cref="IReadOnlyRepository{T}"/>.</typeparam>
    /// <typeparam name="TImplementation">The concrete implementation type of the read-only repository.</typeparam>
    /// <typeparam name="T">The entity type for the repository.</typeparam>
    /// <param name="services">The service collection to add the registration to.</param>
    /// <param name="lifetime">The desired service lifetime (Singleton, Scoped, or Transient).</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddReadOnlyRepository<TInterface, TImplementation, T>
        (this IServiceCollection services, ServiceLifetime lifetime)
        where TInterface : class, IReadOnlyRepository<T>
        where TImplementation : class, TInterface
        where T : class, IndTrace.Domain.Interfaces.IPersistable
    {
        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                services.AddSingleton<TInterface, TImplementation>();
                break;

            case ServiceLifetime.Scoped:
                services.AddScoped<TInterface, TImplementation>();
                break;

            case ServiceLifetime.Transient:
                services.AddTransient<TInterface, TImplementation>();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null!);
        }
        return services;
    }
}