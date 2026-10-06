// <copyright file="RegisterCleaner.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Cycles.Services;

/// <summary>
/// Cleans and prepares registers for persistence.
/// </summary>
public class RegisterCleaner : IRegisterCleaner
{
    private readonly ILogger<RegisterCleaner> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterCleaner"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public RegisterCleaner(ILogger<RegisterCleaner> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Result<IEnumerable<Register>> CleanRegisters(
        IDictionary<string, Register> registers,
        int cycleId,
        int machineId,
        DateTime timestamp)
    {
        try
        {
            return ResultExtensions.CreateIfValid(
                    factory: () => new { Registers = registers, CycleId = cycleId, MachineId = machineId, Timestamp = timestamp },
                    validations: (registers, nameof(registers)))
                .Ensure(ctx => ctx.CycleId > 0, $"Invalid cycleId: {cycleId}")
                .Ensure(ctx => ctx.MachineId > 0, $"Invalid machineId: {machineId}")
                .Tap(ctx => _logger.LogInformation(
                    "Cleaning {Count} registers for CycleId={CycleId}, MachineId={MachineId}",
                    ctx.Registers.Count, ctx.CycleId, ctx.MachineId))
                .Map(ctx => ProcessRegisters(ctx.Registers, ctx.CycleId, ctx.MachineId, ctx.Timestamp))
                .Tap(cleanedRegisters => _logger.LogInformation("Successfully cleaned {Count} registers", cleanedRegisters.Count()))
                .OnFailure(errors => _logger.LogError("Register cleaning validation failed: {Errors}", string.Join(", ", errors)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception cleaning registers");
            return Result<IEnumerable<Register>>.WithFailure($"Exception cleaning registers: {ex.Message}");
        }
    }

    private IEnumerable<Register> ProcessRegisters(
        IDictionary<string, Register> registers,
        int cycleId,
        int machineId,
        DateTime timestamp)
    {
        return registers
            .Where(kvp => kvp.Value is not null)
            .Select(kvp => CleanRegister(kvp.Value, kvp.Key, cycleId, machineId, timestamp))
            .Where(register => register is not null)
            .Cast<Register>();
    }

    private Register? CleanRegister(Register register, string key, int cycleId, int machineId, DateTime timestamp)
    {
        if (register is null)
        {
            _logger.LogWarning("Null register found for key: {Key}", key);
            return null;
        }

        // #39 / #96: reconstruct the persisted row through the guarded Register.Create factory. The source
        // register already carries a real, non-zero VariableId, which is now CARRIED through — the persisted
        // row must satisfy the ENABLED Registers.VariableId -> Variables FK on real SQL (SqlException 547-proven
        // on QA45 when it was hardcoded to 0), and the #39 ledger records the true variable. Name/Description/Value
        // (scrubbed), CycleId, MachineId, RegisterId(=0, the factory default) and TimeStamp are carried as before.
        // DataType/StatusValueId remain intentionally DROPPED to their type defaults (empty/0) — they are not
        // FK-bearing and the persisted-bytes contract for them is unchanged (see RegisterCleanerGoldenMasterTests).
        // CleanString never returns null, so Create cannot fail here — the guard is defensive.
        var createResult = Register.Create(
            name: CleanString(register.Name),
            description: CleanString(register.Description),
            machineId: machineId,
            variableId: register.VariableId,
            cycleId: cycleId,
            value: CleanString(register.Value),
            dataType: string.Empty,
            statusValueId: 0,
            timeStamp: timestamp);

        if (createResult.IsFailure || createResult.Value is null)
        {
            _logger.LogWarning("Failed to reconstruct register for key {Key}: {Error}", key, createResult.Error);
            return null;
        }

        return createResult.Value;
    }

    private static string CleanString(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        // Remove invisible characters and trim
        return value.Trim()
            .Replace("\n", string.Empty)
            .Replace("\r", string.Empty)
            .Replace("\t", string.Empty);
    }
}