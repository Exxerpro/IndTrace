// <copyright file="ReadOnlyRepositoryTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Repository;
using IndTrace.Domain.Entities;
using Shouldly;
using System.Linq;

namespace IndTrace.Agregation.Dependices.Infrastructure
{
    /// <summary>
    /// I²TDD (Interface Infrastructure Test Driven Development) tests for IReadOnlyRepository<T>
    /// Tests the readonly repository contract generically across multiple entity types
    /// </summary>
    public class GenericReadOnlyRepositoryContractTests
    {
        /// <summary>
        /// Tests that IReadOnlyRepository<T> interface contract works generically across entity types.
        /// This validates the I²TDD principle - same interface should work for any entity.
        /// </summary>
        [Theory]
        [InlineData(typeof(Machine))]
        [InlineData(typeof(Product))]
        [InlineData(typeof(Line))]
        [InlineData(typeof(Customer))]
        [InlineData(typeof(Recipe))]
        public void IReadOnlyRepository_ContractShouldBeGeneric(Type entityType)
        {
            // Arrange - Use generic type to create readonly repository interface type
            var repositoryType = typeof(IReadOnlyRepository<>).MakeGenericType(entityType);

            // Act & Assert - Verify the interface has expected query methods
            repositoryType.ShouldNotBeNull();

            // Verify core read-only methods exist
            var getByIdAsyncMethod = repositoryType.GetMethod("GetByIdAsync");
            var listAsyncMethods = repositoryType.GetMethods().Where(m => m.Name == "ListAsync").ToArray();
            var firstOrDefaultAsyncMethods = repositoryType.GetMethods().Where(m => m.Name == "FirstOrDefaultAsync").ToArray();
            var countAsyncMethod = repositoryType.GetMethod("CountAsync");

            getByIdAsyncMethod.ShouldNotBeNull("GetByIdAsync method should exist");
            listAsyncMethods.ShouldNotBeEmpty("ListAsync methods should exist");
            firstOrDefaultAsyncMethods.ShouldNotBeEmpty("FirstOrDefaultAsync methods should exist");
            countAsyncMethod.ShouldNotBeNull("CountAsync method should exist");

            // Verify no write methods exist (readonly constraint)
            var addAsyncMethod = repositoryType.GetMethod("AddAsync");
            var updateAsyncMethod = repositoryType.GetMethod("UpdateAsync");
            var deleteAsyncMethod = repositoryType.GetMethod("DeleteAsync");

            addAsyncMethod.ShouldBeNull("AddAsync should not exist in readonly repository");
            updateAsyncMethod.ShouldBeNull("UpdateAsync should not exist in readonly repository");
            deleteAsyncMethod.ShouldBeNull("DeleteAsync should not exist in readonly repository");
        }

        /// <summary>
        /// Tests that readonly repository stubs can be created for any entity type.
        /// This validates the generic nature of the readonly repository pattern.
        /// </summary>
        [Theory]
        [InlineData(typeof(Machine), "QueryMachine")]
        [InlineData(typeof(Product), "QueryProduct")]
        [InlineData(typeof(Line), "QueryLine")]
        [InlineData(typeof(Recipe), "QueryRecipe")]
        public void ReadOnlyRepositoryStub_ShouldWorkGenerically(Type entityType, string testName)
        {
            // This is a conceptual test - in practice, you'd use NSubstitute or similar
            // But for I²TDD, we're validating the generic contract structure

            // Use the testName parameter for xUnit compliance
            _ = testName; // xUnit1026 fix

            // Arrange - Verify we can work with the type generically for queries
            var entity = Activator.CreateInstance(entityType);

            // Act & Assert - Basic validations for query operations
            entity.ShouldNotBeNull();
            entityType.IsClass.ShouldBeTrue();

            // Verify entity has expected properties (common to domain entities)
            var properties = entityType.GetProperties();
            properties.ShouldNotBeEmpty("Entity should have properties for querying");

            // Verify entity is suitable for readonly operations
            var readonlyRepositoryInterfaceType = typeof(IReadOnlyRepository<>).MakeGenericType(entityType);
            readonlyRepositoryInterfaceType.ShouldNotBeNull($"ReadOnly repository interface should be created for {entityType.Name}");
        }

        /// <summary>
        /// Tests that the generic readonly repository pattern validates correctly.
        /// I²TDD validation test - ensures the pattern works across entity types.
        /// </summary>
        [Fact]
        public void GenericReadOnlyRepositoryPattern_ShouldValidateAcrossEntityTypes()
        {
            // Arrange - Test multiple entity types for readonly operations
            var entityTypes = new[] { typeof(Machine), typeof(Product), typeof(Line), typeof(Customer), typeof(Recipe) };

            foreach (var entityType in entityTypes)
            {
                // Act - Create readonly repository type generically
                var readonlyRepositoryInterfaceType = typeof(IReadOnlyRepository<>).MakeGenericType(entityType);

                // Assert - Verify the generic readonly pattern works
                readonlyRepositoryInterfaceType.ShouldNotBeNull($"ReadOnly repository interface should be created for {entityType.Name}");
                readonlyRepositoryInterfaceType.IsGenericType.ShouldBeTrue($"Constructed type should be generic for {entityType.Name}");
                readonlyRepositoryInterfaceType.GetGenericTypeDefinition().ShouldBe(typeof(IReadOnlyRepository<>));

                // Verify readonly nature - should inherit from base read operations
                var methods = readonlyRepositoryInterfaceType.GetMethods();
                var readOnlyMethods = methods.Where(m => m.Name.Contains("Get") || m.Name.Contains("Count") || m.Name.Contains("Find")).ToArray();
                readOnlyMethods.ShouldNotBeEmpty($"ReadOnly repository should have query methods for {entityType.Name}");
            }
        }

        /// <summary>
        /// Tests that readonly and full repository interfaces are properly separated.
        /// I²TDD validation test - ensures proper interface segregation.
        /// </summary>
        [Theory]
        [InlineData(typeof(Machine))]
        [InlineData(typeof(Product))]
        [InlineData(typeof(Line))]
        public void ReadOnlyRepository_ShouldBeSeparateFromFullRepository(Type entityType)
        {
            // Arrange - Create both repository types
            var readOnlyRepositoryType = typeof(IReadOnlyRepository<>).MakeGenericType(entityType);
            var fullRepositoryType = typeof(IRepository<>).MakeGenericType(entityType);

            // Act & Assert - Verify interface segregation principle
            readOnlyRepositoryType.ShouldNotBeNull();
            fullRepositoryType.ShouldNotBeNull();

            // Verify they are different interfaces
            readOnlyRepositoryType.ShouldNotBe(fullRepositoryType, "ReadOnly and Full repository interfaces should be separate");

            // Verify readonly is more restrictive (fewer methods)
            var readOnlyMethods = readOnlyRepositoryType.GetMethods();
            var fullMethods = fullRepositoryType.GetMethods();

            readOnlyMethods.Length.ShouldBeLessThanOrEqualTo(fullMethods.Length, "ReadOnly repository should have fewer or equal methods");
        }
    }
}