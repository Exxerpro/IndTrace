// <copyright file="EntityUpdateHelper.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Models;
using IndTrace.Persistence.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace IndTrace.Persistence.Repositories;

/// <summary>
/// Provides static helper methods for updating entities in the database context.
/// </summary>
/// <typeparam name="T">The entity type to update.</typeparam>
public static class EntityUpdateHelper<T>
    where T : class
{
    /// <summary>
    /// Updates the specified entity in the context, matching by primary key.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="entity">The entity to update.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="Result"/> indicating the outcome of the update operation.</returns>
    public static async Task<Result> UpdateAsync(
        IIndTraceDbContext context,
        T entity,
        CancellationToken cancellationToken)
    {
        // Early validation with clear error messages
        var validationResult = ValidateInputs(context, entity);
        if (validationResult.IsFailure)
            return validationResult;

        // Extract the FULL primary key (all components) with proper error handling. P0-4 (#64): composite keys must
        // use every key property, not just the first, to avoid matching/updating the wrong row.
        var keyResult = EntityKeyResolver.GetPrimaryKeyValues(entity, context);
        if (keyResult.IsFailure || keyResult.Value is null)
            return Result.WithFailure($"Failed to get primary key: {string.Join(", ", keyResult.Errors)}");

        var keyValues = keyResult.Value;

        // Try to update entity using pattern matching for cleaner flow
        var updateResult = await TryUpdateEntity(context, entity, keyValues, cancellationToken);
        if (updateResult.IsFailure)
            return updateResult;

        // Save changes with proper result handling
        return await SaveChangesWithValidation(context, cancellationToken);
    }

    /// <summary>
    /// Updates specific properties of an entity in the context, matching by primary key.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="entity">The entity with updated property values.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <param name="propertiesToUpdate">The names of the properties to update. If empty, all properties are updated.</param>
    /// <returns>A <see cref="Result"/> indicating the outcome of the update operation.</returns>
    public static async Task<Result> UpdateAsync(
        IIndTraceDbContext context,
        T entity,
        CancellationToken cancellationToken,
        params string[] propertiesToUpdate)
    {
        // Early validation
        var validationResult = ValidateInputs(context, entity);
        if (validationResult.IsFailure)
            return validationResult;

        // Extract the FULL primary key (all components). P0-4 (#64): composite keys must use every key property.
        var keyResult = EntityKeyResolver.GetPrimaryKeyValues(entity, context);
        if (keyResult.IsFailure || keyResult.Value is null)
            return Result.WithFailure($"Failed to get primary key: {string.Join(", ", keyResult.Errors)}");

        var keyValues = keyResult.Value;

        // Find entity to update
        var entityToUpdate = await FindEntityToUpdate(context, keyValues, cancellationToken);
        if (entityToUpdate is null)
            return Result.WithFailure($"Entity of type {typeof(T).Name} with key '{FormatKey(keyValues)}' not found.");

        // Apply updates based on whether specific properties are requested
        var updateMode = propertiesToUpdate?.Length > 0
            ? UpdateMode.Partial
            : UpdateMode.Full;

        var applyResult = ApplyUpdates(context, entityToUpdate, entity, updateMode, propertiesToUpdate);
        if (applyResult.IsFailure)
            return applyResult;

        // Save changes
        return await SaveChangesWithValidation(context, cancellationToken);
    }

    #region Private Helper Methods

    /// <summary>
    /// Validates input parameters.
    /// </summary>
    private static Result ValidateInputs(IIndTraceDbContext? context, T? entity)
    {
        return (context, entity) switch
        {
            (null, _) => Result.WithFailure("Database context cannot be null."),
            (_, null) => Result.WithFailure($"Entity of type {typeof(T).Name} cannot be null."),
            _ => Result.Success()
        };
    }

    /// <summary>
    /// Attempts to update an entity, handling both tracked and untracked scenarios.
    /// </summary>
    private static async Task<Result> TryUpdateEntity(
        IIndTraceDbContext context,
        T entity,
        object?[] keyValues,
        CancellationToken cancellationToken)
    {
        // Check for already tracked entity
        var trackedEntry = FindTrackedEntity(context, keyValues);

        if (trackedEntry is not null)
        {
            // Update tracked entity. F3 (#117): SetValues copies EVERY property from the detached source —
            // including CreatedBy/CreatedOn, which a detached instance typically carries as defaults. The audit
            // stamper only advances the Modified* audit on save (the creation audit is immutable), so preserve
            // the stored creation audit across the copy.
            var trackedCreationAudit = CaptureCreationAudit(trackedEntry.Entity);
            trackedEntry.CurrentValues.SetValues(entity);
            RestoreCreationAudit(trackedEntry.Entity, trackedCreationAudit);
            trackedEntry.State = EntityState.Modified;
            return Result.Success();
        }

        // Entity not tracked, find it in database using the full composite key
        var existing = await context.Set<T>().FindAsync(keyValues, cancellationToken);
        if (existing is null)
            return Result.WithFailure($"Entity of type {typeof(T).Name} with key '{FormatKey(keyValues)}' not found in database.");

        // Update untracked entity, preserving the immutable creation audit (F3 #117, see tracked branch).
        var existingCreationAudit = CaptureCreationAudit(existing);
        context.Entry(existing).CurrentValues.SetValues(entity);
        RestoreCreationAudit(existing, existingCreationAudit);
        context.Entry(existing).State = EntityState.Modified;

        return Result.Success();
    }

    /// <summary>
    /// Captures the immutable creation audit (CreatedBy/CreatedOn) of the stored row before a full property copy,
    /// or null when the entity is not auditable. F3 (#117).
    /// </summary>
    private static (string CreatedBy, DateTime? CreatedOn)? CaptureCreationAudit(T target) =>
        target is AuditableEntity auditable ? (auditable.CreatedBy, auditable.CreatedOn) : null;

    /// <summary>
    /// Restores a previously captured creation audit after SetValues copied the detached source's (typically
    /// default) values over it. The Modified* audit is deliberately left alone — the context's save pipeline
    /// advances it. F3 (#117).
    /// </summary>
    private static void RestoreCreationAudit(T target, (string CreatedBy, DateTime? CreatedOn)? audit)
    {
        if (audit is null || target is not AuditableEntity auditable)
            return;

        auditable.CreatedBy = audit.Value.CreatedBy;
        auditable.CreatedOn = audit.Value.CreatedOn;
    }

    /// <summary>
    /// Finds an already tracked entity by its <b>full</b> primary key (all components). P0-4 (#64): comparing only the
    /// first key component could match the wrong tracked row for a composite key.
    /// </summary>
    private static EntityEntry<T>? FindTrackedEntity(IIndTraceDbContext context, object?[] keyValues)
    {
        return context.ChangeTracker
            .Entries<T>()
            .FirstOrDefault(entry =>
            {
                var trackedKeyResult = EntityKeyResolver.GetPrimaryKeyValues(entry.Entity, context);
                return trackedKeyResult.IsSuccess
                    && trackedKeyResult.Value is not null
                    && KeysEqual(trackedKeyResult.Value, keyValues);
            });
    }

    /// <summary>
    /// Finds an entity to update, checking tracked entities first, then the database.
    /// </summary>
    private static async Task<T?> FindEntityToUpdate(
        IIndTraceDbContext context,
        object?[] keyValues,
        CancellationToken cancellationToken)
    {
        // Check tracked entities first (avoids database round trip)
        var trackedEntry = FindTrackedEntity(context, keyValues);
        if (trackedEntry is not null)
            return trackedEntry.Entity;

        // Not tracked, load from database using the full composite key
        return await context.Set<T>().FindAsync(keyValues, cancellationToken);
    }

    /// <summary>
    /// Compares two ordered primary-key value arrays for element-wise equality.
    /// </summary>
    private static bool KeysEqual(object?[] left, object?[] right)
    {
        if (left.Length != right.Length)
            return false;

        for (var i = 0; i < left.Length; i++)
        {
            if (!Equals(left[i], right[i]))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Formats a composite key array for diagnostic messages.
    /// </summary>
    private static string FormatKey(object?[] keyValues) => string.Join(", ", keyValues);

    /// <summary>
    /// Applies updates to an entity based on the update mode.
    /// </summary>
    private static Result ApplyUpdates(
        IIndTraceDbContext context,
        T entityToUpdate,
        T sourceEntity,
        UpdateMode mode,
        string[]? propertiesToUpdate)
    {
        var entry = context.Entry(entityToUpdate);

        if (entry is null)
        {
            return Result.WithFailure("Invalid update mode.");
        }

        return mode switch
        {
            UpdateMode.Full => ApplyFullUpdate(entry, sourceEntity),
            UpdateMode.Partial => ApplyPartialUpdate(entry, sourceEntity, propertiesToUpdate ?? Array.Empty<string>()),
            _ => Result.WithFailure("Invalid update mode.")
        };
    }

    /// <summary>
    /// Applies a full update to all properties.
    /// </summary>
    private static Result ApplyFullUpdate(EntityEntry entry, T sourceEntity)
    {
        try
        {
            entry.CurrentValues.SetValues(sourceEntity);
            entry.State = EntityState.Modified;
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.WithFailure($"Failed to apply full update: {ex.Message}");
        }
    }

    /// <summary>
    /// Applies a partial update to specific properties only.
    /// </summary>
    private static Result ApplyPartialUpdate(EntityEntry entry, T sourceEntity, string[] propertiesToUpdate)
    {
        var entityType = typeof(T);
        var failedProperties = new List<string>();

        foreach (var propertyName in propertiesToUpdate)
        {
            var property = entityType.GetProperty(propertyName);

            if (property is null)
            {
                failedProperties.Add($"Property '{propertyName}' not found on type {entityType.Name}");
                continue;
            }

            try
            {
                var value = property.GetValue(sourceEntity);
                entry.Property(propertyName).CurrentValue = value;
                entry.Property(propertyName).IsModified = true;
            }
            catch (Exception ex)
            {
                failedProperties.Add($"Failed to update property '{propertyName}': {ex.Message}");
            }
        }

        return failedProperties.Count > 0
            ? Result.WithFailure(failedProperties.ToArray())
            : Result.Success();
    }

    /// <summary>
    /// Saves changes with proper validation and error handling.
    /// </summary>
    private static async Task<Result> SaveChangesWithValidation(
        IIndTraceDbContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            // P0-4 (#64): a 0 affected-row count here means EF Core detected no net column change — an idempotent
            // re-send of identical values — which is a SUCCESS, not a failure. A genuine optimistic-concurrency
            // conflict (the target row was modified/removed under a concurrency token) surfaces as
            // DbUpdateConcurrencyException and is handled below; it does NOT reach here as a silent 0. Reporting 0 as
            // a concurrency failure previously made harmless idempotent updates fail. Any affected count (0, 1, or
            // more incl. triggers/cascades) is therefore a success.
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return Result.WithFailure($"Concurrency conflict: {ex.Message}");
        }
        catch (DbUpdateException ex)
        {
            return Result.WithFailure($"Database update failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result.WithFailure($"Unexpected error during save: {ex.Message}");
        }
    }

    #endregion Private Helper Methods

    /// <summary>
    /// Defines the update mode for entity updates.
    /// </summary>
    private enum UpdateMode
    {
        Full,
        Partial
    }
}

//[Fix]
//CLAUDE
//Date: 02/09/2025
//Reason: [READABILITY] - Refactored using modern C# patterns, improved error handling, and clearer method separation