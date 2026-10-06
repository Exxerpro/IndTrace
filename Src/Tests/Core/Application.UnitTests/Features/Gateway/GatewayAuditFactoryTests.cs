// <copyright file="GatewayAuditFactoryTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Gateway.Auditing;

namespace Application.UnitTests.Features.Gateway;

/// <summary>
/// Issue #123 (F3): <see cref="GatewayAuditFactory.CreateAuditEntryAsync"/> discarded the repository
/// <c>AddAsync</c> Result — a railway-style write failure still logged "Successfully created gateway audit
/// entry" and returned Success, so callers (which already gate on the factory's Result) could never see a
/// dropped audit row. These tests pin that a failed audit write now surfaces as a failure Result and that
/// the success log/Result only occur when the write actually landed.
/// </summary>
public class GatewayAuditFactoryTests
{
    /// <summary>
    /// The false-success pin: a railway failure from <c>AddAsync</c> must propagate as a failure Result
    /// carrying the repository errors — never Success(auditEntry).
    /// </summary>
    /// <returns>The asynchronous test operation.</returns>
    [Fact]
    public async Task CreateAuditEntryAsync_WhenAddAsyncFails_ShouldReturnFailureWithRepositoryErrors()
    {
        var repository = Substitute.For<IRepository<TaskGatewayRequest>>();
        repository.AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.WithFailure("Audit insert failed")));

        var factory = new GatewayAuditFactory(repository, XUnitLogger.CreateLogger<GatewayAuditFactory>());

        var result = await factory.CreateAuditEntryAsync(CreateRequest(), TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Audit insert failed");
    }

    /// <summary>
    /// The success path is unchanged: a landed write returns Success carrying the created audit entry.
    /// </summary>
    /// <returns>The asynchronous test operation.</returns>
    [Fact]
    public async Task CreateAuditEntryAsync_WhenAddAsyncSucceeds_ShouldReturnSuccessWithAuditEntry()
    {
        var repository = Substitute.For<IRepository<TaskGatewayRequest>>();
        repository.AddAsync(Arg.Any<TaskGatewayRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<int>.Success(1)));

        var factory = new GatewayAuditFactory(repository, XUnitLogger.CreateLogger<GatewayAuditFactory>());

        var result = await factory.CreateAuditEntryAsync(CreateRequest(), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        var entry = result.Value.ShouldNotBeNull();
        entry.MachineId.ShouldBe(100001);
        entry.BarCodeId.ShouldBe(42);
        entry.CycleId.ShouldBe(7);
    }

    /// <summary>
    /// Builds a representative audit request for the factory under test.
    /// </summary>
    /// <returns>A populated <see cref="GatewayAuditRequest"/>.</returns>
    private static GatewayAuditRequest CreateRequest()
    {
        return new GatewayAuditRequest(
            MachineId: 100001,
            BarCodeId: 42,
            CycleId: 7,
            CycleStatus: CycleStatus.Started,
            PartStatus: PartStatus.Ok,
            FlowStatus: FlowStatus.InProcess,
            ResultValidation: ResultValidation.Valid,
            GatewayTask: GatewayTask.CreateCycleAsync,
            TimeStamp: DateTimeOffset.UtcNow);
    }
}
