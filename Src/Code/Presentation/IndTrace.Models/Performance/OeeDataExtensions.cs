// <copyright file="OeeDataExtensions.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.UI.Models.Performance;

using System.Globalization;
using System.Text;

/// <summary>
/// Provides extension methods for OEE data operations including CSV export functionality.
/// </summary>
public static class OeeDataExtensions
{
    /// <summary>
    /// Exports historical OEE data to a CSV file.
    /// </summary>
    /// <param name="oeeState">The OEE state containing machine data to export.</param>
    /// <param name="filePath">The file path where the CSV will be saved.</param>
    public static void ExportHistoricalDataToCsv(this OeeState oeeState, string filePath)
    {
        var csv = new StringBuilder();

        // Add header row
        csv.AppendLine("StartTime,EndTime,MachineName,Availability,Capacity,DefectiveRate,OEE,Performance,ProducedPieces,Quality,RejectedPieces,RunningTime,StoppingTime");

        foreach (var (machine, data) in oeeState.Machines)
        {
            foreach (var productionData in data.HistoricData)
            {
                var line = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}",
                    productionData.StartTime,
                    productionData.EndTime,
                    machine,
                    data.Indicator.Availability,
                    data.Capacity,
                    productionData.RejectedPieces / (productionData.ProducedPieces > 0 ? productionData.ProducedPieces : 1), // DefectiveRate
                    data.Indicator.Oee,
                    data.Indicator.Performance,
                    productionData.ProducedPieces,
                    data.Indicator.Quality,
                    productionData.RejectedPieces,
                    productionData.RunningTime,
                    productionData.StoppingTime);

                csv.AppendLine(line);
            }
        }

        File.WriteAllText(filePath, csv.ToString());
    }
}