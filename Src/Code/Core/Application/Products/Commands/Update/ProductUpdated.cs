// <copyright file="ProductUpdated.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Products.Commands.Update;

/// <summary>
/// Represents the ProductUpdated.
/// </summary>
public class ProductUpdated : INotification
{
#nullable enable

    /// <summary>
    /// Gets or sets the ProductId.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets the PartNumber.
    /// </summary>
    public string PartNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ProductName.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Product.
    /// </summary>
    public string Product { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets the IsActive.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the Version.
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Gets or sets the CustomerPartNumber.
    /// </summary>
    public string CustomerPartNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the AliasPartNumber.
    /// </summary>
    public string AliasPartNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Returns a string representation of the product updated notification.
    /// </summary>
    /// <returns>A formatted string containing the product updated details.</returns>
    public override string ToString()
    {
        // [Fix]
        // CLAUDE
        // Date: 23/08/2025
        // Reason: Added meaningful ToString() method for better debugging and logging in MessageDto factory
        return $"Product Updated - ProductId: {this.ProductId}, PartNumber: {this.PartNumber}, ProductName: {this.ProductName}";
    }

    /// <summary>
    /// Represents the ProductUpdatedHandler.
    /// </summary>
    public class ProductUpdatedHandler(INotificationService notification) : Models.Interfaces.INotificationHandler<ProductUpdated>
    {
        /// <summary>
        /// Executes Process operation.
        /// </summary>
        /// <param name="notification1">The notification1.</param>
        /// <param name="cancellationToken">The cancellationToken.</param>
        /// <returns>The result of Process.</returns>
        public async Task<Result> Process(ProductUpdated notification1, CancellationToken cancellationToken)
        {
            // [Fix]
            // CLAUDE
            // Date: 23/08/2025
            // Reason: Added null check for notification1 parameter to ensure Railway-Oriented Programming compliance
            if (notification1 is null)
            {
                return Result.WithFailure(["Notification cannot be null."]);
            }

            // [Fix]
            // CLAUDE
            // Date: 23/08/2025
            // Reason: INotification refactoring - use MessageDto.CreateMessage<T> factory method instead of new MessageDto()
            if (cancellationToken.IsCancellationRequested)
            {
                return Result.WithFailure(["Operation was canceled."]);
            }

            try
            {
                return await notification
                    .SendAsync(MessageDto.CreateMessage<ProductUpdated>(notification1), cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Result.WithFailure([$"Operation finished with an exception {ex.Message}"]);
            }
        }
    }
}
