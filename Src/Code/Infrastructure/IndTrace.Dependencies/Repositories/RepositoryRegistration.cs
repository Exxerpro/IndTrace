// <copyright file="RepositoryRegistration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Repository;
using IndTrace.Persistence.Caching;
using IndTrace.Persistence.Interfaces;
using IndTrace.Persistence.Repositories;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace IndTrace.Dependencies.Repositories;

/// <summary>
/// Provides extension methods for registering repository services with the dependency injection container.
/// </summary>
public static class RepositoryRegistration
{
    /// <summary>
    /// Registers a repository implementation for the specified interface with the given service lifetime.
    /// Intended for repositories that support both read and write operations.
    /// #116: when the composition root also registers an <see cref="ICacheService"/> (production hosts do),
    /// the interface resolves to a <see cref="CacheInvalidatingRepository{T}"/> wrapping the implementation,
    /// so every successful write invalidates the type-tagged read cache. Composition roots WITHOUT an
    /// <see cref="ICacheService"/> (e.g. test hosts) resolve the raw implementation exactly as before.
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
        if (lifetime is not (ServiceLifetime.Singleton or ServiceLifetime.Scoped or ServiceLifetime.Transient))
        {
            throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
        }

        // If no key provided, register as before (unkeyed). The concrete implementation is registered as
        // itself (same construction path DI used before) and the interface resolves through the decorating
        // factory below at the SAME lifetime.
        services.Add(new ServiceDescriptor(typeof(TImplementation), typeof(TImplementation), lifetime));
        services.Add(new ServiceDescriptor(typeof(TInterface), CreateRepository, lifetime));
        return services;

        static object CreateRepository(IServiceProvider serviceProvider)
        {
            var inner = serviceProvider.GetRequiredService<TImplementation>();

            // GetService, not GetRequiredService: composition roots without a cache (unit/aggregation test
            // hosts) must keep resolving the raw repository unchanged.
            var cache = serviceProvider.GetService<ICacheService>();
            if (cache is null)
            {
                return inner;
            }

            var logger = serviceProvider.GetService<ILogger<CacheInvalidatingRepository<T>>>()
                ?? NullLogger<CacheInvalidatingRepository<T>>.Instance;
            var decorated = new CacheInvalidatingRepository<T>(inner, cache, logger);

            // The generic decorator only implements IRepository<T> itself; a wider bespoke repository
            // interface cannot be served by it and falls back to the undecorated implementation.
            return decorated is TInterface ? decorated : inner;
        }
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
                throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
        }
        return services;
    }

    /// <summary>
    /// Registers an append-only repository implementation for the specified interface with the given service lifetime.
    /// Intended for write-once / audit-style entities that must never be updated or deleted in-process.
    /// #116: when the composition root also registers an <see cref="ICacheService"/> (production hosts do),
    /// the interface resolves to a <see cref="CacheInvalidatingAppendOnlyRepository{T}"/> wrapping the
    /// implementation, so every successful append invalidates the type-tagged read cache. Composition roots
    /// WITHOUT an <see cref="ICacheService"/> resolve the raw implementation exactly as before.
    /// </summary>
    /// <typeparam name="TInterface">The append-only repository interface type. Must implement <see cref="IAppendOnlyRepository{T}"/>.</typeparam>
    /// <typeparam name="TImplementation">The concrete implementation type of the append-only repository.</typeparam>
    /// <typeparam name="T">The entity type for the repository.</typeparam>
    /// <param name="services">The service collection to add the registration to.</param>
    /// <param name="lifetime">The desired service lifetime (Singleton, Scoped, or Transient).</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddAppendOnlyRepository<TInterface, TImplementation, T>
        (this IServiceCollection services, ServiceLifetime lifetime)
        where TInterface : class, IAppendOnlyRepository<T>
        where TImplementation : class, TInterface
        where T : class, IndTrace.Domain.Interfaces.IPersistable
    {
        if (lifetime is not (ServiceLifetime.Singleton or ServiceLifetime.Scoped or ServiceLifetime.Transient))
        {
            throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
        }

        // Same decorating shape as AddRepository above: the concrete implementation registered as itself
        // (TryAdd — the same concrete may already be registered by another helper), the interface via a
        // factory that wraps it only when an ICacheService is resolvable.
        services.TryAdd(new ServiceDescriptor(typeof(TImplementation), typeof(TImplementation), lifetime));
        services.Add(new ServiceDescriptor(typeof(TInterface), CreateRepository, lifetime));
        return services;

        static object CreateRepository(IServiceProvider serviceProvider)
        {
            var inner = serviceProvider.GetRequiredService<TImplementation>();

            // GetService, not GetRequiredService: composition roots without a cache must keep resolving the
            // raw repository unchanged.
            var cache = serviceProvider.GetService<ICacheService>();
            if (cache is null)
            {
                return inner;
            }

            var logger = serviceProvider.GetService<ILogger<CacheInvalidatingAppendOnlyRepository<T>>>()
                ?? NullLogger<CacheInvalidatingAppendOnlyRepository<T>>.Instance;
            var decorated = new CacheInvalidatingAppendOnlyRepository<T>(inner, cache, logger);

            // The generic decorator only implements IAppendOnlyRepository<T> itself; a wider bespoke
            // interface cannot be served by it and falls back to the undecorated implementation.
            return decorated is TInterface ? decorated : inner;
        }
    }
}

