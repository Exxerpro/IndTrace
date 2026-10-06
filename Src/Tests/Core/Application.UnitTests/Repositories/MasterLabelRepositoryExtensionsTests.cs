// <copyright file="MasterLabelRepositoryExtensionsTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Application.UnitTests.Repositories;

/// <summary>
/// Unit tests for MasterLabelRepositoryExtensions static extension methods
/// </summary>
public class MasterLabelRepositoryExtensionsTests
{
    private readonly IReadOnlyRepository<MasterLabel> _masterLabelRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="MasterLabelRepositoryExtensionsTests"/> class.
    /// </summary>
    public MasterLabelRepositoryExtensionsTests()
    {
        _masterLabelRepository = Substitute.For<IReadOnlyRepository<MasterLabel>>();
    }

    /// <summary>
    /// Tests GetMasterLabelByPartNumberAsyncFromDb returns success with the label codes when labels exist (issue #110)
    /// </summary>
    [Fact]
    public async Task GetMasterLabelByPartNumberAsyncFromDb_WithMatchingLabels_ShouldReturnSuccess()
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
        var result = await _masterLabelRepository.GetMasterLabelByPartNumberAsyncFromDb(partNumber, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count.ShouldBe(2);
        result.Value.ShouldContain("LABEL1_TEST123");
        result.Value.ShouldContain("LABEL2_TEST123");
    }

    /// <summary>
    /// Tests GetMasterLabelByPartNumberAsyncFromDb returns failure when the repository returns a failed result (guards the guard direction for issue #110)
    /// </summary>
    [Fact]
    public async Task GetMasterLabelByPartNumberAsyncFromDb_WithRepositoryFailure_ShouldReturnFailure()
    {
        // Arrange
        var partNumber = "TEST123";

        _masterLabelRepository.ListAsync(Arg.Any<Specification<MasterLabel>>(), Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<MasterLabel>>.WithFailure("Database connection failed"));

        // Act
        var result = await _masterLabelRepository.GetMasterLabelByPartNumberAsyncFromDb(partNumber, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("No master labels found matching the criteria");
    }
}
