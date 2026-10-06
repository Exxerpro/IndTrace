// <copyright file="HttpClientInterceptor.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Dependencies.Services;

/// <summary>
/// HTTP client interceptor that handles authentication and logging for outgoing HTTP requests.
/// </summary>
/// <param name="logger">Logger instance for the interceptor.</param>
public class HttpClientInterceptor(ILogger<HttpClientInterceptor> logger) : DelegatingHandler
{
    /// <summary>
    /// Intercepts HTTP requests to add authentication headers and log request/response information.
    /// </summary>
    /// <param name="request">The HTTP request message.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation that returns the HTTP response.</returns>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Log the monitorRequest
        logger.LogInformation("Sending monitorRequest to {Url}", request.RequestUri);

        // Add authorization header if needed
        if (request.Headers.Authorization == null)
        {
            var token = await this.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        var response = await base.SendAsync(request, cancellationToken);

        // Log the response
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Response error: {StatusCode} - {Reason}", response.StatusCode, response.ReasonPhrase);
        }

        return response;
    }

    /// <summary>
    /// Gets the authentication token for the HTTP request.
    /// </summary>
    /// <returns>A task representing the asynchronous operation that returns the token string.</returns>
    private Task<string> GetTokenAsync()
    {
        // Implement your logic to get the token, e.g., from local storage or a token service
        return Task.FromResult(string.Empty);
    }
}