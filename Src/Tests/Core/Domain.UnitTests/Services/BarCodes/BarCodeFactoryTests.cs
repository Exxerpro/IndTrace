// <copyright file="BarCodeFactoryTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Interfaces;
using IndTrace.Domain.Services.BarCodes;
using NSubstitute;
using Shouldly;
using Xunit;

namespace IndTrace.Domain.UnitTests.Services.BarCodes;

/// <summary>
/// Unit tests for <see cref="BarCodeFactory"/>.
/// Issue #88: the factory returns a <c>Result</c> failure for missing inputs instead of throwing across the boundary.
/// </summary>
public class BarCodeFactoryTests
{
    private readonly BarCodeFactory _factory = new();

    private static IDateTimeMachine CreateDateTimeMachine()
    {
        var dtm = Substitute.For<IDateTimeMachine>();
        dtm.UtcNow.Returns(new DateTime(2026, 7, 7, 12, 0, 0, DateTimeKind.Utc));
        return dtm;
    }

    [Fact]
    public void CreateBarCode_ValidInput_ShouldReturnSuccessWithBarCode()
    {
        // Arrange
        var dtm = CreateDateTimeMachine();

        // Act
        var result = _factory.CreateBarCode("LABEL-001", productId: 5, machineId: 7, dtm);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Label.Value.ShouldBe("LABEL-001");
        result.Value.MachineId.Value.ShouldBe(7);
    }

    [Fact]
    public void CreateBarCode_NullLabel_ShouldReturnFailureNotThrow()
    {
        // Arrange
        var dtm = CreateDateTimeMachine();

        // Act
        var result = _factory.CreateBarCode(null!, productId: 5, machineId: 7, dtm);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("label cannot be null.");
    }

    [Fact]
    public void CreateBarCode_NullDateTimeMachine_ShouldReturnFailureNotThrow()
    {
        // Act
        var result = _factory.CreateBarCode("LABEL-001", productId: 5, machineId: 7, null!);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("dateTimeMachine cannot be null.");
    }
}
