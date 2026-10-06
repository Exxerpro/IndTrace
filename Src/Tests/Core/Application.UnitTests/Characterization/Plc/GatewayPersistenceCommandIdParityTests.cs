// <copyright file="GatewayPersistenceCommandIdParityTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Characterization.Plc;

/// <summary>
/// Story 32.C2 regression pin — the pipeline-level §7 wire parity the 32.C1 projection golden master could NOT catch.
/// <para>
/// Pre-split, <see cref="GatewayPersistenceBehavior{TRequest, TResponse}"/> mutated the SHARED wire object in place
/// (<c>value.CommandId = commandId</c>) BEFORE the Hub/PLC publish, so the published payload carried the
/// store-generated command id (not the producer's 0 on the create path). The 32.C2 split makes the wire DTO immutable
/// and stamps <c>CommandId</c> on a separate persisted ENTITY; the isolated 32.C1 projection master never exercises
/// the pipeline, so it could not observe the published DTO regressing back to the producer value.
/// </para>
/// <para>
/// These tests drive <see cref="GatewayPersistenceBehavior{TRequest, TResponse}.HandleAsync"/> end-to-end and PIN that
/// the RETURNED/published <see cref="TaskGatewayResponseDto.CommandId"/> equals the store-generated command id after
/// persistence — the byte the Hub broadcasts. They cover both the success and the failure-with-value branches
/// (the pre-split in-place mutation was not branch-specific), and the failure case additionally pins error preservation.
/// </para>
/// </summary>
public class GatewayPersistenceCommandIdParityTests
{
    /// <summary>The store-generated command identity EF assigns during <c>AddAsync</c> (simulated). Non-zero.</summary>
    private const int StoreCommandId = 4242;

    /// <summary>
    /// Success + value: after persistence, the published wire DTO carries the store-generated command id — the
    /// create path starts at producer <c>CommandId == 0</c> and must become <see cref="StoreCommandId"/> on the wire.
    /// FAILS pre-fix (published stays 0), PASSES with the HandleAsync re-projection.
    /// </summary>
    /// <returns>The asynchronous test operation.</returns>
    [Fact]
    public async Task HandleAsync_SuccessWithValue_ReprojectsStoreCommandIdOntoPublishedWireDto()
    {
        var (behavior, command) = BuildBehavior();

        // Producer DTO on the create path carries CommandId 0 (no store identity yet).
        var producer = new TaskGatewayResponseDto { CommandId = 0, MachineId = 100 };
        var producerResult = Result<TaskGatewayResponseDto>.Success(producer);
        RequestFunctionalHandlerDelegate<Result<TaskGatewayResponseDto>> next = () => Task.FromResult(producerResult);

        var published = await behavior.HandleAsync(command, next, TestContext.Current.CancellationToken);

        published.IsSuccess.ShouldBeTrue();
        var value = published.Value.ShouldNotBeNull();
        value.CommandId.ShouldBe(StoreCommandId);
    }

    /// <summary>
    /// Failure + value: the same re-projection applies on the failure branch (the pre-split mutation was not
    /// branch-specific), and the failure ERRORS are preserved (must use the value-keeping failure factory, not the
    /// value-dropping one). FAILS pre-fix, PASSES with the fix.
    /// </summary>
    /// <returns>The asynchronous test operation.</returns>
    [Fact]
    public async Task HandleAsync_FailureWithValue_ReprojectsCommandId_AndPreservesErrors()
    {
        var (behavior, command) = BuildBehavior();

        var producer = new TaskGatewayResponseDto { CommandId = 0, MachineId = 100 };
        var producerResult = Result<TaskGatewayResponseDto>.Failure(new[] { "PLC communication error" }, producer);
        RequestFunctionalHandlerDelegate<Result<TaskGatewayResponseDto>> next = () => Task.FromResult(producerResult);

        var published = await behavior.HandleAsync(command, next, TestContext.Current.CancellationToken);

        published.IsFailure.ShouldBeTrue();
        var value = published.Value.ShouldNotBeNull();
        value.CommandId.ShouldBe(StoreCommandId);
        published.Errors.ShouldContain("PLC communication error");
    }

    /// <summary>
    /// Builds a behavior whose request repository stamps the store-generated command id onto the command during
    /// <c>AddAsync</c> (mirroring the EF identity assignment the real repository performs), plus a no-op response repo.
    /// </summary>
    /// <returns>The behavior under test and the seeded command.</returns>
    private static (GatewayPersistenceBehavior<CreateBarCodeCommand, Result<TaskGatewayResponseDto>> Behavior, CreateBarCodeCommand Command) BuildBehavior()
    {
        var logger = XUnitLogger.CreateLogger<CreateBarCodeCommand>();
        var requestRepo = Substitute.For<IRepository<TaskGatewayRequest>>();
        var responseRepo = Substitute.For<IRepository<TaskGatewayResponse>>();

        var command = new CreateBarCodeCommand();
        command.Command.MachineId = 100;
        command.Command.BarCode = "TEST-BARCODE-PARITY";
        command.Command.GatewayTask = GatewayTask.CreateBarCodeAsync;

        // Simulate the store-generated identity EF assigns to the entity during AddAsync (create path: 0 -> store id).
        requestRepo.AddAsync(command.Command, Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                command.Command.CommandId = StoreCommandId;
                return Task.FromResult(Result<int>.Success(1));
            });

        var behavior = new GatewayPersistenceBehavior<CreateBarCodeCommand, Result<TaskGatewayResponseDto>>(
            logger, requestRepo, responseRepo);

        return (behavior, command);
    }
}
