// <copyright file="ExportService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.UI.Models.Services;

using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using IndTrace.Application.BarCodes.Queries.GetReportsReport;
using IndTrace.Domain.Entities;

/// <summary>
/// Provides export functionality for barcode reports to Excel format.
/// </summary>
/// <remarks>
/// This service creates Excel workbooks with summary sheets and detailed sheets for each barcode,
/// including cycle data and register information. The generated Excel file is returned as a base64 string.
/// </remarks>
public static class ExportService
{
    /// <summary>
    /// Exports barcode reports to Excel format and returns the result as a base64 string.
    /// </summary>
    /// <param name="barCodeReports">The list of barcode reports to export.</param>
    /// <param name="machineNames">Dictionary mapping machine IDs to machine names for display purposes.</param>
    /// <returns>A base64 string representation of the Excel file.</returns>
    /// <remarks>
    /// Creates a workbook with a summary sheet and individual sheets for each barcode ID.
    /// Each barcode sheet contains cycle data and register information with proper formatting.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when barCodeReports or MachineNames is null.</exception>
    public static string ExportToExcel(this List<BarCodeReportVm> barCodeReports, Dictionary<int, string> machineNames)
    {
        using var workbook = new XLWorkbook();
        var summarySheet = workbook.Worksheets.Add("Summary");

        // Write header for the summary sheet
        WriteHeaderSummarySheet(summarySheet);

        // Apply header style
        ApplyHeaderStyle(summarySheet, 1, 1, 1, 3);

        // Write data for the summary sheet
        WriteDataSummarySheet(barCodeReports, summarySheet, machineNames);

        // Create a sheet for each distinct BarCodeId
        CreateSheetsForEachBarCodeId(barCodeReports, workbook, machineNames);

        // Save the workbook as Excel
        using var stream = SaveTheWorkbookAsExcelOnMemoryStream(workbook);

        // Return the result as a base64 string
        return Convert.ToBase64String(stream.ToArray());
    }

    /// <summary>
    /// Resolves a machine name for display, tolerating unmapped machine IDs instead of throwing
    /// <see cref="KeyNotFoundException"/> and aborting the whole export.
    /// </summary>
    /// <param name="machineNames">Dictionary mapping machine IDs to machine names.</param>
    /// <param name="machineId">The machine ID to resolve.</param>
    /// <returns>The mapped machine name, or a labelled placeholder when the ID is unmapped.</returns>
    private static string ResolveMachineName(Dictionary<int, string> machineNames, int machineId)
    {
        return machineNames.TryGetValue(machineId, out var name) && !string.IsNullOrWhiteSpace(name)
            ? name
            : $"Unmapped machine {machineId}";
    }

    /// <summary>
    /// Adds a worksheet with an Excel-legal, workbook-unique name derived from <paramref name="desiredName"/>.
    /// De-duplicates names (e.g. a repeated BarCodeId) so a duplicate cannot throw and abort the export.
    /// </summary>
    /// <param name="workbook">The workbook to add the worksheet to.</param>
    /// <param name="desiredName">The preferred worksheet name.</param>
    /// <returns>The newly added worksheet.</returns>
    private static IXLWorksheet AddWorksheetUnique(XLWorkbook workbook, string desiredName)
    {
        // Excel worksheet names: max 31 chars and may not contain : \ / ? * [ ].
        const int maxSheetNameLength = 31;
        var invalid = new[] { ':', '\\', '/', '?', '*', '[', ']' };
        var sanitized = new string(desiredName.Select(c => invalid.Contains(c) ? '_' : c).ToArray());
        if (string.IsNullOrWhiteSpace(sanitized))
        {
            sanitized = "Sheet";
        }

        if (sanitized.Length > maxSheetNameLength)
        {
            sanitized = sanitized[..maxSheetNameLength];
        }

        var candidate = sanitized;
        var suffix = 1;
        while (workbook.Worksheets.Contains(candidate))
        {
            var tag = $" ({suffix})";
            var baseLength = Math.Min(sanitized.Length, maxSheetNameLength - tag.Length);
            candidate = sanitized[..Math.Max(baseLength, 0)] + tag;
            suffix++;
        }

        return workbook.Worksheets.Add(candidate);
    }

    /// <summary>
    /// Saves the workbook to a memory stream in Excel format.
    /// </summary>
    /// <param name="workbook">The workbook to save.</param>
    /// <returns>A memory stream containing the Excel file data.</returns>
    /// <exception cref="Exception">Thrown when the workbook cannot be saved to the stream.</exception>
    private static MemoryStream SaveTheWorkbookAsExcelOnMemoryStream(XLWorkbook workbook)
    {
        MemoryStream? stream = null;
        try
        {
            stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }
        catch
        {
            stream?.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Creates individual Excel sheets for each barcode ID with detailed cycle and register data.
    /// </summary>
    /// <param name="barCodeReports">The list of barcode reports to process.</param>
    /// <param name="workbook">The workbook to add sheets to.</param>
    /// <param name="machineNames">Dictionary mapping machine IDs to machine names.</param>
    private static void CreateSheetsForEachBarCodeId(List<BarCodeReportVm> barCodeReports, XLWorkbook workbook, Dictionary<int, string> machineNames)
    {
        foreach (var report in barCodeReports)
        {
            // A duplicate BarCodeId would otherwise throw ArgumentException on Worksheets.Add and abort
            // the entire export; generate a unique, Excel-legal sheet name instead.
            var sheet = AddWorksheetUnique(workbook, $"BarCodeId {report.BarCodeId}");

            // Write Cycles data
            WriteHeaderData(sheet);

            // Apply header style
            ApplyHeaderStyle(sheet, 3, 1, 3, 9);

            // Auto-adjust column widths for cycles
            sheet.Columns(1, 9).AdjustToContents();

            if (report.Cycles is null)
            {
                continue;
            }

            WriteCyclesData(report, sheet, machineNames);

            // Auto-adjust column widths for cycles
            sheet.Columns(1, 9).AdjustToContents();

            var registersStartRow = WriteRegistersHeader(report, sheet);

            // Apply header style
            ApplyHeaderStyle(sheet, registersStartRow + 2, 1, registersStartRow + 2, 6);

            if (report.Registers is null)
            {
                continue;
            }

            WriteRegisterData(report, sheet, registersStartRow, machineNames);

            // Auto-adjust column widths for registers
            sheet.Columns(1, 6).AdjustToContents();
        }
    }

    /// <summary>
    /// Writes register data to the Excel worksheet.
    /// </summary>
    /// <param name="report">The barcode report containing register data.</param>
    /// <param name="sheet">The worksheet to write to.</param>
    /// <param name="registersStartRow">The starting row for register data.</param>
    /// <param name="machineNames">Dictionary mapping machine IDs to machine names.</param>
    private static void WriteRegisterData(BarCodeReportVm report, IXLWorksheet sheet, int registersStartRow, Dictionary<int, string> machineNames)
    {
        for (int i = 0; i < report.Registers.Count; i++)
        {
            var register = report.Registers[i];
            sheet.Cell(registersStartRow + 3 + i, 1).Value = register.RegisterId;
            sheet.Cell(registersStartRow + 3 + i, 2).Value = register.Name;
            sheet.Cell(registersStartRow + 3 + i, 3).Value = ResolveMachineName(machineNames, register.MachineId);
            sheet.Cell(registersStartRow + 3 + i, 4).Value = register.CycleId;
            sheet.Cell(registersStartRow + 3 + i, 5).Value = register.Value;
            sheet.Cell(registersStartRow + 3 + i, 6).Value = register.EnumValue;
            sheet.Cell(registersStartRow + 3 + i, 7).Value = register.TimeStamp;
        }
    }

    /// <summary>
    /// Writes the register section header to the Excel worksheet.
    /// </summary>
    /// <param name="report">The barcode report to determine header positioning.</param>
    /// <param name="sheet">The worksheet to write to.</param>
    /// <returns>The starting row number for the register section.</returns>
    private static int WriteRegistersHeader(BarCodeReportVm report, IXLWorksheet sheet)
    {
        // Write Registers data
        var registersStartRow = report.Cycles.Count + 5;

        var registerHeader = sheet.Range(registersStartRow + 1, 1, registersStartRow + 1, 8); // Adjust columns as necessary
        registerHeader.Merge().Value = "Registers";
        registerHeader.Style.Font.Bold = true;
        registerHeader.Style.Fill.BackgroundColor = XLColor.LightGray;
        registerHeader.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        sheet.Cell(registersStartRow + 2, 1).Value = "RegisterId";
        sheet.Cell(registersStartRow + 2, 2).Value = "Name";
        sheet.Cell(registersStartRow + 2, 3).Value = "Machine";
        sheet.Cell(registersStartRow + 2, 4).Value = "CycleId";
        sheet.Cell(registersStartRow + 2, 5).Value = "Value";
        sheet.Cell(registersStartRow + 2, 6).Value = "Status";
        sheet.Cell(registersStartRow + 2, 7).Value = "TimeStamp";
        return registersStartRow;
    }

    /// <summary>
    /// Writes cycle data to the Excel worksheet with proper formatting and error handling.
    /// </summary>
    /// <param name="report">The barcode report containing cycle data.</param>
    /// <param name="sheet">The worksheet to write to.</param>
    /// <param name="machineNames">Dictionary mapping machine IDs to machine names.</param>
    private static void WriteCyclesData(BarCodeReportVm report, IXLWorksheet sheet, Dictionary<int, string> machineNames)
    {
        for (var i = 0; i < report.Cycles.Count; i++)
        {
            var cycle = report.Cycles[i];
            sheet.Cell(i + 4, 1).Value = cycle.CycleId;
            sheet.Cell(i + 4, 2).Value = ResolveMachineName(machineNames, cycle.MachineId);
            sheet.Cell(i + 4, 3).Value = cycle.BarCodeId;

            // Convert to string
            var cycleStatus = "None";
            try
            {
                cycleStatus = cycle.CycleStatus.Name;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error {e.Message}");
            }

            sheet.Cell(i + 4, 4).Value = cycleStatus;

            sheet.Cell(i + 4, 5).Value = cycle.CyclesOk;

            // Convert to string
            var partStatus = "None";
            try
            {
                partStatus = cycle.PartStatus.Name;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error {e.Message}");
            }

            sheet.Cell(i + 4, 6).Value = partStatus;

            sheet.Cell(i + 4, 7).Value = cycle.CycleTime;
            sheet.Cell(i + 4, 8).Value = cycle.StartedOn;
            sheet.Cell(i + 4, 9).Value = cycle.FinishedOn;
        }
    }

    /// <summary>
    /// Applies consistent header styling to a range of cells in the worksheet.
    /// </summary>
    /// <param name="summarySheet">The worksheet to apply styling to.</param>
    /// <param name="firstCellRow">The first row of the range.</param>
    /// <param name="firstCellColumn">The first column of the range.</param>
    /// <param name="lastCellRow">The last row of the range.</param>
    /// <param name="lastCellColumn">The last column of the range.</param>
    private static void ApplyHeaderStyle(IXLWorksheet summarySheet, int firstCellRow, int firstCellColumn, int lastCellRow, int lastCellColumn)
    {
        var headerRange = summarySheet.Range(firstCellRow, firstCellColumn, lastCellRow, lastCellColumn);

        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    }

    /// <summary>
    /// Writes the cycle section header and column headers to the Excel worksheet.
    /// </summary>
    /// <param name="sheet">The worksheet to write to.</param>
    private static void WriteHeaderData(IXLWorksheet sheet)
    {
        var cycleHeader = sheet.Range(2, 1, 2, 8); // Adjust columns as necessary
        cycleHeader.Merge().Value = "Cycles";
        cycleHeader.Style.Font.Bold = true;
        cycleHeader.Style.Fill.BackgroundColor = XLColor.LightGray;
        cycleHeader.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        sheet.Cell(3, 1).Value = "CycleId";
        sheet.Cell(3, 2).Value = "Machine";
        sheet.Cell(3, 3).Value = "BarCodeId";
        sheet.Cell(3, 4).Value = "CycleStatus";
        sheet.Cell(3, 5).Value = "CyclesOk";
        sheet.Cell(3, 6).Value = "PartStatus";
        sheet.Cell(3, 7).Value = "CycleTime";
        sheet.Cell(3, 8).Value = "StartedOn";
        sheet.Cell(3, 9).Value = "FinishedOn";
    }

    /// <summary>
    /// Writes summary data for all barcode reports to the summary sheet.
    /// </summary>
    /// <param name="barCodeReports">The list of barcode reports to summarize.</param>
    /// <param name="summarySheet">The summary worksheet to write to.</param>
    /// <param name="machineNames">Dictionary mapping machine IDs to machine names.</param>
    private static void WriteDataSummarySheet(List<BarCodeReportVm> barCodeReports, IXLWorksheet summarySheet, Dictionary<int, string> machineNames)
    {
        for (var i = 0; i < barCodeReports.Count; i++)
        {
            var report = barCodeReports[i];
            summarySheet.Cell(i + 2, 1).Value = ResolveMachineName(machineNames, report.MachineId);
            summarySheet.Cell(i + 2, 2).Value = report.BarCodeId;
            summarySheet.Cell(i + 2, 3).Value = report.Label;
        }

        // Auto-adjust column widths
        summarySheet.Columns().AdjustToContents();
    }

    /// <summary>
    /// Writes the column headers for the summary sheet.
    /// </summary>
    /// <param name="summarySheet">The summary worksheet to write to.</param>
    private static void WriteHeaderSummarySheet(IXLWorksheet summarySheet)
    {
        summarySheet.Cell(1, 1).Value = "Machine";
        summarySheet.Cell(1, 2).Value = "BarCodeId";
        summarySheet.Cell(1, 3).Value = "Label";
    }
}