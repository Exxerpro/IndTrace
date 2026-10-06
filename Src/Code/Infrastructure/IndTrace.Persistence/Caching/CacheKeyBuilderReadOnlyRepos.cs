// <copyright file="CacheKeyBuilderReadOnlyRepos.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Collections.Generic;
using System.Linq;
using IndTrace.Application.Repository;

namespace IndTrace.Persistence.Caching;

/// <summary>
/// Provides key generation utilities for caching repository queries.
/// </summary>
public static class CacheKeyBuilderReadOnlyRepos
{
    /// <summary>
    /// #116: the key-segment prefix that carries the entity type name inside every key built here
    /// (<c>…|Type:{EntityType}|…</c>). Also the shape of the FusionCache tag every entry is stored under,
    /// so a write can invalidate all cached reads of one entity type via
    /// <c>ICacheService.RemoveByPatternAsync(entityTypeName)</c>.
    /// </summary>
    public const string TypeSegmentPrefix = "Type:";

    private static ICachePartitionProvider? _partitionProvider;
    private static bool _hashSpecKeys;
    private static int _hashLength = 16;

    /// <summary>
    /// Builds the FusionCache tag under which every cache entry of the given entity type is stored
    /// (e.g. <c>Type:Machine</c>). Single source of truth for the tag shape — must stay consistent
    /// with the <c>|Type:{EntityType}|</c> segment the <c>BuildKey</c> overloads emit.
    /// </summary>
    /// <param name="entityTypeName">The entity type name (e.g. <c>nameof(Machine)</c>).</param>
    /// <returns>The type tag for that entity type.</returns>
    public static string BuildTypeTag(string entityTypeName) => $"{TypeSegmentPrefix}{entityTypeName}";

    /// <summary>
    /// Extracts the type tag (<c>Type:{EntityType}</c>) from a cache key built by this builder, or
    /// <see langword="null"/> when the key carries no type segment (e.g. an ad-hoc key not built here).
    /// Tolerates a partition prefix (<c>{prefix}:…</c>) because segments are matched after splitting on
    /// the <c>|</c> separator.
    /// </summary>
    /// <param name="key">The full cache key.</param>
    /// <returns>The type tag segment, or <see langword="null"/> when absent.</returns>
    public static string? TryGetTypeTag(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        foreach (var segment in key.Split('|'))
        {
            if (segment.StartsWith(TypeSegmentPrefix, StringComparison.Ordinal)
                && segment.Length > TypeSegmentPrefix.Length)
            {
                return segment;
            }
        }

        return null;
    }

    /// <summary>
    /// #194: determines whether a cache key built by this builder is scoped to the given repository
    /// operation (e.g. <c>CountAsync</c>). The operation name is canonicalized through the same
    /// normalization the <c>BuildKey</c> overloads use, so callers pass the raw operation name and never
    /// duplicate the alias map. Matching inspects the first <c>|</c>-separated segment of the key and
    /// tolerates a partition prefix (<c>{prefix}:{operation}|…</c>), mirroring
    /// <see cref="TryGetTypeTag"/>. Both spec-key shapes (hashed and unhashed) start with the operation
    /// segment, so both are matched.
    /// </summary>
    /// <param name="key">The full cache key.</param>
    /// <param name="operation">The repository operation name (raw; normalized internally).</param>
    /// <returns><see langword="true"/> when the key's operation segment is the canonical form of
    /// <paramref name="operation"/>.</returns>
    public static bool IsOperationScoped(string key, string operation)
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(operation))
        {
            return false;
        }

        var canonical = NormalizeOperation(operation);
        var separatorIndex = key.IndexOf('|');
        var firstSegment = separatorIndex < 0 ? key : key[..separatorIndex];

        return firstSegment.Equals(canonical, StringComparison.Ordinal)
            || firstSegment.EndsWith($":{canonical}", StringComparison.Ordinal);
    }

    // Normalization map for operation names to avoid key divergence due to aliases/casing.
    private static readonly Dictionary<string, string> OperationMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["getbyid"] = "GetById",
        ["get"] = "GetById",
        ["fetchbyid"] = "GetById",

        ["getbyids"] = "GetByIds",
        ["fetchbyids"] = "GetByIds",
        ["getrangebyids"] = "GetByIds",

        ["listasync"] = "ListAsync",
        ["list"] = "ListAsync",
        ["getall"] = "ListAsync",

        ["firstordefault"] = "FirstOrDefault",
        ["first"] = "FirstOrDefault",

        ["countasync"] = "CountAsync",
        ["count"] = "CountAsync",
    };

    /// <summary>
    /// Sets the partition provider for cache key generation
    /// </summary>
    public static void SetPartitionProvider(ICachePartitionProvider? provider)
    {
        _partitionProvider = provider;
    }

    /// <summary>
    /// Sets cache key options (e.g., hashing behavior for specification keys)
    /// </summary>
    public static void SetOptions(CacheKeyOptions? options)
    {
        if (options is null) return;
        _hashSpecKeys = options.HashSpecKeys;
        _hashLength = options.HashLength > 0 ? options.HashLength : _hashLength;
    }

    /// <summary>
    ///  Builds a unique cache key based on the operation name and specification key.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="operation"></param>
    /// <param name="spec"></param>
    /// <returns></returns>
    public static string BuildKey<T>(string operation, ISpecification<T> spec)
        where T : class
    {
        var canonical = NormalizeOperation(operation);
        if (_hashSpecKeys)
        {
            var hash = ComputeHash(spec.Key, _hashLength);
            var baseKeyHashed = $"{canonical}|Type:{typeof(T).Name}|Spec:{hash}";
            return ApplyPartition(baseKeyHashed);
        }

        // Include the entity type to avoid cross-entity collisions for identical spec keys
        var baseKey = $"{canonical}|Type:{typeof(T).Name}|Spec:{spec.Key}";
        return ApplyPartition(baseKey);
    }

    /// <summary>
    /// Builds a unique cache key based on the operation name, entity type, and identifier.
    /// Uses proper entity-specific ID field names to prevent cache collisions.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="operation"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static string BuildKey<T>(string operation, int id)
        where T : class
    {
        var typeName = typeof(T).Name;
        var canonicalOp = NormalizeOperation(operation);

        // If the entity is known to have a composite key, but a single id is provided,
        // fall back to positional label to avoid misleading single-id field names.
        var composite = GetCompositeIdFieldNames<T>();
        if (composite is not null && composite.Length > 0)
        {
            var baseKeyComposite = $"{canonicalOp}|Type:{typeName}|Id0:{id}";
            return ApplyPartition(baseKeyComposite);
        }

        var baseKey = $"{canonicalOp}|Type:{typeName}|{GetEntityIdFieldName<T>()}:{id}";
        return ApplyPartition(baseKey);
    }

    /// <summary>
    /// Builds a unique cache key based on the operation name and entity type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="operation"></param>
    /// <returns></returns>
    public static string BuildKey<T>(string operation) where T : class
    {
        var baseKey = $"{NormalizeOperation(operation)}|Type:{typeof(T).Name}";
        return ApplyPartition(baseKey);
    }

    /// <summary>
    /// Builds a unique cache key for composite-key entities using ordered field names and values.
    /// </summary>
    public static string BuildKey<T>(string operation, int id1, int id2) where T : class
    {
        var typeName = typeof(T).Name;
        var canonicalOp = NormalizeOperation(operation);
        var fields = GetCompositeIdFieldNames<T>();

        string baseKey;
        if (fields is not null && fields.Length >= 2)
        {
            baseKey = $"{canonicalOp}|Type:{typeName}|{fields[0]}:{id1}|{fields[1]}:{id2}";
        }
        else
        {
            // Fallback to positional labels to remain deterministic
            baseKey = $"{canonicalOp}|Type:{typeName}|Id0:{id1}|Id1:{id2}";
        }

        return ApplyPartition(baseKey);
    }

    /// <summary>
    /// Builds a unique cache key for composite-key entities using ordered field names and values.
    /// </summary>
    public static string BuildKey<T>(string operation, params object[] ids) where T : class
    {
        var typeName = typeof(T).Name;
        var canonicalOp = NormalizeOperation(operation);
        var fields = GetCompositeIdFieldNames<T>();

        string idsPart;
        if (fields is not null && fields.Length == ids.Length)
        {
            var parts = Enumerable.Range(0, ids.Length)
                .Select(i => $"{fields[i]}:{ids[i]}");
            idsPart = string.Join("|", parts);
        }
        else
        {
            // Fallback to positional labels to remain deterministic
            var parts = Enumerable.Range(0, ids.Length)
                .Select(i => $"Id{i}:{ids[i]}");
            idsPart = string.Join("|", parts);
        }

        var baseKey = $"{canonicalOp}|Type:{typeName}|{idsPart}";
        return ApplyPartition(baseKey);
    }

    //[Fix]
    //CLAUDE
    //Date: 05/09/2025
    //Reason: [Performance Enhancement] - Added static dictionary for O(1) entity ID field lookups
    //        Replaces pattern matching with faster dictionary access for high-frequency cache operations

    /// <summary>
    /// Static dictionary for O(1) lookup of entity ID field names.
    /// Initialized once and reused for all cache key generation operations.
    /// </summary>
    private static readonly Dictionary<string, string> EntityIdFieldMap = new()
    {
        [nameof(Machine)] = nameof(Machine.MachineId),
        [nameof(Recipe)] = nameof(Recipe.RecipeId),
        [nameof(Product)] = nameof(Product.ProductId),
        [nameof(Customer)] = nameof(Customer.CustomerId),
        [nameof(Cycle)] = nameof(Cycle.CycleId),
        [nameof(BarCode)] = nameof(BarCode.BarCodeId),
        [nameof(Shift)] = nameof(Shift.ShiftId),
        [nameof(Line)] = nameof(Line.LineId),
        [nameof(Plc)] = nameof(Plc.PlcId),
        [nameof(MasterLabel)] = nameof(MasterLabel.MasterLabelId),
        [nameof(Register)] = nameof(Register.RegisterId),
        [nameof(Rule)] = nameof(Rule.RuleId),
        [nameof(Variable)] = nameof(Variable.VariableId),
        [nameof(VariablesGroup)] = nameof(VariablesGroup.VariableGroupId),
        [nameof(WorkFlow)] = nameof(WorkFlow.WorkFlowId),
        [nameof(TaskGatewayRequest)] = nameof(TaskGatewayRequest.CommandId),
        [nameof(OeeRegister)] = nameof(OeeRegister.OeeRegisterId),
        [nameof(KpiOee)] = nameof(KpiOee.KpiOeeId),
        [nameof(PerformanceData)] = nameof(PerformanceData.PerformanceDataId)
    };

    /// <summary>
    /// Field names for composite keys for known entities (in order).
    /// </summary>
    private static readonly Dictionary<string, string[]> CompositeEntityIdMap = new()
    {
        [nameof(MachinePlc)] = new[] { nameof(MachinePlc.MachineId), nameof(MachinePlc.PlcId) },
        [nameof(DistinctRegister)] = new[] { nameof(DistinctRegister.MachineId), nameof(DistinctRegister.VariableId) },
    };

    /// <summary>
    /// Gets the appropriate ID field name for the entity type to ensure unique cache keys.
    /// Uses O(1) dictionary lookup for optimal performance in high-frequency cache operations.
    /// This prevents cache collisions where different entity types with the same ID
    /// would share the same cache entry.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <returns>The entity-specific ID field name</returns>
    private static string GetEntityIdFieldName<T>()
        where T : class
    {
        var entityType = typeof(T).Name;
        return EntityIdFieldMap.TryGetValue(entityType, out var fieldName)
            ? fieldName
            : $"{entityType}RegisterId"; // Fallback for unmapped entity types
    }

    private static string[]? GetCompositeIdFieldNames<T>() where T : class
    {
        var entityType = typeof(T).Name;
        return CompositeEntityIdMap.TryGetValue(entityType, out var fields)
            ? fields
            : null;
    }

    //[Fix]
    //CLAUDE
    //Date: 13/09/2025
    //Reason: [Cache Partitioning] - Added partition support for test isolation
    /// <summary>
    /// Applies partition prefix if provider is configured
    /// </summary>
    private static string ApplyPartition(string key)
    {
        var prefix = _partitionProvider?.GetPrefix();
        return string.IsNullOrEmpty(prefix) ? key : $"{prefix}:{key}";
    }

    private static string NormalizeOperation(string operation)
    {
        if (string.IsNullOrWhiteSpace(operation))
            return string.Empty;
        var lower = operation.Replace(" ", string.Empty).ToLowerInvariant();
        // Remove spaces to avoid accidental divergence even for unmapped operations
        return OperationMap.TryGetValue(lower, out var canonical)
            ? canonical
            : operation.Replace(" ", string.Empty);
    }

    private static string ComputeHash(string value, int length)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(value);
        var hash = sha.ComputeHash(bytes);
        var hex = BitConverter.ToString(hash).Replace("-", string.Empty);
        return hex.Substring(0, Math.Min(length, hex.Length));
    }
}
