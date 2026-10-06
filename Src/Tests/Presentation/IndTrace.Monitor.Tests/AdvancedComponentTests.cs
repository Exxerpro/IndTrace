// <copyright file="AdvancedComponentTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace IndTrace.Monitor.Tests;

/// <summary>
/// Advanced BUnit tests demonstrating component interactions, events, and complex scenarios.
/// </summary>
public class AdvancedComponentTests : BUnitTestBase
{
    /// <summary>
    /// Executes Component_ShouldHandleUserInteractions operation.
    /// </summary>
    [Fact]
    public void Component_ShouldHandleUserInteractions()
    {
        // Arrange
        var component = RenderComponent<InteractiveComponent>();

        // Act - Click a button
        var button = component.Find("button");
        button.Click();

        // Assert
        component.Find(".result").TextContent.ShouldContain("Clicked");
    }

    /// <summary>
    /// Executes Component_ShouldUpdateOnParameterChanges operation.
    /// </summary>

    [Fact]
    public void Component_ShouldUpdateOnParameterChanges()
    {
        // Arrange
        var component = RenderComponent<ParameterizedComponent>(parameters => parameters
            .Add(p => p.Title, "Initial Title"));

        // Assert initial state
        component.Find("h1").TextContent.ShouldBe("Initial Title");

        // Act - Re-render component with updated parameters
        var updatedComponent = RenderComponent<ParameterizedComponent>(parameters => parameters
            .Add(p => p.Title, "Updated Title"));

        // Assert updated state
        updatedComponent.Find("h1").TextContent.ShouldBe("Updated Title");
    }

    /// <summary>
    /// Executes Component_ShouldEmitEvents operation.
    /// </summary>

    [Fact]
    public void Component_ShouldEmitEvents()
    {
        // Arrange
        var onSaveCalled = false;
        var component = RenderComponent<EventComponent>(parameters => parameters
            .Add(p => p.OnSave, () => onSaveCalled = true));

        // Act
        component.Find("button[data-testid='save']").Click();

        // Assert
        onSaveCalled.ShouldBeTrue();
    }

    /// <summary>
    /// Executes Component_ShouldHandleAsyncOperations operation.
    /// </summary>

    [Fact]
    public void Component_ShouldHandleAsyncOperations()
    {
        // Arrange
        var component = RenderComponent<AsyncComponent>();

        // Act
        component.Find("button[data-testid='load']").Click();

        // Assert - Wait for async operation
        component.WaitForElement(".loading", TimeSpan.FromSeconds(5));
        component.WaitForElement(".loaded", TimeSpan.FromSeconds(5));

        component.Find(".loaded").TextContent.ShouldContain("Data loaded");
    }

    /// <summary>
    /// Executes Component_ShouldValidateFormInputs operation.
    /// </summary>

    [Fact]
    public void Component_ShouldValidateFormInputs()
    {
        // Arrange
        var component = RenderComponent<FormComponent>();

        // Act - Submit without required field
        component.Find("form").Submit();

        // Assert
        component.Find(".validation-error").TextContent.ShouldContain("Required");

        // Act - Fill required field
        component.Find("input[name='name']").Change("Test Name");
        component.Find("form").Submit();

        // Assert - No validation errors
        component.FindAll(".validation-error").ShouldBeEmpty();
    }

    /// <summary>
    /// Executes Component_ShouldHandleNavigation operation.
    /// </summary>

    [Fact]
    public void Component_ShouldHandleNavigation()
    {
        // Arrange
        var navManager = Context.Services.GetRequiredService<NavigationManager>();
        var component = RenderComponent<NavigationComponent>();

        // Act
        component.Find("a[href='/monitor']").Click();

        // Assert
        navManager.Uri.ShouldContain("/monitor");
    }
}

// Example component implementations for testing
/// <summary>
/// Represents the InteractiveComponent.
/// </summary>

public class InteractiveComponent : ComponentBase
{
    private bool isClicked = false;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.OpenElement(1, "button");
        builder.AddAttribute(2, "onclick", EventCallback.Factory.Create(this, () => isClicked = true));
        builder.AddContent(3, "Click me");
        builder.CloseElement();

        if (isClicked)
        {
            builder.OpenElement(4, "div");
            builder.AddAttribute(5, "class", "result");
            builder.AddContent(6, "Button was clicked!");
            builder.CloseElement();
        }
        builder.CloseElement();
    }
}

/// <summary>
/// Represents the ParameterizedComponent.
/// </summary>

public class ParameterizedComponent : ComponentBase
{
    [Parameter] public string Title { get; set; } = string.Empty;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "h1");
        builder.AddContent(1, Title);
        builder.CloseElement();
    }
}

/// <summary>
/// Represents the EventComponent.
/// </summary>

public class EventComponent : ComponentBase
{
    [Parameter] public Action OnSave { get; set; } = () => { };

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "button");
        builder.AddAttribute(1, "data-testid", "save");
        builder.AddAttribute(2, "onclick", EventCallback.Factory.Create(this, OnSave));
        builder.AddContent(3, "Save");
        builder.CloseElement();
    }
}

/// <summary>
/// Represents the AsyncComponent.
/// </summary>

public class AsyncComponent : ComponentBase
{
    private bool isLoading = false;
    private bool isLoaded = false;

    private async Task LoadDataAsync()
    {
        isLoading = true;
        StateHasChanged();

        await Task.Delay(100); // Simulate async operation

        isLoading = false;
        isLoaded = true;
        StateHasChanged();
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");

        builder.OpenElement(1, "button");
        builder.AddAttribute(2, "data-testid", "load");
        builder.AddAttribute(3, "onclick", EventCallback.Factory.Create(this, LoadDataAsync));
        builder.AddContent(4, "Load Data");
        builder.CloseElement();

        if (isLoading)
        {
            builder.OpenElement(5, "div");
            builder.AddAttribute(6, "class", "loading");
            builder.AddContent(7, "Loading...");
            builder.CloseElement();
        }

        if (isLoaded)
        {
            builder.OpenElement(8, "div");
            builder.AddAttribute(9, "class", "loaded");
            builder.AddContent(10, "Data loaded successfully");
            builder.CloseElement();
        }

        builder.CloseElement();
    }
}

/// <summary>
/// Represents the FormComponent.
/// </summary>

public class FormComponent : ComponentBase
{
    private string name = string.Empty;
    private bool hasError = false;

    private void HandleSubmit()
    {
        hasError = string.IsNullOrWhiteSpace(name);
        StateHasChanged();
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "form");
        builder.AddAttribute(1, "onsubmit", EventCallback.Factory.Create(this, HandleSubmit));

        builder.OpenElement(2, "input");
        builder.AddAttribute(3, "name", "name");
        builder.AddAttribute(4, "value", name);
        builder.AddAttribute(5, "onchange", EventCallback.Factory.CreateBinder(this, value => name = value ?? string.Empty, name));
        builder.CloseElement();

        if (hasError)
        {
            builder.OpenElement(6, "div");
            builder.AddAttribute(7, "class", "validation-error");
            builder.AddContent(8, "Name is required");
            builder.CloseElement();
        }

        builder.OpenElement(9, "button");
        builder.AddAttribute(10, "type", "submit");
        builder.AddContent(11, "Submit");
        builder.CloseElement();

        builder.CloseElement();
    }
}

/// <summary>
/// Represents the NavigationComponent.
/// </summary>

public class NavigationComponent : ComponentBase
{
    [Inject] private NavigationManager? Navigation { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "nav");
        builder.OpenElement(1, "a");
        builder.AddAttribute(2, "href", "/monitor");
        builder.AddAttribute(3, "onclick", EventCallback.Factory.Create(this, () => Navigation?.NavigateTo("/monitor")));
        builder.AddContent(4, "Monitor");
        builder.CloseElement();
        builder.CloseElement();
    }
}