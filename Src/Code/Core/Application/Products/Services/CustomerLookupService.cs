// <copyright file="CustomerLookupService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Products.Services;

/// <summary>
/// Customer resolution service with dual lookup strategy.
/// Implements sophisticated CustomerName override logic from original handler.
/// Preserves exact customer resolution behavior and error message formats.
/// </summary>
public class CustomerLookupService : ICustomerLookupService
{
    private readonly IRepository<Customer> _customerRepository;
    private readonly ILogger<CustomerLookupService> _logger;

    public CustomerLookupService(
        IRepository<Customer> customerRepository,
        ILogger<CustomerLookupService> logger)
    {
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Resolves customer using dual resolution strategy.
    /// If CustomerName is provided, it OVERRIDES CustomerId lookup.
    /// Preserves EXACT logic from original handler including edge cases.
    /// </summary>
    public async Task<Result<Customer>> ResolveCustomerAsync(
        int customerId, 
        string customerName, 
        CancellationToken cancellationToken)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<Customer>.WithFailure("Operation was canceled.");
        }

        // Null guard for dependencies
        if (_customerRepository is null)
        {
            return Result<Customer>.WithFailure("Customer repository cannot be null.");
        }

        try
        {
            _logger.LogDebug("Starting customer resolution for CustomerId: {CustomerId}, CustomerName: {CustomerName}", 
                customerId, customerName);

            // DUAL RESOLUTION STRATEGY: CustomerName overrides CustomerId
            // This is critical business logic from the original handler
            
            if (!string.IsNullOrWhiteSpace(customerName))
            {
                // CustomerName PROVIDED: Use name-based lookup (OVERRIDES CustomerId)
                _logger.LogDebug("Using CustomerName-based resolution: {CustomerName}", customerName);
                
                var nameSpec = new Specification<Customer>(c => c.Name == customerName);
                var customerByNameResult = await _customerRepository.FirstOrDefaultAsync(nameSpec, cancellationToken)
                    .ConfigureAwait(false);
                
                if (customerByNameResult.IsSuccess && customerByNameResult.Value is not null)
                {
                    _logger.LogDebug("Customer found by name: {CustomerName}, CustomerId: {CustomerId}", 
                        customerName, customerByNameResult.Value.CustomerId);
                    return Result<Customer>.Success(customerByNameResult.Value);
                }
                
                // CustomerName provided but NOT FOUND - this is a failure case
                _logger.LogWarning("Customer resolution failed - customer not found by name: {CustomerName}", customerName);
                // EXACT error message format from original handler
                return Result<Customer>.WithFailure($"Customer not found {customerName}");
            }
            else
            {
                // CustomerName NOT PROVIDED: Use CustomerId-based lookup
                _logger.LogDebug("Using CustomerId-based resolution: {CustomerId}", customerId);
                
                var customerByIdResult = await _customerRepository.GetByIdAsync(customerId, cancellationToken)
                    .ConfigureAwait(false);
                
                if (customerByIdResult.IsSuccess && customerByIdResult.Value is not null)
                {
                    _logger.LogDebug("Customer found by ID: {CustomerId}, CustomerName: {CustomerName}", 
                        customerId, customerByIdResult.Value.Name);
                    return Result<Customer>.Success(customerByIdResult.Value);
                }
                
                // CustomerId provided but NOT FOUND - this is a failure case
                _logger.LogWarning("Customer resolution failed - customer not found by ID: {CustomerId}", customerId);
                // EXACT error message format from original handler
                return Result<Customer>.WithFailure($"Customer not found {customerId}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while resolving customer for CustomerId: {CustomerId}, CustomerName: {CustomerName}", 
                customerId, customerName);
            return Result<Customer>.WithFailure($"Exception occurred while resolving customer: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates customer exists by CustomerId.
    /// Used for simple ID-based validation without full entity retrieval.
    /// </summary>
    public async Task<Result> ValidateCustomerExistsAsync(int customerId, CancellationToken cancellationToken)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
        {
            return Result.WithFailure("Operation was canceled.");
        }

        // Null guard for dependencies
        if (_customerRepository is null)
        {
            return Result.WithFailure("Customer repository cannot be null.");
        }

        try
        {
            _logger.LogDebug("Validating customer existence for CustomerId: {CustomerId}", customerId);

            var customerResult = await _customerRepository.GetByIdAsync(customerId, cancellationToken)
                .ConfigureAwait(false);

            if (customerResult.IsSuccess && customerResult.Value is not null)
            {
                _logger.LogDebug("Customer validation successful for CustomerId: {CustomerId}", customerId);
                return Result.Success();
            }

            _logger.LogWarning("Customer validation failed - customer not found for CustomerId: {CustomerId}", customerId);
            // EXACT error message format from original handler
            return Result.WithFailure($"Customer not found {customerId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while validating customer existence for CustomerId: {CustomerId}", customerId);
            return Result.WithFailure($"Exception occurred while validating customer: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates customer exists by CustomerName.
    /// Used for name-based validation without full entity retrieval.
    /// </summary>
    public async Task<Result> ValidateCustomerExistsByNameAsync(string customerName, CancellationToken cancellationToken)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
        {
            return Result.WithFailure("Operation was canceled.");
        }

        // Null guard for dependencies
        if (_customerRepository is null)
        {
            return Result.WithFailure("Customer repository cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(customerName))
        {
            return Result.WithFailure("CustomerName cannot be null, empty, or whitespace.");
        }

        try
        {
            _logger.LogDebug("Validating customer existence for CustomerName: {CustomerName}", customerName);

            var nameSpec = new Specification<Customer>(c => c.Name == customerName);
            var customerResult = await _customerRepository.FirstOrDefaultAsync(nameSpec, cancellationToken)
                .ConfigureAwait(false);

            if (customerResult.IsSuccess && customerResult.Value is not null)
            {
                _logger.LogDebug("Customer validation successful for CustomerName: {CustomerName}", customerName);
                return Result.Success();
            }

            _logger.LogWarning("Customer validation failed - customer not found for CustomerName: {CustomerName}", customerName);
            // EXACT error message format from original handler
            return Result.WithFailure($"Customer not found {customerName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while validating customer existence for CustomerName: {CustomerName}", customerName);
            return Result.WithFailure($"Exception occurred while validating customer: {ex.Message}");
        }
    }

    /// <summary>
    /// Resolves customer using ProductInput data.
    /// Convenience method that extracts customer data from ProductInput.
    /// </summary>
    public async Task<Result<Customer>> ResolveCustomerAsync(ProductInput productInput, CancellationToken cancellationToken)
    {
        if (productInput is null)
        {
            return Result<Customer>.WithFailure("ProductInput cannot be null.");
        }

        return await ResolveCustomerAsync(
            productInput.CustomerId,
            productInput.CustomerName,
            cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Gets all active customers available for product assignment.
    /// Placeholder for future enhancement - currently returns empty list.
    /// </summary>
    public async Task<Result<IEnumerable<Customer>>> GetActiveCustomersAsync(CancellationToken cancellationToken)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<IEnumerable<Customer>>.WithFailure("Operation was canceled.");
        }

        try
        {
            _logger.LogDebug("Retrieving active customers for product assignment");

            // Future enhancement: Implement actual active customers query
            // - Query customers with IsActive = true or similar criteria
            // - Order by name or usage frequency
            // - Return collection of active customers

            // For now, return empty list to maintain compatibility
            var emptyList = new List<Customer>();
            _logger.LogDebug("Active customers retrieval successful (placeholder implementation returning empty list)");
            return await Task.FromResult(Result<IEnumerable<Customer>>.Success(emptyList)).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while retrieving active customers");
            return Result<IEnumerable<Customer>>.WithFailure($"Exception occurred while retrieving active customers: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves customer by CustomerId.
    /// Returns the full Customer entity for further processing.
    /// </summary>
    public async Task<Result<Customer>> GetCustomerByIdAsync(int customerId, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<Customer>.WithFailure("Operation was canceled.");
        }
        if (_customerRepository is null)
        {
            return Result<Customer>.WithFailure("Customer repository cannot be null.");
        }
        try
        {
            var result = await _customerRepository.GetByIdAsync(customerId, cancellationToken).ConfigureAwait(false);
            return result.IsSuccess && result.Value is not null
                ? Result<Customer>.Success(result.Value)
                : Result<Customer>.WithFailure($"Customer not found {customerId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetCustomerByIdAsync error");
            return Result<Customer>.WithFailure($"Exception occurred while retrieving customer: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves customer by name for override logic.
    /// </summary>
    public async Task<Result<Customer>> GetCustomerByNameAsync(string customerName, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<Customer>.WithFailure("Operation was canceled.");
        }
        if (_customerRepository is null)
        {
            return Result<Customer>.WithFailure("Customer repository cannot be null.");
        }
        if (string.IsNullOrWhiteSpace(customerName))
        {
            return Result<Customer>.WithFailure("CustomerName cannot be null, empty, or whitespace.");
        }
        try
        {
            var spec = new Specification<Customer>(c => c.Name == customerName);
            var result = await _customerRepository.FirstOrDefaultAsync(spec, cancellationToken).ConfigureAwait(false);
            return result.IsSuccess && result.Value is not null
                ? Result<Customer>.Success(result.Value)
                : Result<Customer>.WithFailure($"Customer not found {customerName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetCustomerByNameAsync error");
            return Result<Customer>.WithFailure($"Exception occurred while retrieving customer: {ex.Message}");
        }
    }
}