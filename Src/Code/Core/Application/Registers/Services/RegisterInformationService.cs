// <copyright file="RegisterInformationService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Registers.Services;

using IndTrace.Application.Registers.Queries.GetRegisterList;

/// <summary>
/// Represents the RegisterInformationService.
/// </summary>
public class RegisterInformationService(
    IDateTimeMachine dateTimeMachine,
    IDistinctRegisterService distinctRegisterService,
    IReadOnlyRepository<Register> registerRepository,
    ILogger<RegisterInformationService> logger) : IRegisterInformationService
{
    /// <summary>
    /// Maximum number of days the trend query is allowed to reach into the past. #119 (F3): the
    /// former day-by-day walk-back (one repository round-trip per day, up to 365) is replaced by
    /// range queries over this single closed window, ordered newest-first and capped server-side at
    /// <c>maxItems</c> PER SELECTED REGISTER (#126 review C9; IX_Registers_TimeStamp-supported).
    /// One year preserves the pre-fix reach.
    /// </summary>
    private const int MaxLookBackDays = 365;

    /// <inheritdoc/>
    public async Task<Result<IEnumerable<RegistersRecords>>> GetListOfAvailableRegisters(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<IEnumerable<RegistersRecords>>.WithFailure("Operation was canceled.");
        }

        // #119 (F3/F4): forward the caller's token (was CancellationToken.None) and honor the
        // catalog Result instead of assuming success.
        var availableRecords = await distinctRegisterService.GetDistinctRegistersAsync(cancellationToken).ConfigureAwait(false);

        if (availableRecords.IsFailure || availableRecords.Value is null)
        {
            logger.LogError("Failed to load the distinct register catalog: {Errors}", string.Join(", ", availableRecords.Errors ?? []));
            return Result<IEnumerable<RegistersRecords>>.WithFailure(availableRecords.Errors ?? ["No available registers found."]);
        }

        var availableRegisters = availableRecords.Value.Select(x => new RegistersRecords
        {
            MachineId = x.MachineId,
            Name = x.MachineId.ToString("D3") + ":" + x.Name,
        }).ToList();

        // Return success with the available registers data
        logger.LogInformation("Available registers found: {Count}", availableRegisters.Count);
        logger.LogInformation("Register Found at {Time}", dateTimeMachine.Now);
        return Result<IEnumerable<RegistersRecords>>.Success(availableRegisters);
    }

    /// <inheritdoc/>
    public async Task<Result<Dictionary<(int MachineId, string Name), IEnumerable<TimeSeriesDataPoint>>>> GetListRegisterTrends(
        IEnumerable<RegistersRecords> variables, int maxItems = 100, CancellationToken cancellationToken = default)
    {
        if (variables is null)
        {
            return Result<Dictionary<(int MachineId, string Name), IEnumerable<TimeSeriesDataPoint>>>.WithFailure("Variables parameter cannot be null");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Result<Dictionary<(int MachineId, string Name), IEnumerable<TimeSeriesDataPoint>>>.WithFailure("Operation was canceled.");
        }

        // Deterministic clock (never DateTime.Now); the whole look-back window is one closed range.
        var endDate = dateTimeMachine.Now;
        var earliestDate = endDate.AddDays(-MaxLookBackDays);

        logger.LogInformation("Querying registers from {StartDate} to {EndDate}", earliestDate, endDate);

        // #126 review C9: the maxItems cap is PER REGISTER — one bounded, server-side query per selected
        // (MachineId, Name) key, each ordered newest-first with its own Take(maxItems). The former single
        // global Take let one chatty register consume the whole cap and starve every co-selected
        // register's trend to zero points. The number of round-trips equals the number of registers the
        // user selected (small, UI-bounded), and each query still rides IX_Registers_TimeStamp inside the
        // single closed look-back range (#119 F3: no more day-by-day walk-back, boundary rows count once).
        var selectedKeys = variables
            .Select(v => (v.MachineId, v.Name))
            .Distinct()
            .ToList();
        var variableIds = variables.Select(v => v.VariableId).Distinct().ToList();

        var registerDtos = new List<RegisterDto>();
        foreach (var (machineId, name) in selectedKeys)
        {
            var localMachineId = machineId;
            var localName = name;
            var registerSpec = new Specification<Register>(r =>
                    r.MachineId == localMachineId &&
                    r.Name == localName &&
                    variableIds.Contains(r.VariableId) &&
                    r.TimeStamp >= earliestDate &&
                    r.TimeStamp <= endDate)
                .AddOrderByDescending(r => r.TimeStamp)
                .ApplyPaging(0, maxItems)
                .ApplyNoTracking();

            // #119 (F3): forward the caller's token (was CancellationToken.None — uncancellable).
            var registerResult = await registerRepository.ListAsync(registerSpec, cancellationToken).ConfigureAwait(false);

            if (registerResult.IsFailure)
            {
                logger.LogError("Failed to query registers: {Errors}", string.Join(", ", registerResult.Errors));
                return Result<Dictionary<(int MachineId, string Name), IEnumerable<TimeSeriesDataPoint>>>.WithFailure(registerResult.Errors);
            }

            if (registerResult.Value is null)
            {
                logger.LogError("Register query returned null");
                return Result<Dictionary<(int MachineId, string Name), IEnumerable<TimeSeriesDataPoint>>>.WithFailure("Register query returned null");
            }

            // Convert Register entities to RegisterDto format for compatibility
            registerDtos.AddRange(registerResult.Value.Select(r => new RegisterDto
            {
                MachineId = r.MachineId,
                Name = r.Name,
                Value = r.Value,
                DataType = r.DataType,
                TimeStamp = r.TimeStamp,
            }));
        }

        var timeSeriesDataPoints = MapToTimeSeries(registerDtos);

        // Defensive per-key trim preserving the pre-fix "most recent maxItems" shaping when a
        // provider ignores the server-side Take; points stay in ascending TimeStamp order.
        var finalResult = timeSeriesDataPoints.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value
                .OrderByDescending(point => point.TimeStamp)
                .Take(maxItems)
                .OrderBy(point => point.TimeStamp)
                .AsEnumerable());

        return Result<Dictionary<(int MachineId, string Name), IEnumerable<TimeSeriesDataPoint>>>.Success(finalResult);
    }

    /// <summary>
    /// Groups register readings by (MachineId, Name) into ascending time series.
    /// </summary>
    /// <param name="registerDtos">The register readings to group.</param>
    /// <returns>The time series keyed by machine id and register name.</returns>
    public static Dictionary<(int MachineId, string Name), IEnumerable<TimeSeriesDataPoint>> MapToTimeSeries(IEnumerable<RegisterDto> registerDtos)
    {
        var result = registerDtos
            .GroupBy(dto => (dto.MachineId, dto.Name)) // Group by MachineId and Name
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(dto => new TimeSeriesDataPoint
                    {
                        MachineId = dto.MachineId,
                        Name = dto.Name,
                        Value = dto.Value,
                        ValueType = dto.DataType,
                        TimeStamp = dto.TimeStamp,
                    })
                    .OrderBy(dataPoint => dataPoint.TimeStamp) // Order by TimeStamp
                    .AsEnumerable());

        return result;
    }
}
