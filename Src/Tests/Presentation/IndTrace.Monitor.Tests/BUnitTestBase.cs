// <copyright file="BUnitTestBase.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Bunit;
using Shouldly;

namespace IndTrace.Monitor.Tests;

/// <summary>
/// Base class for BUnit component tests providing common setup and utilities.
/// </summary>
public abstract class BUnitTestBase : IDisposable
{
    private BunitContext? _context;

    /// <summary>
    /// Gets or creates the BUnit context for the current test.
    /// </summary>
    protected BunitContext Context => _context ??= CreateContext();

    /// <summary>
    /// Creates a fresh BUnit context for each test to avoid cross-test contamination.
    /// </summary>
    private BunitContext CreateContext()
    {
        var context = new BunitContext();
        
        // Configure services for Blazor component testing
        context.Services.AddLogging();
        // Register a test NavigationManager to enable navigation-related tests
        context.Services.AddSingleton<NavigationManager>(new TestNavigationManager());

        // Add any additional services your components need
        // context.Services.AddScoped<IMyService, MyService>();
        
        return context;
    }

    /// <summary>
    /// Renders a component and returns the rendered component for testing.
    /// </summary>
    /// <typeparam name="TComponent">The component type to render.</typeparam>
    /// <returns>The rendered component instance.</returns>
    protected IRenderedComponent<TComponent> RenderComponent<TComponent>(Action<ComponentParameterCollectionBuilder<TComponent>>? parameterBuilder = null)
        where TComponent : IComponent
    {
        return Context.Render<TComponent>(parameterBuilder);
    }

    /// <summary>
    /// Asserts that a component renders without throwing exceptions.
    /// </summary>
    /// <typeparam name="TComponent">The component type to test.</typeparam>
    protected void Component_ShouldRenderWithoutErrors<TComponent>(Action<ComponentParameterCollectionBuilder<TComponent>>? parameterBuilder = null)
        where TComponent : IComponent
    {
        // Act & Assert
        Should.NotThrow(() => RenderComponent<TComponent>(parameterBuilder));
    }

    /// <summary>
    /// Asserts that a component contains specific text content.
    /// </summary>
    /// <typeparam name="TComponent">The component type to test.</typeparam>
    /// <param name="expectedText">The expected text to find in the component.</param>

    protected void Component_ShouldContainText<TComponent>(string expectedText, Action<ComponentParameterCollectionBuilder<TComponent>>? parameterBuilder = null)
        where TComponent : IComponent
    {
        // Act
        var component = RenderComponent<TComponent>(parameterBuilder);

        // Assert
        component.Markup.ShouldContain(expectedText);
    }

    /// <summary>
    /// Asserts that a component contains a specific CSS class.
    /// </summary>
    /// <typeparam name="TComponent">The component type to test.</typeparam>
    /// <param name="expectedClass">The expected CSS class to find.</param>

    protected void Component_ShouldHaveClass<TComponent>(string expectedClass, Action<ComponentParameterCollectionBuilder<TComponent>>? parameterBuilder = null)
        where TComponent : IComponent
    {
        // Act
        var component = RenderComponent<TComponent>(parameterBuilder);

        // Assert
        component.Markup.ShouldContain($"class=\"{expectedClass}\"");
    }

    public void Dispose()
    {
        _context?.Dispose();
        _context = null;
    }
}

/// <summary>
/// Minimal test implementation of NavigationManager for bUnit tests.
/// Tracks navigation by updating the current <see cref="NavigationManager.Uri"/>.
/// </summary>
internal sealed class TestNavigationManager : NavigationManager
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestNavigationManager"/> class.
    /// </summary>
    public TestNavigationManager()
    {
        Initialize("http://localhost/", "http://localhost/");
    }

    /// <summary>
    /// Core navigation handler used by Blazor to perform navigation.
    /// Updates the current Uri to the absolute form of the requested uri.
    /// </summary>
    /// <param name="uri">The destination URI (relative or absolute).</param>
    /// <param name="forceLoad">Whether to force a full page load.</param>
    protected override void NavigateToCore(string uri, bool forceLoad)
    {
        Uri = ToAbsoluteUri(uri).ToString();
    }
}