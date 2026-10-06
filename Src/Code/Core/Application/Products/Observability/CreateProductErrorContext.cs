// <copyright file="CreateProductErrorContext.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Products.Observability;

/// <summary>
/// Structured error context for CreateProduct operations.
/// Captures comprehensive context information for error analysis and debugging.
/// Enables detailed failure investigation in industrial environments.
/// </summary>
public class CreateProductErrorContext
{
    /// <summary>Product part number involved in the operation.</summary>
    public string PartNumber { get; set; } = string.Empty;
    
    /// <summary>Customer ID involved in the operation.</summary>
    public int CustomerId { get; set; }
    
    /// <summary>Customer name involved in the operation.</summary>
    public string CustomerName { get; set; } = string.Empty;
    
    /// <summary>Line ID involved in the operation.</summary>
    public int LineId { get; set; }
    
    /// <summary>Product ID (if already assigned).</summary>
    public int? ProductId { get; set; }
    
    /// <summary>Name of the service where the error occurred.</summary>
    public string ServiceName { get; set; } = string.Empty;
    
    /// <summary>Name of the operation that failed.</summary>
    public string OperationName { get; set; } = string.Empty;
    
    /// <summary>Duration of the operation before it failed.</summary>
    public TimeSpan Duration { get; set; }
    
    /// <summary>Activity ID for distributed tracing correlation.</summary>
    public string? ActivityId { get; set; }
    
    /// <summary>Memory allocated during the operation (bytes).</summary>
    public long? MemoryAllocated { get; set; }
    
    /// <summary>Database operations attempted during the failure.</summary>
    public List<string> DatabaseOperations { get; set; } = new();
    
    /// <summary>Business rules that were being evaluated.</summary>
    public List<string> BusinessRulesInvolved { get; set; } = new();
    
    /// <summary>Additional context information specific to the operation.</summary>
    public Dictionary<string, object> AdditionalContext { get; set; } = new();
    
    /// <summary>Stack trace information (if available).</summary>
    public string? StackTrace { get; set; }
    
    /// <summary>Timestamp when the error occurred.</summary>
    public DateTime ErrorTimestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>User or system that initiated the operation.</summary>
    public string? InitiatedBy { get; set; }
    
    /// <summary>Machine or environment where the error occurred.</summary>
    public string? Environment { get; set; }
}