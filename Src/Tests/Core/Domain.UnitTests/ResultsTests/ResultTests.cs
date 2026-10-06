// <copyright file="ResultTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.ResultsTests;
/// <summary>
/// Represents the ResultTests.
/// </summary>

public class ResultTests
{
    /// <summary>
    /// Executes Result_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully operation.
    /// </summary>
    [Fact]
    public void Result_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully()
    {
        // Arrange & Act - Test parameterless constructor
        var instance = new Result();

        // Assert
        instance.ShouldNotBeNull();
        instance.IsSuccess.ShouldBeFalse();
        instance.IsFailure.ShouldBeTrue();
        instance.Errors.ShouldNotBeNull();
        instance.Errors.ShouldBeEmpty();

        // Arrange & Act - Test static Success factory method
        var successResult = Result.Success();

        // Assert
        successResult.ShouldNotBeNull();
        successResult.IsSuccess.ShouldBeTrue();
        successResult.IsFailure.ShouldBeFalse();
        successResult.Errors.ShouldBeEmpty();

        // Arrange & Act - Test static WithFailure factory method with single error
        var failureResult = Result.WithFailure("Test error");

        // Assert
        failureResult.ShouldNotBeNull();
        failureResult.IsSuccess.ShouldBeFalse();
        failureResult.IsFailure.ShouldBeTrue();
        failureResult.Errors.ShouldContain("Test error");
        failureResult.Errors.Count().ShouldBe(1);

        // Arrange & Act - Test static WithFailure factory method with multiple errors
        var multipleErrors = new List<string> { "Error 1", "Error 2", "Error 3" };
        var multiFailureResult = Result.WithFailure(multipleErrors);

        // Assert
        multiFailureResult.ShouldNotBeNull();
        multiFailureResult.IsSuccess.ShouldBeFalse();
        multiFailureResult.IsFailure.ShouldBeTrue();
        multiFailureResult.Errors.ShouldBeEquivalentTo(multipleErrors.ToArray());
        multiFailureResult.Errors.Count().ShouldBe(3);
    }

    /// <summary>
    /// Executes Result_WithInvalidConfiguration_ShouldHandleErrorsGracefully operation.
    /// </summary>

    [Fact]
    public void Result_WithInvalidConfiguration_ShouldHandleErrorsGracefully()
    {
        // Arrange & Act & Assert - Test generic Result<T> constructor with null checks
        var resultWithNullValue = new Result<string>(true, null, null);
        resultWithNullValue.IsSuccess.ShouldBeFalse();
        resultWithNullValue.Value.ShouldBeNull();

        // Arrange & Act & Assert - Test generic Result<T> with empty enumerable
        var emptyList = new List<int>();
        var resultWithEmptyList = new Result<List<int>>(true, null, emptyList);
        resultWithEmptyList.IsSuccess.ShouldBeTrue(); // Empty enumerable is now allowed
        resultWithEmptyList.Value.ShouldNotBeNull();
        resultWithEmptyList.Value.Count.ShouldBe(0);

        // Arrange & Act & Assert - Test generic Result<T> with errors present
        var errors = new List<string> { "Error 1", "Error 2" };
        var resultWithErrors = new Result<string>(true, errors, "test value");
        resultWithErrors.IsSuccess.ShouldBeTrue(); // Errors are now allowed with success=true
        resultWithErrors.Errors.ShouldBeEquivalentTo(errors.ToArray());

        // Arrange & Act & Assert - Test CombineErrors edge cases
        var combineResult = Result.CombineErrors(null, null);
        combineResult.IsFailure.ShouldBeTrue();
        combineResult.Errors.Select(e => e.Contains("No errors were found")).Count().ShouldBeGreaterThanOrEqualTo(1);

        // Arrange & Act & Assert - Test generic CombineErrors edge cases
        var genericCombineResult = Result<string>.CombineErrors<string>(null, null);
        genericCombineResult.IsFailure.ShouldBeTrue();
        genericCombineResult.Errors.Select(e => e.Contains("No errors were found")).Count().ShouldBeGreaterThanOrEqualTo(1);
        genericCombineResult.Value.ShouldBeNull();
    }

    /// <summary>
    /// Executes Result_WhenPropertiesAssigned_ShouldMaintainAllValues operation.
    /// </summary>

    [Fact]
    public void Result_WhenPropertiesAssigned_ShouldMaintainAllValues()
    {
        // Arrange & Act - Test non-generic Result properties
        var successResult = Result.Success();
        var failureResult = Result.WithFailure("Test failure");

        // Assert - Success result properties
        successResult.IsSuccess.ShouldBeTrue();
        successResult.IsFailure.ShouldBeFalse();
        successResult.Errors.ShouldBeEmpty();

        // Assert - Failure result properties
        failureResult.IsSuccess.ShouldBeFalse();
        failureResult.IsFailure.ShouldBeTrue();
        failureResult.Errors.ShouldContain("Test failure");

        // Arrange & Act - Test generic Result<T> properties
        var successGenericResult = Result<int>.Success(42);
        var failureGenericResult = Result<string>.WithFailure("Generic failure", "fallback value");

        // Assert - Generic success result properties
        successGenericResult.IsSuccess.ShouldBeTrue();
        successGenericResult.IsFailure.ShouldBeFalse();
        successGenericResult.Value.ShouldBe(42);
        successGenericResult.Errors.ShouldBeEmpty();

        // Assert - Generic failure result properties
        failureGenericResult.IsSuccess.ShouldBeFalse();
        failureGenericResult.IsFailure.ShouldBeTrue();
        failureGenericResult.Value.ShouldBe("fallback value");
        failureGenericResult.Errors.ShouldContain("Generic failure");

        // Arrange & Act - Test warning properties
        var warningResult = Result<string>.WithWarnings(["Warning 1"], "success value", confidence: 0.8, missingDataRatio: 0.25);

        // Assert - Warning result properties
        warningResult.IsSuccess.ShouldBeTrue(); // Should be success with warnings
        warningResult.HasWarnings.ShouldBeTrue();
        warningResult.IsRecoverable.ShouldBeTrue();
        warningResult.Value.ShouldBe("success value");
        warningResult.Warnings.ShouldContain("Warning 1");
        warningResult.Confidence.ShouldBe(0.8, 0.0001);
        warningResult.MissingDataRatio.ShouldBe(0.25, 0.0001);

        // Arrange & Act - Test enumerable value handling
        var listValue = new List<string> { "item1", "item2" };
        var resultWithList = Result<List<string>>.Success(listValue);

        // Assert - Enumerable value properties
        resultWithList.IsSuccess.ShouldBeTrue();
        resultWithList.Value.ShouldNotBeNull();
        resultWithList.Value.ShouldBeEquivalentTo(listValue);
        resultWithList.Value.Count.ShouldBe(2);
    }

    /// <summary>
    /// Executes Result_WhenMethodsInvoked_ShouldProduceExpectedOutcomes operation.
    /// </summary>

    [Fact]
    public void Result_WhenMethodsInvoked_ShouldProduceExpectedOutcomes()
    {
        // Arrange
        var successResult = Result.Success();
        var failureResult = Result.WithFailure("Initial failure");
        var actionExecuted = false;
        var capturedErrors = new List<string>();

        // Act & Assert - Test OnSuccess method
        successResult.OnSuccess(() => actionExecuted = true);
        actionExecuted.ShouldBeTrue();

        actionExecuted = false; // Reset
        failureResult.OnSuccess(() => actionExecuted = true);
        actionExecuted.ShouldBeFalse(); // Should not execute on failure

        // Act & Assert - Test OnFailure method
        failureResult.OnFailure(errors => capturedErrors.AddRange(errors));
        capturedErrors.ShouldContain("Initial failure");

        capturedErrors.Clear(); // Reset
        successResult.OnFailure(errors => capturedErrors.AddRange(errors));
        capturedErrors.ShouldBeEmpty(); // Should not execute on success

        // Arrange & Act - Test Map method
        var mappedResult = successResult.Map(() => "Mapped value");
        var failedMappedResult = failureResult.Map(() => "This won't be returned");

        // Assert - Map method results
        mappedResult.IsSuccess.ShouldBeTrue();
        mappedResult.Value.ShouldNotBeNull();
        mappedResult.Value.ShouldBe("Mapped value");
        failedMappedResult.IsFailure.ShouldBeTrue();
        failedMappedResult.Errors.ShouldContain("Initial failure");

        // Arrange & Act - Test Bind method
        var boundResult = successResult.Bind(() => Result<int>.Success(100));
        var failedBoundResult = failureResult.Bind(() => Result<int>.Success(200));

        // Assert - Bind method results
        boundResult.IsSuccess.ShouldBeTrue();
        boundResult.Value.ShouldBe(100);
        failedBoundResult.IsFailure.ShouldBeTrue();
        failedBoundResult.Errors.ShouldContain("Initial failure");

        // Arrange & Act - Test Ensure method
        var ensuredSuccess = successResult.Ensure(() => true, "Should not fail");
        var ensuredFailure = successResult.Ensure(() => false, "Condition failed");

        // Assert - Ensure method results
        ensuredSuccess.IsSuccess.ShouldBeTrue();
        ensuredFailure.IsFailure.ShouldBeTrue();
        ensuredFailure.Errors.ShouldContain("Condition failed");

        // Arrange & Act - Test Tap method
        var tapExecuted = false;
        successResult.Tap(() => tapExecuted = true);

        // Assert - Tap method result
        tapExecuted.ShouldBeTrue();

        // Arrange & Act - Test Combine method
        var otherResult = Result.Success();
        var combinedSuccess = successResult.Combine(otherResult);
        var combinedFailure = successResult.Combine(failureResult);

        // Assert - Combine method results
        combinedSuccess.IsSuccess.ShouldBeTrue();
        combinedFailure.IsFailure.ShouldBeTrue();
        combinedFailure.Errors.ShouldContain("Initial failure");

        // Arrange & Act - Test Match method
        var matchResult = successResult.Match(() => "Success!", errors => $"Failed: {string.Join(", ", errors)}");
        var matchFailResult = failureResult.Match(() => "Success!", errors => $"Failed: {string.Join(", ", errors)}");

        // Assert - Match method results
        matchResult.ShouldBe("Success!");
        matchFailResult.ShouldStartWith("Failed:");
        matchFailResult.ShouldContain("Initial failure");

        // Arrange & Act - Test Recover method
        var recoveredResult = failureResult.Recover(() => Result.Success());
        var noRecoveryResult = successResult.Recover(() => Result.WithFailure("Should not execute"));

        // Assert - Recover method results
        recoveredResult.IsSuccess.ShouldBeTrue();
        noRecoveryResult.IsSuccess.ShouldBeTrue();

        // Arrange & Act - Test ToString method
        var successString = successResult.ToString();
        var failureString = failureResult.ToString();

        // Assert - ToString method results
        successString.ShouldBe("Success");
        failureString.ShouldContain("Failure:"); // Updated to match refactored Result.ToString() format
        failureString.ShouldContain("Initial failure");
    }

    /// <summary>
    /// Executes Result_BusinessScenario_ShouldEnforceAllDomainRules operation.
    /// </summary>

    [Fact]
    public void Result_BusinessScenario_ShouldEnforceAllDomainRules()
    {
        // Arrange - Create domain scenarios for manufacturing context
        var validPartNumber = "PN-001-VALID";
        var invalidPartNumber = "";
        var validQuality = 95.5;
        var invalidQuality = 45.0;

        // Act & Assert - Test manufacturing validation business rules
        var validationResult = Result.Success()
            .Ensure(() => !string.IsNullOrEmpty(validPartNumber), "Part number is required")
            .Ensure(() => validQuality >= 90.0, "Quality must be at least 90%")
            .Map(() => new { PartNumber = validPartNumber, Quality = validQuality });

        validationResult.IsSuccess.ShouldBeTrue();
        validationResult.Value.ShouldNotBeNull();
        validationResult.Value.PartNumber.ShouldBe(validPartNumber);
        validationResult.Value.Quality.ShouldBe(validQuality);

        // Act & Assert - Test failing validation business rules
        var failedValidationResult = Result.Success()
            .Ensure(() => !string.IsNullOrEmpty(invalidPartNumber), "Part number is required")
            .Ensure(() => invalidQuality >= 90.0, "Quality must be at least 90%");

        failedValidationResult.IsFailure.ShouldBeTrue();
        failedValidationResult.Errors.ShouldContain("Part number is required");

        // Act & Assert - Test manufacturing process workflow
        var processSteps = new List<string>();
        var manufacturingWorkflow = Result<string>.Success("Raw Material")
            .Tap(material => processSteps.Add($"Processing: {material}"))
            .Map(material => $"Machined {material}")
            .Tap(product => processSteps.Add($"Quality Check: {product}"))
            .Bind(product => Result<string>.Success($"Finished {product}"))
            .OnSuccess(finalProduct => processSteps.Add($"Completed: {finalProduct}"));

        manufacturingWorkflow.IsSuccess.ShouldBeTrue();
        manufacturingWorkflow.Value.ShouldNotBeNull();
        manufacturingWorkflow.Value.ShouldBe("Finished Machined Raw Material");
        processSteps.Count.ShouldBe(3);
        processSteps.ShouldContain("Processing: Raw Material");
        processSteps.ShouldContain("Quality Check: Machined Raw Material");
        processSteps.ShouldContain("Completed: Finished Machined Raw Material");

        // Act & Assert - Test error recovery in manufacturing context
        var primaryErrors = new List<string> { "Machine malfunction", "Power outage" };
        var secondaryErrors = new List<string> { "Backup system activated", "Manual override required" };

        var errorRecoveryResult = Result.CombineErrors(primaryErrors, secondaryErrors)
            .Recover(() => Result.Success().Tap(() => processSteps.Add("Emergency protocols activated")));

        errorRecoveryResult.IsSuccess.ShouldBeTrue();
        processSteps.ShouldContain("Emergency protocols activated");

        // Act & Assert - Test complex domain scenario with multiple validations
        var batchProcessingResult = Result<int>.Success(1000) // Batch size
            .Ensure(batchSize => batchSize > 0, "Batch size must be positive")
            .Ensure(batchSize => batchSize <= 5000, "Batch size exceeds maximum capacity")
            .Map(batchSize => new { BatchSize = batchSize, EstimatedTime = batchSize * 0.5 })
            .Bind(batch => batch.EstimatedTime <= 2000
                ? Result<object>.Success(new { batch.BatchSize, batch.EstimatedTime, Status = "Approved" })
                : Result<object>.WithFailure("Processing time exceeds shift duration"));

        batchProcessingResult.IsSuccess.ShouldBeTrue();
        var batchResult = batchProcessingResult.Value;
        batchResult.ShouldNotBeNull();
        batchResult.ShouldNotBeNull();

        // Act & Assert - Test implicit conversions and deconstruction
        Result<string> implicitResult = "Test Value"; // Implicit conversion from T to Result<T>
        implicitResult.IsSuccess.ShouldBeTrue();
        implicitResult.Value.ShouldNotBeNull();
        implicitResult.Value.ShouldBe("Test Value");

        Result nonGenericResult = implicitResult; // Implicit conversion from Result<T> to Result
        nonGenericResult.IsSuccess.ShouldBeTrue();

        // Test deconstruction
        var (succeeded, data, errors) = Result<string>.Success("Deconstructed Value");
        succeeded.ShouldBeTrue();
        data.ShouldBe("Deconstructed Value");
        errors.ShouldBeEmpty();
    }

    /// <summary>
    /// Executes CombineErrors_NonGeneric_BothNull_ShouldReturnNoErrorsFoundMessage operation.
    /// </summary>

    [Fact]
    public void CombineErrors_NonGeneric_BothNull_ShouldReturnNoErrorsFoundMessage()
    {
        // Act
        var result = Result.CombineErrors(null, null);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("No errors were found");
    }

    /// <summary>
    /// Executes CombineErrors_NonGeneric_PrimaryErrorsOnly_ShouldReturnPrimaryErrors operation.
    /// </summary>

    [Fact]
    public void CombineErrors_NonGeneric_PrimaryErrorsOnly_ShouldReturnPrimaryErrors()
    {
        // Arrange
        var primaryErrors = new List<string> { "Error 1", "Error 2" };

        // Act
        var result = Result.CombineErrors(primaryErrors, null);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Error 1");
        result.Errors.ShouldContain("Error 2");
    }

    /// <summary>
    /// Executes CombineErrors_NonGeneric_SecondaryErrorsOnly_ShouldReturnSecondaryErrors operation.
    /// </summary>

    [Fact]
    public void CombineErrors_NonGeneric_SecondaryErrorsOnly_ShouldReturnSecondaryErrors()
    {
        // Arrange
        var secondaryErrors = new List<string> { "Error A", "Error B" };

        // Act
        var result = Result.CombineErrors(null, secondaryErrors);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Error A");
        result.Errors.ShouldContain("Error B");
    }

    /// <summary>
    /// Executes CombineErrors_NonGeneric_BothPrimaryAndSecondaryErrors_ShouldReturnCombinedErrors operation.
    /// </summary>

    [Fact]
    public void CombineErrors_NonGeneric_BothPrimaryAndSecondaryErrors_ShouldReturnCombinedErrors()
    {
        // Arrange
        var primaryErrors = new List<string> { "Error 1", "Error 2" };
        var secondaryErrors = new List<string> { "Error A", "Error B" };

        // Act
        var result = Result.CombineErrors(primaryErrors, secondaryErrors);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Error 1");
        result.Errors.ShouldContain("Error 2");
        result.Errors.ShouldContain("Error A");
        result.Errors.ShouldContain("Error B");
    }

    /// <summary>
    /// Executes CombineErrors_NonGeneric_NoErrors_ShouldReturnNoErrorsFoundMessage operation.
    /// </summary>

    [Fact]
    public void CombineErrors_NonGeneric_NoErrors_ShouldReturnNoErrorsFoundMessage()
    {
        // Act
        var result = Result.CombineErrors(null, null);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.Count().ShouldBe(1);
        result.Errors.ShouldContain("No errors were found");
    }

    /// <summary>
    /// Executes CombineErrors_BothNull_ShouldReturnNoErrorsFoundMessageWithDefault operation.
    /// </summary>

    [Fact]
    public void CombineErrors_BothNull_ShouldReturnNoErrorsFoundMessageWithDefault()
    {
        // Act
        var result = Result<string>.CombineErrors<string>(null, null);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("No errors were found");
        result.Value.ShouldBeNull();
    }

    /// <summary>
    /// Executes CombineErrors_PrimaryErrorsOnly_ShouldReturnPrimaryErrorsWithDefault operation.
    /// </summary>

    [Fact]
    public void CombineErrors_PrimaryErrorsOnly_ShouldReturnPrimaryErrorsWithDefault()
    {
        // Arrange
        var primaryErrors = new List<string> { "Error 1", "Error 2" };

        // Act
        var result = Result<string>.CombineErrors<string>(primaryErrors, null);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldBeEquivalentTo(primaryErrors.ToArray());
        result.Value.ShouldBeNull();
    }

    /// <summary>
    /// Executes CombineErrors_SecondaryErrorsOnly_ShouldReturnSecondaryErrorsWithValue operation.
    /// </summary>

    [Fact]
    public void CombineErrors_SecondaryErrorsOnly_ShouldReturnSecondaryErrorsWithValue()
    {
        // Arrange
        var secondaryErrors = new List<string> { "Error A", "Error B" };
        var value = "TestValue";

        // Act
        var result = Result<string>.CombineErrors<string>(null, secondaryErrors, value);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldBeEquivalentTo(secondaryErrors.ToArray());
        result.Value.ShouldBe(value);
    }

    /// <summary>
    /// Executes CombineErrors_BothPrimaryAndSecondaryErrors_ShouldReturnCombinedErrorsWithValue operation.
    /// </summary>

    [Fact]
    public void CombineErrors_BothPrimaryAndSecondaryErrors_ShouldReturnCombinedErrorsWithValue()
    {
        // Arrange
        var primaryErrors = new List<string> { "Error 1", "Error 2" };
        var secondaryErrors = new List<string> { "Error A", "Error B" };
        var value = "CombinedValue";

        // Act
        var result = Result<string>.CombineErrors<string>(primaryErrors, secondaryErrors, value);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldBeEquivalentTo(new[] { "Error 1", "Error 2", "Error A", "Error B" });
        result.Value.ShouldBe(value);
    }

    /// <summary>
    /// Executes CombineErrors_NoErrors_ShouldReturnNoErrorsFoundMessageWithValue operation.
    /// </summary>

    [Fact]
    public void CombineErrors_NoErrors_ShouldReturnNoErrorsFoundMessageWithValue()
    {
        // Arrange
        var value = "TestValue";

        // Act
        var result = Result<string>.CombineErrors<string>(null, null, value);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("No errors were found");
        result.Value.ShouldBe(value);
    }

    /// <summary>
    /// Executes Result_Success_ShouldBeSuccessful operation.
    /// </summary>

    [Fact]
    public void Result_Success_ShouldBeSuccessful()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Errors.ShouldBeEmpty();
    }

    /// <summary>
    /// Executes ResultGeneric_Success_ShouldBeSuccessful operation.
    /// </summary>

    [Fact]
    public void ResultGeneric_Success_ShouldBeSuccessful()
    {
        // Act
        var result = Result<int>.Success(4);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Errors.ShouldBeEmpty(); // Fixed: Success should have empty errors, not null
        result.Value.ShouldBe(4);
    }

    /// <summary>
    /// Executes Result_Failure_ShouldBeFailureWithErrors operation.
    /// </summary>

    [Fact]
    public void Result_Failure_ShouldBeFailureWithErrors()
    {
        // Arrange
        var errors = new List<string> { "Error1", "Error2" };

        // Act
        var result = Result.WithFailure(errors);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        errors.All(result.Errors.Contains).ShouldBeTrue();
    }

    /// <summary>
    /// Executes ResultGeneric_Failure_SingleError_ShouldBeFailureWithSingleError operation.
    /// </summary>

    [Fact]
    public void ResultGeneric_Failure_SingleError_ShouldBeFailureWithSingleError()
    {
        // Arrange
        var error = "Error1";

        // Act
        var result = Result<int>.WithFailure(error);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Errors.Count().ShouldBe(1);
        result.Errors.FirstOrDefault().ShouldBe(error);
    }

    /// <summary>
    /// Executes ResultGeneric_ToString_ShouldReturnSuccessOrFailureString operation.
    /// </summary>

    [Fact]
    public void ResultGeneric_ToString_ShouldReturnSuccessOrFailureString()
    {
        // Act
        var successResult = Result<int>.Success(4);
        var failureResult = Result<int>.WithFailure("Error1");

        // Assert
        successResult.ToString().StartsWith("Success:").ShouldBeTrue();
        failureResult.ToString().ShouldContain("Failure:");
        failureResult.ToString().ShouldContain("Error1");
    }

    /// <summary>
    /// Executes Result_Failure_SingleError_ShouldBeFailureWithSingleError operation.
    /// </summary>

    [Fact]
    public void Result_Failure_SingleError_ShouldBeFailureWithSingleError()
    {
        // Arrange
        var error = "Error1";

        // Act
        var result = Result.WithFailure(error);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Errors.Count().ShouldBe(1);
        result.Errors.FirstOrDefault().ShouldBe(error);
    }

    /// <summary>
    /// Executes Result_ToString_ShouldReturnSuccessOrFailureString operation.
    /// </summary>

    [Fact]
    public void Result_ToString_ShouldReturnSuccessOrFailureString()
    {
        // Act
        var successResult = Result.Success();
        var failureResult = Result.WithFailure("Error1");

        // Assert
        successResult.ToString().ShouldBe("Success");
        failureResult.ToString().ShouldContain("Error1");
    }

    /// <summary>
    /// Executes Result_OnSuccess_ShouldInvokeAction_WhenResultIsSuccess operation.
    /// </summary>

    [Fact]
    public void Result_OnSuccess_ShouldInvokeAction_WhenResultIsSuccess()
    {
        // Arrange
        var result = Result.Success();
        var action = Substitute.For<Action>();

        // Act
        result.OnSuccess(action);

        // Assert
        action.Received(1).Invoke();
    }

    /// <summary>
    /// Executes Result_OnFailure_ShouldInvokeActionWithErrors_WhenResultIsFailure operation.
    /// </summary>

    [Fact]
    public void Result_OnFailure_ShouldInvokeActionWithErrors_WhenResultIsFailure()
    {
        // Arrange
        var errors = new List<string> { "Error1", "Error2" };
        var result = Result.WithFailure(errors);
        var action = Substitute.For<Action<IEnumerable<string>>>();

        // Act
        result.OnFailure(action);

        // Assert
        action.Received(1).Invoke(Arg.Is<IEnumerable<string>>(e => e.SequenceEqual(errors)));
    }

    /// <summary>
    /// Executes Result_Ensure_ShouldReturnFailure_WhenConditionFails operation.
    /// </summary>

    [Fact]
    public void Result_Ensure_ShouldReturnFailure_WhenConditionFails()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var ensuredResult = result.Ensure(() => false, "Condition failed");

        // Assert
        ensuredResult.IsFailure.ShouldBeTrue();
        ensuredResult.Errors.ShouldContain("Condition failed");
    }

    /// <summary>
    /// Executes Result_Tap_ShouldInvokeAction_WhenResultIsSuccess operation.
    /// </summary>

    [Fact]
    public void Result_Tap_ShouldInvokeAction_WhenResultIsSuccess()
    {
        // Arrange
        var result = Result.Success();
        var action = Substitute.For<Action>();

        // Act
        result.Tap(action);

        // Assert
        action.Received(1).Invoke();
    }

    /// <summary>
    /// Executes Result_Recover_ShouldReturnRecoveredResult_WhenResultIsFailure operation.
    /// </summary>

    [Fact]
    public void Result_Recover_ShouldReturnRecoveredResult_WhenResultIsFailure()
    {
        // Arrange
        var result = Result.WithFailure("Initial failure");
        var recoverResult = Result.Success();

        // Act
        var finalResult = result.Recover(() => recoverResult);

        // Assert
        finalResult.IsSuccess.ShouldBeTrue();
    }
}