// <copyright file="CycleUpdateHandlerRegistrationTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Cycles.Commands.UpdateCyclesNok;
using IndTrace.Application.Cycles.Commands.UpdateCyclesOk;
using IndTrace.Application.StateMachine;

namespace Application.UnitTests.Commands;

/// <summary>
/// DI-resolution + dispatch-scoping guard for the PLC gateway host wiring of the unified cycle-update handler
/// (Story 6.5, blocker B1).
///
/// Background: <see cref="GatewayCommandDispatcher"/> is a SINGLETON shared by every PLC worker. It resolves
/// <c>IGatewayRequestHandler&lt;UpdateCyclesOkCommand, TaskGatewayResponseDto&gt;</c> /
/// <c>IGatewayRequestHandler&lt;UpdateCyclesNotOkCommand, TaskGatewayResponseDto&gt;</c> on the first PLC
/// UpdateCycleOk / UpdateCycleNotOk. Those interfaces are registered <c>AddScoped</c> inside the
/// <c>useRefactoredHandlers</c> branch of <see cref="CyclesCompositionRoot.AddCycleServices"/>. Before this
/// story the dispatcher resolved handlers from the ROOT provider, so a singleton resolving a scoped service
/// threw <see cref="InvalidOperationException"/> ("Cannot resolve scoped service ... from root provider") under
/// <c>ValidateScopes</c> (the Development default). Story 6.5 (Task 1) makes the dispatcher open a fresh DI
/// scope PER DISPATCH and resolve the handler from it, closing B1.
///
/// These tests lock the contract from both sides: (1) the production registration graph is resolvable and
/// scoped, and (2) the singleton dispatcher resolves+runs a scoped cycle-update handler under
/// <c>ValidateScopes</c>/<c>ValidateOnBuild</c> without throwing.
/// </summary>
public class CycleUpdateHandlerRegistrationTests
{
    /// <summary>
    /// Builds the companion registrations the unified handler graph needs beyond <c>AddCycleServices</c>:
    /// the leaf dependencies the SRP services and strategies require (repositories, shift service, clock,
    /// barcode result) plus logging. Mirrors what production registers around <c>AddCycleServices</c>.
    /// </summary>
    private static IServiceCollection BuildCycleHostServices(bool useRefactoredHandlers)
    {
        var services = new ServiceCollection();

        // Logging - registers ILoggerFactory + the open-generic ILogger<> the SRP graph constructor-injects.
        services.AddLogging();

        // Leaf dependencies of the unified handler + strategies (mocked at the boundary like sibling tests).
        services.AddSingleton(Substitute.For<IBarCodeResult>());

        // Issue #33 (Chunk 3): BarCodeInfoProvider now depends on the stateless loader, not the god-object.
        services.AddSingleton(Substitute.For<IBarCodeDetailsLoader>());
        services.AddSingleton(Substitute.For<IShiftService>());
        services.AddSingleton(Substitute.For<IDateTimeMachine>());
        services.AddSingleton(Substitute.For<IRepository<TaskGatewayRequest>>());
        services.AddSingleton(Substitute.For<IAppendOnlyRepository<Register>>());
        services.AddSingleton(Substitute.For<IReadOnlyRepository<Cycle>>());
        services.AddSingleton(Substitute.For<IRepository<BarCode>>());

        // The system under test: the exact call the PLC gateway host makes.
        services.AddCycleServices(useRefactoredHandlers: useRefactoredHandlers);

        return services;
    }

    /// <summary>
    /// Proves the production registration graph: with <c>AddCycleServices(useRefactoredHandlers: true)</c>, both
    /// cycle-update gateway handlers resolve without throwing, from the root provider AND from a created scope
    /// (the unified handler and its strategies are Scoped, so scoped resolution surfaces lifetime mismatches).
    /// </summary>
    [Fact]
    [Trait("Category", "DI_Validation")]
    [Trait("Priority", "Critical")]
    public void AddCycleServices_WithRefactoredHandlersTrue_ResolvesBothCycleUpdateHandlers()
    {
        // Arrange
        var services = BuildCycleHostServices(useRefactoredHandlers: true);
        using var provider = services.BuildServiceProvider();

        // Act & Assert - root provider resolution
        Should.NotThrow(() =>
            provider.GetRequiredService<IGatewayRequestHandler<UpdateCyclesOkCommand, TaskGatewayResponseDto>>());
        Should.NotThrow(() =>
            provider.GetRequiredService<IGatewayRequestHandler<UpdateCyclesNotOkCommand, TaskGatewayResponseDto>>());

        // Act & Assert - scoped resolution (Scoped handler/strategies must resolve inside a scope)
        using var scope = provider.CreateScope();
        var okHandler = scope.ServiceProvider
            .GetRequiredService<IGatewayRequestHandler<UpdateCyclesOkCommand, TaskGatewayResponseDto>>();
        var notOkHandler = scope.ServiceProvider
            .GetRequiredService<IGatewayRequestHandler<UpdateCyclesNotOkCommand, TaskGatewayResponseDto>>();

        okHandler.ShouldNotBeNull();
        notOkHandler.ShouldNotBeNull();

        // Both interfaces are served by the single unified UpdateCyclesCommandHandler instance within a scope.
        okHandler.ShouldBeSameAs(notOkHandler);
    }

    /// <summary>
    /// Proves the guard genuinely covers the gap: with <c>AddCycleServices(useRefactoredHandlers: false)</c>
    /// (the legacy default the PLC host uses today) the OK cycle-update gateway handler is unregistered, so
    /// <see cref="ServiceProviderServiceExtensions.GetRequiredService"/> throws - exactly the failure
    /// <see cref="GatewayCommandDispatcher"/> would hit on the first PLC UpdateCycleOk.
    /// </summary>
    [Fact]
    [Trait("Category", "DI_Validation")]
    [Trait("Priority", "Critical")]
    public void AddCycleServices_WithRefactoredHandlersFalse_DoesNotRegisterCycleUpdateHandler()
    {
        // Arrange
        var services = BuildCycleHostServices(useRefactoredHandlers: false);
        using var provider = services.BuildServiceProvider();

        // Act & Assert
        Should.Throw<InvalidOperationException>(() =>
            provider.GetRequiredService<IGatewayRequestHandler<UpdateCyclesOkCommand, TaskGatewayResponseDto>>());
    }

    /// <summary>
    /// AC1 (B1 fix) + AC7 (path stamping). Reproduces production wiring: a SINGLETON
    /// <see cref="GatewayCommandDispatcher"/> built from a root provider that has
    /// <c>ValidateScopes</c>/<c>ValidateOnBuild</c> turned on (the Development default that surfaced B1),
    /// dispatching a cycle-update command whose handler is registered <c>AddScoped</c> (as the refactored
    /// branch does). With the Task-1 per-dispatch scope the dispatch resolves and runs WITHOUT throwing a
    /// "scoped service from root provider" exception - if the dispatcher reverted to root-provider resolution,
    /// this test would throw under <c>ValidateScopes</c>. A scoped fake handler is used so the test isolates the
    /// DI-scoping mechanism (which is all B1 is about) from the real handler's internals. It also asserts the
    /// Story-3.5 PLC path stamp flows INTO the per-dispatch scope (AC7).
    /// </summary>
    [Fact]
    [Trait("Category", "DI_Validation")]
    [Trait("Priority", "Critical")]
    public async Task SingletonDispatcher_UnderValidateScopes_ResolvesAndRunsScopedCycleUpdateHandler()
    {
        // Arrange - production-shaped graph: singleton dispatcher, scoped cycle-update handler, singleton path.
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<ITransitionPathContext, TransitionPathContext>();
        var pathProbe = new PathProbe();
        services.AddSingleton(pathProbe);
        services.AddScoped<IGatewayRequestHandler<UpdateCyclesOkCommand, TaskGatewayResponseDto>, PathCapturingScopedHandler>();
        services.AddSingleton<IGatewayCommandDispatcher, GatewayCommandDispatcher>();

        // Root provider with the validation that made B1 throw before this story.
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true,
        });

        // The SINGLETON dispatcher, resolved from the root - exactly as the PLC host holds it.
        var dispatcher = provider.GetRequiredService<IGatewayCommandDispatcher>();
        var command = new UpdateCyclesOkCommand();

        // Act - the dispatch must not throw a "scoped service from root provider" exception.
        Result<TaskGatewayResponseDto>? result = null;
        await Should.NotThrowAsync(async () =>
            result = await dispatcher.ProcessAsync(command, TestContext.Current.CancellationToken));

        // Assert - the scoped handler ran (resolved from the per-dispatch scope, not the root provider) ...
        var dispatched = result.ShouldNotBeNull();
        dispatched.IsSuccess.ShouldBeTrue();

        // ... and the Story-3.5 PLC path stamp set before the scope flowed into the scoped handler (AC7).
        pathProbe.ObservedPath.ShouldBe(TransitionPath.Plc);
    }

    /// <summary>Singleton sink the scoped handler writes the path it observed at execution time into.</summary>
    private sealed class PathProbe
    {
        public TransitionPath ObservedPath { get; set; } = TransitionPath.Unknown;
    }

    /// <summary>
    /// Scoped fake cycle-update handler. Registered <c>AddScoped</c> so resolving it from the root provider
    /// under <c>ValidateScopes</c> would throw - which is precisely what the per-dispatch scope avoids. Reads
    /// the ambient <see cref="ITransitionPathContext.Current"/> at execution time to prove the PLC stamp the
    /// dispatcher set before the scope flowed into the scope's async context.
    /// </summary>
    private sealed class PathCapturingScopedHandler : IGatewayRequestHandler<UpdateCyclesOkCommand, TaskGatewayResponseDto>
    {
        private readonly ITransitionPathContext pathContext;
        private readonly PathProbe probe;

        public PathCapturingScopedHandler(ITransitionPathContext pathContext, PathProbe probe)
        {
            this.pathContext = pathContext;
            this.probe = probe;
        }

        public Task<Result<TaskGatewayResponseDto>> ProcessAsync(UpdateCyclesOkCommand request, CancellationToken cancellationToken)
        {
            this.probe.ObservedPath = this.pathContext.Current;
            return Task.FromResult(Result<TaskGatewayResponseDto>.Success(new TaskGatewayResponseDto()));
        }
    }
}
