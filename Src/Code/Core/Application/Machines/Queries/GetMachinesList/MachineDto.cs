// <copyright file="MachineDto.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Machines.Queries.GetMachinesList;

/// <summary>
/// Data transfer object representing a manufacturing machine with its configuration and connectivity information.
/// </summary>
/// <remarks>
/// This DTO contains all machine details including identification, location, traceability settings,
/// network configuration, and workflow relationships. It provides mapping methods for conversion between entity and DTO.
/// #225: all properties are init-only — instances live inside the 60-minute cached
/// <c>ApplicationConfiguration</c> served by reference to every consumer, so post-construction
/// mutation would leak plant-wide.
/// </remarks>
public class MachineDto
{
    /// <summary>
    /// Gets the unique identifier for the machine.
    /// </summary>
    /// <value>The machine ID as an integer.</value>
    public int MachineId { get; init; }

    /// <summary>
    /// Gets the descriptive name of the machine.
    /// </summary>
    /// <value>The machine name as a string. Defaults to empty string if not specified.</value>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the physical location of the machine within the facility.
    /// </summary>
    /// <value>The machine location as a string. Defaults to empty string if not specified.</value>
    public string Location { get; init; } = string.Empty;

    /// <summary>
    /// Gets a detailed description of the machine and its capabilities.
    /// </summary>
    /// <value>The machine description as a string. Defaults to empty string if not specified.</value>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets the type/category of the machine (e.g., Printer, Scanner, Assembly).
    /// </summary>
    /// <value>A MachineType enumeration value indicating the machine's functional category.</value>
    public MachineType MachineType { get; init; } = Domain.Enum.MachineType.None;

    /// <summary>
    /// Gets the workflow type that defines how this machine participates in the production process.
    /// </summary>
    /// <value>A WorkFlowType enumeration value indicating the machine's workflow participation.</value>
    public WorkFlowType WorkFlowType { get; init; } = Domain.Enum.WorkFlowType.None;

    /// <summary>
    /// Gets the IP address for network communication with the machine.
    /// </summary>
    /// <value>The IP address as a string. Defaults to empty string if not specified.</value>
    public string IpAddress { get; init; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether application-level traceability is enabled for this machine.
    /// </summary>
    /// <value>1 if application traceability is enabled, 0 if disabled.</value>
    public int EnableAppTraceability { get; init; }

    /// <summary>
    /// Gets a value indicating whether traceability bypass is enabled for this machine.
    /// </summary>
    /// <value>1 if bypass traceability is enabled, 0 if disabled.</value>
    /// <remarks>
    /// When enabled, this allows the machine to bypass normal traceability requirements.
    /// </remarks>
    public int EnableBypassTraceability { get; init; }

    /// <summary>
    /// Gets a value indicating whether the machine is enabled for operation.
    /// </summary>
    /// <value>
    /// True unless the machine is on the exact disabled combination (App=0, Bypass=1); all other
    /// combinations are enabled by design (fail-safe).
    /// </value>
    /// <remarks>
    /// Two-flag traceability gate (security-by-design). Delegates to <see cref="Machine.IsEnabledFor"/>
    /// so the gate logic has a single source of truth and is never re-derived inline. This is an
    /// intentional safety gate, not primitive obsession, and must not be collapsed to a single boolean.
    /// </remarks>
    public bool IsEnabled => Machine.IsEnabledFor(this.EnableAppTraceability, this.EnableBypassTraceability);

    /// <summary>
    /// Gets the number of retry attempts for operations on this machine.
    /// </summary>
    /// <value>The retry count as an integer. Defaults to 1 if not specified.</value>
    public int Retry { get; init; } = 1;

    /// <summary>
    /// Gets the file name or path of the image associated with this machine.
    /// </summary>
    /// <value>The image file name as a string.</value>
    public string ImageName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the identifier of the business rule associated with this machine.
    /// </summary>
    /// <value>The rule ID as an integer.</value>
    public int RuleId { get; init; }

    /// <summary>
    /// Converts a Machine entity to a MachineDto.
    /// </summary>
    /// <param name="src">The Machine entity to convert.</param>
    /// <returns>A MachineDto containing the converted machine data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when src is null.</exception>
    /// <remarks>
    /// This method handles conversion of enumeration values using EnumModel.FromValue for type safety.
    /// Edge collections are directly assigned as they are reference types.
    /// </remarks>
    /// <summary>
    /// Converts a Machine entity to a MachineDto.
    /// </summary>
    /// <param name="src">The Machine entity to convert.</param>
    /// <returns>A Result containing the MachineDto or failure information.</returns>
    public static Result<MachineDto> ToDto(Machine src)
    {
        if (src == null)
        {
            return Result<MachineDto>.WithFailure($"Parameter '{nameof(src)}' cannot be null");
        }

        return Result<MachineDto>.Success(new MachineDto
        {
            MachineId = src.MachineId.Value,
            Name = src.Name,
            Location = src.Location,
            Description = src.Description,
            MachineType = EnumModel.FromValue<MachineType>(src.MachineType),
            WorkFlowType = EnumModel.FromValue<WorkFlowType>(src.WorkFlowType),
            EnableAppTraceability = src.EnableAppTraceability,
            EnableBypassTraceability = src.EnableBypassTraceability,
            Retry = src.Retry,
            RuleId = src.RuleId,
        });
    }

    /// <summary>
    /// Converts a MachineDto to a Machine entity.
    /// </summary>
    /// <param name="src">The MachineDto to convert.</param>
    /// <returns>A Machine entity containing the converted data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when src is null.</exception>
    /// <remarks>
    /// This method extracts the enumeration values using implicit conversion for database storage.
    /// Edge collections are directly assigned as they are reference types.
    /// </remarks>
    /// <summary>
    /// Converts a MachineDto to a Machine entity.
    /// </summary>
    /// <param name="src">The MachineDto to convert.</param>
    /// <returns>A Result containing the Machine entity or failure information.</returns>
    public static Result<Machine> ToEntity(MachineDto src)
    {
        if (src == null)
        {
            return Result<Machine>.WithFailure($"Parameter '{nameof(src)}' cannot be null");
        }

        return Result<Machine>.Success(new Machine
        {
            MachineId = new MachineId(src.MachineId),
            Name = src.Name,
            Location = src.Location,
            Description = src.Description,
            MachineType = (int)src.MachineType,
            WorkFlowType = (int)src.WorkFlowType,
            EnableAppTraceability = src.EnableAppTraceability,
            EnableBypassTraceability = src.EnableBypassTraceability,
            Retry = src.Retry,
            RuleId = src.RuleId,
        });
    }
}