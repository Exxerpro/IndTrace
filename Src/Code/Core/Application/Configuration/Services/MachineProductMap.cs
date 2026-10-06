// <copyright file="MachineProductMap.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Configuration.Services
{
    /// <summary>
    /// Represents a mapping between a machine and a product, including customer and workflow information.
    /// </summary>
    public class MachineProductMap
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MachineProductMap"/> class.
        /// Initializes a new instance of the class.
        /// </summary>
        public MachineProductMap()
        {
            this.MachineName = string.Empty;
            this.PartNumber = string.Empty;
            this.CustomerName = string.Empty;
        }

        /// <summary>
        /// Gets or sets the machine identifier.
        /// </summary>
        public int MachineId { get; set; }

        /// <summary>
        /// Gets or sets the name of the machine.
        /// </summary>
        public string MachineName { get; set; }

        /// <summary>
        /// Gets or sets the product identifier.
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the part number of the product.
        /// </summary>
        public string PartNumber { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier.
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the name of the customer.
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// Gets or sets the workflow identifier.
        /// </summary>
        public int WorkFlowId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the last machine in the workflow, if any.
        /// </summary>
        public int? LastMachineId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the next machine in the workflow, if any.
        /// </summary>
        public int? NextMachineId { get; set; }

        /// <summary>
        /// Gets the product alias for fast access and filtering, formatted as "CustomerName:PartNumber".
        /// </summary>
        public string ProductAlias => $"{this.CustomerName}:{this.PartNumber}";

    }
}
