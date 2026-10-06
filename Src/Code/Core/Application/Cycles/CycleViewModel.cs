// <copyright file="CycleViewModel.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Cycles;

/// <summary>
/// View model representing a manufacturing cycle with comparison and mapping capabilities.
/// </summary>
/// <remarks>
/// This view model provides a representation of manufacturing cycles for UI binding and data transfer.
/// It implements IEquatable and IComparable for collection operations and sorting.
/// Contains methods for mapping between entities and DTOs.
/// </remarks>
public class CycleViewModel : IEquatable<CycleViewModel>, IComparable<CycleViewModel>// , IMapFrom<Cycle>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CycleViewModel"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public CycleViewModel()
    {
        this.CycleStatus = CycleStatus.None;
        this.PartStatus = PartStatus.None;
    }

    /// <summary>
    /// Gets or sets the unique identifier for the manufacturing cycle.
    /// </summary>
    /// <value>The cycle ID as an integer.</value>
    public int CycleId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the machine that executed this cycle.
    /// </summary>
    /// <value>The machine ID as an integer.</value>
    public int MachineId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the barcode associated with this cycle.
    /// </summary>
    /// <value>The barcode ID as an integer.</value>
    public int BarCodeId { get; set; }

    /// <summary>
    /// Gets or sets the current status of the manufacturing cycle.
    /// </summary>
    /// <value>A CycleStatus enumeration value indicating the cycle's state.</value>
    public CycleStatus CycleStatus { get; set; }

    /// <summary>
    /// Gets or sets the status of the part being processed in this cycle.
    /// </summary>
    /// <value>A PartStatus enumeration value indicating the part's quality status.</value>
    public PartStatus PartStatus { get; set; }

    /// <summary>
    /// Gets or sets the actual time taken to complete this cycle in seconds.
    /// </summary>
    /// <value>The cycle time as an integer in seconds.</value>
    public int CycleTime { get; set; }

    /// <summary>
    /// Gets or sets the target takt time for this cycle in seconds.
    /// </summary>
    /// <value>The takt time as an integer in seconds.</value>
    /// <remarks>
    /// Takt time represents the desired time between finished products to meet customer demand.
    /// </remarks>
    public int TaktTime { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this cycle was started.
    /// </summary>
    /// <value>The start date and time of the cycle.</value>
    public DateTime StartedOn { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this cycle was completed.
    /// </summary>
    /// <value>The finish date and time of the cycle.</value>
    public DateTime FinishedOn { get; set; }

    // public void Mapping(Profile profile){
    //        profile.CreateMap<Cycle, CycleViewModel>()
    //            .ReverseMap();
    //
    //    }

    /// <summary>
    /// Converts a Cycle entity to a destination DTO type.
    /// </summary>
    /// <typeparam name="TDest">The destination DTO type to convert to.</typeparam>
    /// <param name="src">The source Cycle entity to convert from.</param>
    /// <param name="dest">The destination object to populate, or null to create a new instance.</param>
    /// <returns>The populated destination object.</returns>
    /// <exception cref="ArgumentNullException">Thrown when src is null.</exception>
    /// <remarks>
    /// This method is currently incomplete and requires implementation of property mapping logic.
    /// </remarks>
    public IndQuestResults.Result<TDest> ToDto<TDest>(Cycle src, TDest dest)
        where TDest : new()
    {
        // TODO finish this method
        // 19 MAY 2025
        // CRITICAL
        if (src == null)
        {
            return IndQuestResults.Result<TDest>.WithFailure("Cycle source cannot be null");
        }

        if (dest == null)
        {
            dest = new TDest();

            // Map properties from src.Command to dest as needed
            // Example: Assuming CycleViewModel has properties to map
            // dest.SomeProperty = src.Command.SomeProperty;
            return IndQuestResults.Result<TDest>.Success(dest);
        }

        // Map properties from src.Command to dest as needed
        // Example: Assuming CycleViewModel has properties to map
        // dest.SomeProperty = src.Command.SomeProperty;
        return IndQuestResults.Result<TDest>.Success(dest);
    }

    /// <summary>
    /// Converts a source object to a Cycle entity.
    /// </summary>
    /// <typeparam name="T">The source object type to convert from.</typeparam>
    /// <param name="src">The source object to convert from.</param>
    /// <param name="dest">The destination Cycle entity to populate, or null to create a new instance.</param>
    /// <returns>The populated Cycle entity.</returns>
    /// <exception cref="ArgumentNullException">Thrown when src is null.</exception>
    /// <remarks>
    /// This method is currently incomplete and requires implementation of property mapping logic.
    /// </remarks>
    public IndQuestResults.Result<Cycle> ToEntity<T>(T src, Cycle dest)
    {
        // TODO finish this method
        // 19 MAY 2025
        // CRITICAL
        if (src == null)
        {
            return IndQuestResults.Result<Cycle>.WithFailure("Cycle DTO source cannot be null");
        }

        if (dest == null)
        {
            dest = new Cycle();

            // Map properties from src.Command to dest as needed
            // Example: Assuming Cycle has properties to map
            // dest.SomeProperty = src.Command.SomeProperty;
            return IndQuestResults.Result<Cycle>.Success(dest);
        }

        // Map properties from src.Command to dest as needed
        // Example: Assuming Cycle has properties to map
        // dest.SomeProperty = src.Command.SomeProperty;
        return IndQuestResults.Result<Cycle>.Success(dest);
    }

    /// <summary>
    /// Compares this cycle view model with another for sorting purposes.
    /// </summary>
    /// <param name="other">The other CycleViewModel to compare with.</param>
    /// <returns>A value indicating the relative order of the objects being compared.</returns>
    /// <remarks>
    /// Comparison is based on the CycleId property.
    /// </remarks>
    int IComparable<CycleViewModel>.CompareTo(CycleViewModel? other)
    {
        if (other is null)
        {
            return 1;
        }

        return this.CycleId.CompareTo(other.CycleId);
    }

    /// <summary>
    /// Determines whether this cycle view model is equal to another.
    /// </summary>
    /// <param name="other">The other CycleViewModel to compare with.</param>
    /// <returns>True if the objects are equal based on CycleId, false otherwise.</returns>
    /// <remarks>
    /// Equality is determined by comparing the CycleId properties.
    /// </remarks>
    bool IEquatable<CycleViewModel>.Equals(CycleViewModel? other)
    {
        return this.CycleId == other?.CycleId;
    }
}