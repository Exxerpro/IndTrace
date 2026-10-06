// <copyright file="CustomerDto.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Customers
{
    /// <summary>
    /// Data transfer object for customer information used in application layer operations.
    /// </summary>
    /// <remarks>
    /// This DTO facilitates the transfer of customer data between layers while providing
    /// mapping functionality to convert between domain entities and DTOs.
    /// </remarks>
    public class CustomerDto
    {
        /// <summary>
        /// Gets the unique identifier for the customer.
        /// </summary>
        public int CustomerId { get; init; }

        /// <summary>
        /// Gets the customer name.
        /// </summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether the customer is active.
        /// </summary>
        public bool IsActive { get; init; } = true;

        /// <summary>
        /// Gets a value indicating whether the customer has products running on the production line.
        /// </summary>
        public bool HasProductRunningOnLine { get; init; } = true;

        /// <summary>
        /// Gets the user who created this customer record.
        /// </summary>
        public string CreatedBy { get; init; } = string.Empty;

        /// <summary>
        /// Gets the timestamp when this customer record was created.
        /// </summary>
        public DateTime? CreatedOn { get; init; }

        /// <summary>
        /// Gets the user who last modified this customer record.
        /// </summary>
        public string ModifiedBy { get; init; } = string.Empty;

        /// <summary>
        /// Gets the timestamp when this customer record was last modified.
        /// </summary>
        public DateTime? ModifiedOn { get; init; }

        /// <summary>
        /// Converts a Customer domain entity to a CustomerDto.
        /// </summary>
        /// <param name="src">The customer domain entity to convert.</param>
        /// <returns>A CustomerDto containing the customer data.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source customer is null.</exception>
        public static Result<CustomerDto> ToDto(Customer src)
        {
            if (src == null)
            {
                return Result<CustomerDto>.WithFailure("Customer source cannot be null");
            }

            return Result<CustomerDto>.Success(new CustomerDto
            {
                CustomerId = src.CustomerId,
                Name = src.Name,
                IsActive = src.IsActive,
                CreatedBy = (src as AuditableEntity)?.CreatedBy ?? string.Empty,
                CreatedOn = (src as AuditableEntity)?.CreatedOn,
                ModifiedBy = (src as AuditableEntity)?.ModifiedBy ?? string.Empty,
                ModifiedOn = (src as AuditableEntity)?.ModifiedOn,
            });
        }

        /// <summary>
        /// Converts a CustomerDto to a Customer domain entity.
        /// </summary>
        /// <param name="src">The customer DTO to convert.</param>
        /// <returns>A Customer domain entity containing the DTO data.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source DTO is null.</exception>
        public static Result<Customer> ToEntity(CustomerDto src)
        {
            if (src == null)
            {
                return Result<Customer>.WithFailure("CustomerDto source cannot be null");
            }

            var entity = new Customer
            {
                CustomerId = src.CustomerId,
                Name = src.Name,
                IsActive = src.IsActive,
            };
            if (entity is AuditableEntity auditable)
            {
                auditable.CreatedBy = src.CreatedBy ?? string.Empty;
                auditable.CreatedOn = src.CreatedOn;
                auditable.ModifiedBy = src.ModifiedBy ?? string.Empty;
                auditable.ModifiedOn = src.ModifiedOn;
            }

            return Result<Customer>.Success(entity);
        }

        /// <summary>
        /// Converts a collection of Customer domain entities to a collection of CustomerDtos.
        /// </summary>
        /// <param name="src">The collection of customer domain entities to convert.</param>
        /// <returns>A collection of CustomerDtos containing the customer data.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source collection is null.</exception>
        public static Result<List<CustomerDto>> ToDtoList(IEnumerable<Customer> src)
        {
            if (src == null)
            {
                return Result<List<CustomerDto>>.WithFailure("Customer collection cannot be null");
            }

            var list = src.Select(s => new CustomerDto
            {
                CustomerId = s.CustomerId,
                Name = s.Name,
                IsActive = s.IsActive,
                CreatedBy = (s as AuditableEntity)?.CreatedBy ?? string.Empty,
                CreatedOn = (s as AuditableEntity)?.CreatedOn,
                ModifiedBy = (s as AuditableEntity)?.ModifiedBy ?? string.Empty,
                ModifiedOn = (s as AuditableEntity)?.ModifiedOn,
            }).ToList();
            return Result<List<CustomerDto>>.Success(list);
        }

        /// <summary>
        /// Converts a collection of CustomerDtos to a collection of Customer domain entities.
        /// </summary>
        /// <param name="src">The collection of customer DTOs to convert.</param>
        /// <returns>A collection of Customer domain entities containing the DTO data.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source collection is null.</exception>
        public static Result<List<Customer>> ToEntityList(IEnumerable<CustomerDto> src)
        {
            if (src == null)
            {
                return Result<List<Customer>>.WithFailure("CustomerDto collection cannot be null");
            }

            var list = src.Select(s => new Customer
            {
                CustomerId = s.CustomerId,
                Name = s.Name,
                IsActive = s.IsActive,
            }).ToList();
            foreach (var (entity, dto) in list.Zip(src))
            {
                if (entity is AuditableEntity auditable)
                {
                    auditable.CreatedBy = dto.CreatedBy ?? string.Empty;
                    auditable.CreatedOn = dto.CreatedOn;
                    auditable.ModifiedBy = dto.ModifiedBy ?? string.Empty;
                    auditable.ModifiedOn = dto.ModifiedOn;
                }
            }

            return Result<List<Customer>>.Success(list);
        }
    }
}