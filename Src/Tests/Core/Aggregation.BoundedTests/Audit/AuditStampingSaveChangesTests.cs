// <copyright file="AuditStampingSaveChangesTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.Audit;

using System;
using System.Threading.Tasks;
using IndTrace.Aggregation.BoundedTests.Services;
using IndTrace.Domain.Entities;
using IndTrace.Domain.Models;
using IndTrace.Persistence.DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;

/// <summary>
/// #29 (F6) — regression guard for the audit-stamping performed by
/// <see cref="IndTraceDbContext.SaveChangesAsync"/>. The persistence layer is the single source of audit
/// timestamps (the entity no longer self-stamps in its constructor). These tests pin the CORRECT contract:
/// an insert stamps <c>CreatedOn</c>/<c>CreatedBy</c>/<c>ModifiedOn</c>; a subsequent update advances
/// <c>ModifiedOn</c>/<c>ModifiedBy</c> and — critically — leaves the original <c>CreatedOn</c> untouched.
/// Before the fix the Modified branch wrote <c>CreatedOn</c> (not <c>ModifiedOn</c>), so every update clobbered
/// the creation timestamp and never advanced the modification timestamp — a life-critical audit-trail defect.
/// </summary>
public sealed class AuditStampingSaveChangesTests
{
    private static IndTraceDbContext NewContext(DateTimeMachine clock)
    {
        var options = new DbContextOptionsBuilder<IndTraceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new IndTraceDbContext(options);
        context.SetTestingInterfaces(new TesterUserService(), clock);
        return context;
    }

    /// <summary>
    /// On insert the persistence layer stamps CreatedBy/CreatedOn and an equal ModifiedOn.
    /// </summary>
    [Fact]
    public async Task Insert_StampsCreatedAndModifiedFromPersistenceClock()
    {
        // Arrange
        var start = new DateTimeOffset(2026, 7, 3, 8, 0, 0, TimeSpan.Zero);
        var clock = new DateTimeMachine(new FakeTimeProvider(start));
        await using var context = NewContext(clock);
        await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

        var customer = new Customer { Name = "Acme" };

        // Act
        context.Add(customer);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        customer.CreatedBy.ShouldBe("Admin");
        customer.CreatedOn.ShouldNotBeNull();
        customer.ModifiedOn.ShouldNotBeNull();
        customer.ModifiedOn.ShouldBe(customer.CreatedOn); // created == modified at birth
    }

    /// <summary>
    /// On update the persistence layer advances ModifiedOn/ModifiedBy and PRESERVES the original CreatedOn.
    /// This is the guard against the historic "every update clobbers CreatedOn / never sets ModifiedOn" defect.
    /// </summary>
    [Fact]
    public async Task Update_AdvancesModifiedOn_AndPreservesCreatedOn()
    {
        // Arrange
        var start = new DateTimeOffset(2026, 7, 3, 8, 0, 0, TimeSpan.Zero);
        var fake = new FakeTimeProvider(start);
        var clock = new DateTimeMachine(fake);
        await using var context = NewContext(clock);
        await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

        var customer = new Customer { Name = "Acme" };
        context.Add(customer);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var createdOnAtInsert = customer.CreatedOn;
        createdOnAtInsert.ShouldNotBeNull();

        // Act — advance the clock an hour, then modify and persist.
        fake.Advance(TimeSpan.FromHours(1));
        customer.Name = "Acme Renamed";
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert — CreatedOn is immutable across the update; ModifiedOn advanced by exactly the elapsed hour.
        customer.CreatedOn.ShouldBe(createdOnAtInsert);
        customer.ModifiedOn.ShouldNotBeNull();
        customer.ModifiedOn.Value.ShouldBe(createdOnAtInsert.Value.AddHours(1));

        // #52: ModifiedBy must be stamped from CurrentUserName ("Admin"), NOT CurrentUserId ("user-id-0001").
        // With the tester service now returning DISTINCT id vs name, this assertion fails if the Modified
        // branch reverts to stamping the opaque id — pinning that CreatedBy and ModifiedBy share one kind.
        customer.ModifiedBy.ShouldBe("Admin");
        customer.ModifiedBy.ShouldBe(customer.CreatedBy);
    }
}
