// <copyright file="DiagnosticsController.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Microsoft.AspNetCore.Mvc;

namespace IndTrace.Dependencies.Diagnostics;

/// <summary>
/// API controller for diagnostics, providing endpoints to inspect registered services.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class DiagnosticsController : ControllerBase
{
    private readonly IServiceProvider serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiagnosticsController"/> class with the specified service provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve services.</param>
    public DiagnosticsController(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Gets a list of all registered services in the application.
    /// </summary>
    /// <returns>An HTML content result listing all registered services.</returns>
    [HttpGet("services")]
    public IActionResult GetRegisteredServices()
    {
        var serviceDescriptors = this.serviceProvider.GetService<IServiceCollection>()?.ToList() ?? new List<ServiceDescriptor>();

        StringBuilder sb = new StringBuilder();
        sb.Append("<html><body><h1>Registered Services</h1><ul>");

        foreach (var serviceDescriptor in serviceDescriptors)
        {
            sb.Append("<li>");
            sb.Append($"{serviceDescriptor.ServiceType.FullName}");
            if (serviceDescriptor.ImplementationType != null)
            {
                sb.Append($" - {serviceDescriptor.ImplementationType.FullName}");
            }
            sb.Append($" (Lifetime: {serviceDescriptor.Lifetime})");
            sb.Append("</li>");
        }

        sb.Append("</ul></body></html>");

        return new ContentResult
        {
            Content = sb.ToString(),
            ContentType = "text/html",
        };
    }
}