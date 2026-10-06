// <copyright file="EntityKeyResolver.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Persistence.Interfaces;

namespace IndTrace.Persistence.Repositories;

/// <summary>
/// Provides helper methods for resolving primary key values from entities using the EF Core model.
/// </summary>
public static class EntityKeyResolver
{
    /// <summary>
    /// Gets the value of the primary key property for the specified entity.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="entity">The entity instance.</param>
    /// <param name="context">The database context containing the EF Core model.</param>
    /// <returns>The value of the primary key property, or null if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="entity"/> or <paramref name="context"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the entity type or primary key is not defined in the model.</exception>
    public static IndQuestResults.Result<object?> GetPrimaryKeyValue<T>(T entity, IIndTraceDbContext context)
    {

        if (entity is null)
        {
            return IndQuestResults.Result<object?>.WithFailure("entity cannot be null");
        }

        if (context is null)
        {
            return IndQuestResults.Result<object?>.WithFailure("context cannot be null");
        }

        var entityType = context.Model.FindEntityType(typeof(T));
        if (entityType is null)
        {
            return IndQuestResults.Result<object?>.WithFailure(
                $"Entity type '{typeof(T).Name}' not found in EF Core model.");
        }

        var valuesResult = GetPrimaryKeyValues(entity, context);
        if (valuesResult.IsFailure)
        {
            return IndQuestResults.Result<object?>.WithFailure(valuesResult.Errors);
        }

        // Back-compat single-value accessor: the first key component. Prefer GetPrimaryKeyValues for correctness on
        // composite keys.
        return IndQuestResults.Result<object?>.Success(valuesResult.Value is { Length: > 0 } vals ? vals[0] : null);
    }

    /// <summary>
    /// Gets the values of <b>all</b> primary key properties for the specified entity, in EF Core model order.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="entity">The entity instance.</param>
    /// <param name="context">The database context containing the EF Core model.</param>
    /// <returns>
    /// A result containing an ordered array of key values (one element per key property). For a single-column key the
    /// array has one element; for a composite key it has one element per component. Used to build the argument array
    /// for <c>DbSet.FindAsync</c> and to compare tracked entities by their full key.
    /// </returns>
    /// <remarks>
    /// P0-4 (#64) fix: the previous single-value resolver used only the <b>first</b> key property, so on a composite
    /// key (e.g. <c>MachinePlc(MachineId, PlcId)</c>, <c>DistinctRegister(Name, VariableId, MachineId)</c>) the update
    /// path either matched the wrong tracked row or threw on <c>FindAsync</c> (wrong number of key values). Returning
    /// every key component closes that wrong-row-update risk.
    /// </remarks>
    public static IndQuestResults.Result<object?[]> GetPrimaryKeyValues<T>(T entity, IIndTraceDbContext context)
    {
        if (entity is null)
        {
            return IndQuestResults.Result<object?[]>.WithFailure("entity cannot be null");
        }

        if (context is null)
        {
            return IndQuestResults.Result<object?[]>.WithFailure("context cannot be null");
        }

        var entityType = context.Model.FindEntityType(typeof(T));
        if (entityType is null)
        {
            return IndQuestResults.Result<object?[]>.WithFailure(
                $"Entity type '{typeof(T).Name}' not found in EF Core model.");
        }

        var key = entityType.FindPrimaryKey();
        if (key is null || key.Properties.Count == 0)
        {
            return IndQuestResults.Result<object?[]>.WithFailure(
                $"Primary key not defined for entity type '{typeof(T).Name}'.");
        }

        var values = new object?[key.Properties.Count];
        for (var i = 0; i < key.Properties.Count; i++)
        {
            var keyProperty = key.Properties[i];
            var propertyInfo = typeof(T).GetProperty(keyProperty.Name);
            if (propertyInfo is null)
            {
                return IndQuestResults.Result<object?[]>.WithFailure(
                    $"Property '{keyProperty.Name}' not found on type '{typeof(T).Name}'.");
            }

            values[i] = propertyInfo.GetValue(entity);
        }

        return IndQuestResults.Result<object?[]>.Success(values);
    }
}