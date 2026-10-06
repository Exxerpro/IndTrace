// <copyright file="UserOfflineCreationService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.UserService;

using System.Collections.Concurrent;
using System.Security.Cryptography;
using IndTrace.Domain.Interfaces;
using IndTrace.Domain.Models;

/// <summary>
/// Represents the UserOfflineCreationService.
/// </summary>
/// <remarks>
/// Issued challenges are held in a bounded, thread-safe store. Each challenge expires
/// <see cref="ChallengeTimeToLive"/> after issuance and a response can be consumed by the single-argument
/// <see cref="ValidateResponse(string)"/> path exactly once (single-use); expired entries are pruned on
/// every access so the store never grows without bound.
/// </remarks>
public class UserOfflineCreationService : IUserOfflineCreationService
{
    private const long PrimeMultiplier = 777_767_777L;
    private const long PrimeModulo = 323_232_323L;
    private const long MaxDigits = 100_000_000L;

    /// <summary>
    /// Lifetime of an unconsumed challenge; after this window it can no longer validate.
    /// </summary>
    private static readonly TimeSpan ChallengeTimeToLive = TimeSpan.FromMinutes(15);

    private readonly IDateTimeMachine dateTimeMachine;

    // Keyed by challenge; thread-safe and pruned on access so it stays bounded. Random.Shared is itself
    // thread-safe, so no additional lock is required around challenge generation.
    private readonly ConcurrentDictionary<string, ChallengeRecord> challenges = new(StringComparer.Ordinal);

    /// <summary>
    /// Initializes a new instance of the <see cref="UserOfflineCreationService"/> class.
    /// </summary>
    /// <param name="dateTimeMachine">The deterministic clock used for issuance/expiry timestamps.</param>
    public UserOfflineCreationService(IDateTimeMachine dateTimeMachine)
    {
        ArgumentNullException.ThrowIfNull(dateTimeMachine);
        this.dateTimeMachine = dateTimeMachine;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserOfflineCreationService"/> class using the
    /// system clock. Provided for callers (and tests) that do not supply a clock; production hosts
    /// register the service with an explicit <see cref="IDateTimeMachine"/>.
    /// </summary>
    public UserOfflineCreationService()
        : this(new DateTimeMachine())
    {
    }

    /// <summary>
    /// Gets the challenges currently live (issued and not yet expired or consumed).
    /// </summary>
    public List<string> LastGeneratedChallenge
    {
        get
        {
            this.PruneExpired();
            return this.challenges.Keys.ToList();
        }
    }

    /// <summary>
    /// Gets the live challenge-to-response map (issued and not yet expired or consumed).
    /// </summary>
    public Dictionary<string, string> LastGeneratedResponse
    {
        get
        {
            this.PruneExpired();
            return this.challenges.ToDictionary(entry => entry.Key, entry => entry.Value.Response, StringComparer.Ordinal);
        }
    }

    /// <summary>
    /// Generates an 8-digit random challenge number.
    /// </summary>
    /// <returns></returns>
    public string GenerateChallenge()
    {
        this.PruneExpired();

        var challenge = Random.Shared.Next(10000000, 99999999).ToString("D8"); // 8-digit challenge
        var response = this.ComputeResponse(challenge);

        // Indexer assignment overwrites on the (rare) random collision instead of throwing, as the
        // previous Dictionary.Add did. The response is deterministic, so an overwrite is harmless and
        // simply refreshes the issuance timestamp.
        this.challenges[challenge] = new ChallengeRecord(response, this.dateTimeMachine.UtcNow);
        return challenge;
    }

    /// <summary>
    /// Verifies the response against the challenge using the shared secret algorithm.
    /// </summary>
    /// <returns></returns>
    public bool ValidateResponse(string challenge, string response)
    {
        if (challenge is null || response is null)
        {
            return false;
        }

        var resultChallenge = long.TryParse(challenge, out var challengeNumeric);
        if (resultChallenge == false)
        {
            return false;
        }

        var resultResponse = long.TryParse(response, out var responseNumeric);
        if (resultResponse == false)
        {
            return false;
        }

        return this.VerifyResponse(challengeNumeric, responseNumeric);
    }

    /// <summary>
    /// Validates a response against the stored live challenges. On the first successful match the
    /// challenge is consumed (single-use) and cannot validate again.
    /// </summary>
    /// <returns></returns>
    public bool ValidateResponse(string response)
    {
        if (string.IsNullOrEmpty(response))
        {
            return false;
        }

        this.PruneExpired();

        foreach (var entry in this.challenges)
        {
            if (entry.Value.Response == response)
            {
                // Single-use: consume the matched challenge. TryRemove is the atomic guard against a
                // concurrent validation of the same response — only the caller that removes it wins.
                if (this.challenges.TryRemove(entry.Key, out _))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Executes VerifyResponse operation.
    /// </summary>
    /// <param name="challenge">The challenge.</param>
    /// <param name="response">The response.</param>
    /// <returns>The result of VerifyResponse.</returns>
    public bool VerifyResponse(long challenge, long response)
    {
        var expectedResponse = this.ComputeResponse(challenge);
        var result = expectedResponse == response;
        return result;
    }

    /// <summary>
    /// Computes the response for a given challenge using the shared secret algorithm.
    /// </summary>
    /// <returns></returns>
    public long ComputeResponse(long challenge)
    {
        var largeNumber = challenge * PrimeMultiplier; // Multiply by a large prime number
        var intermediateValue = largeNumber % PrimeModulo; // Modulo prime number
        var result = intermediateValue % MaxDigits; // Modulo prime number
        return result;
    }

    /// <summary>
    /// Computes the response for a given challenge using the shared secret algorithm.
    /// </summary>
    /// <returns></returns>
    public string ComputeResponse(string challenge)
    {
        // we must validate string lenght ??
        // one test is expecting that
        // yes we have 10000000 to 99999999
        if (string.IsNullOrEmpty(challenge) || challenge.Length != 8)
        {
            return string.Empty;
        }

        var resultChallenge = long.TryParse(challenge, out var challengeNumeric);
        if (resultChallenge == false)
        {
            return string.Empty;
        }

        var result = this.ComputeResponse(challengeNumeric);
        return result.ToString("D8");
    }

    /// <summary>
    /// Computes the SHA-256 hash of the input string.
    /// </summary>
    /// <returns></returns>
    public static string ComputeSha256Hash(string rawData)
    {
        using var sha256Hash = SHA256.Create();
        var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
        var builder = new StringBuilder();
        foreach (var byteValue in bytes)
        {
            builder.Append(byteValue.ToString("x2"));
        }

        return builder.ToString();
    }

    /// <summary>
    /// Removes challenges whose time-to-live has elapsed, keeping the store bounded.
    /// </summary>
    private void PruneExpired()
    {
        var nowUtc = this.dateTimeMachine.UtcNow;
        foreach (var entry in this.challenges)
        {
            if (nowUtc - entry.Value.IssuedUtc >= ChallengeTimeToLive)
            {
                this.challenges.TryRemove(entry.Key, out _);
            }
        }
    }

    /// <summary>
    /// A single issued challenge: its computed response and the UTC instant it was issued.
    /// </summary>
    private sealed record ChallengeRecord(string Response, DateTime IssuedUtc);
}
