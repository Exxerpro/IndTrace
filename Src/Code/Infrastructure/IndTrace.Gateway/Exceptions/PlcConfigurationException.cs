// <copyright file="PlcConfigurationException.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Gateway.Exceptions
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Exception thrown when there is a configuration error with a PLC variable.
    /// </summary>
    [Serializable]
    public class PlcConfigurationException : Exception
    {
        /// <summary>
        /// Gets the IP address associated with the configuration error.
        /// </summary>
        public string IpAddress { get; init; } = string.Empty; // Ensure non-null default value

        /// <summary>
        /// Gets the variable key associated with the configuration error.
        /// </summary>
        public string VariableKey { get; init; } = string.Empty; // Ensure non-null default value

        /// <summary>
        /// Gets the duplicate address found in the configuration.
        /// </summary>
        public string DuplicateAddress { get; init; } = string.Empty; // Ensure non-null default value

        /// <summary>
        /// Initializes a new instance of the <see cref="PlcConfigurationException"/> class with the specified details.
        /// </summary>
        /// <param name="ipAddress">The IP address of the PLC.</param>
        /// <param name="key">The variable key.</param>
        /// <param name="address">The duplicate address.</param>
        /// <param name="message">The error message.</param>
        public PlcConfigurationException(string ipAddress, string key, string address, string message)
            : base(message)
        {
            this.IpAddress = ipAddress;
            this.VariableKey = key;
            this.DuplicateAddress = address;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlcConfigurationException"/> class.
        /// </summary>
        public PlcConfigurationException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlcConfigurationException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public PlcConfigurationException(string? message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlcConfigurationException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public PlcConfigurationException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Serializes this exception to a JSON string.
        /// </summary>
        /// <returns>A JSON string representation of this exception.</returns>
        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }

        /// <summary>
        /// Deserializes a <see cref="PlcConfigurationException"/> from a JSON string.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized <see cref="PlcConfigurationException"/>.</returns>
        public static PlcConfigurationException FromJson(string json)
        {
            var deserialized = JsonSerializer.Deserialize<PlcConfigurationException>(json);
            if (deserialized == null)
            {
                throw new InvalidOperationException("Deserialization resulted in a null object.");
            }

            return deserialized;
        }
    }
}