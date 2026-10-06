// <copyright file="UserOfflineCreationServiceSecurityTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.UserService;

namespace Application.UnitTests.Features.UserService;

/// <summary>
/// Regression tests for the #127 hardening of <see cref="UserOfflineCreationService"/>:
/// single-use consumption, 15-minute TTL, and bounded thread-safe generation.
/// Time is driven deterministically via <see cref="FakeTimeProvider"/> through the injected
/// <see cref="DateTimeMachine"/> clock — never the wall clock.
/// </summary>
public class UserOfflineCreationServiceSecurityTests
{
    private static readonly DateTimeOffset Anchor = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// A response validates exactly once; the second attempt fails because the challenge is consumed.
    /// </summary>
    [Fact]
    public void ValidateResponse_ByResponse_IsSingleUse()
    {
        // Arrange
        var service = new UserOfflineCreationService(new DateTimeMachine(new FakeTimeProvider(Anchor)));
        var challenge = service.GenerateChallenge();
        var response = service.LastGeneratedResponse[challenge];

        // Act & Assert
        service.ValidateResponse(response).ShouldBeTrue();  // first use consumes the challenge
        service.ValidateResponse(response).ShouldBeFalse(); // single-use: no replay
        service.LastGeneratedChallenge.ShouldNotContain(challenge);
    }

    /// <summary>
    /// A challenge older than the 15-minute TTL can no longer validate and is pruned from the store.
    /// </summary>
    [Fact]
    public void ValidateResponse_ByResponse_FailsAfterTtlExpiry()
    {
        // Arrange
        var fakeTime = new FakeTimeProvider(Anchor);
        var service = new UserOfflineCreationService(new DateTimeMachine(fakeTime));
        var challenge = service.GenerateChallenge();
        var response = service.LastGeneratedResponse[challenge];

        // Act — advance past the 15-minute TTL
        fakeTime.Advance(TimeSpan.FromMinutes(16));

        // Assert
        service.ValidateResponse(response).ShouldBeFalse();
        service.LastGeneratedChallenge.ShouldBeEmpty(); // expired entry pruned on access
    }

    /// <summary>
    /// A challenge still inside the TTL validates successfully.
    /// </summary>
    [Fact]
    public void ValidateResponse_ByResponse_SucceedsJustInsideTtl()
    {
        // Arrange
        var fakeTime = new FakeTimeProvider(Anchor);
        var service = new UserOfflineCreationService(new DateTimeMachine(fakeTime));
        var challenge = service.GenerateChallenge();
        var response = service.LastGeneratedResponse[challenge];

        // Act — still within the 15-minute window
        fakeTime.Advance(TimeSpan.FromMinutes(14));

        // Assert
        service.ValidateResponse(response).ShouldBeTrue();
    }

    /// <summary>
    /// Concurrent challenge generation neither throws (the old Dictionary.Add threw on collision) nor
    /// lets the store grow past the number of issued challenges.
    /// </summary>
    [Fact]
    public async Task GenerateChallenge_Concurrent_DoesNotThrow_AndStaysBounded()
    {
        // Arrange
        const int iterations = 500;
        var service = new UserOfflineCreationService(new DateTimeMachine(new FakeTimeProvider(Anchor)));

        // Act — hammer GenerateChallenge from many threads
        var tasks = Enumerable.Range(0, iterations)
            .Select(_ => Task.Run(() => service.GenerateChallenge()))
            .ToArray();

        var challenges = await Task.WhenAll(tasks); // must not throw

        // Assert — every call produced an 8-digit challenge and the store is bounded
        challenges.ShouldAllBe(c => c.Length == 8);
        var liveCount = service.LastGeneratedChallenge.Count;
        liveCount.ShouldBeGreaterThan(0);
        liveCount.ShouldBeLessThanOrEqualTo(iterations);
    }

    /// <summary>
    /// The injected-clock constructor guards against a null clock.
    /// </summary>
    [Fact]
    public void Constructor_WithNullClock_Throws()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new UserOfflineCreationService(null!));
    }
}
