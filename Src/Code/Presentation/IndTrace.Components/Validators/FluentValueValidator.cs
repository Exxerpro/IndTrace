// <copyright file="FluentValueValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using FluentValidation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndTrace.Components.Validators
{
    /// <summary>
    /// A glue class to make it easy to define validation rules for single values using FluentValidation.
    /// You can reuse this class for all your fields, like for the credit card rules above.
    /// </summary>
    /// <typeparam name="T">The type of value to validate.</typeparam>
    public class FluentValueValidator<T> : AbstractValidator<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FluentValueValidator{T}"/> class with the specified validation rule.
        /// </summary>
        /// <param name="rule">The validation rule to apply to the value.</param>
        public FluentValueValidator(Action<IRuleBuilderInitial<T, T>> rule)
        {
            rule(this.RuleFor(x => x));
        }

        /// <summary>
        /// Validates the specified value and returns a collection of error messages, if any.
        /// </summary>
        /// <param name="arg">The value to validate.</param>
        /// <returns>A collection of error messages, or an empty collection if valid.</returns>
        private IEnumerable<string> ValidateValue(T arg)
        {
            var result = this.Validate(arg);
            if (result.IsValid)
                return new string[0];
            return result.Errors.Select(e => e.ErrorMessage);
        }

        /// <summary>
        /// Gets the validation function that can be used to validate values.
        /// </summary>
        public Func<T, IEnumerable<string>> Validation => this.ValidateValue;
    }

    /// <summary>
    /// Represents a model for barcode validation with required label constraints.
    /// </summary>
    public class ModelBarCode
    {
        /// <summary>
        /// Gets or sets the barcode label with length validation.
        /// </summary>
        [Required]
        [StringLength(120, ErrorMessage = "Label length can't be more than 120.")]
        public required string Label { get; set; }
    }

    /// <summary>
    /// Provides information about the IndTrace Components library.
    /// </summary>
    public class IndTraceComponents
    {
        /// <summary>
        /// Gets the name of the IndTrace Components library.
        /// </summary>
        public string LibraryName => "IndTrace.Components";

        /// <summary>
        /// Gets the version of the IndTrace Components library.
        /// </summary>
        public string Version => "1.0.0";
    }
}

