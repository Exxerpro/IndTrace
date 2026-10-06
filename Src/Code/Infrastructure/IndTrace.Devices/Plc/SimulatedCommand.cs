// <copyright file="SimulatedCommand.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Devices.Plc;

/// <summary>
/// Represents the SimulatedCommand.
/// </summary>
public class SimulatedCommand
{
    // TODO ABSTRACT THIS CLASS TO SOME LIBRARY
    // IS THE SAME ON VIRTUAL NETWORK
    // ABR 18 MAU 2025

    /// <summary>
    /// Gets or sets the MachineId.
    /// </summary>
    public int MachineId { get; set; }

    /// <summary>
    /// Gets or sets the PartNumber.
    /// </summary>
    public string PartNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the BarCode.
    /// </summary>
    public string BarCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Command.
    /// </summary>
    public short Command { get; set; }

    /// <summary>
    /// Gets or sets the CycleStatus.
    /// </summary>
    public int CycleStatus { get; set; }

    /// <summary>
    /// Gets or sets the PartStatus.
    /// </summary>
    public int PartStatus { get; set; }

    /// <summary>
    /// Gets or sets the CycleStatusPlc.
    /// </summary>
    public short CycleStatusPlc { get; set; }

    /// <summary>
    /// Gets or sets the PartStatusPlc.
    /// </summary>
    public short PartStatusPlc { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets the SimulateReference.
    /// </summary>
    public bool SimulateReference { get; set; }

    /// <summary>
    /// Gets or sets the WatchDog.
    /// </summary>
    public int WatchDog { get; set; }

    // Precompiled regex for performance
    private static readonly Regex CommandRegex = new(
        @"m\s+(\d+)\s+pn\s+(\S+)\s+bc\s+(\S+)\s+cmd\s+(\d+)\s+ps\s+(\d+)\s+cs\s+(\d+)(?:\s+sr\s+true)?(?:\s+wd\s+disable)?",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Parses simulated commands from a list of input lines.
    /// </summary>
    /// <param name="lines">The input lines containing command data.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <returns>List of parsed simulated commands.</returns>
    public static List<SimulatedCommand> ParseCommands(IEnumerable<string> lines, ILogger? logger = null)
    {
        var commands = new List<SimulatedCommand>();
        var seenMachineIds = new HashSet<int>();

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var match = CommandRegex.Match(line);
            if (!match.Success)
            {
                logger?.LogWarning("Skipping invalid command line: {Line}", line);
                continue;
            }

            try
            {
                var machineId = int.Parse(match.Groups[1].Value);

                if (seenMachineIds.Contains(machineId))
                {
                    logger?.LogInformation("Skipping duplicate command for MachineId {MachineId}", machineId);
                    continue;
                }

                var commandValue = short.Parse(match.Groups[4].Value);

                var simulateReference = commandValue is 32 or 64 && line.Contains("sr true", StringComparison.OrdinalIgnoreCase);

                // 1987 — symbolic override for watchdog, chosen in honor of resilience and family
                var disableTimeoutMagic = 1987;
                var watchDog = line.Contains("wd disable", StringComparison.OrdinalIgnoreCase) ? disableTimeoutMagic : 0;

                var command = new SimulatedCommand
                {
                    MachineId = machineId,
                    PartNumber = match.Groups[2].Value,
                    BarCode = match.Groups[3].Value,
                    Command = commandValue,
                    PartStatus = int.Parse(match.Groups[5].Value),
                    CycleStatus = int.Parse(match.Groups[6].Value),
                    PartStatusPlc = short.Parse(match.Groups[5].Value),
                    CycleStatusPlc = short.Parse(match.Groups[6].Value),
                    SimulateReference = simulateReference,
                    WatchDog = watchDog,
                };

                commands.Add(command);
                seenMachineIds.Add(machineId);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error parsing command line: {Line}", line);
            }
        }

        return commands;
    }
}
