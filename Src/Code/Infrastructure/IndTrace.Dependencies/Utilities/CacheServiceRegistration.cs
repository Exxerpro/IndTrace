// <copyright file="CacheServiceRegistration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Repository;
using IndTrace.Persistence.Caching;
using System.Text.Json;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

namespace IndTrace.Dependencies.Utilities;

/// <summary>
/// Single shared registration for the IndTrace read-cache service (#216).
/// </summary>
/// <remarks>
/// Every production composition root (Monitor via <c>AddCommonServices</c>, the Communications PLC gateway via
/// its own <c>Program.cs</c>) must register <see cref="ICacheService"/> through this ONE extension so the
/// "Caching:Toggle" kill-switch decorator (#116) can never be silently dropped again. Pre-#216 the
/// Communications host registered the raw <c>FusionCacheService</c> directly, which made
/// <c>Caching:Toggle:Enabled=false</c> a no-op in that host.
/// </remarks>
public static class CacheServiceRegistration
{
    /// <summary>
    /// Registers FusionCache (with the IndTrace SmartEnum / strongly-typed-id / BarCodeLabel serializer
    /// converters) and exposes <see cref="ICacheService"/> as the <c>CacheToggleCacheService</c> kill-switch
    /// decorator bound to the "Caching:Toggle" configuration section.
    /// </summary>
    /// <param name="services">The service collection to add the registrations to.</param>
    /// <param name="configuration">The application configuration (source of the "Caching:Toggle" section).
    /// When the section is absent the toggle defaults to enabled, so behavior is byte-identical to the
    /// undecorated cache.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddToggleableCacheService(this IServiceCollection services, IConfiguration configuration)
    {
        // Add FusionCache and serializer (used by ICacheService implementations)
        services.AddFusionCache()
            .WithSerializer(
                new FusionCacheSystemTextJsonSerializer(
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = false,
                        Converters =
                        {
                            new IndTrace.Domain.ValueObjects.EnumModelJsonConverter(), // Critical: Support for SmartEnum serialization (#188: moved to Domain)
                            new IndTrace.Domain.ValueObjects.StronglyTypedIdJsonConverterFactory(), // Story 35.D2: any IIntId struct <-> bare int cache blob
                            new IndTrace.Domain.ValueObjects.BarCodeLabelJsonConverter(), // Story 27.2b-2: BarCode.Label VO <-> bare string cache blob
                        }
                    }));

        // Register concrete inner cache service (FusionCache-based)
        services.AddSingleton<FusionCacheService>();

        // Bind toggle options and expose ICacheService as a toggle decorator (#116 kill-switch).
        // CacheToggleOptions.Enabled defaults to true, so an absent section keeps caching ON — behavior is
        // byte-identical to the undecorated registration until the switch is explicitly flipped OFF.
        services.Configure<CacheToggleOptions>(configuration.GetSection("Caching:Toggle"));
        services.AddSingleton<ICacheService>(sp =>
        {
            var toggleOptions = sp.GetRequiredService<IOptions<CacheToggleOptions>>().Value;
            var logger = sp.GetRequiredService<ILogger<CacheToggleCacheService>>();
            ICacheService inner = sp.GetRequiredService<FusionCacheService>();
            return new CacheToggleCacheService(inner, toggleOptions, logger);
        });

        return services;
    }
}
