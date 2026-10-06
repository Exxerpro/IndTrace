// <copyright file="IndTraceDbContext.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Castle.Core.Logging;
using IndTrace.Application.UserService;
using IndTrace.Domain.Entities;
using IndTrace.Domain.Entities.BarCodes;
using IndTrace.Domain.Enum;
using IndTrace.Domain.Enum.LookUpTable;
using IndTrace.Domain.Interfaces;
using IndTrace.Domain.Models;

using IndTrace.Persistence.Converters;
using IndTrace.Persistence.Extensions;
using IndTrace.Persistence.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using ILogger = Castle.Core.Logging.ILogger;
using Rule = IndTrace.Domain.Entities.Rule;

namespace IndTrace.Persistence.DBContext;

/// <summary>
/// Represents the primary Entity Framework database context for the IndTrace manufacturing traceability system.
/// Manages database entities for production cycles, machine data, PLCs, barcode scanning, and OEE calculations.
/// Implements auditable entity tracking and provides specialized methods for SQL Server identity operations.
/// </summary>
public class IndTraceDbContext : DbContext, IIndTraceDbContext, IAsyncDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IndTraceDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by the DbContext.</param>
    public IndTraceDbContext(DbContextOptions<IndTraceDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Maps to SQL Server's SOUNDEX function for phonetic string comparison in manufacturing part identification.
    /// Used for finding similar part numbers when barcode reads are partially corrupted or unclear.
    /// </summary>
    /// <param name="input">The input string to generate a SOUNDEX code for.</param>
    /// <returns>A SOUNDEX phonetic representation of the input string.</returns>
    [DbFunction(Name = "SoundEx", IsBuiltIn = true)]
    public static string SoundLike(string input)
    {
        throw new NotImplementedException();
    }

    // NOTE: there is intentionally NO DbSet<ResultValidation>. ResultValidation is a data-complete
    // smart enum (EnumModel) and is Ignore<>()d from the model below; its persisted twin is
    // ResultValidationEntity (configured via ResultValidationConfiguration). The former dead
    // DbSet<ResultValidation> property contradicted that Ignore<>() and would have thrown at runtime
    // if ever accessed — removed. The SmartEnumPersistenceGuard architecture test enforces this.

    /// <summary>
    /// Gets or sets the DbSet for CycleStatus entities.
    /// </summary>
    public DbSet<CycleStatusEntity> CycleStatus => Set<CycleStatusEntity>();

    /// <summary>
    /// Gets or sets the DbSet for FlowStatus entities that define production flow states.
    /// </summary>
    public DbSet<FlowStatusEntity> FlowStatus => Set<FlowStatusEntity>();

    /// <summary>
    /// Gets or sets the DbSet for PartStatus entities that define quality status of manufactured parts.
    /// </summary>
    public DbSet<PartStatusEntity> PartStatus => Set<PartStatusEntity>();

    /// <summary>
    /// Gets or sets the DbSet for MachineType entities that categorize manufacturing equipment.
    /// </summary>
    [Required]
    public DbSet<MachineTypeEntity> MachineTypes => Set<MachineTypeEntity>();

    /// <summary>
    /// Gets or sets the DbSet for TagsGroup entities used for organizing PLC variable tags.
    /// </summary>
    [Required]
    public DbSet<TagsGroupEntity> TagsGroups => Set<TagsGroupEntity>();

    /// <summary>
    /// Gets or sets the DbSet for DistinctRegister entities containing unique register data.
    /// </summary>
    [Required]
    public DbSet<DistinctRegister> DistinctRegisters => Set<DistinctRegister>();

    /// <summary>
    /// Gets or sets the DbSet for WorkFlowType entities that define manufacturing workflow categories.
    /// </summary>
    [Required]
    public DbSet<WorkFlowTypeEntity> WorkFlowTypes => Set<WorkFlowTypeEntity>();

    /// <summary>
    /// Gets or sets the DbSet for GatewayTask entities that define available PLC communication tasks.
    /// </summary>
    [Required]
    public DbSet<GatewayTaskEntity> GatewayTask => Set<GatewayTaskEntity>();

    /// <summary>
    /// Gets or sets the DbSet for TaskGatewayRequest entities used for tracking PLC communication commands.
    /// </summary>
    [Required]
    public DbSet<TaskGatewayRequest> Requests => Set<TaskGatewayRequest>();

    /// <summary>
    /// Gets or sets the DbSet for TaskGatewayResponse entities used for tracking PLC communication responses.
    /// </summary>
    [Required]
    public DbSet<TaskGatewayResponse> Responses => Set<TaskGatewayResponse>();

    /// <summary>
    /// Gets or sets the DbSet for ConfigApp entities containing application configuration settings.
    /// </summary>
    [Required]
    public DbSet<ConfigApp> ConfigApps => Set<ConfigApp>();

    /// <summary>
    /// Gets or sets the DbSet for ShiftsCatalog entities that define work shift schedules.
    /// </summary>
    [Required]
    public DbSet<ShiftsCatalog> ShiftsCatalog => Set<ShiftsCatalog>();

    /// <summary>
    /// Gets or sets the DbSet for ConfigDatabaseLog entities for database logging configuration.
    /// </summary>
    [Required]
    public DbSet<ConfigDatabaseLog> ConfigDatabaseLog => Set<ConfigDatabaseLog>();

    /// <summary>
    /// Gets or sets the DbSet for ConfigDb entities containing database configuration settings.
    /// </summary>
    [Required]
    public DbSet<ConfigDb> ConfigDb => Set<ConfigDb>();

    /// <summary>
    /// Gets or sets the DbSet for KpiOee entities containing Overall Equipment Effectiveness metrics.
    /// </summary>
    [Required]
    public DbSet<KpiOee> KpiOee => Set<KpiOee>();

    /// <summary>
    /// Gets or sets the DbSet for BarCode entities representing scanned manufacturing labels.
    /// </summary>
    [Required]
    public DbSet<BarCode> BarCodes => Set<BarCode>();

    /// <summary>
    /// Gets or sets the DbSet for Cycle entities tracking individual production cycles.
    /// </summary>
    [Required]
    public DbSet<Cycle> Cycles => Set<Cycle>();

    /// <summary>
    /// Gets or sets the DbSet for <see cref="CycleCompletion"/> entities — the #40 one-per-cycle idempotency
    /// marker (<c>UNIQUE(CycleId)</c>) staged inside the BarCode aggregate's atomic batch.
    /// </summary>
    [Required]
    public DbSet<CycleCompletion> CycleCompletions => Set<CycleCompletion>();

    /// <summary>
    /// Gets or sets the DbSet for Defect entities recording quality defects found during production.
    /// </summary>
    [Required]
    public DbSet<Defect> Defects => Set<Defect>();

    /// <summary>
    /// Gets or sets the DbSet for MasterLabel entities containing barcode label master data.
    /// </summary>
    [Required]
    public DbSet<MasterLabel> MasterLabel => Set<MasterLabel>();

    /// <summary>
    /// Gets or sets the DbSet for WorkFlow entities defining manufacturing process workflows.
    /// </summary>
    [Required]
    public DbSet<WorkFlow> WorkFlows => Set<WorkFlow>();

    /// <summary>
    /// Gets or sets the DbSet for <see cref="RoutingNodeRow"/> entities — first-class routing nodes
    /// keyed by (product, machine) carrying the node's composite WorkFlowType bitmask (C2 redesign).
    /// </summary>
    [Required]
    public DbSet<RoutingNodeRow> RoutingNodes => Set<RoutingNodeRow>();

    /// <summary>
    /// Gets or sets the DbSet for Machine entities representing manufacturing equipment.
    /// </summary>
    [Required]
    public DbSet<Machine> Machines => Set<Machine>();

    /// <summary>
    /// Gets or sets the DbSet for Plc entities representing programmable logic controllers.
    /// </summary>
    [Required]
    public DbSet<Plc> Plcs => Set<Plc>();

    /// <summary>
    /// Gets or sets the DbSet for MachinePlc entities linking machines to their PLCs.
    /// </summary>
    [Required]
    public DbSet<MachinePlc> MachinePlcs => Set<MachinePlc>();

    /// <summary>
    /// Gets or sets the DbSet for Tooling entities representing manufacturing tools and fixtures.
    /// </summary>
    [Required]
    public DbSet<Tooling> Toolings => Set<Tooling>();

    /// <summary>
    /// Gets or sets the DbSet for Stoppage entities recording production line stoppages.
    /// </summary>
    [Required]
    public DbSet<Stoppage> Stoppages => Set<Stoppage>();

    /// <summary>
    /// Gets or sets the DbSet for PerformanceSpec entities defining performance specifications.
    /// </summary>
    [Required]
    public DbSet<PerformanceSpec> PerformanceSpecs => Set<PerformanceSpec>();

    /// <summary>
    /// Gets or sets the DbSet for ProductSpec entities defining product specifications.
    /// </summary>
    [Required]
    public DbSet<ProductSpec> ProductSpecs => Set<ProductSpec>();

    /// <summary>
    /// Gets or sets the DbSet for Product entities.
    /// </summary>
    [Required]
    public DbSet<Product> Products => Set<Product>();

    /// <summary>
    /// Gets or sets the DbSet for Customer entities.
    /// </summary>
    [Required]
    public DbSet<Customer> Customers => Set<Customer>();

    /// <summary>
    /// Gets or sets the DbSet for Shift entities representing work shift schedules and assignments.
    /// </summary>
    [Required]
    public DbSet<Shift> Shifts => Set<Shift>();

    /// <summary>
    /// Gets or sets the DbSet for Recipe entities containing manufacturing process recipes.
    /// </summary>
    [Required]
    public DbSet<Recipe> Recipes => Set<Recipe>();

    /// <summary>
    /// Gets or sets the DbSet for DefectRegister entities tracking recorded quality defects.
    /// </summary>
    [Required]
    public DbSet<DefectRegister> DefectRegisters => Set<DefectRegister>();

    /// <summary>
    /// Gets or sets the DbSet for StoppageRegister entities tracking recorded production stoppages.
    /// </summary>
    [Required]
    public DbSet<StoppageRegister> StoppageRegisters => Set<StoppageRegister>();

    /// <summary>
    /// Gets or sets the DbSet for Setting entities containing system configuration settings.
    /// </summary>
    [Required]
    public DbSet<Setting> Settings => Set<Setting>();

    /// <summary>
    /// Gets or sets the DbSet for ConnectionStatus entities tracking PLC connection states.
    /// </summary>
    [Required]
    public DbSet<ConnectionStatus> ConnectionStatus => Set<ConnectionStatus>();

    /// <summary>
    /// Gets or sets the DbSet for StatusConfiguration entities for system status configuration.
    /// </summary>
    [Required]
    public DbSet<StatusConfiguration> StatusConfigurations => Set<StatusConfiguration>();

    /// <summary>
    /// Gets or sets the DbSet for MachineStatus entities tracking current machine operational states.
    /// </summary>
    [Required]
    public DbSet<MachineStatus> MachineStatus => Set<MachineStatus>();

    /// <summary>
    /// Gets or sets the DbSet for VariablesGroup entities for organizing PLC variables into logical groups.
    /// </summary>
    [Required]
    public DbSet<VariablesGroup> VariablesGroups => Set<VariablesGroup>();

    /// <summary>
    /// Gets or sets the DbSet for IndTraceUser entities representing system users.
    /// </summary>
    [Required]
    public DbSet<IndTraceUser> Users => Set<IndTraceUser>();

    /// <summary>
    /// Gets or sets the DbSet for Register entities containing manufacturing data register records.
    /// </summary>
    [Required]
    public DbSet<Register> Registers => Set<Register>();

    /// <summary>
    /// Gets or sets the DbSet for Variable entities.
    /// </summary>
    [Required]
    public DbSet<Variable> Variables => Set<Variable>();

    /// <summary>
    /// Gets or sets the DbSet for PerformanceData entities containing machine performance metrics.
    /// </summary>
    [Required]
    public DbSet<Domain.Entities.PerformanceData> PerformanceDatas => Set<Domain.Entities.PerformanceData>();

    /// <summary>
    /// Gets or sets the DbSet for OeeRegister entities tracking Overall Equipment Effectiveness data.
    /// </summary>
    [Required]
    public DbSet<Domain.Entities.OeeRegister> OeeRegisters => Set<Domain.Entities.OeeRegister>();

    /// <summary>
    /// Gets or sets the DbSet for Rule entities defining business rules for manufacturing operations.
    /// </summary>
    [Required]
    public DbSet<Rule> Rules => Set<Rule>();

    /// <summary>
    /// Gets or sets the DbSet for the additive Story 3.5 <see cref="FlowTransitionLog"/> entities — the
    /// append-only diagnostic record of every attempted item lifecycle transition (FR5 / CR2).
    /// </summary>
    [Required]
    public DbSet<FlowTransitionLog> FlowTransitionLogs => Set<FlowTransitionLog>();

    /// <summary>
    /// Gets or sets the model for the context, or null if the context is disposed.
    /// </summary>
    public new IModel? Model
    {
        get
        {
            try
            {
                return base.Model;
            }
            catch (ObjectDisposedException)
            {
                // This exception occurs when the context is disposed and the model is accessed.
                // in this case, we can return null or handle it as needed.
                // For example, you might want to log the error or throw a custom exception.
                // I think we don't have a logger yet, so let's just return null.
                return null;
            }
        }
    }

    /// <summary>
    /// Gets or sets the DbSet for Line entities representing production lines in the manufacturing facility.
    /// </summary>
    [Required]
    public DbSet<Line> Lines => Set<Line>();

    /// <summary>
    /// Enables the identity insert for a specific table and saves changes asynchronously.
    /// </summary>
    /// <remarks>
    /// F6a (#117): a transient fault inside the retriable unit PROPAGATES to the configured execution strategy
    /// (up to the <c>EnableRetryOnFailure</c> retry budget); the exception→<see cref="Result{T}"/> conversion
    /// happens once, around <c>strategy.ExecuteAsync</c>, so callers still always receive a
    /// <see cref="Result{T}"/> and no exception escapes this method.
    /// </remarks>
    /// <param name="tableName">The table name.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A result containing the number of affected rows or an error message.</returns>
    public async Task<Result<int>> SaveChangesAsync(string tableName, CancellationToken cancellationToken = default)
    {
        if (!this.Database.IsRelational())
            return Result<int>.WithFailure(["The operation is only supported for relational databases."]);

        // P0-4 (#64): the table name is interpolated into a SET IDENTITY_INSERT statement, which cannot be a bound
        // SQL parameter. Validate it against the model's known tables and emit a safely bracket-quoted delimited
        // identifier so an attacker-influenced/arbitrary string can never be concatenated into raw SQL.
        var safeIdentifierResult = this.ResolveSafeTableIdentifier(tableName);
        if (safeIdentifierResult.IsFailure || safeIdentifierResult.Value is null)
            return Result<int>.WithFailure(safeIdentifierResult.Errors);

        var safeIdentifier = safeIdentifierResult.Value;

        // Create an execution strategy
        var strategy = this.Database.CreateExecutionStrategy();

        try
        {
            // Execute all operations in a single retriable unit of work. Each attempt is retry-safe: the
            // transaction, the IDENTITY_INSERT ON/OFF pair and the flush are all begun INSIDE the lambda, so a
            // retried attempt starts from a fresh transaction (F6a, #117).
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await this.Database.BeginTransactionAsync(cancellationToken);

                // SET IDENTITY_INSERT is a session-scoped setting, NOT transactional: rolling back the transaction does
                // NOT turn it OFF. Track whether we turned it ON so the finally block can always turn it back OFF,
                // otherwise the pooled connection would leak IDENTITY_INSERT = ON into unrelated later operations.
                // Build the raw SQL once from the validated+quoted identifier. Assigning to a plain string (rather than
                // interpolating at the ExecuteSqlRaw call site) keeps the identifier safe: it has already been checked
                // against the model and bracket-quoted by ResolveSafeTableIdentifier.
                var identityInsertOnSql = "SET IDENTITY_INSERT " + safeIdentifier + " ON";
                var identityInsertOffSql = "SET IDENTITY_INSERT " + safeIdentifier + " OFF";

                var identityInsertOn = false;
                try
                {
                    await this.Database.ExecuteSqlRawAsync(identityInsertOnSql, cancellationToken);
                    identityInsertOn = true;

                    // #126 (C3): the flush must NOT accept the tracker changes yet — the IDENTITY_INSERT OFF
                    // statement and the commit below can still throw a transient fault AFTER a successful
                    // flush. With the default (accept-on-success) overload, that fault rolled the INSERT back
                    // while the tracker stayed ACCEPTED, so the strategy's retried attempt found an EMPTY
                    // tracker, flushed 0 rows, committed, and reported Success(0) — silent data loss. Deferring
                    // acceptance keeps the pending changes staged across a rolled-back attempt (EF holds the
                    // store-generated values in the tracker sidecar, not on the entities), so a retry
                    // re-flushes them; acceptance happens exactly once, after the commit is durable.
                    var result = await this.SaveChangesAsync(acceptAllChangesOnSuccess: false, cancellationToken);

                    await this.Database.ExecuteSqlRawAsync(identityInsertOffSql, cancellationToken);
                    identityInsertOn = false;

                    await transaction.CommitAsync(cancellationToken);

                    this.ChangeTracker.AcceptAllChanges();

                    return Result<int>.Success(result);
                }
                catch
                {
                    // F6a (#117): roll back, then RETHROW. The retrying execution strategy only retries when the
                    // retriable lambda THROWS a transient exception — converting the fault to a Result here made
                    // EnableRetryOnFailure dead code (the strategy never saw a failure). The exception→Result
                    // conversion happens exactly once, in the catch AROUND strategy.ExecuteAsync below, so no
                    // exception ever escapes this method. Rollback is best-effort: the connection may already be
                    // broken, and a secondary rollback fault must never mask the primary one being rethrown.
                    try
                    {
                        await transaction.RollbackAsync(cancellationToken);
                    }
                    catch (Exception rollbackEx)
                    {
                        _ = rollbackEx;
                    }

                    throw;
                }
                finally
                {
                    if (identityInsertOn)
                    {
                        try
                        {
                            await this.Database.ExecuteSqlRawAsync(identityInsertOffSql, cancellationToken);
                        }
                        catch (Exception offEx)
                        {
                            // Best-effort cleanup: the connection may already be broken. Swallow so the primary
                            // failure/result is never masked by a secondary cleanup error. (This context has no
                            // injected logger; the primary error is already surfaced via the Result.)
                            _ = offEx;
                        }
                    }
                }
            });
        }
        catch (Microsoft.EntityFrameworkCore.Storage.RetryLimitExceededException ex)
        {
            // The transient fault survived every retry attempt. Surface the ORIGINAL fault's message (the
            // strategy wraps it) so callers see the same failure content they did before retries existed.
            return Result<int>.WithFailure([ex.InnerException?.Message ?? ex.Message]);
        }
        catch (Exception ex)
        {
            // Result railway boundary: a non-transient fault (or any other escape) is converted here — the ONE
            // conversion point — so no exception ever crosses this method's boundary.
            return Result<int>.WithFailure([ex.Message]);
        }
    }

    /// <summary>
    /// Validates a caller-supplied table name against the built EF Core model and returns a safely bracket-quoted,
    /// schema-qualified delimited identifier (e.g. <c>[dbo].[Products]</c>). P0-4 (#64): closes the SQL-injection seam
    /// on <see cref="SaveChangesAsync(string, CancellationToken)"/> — only a name that maps to a real modelled table
    /// is accepted, and the returned identifier is quoted with <c>]</c> escaped.
    /// </summary>
    /// <param name="tableName">The unqualified table name supplied by the caller.</param>
    /// <returns>A result with the safe delimited identifier, or a failure if the name is unknown/ambiguous.</returns>
    private Result<string?> ResolveSafeTableIdentifier(string? tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
            return Result<string?>.WithFailure(["Table name cannot be null or empty."]);

        string? matchedSchema = null;
        string? matchedTable = null;

        var model = this.Model;
        if (model is null)
            return Result<string?>.WithFailure(["The EF Core model is not available."]);

        foreach (var entityType in model.GetEntityTypes())
        {
            var candidate = entityType.GetTableName();
            if (!string.IsNullOrEmpty(candidate) && string.Equals(candidate, tableName, StringComparison.Ordinal))
            {
                matchedSchema = entityType.GetSchema();
                matchedTable = candidate;
                break;
            }
        }

        if (string.IsNullOrEmpty(matchedTable))
            return Result<string?>.WithFailure([$"Table name '{tableName}' is not a known entity table in the model."]);

        static string Quote(string identifier) => "[" + identifier.Replace("]", "]]", StringComparison.Ordinal) + "]";

        var safe = string.IsNullOrEmpty(matchedSchema) ? Quote(matchedTable) : $"{Quote(matchedSchema)}.{Quote(matchedTable)}";
        return Result<string?>.Success(safe);
    }

    /// <summary>
    /// Gets a value indicating whether the database supports transactions.
    /// </summary>
    public bool SupportsTransactions => this.Database.IsRelational();

    IModel IIndTraceDbContext.Model => base.Model;

    /// <summary>
    /// Sets testing interfaces for dependency injection during unit and integration testing.
    /// </summary>
    /// <param name="indTraceUserService">The user service instance for testing, or null to use default.</param>
    /// <param name="dateTimeMachine">The date time machine instance for testing, or null to use default.</param>
    /// <remarks>
    /// This method is intended for testing scenarios where mock implementations of services are required.
    /// If null values are provided, default implementations will be used.
    /// </remarks>
    public void SetTestingInterfaces(IIndTraceUserService indTraceUserService, IDateTimeMachine dateTimeMachine)
    {
        this.indTraceUserService = indTraceUserService ?? new IndTraceUserService();
        this.dateTimeMachine = dateTimeMachine ?? new DateTimeMachine();
    }

    private IIndTraceUserService indTraceUserService = new IndTraceUserService();

    private IDateTimeMachine dateTimeMachine = new DateTimeMachine();
    /// <summary>
    /// Executes SaveChangesAsync operation (accepting all tracker changes on success).
    /// </summary>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <returns>The result of SaveChangesAsync.</returns>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
    {
        return this.SaveChangesAsync(acceptAllChangesOnSuccess: true, cancellationToken);
    }

    /// <summary>
    /// Executes SaveChangesAsync with explicit control over change-tracker acceptance. The audit stamper runs
    /// here so BOTH overloads stamp identically. #126 (C3/C4): flushes inside an explicit transaction of a
    /// retriable execution-strategy unit pass <paramref name="acceptAllChangesOnSuccess"/> =
    /// <see langword="false"/> and accept only after the commit succeeds, so a flushed-then-rolled-back
    /// attempt never leaves an accepted tracker (or store-generated values on the entity instances) behind
    /// for the retry to trip over.
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess">Whether the tracker accepts all changes when the flush succeeds.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <returns>The result of SaveChangesAsync.</returns>
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
    {
        var auditableEntries = this.ChangeTracker.Entries<AuditableEntity>().ToList();

        // F6b (#117): resolve the audited username ONCE per save instead of awaiting the user service for
        // every auditable entry — the same save always stamps the same user.
        var currentUserName = auditableEntries.Count > 0
            ? await this.indTraceUserService.CurrentUserName
            : string.Empty;

        foreach (var entry in auditableEntries)
        {
            // Persistence is the single source of audit timestamps (the entity no longer self-stamps).
            // P0-4 (#64): use the injected clock's value as-is. Applying ToLocalTime() re-derived the stamp from the
            // host's local timezone, defeating the deterministic IDateTimeMachine abstraction (non-reproducible across
            // machines/timezones and untestable). The clock is the single source of truth for the audit instant.
            var stampedOn = this.dateTimeMachine.Now;

            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = currentUserName;
                    entry.Entity.CreatedOn = stampedOn;
                    entry.Entity.ModifiedOn = stampedOn; // created == modified at birth
                    break;

                case EntityState.Modified:
                    // Advance the modification audit only — the creation audit is immutable.
                    // #52: unify on CurrentUserName so ModifiedBy matches CreatedBy's identifier kind
                    // (a human-readable audit trail is more useful in a life-critical traceability system
                    // than an opaque id, and a row's audit columns must not mix name-vs-id).
                    entry.Entity.ModifiedBy = currentUserName;
                    entry.Entity.ModifiedOn = stampedOn;
                    break;

                case EntityState.Detached:
                    break;

                case EntityState.Unchanged:
                    break;

                case EntityState.Deleted:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //[Fix]
        //CLAUDE
        //Date: 25/08/2025
        //Reason: EF Core validation fix - Exclude non-persisted entities from model
        modelBuilder.Ignore<RuleFragment>();

        //[Fix]
        //CLAUDE
        //Date: 25/08/2025
        //Reason: EF Core validation fix - Exclude EnumModel types as only Entity versions are persisted
        modelBuilder.Ignore<ActiveStatus>();
        modelBuilder.Ignore<CycleStatus>();
        modelBuilder.Ignore<FlowStatus>();
        modelBuilder.Ignore<GatewayTask>();
        modelBuilder.Ignore<MachineType>();
        modelBuilder.Ignore<PartStatus>();
        modelBuilder.Ignore<ResultValidation>();
        modelBuilder.Ignore<ShiftType>();
        modelBuilder.Ignore<WorkFlowType>();

        // Story 27.2b-2 (#27/F4): BarCodeLabel is a value object mapped onto BarCode.Label via an EF value converter
        // (see BarCodeConfiguration), NOT a persisted entity. Like the smart-enum reference types above, it must be
        // explicitly ignored so the entity-discovery convention does not register it as a DbSet<T> — otherwise
        // EnforceModelConfiguration flags "BarCodeLabel must implement IEntityRoot or ILookupEntity".
        modelBuilder.Ignore<IndTrace.Domain.ValueObjects.BarCodeLabel>();

        // Story 26.B1 (#31 remainder): Ratio and PerformanceRatio are the bounded value objects mapped onto the
        // four KpiOee metric properties via EF value converters (see KpiOeeConfiguration), NOT persisted entities.
        // As with BarCodeLabel above, the entity-discovery convention would otherwise register them as DbSet<T>
        // and EnforceModelConfiguration would flag "Ratio/PerformanceRatio must implement IEntityRoot or
        // ILookupEntity". Ignore them explicitly so they stay scalar-converted columns only.
        modelBuilder.Ignore<IndTrace.Domain.ValueObjects.Ratio>();
        modelBuilder.Ignore<IndTrace.Domain.ValueObjects.PerformanceRatio>();

        //[Fix]
        //CLAUDE
        //Date: 02/09/2025
        //Reason: [Conflict Resolution] - ShiftsCatalog DbSet exists but was ignored, causing NullReferenceException in EntityKeyResolver
        // Removed modelBuilder.Ignore<ShiftsCatalog>() to allow EF Core model recognition

        //[Fix]
        //CLAUDE
        //Date: 25/08/2025
        //Reason: EF Core validation fix - Exclude Result<T> pattern classes from being treated as entities
        modelBuilder.Ignore<Result>();
        modelBuilder.Ignore<Result<object>>();

        //[Fix]
        //CLAUDE
        //Date: 18/06/2025
        //Reason: EF Core validation fix - System.Exception leaks into the model via the Result<T>
        //  pattern (Result owns an Exception). Ignoring Result/Result<object> does not strip the
        //  nested Exception entity, so EnforceModelConfiguration flagged Exception.HelpLink/Source
        //  (Missing MaxLength) + invalid DbSet registration on every DB-touching test. Exception is
        //  never a persisted entity; exclude it explicitly, matching the Result<T> ignore pattern.
        modelBuilder.Ignore<Exception>();

        modelBuilder.UseValueConverterForType<Label>(new ValueConverterLabel());

        // Ensure critical configurations are applied explicitly
        modelBuilder.ApplyConfiguration(new IndTrace.Persistence.Configurations.ShiftsCatalogConfiguration());

        // Apply the rest from this assembly, excluding ShiftsCatalogConfiguration (already applied
        // explicitly above) so the ShiftsCatalog entity is mapped exactly once.
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(IndTraceDbContext).Assembly,
            type => type != typeof(IndTrace.Persistence.Configurations.ShiftsCatalogConfiguration));

        // Enforce explicit EF Core model configuration - Tasks 1 & 2
        modelBuilder.EnforceModelConfiguration();
    }

    /// <summary>
    /// Creates a DbSet for lookup table entities that extend EnumModel.
    /// </summary>
    /// <typeparam name="TLookupEntity">Lookup entity type that extends EnumModel.</typeparam>
    /// <returns>A set for the given lookup entity type.</returns>
    public DbSet<TLookupEntity> LookupSet<TLookupEntity>() where TLookupEntity : class
    {
        return Set<TLookupEntity>();
    }

    /// <summary>
    /// Creates a DbSet for app-wide shared objects (configs, app status, health, etc.).
    /// </summary>
    /// <typeparam name="TState">State entity type that implements IAppState.</typeparam>
    /// <returns>A set for the given state entity type.</returns>
    public DbSet<TState> SetState<TState>() where TState : class, IAppState, new()
    {
        return Set<TState>();
    }

    /// <summary>
    /// Creates a DbSet for aggregate-specific entities that implement aggregate root pattern.
    /// </summary>
    /// <typeparam name="TAggregate">Aggregate entity type that implements IAggregateRoot.</typeparam>
    /// <returns>A set for the given aggregate entity type.</returns>
    public DbSet<TAggregate> SetAggregate<TAggregate>() where TAggregate : class, IAggregateRoot
    {
        return Set<TAggregate>();
    }

}