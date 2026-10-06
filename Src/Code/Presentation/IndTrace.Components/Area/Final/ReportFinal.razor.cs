// <copyright file="ReportFinal.razor.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.BarCodes.Queries.GetBarCodeDetail;
using IndTrace.Application.Cycles;
using IndTrace.Application.Models.BarCodesUtilities;
using IndTrace.Devices.Scanning;
using IndTrace.Domain.Entities;
using IndTrace.Domain.Enum;
using IndTrace.Domain.Interfaces;
using IndTrace.Domain.ValueObjects;
using IndTrace.UI.Models.BarCodes;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using QRCoder;

namespace IndTrace.Components.Area.Final;

/// <summary>
/// Component for displaying the final report and barcode details, including QR code generation and cycle status.
/// </summary>

/// <summary>
/// Initializes a new instance of the <see cref="ReportFinal"/> class.
/// </summary>
/// <param name="barCodeReader">The barcode reader (a hardware driver, or the community null reader).</param>
/// <param name="logger">The logger instance.</param>
/// <param name="monitorRequestDispatcher">The Monitor command dispatcher.</param>
public partial class ReportFinal(
    IBarCodeReader barCodeReader,
    ILogger<ReportFinal> logger,
    IMonitorRequestDispatcher monitorRequestDispatcher) : IDisposable
{
    private IDisposable? barCodeSubscription;
    private bool disposed;

    /// <summary>
    /// Gets or sets the visual display mode for the report.
    /// </summary>
    public string Visual = "Visual";

    /// <summary>
    /// Gets the barcode model instance for managing barcode data.
    /// </summary>
    public readonly BarCodeModel BarCodeModel = new BarCodeModel();

    /// <summary>
    /// Generates a QR code from the current barcode label and returns it as a base64 data URI.
    /// </summary>
    /// <returns>A base64 encoded data URI representing the QR code image.</returns>
    public string GetQrCode()
    {
        //N40299054210082
        var qroCodeGenerator = new QRCodeGenerator();
        var barCodeImage = qroCodeGenerator.CreateQrCode(this.BarCodeModel.Label, QRCodeGenerator.ECCLevel.H, true);
        return barCodeImage.ToBase64DataUri();
    }

    /// <summary>
    /// Gets or sets the barcode detail result from the query operation.
    /// </summary>
    public BarCodeDetailVm? BarCodeDetailResult { get; set; }

    private Cycle? Cycle { get; set; }

    private CycleViewModel CycleView { get; set; } = new CycleViewModel();

    /// <summary>
    /// Gets or sets the result barcode string after processing.
    /// </summary>
    public string ResultBarCode { get; set; } = string.Empty;

    private string ScannedBarCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current status message for the report operation.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Handles asynchronous barcode reading and processing from the barcode reader.
    /// </summary>
    /// <param name="scannedBarCode">The barcode captured for THIS scan, taken from the emission's
    /// snapshot payload. Re-reading the shared mutable <c>barCodeReader.Result</c> at handling time lost
    /// the first of two back-to-back scans (the property was already overwritten).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task HandleBarCodeReadAsync(string? scannedBarCode)
    {
        if (string.IsNullOrWhiteSpace(scannedBarCode)) return;
        if (this.ScannedBarCode == scannedBarCode) return;
        this.ScannedBarCode = scannedBarCode;
        if (await this.FillQueryAsync(this.ScannedBarCode))
            await this.SendQueryAsync();
    }

    /// <summary>
    /// Handles the valid form submission for barcode processing.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task OnValidSubmit()
    {
        logger.LogInformation("OnValidSubmit {Label}", this.BarCodeModel.Label);

        if (string.IsNullOrWhiteSpace(this.BarCodeModel.Label)) return;

        if (await this.FillQueryAsync(this.BarCodeModel.Label))
            await this.SendQueryAsync();
    }

    /// <summary>
    /// Gets or sets a value indicating whether to display detailed values in the UI.
    /// </summary>
    public bool ShowValues { get; set; }

    /// <summary>
    /// Updates the heading display based on mouse click events.
    /// </summary>
    /// <param name="e">The mouse event arguments.</param>
    public void UpdateHeading(MouseEventArgs e)
    {
        this.ShowValues = !this.ShowValues;
    }

    /// <summary>
    /// Resets the report state and validates the provided barcode label before a query is sent.
    /// (Historically this also built a query object, but that query was dead code — see the note
    /// on machine-scoping below.)
    /// </summary>
    /// <param name="label">The barcode label to process.</param>
    /// <returns>A task that returns true if the label is valid and a query should be sent, false otherwise.</returns>
    public async Task<bool> FillQueryAsync(string? label)
    {
        this.ScannedBarCode = label ?? string.Empty;

        this.Status = string.Empty;
        this.ResultBarCode = string.Empty;
        this.BarCodeDetailResult = null;
        this.Cycle = null;

        if (!barCodeReader.ValidateLabel(this.ScannedBarCode))
        {
            this.Status = "BarCodeInvalid";
            await this.InvokeAsync(this.StateHasChanged);
            return false;
        }

        // The detail query (built and sent in SendQueryAsync) is deliberately unscoped by machine
        // pending a PO ruling on machine-scoping (#124). A dead query previously built here set a
        // hardcoded MachineId = 500 but was never sent anywhere.
        return true;
    }

    /// <summary>
    /// Sends the barcode query to the backend and processes the response.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SendQueryAsync()
    {
        try
        {
            var query = new GetBarCodeDetailQuery
            {
                BarCode = this.ScannedBarCode,
            };
            var vm = await monitorRequestDispatcher.QueryAsync(query);
            this.BarCodeDetailResult = vm.Value;
            if (this.BarCodeDetailResult is null)
            {
                this.Status = "No data returned";
                await this.InvokeAsync(this.StateHasChanged);
                return;
            }
            this.ResultBarCode = this.BarCodeDetailResult is not null && Equals(this.BarCodeDetailResult.ResultValidation, ResultValidation.Valid) ? this.ScannedBarCode : string.Empty;

            if (this.BarCodeDetailResult is not null && Equals(this.BarCodeDetailResult.ResultValidation, ResultValidation.Valid))
            {
                await this.InvokeAsync(this.StateHasChanged);
                return;
            }

            if (this.BarCodeDetailResult?.Cycles is null || this.BarCodeDetailResult.Cycles.Count == 0)
            {
                this.Status = "No cycles available";
                await this.InvokeAsync(this.StateHasChanged);
                return;
            }

            var lastCycle = this.BarCodeDetailResult.Cycles.Aggregate((curMax, x) => x.MachineId.Value > curMax.MachineId.Value ? x : curMax);

            // The displayed CycleStatus is a VIEW-MODEL value (the markup binds CycleView.CycleStatus, not the
            // domain entity's status — see ReportFinal.razor). Default it to the entity's current status; the
            // re-print branch below overrides it without touching the domain entity.
            var displayCycleStatus = lastCycle.CycleStatus;

            if (lastCycle.CycleStatus != CycleStatus.FinishedOk && lastCycle.PartStatus == PartStatus.Ok)
            {
                lastCycle.MachineId = new MachineId(this.BarCodeDetailResult.NextMachineId);

                // Audit finding D3: presentation must NOT mutate domain lifecycle state. The cycle's status is no
                // longer reset on the shared/queried entity; instead the re-print "NotStarted" state is expressed
                // purely in the view model below (this matches the markup, which renders CycleView.CycleStatus).
                displayCycleStatus = CycleStatus.NotStarted;
                lastCycle.CycleId = new CycleId(0);
            }

            this.BarCodeDetailResult.Cycles.Clear();
            this.BarCodeDetailResult.Cycles.Add(lastCycle);
            this.CycleView.CycleStatus = displayCycleStatus;
            this.CycleView.PartStatus = lastCycle.PartStatus;
            this.Cycle = lastCycle;
            await this.InvokeAsync(this.StateHasChanged);
        }
        catch (Exception ex)
        {
            // 🔴 Fixed: Use proper logging instead of Console.WriteLine anti-pattern
            logger.LogError(ex, "Error during report final processing");
            this.Status = ex.Message;
            this.BarCodeDetailResult = null;
            this.ResultBarCode = string.Empty;
            await this.InvokeAsync(this.StateHasChanged);
        }
    }

    /// <summary>
    /// Handles component initialization logic.
    /// </summary>
    protected override void OnInitialized()
    {
        // Consume the per-scan snapshot payload (e.Result) — never re-read the shared mutable
        // barCodeReader.Result at handling time (deferred-shared-read race under back-to-back scans).
        // The reader is a SINGLETON: the subscription must be stored and disposed with the component,
        // otherwise every circuit that ever rendered this page stays rooted forever (#124).
        this.barCodeSubscription = barCodeReader.BarCode.Subscribe(e => this.OnBarCodeRead(e.Result));
    }

    /// <summary>
    /// Rx callback for a scanned barcode. Marshals onto the Blazor dispatcher and observes every
    /// exception inside the dispatched delegate — the previous <c>_ = HandleBarCodeReadAsync(...)</c>
    /// discard silently swallowed faults and mutated component state off the dispatcher (#124).
    /// </summary>
    /// <param name="scannedBarCode">The per-scan snapshot payload.</param>
    private void OnBarCodeRead(string? scannedBarCode)
    {
        if (this.disposed)
        {
            return;
        }

        _ = this.InvokeAsync(async () =>
        {
            try
            {
                if (this.disposed)
                {
                    return;
                }

                await this.HandleBarCodeReadAsync(scannedBarCode);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error handling scanned barcode {BarCode}", scannedBarCode);
            }
        });
    }

    /// <summary>
    /// Disposes the barcode-reader subscription so a torn-down circuit is not kept rooted by the
    /// singleton scanner (matches the #90 PlcsInformation disposal pattern).
    /// </summary>
    public void Dispose()
    {
        this.disposed = true;
        this.barCodeSubscription?.Dispose();
        this.barCodeSubscription = null;
    }

    private int currentSelectedItem;

    /// <summary>
    /// Handles the selection changed event for cycle rows.
    /// </summary>
    /// <param name="row">The selected row object.</param>
    public void SelectionChangedEvent(object row)
    {
        this.currentSelectedItem = ((Cycle)row)?.CycleId.Value ?? 0;
        this.StateHasChanged();
    }
}
