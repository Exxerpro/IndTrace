// <copyright file="DateTimeMachineTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Interfaces;
using IndTrace.Domain.Models;
using Microsoft.Extensions.Time.Testing;
using Shouldly;

namespace IndTrace.Domain.UnitTests;

/// <summary>
/// Tests for DateTimeMachine dependency injection functionality.
/// </summary>
public class DateTimeMachineTests
{
    [Fact]
    public void DateTimeMachine_Constructor_ShouldCreateInstanceWithDefaultTimeProvider()
    {
        // Act
        var dateTimeMachine = new DateTimeMachine();

        // Assert
        dateTimeMachine.ShouldNotBeNull();
        dateTimeMachine.Now.ShouldBeGreaterThan(DateTime.MinValue);
        dateTimeMachine.UtcNow.ShouldBeGreaterThan(DateTime.MinValue);
    }

    [Fact]
    public void DateTimeMachine_WithFakeTimeProvider_ShouldUseProvidedTime()
    {
        // Arrange
        var fakeTime = new DateTime(2024, 1, 15, 10, 30, 0);
        var fakeTimeProvider = new FakeTimeProvider(fakeTime);
        var dateTimeMachine = new DateTimeMachine(fakeTimeProvider);

        // Act
        var result = dateTimeMachine.Now;

        // Assert
        result.ShouldBe(fakeTime);
    }

    [Fact]
    public void TaskGatewayResponse_Constructor_ShouldUseDateTimeMachine()
    {
        // Arrange
        var fakeTime = new DateTime(2024, 1, 15, 10, 30, 0);
        var fakeTimeProvider = new FakeTimeProvider(fakeTime);
        var dateTimeMachine = new DateTimeMachine(fakeTimeProvider);

        // Act
        var response = new TaskGatewayResponse(dateTimeMachine);

        // Assert
        response.TimeStamp.ShouldBe(fakeTime);
    }

    [Fact]
    public void OeeRegister_CreateKpiOee_ShouldUseDateTimeMachine()
    {
        // Arrange
        var fakeTime = new DateTime(2024, 1, 15, 10, 30, 0);
        var fakeTimeProvider = new FakeTimeProvider(fakeTime);
        var dateTimeMachine = new DateTimeMachine(fakeTimeProvider);
        var register = new OeeRegister();

        // Act
        var kpiOee = OeeRegister.ToKpiOee(register, dateTimeMachine);

        // Assert
        kpiOee.TimeStamp.ShouldBe(fakeTime);
    }

    [Fact]
    public void DateTimeMachine_AdvanceTime_WithFakeTimeProvider_ShouldAdvanceTime()
    {
        // Arrange
        var fakeTime = new DateTime(2024, 1, 15, 10, 30, 0);
        var fakeTimeProvider = new FakeTimeProvider(fakeTime);
        var dateTimeMachine = new DateTimeMachine(fakeTimeProvider);
        var advanceBy = TimeSpan.FromHours(2);

        // Act
        var result = dateTimeMachine.AdvanceTime(advanceBy);
        var newTime = dateTimeMachine.Now;

        // Assert
        result.IsSuccess.ShouldBeTrue();
        newTime.ShouldBe(fakeTime.Add(advanceBy));
    }

    [Fact]
    public void DateTimeMachine_AdvanceTime_WithRealTimeProvider_ShouldReturnFailure()
    {
        // Arrange
        var dateTimeMachine = new DateTimeMachine();
        var advanceBy = TimeSpan.FromHours(2);

        // Act
        var result = dateTimeMachine.AdvanceTime(advanceBy);

        // Assert
        // This test con only be true if mode is release

        if (BuildConfig.IsDebug)
        {
            // Debug-specific logic
            result.IsSuccess.ShouldBeTrue();
        }
        else
        {
            result.IsFailure.ShouldBeTrue();

            result.Errors.ShouldContain("AdvanceTime not supported in production");
            // Release-specific logic
        }
    }
}

public static class BuildConfig
{
    public static bool IsDebug
    {
        get
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }
}