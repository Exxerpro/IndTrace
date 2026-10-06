// <copyright file="CustomerProduct.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities
{
    using System.Text;

    /// <summary>
    /// Represents a customer and their associated products, including logo information.
    /// </summary>
    public class CustomerProduct
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerProduct"/> class.
        /// Initializes a new instance of the class.
        /// </summary>
        public CustomerProduct()
        {
            this.Name = string.Empty;
        }

        /// <summary>
        /// Gets or sets the unique identifier for the customer.
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the name of the customer.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the path to the customer's logo image.
        /// </summary>
        public string CustomerLogo => "/img/Logs/" + this.Name + ".png";

        /// <summary>
        /// Gets or sets the list of products associated with the customer.
        /// </summary>
        public List<Product> Products { get; set; } = [];
    }
}
