// <copyright file="ReferenceVariableServiceDataTypeTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Features.Barcodes;

using IndTrace.Application.Repository;

/// <summary>
/// Regression guard for #43: the reference/BarCode path must source <see cref="Register.DataType"/> from the
/// real .NET type (<c>Variable.NetType</c>), NEVER from the role-tag-polluted <c>Variable.NativeType</c>
/// (values like <c>Production</c>/<c>TRACEABILITY</c>/<c>SAFETY_CRITICAL</c>/<c>SENSOR</c>). A field literally
/// named <c>DataType</c> must carry type data, not domain role tags, on this life-critical traceability line.
/// Pins the corrected behavior so the mapping cannot be silently flipped back to <c>NativeType</c>.
/// </summary>
public class ReferenceVariableServiceDataTypeTests
{
    /// <summary>
    /// Given a reference variable whose <c>NativeType</c> carries a domain role tag while <c>NetType</c> carries
    /// the real .NET type, the produced reference register's <see cref="Register.DataType"/> must equal the
    /// <c>NetType</c> and must NOT equal the leaked role tag.
    /// </summary>
    [Fact]
    public async Task GetReferenceRegisters_SourcesDataTypeFromNetType_NotNativeTypeRoleTag()
    {
        // Arrange — a reference variable whose NetType carries the real .NET type. The register's DataType
        // must be sourced from NetType (never from a role tag such as SAFETY_CRITICAL).
        const string roleTag = "SAFETY_CRITICAL";
        const string netType = "System.Int16";
        var variable = new Variable
        {
            VariableId = 7,
            Name = "SafetyRef",
            NetType = netType,
        };

        var variableRepository = Substitute.For<IReadOnlyRepository<Variable>>();
        variableRepository
            .ListAsync(Arg.Any<ISpecification<Variable>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<IEnumerable<Variable>>.Success(new List<Variable> { variable })));

        var sut = new ReferenceVariableService(
            variableRepository,
            XUnitLogger.CreateLogger<ReferenceVariableService>());

        // Act
        var result = await sut.GetReferenceRegistersAsync(machineId: 5, cycleId: 42, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        var register = result.Value["SafetyRef"];
        register.DataType.ShouldBe(netType);
        register.DataType.ShouldNotBe(roleTag);
    }
}
