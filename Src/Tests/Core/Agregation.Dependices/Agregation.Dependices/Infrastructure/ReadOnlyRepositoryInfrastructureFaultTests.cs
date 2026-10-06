// <copyright file="ReadOnlyRepositoryInfrastructureFaultTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Data.Common;
using IndTrace.Domain.Diagnostics;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace IndTrace.Agregation.Dependices.Infrastructure;

/// <summary>
/// Issue #23: proves <see cref="ReadOnlyRepository{T}"/> classifies a database / infrastructure fault
/// (a <see cref="DbException"/> — the base of <c>Microsoft.Data.SqlClient.SqlException</c>, which is what a
/// missing audit column such as <c>CreatedBy</c> on a restored-from-old-backup schema raises) DISTINCTLY,
/// so it can never be mistaken for an ordinary "not found" / empty result.
/// </summary>
/// <remarks>
/// The read path is exercised with caching disabled (so it goes straight to the DB helper) and the context
/// factory substituted to throw. EF InMemory cannot raise a <see cref="SqlException"/> and
/// <c>SqlException</c> has no public constructor, so a <see cref="DbException"/> subclass is the testable seam.
/// </remarks>
public class ReadOnlyRepositoryInfrastructureFaultTests
{
    /// <summary>A concrete, throwable <see cref="DbException"/> standing in for a real SqlException.</summary>
    private sealed class FakeSchemaDbException : DbException
    {
        public FakeSchemaDbException(string message)
            : base(message)
        {
        }
    }

    private const string MissingColumnMessage = "Invalid column name 'CreatedBy'.";

    private static ReadOnlyRepository<Machine> BuildSut(Exception thrown)
    {
        var contextFactory = Substitute.For<IIndTraceDbContextFactory>();
        contextFactory
            .CreateDbContextAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(thrown);

        var cache = Substitute.For<ICacheService>();
        var logger = XUnitLogger.CreateLogger<ReadOnlyRepository<Machine>>();

        // Cache disabled => the read goes straight to the *FromDbAsync path, which calls the throwing factory.
        var toggle = Options.Create(new CacheToggleOptions { Enabled = false });

        return new ReadOnlyRepository<Machine>(contextFactory, cache, logger, string.Empty, toggle);
    }

    /// <summary>
    /// A <see cref="DbException"/> raised on <c>ListAsync</c> is returned as an INFRASTRUCTURE-marked failure
    /// carrying the real detail — distinct from a not-found miss.
    /// </summary>
    [Fact]
    public async Task ListAsync_WhenDbExceptionThrown_ReturnsDistinctInfrastructureFault()
    {
        var sut = BuildSut(new FakeSchemaDbException(MissingColumnMessage));

        var result = await sut.ListAsync(TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        InfrastructureFault.IsInfrastructureFault(result.Error).ShouldBeTrue();
        result.Error.ShouldContain(MissingColumnMessage);
    }

    /// <summary>
    /// A <see cref="DbException"/> raised on <c>FirstOrDefaultAsync</c> is likewise classified as an
    /// infrastructure fault, never a not-found miss.
    /// </summary>
    [Fact]
    public async Task FirstOrDefaultAsync_WhenDbExceptionThrown_ReturnsDistinctInfrastructureFault()
    {
        var sut = BuildSut(new FakeSchemaDbException(MissingColumnMessage));

        var result = await sut.FirstOrDefaultAsync(TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        InfrastructureFault.IsInfrastructureFault(result.Error).ShouldBeTrue();
    }

    /// <summary>
    /// A genuinely unexpected NON-database exception is NOT flagged as an infrastructure fault — the
    /// classification distinguishes the two categories rather than tarring every failure with the marker.
    /// </summary>
    [Fact]
    public async Task ListAsync_WhenNonDbExceptionThrown_IsNotClassifiedAsInfrastructureFault()
    {
        var sut = BuildSut(new InvalidOperationException("boom"));

        var result = await sut.ListAsync(TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        InfrastructureFault.IsInfrastructureFault(result.Error).ShouldBeFalse();
        result.Error.ShouldBe("boom");
    }
}
