// <copyright file="ResultValidation.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Enum;

/// <summary>
/// Represents the validation result of an operation or entity in the system.
/// </summary>
public class ResultValidation : EnumModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResultValidation"/> class.
    /// </summary>
    public ResultValidation()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResultValidation"/> class with specified values.
    /// </summary>
    /// <param name="value">The integer value.</param>
    /// <param name="name">The name.</param>
    /// <param name="displayName">The display name.</param>
    private ResultValidation(int value, string name, string? displayName = null)
        : base(value, name, displayName ?? string.Empty)
    {
    }

    /// <summary>
    /// Gets the 'None' validation result.
    /// </summary>
    public static readonly ResultValidation None
        = new(0, "None");

    /// <summary>
    /// Gets the 'Valid' validation result.
    /// </summary>
    public static readonly ResultValidation Valid
        = new(1, "Valid");

    /// <summary>
    /// Gets the 'Invalid' validation result.
    /// </summary>
    public static readonly ResultValidation Invalid
        = new(-1, "Invalid");

    /// <summary>
    /// Gets the 'BarCodeNotFound' validation result.
    /// </summary>
    public static readonly ResultValidation BarCodeNotFound
        = new(-2, "BarCodeNotFound");

    /// <summary>
    /// Gets the 'WorkFlowNotFound' validation result.
    /// </summary>
    public static readonly ResultValidation WorkFlowNotFound
        = new(-4, "WorkFlowNotFound");

    /// <summary>
    /// Gets the 'MachineNotFound' validation result.
    /// </summary>
    public static readonly ResultValidation MachineNotFound
        = new(-8, "MachineNotFound");

    /// <summary>
    /// Gets the 'CycleNotFound' validation result.
    /// </summary>
    public static readonly ResultValidation CycleNotFound
        = new(-16, "CycleNotFound");

    /// <summary>
    /// Gets the 'WorkFlowNotValid' validation result.
    /// </summary>
    public static readonly ResultValidation WorkFlowNotValid
        = new(-32, "WorkFlowNotValid");

    /// <summary>
    /// Gets the 'PartNotValid' validation result.
    /// </summary>
    public static readonly ResultValidation PartNotValid
        = new(-64, "PartNotValid");

    /// <summary>
    /// Gets the 'DestinationNotValid' validation result.
    /// </summary>
    public static readonly ResultValidation DestinationNotValid
        = new(-128, "DestinationNotValid");

    /// <summary>
    /// Gets the 'PartNumberNotValid' validation result.
    /// </summary>
    public static readonly ResultValidation PartNumberNotValid
        = new(-256, "PartNumberNotValid");

    /// <summary>
    /// Gets the 'RecipeNotFound' validation result.
    /// </summary>
    public static readonly ResultValidation RecipeNotFound
        = new(-512, "RecipeNotFound");

    /// <summary>
    /// Gets the 'ReferencesNotFound' validation result.
    /// </summary>
    public static readonly ResultValidation ReferencesNotFound
        = new(-1024, "ReferencesNotFound");

    /// <summary>
    /// Gets the 'PartRejected' validation result.
    /// </summary>
    public static readonly ResultValidation PartRejected
        = new(-2048, "PieceRejected");

    /// <summary>
    /// Gets the 'InvalidMachine' validation result.
    /// </summary>
    public static readonly ResultValidation InvalidMachine
        = new(-4096, "InvalidMachine");

    /// <summary>
    /// Gets the 'RuleNotFound' validation result.
    /// </summary>
    public static readonly ResultValidation RuleNotFound
        = new(-8192, "RuleNotFound");

    /// <summary>
    /// Gets the 'ProductNotFound' validation result.
    /// </summary>
    public static readonly ResultValidation ProductNotFound
        = new(-16384, "ProductNotFound");

    /// <summary>
    /// Gets the 'ShiftInvalid' validation result.
    /// </summary>
    public static readonly ResultValidation ShiftInvalid
        = new(-32768, "ShiftInvalid");

    /// <summary>
    /// Gets the 'OperationCancelled' validation result.
    /// </summary>
    public static readonly ResultValidation OperationCancelled
        = new(-65536, "OperationCancelled");

    /// <summary>
    /// Gets the 'ExceptionResultValidation' validation result.
    /// </summary>
    public static readonly ResultValidation ExceptionResultValidation
        = new(-131072, "ExceptionResultValidation");

    /// <summary>
    /// Gets the 'InfrastructureFailure' validation result. Distinct from any "not found" / empty-result
    /// miss: it denotes an INFRASTRUCTURE / DATABASE fault on the read path (for example a missing SQL
    /// column on a restored-from-old-backup schema surfacing as a <c>DbException</c>). Kept separate so a
    /// schema/infrastructure fault is never masked as a domain "not found" outcome.
    /// </summary>
    public static readonly ResultValidation InfrastructureFailure
        = new(-262144, "InfrastructureFailure");

    /// <summary>
    /// Implicitly converts a ResultValidation to its integer representation.
    /// </summary>
    /// <param name="enumerator">The enumerator to convert.</param>
    public static implicit operator int(ResultValidation enumerator) => enumerator.Value;

    /// <summary>
    /// Implicitly converts a ResultValidation to its nullable integer representation.
    /// </summary>
    /// <param name="enumerator">The enumerator to convert.</param>
    public static implicit operator int?(ResultValidation enumerator) => enumerator.Value;

    /// <summary>
    /// Implicitly converts a ResultValidation to its string representation.
    /// </summary>
    /// <param name="enumerator">The enumerator to convert.</param>
    public static implicit operator string(ResultValidation enumerator) => enumerator.Value.ToString();

    /// <summary>
    /// Implicitly converts an integer value to a ResultValidation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator ResultValidation(int value) => FromValue<ResultValidation>(value);

    /// <summary>
    /// Implicitly converts a nullable integer value to a ResultValidation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator ResultValidation(int? value) => FromValue<ResultValidation>(value ?? -1);
}