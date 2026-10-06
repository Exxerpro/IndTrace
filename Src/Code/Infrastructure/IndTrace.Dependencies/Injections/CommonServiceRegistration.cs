// <copyright file="CommonServiceRegistration.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.UserService;
using IndTrace.Domain.Interfaces;
using IndTrace.Identity.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ILogger = Serilog.ILogger;
using IndTrace.Identity.Users;

namespace IndTrace.Dependencies.Injections;

/// <summary>
/// Provides extension methods for registering common services in the IndTrace Monitor application.
/// </summary>
public static class CommonServiceRegistration
{
    /// <summary>
    /// Adds IndTrace Identity services including authentication, authorization, and database context.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="environment">The host environment; used to gate development-only diagnostics (EF sensitive-data logging).</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddIndTraceIdentity(this IServiceCollection services, IConfiguration configuration, ILogger logger, IHostEnvironment environment)
    {
        // NOTE: the legacy public Identity.Users.IdentityUserAccessor (ctor UserManager<IndTraceUser>)
        // is intentionally NOT registered — nothing injects it (the Monitor's account pages use their
        // own internal IdentityUserAccessor over UserManager<ApplicationUser>), and registering it made
        // the container demand an unregistered UserManager<IndTraceUser>, failing ValidateOnBuild.
        // Wiring the Monitor's internal account accessors is a separate account-pages concern.
        services.AddScoped<Identity.Users.IdentityRedirectManager>();
        services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

        services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies();

        var connectionString = configuration.GetConnectionString(nameof(IndTraceDbIdentity))
                               ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<IndTraceDbIdentity>(options =>
        {
            options.UseSqlServer(connectionString, actions =>
            {
                actions.MigrationsAssembly(typeof(IndTraceDbIdentity).Assembly.FullName)
                    .EnableRetryOnFailure(maxRetryCount: 4, maxRetryDelay: TimeSpan.FromSeconds(2),
                        errorNumbersToAdd: Array.Empty<int>());
            })
                .EnableDetailedErrors()
                .ConfigureWarnings(warnings =>
                {
                    warnings.Default(WarningBehavior.Log).Log(new[]
                    {
                        CoreEventId.SaveChangesCompleted,
                        CoreEventId.FirstWithoutOrderByAndFilterWarning,
                        CoreEventId.RowLimitingOperationWithoutOrderByWarning,
                    });
                });

            // SECURITY (#70): sensitive-data logging writes entity values (user data, credentials) into
            // logs. This is a Development-only diagnostic; it must never be enabled in production.
            if (environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
            }
        });

        services.AddDatabaseDeveloperPageExceptionFilter();
        services
            .AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<IndTraceDbIdentity>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddScoped<IIndTraceUserService, IndTraceUserService>();

        // UserManager<ApplicationUser> and SignInManager<ApplicationUser> are already
        // registered by AddIdentityCore(...).AddEntityFrameworkStores(...).AddSignInManager() above.
        // The concrete ApplicationUser (not the IIndTraceApplicationUser interface) is used because
        // AddEntityFrameworkStores<IndTraceDbIdentity> binds the user store to IdentityDbContext<ApplicationUser>;
        // an interface cannot satisfy the IdentityUser<TKey> constraint (that mismatch crashed startup).
        // The former explicit registrations here re-registered UserManager as a factory that resolved
        // itself (a self-referential stack overflow at resolve time) plus a redundant self-mapping; both
        // are removed so the framework registration is the single source of truth.

        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireAdministratorRole", policy => policy.RequireRole("Administrator"));
        });

        return services;
    }
}