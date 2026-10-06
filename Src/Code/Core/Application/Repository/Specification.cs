// <copyright file="Specification.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Repository;

using System.Security.Cryptography;
using System.Text.Json.Serialization;

/// <summary>
/// Represents the Specification.
/// </summary>
public class Specification<T>(Expression<Func<T, bool>> criteria) : ISpecification<T>
    where T : class
{
    /// <summary>
    /// Gets the Criteria.
    /// </summary>
    public Expression<Func<T, bool>> Criteria { get; } = criteria;

    /// <summary>
    /// Gets the Includes.
    /// </summary>
    public List<Expression<Func<T, object>>> Includes { get; } = [];

    /// <summary>
    /// Gets the IncludeStrings.
    /// </summary>
    public List<string> IncludeStrings { get; } = [];

    /// <summary>
    /// Gets the OrderBy.
    /// </summary>
    public Expression<Func<T, object>>? OrderBy { get; private set; }

    /// <summary>
    /// Gets the OrderByDescending.
    /// </summary>
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }

    /// <summary>
    /// Gets the ThenBy.
    /// </summary>
    public Expression<Func<T, object>>? ThenBy { get; private set; }

    /// <summary>
    /// Gets the ThenByDescending.
    /// </summary>
    public Expression<Func<T, object>>? ThenByDescending { get; private set; }

    /// <summary>
    /// Gets the Select.
    /// </summary>
    public Expression<Func<T, object>>? Select { get; private set; }

    private string? key;

    /// <summary>
    /// Gets unique cache key based on specification values.
    /// </summary>
    public string Key => this.key ??= this.GenerateKey();

    /// <summary>
    /// Gets the Skip.
    /// </summary>
    public int? Skip { get; private set; }

    /// <summary>
    /// Gets the Take.
    /// </summary>
    public int? Take { get; private set; }

    /// <summary>
    /// Gets a value indicating whether gets or sets the IsTracking.
    /// </summary>
    public bool IsTracking { get; private set; } = true;

    /// <summary>
    /// Executes AddInclude operation.
    /// </summary>
    /// <param name="Expression<Func<T">The Expression.<Func<T.</param>
    /// <param name="includeExpression">The includeExpression.</param>
    /// <returns>The result of AddInclude.</returns>
    public ISpecification<T> AddInclude(Expression<Func<T, object>> includeExpression)
    {
        this.Includes.Add(includeExpression);
        return this;
    }

    /// <summary>
    /// Executes AddInclude operation.
    /// </summary>
    /// <param name="includeString">The includeString.</param>
    /// <returns>The result of AddInclude.</returns>
    public ISpecification<T> AddInclude(string includeString)
    {
        this.IncludeStrings.Add(includeString);
        return this;
    }

    /// <summary>
    /// Executes AddOrderBy operation.
    /// </summary>
    /// <param name="Expression<Func<T">The Expression.<Func<T.</param>
    /// <param name="orderByExpression">The orderByExpression.</param>
    /// <returns>The result of AddOrderBy.</returns>
    public ISpecification<T> AddOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        this.OrderBy = orderByExpression;
        return this;
    }

    /// <summary>
    /// Executes AddOrderByDescending operation.
    /// </summary>
    /// <param name="Expression<Func<T">The Expression.<Func<T.</param>
    /// <param name="orderByDescExpression">The orderByDescExpression.</param>
    /// <returns>The result of AddOrderByDescending.</returns>
    public ISpecification<T> AddOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
    {
        this.OrderByDescending = orderByDescExpression;
        return this;
    }

    /// <summary>
    /// Executes AddThenBy operation.
    /// </summary>
    /// <param name="Expression<Func<T">The Expression.<Func<T.</param>
    /// <param name="thenByExpression">The thenByExpression.</param>
    /// <returns>The result of AddThenBy.</returns>
    public ISpecification<T> AddThenBy(Expression<Func<T, object>> thenByExpression)
    {
        this.ThenBy = thenByExpression;
        return this;
    }

    /// <summary>
    /// Executes AddThenByDescending operation.
    /// </summary>
    /// <param name="Expression<Func<T">The Expression.<Func<T.</param>
    /// <param name="thenByDescExpression">The thenByDescExpression.</param>
    /// <returns>The result of AddThenByDescending.</returns>
    public ISpecification<T> AddThenByDescending(Expression<Func<T, object>> thenByDescExpression)
    {
        this.ThenByDescending = thenByDescExpression;
        return this;
    }

    /// <summary>
    /// Executes ApplyPaging operation.
    /// </summary>
    /// <param name="skip">The skip.</param>
    /// <param name="take">The take.</param>
    /// <returns>The result of ApplyPaging.</returns>
    public ISpecification<T> ApplyPaging(int skip, int take)
    {
        this.Skip = skip;
        this.Take = take;
        return this;
    }

    /// <summary>
    /// Executes ApplyNoTracking operation.
    /// </summary>
    /// <returns>The result of ApplyNoTracking.</returns>
    public ISpecification<T> ApplyNoTracking()
    {
        this.IsTracking = false;
        return this;
    }

    /// <summary>
    /// Executes And operation.
    /// </summary>
    /// <param name="specification">The specification.</param>
    /// <returns>The result of And.</returns>
    public ISpecification<T> And(ISpecification<T> specification)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var leftBody = Expression.Invoke(this.Criteria, parameter);
        var rightBody = Expression.Invoke(specification.Criteria, parameter);
        var combinedBody = Expression.AndAlso(leftBody, rightBody);
        var combinedCriteria = Expression.Lambda<Func<T, bool>>(combinedBody, parameter);

        var newSpec = new Specification<T>(combinedCriteria)
        {
            OrderBy = this.OrderBy,
            OrderByDescending = this.OrderByDescending,
            ThenBy = this.ThenBy,
            ThenByDescending = this.ThenByDescending,
            Select = this.Select,
            Skip = this.Skip,
            Take = this.Take,
            IsTracking = this.IsTracking,
        };

        // Combine includes
        newSpec.Includes.AddRange(this.Includes);
        newSpec.Includes.AddRange(specification.Includes);
        newSpec.IncludeStrings.AddRange(this.IncludeStrings);
        newSpec.IncludeStrings.AddRange(specification.IncludeStrings);

        return newSpec;
    }

    /// <summary>
    /// Executes Or operation.
    /// </summary>
    /// <param name="specification">The specification.</param>
    /// <returns>The result of Or.</returns>
    public ISpecification<T> Or(ISpecification<T> specification)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var leftBody = Expression.Invoke(this.Criteria, parameter);
        var rightBody = Expression.Invoke(specification.Criteria, parameter);
        var combinedBody = Expression.OrElse(leftBody, rightBody);
        var combinedCriteria = Expression.Lambda<Func<T, bool>>(combinedBody, parameter);

        var newSpec = new Specification<T>(combinedCriteria)
        {
            OrderBy = this.OrderBy,
            OrderByDescending = this.OrderByDescending,
            ThenBy = this.ThenBy,
            ThenByDescending = this.ThenByDescending,
            Select = this.Select,
            Skip = this.Skip,
            Take = this.Take,
            IsTracking = this.IsTracking,
        };

        // Combine includes
        newSpec.Includes.AddRange(this.Includes);
        newSpec.Includes.AddRange(specification.Includes);
        newSpec.IncludeStrings.AddRange(this.IncludeStrings);
        newSpec.IncludeStrings.AddRange(specification.IncludeStrings);

        return newSpec;
    }

    /// <summary>
    /// Executes AddSelect operation.
    /// </summary>
    /// <param name="Expression<Func<T">The Expression.<Func<T.</param>
    /// <param name="selectExpression">The selectExpression.</param>
    /// <returns>The result of AddSelect.</returns>
    public ISpecification<T> AddSelect(Expression<Func<T, object>> selectExpression)
    {
        this.Select = selectExpression;
        return this;
    }

    /// <summary>
    /// Generates a unique string key based on the internal state of the specification.
    /// </summary>
    private string GenerateKey()
    {
        var parts = new List<string>
        {
            $"Type:{typeof(T).Name}",
            $"Criteria:{(this.Criteria is not null ? ExpressionToKeyStringVisitor.ToKeyString(this.Criteria) : "null")}",
            $"Includes:[{string.Join(",", this.Includes.Select(x => x.ToString()).OrderBy(s => s))}]",
            $"IncludeStrings:[{string.Join(",", this.IncludeStrings.OrderBy(s => s))}]",
        };

        if (this.OrderBy is not null)
        {
            parts.Add($"OrderBy:{ExpressionToKeyStringVisitor.ToKeyString(this.OrderBy)}");
        }

        if (this.OrderByDescending is not null)
        {
            parts.Add($"OrderByDescending:{ExpressionToKeyStringVisitor.ToKeyString(this.OrderByDescending)}");
        }

        if (this.ThenBy is not null)
        {
            parts.Add($"ThenBy:{ExpressionToKeyStringVisitor.ToKeyString(this.ThenBy)}");
        }

        if (this.ThenByDescending is not null)
        {
            parts.Add($"ThenByDescending:{ExpressionToKeyStringVisitor.ToKeyString(this.ThenByDescending)}");
        }

        if (this.Select is not null)
        {
            parts.Add($"Select:{ExpressionToKeyStringVisitor.ToKeyString(this.Select)}");
        }

        if (this.Skip.HasValue)
        {
            parts.Add($"Skip:{this.Skip.Value}");
        }

        if (this.Take.HasValue)
        {
            parts.Add($"Take:{this.Take.Value}");
        }

        if (!this.IsTracking)
        {
            parts.Add("NoTracking");
        }

        return string.Join("|", parts);
    }
}

