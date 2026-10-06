// <copyright file="TemplateCommandHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Aggregation.BoundedTests.Templates;

// ============================================================================
// TEMPLATE TEST - DO NOT MODIFY
// Copy this file into your target slice (e.g., Settings/Commands) and rename.
// ============================================================================

public sealed class TemplateCommandHandlerTests : DependenciesFactory
{
    public TemplateCommandHandlerTests(ITestOutputHelper output) : base(output) { }

    [Fact(Skip = "TEMPLATE — DO NOT MODIFY. Copy into your slice and rename.")]
    public async Task CreateEntity_Persists_And_Returns_Id()
    {
        await Initialization;

        // Arrange
        // var logger = XUnitLogger.CreateLogger<CreateEntityCommandHandler>();
        // var sut = new CreateEntityCommandHandler(Dp<Entity>Repository, logger);
        // var cmd = new CreateEntityCommand { /* required fields */ };

        // Act
        // var result = await sut.ProcessAsync(cmd, TestContext.Current.CancellationToken);

        // Assert (example)
        // result.IsSuccess.ShouldBeTrue();
        // var saved = await Dp<Entity>Repository.GetAsync(result.Value.Id, TestContext.Current.CancellationToken);
        // saved.ShouldNotBeNull();
    }
}

