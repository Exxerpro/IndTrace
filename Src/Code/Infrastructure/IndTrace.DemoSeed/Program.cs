// <copyright file="Program.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

// Creates a demo-ready IndTrace database.
//
//   dotnet run --project Src/Code/Infrastructure/IndTrace.DemoSeed [-- --reset]
//
// Settings (environment variables, or appsettings.json next to the executable):
//   ConnectionStrings__IndTraceDbContext   data database (required)
//   ConnectionStrings__IndTraceDbIdentity  sign-in database (required)
//   DEMO_PASSWORD                          password of the demo user (required)
//   DEMO_USER                              demo user name and e-mail (default demo@example.com)
//
// It creates the data schema, migrates the sign-in database, writes the demo line, and creates the demo user in the
// Administrator role. --reset deletes both databases first. Without it, a database that already holds stations is
// refused.
using IndTrace.DemoSeed;
using IndTrace.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var reset = args.Contains("--reset");
var builder = Host.CreateApplicationBuilder();
var dataConnection = builder.Configuration.GetConnectionString("IndTraceDbContext");
var identityConnection = builder.Configuration.GetConnectionString("IndTraceDbIdentity");
var demoPassword = builder.Configuration["DEMO_PASSWORD"];
var demoUser = builder.Configuration["DEMO_USER"] is { Length: > 0 } user ? user : "demo@example.com";
if (string.IsNullOrWhiteSpace(dataConnection) || string.IsNullOrWhiteSpace(identityConnection) || string.IsNullOrWhiteSpace(demoPassword))
{
    Console.Error.WriteLine("Set ConnectionStrings__IndTraceDbContext, ConnectionStrings__IndTraceDbIdentity and DEMO_PASSWORD.");
    return 2;
}

builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
builder.Services.AddDbContext<IndTraceDbContext>(options => options.UseSqlServer(dataConnection));
builder.Services.AddDbContext<IndTraceDbIdentity>(options => options.UseSqlServer(identityConnection));
builder.Services.AddIdentityCore<ApplicationUser>().AddRoles<IdentityRole>().AddEntityFrameworkStores<IndTraceDbIdentity>();
builder.Services.AddSingleton<IDateTimeMachine>(_ => new DateTimeMachine());
builder.Services.AddScoped<DemoDatabaseSeeder>();

using var host = builder.Build();
await using var scope = host.Services.CreateAsyncScope();
var services = scope.ServiceProvider;
var cancellationToken = CancellationToken.None;

var data = services.GetRequiredService<IndTraceDbContext>();
var identity = services.GetRequiredService<IndTraceDbIdentity>();
if (reset)
{
    await data.Database.EnsureDeletedAsync(cancellationToken);
    await identity.Database.EnsureDeletedAsync(cancellationToken);
}

// The data migrations lag the current model (EF reports pending model changes), so the data schema is created
// from the model itself. The sign-in database is migrated normally.
await data.Database.EnsureCreatedAsync(cancellationToken);
await identity.Database.MigrateAsync(cancellationToken);

var dataset = DemoDataset.Create(services.GetRequiredService<IDateTimeMachine>());
if (dataset.IsFailure || dataset.Value is null)
{
    Console.Error.WriteLine("Demo dataset is invalid: " + string.Join("; ", dataset.Errors));
    return 1;
}

var seeded = await services.GetRequiredService<DemoDatabaseSeeder>().SeedAsync(dataset.Value, cancellationToken);
if (seeded.IsFailure)
{
    Console.Error.WriteLine(string.Join("; ", seeded.Errors));
    return 1;
}

var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
if (!await roles.RoleExistsAsync("Administrator"))
{
    await roles.CreateAsync(new IdentityRole("Administrator"));
}

var users = services.GetRequiredService<UserManager<ApplicationUser>>();
if (await users.FindByNameAsync(demoUser) is null)
{
    var account = new ApplicationUser { UserName = demoUser, Email = demoUser, EmailConfirmed = true };
    var created = await users.CreateAsync(account, demoPassword);
    if (!created.Succeeded)
    {
        Console.Error.WriteLine("Demo user not created: " + string.Join("; ", created.Errors.Select(e => e.Description)));
        return 1;
    }

    await users.AddToRoleAsync(account, "Administrator");
}

Console.WriteLine($"Demo database ready: {dataset.Value.Machines.Count} stations, {dataset.Value.Products.Count} products, demo user {demoUser}.");
return 0;
