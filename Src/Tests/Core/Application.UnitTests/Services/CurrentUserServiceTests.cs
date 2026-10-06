// <copyright file="CurrentUserServiceTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

//[Move]
//CLAUDE
//Date: 26/08/2025
//Reason: [Test Relocation] - Moved to correct architectural layer based on its responsibility

using IndTrace.Application.UserService;

namespace Application.UnitTests.Services;

/// <summary>
/// Represents the CurrentUserServiceTests.
/// </summary>

public class CurrentUserServiceTests
{
    //[Fix]
    //CLAUDE
    //Date: 29/08/2025
    //Reason: [CS0414] Removed unused _userManager field
    private readonly IUserOfflineCreationService _userOfflineCreationService = new UserOfflineCreationService();
    /// <summary>
    /// Executes GenerateChallenge_ShouldReturn_8DigitNumber operation.
    /// </summary>

    [Fact]
    public void GenerateChallenge_ShouldReturn_8DigitNumber()
    {
        // Act
        var challenge = _userOfflineCreationService.GenerateChallenge();

        // Assert
        // The challenge should be an 8-digit number
        challenge.Length.ShouldBe(8);
        challenge.ShouldBeOfType<string>();
    }
    /// <summary>
    /// Executes VerifyResponse_ShouldReturn_FalseForInvalidResponse operation.
    /// </summary>

    [Fact]
    public void VerifyResponse_ShouldReturn_FalseForInvalidResponse()
    {
        // Arrange
        var challenge = _userOfflineCreationService.GenerateChallenge();

        // Create an invalid response
        var invalidResponse = challenge;

        // Act
        // Validate the response
        var isValidResponse = _userOfflineCreationService.ValidateResponse(challenge, invalidResponse);

        // Assert
        // The response should not be verified
        isValidResponse.ShouldBeFalse();
    }
    /// <summary>
    /// Executes VerifyResponse_ShouldReturn_TrueForValidResponse operation.
    /// </summary>

    [Fact]
    public void VerifyResponse_ShouldReturn_TrueForValidResponse()
    {
        // Arrange
        var challengeString = _userOfflineCreationService.GenerateChallenge();

        long.TryParse(challengeString, out var challenge);

        long PrimeMultiplier = 777_767_777L;
        long PrimeModulo = 323_232_323L;
        long MaxDigits = 100_000_000L;

        var largeNumber = challenge * PrimeMultiplier; // Multiply by a large prime number
        var intermediateValue = largeNumber % PrimeModulo; // Modulo prime number
        var result = intermediateValue % MaxDigits; // Modulo prime number

        // Compute response using the same logic as the service to simulate authorized user behavior
        var response = result.ToString("D8");
        // Act
        // Validate the response
        var isValidResponse = _userOfflineCreationService.ValidateResponse(challengeString, response);

        // Assert
        // The response should be verified successfully
        isValidResponse.ShouldBeTrue();
    }
}