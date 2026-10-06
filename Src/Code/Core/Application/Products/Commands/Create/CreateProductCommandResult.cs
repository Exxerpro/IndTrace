// <copyright file="CreateProductCommandResult.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

// Fixed: ProductCreatedEvent is in the same namespace, not in Domain.Events.Products

using IndTrace.Application.Products.Events;

namespace IndTrace.Application.Products.Commands.Create;

/// <summary>
/// Result of CreateProduct command execution containing all created entities and metadata.
/// Provides comprehensive information about the product creation operation for validation and monitoring.
/// </summary>
public class CreateProductCommandResult
{
    /// <summary>The created Product entity.</summary>
    public Product Product { get; set; } = new();

    /// <summary>The created Workflow entity.</summary>
    public WorkFlow Workflow { get; set; } = new();

    /// <summary>The created Rule entity.</summary>
    public Rule Rule { get; set; } = new();

    /// <summary>Collection of created Recipe entities (one per machine).</summary>
    public IEnumerable<Recipe> Recipes { get; set; } = new List<Recipe>();

    /// <summary>The ProductCreatedEvent that will be published.</summary>
    public ProductCreatedEvent ProductCreatedEvent { get; set; } = new();

    /// <summary>Dictionary tracking the count of entities created by type.</summary>
    public Dictionary<string, int> CreatedEntities { get; set; } = new();

    /// <summary>Processing metrics for performance monitoring.</summary>
    public ProcessingMetrics ProcessingMetrics { get; set; } = new();

    /// <summary>Additional metadata about the operation.</summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Processing metrics for CreateProduct operations.
/// </summary>
public class ProcessingMetrics
{
    /// <summary>Total number of entities created during the operation.</summary>
    public int TotalEntitiesCreated { get; set; }

    /// <summary>Duration of the entire operation.</summary>
    public TimeSpan TotalDuration { get; set; }

    /// <summary>Duration breakdown by service.</summary>
    public Dictionary<string, TimeSpan> ServiceDurations { get; set; } = new();

    /// <summary>Memory allocated during the operation (bytes).</summary>
    public long MemoryAllocated { get; set; }

    /// <summary>Number of database operations performed.</summary>
    public int DatabaseOperations { get; set; }

    /// <summary>Timestamp when the operation started.</summary>
    public DateTime OperationStarted { get; set; }

    /// <summary>Timestamp when the operation completed.</summary>
    public DateTime OperationCompleted { get; set; }
}