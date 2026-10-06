// <copyright file="GetProductoDetailQueryValidator.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Products.Queries.GetProductDetail;

/// <summary>
/// Represents the GetProductoDetailQueryValidator.
/// </summary>
public class GetProductoDetailQueryValidator : AbstractValidator<GetProductDetailQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetProductoDetailQueryValidator"/> class.
    /// Initializes a new instance of the class.
    /// </summary>
    public GetProductoDetailQueryValidator()
    {
        this.RuleFor(v => v.ProductId).GreaterThan(0).LessThan(100);
    }
}