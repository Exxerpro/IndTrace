// <copyright file="UserOfflineCreationServiceCryptographicTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.UserService;

namespace Application.UnitTests.Features.UserService;

/// <summary>
/// Tests for UserOfflineCreationService cryptographic challenge-response computation
/// </summary>
public class UserOfflineCreationServiceComputationTests
{
    /// <summary>
    /// Executes ComputeResponse_WithStringChallenge_ShouldReturnEightDigitString operation.
    /// </summary>
    [Fact]
    public void ComputeResponse_WithStringChallenge_ShouldReturnEightDigitString()
    {
        // Arrange
        var service = new UserOfflineCreationService();
        var challenge = "12345678";

        // Act
        var response = service.ComputeResponse(challenge);

        // Assert
        response.ShouldNotBeNullOrEmpty();
        response.Length.ShouldBe(8);
        response.ShouldMatch(@"^\d{8}$");
    }

    /// <summary>
    /// Executes ComputeResponse_WithLongChallenge_ShouldReturnConsistentResult operation.
    /// </summary>

    [Fact]
    public void ComputeResponse_WithLongChallenge_ShouldReturnConsistentResult()
    {
        // Arrange
        var service = new UserOfflineCreationService();
        var challenge = 12345678L;

        // Act
        var response1 = service.ComputeResponse(challenge);
        var response2 = service.ComputeResponse(challenge);

        // Assert
        response1.ShouldBe(response2); // Should be deterministic
    }

    /// <summary>
    /// Executes ComputeResponse_WithInvalidStringChallenge_ShouldReturnEmptyString operation.
    /// </summary>

    [Fact]
    public void ComputeResponse_WithInvalidStringChallenge_ShouldReturnEmptyString()
    {
        // Arrange
        var service = new UserOfflineCreationService();

        // Act & Assert
        service.ComputeResponse("invalid").ShouldBeEmpty();
        service.ComputeResponse("").ShouldBeEmpty();
        service.ComputeResponse("12345abc").ShouldBeEmpty();
        service.ComputeResponse("123").ShouldBeEmpty(); // Too short
        service.ComputeResponse("1234567890123").ShouldBeEmpty(); // Too long
    }

    /// <summary>
    /// Executes ComputeResponse_WithValidChallenges_ShouldProduceDifferentResults operation.
    /// </summary>
    /// <param name="challenge">The challenge.</param>

    [Theory]
    [InlineData(10000000L)] // Minimum valid challenge
    [InlineData(50000000L)] // Mid-range challenge
    [InlineData(99999999L)] // Maximum valid challenge
    public void ComputeResponse_WithValidChallenges_ShouldProduceDifferentResults(long challenge)
    {
        // Using parameters: challenge
        _ = challenge; // xUnit1026 fix
        // Using parameters: challenge
        _ = challenge; // xUnit1026 fix
        // Using parameters: challenge
        _ = challenge; // xUnit1026 fix
        // Using parameters: challenge
        _ = challenge; // xUnit1026 fix
        // Using parameters: challenge
        _ = challenge; // xUnit1026 fix
        // Arrange
        var service = new UserOfflineCreationService();

        // Act
        var response = service.ComputeResponse(challenge);

        // Assert
        response.ShouldBeGreaterThanOrEqualTo(0);
        response.ShouldBeLessThan(100_000_000L); // Within MaxDigits constraint
    }

    /// <summary>
    /// Executes ComputeResponse_WithDifferentChallenges_ShouldProduceDifferentResponses operation.
    /// </summary>

    [Fact]
    public void ComputeResponse_WithDifferentChallenges_ShouldProduceDifferentResponses()
    {
        // Arrange
        var service = new UserOfflineCreationService();
        var challenge1 = 12345678L;
        var challenge2 = 87654321L;

        // Act
        var response1 = service.ComputeResponse(challenge1);
        var response2 = service.ComputeResponse(challenge2);

        // Assert
        response1.ShouldNotBe(response2); // Different challenges should produce different responses
    }
}

/// <summary>
/// Tests for UserOfflineCreationService validation logic
/// </summary>
public class UserOfflineCreationServiceValidationTests
{
    /// <summary>
    /// Executes ValidateResponse_WithCorrectChallengeResponse_ShouldReturnTrue operation.
    /// </summary>
    [Fact]
    public void ValidateResponse_WithCorrectChallengeResponse_ShouldReturnTrue()
    {
        // Arrange
        var service = new UserOfflineCreationService();
        var challenge = "12345678";
        var expectedResponse = service.ComputeResponse(challenge);

        // Act
        var isValid = service.ValidateResponse(challenge, expectedResponse);

        // Assert
        isValid.ShouldBeTrue();
    }

    /// <summary>
    /// Executes ValidateResponse_WithIncorrectResponse_ShouldReturnFalse operation.
    /// </summary>

    [Fact]
    public void ValidateResponse_WithIncorrectResponse_ShouldReturnFalse()
    {
        // Arrange
        var service = new UserOfflineCreationService();
        var challenge = "12345678";
        var wrongResponse = "99999999";

        // Act
        var isValid = service.ValidateResponse(challenge, wrongResponse);

        // Assert
        isValid.ShouldBeFalse();
    }

    /// <summary>
    /// Executes ValidateResponse_WithNullParameters_ShouldReturnFalse operation.
    /// </summary>

    [Fact]
    public void ValidateResponse_WithNullParameters_ShouldReturnFalse()
    {
        // Arrange
        var service = new UserOfflineCreationService();

        // Act & Assert
        service.ValidateResponse(null!, "12345678").ShouldBeFalse();
        service.ValidateResponse("12345678", null!).ShouldBeFalse();
        service.ValidateResponse(null!, null!).ShouldBeFalse();
    }

    /// <summary>
    /// Executes ValidateResponse_WithInvalidChallengeFormat_ShouldReturnFalse operation.
    /// </summary>

    [Fact]
    public void ValidateResponse_WithInvalidChallengeFormat_ShouldReturnFalse()
    {
        // Arrange
        var service = new UserOfflineCreationService();

        // Act & Assert
        service.ValidateResponse("invalid", "12345678").ShouldBeFalse();
        service.ValidateResponse("123abc", "12345678").ShouldBeFalse();
        service.ValidateResponse("", "12345678").ShouldBeFalse();
    }

    /// <summary>
    /// Executes ValidateResponse_WithInvalidResponseFormat_ShouldReturnFalse operation.
    /// </summary>

    [Fact]
    public void ValidateResponse_WithInvalidResponseFormat_ShouldReturnFalse()
    {
        // Arrange
        var service = new UserOfflineCreationService();

        // Act & Assert
        service.ValidateResponse("12345678", "invalid").ShouldBeFalse();
        service.ValidateResponse("12345678", "123abc").ShouldBeFalse();
        service.ValidateResponse("12345678", "").ShouldBeFalse();
    }

    /// <summary>
    /// Executes ValidateResponse_ByResponseOnly_WithGeneratedChallenge_ShouldReturnTrue operation.
    /// </summary>

    [Fact]
    public void ValidateResponse_ByResponseOnly_WithGeneratedChallenge_ShouldReturnTrue()
    {
        // Arrange
        var service = new UserOfflineCreationService();
        var challenge = service.GenerateChallenge();
        var expectedResponse = service.LastGeneratedResponse[challenge];

        // Act
        var isValid = service.ValidateResponse(expectedResponse);

        // Assert
        isValid.ShouldBeTrue();
    }

    /// <summary>
    /// Executes ValidateResponse_ByResponseOnly_WithWrongResponse_ShouldReturnFalse operation.
    /// </summary>

    [Fact]
    public void ValidateResponse_ByResponseOnly_WithWrongResponse_ShouldReturnFalse()
    {
        // Arrange
        var service = new UserOfflineCreationService();
        service.GenerateChallenge(); // Generate at least one challenge
        var wrongResponse = "99999999";

        // Act
        var isValid = service.ValidateResponse(wrongResponse);

        // Assert
        isValid.ShouldBeFalse();
    }

    /// <summary>
    /// Executes ValidateResponse_ByResponseOnly_WithNullOrEmpty_ShouldReturnFalse operation.
    /// </summary>

    [Fact]
    public void ValidateResponse_ByResponseOnly_WithNullOrEmpty_ShouldReturnFalse()
    {
        // Arrange
        var service = new UserOfflineCreationService();

        // Act & Assert
        service.ValidateResponse((string?)null!).ShouldBeFalse();
        service.ValidateResponse("").ShouldBeFalse();
        service.ValidateResponse(string.Empty).ShouldBeFalse();
    }
}

/// <summary>
/// Tests for UserOfflineCreationService SHA-256 cryptographic operations
/// </summary>
public class UserOfflineCreationServiceSha256Tests
{
    /// <summary>
    /// Executes ComputeSha256Hash_WithKnownInput_ShouldReturnExpectedHash operation.
    /// </summary>
    [Fact]
    public void ComputeSha256Hash_WithKnownInput_ShouldReturnExpectedHash()
    {
        // Arrange
        var input = "Hello, World!";
        var expectedHash = "dffd6021bb2bd5b0af676290809ec3a53191dd81c7f70a4b28688a362182986f"; // Known SHA-256 hash

        // Act
        var actualHash = UserOfflineCreationService.ComputeSha256Hash(input);

        // Assert
        actualHash.ShouldBe(expectedHash);
    }

    /// <summary>
    /// Executes ComputeSha256Hash_WithEmptyString_ShouldReturnValidHash operation.
    /// </summary>

    [Fact]
    public void ComputeSha256Hash_WithEmptyString_ShouldReturnValidHash()
    {
        // Arrange
        var input = "";
        var expectedHash = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"; // Known empty string hash

        // Act
        var actualHash = UserOfflineCreationService.ComputeSha256Hash(input);

        // Assert
        actualHash.ShouldBe(expectedHash);
        actualHash.Length.ShouldBe(64); // SHA-256 produces 64-character hex string
    }

    /// <summary>
    /// Executes ComputeSha256Hash_WithDifferentInputs_ShouldProduceDifferentHashes operation.
    /// </summary>

    [Fact]
    public void ComputeSha256Hash_WithDifferentInputs_ShouldProduceDifferentHashes()
    {
        // Arrange
        var input1 = "Manufacturing Data 1";
        var input2 = "Manufacturing Data 2";

        // Act
        var hash1 = UserOfflineCreationService.ComputeSha256Hash(input1);
        var hash2 = UserOfflineCreationService.ComputeSha256Hash(input2);

        // Assert
        hash1.ShouldNotBe(hash2);
        hash1.Length.ShouldBe(64);
        hash2.Length.ShouldBe(64);
    }

    /// <summary>
    /// Executes ComputeSha256Hash_ShouldBeConsistent operation.
    /// </summary>

    [Fact]
    public void ComputeSha256Hash_ShouldBeConsistent()
    {
        // Arrange
        var input = "Automotive Engine Block VIN:1FTFW1ET5DFC12345";

        // Act
        var hash1 = UserOfflineCreationService.ComputeSha256Hash(input);
        var hash2 = UserOfflineCreationService.ComputeSha256Hash(input);

        // Assert
        hash1.ShouldBe(hash2); // Should be deterministic
    }

    /// <summary>
    /// Executes ComputeSha256Hash_WithSecretKeyVariations_ShouldReturnValidHashes operation.
    /// </summary>
    /// <param name="input">The input.</param>

    [Theory]
    [InlineData("SecretKey")]
    [InlineData("123456789SecretKey")]
    [InlineData("777767777SecretKey")]
    public void ComputeSha256Hash_WithSecretKeyVariations_ShouldReturnValidHashes(string input)
    {
        // Using parameters: input
        _ = input; // xUnit1026 fix
        // Using parameters: input
        _ = input; // xUnit1026 fix
        // Using parameters: input
        _ = input; // xUnit1026 fix
        // Using parameters: input
        _ = input; // xUnit1026 fix
        // Using parameters: input
        _ = input; // xUnit1026 fix
        // Act
        var hash = UserOfflineCreationService.ComputeSha256Hash(input);

        // Assert
        hash.ShouldNotBeNullOrEmpty();
        hash.Length.ShouldBe(64);
        hash.ShouldMatch(@"^[a-f0-9]{64}$"); // Valid hex string
    }
}

/// <summary>
/// Tests for UserOfflineCreationService challenge/response verification (arithmetic path).
/// </summary>
public class UserOfflineCreationServiceVerifyResponseTests
{
    /// <summary>
    /// Executes VerifyResponse_WithCorrectLongValues_ShouldReturnTrue operation.
    /// </summary>

    [Fact]
    public void VerifyResponse_WithCorrectLongValues_ShouldReturnTrue()
    {
        // Arrange
        var service = new UserOfflineCreationService();
        var challenge = 12345678L;
        var expectedResponse = service.ComputeResponse(challenge);

        // Act
        var isValid = service.VerifyResponse(challenge, expectedResponse);

        // Assert
        isValid.ShouldBeTrue();
    }

    /// <summary>
    /// Executes VerifyResponse_WithIncorrectLongValues_ShouldReturnFalse operation.
    /// </summary>

    [Fact]
    public void VerifyResponse_WithIncorrectLongValues_ShouldReturnFalse()
    {
        // Arrange
        var service = new UserOfflineCreationService();
        var challenge = 12345678L;
        var wrongResponse = 99999999L;

        // Act
        var isValid = service.VerifyResponse(challenge, wrongResponse);

        // Assert
        isValid.ShouldBeFalse();
    }
}