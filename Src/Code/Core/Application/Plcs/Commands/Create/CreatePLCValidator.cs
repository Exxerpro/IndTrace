// <copyright file="CreatePLCValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Plcs.Commands.Create;

/// <summary>
/// Validator for <see cref="CreatePlcCommand"/>. Ensures that the IP address is not empty and has the correct length.
/// </summary>
public class CreatePlcValidator : AbstractValidator<CreatePlcCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePlcValidator"/> class.
    /// </summary>
    public CreatePlcValidator()
    {
        // [Fix]
        // CLAUDE
        // Date: 20/08/2025
        // Reason: CRITICAL FIX - IP validation was broken (required exactly 10 chars), added comprehensive validation for all 8 properties missing for null safety and railway-oriented programming

        // Validate ID must be valid positive integer
        this.RuleFor(v => v.PlcId)
            .GreaterThan(0)
            .WithMessage("PLC ID must be greater than 0.");

        // Validate enabled flag (0 or 1)
        this.RuleFor(v => v.Enabled)
            .Must(value => value == 0 || value == 1)
            .WithMessage("Enabled must be 0 (disabled) or 1 (enabled).");

        // CRITICAL FIX: Proper IP address validation instead of broken Length(10)
        this.RuleFor(v => v.IpAddress)
            .NotNull()
            .NotEmpty()
            .Must(BeValidIpAddress)
            .WithMessage("IP Address must be a valid IPv4 address (e.g., 192.168.1.100).");

        // Validate string properties for null safety
        this.RuleFor(v => v.PlcType)
            .NotNull()
            .NotEmpty()
            .Length(1, 50)
            .WithMessage("PLC Type must be between 1 and 50 characters.");

        this.RuleFor(v => v.PlcBrand)
            .NotNull()
            .NotEmpty()
            .Length(1, 50)
            .WithMessage("PLC Brand must be between 1 and 50 characters.");

        this.RuleFor(v => v.CommLibrary)
            .NotNull()
            .NotEmpty()
            .Length(1, 100)
            .WithMessage("Communication Library must be between 1 and 100 characters.");

        this.RuleFor(v => v.BrandOwner)
            .NotNull()
            .NotEmpty()
            .Length(1, 100)
            .WithMessage("Brand Owner must be between 1 and 100 characters.");

        this.RuleFor(v => v.Name)
            .NotNull()
            .NotEmpty()
            .Length(1, 100)
            .WithMessage("Name must be between 1 and 100 characters.");
    }

    /// <summary>
    /// Validates that a string represents a valid IPv4 address.
    /// </summary>
    /// <param name="ipAddress">The IP address string to validate.</param>
    /// <returns>True if valid IPv4 address; otherwise, false.</returns>
    private static bool BeValidIpAddress(string ipAddress)
    {
        // [Fix]
        // CLAUDE
        // Date: 22/08/2025
        // Reason: CLUSTER 1 Fix - Enhanced IPv4 validation for strict industrial PLC requirements. IPAddress.TryParse is too lenient (allows "1", "192.168.1"). Added strict 4-octet validation for manufacturing safety.
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            return false;
        }

        // First check with IPAddress.TryParse for basic validity
        if (!System.Net.IPAddress.TryParse(ipAddress, out var addr) ||
            addr.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
        {
            return false;
        }

        // Strict validation: Must have exactly 4 octets separated by dots
        // This catches incomplete IPs like "192.168.1", "172.16.1", "1" that TryParse allows
        var parts = ipAddress.Split('.');
        if (parts.Length != 4)
        {
            return false;
        }

        // Each octet must be a valid number 0-255
        foreach (var part in parts)
        {
            if (!int.TryParse(part, out var octet) || octet < 0 || octet > 255)
            {
                return false;
            }
        }

        return true;
    }
}