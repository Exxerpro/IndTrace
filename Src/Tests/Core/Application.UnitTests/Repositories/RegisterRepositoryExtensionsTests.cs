// <copyright file="RegisterRepositoryExtensionsTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Repositories;

/// <summary>
/// Unit tests for RegisterRepositoryExtensions static extension methods
/// </summary>
public class RegisterRepositoryExtensionsTests
{
    private readonly IReadOnlyRepository<Register> _registerRepository;
    private readonly IRepository<Variable> _variableRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterRepositoryExtensionsTests"/> class.
    /// </summary>
    public RegisterRepositoryExtensionsTests()
    {
        _registerRepository = Substitute.For<IReadOnlyRepository<Register>>();
        _variableRepository = Substitute.For<IRepository<Variable>>();
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
        var response = await _registerRepository.GetRegistersGroupedByMachineAsync(_variableRepository, cycleIdList, TestContext.Current.CancellationToken);

        // Assert
        response.IsSuccess.ShouldBeTrue();
        response.Value.ShouldNotBeNull();
        response.Value.Count.ShouldBe(2);
        response.Value.ShouldContain(registers[0]);
        response.Value.ShouldContain(registers[1]);
    }

    /// <summary>
    /// Tests GetRegistersGroupedByMachineAsync returns failure when no active variables exist
    /// </summary>
    [Fact]
    public async Task GetRegistersGroupedByMachineAsync_WhenNoActiveVariables_ShouldReturnFailure()
    {
        // Arrange
        var cycleIdList = new List<int> { 1, 2, 3 };
        _variableRepository.ListAsync(Arg.Any<Specification<Variable>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<Variable>>.Success(new List<Variable>()));

        // Act
        var response = await _registerRepository.GetRegistersGroupedByMachineAsync(_variableRepository, cycleIdList, TestContext.Current.CancellationToken);

        // Assert
        response.IsFailure.ShouldBeTrue();
        response.Errors.ShouldContain("No active variables found");
    }
}
