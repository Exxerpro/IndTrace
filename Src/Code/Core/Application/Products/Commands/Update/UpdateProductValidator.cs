// <copyright file="UpdateProductValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Products.Commands.Update;

/// <summary>
/// Represents the UpdateProductValidator.
/// </summary>
public class UpdateProductValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductValidator()
    {
        // Identifier group: ProductId (>0), NoParte, ProductName
        this.RuleFor(v => v)
            .Must(v =>
                (v.ProductId.HasValue && v.ProductId > 0) ||
                !string.IsNullOrWhiteSpace(v.NoParte) ||
                !string.IsNullOrWhiteSpace(v.ProductName))
            .WithMessage("At least one unique identifier (ProductId > 0, PartNumber, or ProductName) must be provided.");

        // Updatable fields group
        this.RuleFor(v => v)
            .Must(v =>
                !string.IsNullOrWhiteSpace(v.Product) ||
                v.IsActive.HasValue ||
                v.Version.HasValue ||
                !string.IsNullOrWhiteSpace(v.CustomerPartNumber) ||
                !string.IsNullOrWhiteSpace(v.AliasNoParte) ||
                !string.IsNullOrWhiteSpace(v.Description))
            .WithMessage("At least one field to update must be provided.");

        // Individual field validations (only when provided)
        this.When(v => v.ProductId.HasValue, () =>
        {
            this.RuleFor(v => v.ProductId)
                .GreaterThan(0)
                .WithMessage("Product ID must be greater than 0.");
        });

        this.When(v => !string.IsNullOrWhiteSpace(v.NoParte), () =>
        {
            this.RuleFor(v => v.NoParte)
                .Length(3, 50).WithMessage("Part number must be between 3 and 50 characters.");
        });

        this.When(v => !string.IsNullOrWhiteSpace(v.ProductName), () =>
        {
            this.RuleFor(v => v.ProductName)
                .Length(1, 100).WithMessage("Product name must be between 1 and 100 characters.");
        });

        this.When(v => !string.IsNullOrWhiteSpace(v.Product), () =>
        {
            this.RuleFor(v => v.Product)
                .Length(1, 100).WithMessage("Product must be between 1 and 100 characters.");
        });

        this.When(v => !string.IsNullOrWhiteSpace(v.CustomerPartNumber), () =>
        {
            this.RuleFor(v => v.CustomerPartNumber)
                .Length(1, 50).WithMessage("Customer part number must be between 1 and 50 characters.");
        });

        this.When(v => !string.IsNullOrWhiteSpace(v.AliasNoParte), () =>
        {
            this.RuleFor(v => v.AliasNoParte)
                .Length(1, 50).WithMessage("Alias part number must be between 1 and 50 characters.");
        });

        this.When(v => !string.IsNullOrWhiteSpace(v.Description), () =>
        {
            this.RuleFor(v => v.Description)
                .Length(1, 500).WithMessage("Description must be between 1 and 500 characters.");
        });

        this.When(v => v.IsActive.HasValue, () =>
        {
            this.RuleFor(v => v.IsActive)
                .Must(value => value == 0 || value == 1)
                .WithMessage("IsActive must be 0 (inactive) or 1 (active).");
        });

        this.When(v => v.Version.HasValue, () =>
        {
            this.RuleFor(v => v.Version)
                .GreaterThan(0)
                .WithMessage("Version must be greater than 0.");
        });
    }
}