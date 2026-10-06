// <copyright file="InterceptorServiceCollectionExtensions.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Dependencies.Interceptors;

/// <summary>
/// Provides extension methods for registering services with dynamic proxy interception in the dependency injection container.
/// </summary>
public static class InterceptorServiceCollectionExtensions
{
    private static readonly ProxyGenerator ProxyGenerator = new();

    /// <summary>
    /// Registers a scoped service with an interceptor for the specified interface and implementation.
    /// </summary>
    /// <typeparam name="TInterface">The interface type to register.</typeparam>
    /// <typeparam name="TImplementation">The implementation type to register.</typeparam>
    /// <typeparam name="TInterceptor">The interceptor type to use.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddInterceptedScoped<TInterface, TImplementation, TInterceptor>(
        this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
        where TInterceptor : class, IInterceptor
    {
        services.AddScoped<TImplementation>();
        services.AddScoped<TInterceptor>();
        services.AddScoped<TInterface>(provider =>
        {
            var implementation = provider.GetRequiredService<TImplementation>();
            var interceptor = provider.GetRequiredService<TInterceptor>();

            return ProxyGenerator.CreateInterfaceProxyWithTarget<TInterface>(implementation, interceptor);
        });

        return services;
    }

    /// <summary>
    /// Registers a singleton service with an interceptor for the specified interface and implementation.
    /// </summary>
    /// <typeparam name="TInterface">The interface type to register.</typeparam>
    /// <typeparam name="TImplementation">The implementation type to register.</typeparam>
    /// <typeparam name="TInterceptor">The interceptor type to use.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddInterceptedSingleton<TInterface, TImplementation, TInterceptor>(
        this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
        where TInterceptor : class, IInterceptor
    {
        services.AddSingleton<TImplementation>();
        services.AddSingleton<TInterceptor>();
        services.AddSingleton<TInterface>(provider =>
        {
            var implementation = provider.GetRequiredService<TImplementation>();
            var interceptor = provider.GetRequiredService<TInterceptor>();

            return ProxyGenerator.CreateInterfaceProxyWithTarget<TInterface>(implementation, interceptor);
        });

        return services;
    }

    /// <summary>
    /// Registers a transient service with an interceptor for the specified interface and implementation.
    /// </summary>
    /// <typeparam name="TInterface">The interface type to register.</typeparam>
    /// <typeparam name="TImplementation">The implementation type to register.</typeparam>
    /// <typeparam name="TInterceptor">The interceptor type to use.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddInterceptedTransient<TInterface, TImplementation, TInterceptor>(
        this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
        where TInterceptor : class, IInterceptor
    {
        services.AddTransient<TImplementation>();
        services.AddTransient<TInterceptor>();
        services.AddTransient<TInterface>(provider =>
        {
            var implementation = provider.GetRequiredService<TImplementation>();
            var interceptor = provider.GetRequiredService<TInterceptor>();

            return ProxyGenerator.CreateInterfaceProxyWithTarget<TInterface>(implementation, interceptor);
        });

        return services;
    }
}