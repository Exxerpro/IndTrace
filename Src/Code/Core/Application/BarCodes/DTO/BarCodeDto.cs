// <copyright file="BarCodeDto.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.DTO;

/// <summary>
/// Data Transfer Object (DTO) representing a barcode for communication between application layers.
/// </summary>
/// <remarks>
/// This DTO provides a simplified view of barcode data and includes mapping methods
/// for converting between entity and DTO representations. It's designed for use in
/// API responses, inter-service communication, and UI data binding.
/// </remarks>
public class BarCodeDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the barcode.
    /// </summary>
    /// <value>The barcode's unique identifier.</value>
    public int BarCodeId { get; set; }

    /// <summary>
    /// Gets or sets the label value of the barcode.
    /// </summary>
    /// <value>The barcode label string.</value>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the product associated with this barcode.
    /// </summary>
    /// <value>The product's unique identifier.</value>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the machine that created or processed this barcode.
    /// </summary>
    /// <value>The machine's unique identifier.</value>
    public int MachineId { get; set; }

    /// <summary>
    /// Gets or sets the current status of the part associated with this barcode.
    /// </summary>
    /// <value>The part status enumeration value.</value>
    public PartStatus PartStatus { get; set; }

    /// <summary>
    /// Gets or sets the current flow status of the barcode within the production process.
    /// </summary>
    /// <value>The flow status enumeration value.</value>
    public FlowStatus FlowStatus { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the barcode was created.
    /// </summary>
    /// <value>The creation date and time.</value>
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the barcode was last modified.
    /// </summary>
    /// <value>The last modification date and time.</value>
    public DateTime ModifiedOn { get; set; }

    /// <summary>
    /// Gets or sets the count of cycles associated with this barcode.
    /// </summary>
    /// <value>The number of production cycles.</value>
    public int CycleCount { get; set; }

    /// <summary>
    /// Converts a <see cref="BarCode"/> entity to a <see cref="BarCodeDto"/>.
    /// </summary>
    /// <param name="src">The source barcode entity to convert.</param>
    /// <returns>A new <see cref="BarCodeDto"/> instance with data from the entity.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="src"/> is <c>null</c>.</exception>
    /// <remarks>
    /// This method provides a safe conversion from entity to DTO, preserving all relevant data
    /// while providing a clean interface for external consumers.
    /// </remarks>
    public static IndQuestResults.Result<BarCodeDto> ToDto(BarCode src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<BarCodeDto>.WithFailure("BarCode source cannot be null");
        }

        return IndQuestResults.Result<BarCodeDto>.Success(new BarCodeDto
        {
            BarCodeId = src.BarCodeId.Value,
            Label = src.Label.Value,
            ProductId = src.ProductId.Value,
            MachineId = src.MachineId.Value,

            PartStatus = src.PartStatus,
            FlowStatus = src.FlowStatus,
            CreatedOn = src.CreatedOn,
            ModifiedOn = src.ModifiedOn,
        });
    }

    /// <summary>
    /// Converts a <see cref="BarCodeDto"/> to a <see cref="BarCode"/> entity.
    /// </summary>
    /// <param name="src">The source DTO to convert.</param>
    /// <returns>A new <see cref="BarCode"/> entity instance with data from the DTO.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="src"/> is <c>null</c>.</exception>
    /// <remarks>
    /// This method enables conversion from DTO back to entity for persistence operations.
    /// Care should be taken to ensure all required entity properties are properly set.
    /// </remarks>
    public static IndQuestResults.Result<BarCode> ToEntity(BarCodeDto src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<BarCode>.WithFailure("BarCodeDto source cannot be null");
        }

        var entity = new BarCode
        {
            BarCodeId = new BarCodeId(src.BarCodeId),
            Label = BarCodeLabel.FromPersisted(src.Label),
            ProductId = new ProductId(src.ProductId),
            MachineId = new MachineId(src.MachineId),
            CreatedOn = src.CreatedOn,
            ModifiedOn = src.ModifiedOn,
        };

        // Story 6.4: the status setters are now private set; apply the source values via the trusted seam.
        entity.ApplyFlowAndPartStatus(src.FlowStatus, src.PartStatus);
        return IndQuestResults.Result<BarCode>.Success(entity);
    }

    /// <summary>
    /// Converts a collection of <see cref="BarCode"/> entities to a collection of <see cref="BarCodeDto"/> objects.
    /// </summary>
    /// <param name="src">The source collection of barcode entities.</param>
    /// <returns>A collection of <see cref="BarCodeDto"/> objects converted from the entities.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="src"/> is <c>null</c>.</exception>
    /// <remarks>
    /// This method provides efficient batch conversion of entities to DTOs, useful for
    /// list operations and API responses that need to return multiple barcodes.
    /// </remarks>
    public static IndQuestResults.Result<IEnumerable<BarCodeDto>> ToDtoList(IEnumerable<BarCode> src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<IEnumerable<BarCodeDto>>.WithFailure("BarCode sequence cannot be null");
        }

        // Map using functional ToDto
        var list = new List<BarCodeDto>();
        foreach (var bc in src)
        {
            var mapped = ToDto(bc);
            if (mapped.IsFailure)
            {
                return IndQuestResults.Result<IEnumerable<BarCodeDto>>.WithFailure(mapped.Errors);
            }

            if (mapped.Value is null)
            {
                return IndQuestResults.Result<IEnumerable<BarCodeDto>>.WithFailure("Mapping produced null value for BarCodeDto");
            }

            list.Add(mapped.Value);
        }

        return IndQuestResults.Result<IEnumerable<BarCodeDto>>.Success(list);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BarCodeDto"/> class.
    /// </summary>
    public BarCodeDto()
    {
        this.Label = string.Empty;
        this.PartStatus = PartStatus.None;
        this.FlowStatus = FlowStatus.None;
    }

    // Ensure all required properties are set and handle missing/invalid values gracefully.
}
