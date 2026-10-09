// Builds an IndTrace demo database from the repository's fictitious test fixtures, plus a demo sign-in user.
// Settings come from docs/site-media/env.sh (DEMO_PASSWORD, DEMO_SQL_PORT, DEMO_DB, DEMO_IDENTITY_DB, DEMO_USER).
//   dotnet run --project docs/site-media/demo-db                    recreate both databases from scratch
//   dotnet run --project docs/site-media/demo-db -- --only Variables reload one fixture, keep the rest
using System.Reflection;
using IndTrace.Identity.Data;
using IndTrace.Persistence.DBContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

static string Env(string name, string? fallback = null) =>
    Environment.GetEnvironmentVariable(name) is { Length: > 0 } value
        ? value
        : fallback ?? throw new InvalidOperationException($"{name} is not set; source docs/site-media/env.sh first.");

var onlyAt = Array.IndexOf(args, "--only");
var only = onlyAt >= 0 && onlyAt + 1 < args.Length ? args[onlyAt + 1] : null;
var password = Env("DEMO_PASSWORD");
var server = $"localhost,{Env("DEMO_SQL_PORT", "14333")}";
var auth = $"User Id=sa;Password={password};Encrypt=False;TrustServerCertificate=True";
var dataCs = $"Server={server};Database={Env("DEMO_DB", "IndTraceData")};{auth}";
var identityCs = $"Server={server};Database={Env("DEMO_IDENTITY_DB", "IndTraceIdentity")};{auth}";
var demoUser = Env("DEMO_USER", "demo@example.com");

var opts = new DbContextOptionsBuilder<IndTraceDbContext>().UseSqlServer(dataCs).Options;
if (only is null)
await using (var ctx = new IndTraceDbContext(opts))
{
    await ctx.Database.EnsureDeletedAsync();
    await ctx.Database.EnsureCreatedAsync();
    // Demo workaround: the model declares Product.CustomerId unique (one product per customer).
    await ctx.Database.ExecuteSqlRawAsync("DROP INDEX [IDX.IndTraceData.Customer.CustomerId] ON [dbo].[Products]");
    Console.WriteLine("schema created");
}

var testData = typeof(IndTrace.TestData.TestDataLoader).Assembly;
string[] order = ["ConfigApp", "Customer", "Line", "Machine", "Product", "Shift", "Plc", "MachinePlc", "Rule",
    "WorkFlow", "Settings", "Recipe", "VariablesGroup", "Variables", "MasterLabel", "BarCode", "Cycles",
    "Registers", "OeeRegister", "KpiOee", "PerformanceData"];

await using (var ctx = new IndTraceDbContext(opts))
{
    ctx.ChangeTracker.AutoDetectChangesEnabled = false;
    await ctx.Database.OpenConnectionAsync();
    await ctx.Database.ExecuteSqlRawAsync("EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'");
    foreach (var name in only is { } single ? [single] : order)
    {
        var type = testData.GetType($"IndTrace.TestData.RawData.{name}RawData")
                   ?? throw new InvalidOperationException($"no fixture {name}");
        const BindingFlags flags = BindingFlags.Public | BindingFlags.Static;
        var raw = type.GetProperty("Fixture", flags)?.GetValue(null) ?? type.GetField("Fixture", flags)?.GetValue(null);
        if (raw is not System.Collections.IEnumerable list) { Console.WriteLine($"{name}: no Fixture member"); continue; }
        var items = list.Cast<object>().ToList();
        if (items.Count == 0) { Console.WriteLine($"{name}: 0"); continue; }
        var et = ctx.Entry(items[0]).Metadata;
        var table = $"[{et.GetSchema() ?? "dbo"}].[{et.GetTableName()}]";
        var hasIdentity = await ctx.Database.SqlQueryRaw<int>(
            $"SELECT COUNT(*) AS Value FROM sys.identity_columns WHERE object_id = OBJECT_ID('{table}')").SingleAsync() > 0;
        if (only is not null) await ctx.Database.ExecuteSqlRawAsync($"DELETE FROM {table}");
        try
        {
            if (hasIdentity) await ctx.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT {table} ON");
            int ok = 0, skipped = 0; string? firstError = null;
            foreach (var item in items)
            {
                ctx.Entry(item).State = EntityState.Added;
                try { await ctx.SaveChangesAsync(); ok++; }
                catch (DbUpdateException ex) { skipped++; firstError ??= ex.GetBaseException().Message; }
                ctx.ChangeTracker.Clear();
            }
            Console.WriteLine($"{name}: {ok} rows -> {table}" + (skipped > 0 ? $", {skipped} skipped ({firstError?.Split('.')[0]})" : ""));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{name}: FAILED {ex.GetBaseException().Message}");
        }
        finally
        {
            if (hasIdentity) await ctx.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT {table} OFF");
            ctx.ChangeTracker.Clear();
        }
    }
    await ctx.Database.ExecuteSqlRawAsync("EXEC sp_MSforeachtable 'ALTER TABLE ? CHECK CONSTRAINT ALL'");
}

if (only is not null) return;
var services = new ServiceCollection().AddLogging(b => b.AddConsole().SetMinimumLevel(LogLevel.Warning));
services.AddDbContext<IndTraceDbIdentity>(o => o.UseSqlServer(identityCs));
services.AddIdentityCore<ApplicationUser>().AddRoles<IdentityRole>().AddEntityFrameworkStores<IndTraceDbIdentity>();
await using var sp = services.BuildServiceProvider();
await using var scope = sp.CreateAsyncScope();
var idb = scope.ServiceProvider.GetRequiredService<IndTraceDbIdentity>();
await idb.Database.EnsureDeletedAsync();
await idb.Database.EnsureCreatedAsync();
var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
await roles.CreateAsync(new IdentityRole("Administrator"));
var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
var user = new ApplicationUser { UserName = demoUser, Email = demoUser, EmailConfirmed = true };
var created = await users.CreateAsync(user, password);
Console.WriteLine(created.Succeeded ? "demo user created" : string.Join("; ", created.Errors.Select(e => e.Description)));
await users.AddToRoleAsync(user, "Administrator");
