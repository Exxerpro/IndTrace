// <copyright file="RepositoryAuditPreservationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.ValueObjects;

namespace IndTrace.Aggregation.BoundedTests.Repository;

/// <summary>
/// Aggregation tests (real EF Core InMemory) covering issue #117 F3: a full update via the repository
/// must NOT clobber the immutable creation audit. <c>EntityUpdateHelper.TryUpdateEntity</c> copies every
/// property from the detached source entity via <c>SetValues</c>; a detached instance typically carries
/// default <c>CreatedBy</c>/<c>CreatedOn</c>, and the audit stamper deliberately only advances the
/// modification audit on save — so without preservation the defaults overwrite the stored creation audit.
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="RepositoryAuditPreservationTests"/> class.</remarks>
public class RepositoryAuditPreservationTests(ITestOutputHelper outputHelper) : DependenciesFactory(outputHelper)
{
    // Composite-key space chosen high to avoid collision with MachinePlcRawData.Fixture and the
    // RepositoryP04FixTests 9100-range. InMemory does not enforce the FK to Machine/Plc.
    private const int BaseMachineId = 9300;

    /// <summary>
    /// Issue #117 F3: updating with a detached instance whose CreatedBy/CreatedOn are defaults must
    /// preserve the stored creation audit while the modification audit advances.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_DetachedEntityWithDefaultAudit_PreservesCreationAudit()
    {
        var ct = TestContext.Current.CancellationToken;
        const int machineId = BaseMachineId + 1;

        // Create — the persistence layer stamps the creation audit.
        (await DpMachinePlcRepository.AddAsync(new MachinePlc(machineId, 1, ActiveStatus.Active), ct)).IsSuccess.ShouldBeTrue();

        var created = await ReadMachinePlcAsync(machineId, 1, ct);
        created.ShouldNotBeNull();
        created!.CreatedOn.ShouldNotBeNull("the creation audit must be stamped on add");
        created.CreatedBy.ShouldNotBeNullOrEmpty("the creation audit must be stamped on add");
        var originalCreatedBy = created.CreatedBy;
        var originalCreatedOn = created.CreatedOn;

        // Full update with a DETACHED instance carrying default audit values (CreatedBy empty, CreatedOn null)
        // — exactly what a handler that maps a DTO onto a fresh entity produces.
        var detached = new MachinePlc(machineId, 1, ActiveStatus.Inactive);
        detached.CreatedBy.ShouldBeEmpty();
        detached.CreatedOn.ShouldBeNull();

        (await DpMachinePlcRepository.UpdateAsync(detached, ct)).IsSuccess.ShouldBeTrue();

        var updated = await ReadMachinePlcAsync(machineId, 1, ct);
        updated.ShouldNotBeNull();
        updated!.IsActive.ShouldBe(ActiveStatus.Inactive);                        // payload change applied
        updated.CreatedBy.ShouldBe(originalCreatedBy);                            // creation audit immutable
        updated.CreatedOn.ShouldBe(originalCreatedOn);                            // creation audit immutable
        updated.ModifiedOn.ShouldNotBeNull();                                     // modification audit advanced
        updated.ModifiedOn!.Value.ShouldBeGreaterThanOrEqualTo(originalCreatedOn!.Value);
        updated.ModifiedBy.ShouldNotBeNullOrEmpty();
    }

    private async Task<MachinePlc?> ReadMachinePlcAsync(int machineId, int plcId, CancellationToken ct)
    {
        var spec = new Specification<MachinePlc>(mp => mp.MachineId == new MachineId(machineId) && mp.PlcId == plcId);
        spec.ApplyNoTracking();
        var result = await DpMachinePlcRepository.ListAsync(spec, ct);
        return result.IsSuccess && result.Value is not null ? result.Value.FirstOrDefault() : null;
    }
}
