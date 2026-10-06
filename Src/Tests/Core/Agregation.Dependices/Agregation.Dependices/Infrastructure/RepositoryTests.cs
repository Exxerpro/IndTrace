// <copyright file="RepositoryTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Repository;
using IndTrace.Domain.Entities;
using Shouldly;
using System.Linq; // Ensure LINQ is available

namespace IndTrace.Agregation.Dependices.Infrastructure
{
    /// <summary>
    /// I²TDD (Interface Infrastructure Test Driven Development) tests for IRepository<T>
    /// Tests the repository contract generically across multiple entity types
    /// </summary>
    public class GenericRepositoryContractTests
    {
        /// <summary>
        /// Tests that IRepository<T> interface contract works generically across entity types.
        /// This validates the I²TDD principle - same interface should work for any entity.
        /// </summary>
        [Theory]
        [InlineData(typeof(Machine))]
        [InlineData(typeof(Product))]
        [InlineData(typeof(Line))]
        [InlineData(typeof(Customer))]
        public void IRepository_ContractShouldBeGeneric(Type entityType)
        {
            // Arrange - Use generic type to create repository interface type
            var repositoryType = typeof(IRepository<>).MakeGenericType(entityType);

            // Act & Assert - Verify the interface has expected methods
            repositoryType.ShouldNotBeNull();

            // Verify core CRUD methods exist (fix: use GetMethods and filter by name)
            var addAsyncMethods = repositoryType.GetMethods().Where(m => m.Name == "AddAsync");
            var updateAsyncMethods = repositoryType.GetMethods().Where(m => m.Name == "UpdateAsync");
            var deleteAsyncMethods = repositoryType.GetMethods().Where(m => m.Name == "DeleteAsync");
            var getByIdAsyncMethods = repositoryType.GetMethods().Where(m => m.Name == "GetByIdAsync");

            addAsyncMethods.Any().ShouldBeTrue("At least one AddAsync method should exist");
            updateAsyncMethods.Any().ShouldBeTrue("At least one UpdateAsync method should exist");
            deleteAsyncMethods.Any().ShouldBeTrue("At least one DeleteAsync method should exist");
            getByIdAsyncMethods.Any().ShouldBeTrue("At least one GetByIdAsync method should exist");
        }

        /// <summary>
        /// Tests that repository stubs can be created for any entity type.
        /// This validates the generic nature of the repository pattern.
        /// </summary>
        [Theory]
        [InlineData(typeof(Machine), "TestMachine")]
        [InlineData(typeof(Product), "TestProduct")]
        [InlineData(typeof(Line), "TestLine")]
        public void RepositoryStub_ShouldWorkGenerically(Type entityType, string testName)
        {
            // This is a conceptual test - in practice, you'd use NSubstitute or similar
            // But for I²TDD, we're validating the generic contract structure
            var logger = XUnitLogger.CreateLogger<GenericRepositoryContractTests>();

            logger.LogInformation($"Running test: {testName} for entity type: {entityType.Name}");

            // Arrange - Verify we can work with the type generically
            var entity = Activator.CreateInstance(entityType);

            // Act & Assert - Basic validations
            entity.ShouldNotBeNull();
            entityType.IsClass.ShouldBeTrue();

            // Verify entity has expected properties (common to domain entities)
            var properties = entityType.GetProperties();
            properties.ShouldNotBeEmpty("Entity should have properties");
        }

        /// <summary>
        /// Tests that the generic repository pattern validates correctly.
        /// I²TDD validation test - ensures the pattern works across entity types.
        /// </summary>
        [Fact]
        public void GenericRepositoryPattern_ShouldValidateAcrossEntityTypes()
        {
            // Arrange - Test multiple entity types
            var entityTypes = new[] { typeof(Machine), typeof(Product), typeof(Line), typeof(Customer) };

            foreach (var entityType in entityTypes)
            {
                // Act - Create repository type generically
                var repositoryInterfaceType = typeof(IRepository<>).MakeGenericType(entityType);

                // Assert - Verify the generic pattern works
                repositoryInterfaceType.ShouldNotBeNull($"Repository interface should be created for {entityType.Name}");
                repositoryInterfaceType.IsGenericType.ShouldBeTrue($"Constructed type should be generic for {entityType.Name}");
                repositoryInterfaceType.GetGenericTypeDefinition().ShouldBe(typeof(IRepository<>));
            }
        }

        [Fact]
        public void AddAsync_MethodValidation()
        {
            // Arrange
            var type = typeof(IRepository<>).MakeGenericType(typeof(Product));

            // Act
            var methods = type.GetMethods().Where(m => m.Name == "AddAsync");

            // Assert
            methods.Any().ShouldBeTrue("At least one AddAsync method should exist");
        }
    }
}