// <copyright file="CreateProductHealthCheck.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Products.Observability;
using IndTrace.Application.Repositories;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace IndTrace.Application.Products.HealthChecks;

/// <summary>
/// Health check for CreateProduct SRP services.
/// Validates repository connectivity and service availability for industrial monitoring.
/// Ensures all dependencies are operational before product creation operations.
/// </summary>
public class CreateProductHealthCheck : IHealthCheck
{
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Customer> _customerRepository;
    private readonly IRepository<Line> _lineRepository;

    // #95 Phase 2 Slice D (policy C, writes-only): this health check only COUNTS workflows, so it consumes
    // the read-only surface. WorkFlow writes are aggregate-scoped through IAggregateRepository<ProductRouting>.
    private readonly IReadOnlyRepository<WorkFlow> _workflowRepository;
    private readonly IRepository<Rule> _ruleRepository;

    // #95 Phase 2 Slice B (policy C, writes-only): this health check only COUNTS recipes, so it consumes the
    // read-only surface. Recipe writes are aggregate-scoped through IAggregateRepository<Product>.
    private readonly IReadOnlyRepository<Recipe> _recipeRepository;
    private readonly ILogger<CreateProductHealthCheck> _logger;
    private readonly IDateTimeMachine _dateTimeMachine;

    public CreateProductHealthCheck(
        IRepository<Product> productRepository,
        IRepository<Customer> customerRepository,
        IRepository<Line> lineRepository,
        IReadOnlyRepository<WorkFlow> workflowRepository,
        IRepository<Rule> ruleRepository,
        IReadOnlyRepository<Recipe> recipeRepository,
        ILogger<CreateProductHealthCheck> logger,
        IDateTimeMachine dateTimeMachine)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
        _lineRepository = lineRepository ?? throw new ArgumentNullException(nameof(lineRepository));
        _workflowRepository = workflowRepository ?? throw new ArgumentNullException(nameof(workflowRepository));
        _ruleRepository = ruleRepository ?? throw new ArgumentNullException(nameof(ruleRepository));
        _recipeRepository = recipeRepository ?? throw new ArgumentNullException(nameof(recipeRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dateTimeMachine = dateTimeMachine ?? throw new ArgumentNullException(nameof(dateTimeMachine));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = CreateProductActivitySource.Source.StartActivity("CreateProduct.HealthCheck");
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogDebug(CreateProductLogEvents.HealthCheckStart,
                "Starting CreateProduct health check, Activity: {ActivityId}", activity?.Id);

            // Test repository connectivity with parallel execution for performance
            var healthCheckTasks = new[]
            {
                CheckRepositoryHealthAsync<Product>("Product", _productRepository.CountAsync, cancellationToken),
                CheckRepositoryHealthAsync<Customer>("Customer", _customerRepository.CountAsync, cancellationToken),
                CheckRepositoryHealthAsync<Line>("Line", _lineRepository.CountAsync, cancellationToken),
                CheckRepositoryHealthAsync<WorkFlow>("WorkFlow", _workflowRepository.CountAsync, cancellationToken),
                CheckRepositoryHealthAsync<Rule>("Rule", _ruleRepository.CountAsync, cancellationToken),
                CheckRepositoryHealthAsync<Recipe>("Recipe", _recipeRepository.CountAsync, cancellationToken)
            };

            var results = await Task.WhenAll(healthCheckTasks);
            stopwatch.Stop();

            // Analyze results
            var healthyRepositories = results.Where(r => r.IsHealthy).ToList();
            var unhealthyRepositories = results.Where(r => !r.IsHealthy).ToList();

            var data = new Dictionary<string, object>
            {
                ["TotalRepositories"] = results.Length,
                ["HealthyRepositories"] = healthyRepositories.Count,
                ["UnhealthyRepositories"] = unhealthyRepositories.Count,
                ["ResponseTimeMs"] = stopwatch.ElapsedMilliseconds,
                ["CheckTimestamp"] = _dateTimeMachine.UtcNow
            };

            // Add individual repository details
            foreach (var result in results)
            {
                data[$"{result.RepositoryName}Count"] = result.Count;
                data[$"{result.RepositoryName}Healthy"] = result.IsHealthy;
                data[$"{result.RepositoryName}ResponseTimeMs"] = result.ResponseTime.TotalMilliseconds;

                if (!result.IsHealthy && !string.IsNullOrEmpty(result.Error))
                {
                    data[$"{result.RepositoryName}Error"] = result.Error;
                }
            }

            // Set activity context
            CreateProductActivitySource.SetServiceContext(activity, nameof(CreateProductHealthCheck), "CheckHealth");
            activity?.SetTag("repositories.total", results.Length.ToString());
            activity?.SetTag("repositories.healthy", healthyRepositories.Count.ToString());
            activity?.SetTag("repositories.unhealthy", unhealthyRepositories.Count.ToString());
            activity?.SetTag("healthCheck.responseTime", stopwatch.ElapsedMilliseconds.ToString());

            // Determine overall health status
            if (unhealthyRepositories.Count == 0)
            {
                // All repositories healthy
                _logger.LogInformation(CreateProductLogEvents.HealthCheckSuccess,
                    "CreateProduct health check passed - all {RepositoryCount} repositories healthy in {Duration}ms, Activity: {ActivityId}",
                    results.Length, stopwatch.ElapsedMilliseconds, activity?.Id);

                return HealthCheckResult.Healthy("CreateProduct services fully operational", data);
            }
            else if (healthyRepositories.Count > unhealthyRepositories.Count)
            {
                // Majority healthy - degraded
                var failedNames = unhealthyRepositories.Select(r => r.RepositoryName).ToList();

                _logger.LogWarning(CreateProductLogEvents.HealthCheckDegraded,
                    "CreateProduct health check degraded - {FailedCount}/{TotalCount} repositories failed: {FailedRepositories}, Duration: {Duration}ms, Activity: {ActivityId}",
                    unhealthyRepositories.Count, results.Length, string.Join(", ", failedNames),
                    stopwatch.ElapsedMilliseconds, activity?.Id);

                return HealthCheckResult.Degraded(
                    $"Repository issues detected: {string.Join(", ", failedNames)}",
                    null, data);
            }
            else
            {
                // Majority unhealthy - unhealthy
                var failedNames = unhealthyRepositories.Select(r => r.RepositoryName).ToList();

                _logger.LogError(CreateProductLogEvents.HealthCheckUnhealthy,
                    "CreateProduct health check failed - {FailedCount}/{TotalCount} repositories failed: {FailedRepositories}, Duration: {Duration}ms, Activity: {ActivityId}",
                    unhealthyRepositories.Count, results.Length, string.Join(", ", failedNames),
                    stopwatch.ElapsedMilliseconds, activity?.Id);

                return HealthCheckResult.Unhealthy(
                    $"Critical repository failures: {string.Join(", ", failedNames)}",
                    null, data);
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            CreateProductActivitySource.SetErrorContext(activity, new[] { ex.Message }, ex);

            _logger.LogError(CreateProductLogEvents.HealthCheckUnhealthy, ex,
                "CreateProduct health check exception after {Duration}ms, Activity: {ActivityId}",
                stopwatch.ElapsedMilliseconds, activity?.Id);

            var errorData = new Dictionary<string, object>
            {
                ["Error"] = ex.Message,
                ["ExceptionType"] = ex.GetType().Name,
                ["ResponseTimeMs"] = stopwatch.ElapsedMilliseconds,
                ["CheckTimestamp"] = _dateTimeMachine.UtcNow
            };

            return HealthCheckResult.Unhealthy("CreateProduct services unavailable due to exception", ex, errorData);
        }
    }

    /// <summary>
    /// Checks the health of an individual repository through its count probe. Takes the count delegate rather
    /// than a repository interface so the same probe serves both the mutating repositories and the read-only
    /// Recipe repository (#95 Phase 2 Slice B downgraded the Recipe dependency to IReadOnlyRepository).
    /// </summary>
    private async Task<RepositoryHealthResult> CheckRepositoryHealthAsync<T>(
        string repositoryName,
        Func<ISpecification<T>, CancellationToken, Task<Result<int>>> countAsync,
        CancellationToken cancellationToken) where T : class, IndTrace.Domain.Interfaces.IPersistable
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var spec = new Specification<T>(x => true); // Simple spec to count all entities
            // Simple count operation to test connectivity
            var countResult = await countAsync(spec, cancellationToken);
            stopwatch.Stop();

            if (countResult.IsSuccess)
            {
                return new RepositoryHealthResult
                {
                    RepositoryName = repositoryName,
                    IsHealthy = true,
                    Count = countResult.Value,
                    ResponseTime = stopwatch.Elapsed
                };
            }
            else
            {
                var errors = string.Join("; ", countResult.Errors);

                _logger.LogWarning(new EventId(4110, "RepositoryHealthFailure"),
                    "Repository {RepositoryName} health check failed: {Errors}, Duration: {Duration}ms",
                    repositoryName, errors, stopwatch.ElapsedMilliseconds);

                return new RepositoryHealthResult
                {
                    RepositoryName = repositoryName,
                    IsHealthy = false,
                    Count = -1,
                    ResponseTime = stopwatch.Elapsed,
                    Error = errors
                };
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(new EventId(4111, "RepositoryHealthException"), ex,
                "Repository {RepositoryName} health check exception: {Message}, Duration: {Duration}ms",
                repositoryName, ex.Message, stopwatch.ElapsedMilliseconds);

            return new RepositoryHealthResult
            {
                RepositoryName = repositoryName,
                IsHealthy = false,
                Count = -1,
                ResponseTime = stopwatch.Elapsed,
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// Result of checking an individual repository's health.
    /// </summary>
    private class RepositoryHealthResult
    {
        public string RepositoryName { get; set; } = string.Empty;
        public bool IsHealthy { get; set; }
        public int Count { get; set; }
        public TimeSpan ResponseTime { get; set; }
        public string? Error { get; set; }
    }
}