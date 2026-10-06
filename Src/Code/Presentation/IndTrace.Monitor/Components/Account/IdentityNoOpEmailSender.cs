// <copyright file="IdentityNoOpEmailSender.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using IndTrace.Monitor.Data;

namespace IndTrace.Monitor.Components.Account;

/// <summary>
/// A no-operation email sender implementation for Identity that does not send actual emails.
/// Remove the "else if (EmailSender is IdentityNoOpEmailSender)" block from RegisterConfirmation.razor after updating with a real implementation.
/// </summary>
internal sealed class IdentityNoOpEmailSender : IEmailSender<ApplicationUser>
{
    private readonly IEmailSender emailSender = new NoOpEmailSender();

    /// <summary>
    /// Sends a confirmation link email to the user.
    /// </summary>
    /// <param name="user">The user to send the email to.</param>
    /// <param name="email">The email address to send to.</param>
    /// <param name="confirmationLink">The confirmation link to include in the email.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
        this.emailSender.SendEmailAsync(email, "Confirm your email", $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");

    /// <summary>
    /// Sends a password reset link email to the user.
    /// </summary>
    /// <param name="user">The user to send the email to.</param>
    /// <param name="email">The email address to send to.</param>
    /// <param name="resetLink">The password reset link to include in the email.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
        this.emailSender.SendEmailAsync(email, "Reset your password", $"Please reset your password by <a href='{resetLink}'>clicking here</a>.");

    /// <summary>
    /// Sends a password reset code email to the user.
    /// </summary>
    /// <param name="user">The user to send the email to.</param>
    /// <param name="email">The email address to send to.</param>
    /// <param name="resetCode">The password reset code to include in the email.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
        this.emailSender.SendEmailAsync(email, "Reset your password", $"Please reset your password using the following code: {resetCode}");
}