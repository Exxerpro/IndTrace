// <copyright file="IdentityRedirectManager.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Components;

namespace IndTrace.Monitor.Components.Account;

/// <summary>
/// Manages redirects for Identity components with support for status messages and secure navigation.
/// </summary>
internal sealed class IdentityRedirectManager(NavigationManager navigationManager)
{
    /// <summary>
    /// The name of the cookie used to store status messages.
    /// </summary>
    public const string StatusCookieName = "Identity.StatusMessage";

    private static readonly CookieBuilder StatusCookieBuilder = new()
    {
        SameSite = SameSiteMode.Strict,
        HttpOnly = true,
        IsEssential = true,
        MaxAge = TimeSpan.FromSeconds(5),
    };

    /// <summary>
    /// Redirects to the specified URI with protection against open redirects.
    /// </summary>
    /// <param name="uri">The URI to redirect to.</param>
    [DoesNotReturn]
    public void RedirectTo(string? uri)
    {
        uri ??= string.Empty;

        // Prevent open redirects.
        if (!Uri.IsWellFormedUriString(uri, UriKind.Relative))
        {
            uri = navigationManager.ToBaseRelativePath(uri);
        }

        // During static rendering, NavigateTo throws a NavigationException which is handled by the framework as a redirect.
        // So as long as this is called from a statically rendered Identity component, the InvalidOperationException is never thrown.
        navigationManager.NavigateTo(uri);
        throw new InvalidOperationException($"{nameof(IdentityRedirectManager)} can only be used during static rendering.");
    }

    /// <summary>
    /// Redirects to the specified URI with query parameters.
    /// </summary>
    /// <param name="uri">The URI to redirect to.</param>
    /// <param name="queryParameters">The query parameters to include in the redirect.</param>
    [DoesNotReturn]
    public void RedirectTo(string uri, Dictionary<string, object?> queryParameters)
    {
        var uriWithoutQuery = navigationManager.ToAbsoluteUri(uri).GetLeftPart(UriPartial.Path);
        var newUri = navigationManager.GetUriWithQueryParameters(uriWithoutQuery, queryParameters);
        this.RedirectTo(newUri);
    }

    /// <summary>
    /// Redirects to the specified URI with a status message cookie.
    /// </summary>
    /// <param name="uri">The URI to redirect to.</param>
    /// <param name="message">The status message to include in the cookie.</param>
    /// <param name="context">The HTTP context for setting the cookie.</param>
    [DoesNotReturn]
    public void RedirectToWithStatus(string uri, string message, HttpContext context)
    {
        context.Response.Cookies.Append(StatusCookieName, message, StatusCookieBuilder.Build(context));
        this.RedirectTo(uri);
    }

    /// <summary>
    /// Gets the current path of the navigation manager.
    /// </summary>
    private string CurrentPath => navigationManager.ToAbsoluteUri(navigationManager.Uri).GetLeftPart(UriPartial.Path);

    /// <summary>
    /// Redirects to the current page.
    /// </summary>
    [DoesNotReturn]
    public void RedirectToCurrentPage() => this.RedirectTo(this.CurrentPath);

    /// <summary>
    /// Redirects to the current page with a status message cookie.
    /// </summary>
    /// <param name="message">The status message to include in the cookie.</param>
    /// <param name="context">The HTTP context for setting the cookie.</param>
    [DoesNotReturn]
    public void RedirectToCurrentPageWithStatus(string message, HttpContext context)
        => this.RedirectToWithStatus(this.CurrentPath, message, context);
}