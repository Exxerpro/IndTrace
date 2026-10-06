// <copyright file="ISpecification.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Repository;

/// <summary>
/// Defines a specification pattern for querying entities, including filtering, sorting, paging, and including related data.
/// </summary>
/// <typeparam name="T">The type of entity to which the specification applies.</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Gets the criteria expression that defines the condition(s) to be met by entities.
    /// </summary>
    Expression<Func<T, bool>> Criteria { get; }

    /// <summary>
    /// Gets a list of expressions for related entities to be included in the query result.
    /// </summary>
    List<Expression<Func<T, object>>> Includes { get; }

    /// <summary>
    /// Gets a list of strings representing the related entities to be included using string-based paths.
    /// Useful for including nested related entities.
    /// </summary>
    List<string> IncludeStrings { get; }

    /// <summary>
    /// Gets the expression that defines the order by condition for sorting entities in ascending order.
    /// </summary>
    Expression<Func<T, object>>? OrderBy { get; }

    /// <summary>
    /// Gets the expression that defines the order by condition for sorting entities in descending order.
    /// </summary>
    Expression<Func<T, object>>? OrderByDescending { get; }

    /// <summary>
    /// Gets the expression that defines the secondary order by condition for sorting entities in ascending order.
    /// </summary>
    Expression<Func<T, object>>? ThenBy { get; }

    /// <summary>
    /// Gets the expression that defines the secondary order by condition for sorting entities in descending order.
    /// </summary>
    Expression<Func<T, object>>? ThenByDescending { get; }

    /// <summary>
    /// Gets the projection expression to transform the query result.
    /// </summary>
    Expression<Func<T, object>>? Select { get; }

    /// <summary>
    /// Gets a unique key representing this specification instance.
    /// </summary>
    string Key { get; }

    /// <summary>
    /// Gets the number of entities to skip before starting to return the results. Useful for pagination.
    /// </summary>
    int? Skip { get; }

    /// <summary>
    /// Gets the number of entities to return. Useful for pagination.
    /// </summary>
    int? Take { get; }

    /// <summary>
    /// Gets a value indicating whether the query should track the retrieved entities in the context.
    /// </summary>
    bool IsTracking { get; }

    /// <summary>
    /// Adds an include expression for related entities to the specification.
    /// </summary>
    /// <param name="includeExpression">The include expression.</param>
    /// <returns>The updated specification.</returns>
    ISpecification<T> AddInclude(Expression<Func<T, object>> includeExpression);

    /// <summary>
    /// Adds a string-based include for related entities to the specification.
    /// </summary>
    /// <param name="includeString">The include string path.</param>
    /// <returns>The updated specification.</returns>
    ISpecification<T> AddInclude(string includeString);

    /// <summary>
    /// Adds an order by expression for sorting entities in ascending order.
    /// </summary>
    /// <param name="orderByExpression">The order by expression.</param>
    /// <returns>The updated specification.</returns>
    ISpecification<T> AddOrderBy(Expression<Func<T, object>> orderByExpression);

    /// <summary>
    /// Adds an order by descending expression for sorting entities in descending order.
    /// </summary>
    /// <param name="orderByDescExpression">The order by descending expression.</param>
    /// <returns>The updated specification.</returns>
    ISpecification<T> AddOrderByDescending(Expression<Func<T, object>> orderByDescExpression);

    /// <summary>
    /// Adds a secondary order by expression for sorting entities in ascending order.
    /// </summary>
    /// <param name="thenByExpression">The secondary order by expression.</param>
    /// <returns>The updated specification.</returns>
    ISpecification<T> AddThenBy(Expression<Func<T, object>> thenByExpression);

    /// <summary>
    /// Adds a secondary order by descending expression for sorting entities in descending order.
    /// </summary>
    /// <param name="thenByDescExpression">The secondary order by descending expression.</param>
    /// <returns>The updated specification.</returns>
    ISpecification<T> AddThenByDescending(Expression<Func<T, object>> thenByDescExpression);

    /// <summary>
    /// Applies paging to the specification.
    /// </summary>
    /// <param name="skip">The number of entities to skip.</param>
    /// <param name="take">The number of entities to take.</param>
    /// <returns>The updated specification.</returns>
    ISpecification<T> ApplyPaging(int skip, int take);

    /// <summary>
    /// Applies no-tracking behavior to the specification.
    /// </summary>
    /// <returns>The updated specification.</returns>
    ISpecification<T> ApplyNoTracking();

    /// <summary>
    /// Combines this specification with another using AND logic.
    /// </summary>
    /// <param name="specification">The specification to combine with.</param>
    /// <returns>A new specification with combined criteria.</returns>
    ISpecification<T> And(ISpecification<T> specification);

    /// <summary>
    /// Combines this specification with another using OR logic.
    /// </summary>
    /// <param name="specification">The specification to combine with.</param>
    /// <returns>A new specification with combined criteria.</returns>
    ISpecification<T> Or(ISpecification<T> specification);

    /// <summary>
    /// Applies a projection to transform the query result.
    /// </summary>
    /// <param name="selectExpression">The projection expression.</param>
    /// <returns>The updated specification.</returns>
    ISpecification<T> AddSelect(Expression<Func<T, object>> selectExpression);
}

