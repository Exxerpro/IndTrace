// <copyright file="UpdatePLCValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Plcs.Commands.Update;

/// <summary>
/// Represents the UpdatePlcValidator.
/// </summary>
public class UpdatePlcValidator : AbstractValidator<UpdatePlcCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePlcValidator"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public UpdatePlcValidator()
    {
        // [Fix]
        // CLAUDE
        // Date: 20/08/2025
        // Reason: Added comprehensive validation for all properties - validator was extremely incomplete, only validating PlcId but missing 7 other properties. Added proper IP validation and string properties for null safety and railway-oriented programming

        // Validate PlcId is required for updates
        this.RuleFor(v => v.PlcId)
            .GreaterThan(0)
            .WithMessage("PLC ID must be greater than 0.");

        // Validate IP address when provided (optional for updates)
        this.When(v => !string.IsNullOrEmpty(v.IpAddress), () =>
        {
            this.RuleFor(v => v.IpAddress)
                .Must(BeValidIpAddress)
                .WithMessage("IP Address must be a valid IPv4 address (e.g., 192.168.1.100).");
        });

        // Validate string properties when provided (optional for updates)
        this.When(v => !string.IsNullOrEmpty(v.PlcType), () =>
        {
            this.RuleFor(v => v.PlcType)
                .Length(1, 50)
                .WithMessage("PLC Type must be between 1 and 50 characters.");
        });

        this.When(v => !string.IsNullOrEmpty(v.PlcBrand), () =>
        {
            this.RuleFor(v => v.PlcBrand)
                .Length(1, 50)
                .WithMessage("PLC Brand must be between 1 and 50 characters.");
        });

        this.When(v => !string.IsNullOrEmpty(v.CommLibrary), () =>
        {
            this.RuleFor(v => v.CommLibrary)
                .Length(1, 100)
                .WithMessage("Communication Library must be between 1 and 100 characters.");
        });

        this.When(v => !string.IsNullOrEmpty(v.Name), () =>
        {
            this.RuleFor(v => v.Name)
                .Length(1, 100)
                .WithMessage("Name must be between 1 and 100 characters.");
        });

        this.When(v => !string.IsNullOrEmpty(v.BrandOwner), () =>
        {
            this.RuleFor(v => v.BrandOwner)
                .Length(1, 100)
                .WithMessage("Brand Owner must be between 1 and 100 characters.");
        });
    }

    /// <summary>
    /// Validates that a string represents a valid IPv4 address.
    /// </summary>
    /// <param name="ipAddress">The IP address string to validate.</param>
    /// <returns>True if valid IPv4 address; otherwise, false.</returns>
    private static bool BeValidIpAddress(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            return false;
        }

        return System.Net.IPAddress.TryParse(ipAddress, out var addr) &&
               addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
    }
}