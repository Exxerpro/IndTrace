// <copyright file="ProductDefinition.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.UI.Models.Products
{
    /// <summary>
    /// Defines a product with machine configuration options.
    /// </summary>
    public class ProductDefinition(string name, bool newMachine, string newMachineName)
    {
        /// <summary>
        /// Gets the name of the product.
        /// </summary>
        public string Name { get; init; } = name;

        /// <summary>
        /// Gets or sets a value indicating whether this product requires a new machine.
        /// </summary>
        public bool NewMachine { get; set; } = newMachine;

        /// <summary>
        /// Gets or sets the name of the new machine if NewMachine is true.
        /// </summary>
        public string NewMachineName { get; set; } = newMachineName;
    }
}
