// <copyright file="SpecificationExample.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Repository;

/// <summary>
/// Example usage of the enhanced Specification pattern with all new features.
/// </summary>
public static class SpecificationExample
{
    /// <summary>
    /// Example entity for demonstration purposes.
    /// </summary>
    public class Product
    {
        /// <summary>
        /// Gets or sets the RegisterId.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Price.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the Category.
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the IsActive.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the CreatedDate.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the CategoryNavigation.
        /// </summary>
        public Category? CategoryNavigation { get; set; }

        /// <summary>
        /// Gets or sets the Orders.
        /// </summary>
        public List<Order> Orders { get; set; } = [];
    }

    /// <summary>
    /// Represents the Category.
    /// </summary>
    public class Category
    {
        /// <summary>
        /// Gets or sets the RegisterId.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents the Order.
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Gets or sets the RegisterId.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the OrderDate.
        /// </summary>
        public DateTime OrderDate { get; set; }

        /// <summary>
        /// Gets or sets the Amount.
        /// </summary>
        public decimal Amount { get; set; }
    }

    /// <summary>
    /// Demonstrates basic specification usage with filtering.
    /// </summary>
    /// <returns></returns>
    public static ISpecification<Product> GetActiveProductsSpec()
    {
        return new Specification<Product>(p => p.IsActive);
    }

    /// <summary>
    /// Demonstrates specification with string-based includes.
    /// </summary>
    /// <returns></returns>
    public static ISpecification<Product> GetProductsWithCategorySpec()
    {
        return new Specification<Product>(p => p.IsActive)
            .AddInclude("CategoryNavigation")
            .AddInclude("Orders");
    }

    /// <summary>
    /// Demonstrates specification with expression-based includes.
    /// </summary>
    /// <returns></returns>
    public static ISpecification<Product> GetProductsWithCategoryExpressionSpec()
    {
        return new Specification<Product>(p => p.IsActive)
            .AddInclude(nameof(Product.CategoryNavigation))
            .AddInclude(p => p.Orders);
    }

    /// <summary>
    /// Demonstrates specification with ordering (primary and secondary).
    /// </summary>
    /// <returns></returns>
    public static ISpecification<Product> GetProductsOrderedSpec()
    {
        return new Specification<Product>(p => p.IsActive)
            .AddOrderBy(p => p.Category)
            .AddThenBy(p => p.Name)
            .AddInclude(nameof(Product.CategoryNavigation));
    }

    /// <summary>
    /// Demonstrates specification with descending ordering.
    /// </summary>
    /// <returns></returns>
    public static ISpecification<Product> GetProductsOrderedDescSpec()
    {
        return new Specification<Product>(p => p.IsActive)
            .AddOrderByDescending(p => p.Price)
            .AddThenByDescending(p => p.CreatedDate)
            .AddInclude(nameof(Product.CategoryNavigation));
    }

    /// <summary>
    /// Demonstrates specification with paging.
    /// </summary>
    /// <returns></returns>
    public static ISpecification<Product> GetProductsPagedSpec(int page, int pageSize)
    {
        var skip = (page - 1) * pageSize;
        return new Specification<Product>(p => p.IsActive)
            .AddOrderBy(p => p.Name)
            .ApplyPaging(skip, pageSize)
            .AddInclude(nameof(Product.CategoryNavigation));
    }

    /// <summary>
    /// Demonstrates specification with projection/select.
    /// </summary>
    /// <returns></returns>
    public static ISpecification<Product> GetProductNamesSpec()
    {
        return new Specification<Product>(p => p.IsActive)
            .AddSelect(p => new { p.Id, p.Name, p.Category })
            .AddOrderBy(p => p.Name);
    }

    /// <summary>
    /// Demonstrates predicate composition with AND logic.
    /// </summary>
    /// <returns></returns>
    public static ISpecification<Product> GetActiveExpensiveProductsSpec()
    {
        var activeSpec = new Specification<Product>(p => p.IsActive);
        var expensiveSpec = new Specification<Product>(p => p.Price > 100);

        return activeSpec.And(expensiveSpec)
            .AddOrderBy(p => p.Price)
            .AddInclude(nameof(Product.CategoryNavigation));
    }

    /// <summary>
    /// Demonstrates predicate composition with OR logic.
    /// </summary>
    /// <returns></returns>
    public static ISpecification<Product> GetActiveOrExpensiveProductsSpec()
    {
        var activeSpec = new Specification<Product>(p => p.IsActive);
        var expensiveSpec = new Specification<Product>(p => p.Price > 100);

        return activeSpec.Or(expensiveSpec)
            .AddOrderBy(p => p.Name)
            .AddInclude(nameof(Product.CategoryNavigation));
    }

    /// <summary>
    /// Demonstrates complex specification with multiple features combined.
    /// </summary>
    /// <returns></returns>
    public static ISpecification<Product> GetComplexProductSpec()
    {
        var baseSpec = new Specification<Product>(p => p.IsActive && p.Price > 50);

        return baseSpec
            .AddInclude("CategoryNavigation")
            .AddInclude("Orders")
            .AddOrderBy(p => p.Category)
            .AddThenBy(p => p.Name)
            .ApplyPaging(0, 20)
            .ApplyNoTracking();
    }

    /// <summary>
    /// Demonstrates specification with no-tracking for read-only operations.
    /// </summary>
    /// <returns></returns>
    public static ISpecification<Product> GetReadOnlyProductsSpec()
    {
        return new Specification<Product>(p => p.IsActive)
            .AddInclude(nameof(Product.CategoryNavigation))
            .AddOrderBy(p => p.Name)
            .ApplyNoTracking();
    }
}

