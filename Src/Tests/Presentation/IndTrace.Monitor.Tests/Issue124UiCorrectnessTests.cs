// <copyright file="Issue124UiCorrectnessTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.Commands.Reject;
using IndTrace.Application.ConfigStations.Queries.GetConfigStationList;
using IndTrace.Application.Configuration.Services;
using IndTrace.Application.Customers;
using IndTrace.Application.Machines.Queries.GetMachinesList;
using IndTrace.Application.Products.Queries.GetProductDetail;
using IndTrace.Components.Area.BarCode;
using IndTrace.Components.Area.Machines;
using IndTrace.Components.Area.Products;
using IndTrace.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;

namespace IndTrace.Monitor.Tests;

/// <summary>
/// Component tests for the issue #124 chunk B UI-correctness fixes (F14): <see cref="BarCodeReject"/> must render a
/// genuine three-state UI — no alert before any action, the real dispatcher errors on failure, and the rejected
/// barcode details on success. Before the fix the "No BarCode Found" error alert rendered in the initial state and a
/// real failure was indistinguishable from "no action yet".
/// </summary>
public sealed class BarCodeRejectThreeStateTests : IDisposable
{
    private readonly BunitContext context;
    private readonly IMonitorRequestDispatcher dispatcher = Substitute.For<IMonitorRequestDispatcher>();

    /// <summary>
    /// Initializes a new instance of the <see cref="BarCodeRejectThreeStateTests"/> class: MudBlazor services for the
    /// alert/card chrome, loose JS interop, an authorized principal (the Reject button sits inside an
    /// <c>AuthorizeView</c>), and a substituted monitor dispatcher.
    /// </summary>
    public BarCodeRejectThreeStateTests()
    {
        this.context = new BunitContext();
        this.context.Services.AddLogging();
        this.context.Services.AddMudServices();
        this.context.JSInterop.Mode = JSRuntimeMode.Loose;
        this.context.AddAuthorization().SetAuthorized("operator");
        this.context.Services.AddSingleton(this.dispatcher);
    }

    /// <summary>
    /// Before any action neither the failure alert nor the success alert renders — in particular the legacy
    /// pre-action "No BarCode Found" error is gone.
    /// </summary>
    [Fact]
    public void InitialRender_ShowsNoOutcomeAlert()
    {
        var cut = this.context.Render<BarCodeReject>(p => p.Add(x => x.BarCode, "BC-1"));

        cut.FindAll("[data-testid='reject-failure']").Count.ShouldBe(0);
        cut.FindAll("[data-testid='reject-success']").Count.ShouldBe(0);
        cut.Markup.ShouldNotContain("No BarCode Found");
    }

    /// <summary>
    /// A failed reject renders the error alert carrying the dispatcher's actual errors, and no success alert.
    /// </summary>
    [Fact]
    public void FailedReject_RendersErrorAlertWithRealErrors()
    {
        this.dispatcher
            .QueryAsync(Arg.Any<IMonitorRequest<BarCodeRejectedView>>(), Arg.Any<CancellationToken>())
            .Returns(Result<BarCodeRejectedView>.WithFailure(["BarCode not found BC-1"]));

        var cut = this.context.Render<BarCodeReject>(p => p.Add(x => x.BarCode, "BC-1"));

        cut.FindAll("button").Single(b => b.TextContent.Contains("Reject", StringComparison.Ordinal)).Click();

        cut.WaitForAssertion(() =>
        {
            var alert = cut.Find("[data-testid='reject-failure']");
            alert.TextContent.ShouldContain("BarCode not found BC-1");
            cut.FindAll("[data-testid='reject-success']").Count.ShouldBe(0);
        });
    }

    /// <summary>
    /// A successful reject renders the success alert and the rejected barcode details.
    /// </summary>
    [Fact]
    public void SuccessfulReject_RendersSuccessAlertAndDetails()
    {
        var view = new BarCodeRejectedView
        {
            Label = "BC-1",
            MachineId = 42,
        };
        this.dispatcher
            .QueryAsync(Arg.Any<IMonitorRequest<BarCodeRejectedView>>(), Arg.Any<CancellationToken>())
            .Returns(Result<BarCodeRejectedView>.Success(view));

        var cut = this.context.Render<BarCodeReject>(p => p.Add(x => x.BarCode, "BC-1"));

        cut.FindAll("button").Single(b => b.TextContent.Contains("Reject", StringComparison.Ordinal)).Click();

        cut.WaitForAssertion(() =>
        {
            cut.Find("[data-testid='reject-success']").TextContent.ShouldContain("BarCode Rejected");
            cut.FindAll("[data-testid='reject-failure']").Count.ShouldBe(0);
            cut.Markup.ShouldContain("BC-1");
        });
    }

    /// <inheritdoc/>
    public void Dispose() => this.context.Dispose();
}

/// <summary>
/// Component tests for the issue #124 chunk B fix (F5, SAFETY): <see cref="MachineDetails"/> must never toast success
/// when the traceability-gate toggle itself failed. Before the fix the toggle Result was discarded and the snackbar
/// only reflected the follow-up configuration fetch.
/// </summary>
public sealed class MachineDetailsToggleFailureTests : IAsyncDisposable
{
    private readonly BunitContext context;
    private readonly IMonitorRequestDispatcher dispatcher = Substitute.For<IMonitorRequestDispatcher>();
    private readonly ISnackbar snackbar = Substitute.For<ISnackbar>();

    /// <summary>
    /// Initializes a new instance of the <see cref="MachineDetailsToggleFailureTests"/> class. The component is a
    /// MudDialog, so tests open it through the real <see cref="IDialogService"/> against a rendered
    /// <see cref="MudDialogProvider"/>. The substituted <see cref="ISnackbar"/> is registered after
    /// <c>AddMudServices</c> so it wins resolution.
    /// </summary>
    public MachineDetailsToggleFailureTests()
    {
        this.context = new BunitContext();
        this.context.Services.AddLogging();
        this.context.Services.AddMudServices();
        this.context.JSInterop.Mode = JSRuntimeMode.Loose;
        this.context.AddAuthorization().SetAuthorized("operator");

        this.context.Services.AddSingleton(this.dispatcher);
        this.context.Services.AddSingleton(this.snackbar);

        // The component injects the concrete IndTraceConfigurationService; feed it a substituted handler whose
        // refresh succeeds, so any success/failure signal in the test comes from the TOGGLE result alone.
        var appDetailsHandler = Substitute.For<IMonitorRequestHandler<GetAppDetailsMonitorRequest, ApplicationConfiguration>>();
        appDetailsHandler
            .ProcessAsync(Arg.Any<GetAppDetailsMonitorRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<ApplicationConfiguration>.Success(new ApplicationConfiguration()));
        this.context.Services.AddSingleton(new IndTraceConfigurationService(appDetailsHandler));
    }

    /// <summary>
    /// A failed enable toggle must produce an error snackbar carrying the dispatcher errors and must NOT claim
    /// success — the pre-fix behavior showed "Enabled successfully!" because the follow-up config fetch succeeded.
    /// </summary>
    [Fact]
    public async Task FailedToggle_ShowsErrorSnackbar_NeverSuccess()
    {
        this.dispatcher
            .ProcessAsync(Arg.Any<IMonitorRequest<MachineDto>>(), Arg.Any<CancellationToken>())
            .Returns(Result<MachineDto>.WithFailure(["PLC rejected the toggle"]));

        var provider = this.context.Render<MudDialogProvider>();
        var dialogService = this.context.Services.GetRequiredService<IDialogService>();

        // EnableAppTraceability=0 / EnableBypassTraceability=1 is the exact "disabled" combination of the two-flag
        // gate, so the dialog offers the Enable submit.
        var machine = new MachineDto
        {
            MachineId = 7,
            Name = "Weld",
            EnableAppTraceability = 0,
            EnableBypassTraceability = 1,
        };
        var parameters = new DialogParameters { { "Machine", machine } };

        await provider.InvokeAsync(() => dialogService.ShowAsync<MachineDetails>("Machine 7", parameters));

        var submit = provider.FindAll("button").First(b => b.TextContent.Trim() == "Submit");
        await submit.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        provider.WaitForAssertion(() =>
        {
            this.snackbar.Received().Add(
                Arg.Is<string>(s => s.Contains("Failed to Enable machine 7", StringComparison.Ordinal) && s.Contains("PLC rejected the toggle", StringComparison.Ordinal)),
                Severity.Error,
                Arg.Any<Action<SnackbarOptions>>(),
                Arg.Any<string>());

            this.snackbar.DidNotReceive().Add(
                Arg.Is<string>(s => s.Contains("successfully", StringComparison.OrdinalIgnoreCase)),
                Arg.Any<Severity>(),
                Arg.Any<Action<SnackbarOptions>>(),
                Arg.Any<string>());
        });
    }

    /// <summary>
    /// A successful toggle followed by a successful configuration refresh still toasts success (happy path kept).
    /// </summary>
    [Fact]
    public async Task SuccessfulToggle_ShowsSuccessSnackbar()
    {
        this.dispatcher
            .ProcessAsync(Arg.Any<IMonitorRequest<MachineDto>>(), Arg.Any<CancellationToken>())
            .Returns(Result<MachineDto>.Success(new MachineDto { MachineId = 7 }));

        var provider = this.context.Render<MudDialogProvider>();
        var dialogService = this.context.Services.GetRequiredService<IDialogService>();

        var machine = new MachineDto
        {
            MachineId = 7,
            Name = "Weld",
            EnableAppTraceability = 0,
            EnableBypassTraceability = 1,
        };
        var parameters = new DialogParameters { { "Machine", machine } };

        await provider.InvokeAsync(() => dialogService.ShowAsync<MachineDetails>("Machine 7", parameters));

        var submit = provider.FindAll("button").First(b => b.TextContent.Trim() == "Submit");
        await submit.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        provider.WaitForAssertion(() =>
        {
            this.snackbar.Received().Add(
                Arg.Is<string>(s => s.Contains("Enabled successfully", StringComparison.Ordinal)),
                Severity.Success,
                Arg.Any<Action<SnackbarOptions>>(),
                Arg.Any<string>());

            this.snackbar.DidNotReceive().Add(
                Arg.Any<string>(),
                Severity.Error,
                Arg.Any<Action<SnackbarOptions>>(),
                Arg.Any<string>());
        });
    }

    /// <inheritdoc/>
    /// <remarks>MudDialogProvider pulls in MudBlazor services that only implement IAsyncDisposable.</remarks>
    public ValueTask DisposeAsync() => this.context.DisposeAsync();
}

/// <summary>
/// Component tests for the issue #124 chunk B fix (F12): <see cref="AddProduct.SaveNewModel"/> is a button handler
/// and must never throw — a thrown exception kills the Blazor circuit. Duplicate customer names, a missing
/// configuration, and a not-found customer must all surface as user-visible error messages instead.
/// </summary>
public sealed class AddProductSaveNoThrowTests : IDisposable
{
    private readonly BunitContext context;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddProductSaveNoThrowTests"/> class with the MudBlazor services
    /// the component chrome needs.
    /// </summary>
    public AddProductSaveNoThrowTests()
    {
        this.context = new BunitContext();
        this.context.Services.AddLogging();
        this.context.Services.AddMudServices();
        this.context.JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private static ProductDto NewProductFor(string customerName) => new()
    {
        ProductName = "Widget",
        PartNumber = "PN-1",
        CustomerName = customerName,
    };

    /// <summary>
    /// Two customers sharing the saved name previously blew up in <c>SingleOrDefault</c> with
    /// <see cref="InvalidOperationException"/>; now the save aborts with a distinct error naming the customer and the
    /// product callback is never invoked.
    /// </summary>
    [Fact]
    public async Task DuplicateCustomerName_DoesNotThrow_ShowsDistinctError()
    {
        var configuration = new ApplicationConfiguration
        {
            Customers =
            [
                new CustomerDto { CustomerId = 1, Name = "Acme" },
                new CustomerDto { CustomerId = 2, Name = "Acme" },
            ],
        };

        var added = false;
        var cut = this.context.Render<AddProduct>(p => p
            .Add(x => x.NewProduct, NewProductFor("Acme"))
            .Add(x => x.ApplicationConfiguration, configuration)
            .Add(x => x.OnProductAdded, _ =>
            {
                added = true;
                return Task.CompletedTask;
            }));

        await cut.InvokeAsync(() => cut.Instance.SaveNewModel());

        added.ShouldBeFalse();
        cut.Instance.Message.ShouldContain("more than one customer is named 'Acme'");
    }

    /// <summary>
    /// A customer name with no match previously fell through to a throwing not-found path; now it surfaces a
    /// user-visible error and the product callback is never invoked.
    /// </summary>
    [Fact]
    public async Task UnknownCustomerName_DoesNotThrow_ShowsNotFoundError()
    {
        var configuration = new ApplicationConfiguration
        {
            Customers = [new CustomerDto { CustomerId = 1, Name = "Acme" }],
        };

        var added = false;
        var cut = this.context.Render<AddProduct>(p => p
            .Add(x => x.NewProduct, NewProductFor("Ghost"))
            .Add(x => x.ApplicationConfiguration, configuration)
            .Add(x => x.OnProductAdded, _ =>
            {
                added = true;
                return Task.CompletedTask;
            }));

        await cut.InvokeAsync(() => cut.Instance.SaveNewModel());

        added.ShouldBeFalse();
        cut.Instance.Message.ShouldContain("'Ghost' was not found");
    }

    /// <summary>
    /// A null configuration previously threw out of the button handler; now it surfaces a user-visible error.
    /// </summary>
    [Fact]
    public async Task NullConfiguration_DoesNotThrow_ShowsError()
    {
        var added = false;
        var cut = this.context.Render<AddProduct>(p => p
            .Add(x => x.NewProduct, NewProductFor("Acme"))
            .Add(x => x.OnProductAdded, _ =>
            {
                added = true;
                return Task.CompletedTask;
            }));

        await cut.InvokeAsync(() => cut.Instance.SaveNewModel());

        added.ShouldBeFalse();
        cut.Instance.Message.ShouldContain("configuration is not loaded");
    }

    /// <summary>
    /// The happy path is unchanged: exactly one matching customer resolves the id and invokes the save callback.
    /// </summary>
    [Fact]
    public async Task SingleMatchingCustomer_InvokesSaveWithResolvedCustomerId()
    {
        var configuration = new ApplicationConfiguration
        {
            Customers = [new CustomerDto { CustomerId = 7, Name = "Acme" }],
        };

        ProductDto? saved = null;
        var cut = this.context.Render<AddProduct>(p => p
            .Add(x => x.NewProduct, NewProductFor("Acme"))
            .Add(x => x.ApplicationConfiguration, configuration)
            .Add(x => x.OnProductAdded, product =>
            {
                saved = product;
                return Task.CompletedTask;
            }));

        await cut.InvokeAsync(() => cut.Instance.SaveNewModel());

        var savedProduct = saved.ShouldNotBeNull();
        savedProduct.Customer.CustomerId.ShouldBe(7);
        savedProduct.Customer.Name.ShouldBe("Acme");
    }

    /// <inheritdoc/>
    public void Dispose() => this.context.Dispose();
}
