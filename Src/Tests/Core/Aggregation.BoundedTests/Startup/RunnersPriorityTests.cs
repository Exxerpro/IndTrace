// <copyright file="RunnersPriorityTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.Startup;

using System.ComponentModel;
using System.Diagnostics;
using IndTrace.Dependencies.Startup;

/// <summary>
/// The gateway raises its process priority at start-up. The OS may refuse (Linux without CAP_SYS_NICE fails with
/// "Permission denied"); that refusal must be logged and tolerated, not crash the gateway before it starts.
/// </summary>
public sealed class RunnersPriorityTests
{
    [Fact]
    public void EnsureProgramIsHighPriority_WhenAllowed_SetsHighAndReturnsTrue()
    {
        // Arrange
        ProcessPriorityClass? applied = null;

        // Act
        var raised = Runners.EnsureProgramIsHighPriority(XUnitLogger.CreateLogger<RunnersPriorityTests>(), p => applied = p);

        // Assert
        raised.ShouldBeTrue();
        applied.ShouldBe(ProcessPriorityClass.High);
    }

    [Fact]
    public void EnsureProgramIsHighPriority_WhenOsDenies_ReturnsFalseInsteadOfThrowing()
    {
        // Act — the exact failure seen on Linux: Win32Exception (13) Permission denied.
        var raised = Runners.EnsureProgramIsHighPriority(
            XUnitLogger.CreateLogger<RunnersPriorityTests>(),
            _ => throw new Win32Exception(13, "Permission denied"));

        // Assert
        raised.ShouldBeFalse();
    }
}
