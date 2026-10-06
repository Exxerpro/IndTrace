// <copyright file="BarCodeDetailVm.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.BarCodes.Queries.GetBarCodeDetail;

/// <summary>
/// View model containing comprehensive barcode details including machine assignments, production data, and processing status.
/// </summary>
/// <remarks>
/// This view model aggregates all information related to a barcode's processing journey including:
/// - Machine routing information (current, last, next)
/// - Production cycle data and status information
/// - Manufacturing shift and timing data
/// - Quality validation results
/// - Associated registers and variables
/// It provides multiple mapping methods for different source types.
/// </remarks>
public class BarCodeDetailVm
{
    /// <summary>
    /// Gets or sets the identifier of the machine currently processing this barcode.
    /// </summary>
    /// <value>The current machine ID as an integer.</value>
    public int MachineId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the barcode being tracked.
    /// </summary>
    /// <value>The barcode ID as an integer.</value>
    public int BarCodeId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the current manufacturing cycle.
    /// </summary>
    /// <value>The cycle ID as an integer.</value>
    public int CycleId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the machine that previously processed this barcode.
    /// </summary>
    /// <value>The last machine ID as an integer.</value>
    public int LastMachineId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the machine that will next process this barcode.
    /// </summary>
    /// <value>The next machine ID as an integer.</value>
    public int NextMachineId { get; set; }

    /// <summary>
    /// Gets or sets any error message associated with barcode processing.
    /// </summary>
    /// <value>The error message as a string, or empty string if no error occurred.</value>
    public string Error { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the production shift information when this barcode was processed.
    /// </summary>
    /// <value>A Shift object containing shift timing and scheduling data.</value>
    public Shift Shift { get; set; }

    /// <summary>
    /// Gets or sets the production metrics and performance data for this barcode.
    /// </summary>
    /// <value>A ProductionData value object containing performance metrics.</value>
    public ProductionData ProductionData { get; set; }

    /// <summary>
    /// Gets or sets the current status of the manufacturing cycle.
    /// </summary>
    /// <value>A CycleStatus enumeration indicating the cycle's progression state.</value>
    public CycleStatus CycleStatus { get; set; }

    /// <summary>
    /// Gets or sets the status of the barcode within the production workflow.
    /// </summary>
    /// <value>A FlowStatus enumeration indicating the workflow position.</value>
    public FlowStatus FlowStatus { get; set; }

    /// <summary>
    /// Gets or sets the quality status of the part associated with this barcode.
    /// </summary>
    /// <value>A PartStatus enumeration indicating quality validation results.</value>
    public PartStatus PartStatus { get; set; }

    /// <summary>
    /// Gets or sets the type of machine currently processing this barcode.
    /// </summary>
    /// <value>A MachineType enumeration indicating the functional category of the machine.</value>
    public MachineType MachineType { get; set; }

    /// <summary>
    /// Gets or sets the type of workflow this barcode is following.
    /// </summary>
    /// <value>A WorkFlowType enumeration indicating the processing workflow pattern.</value>
    public WorkFlowType WorkFlowType { get; set; }

    /// <summary>
    /// Gets or sets the validation result for this barcode's processing.
    /// </summary>
    /// <value>A ResultValidation enumeration indicating validation outcome.</value>
    public ResultValidation ResultValidation { get; set; }

    /// <summary>
    /// Gets or sets the human-readable label of the barcode.
    /// </summary>
    /// <value>The barcode label as a string.</value>
    public string Label { get; set; }

    /// <summary>
    /// Gets or sets the complete barcode entity with all its properties.
    /// </summary>
    /// <value>A BarCode entity containing full barcode information, or <c>null</c> when no part is scanned.</value>
    public BarCode? BarCode { get; set; }

    /// <summary>
    /// Gets or sets the collection of manufacturing cycles associated with this barcode.
    /// </summary>
    /// <value>A list of Cycle entities representing the processing history.</value>
    public List<Cycle> Cycles { get; set; }

    /// <summary>
    /// Gets or sets the collection of register data captured during barcode processing.
    /// </summary>
    /// <value>A list of Register entities containing measurement and status data.</value>
    public List<Register> Registers { get; set; }

    /// <summary>
    /// Gets or sets the collection of register view models for UI display.
    /// </summary>
    /// <value>A list of RegisterVm objects formatted for user interface presentation.</value>
    public List<RegisterVm> RegistersVm { get; set; }

    /// <summary>
    /// Gets or sets the status monitoring information for this barcode.
    /// </summary>
    /// <value>A StatusMonitor object containing real-time status tracking data.</value>
    public StatusMonitor StatusMonitor { get; set; }

    /// <summary>
    /// Gets or sets the collection of variables associated with barcode processing.
    /// </summary>
    /// <value>A list of Variable entities containing process parameters and measurements.</value>
    public List<Variable> Variables { get; set; }

    /// <summary>
    /// Converts a BarCode entity to a BarCodeDetailVm.
    /// </summary>
    /// <param name="src">The source BarCode entity to convert.</param>
    /// <returns>A BarCodeDetailVm containing the converted barcode data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when src is null.</exception>
    /// <remarks>
    /// This overload maps basic barcode properties and converts enumeration values using EnumModel.FromValue.
    /// Additional properties may require separate population.
    /// </remarks>
    public static IndQuestResults.Result<BarCodeDetailVm> ToDto(BarCode src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<BarCodeDetailVm>.WithFailure("BarCode source cannot be null");
        }

        return IndQuestResults.Result<BarCodeDetailVm>.Success(new BarCodeDetailVm
        {
            MachineId = src.MachineId.Value,
            BarCodeId = src.BarCodeId.Value,
            Label = src.Label.Value,
            FlowStatus = EnumModel.FromValue<FlowStatus>(src.FlowStatus),
            PartStatus = EnumModel.FromValue<PartStatus>(src.PartStatus),
            BarCode = src,

            // Map other properties as needed
        });
    }

    /// <summary>
    /// Converts an IBarCodeResult to a BarCodeDetailVm.
    /// </summary>
    /// <param name="src">The source IBarCodeResult to convert.</param>
    /// <returns>A BarCodeDetailVm containing the comprehensive barcode processing data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when src is null.</exception>
    /// <remarks>
    /// This overload provides more comprehensive mapping including machine routing,
    /// status information, cycle data, and error handling. It initializes collections safely.
    /// </remarks>
    public static IndQuestResults.Result<BarCodeDetailVm> ToDto(IBarCodeResult src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<BarCodeDetailVm>.WithFailure("BarCodeResult source cannot be null");
        }

        return IndQuestResults.Result<BarCodeDetailVm>.Success(new BarCodeDetailVm
        {
            MachineId = src.MachineId,
            BarCodeId = src.BarCodeId,
            CycleId = src.CycleId,
            LastMachineId = src.LastMachineId,
            NextMachineId = src.NextMachineId,
            Error = src.Error ?? string.Empty,
            ResultValidation = src.ResultValidation,
            FlowStatus = src.FlowStatus,
            PartStatus = src.PartStatus,
            CycleStatus = src.CycleStatus,
            MachineType = src.MachineType,
            WorkFlowType = src.WorkFlowType,
            Label = src.Label ?? string.Empty,
            BarCode = src.BarCode,
            Cycles = src.Cycles?.ToList() ?? [],
            RegistersVm = [],

            // Add more mappings as needed
        });
    }

    /// <summary>
    /// Converts an immutable <see cref="BarCodeSnapshot"/> to a BarCodeDetailVm.
    /// </summary>
    /// <param name="src">The source snapshot to convert.</param>
    /// <returns>A BarCodeDetailVm containing the comprehensive barcode processing data.</returns>
    /// <remarks>
    /// Issue #33 (Chunk 3) — the loader-path counterpart to <see cref="ToDto(IBarCodeResult)"/>, mapping the SAME
    /// fields field-for-field so the report view model is byte-identical whether sourced from the god-object or the
    /// immutable snapshot.
    /// </remarks>
    public static IndQuestResults.Result<BarCodeDetailVm> ToDto(BarCodeSnapshot src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<BarCodeDetailVm>.WithFailure("BarCodeSnapshot source cannot be null");
        }

        return IndQuestResults.Result<BarCodeDetailVm>.Success(new BarCodeDetailVm
        {
            MachineId = src.MachineId,
            BarCodeId = src.BarCodeId,
            CycleId = src.CycleId,
            LastMachineId = src.LastMachineId,
            NextMachineId = src.NextMachineId,
            Error = src.Error ?? string.Empty,
            ResultValidation = src.ResultValidation,
            FlowStatus = src.FlowStatus,
            PartStatus = src.PartStatus,
            CycleStatus = src.CycleStatus,
            MachineType = src.MachineType,
            WorkFlowType = src.WorkFlowType,
            Label = src.Label ?? string.Empty,
            BarCode = src.BarCode,
            Cycles = src.Cycles?.ToList() ?? [],
            RegistersVm = [],
        });
    }

    /// <summary>
    /// Converts a BarCodeDetailVm back to a BarCode entity.
    /// </summary>
    /// <param name="src">The source BarCodeDetailVm to convert.</param>
    /// <returns>A BarCode entity containing the core barcode data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when src is null.</exception>
    /// <remarks>
    /// This method extracts the essential barcode properties for persistence.
    /// Complex view model data is not included in the entity conversion.
    /// </remarks>
    public static IndQuestResults.Result<BarCode> ToEntity(BarCodeDetailVm src)
    {
        if (src == null)
        {
            return IndQuestResults.Result<BarCode>.WithFailure("BarCodeDetailVm source cannot be null");
        }

        var entity = new BarCode
        {
            MachineId = new MachineId(src.MachineId),
            BarCodeId = new BarCodeId(src.BarCodeId),
            Label = BarCodeLabel.FromPersisted(src.Label ?? string.Empty),

            // Map other properties as needed
        };

        // Story 6.4: the status setters are now private set; apply the source values via the trusted seam.
        entity.ApplyFlowAndPartStatus(src.FlowStatus.Value, src.PartStatus.Value);
        return IndQuestResults.Result<BarCode>.Success(entity);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BarCodeDetailVm"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public BarCodeDetailVm()
    {
        this.CycleStatus = CycleStatus.None;
        this.FlowStatus = FlowStatus.None;
        this.PartStatus = PartStatus.None;
        this.MachineType = MachineType.None;
        this.WorkFlowType = WorkFlowType.None;
        this.ResultValidation = ResultValidation.None;
        this.Shift = new Shift(new DateTimeMachine());
        this.ProductionData = new ProductionData();
        this.Label = string.Empty;

        // Story 27.2b-2: "no part scanned" is an ABSENT reference (null), not a placeholder empty-label BarCode.
        this.BarCode = null;
        this.Cycles = new List<Cycle>();
        this.Registers = new List<Register>();
        this.RegistersVm = new List<RegisterVm>();
        this.StatusMonitor = new StatusMonitor();
        this.Variables = new List<Variable>();
    }
}