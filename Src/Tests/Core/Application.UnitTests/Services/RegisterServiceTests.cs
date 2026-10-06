// <copyright file="RegisterServiceTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.Services;

namespace Application.UnitTests.Services
{
    /// <summary>
    /// Unit tests for RegisterService implementation
    /// </summary>
    public class RegisterServiceTests
    {
        private readonly IReadOnlyRepository<Register> _registerRepository = null!;
        private readonly IRepository<Variable> _variableRepository = null!;
        private readonly RegisterService _service = null!;

        public RegisterServiceTests()
        {
            _registerRepository = Substitute.For<IReadOnlyRepository<Register>>();
            _variableRepository = Substitute.For<IRepository<Variable>>();
            _service = new RegisterService(_registerRepository, _variableRepository);
        }

        /// <summary>
        /// Tests that Constructor creates instance with valid dependencies
        /// </summary>
        [Fact]
        public void Constructor_WithValidDependencies_ShouldCreateInstance()
        {
            // Arrange & Act
            var service = new RegisterService(_registerRepository, _variableRepository);

            // Assert
            service.ShouldNotBeNull();
        }

        /// <summary>
        /// Tests that Constructor throws ArgumentNullException with null registerRepository
        /// </summary>
        [Fact]
        public void Constructor_WithNullRegisterRepository_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Should.Throw<ArgumentNullException>(() =>
                new RegisterService(null!, _variableRepository));
        }

        /// <summary>
        /// Tests that Constructor throws ArgumentNullException with null variableRepository
        /// </summary>
        [Fact]
        public void Constructor_WithNullVariableRepository_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Should.Throw<ArgumentNullException>(() =>
                new RegisterService(_registerRepository, null!));
        }

        /// <summary>
        /// Tests GetRegistersGroupedByMachineAsync returns failure when no active variables
        /// </summary>
        [Fact]
        public async Task GetRegistersGroupedByMachineAsync_WhenNoActiveVariables_ShouldReturnFailure()
        {
            // Arrange
            var cycleIdList = new List<int> { 1, 2, 3 };
            _variableRepository.ListAsync(Arg.Any<Specification<Variable>>(), Arg.Any<CancellationToken>())
                .Returns(Result<IEnumerable<Variable>>.Success(new List<Variable>()));

            // Act
            var response = await _service.GetRegistersGroupedByMachineAsync(cycleIdList, TestContext.Current.CancellationToken);

            // Assert
            response.IsFailure.ShouldBeTrue();
            response.Errors.ShouldContain("No active variables found");
        }

        /// <summary>
        /// Tests GetRegistersGroupedByMachineAsync returns success with the fetched registers when valid data exists (issue #110)
        /// </summary>
        [Fact]
        public async Task GetRegistersGroupedByMachineAsync_WithValidData_ShouldReturnSuccess()
        {
            // Arrange
            var cycleIdList = new List<int> { 1, 2 };
            var variables = new List<Variable>
            {
                new() { VariableId = 1, IsActive = 1, MachineId = 10000, Description = "Test Var 1" },
                new() { VariableId = 2, IsActive = 1, MachineId = 10001, Description = "Test Var 2" }
            };
            var registers = new List<Register>
            {
                Register.CreateFixture(registerId: 1, cycleId: 1, variableId: 1, name: "Reg 1", value: "10.5", dataType: "REAL", statusValueId: 1, timeStamp: DateTime.UtcNow),
                Register.CreateFixture(registerId: 2, cycleId: 2, variableId: 2, name: "Reg 2", value: "20.5", dataType: "REAL", statusValueId: 1, timeStamp: DateTime.UtcNow)
            };

            _variableRepository.ListAsync(Arg.Any<Specification<Variable>>(), Arg.Any<CancellationToken>())
                .Returns(Result<IEnumerable<Variable>>.Success(variables));
            _registerRepository.ListAsync(Arg.Any<Specification<Register>>(), Arg.Any<CancellationToken>())
                .Returns(Result<IEnumerable<Register>>.Success(registers));

            // Act
            var response = await _service.GetRegistersGroupedByMachineAsync(cycleIdList, TestContext.Current.CancellationToken);

            // Assert
            response.IsSuccess.ShouldBeTrue();
            response.Value.ShouldNotBeNull();
            response.Value.Count.ShouldBe(2);
            response.Value.ShouldContain(registers[0]);
            response.Value.ShouldContain(registers[1]);
        }

        /// <summary>
        /// Tests GetRegistersWithVariablesAsync returns success with valid data
        /// </summary>
        [Fact]
        public async Task GetRegistersWithVariablesAsync_WithValidData_ShouldReturnSuccess()
        {
            // Arrange
            var cycleIdList = new List<int> { 1, 2 };
            var variables = new List<Variable>
            {
                new() { VariableId = 1, IsActive = 1, MachineId = 10000, Description = "Test Var 1" },
                new() { VariableId = 2, IsActive = 1, MachineId = 10001, Description = "Test Var 2" }
            };
            var registers = new List<Register>
            {
                Register.CreateFixture(registerId: 1, cycleId: 1, variableId: 1, name: "Reg 1", value: "10.5", dataType: "REAL", statusValueId: 1, timeStamp: DateTime.UtcNow),
                Register.CreateFixture(registerId: 2, cycleId: 2, variableId: 2, name: "Reg 2", value: "20.5", dataType: "REAL", statusValueId: 1, timeStamp: DateTime.UtcNow)
            };

            _variableRepository.ListAsync(Arg.Any<Specification<Variable>>(), Arg.Any<CancellationToken>())
                .Returns(Result<IEnumerable<Variable>>.Success(variables));
            _registerRepository.ListAsync(Arg.Any<Specification<Register>>(), Arg.Any<CancellationToken>())
                .Returns(Result<IEnumerable<Register>>.Success(registers));

            // Act
            var response = await _service.GetRegistersWithVariablesAsync(cycleIdList, TestContext.Current.CancellationToken);

            // Assert
            response.IsSuccess.ShouldBeTrue();
            response.Value.ShouldNotBeEmpty();
            response.Value.ShouldNotBeNull();
            response.Value.Count.ShouldBe(2);
        }

        /// <summary>
        /// Tests GetRegistersWithVariablesAsync returns failure when no registers found
        /// </summary>
        [Fact]
        public async Task GetRegistersWithVariablesAsync_WhenNoRegistersFound_ShouldReturnFailure()
        {
            // Arrange
            var cycleIdList = new List<int> { 1, 2 };
            var variables = new List<Variable> { new() { VariableId = 1, IsActive = 1 } };

            _variableRepository.ListAsync(Arg.Any<Specification<Variable>>(), Arg.Any<CancellationToken>())
                .Returns(Result<IEnumerable<Variable>>.Success(variables));
            _registerRepository.ListAsync(Arg.Any<Specification<Register>>(), Arg.Any<CancellationToken>())
                .Returns(Result<IEnumerable<Register>>.Success(new List<Register>()));

            // Act
            var response = await _service.GetRegistersWithVariablesAsync(cycleIdList, TestContext.Current.CancellationToken);

            // Assert
            response.IsFailure.ShouldBeTrue();
            response.Errors.ShouldContain("No registers found for the specified Cycle IDs");
        }

        /// <summary>
        /// Tests GetRegisterByCycleIdListAsync returns success with valid data
        /// </summary>
        [Fact]
        public async Task GetRegisterByCycleIdListAsync_WithValidData_ShouldReturnSuccess()
        {
            // Arrange
            var cycleIdList = new List<int> { 1, 2 };
            var variables = new List<Variable>
            {
                new() { VariableId = 1, IsActive = 1, Description = "Test Variable 1" },
                new() { VariableId = 2, IsActive = 1, Description = "Test Variable 2" }
            };
            var registers = new List<Register>
            {
                Register.CreateFixture(registerId: 1, cycleId: 1, variableId: 1, machineId: 10000, name: "Register 1", value: "Value1", dataType: "STRING", statusValueId: 1, timeStamp: DateTime.UtcNow),
                Register.CreateFixture(registerId: 2, cycleId: 2, variableId: 2, machineId: 10001, name: "Register 2", value: "Value2", dataType: "STRING", statusValueId: 1, timeStamp: DateTime.UtcNow)
            };

            _variableRepository.ListAsync(Arg.Any<Specification<Variable>>(), Arg.Any<CancellationToken>())
                .Returns(Result<IEnumerable<Variable>>.Success(variables));
            _registerRepository.ListAsync(Arg.Any<Specification<Register>>(), Arg.Any<CancellationToken>())
                .Returns(Result<IEnumerable<Register>>.Success(registers));

            // Act
            var response = await _service.GetRegisterByCycleIdListAsync(cycleIdList, TestContext.Current.CancellationToken);

            // Assert
            response.IsSuccess.ShouldBeTrue();
            response.Value.ShouldNotBeEmpty();
            response.Value.ShouldNotBeNull();
            response.Value.Count.ShouldBe(2);

            // Verify grouping by (MachineId, EntitieId)
            var firstView = response.Value.First();
            firstView.MachineId.ShouldBe(10000);
            firstView.VariableId.ShouldBe(1);
            firstView.Description.ShouldBe("Test Variable 1");
        }

        /// <summary>
        /// Tests GetRegisterByCycleIdListAsync returns failure when no active variables found
        /// </summary>
        [Fact]
        public async Task GetRegisterByCycleIdListAsync_WhenNoActiveVariables_ShouldReturnFailure()
        {
            // Arrange
            var cycleIdList = new List<int> { 1, 2 };
            _variableRepository.ListAsync(Arg.Any<Specification<Variable>>(), Arg.Any<CancellationToken>())
                .Returns(Result<IEnumerable<Variable>>.Success(new List<Variable>()));

            // Act
            var response = await _service.GetRegisterByCycleIdListAsync(cycleIdList, TestContext.Current.CancellationToken);

            // Assert
            response.IsFailure.ShouldBeTrue();
            response.Errors.ShouldContain("No active variables found.");
        }

        /// <summary>
        /// Tests GetRegisterByCycleIdListAsync handles inactive variables correctly
        /// </summary>
        [Fact]
        public async Task GetRegisterByCycleIdListAsync_WithInactiveVariables_ShouldSkipInactiveVariables()
        {
            // Arrange
            var cycleIdList = new List<int> { 1, 2 };
            var variables = new List<Variable>
            {
                new() { VariableId = 1, IsActive = 1, Description = "Active Variable" },
                new() { VariableId = 2, IsActive = 0, Description = "Inactive Variable" } // This should be skipped
            };
            var registers = new List<Register>
            {
                Register.CreateFixture(registerId: 1, cycleId: 1, variableId: 1, machineId: 10000, name: "Register 1", value: "Value1", dataType: "STRING", statusValueId: 1, timeStamp: DateTime.UtcNow),
                Register.CreateFixture(registerId: 2, cycleId: 2, variableId: 2, machineId: 10001, name: "Register 2", value: "Value2", dataType: "STRING", statusValueId: 1, timeStamp: DateTime.UtcNow)
            };

            _variableRepository.ListAsync(Arg.Any<Specification<Variable>>(), Arg.Any<CancellationToken>())
                .Returns(Result<IEnumerable<Variable>>.Success(variables));
            _registerRepository.ListAsync(Arg.Any<Specification<Register>>(), Arg.Any<CancellationToken>())
                .Returns(Result<IEnumerable<Register>>.Success(registers));

            // Act
            var response = await _service.GetRegisterByCycleIdListAsync(cycleIdList, TestContext.Current.CancellationToken);

            // Assert
            response.IsSuccess.ShouldBeTrue();
            response.Value.ShouldNotBeNull();
            response.Value.Count.ShouldBe(1); // Only the register with active variable should be included
            var firstItem = response.Value.First();
            firstItem.VariableId.ShouldBe(1);
        }

        /// <summary>
        /// Tests that all methods call repositories with proper specifications
        /// </summary>
        [Fact]
        public async Task ServiceMethods_ShouldCallRepositoriesWithProperSpecifications()
        {
            // Arrange
            var cycleIdList = new List<int> { 1, 2, 3 };
            _variableRepository.ListAsync(Arg.Any<Specification<Variable>>(), Arg.Any<CancellationToken>())
                .Returns(Result<IEnumerable<Variable>>.Success(new List<Variable>()));
            _registerRepository.ListAsync(Arg.Any<Specification<Register>>(), Arg.Any<CancellationToken>())
                .Returns(Result<IEnumerable<Register>>.Success(new List<Register>()));

            // Act - Call all methods to verify they use specifications
            await _service.GetRegistersGroupedByMachineAsync(cycleIdList, TestContext.Current.CancellationToken);
            await _service.GetRegistersWithVariablesAsync(cycleIdList, TestContext.Current.CancellationToken);
            await _service.GetRegisterByCycleIdListAsync(cycleIdList, TestContext.Current.CancellationToken);

            // Assert - Verify repository calls were made with specifications
            await _variableRepository.Received(3).ListAsync(Arg.Any<Specification<Variable>>(), Arg.Any<CancellationToken>());
            // Note: registerRepository calls may vary based on variable results, but at least GetRegistersGroupedByMachineAsync should call it
        }
    }
}