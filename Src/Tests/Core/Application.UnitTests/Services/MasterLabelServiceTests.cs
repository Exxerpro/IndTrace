// <copyright file="MasterLabelServiceTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Services;

/// <summary>
/// Unit tests for MasterLabelService
/// </summary>
public class MasterLabelServiceTests
{
    private readonly IReadOnlyRepository<MasterLabel> _masterLabelRepository = null!;
    private readonly MasterLabelService _service = null!;

    public MasterLabelServiceTests()
    {
        //[Fix]
        //CLAUDE
        //Date: 25/08/2025
        //Reason: [DI SIMPLIFICATION] - Remove HybridCache dependency, now handled by DI at repository level
        _masterLabelRepository = Substitute.For<IReadOnlyRepository<MasterLabel>>();
        _service = new MasterLabelService(_masterLabelRepository);
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var service = new MasterLabelService(_masterLabelRepository);

        // Assert
        service.ShouldNotBeNull();
        service.ShouldBeAssignableTo<IMasterLabelService>();
    }

    [Fact]
    public async Task GetMasterLabelByPartNumberAsync_WithValidPartNumber_ShouldReturnSuccess()
    {
        // Arrange
        var partNumber = "TEST123";
        var masterLabels = new List<MasterLabel>
        {
            new() { MasterLabelId = 1, MasterLabelCode = "LABEL1_TEST123" },
            new() { MasterLabelId = 2, MasterLabelCode = "LABEL2_TEST123" }
        };

        _masterLabelRepository.ListAsync(Arg.Any<Specification<MasterLabel>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<MasterLabel>>.Success(masterLabels));

        // Act
        var result = await _service.GetMasterLabelByPartNumberAsync(partNumber, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldContain("LABEL1_TEST123");
        result.Value.ShouldContain("LABEL2_TEST123");
        result.Value.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetMasterLabelByPartNumberAsync_WithNoMatches_ShouldReturnFailure()
    {
        // Arrange
        var partNumber = "NONEXISTENT";
        var emptyList = new List<MasterLabel>();

        _masterLabelRepository.ListAsync(Arg.Any<Specification<MasterLabel>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<MasterLabel>>.Success(emptyList));

        // Act
        var result = await _service.GetMasterLabelByPartNumberAsync(partNumber, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("No master labels found matching the criteria");
    }

    [Fact]
    public async Task GetMasterLabelByPartNumberAsync_WithRepositoryFailure_ShouldReturnFailure()
    {
        // Arrange
        var partNumber = "TEST123";

        _masterLabelRepository.ListAsync(Arg.Any<Specification<MasterLabel>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<MasterLabel>>.WithFailure("Database connection failed"));

        // Act
        var result = await _service.GetMasterLabelByPartNumberAsync(partNumber, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain("No master labels found matching the criteria");
    }

    [Fact]
    public async Task GetMasterLabelByPartNumberAsync_WithException_ShouldReturnFailure()
    {
        // Arrange
        var partNumber = "TEST123";

        // Act
        var result = await _service.GetMasterLabelByPartNumberAsync(partNumber, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Error retrieving master labels"));
    }
}