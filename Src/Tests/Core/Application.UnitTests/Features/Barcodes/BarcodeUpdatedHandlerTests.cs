// <copyright file="BarcodeUpdatedHandlerTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using static IndTrace.Application.BarCodes.Commands.Update.BarCodeUpdated;

namespace Application.UnitTests.Features.Barcodes;

/// <summary>
/// Unit tests for BarcodeUpdatedHandler
/// </summary>
public class BarcodeUpdatedHandlerTests
{
    /// <summary>
    /// Executes Constructor_WithValidParameters_ShouldCreateInstance operation.
    /// </summary>
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var notificationService = Substitute.For<INotificationService>();
        // Act
        var instance = new BarcodeUpdatedHandler(notificationService);
        // Assert
        instance.ShouldNotBeNull();
    }
}