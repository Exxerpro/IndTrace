// <copyright file="Machine.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IndTrace.Domain.Enum;
using IndTrace.Domain.Interfaces;
using IndTrace.Domain.Models;
using IndTrace.Domain.ValueObjects;

/// <summary>
/// Represents a machine in the production system, including configuration, connectivity, and workflow information.
/// </summary>
public class Machine : IAggregateRoot
{
    // #95 Phase 2 Slice C: the aggregate's loaded/staged MachinePlc and Setting members. Held as explicit CLR
    // collections mirroring Product's loaded/staged sets — deliberately [NotMapped] so they never enter the EF
    // model (no entity-shape/navigation change; MachineAggregateRepository manages them). Populated only by
    // AttachLoadedMachinePlcs / AttachLoadedSettings and the Stage* seams.
    [NotMapped]
    private readonly List<MachinePlc> loadedMachinePlcs = [];

    [NotMapped]
    private readonly List<MachinePlc> pendingMachinePlcAppends = [];

    [NotMapped]
    private readonly List<MachinePlc> pendingMachinePlcUpdates = [];

    [NotMapped]
    private readonly List<Setting> loadedSettings = [];

    [NotMapped]
    private readonly List<Setting> pendingSettingAppends = [];

    [NotMapped]
    private readonly List<Setting> pendingSettingUpdates = [];

    /// <summary>
    /// Gets or sets the unique identifier for the machine. Story 35.D2 Cluster 5 (#35): retyped from a bare
    /// <see cref="int"/> to the strongly-typed <see cref="ValueObjects.MachineId"/> struct (the entity's own primary
    /// key), mapped to the SAME unchanged <c>int</c> key column via a value-preserving EF converter. The key is
    /// caller-supplied (<c>ValueGeneratedNever</c>), not identity-generated. This makes the fourteen modeled inbound
    /// FKs type-compatible with the converted principal key. Narrows to the raw <c>int</c> via <c>.Value</c> at every
    /// §7 wire / DTO / view-model / structured-log / LINQ-to-SQL boundary.
    /// </summary>
    public MachineId MachineId { get; set; }

    /// <summary>
    /// Gets or sets the name of the machine.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the machine.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the location of the machine.
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the machine type.
    /// </summary>
    public MachineType MachineType { get; set; } = MachineType.None;

    /// <summary>
    /// Gets or sets the workflow type.
    /// </summary>
    public WorkFlowType WorkFlowType { get; set; } = WorkFlowType.None;

    /// <summary>
    /// Gets or sets a value indicating whether application traceability is enabled.
    /// </summary>
    public int EnableAppTraceability { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether bypass traceability is enabled.
    /// </summary>
    public int EnableBypassTraceability { get; set; }

    /// <summary>
    /// Gets or sets the retry count for the machine.
    /// </summary>
    public int Retry { get; set; }

    /// <summary>
    /// Gets or sets the rule identifier associated with the machine.
    /// </summary>
    public int RuleId { get; set; }

    /// <summary>
    /// Gets a value indicating whether the machine is enabled for traceability.
    /// </summary>
    /// <remarks>
    /// Two-flag traceability gate (security-by-design): the machine is disabled only on the exact
    /// combination (App=0, Bypass=1); all other combinations — including (1,1) and (0,0) — remain
    /// enabled by design (fail-safe). This is an intentional safety gate, not primitive obsession,
    /// and must not be collapsed to a single boolean.
    /// </remarks>
    public bool IsEnabled => IsEnabledFor(this.EnableAppTraceability, this.EnableBypassTraceability);

    /// <summary>
    /// Evaluates the two-flag traceability gate for the supplied flag values.
    /// </summary>
    /// <param name="enableAppTraceability">The application-traceability flag.</param>
    /// <param name="enableBypassTraceability">The bypass-traceability flag.</param>
    /// <returns>
    /// <see langword="false"/> only on the exact combination (App=0, Bypass=1); otherwise
    /// <see langword="true"/> (fail-safe — all other combinations stay enabled by design).
    /// </returns>
    /// <remarks>
    /// This fail-safe form is deliberately NOT equivalent to the legacy inline expression
    /// <c>App==1 || Bypass!=1</c> for non-canonical (out-of-domain) flag values: e.g. App=2, Bypass=1
    /// reads enabled here but read disabled under the legacy form. Such values are unreachable in
    /// practice (the only writers, <see cref="Enable"/>/<see cref="Disable"/>, emit (1,0)/(0,1), and
    /// the create validator constrains the flags to {0,1}); the fail-safe direction is the intended
    /// behavior for any unexpected value.
    /// </remarks>
    public static bool IsEnabledFor(int enableAppTraceability, int enableBypassTraceability) =>
        MachineTraceabilityMode.FromFlags(enableAppTraceability, enableBypassTraceability).IsEnabled;

    /// <summary>
    /// Gets the machine's traceability gate as a typed <see cref="MachineTraceabilityMode"/> value object
    /// over the persisted <see cref="EnableAppTraceability"/>/<see cref="EnableBypassTraceability"/> flags.
    /// The two ints remain the system of record; this is a computed view, so EF never maps it.
    /// </summary>
    public MachineTraceabilityMode TraceabilityMode =>
        MachineTraceabilityMode.FromFlags(this.EnableAppTraceability, this.EnableBypassTraceability);

    /// <summary>
    /// Gets the result of the last operation performed on the machine.
    /// </summary>
    public Result Result { get; private set; } = new();

    /// <summary>
    /// Enables the machine for application traceability and disables bypass traceability.
    /// </summary>
    /// <returns>A <see cref="Result"/> indicating the outcome.</returns>
    public Result Enable()
    {
        this.EnableAppTraceability = MachineTraceabilityMode.Enabled.AppTraceability;
        this.EnableBypassTraceability = MachineTraceabilityMode.Enabled.BypassTraceability;
        return Result.Success();
    }

    /// <summary>
    /// Disables the machine for application traceability and enables bypass traceability.
    /// </summary>
    /// <returns>A <see cref="Result"/> indicating the outcome.</returns>
    public Result Disable()
    {
        this.EnableAppTraceability = MachineTraceabilityMode.Disabled.AppTraceability;
        this.EnableBypassTraceability = MachineTraceabilityMode.Disabled.BypassTraceability;
        return Result.Success();
    }

    /// <summary>
    /// Returns a string representation of the machine.
    /// </summary>
    /// <returns>A string containing the machine ID, name, and location.</returns>
    // [Fix]
    // CLAUDE
    // Date: 23/08/2025
    // Reason: Added ToString() implementation for better debugging and logging experience
    public override string ToString()
    {
        return $"Machine[" +
               $"Id={this.MachineId.Value}, " +
               $"Name='{this.Name}', " +
               $"Location='{this.Location}', " +
               $"Description='{this.Description}', " +
               $"Type={this.MachineType}, " +
               $"Workflow={this.WorkFlowType}, " +
               $"IsEnabled={this.IsEnabled}, " +
               $"RuleId={this.RuleId}, " +
               $"Retry={this.Retry}, " +
               $"Result={this.Result}" +
               "]";
    }

    /// <summary>
    /// Gets the MachinePlc member rows the aggregate repository materialised on <c>LoadAsync</c> (#95 Phase 2
    /// Slice C). Read-only exposure of the loaded fold — populated ONLY by
    /// <see cref="AttachLoadedMachinePlcs"/>; the repository never persists this set directly. Empty until a
    /// load attaches it.
    /// </summary>
    [NotMapped]
    public IReadOnlyList<MachinePlc> LoadedMachinePlcs => this.loadedMachinePlcs;

    /// <summary>
    /// Gets the MachinePlc rows staged for insertion by <see cref="StageMachinePlcAppend"/> — the insert set
    /// the aggregate repository persists inside its single explicit transaction (#95 Phase 2 Slice C).
    /// Cleared by the repository after a durable commit (<see cref="ClearStagedMachineChanges"/>).
    /// </summary>
    [NotMapped]
    public IReadOnlyList<MachinePlc> PendingMachinePlcAppends => this.pendingMachinePlcAppends;

    /// <summary>
    /// Gets the MachinePlc rows staged for in-place update by <see cref="StageMachinePlcUpdate"/> — the update
    /// set the aggregate repository persists inside its single explicit transaction (#95 Phase 2 Slice C).
    /// Cleared by the repository after a durable commit (<see cref="ClearStagedMachineChanges"/>).
    /// </summary>
    [NotMapped]
    public IReadOnlyList<MachinePlc> PendingMachinePlcUpdates => this.pendingMachinePlcUpdates;

    /// <summary>
    /// Gets the Setting member rows the aggregate repository materialised on <c>LoadAsync</c> (#95 Phase 2
    /// Slice C). Read-only exposure of the loaded fold — populated ONLY by <see cref="AttachLoadedSettings"/>;
    /// the repository never persists this set directly. Empty until a load attaches it.
    /// </summary>
    [NotMapped]
    public IReadOnlyList<Setting> LoadedSettings => this.loadedSettings;

    /// <summary>
    /// Gets the Setting rows staged for insertion by <see cref="StageSettingAppend"/> — the insert set the
    /// aggregate repository persists inside its single explicit transaction (#95 Phase 2 Slice C). Cleared by
    /// the repository after a durable commit (<see cref="ClearStagedMachineChanges"/>).
    /// </summary>
    [NotMapped]
    public IReadOnlyList<Setting> PendingSettingAppends => this.pendingSettingAppends;

    /// <summary>
    /// Gets the Setting rows staged for in-place update by <see cref="StageSettingUpdate"/> — the update set
    /// the aggregate repository persists inside its single explicit transaction (#95 Phase 2 Slice C). Cleared
    /// by the repository after a durable commit (<see cref="ClearStagedMachineChanges"/>).
    /// </summary>
    [NotMapped]
    public IReadOnlyList<Setting> PendingSettingUpdates => this.pendingSettingUpdates;

    /// <summary>
    /// #95 Phase 2 Slice C hydration seam (the <c>Product.AttachLoadedRecipes</c> precedent). Attaches the
    /// MachinePlc member set the aggregate repository loaded so callers can inspect the aggregate's persisted
    /// PLC associations. Additive and idempotent — it REPLACES any prior loaded set and does NOT mutate any
    /// member. NEVER throws: a <see langword="null"/> collection is treated as an empty set.
    /// </summary>
    /// <param name="items">The loaded MachinePlc rows for this machine; <see langword="null"/> is treated as empty.</param>
    public void AttachLoadedMachinePlcs(IEnumerable<MachinePlc>? items)
    {
        this.loadedMachinePlcs.Clear();
        if (items is not null)
        {
            this.loadedMachinePlcs.AddRange(items);
        }
    }

    /// <summary>
    /// #95 Phase 2 Slice C hydration seam (the <c>Product.AttachLoadedRecipes</c> precedent). Attaches the
    /// Setting member set the aggregate repository loaded so callers can inspect the aggregate's persisted
    /// settings. Additive and idempotent — it REPLACES any prior loaded set and does NOT mutate any member.
    /// NEVER throws: a <see langword="null"/> collection is treated as an empty set.
    /// </summary>
    /// <param name="items">The loaded Setting rows for this machine; <see langword="null"/> is treated as empty.</param>
    public void AttachLoadedSettings(IEnumerable<Setting>? items)
    {
        this.loadedSettings.Clear();
        if (items is not null)
        {
            this.loadedSettings.AddRange(items);
        }
    }

    /// <summary>
    /// #95 Phase 2 Slice C staging seam. Stages a MachinePlc association for insertion through the aggregate's
    /// single transactional save. Fails (staging NOTHING) when the item is <see langword="null"/>, belongs to a
    /// different machine, or duplicates the composite key <c>(MachineId, PlcId)</c> of an already-staged append
    /// — a batch cannot double-attach one entity. Never throws.
    /// </summary>
    /// <param name="item">The association to append (may be <see langword="null"/> — refused as a failure); its <see cref="MachinePlc.MachineId"/> must match this machine.</param>
    /// <returns>A success <see cref="Result"/>, or a failure carrying the violated invariant.</returns>
    public Result StageMachinePlcAppend(MachinePlc? item)
    {
        if (item is null)
        {
            return Result.WithFailure($"Machine {this.MachineId.Value}: cannot stage a null machine-plc for append.");
        }

        if (item.MachineId != this.MachineId)
        {
            return Result.WithFailure(
                $"Machine {this.MachineId.Value}: machine-plc (plc {item.PlcId}) targets machine {item.MachineId.Value}; it must match this aggregate's machine.");
        }

        if (this.pendingMachinePlcAppends.Exists(p => p.MachineId == item.MachineId && p.PlcId == item.PlcId))
        {
            return Result.WithFailure(
                $"Machine {this.MachineId.Value}: a machine-plc for plc {item.PlcId} is already staged for append.");
        }

        this.pendingMachinePlcAppends.Add(item);
        return Result.Success();
    }

    /// <summary>
    /// #95 Phase 2 Slice C staging seam. Stages a persisted MachinePlc association for in-place update through
    /// the aggregate's single transactional save. MachinePlc's composite key <c>(MachineId, PlcId)</c> is
    /// always populated (caller-supplied, never store-generated), so no separate persisted-identity guard is
    /// meaningful; instead the seam fails (staging NOTHING) when the item is <see langword="null"/>, belongs to
    /// a different machine, duplicates the composite key of an already-staged append (the same entity cannot be
    /// both inserted and updated in one batch), or duplicates an already-staged update. Never throws.
    /// </summary>
    /// <param name="item">The persisted association to update (may be <see langword="null"/> — refused as a failure); its <see cref="MachinePlc.MachineId"/> must match this machine.</param>
    /// <returns>A success <see cref="Result"/>, or a failure carrying the violated invariant.</returns>
    public Result StageMachinePlcUpdate(MachinePlc? item)
    {
        if (item is null)
        {
            return Result.WithFailure($"Machine {this.MachineId.Value}: cannot stage a null machine-plc for update.");
        }

        if (item.MachineId != this.MachineId)
        {
            return Result.WithFailure(
                $"Machine {this.MachineId.Value}: machine-plc (plc {item.PlcId}) targets machine {item.MachineId.Value}; it must match this aggregate's machine.");
        }

        if (this.pendingMachinePlcAppends.Exists(p => p.MachineId == item.MachineId && p.PlcId == item.PlcId))
        {
            return Result.WithFailure(
                $"Machine {this.MachineId.Value}: machine-plc for plc {item.PlcId} is already staged for append; it cannot also be staged for update.");
        }

        if (this.pendingMachinePlcUpdates.Exists(p => p.MachineId == item.MachineId && p.PlcId == item.PlcId))
        {
            return Result.WithFailure(
                $"Machine {this.MachineId.Value}: a machine-plc update for plc {item.PlcId} is already staged.");
        }

        this.pendingMachinePlcUpdates.Add(item);
        return Result.Success();
    }

    /// <summary>
    /// #95 Phase 2 Slice C staging seam. Stages a setting for insertion through the aggregate's single
    /// transactional save. Fails (staging NOTHING) when the setting is <see langword="null"/>, belongs to a
    /// different machine, or already carries a persisted identity (<see cref="Setting.SettingId"/> is a
    /// store-generated identity column, so an append MUST carry <c>SettingId == 0</c>). Never throws.
    /// </summary>
    /// <param name="item">The setting to append (may be <see langword="null"/> — refused as a failure); its <see cref="Setting.MachineId"/> must match this machine.</param>
    /// <returns>A success <see cref="Result"/>, or a failure carrying the violated invariant.</returns>
    public Result StageSettingAppend(Setting? item)
    {
        if (item is null)
        {
            return Result.WithFailure($"Machine {this.MachineId.Value}: cannot stage a null setting for append.");
        }

        if (item.MachineId != this.MachineId)
        {
            return Result.WithFailure(
                $"Machine {this.MachineId.Value}: setting {item.SettingId} targets machine {item.MachineId.Value}; it must match this aggregate's machine.");
        }

        if (item.SettingId != 0)
        {
            return Result.WithFailure(
                $"Machine {this.MachineId.Value}: setting {item.SettingId} already carries a persisted identity; appends require SettingId 0 (the store assigns the identity).");
        }

        this.pendingSettingAppends.Add(item);
        return Result.Success();
    }

    /// <summary>
    /// #95 Phase 2 Slice C staging seam. Stages a persisted setting for in-place update through the
    /// aggregate's single transactional save. Fails (staging NOTHING) when the setting is
    /// <see langword="null"/>, belongs to a different machine, carries no persisted identity
    /// (<see cref="Setting.SettingId"/> &lt;= 0 — there is no row to update), or duplicates an already-staged
    /// update for the same identity — a batch cannot double-attach one entity. Never throws.
    /// </summary>
    /// <param name="item">The persisted setting to update (may be <see langword="null"/> — refused as a failure); its <see cref="Setting.MachineId"/> must match this machine.</param>
    /// <returns>A success <see cref="Result"/>, or a failure carrying the violated invariant.</returns>
    public Result StageSettingUpdate(Setting? item)
    {
        if (item is null)
        {
            return Result.WithFailure($"Machine {this.MachineId.Value}: cannot stage a null setting for update.");
        }

        if (item.MachineId != this.MachineId)
        {
            return Result.WithFailure(
                $"Machine {this.MachineId.Value}: setting {item.SettingId} targets machine {item.MachineId.Value}; it must match this aggregate's machine.");
        }

        if (item.SettingId <= 0)
        {
            return Result.WithFailure(
                $"Machine {this.MachineId.Value}: setting has no persisted identity; only persisted settings can be staged for update.");
        }

        if (this.pendingSettingUpdates.Exists(s => s.SettingId == item.SettingId))
        {
            return Result.WithFailure(
                $"Machine {this.MachineId.Value}: a setting update for setting {item.SettingId} is already staged.");
        }

        this.pendingSettingUpdates.Add(item);
        return Result.Success();
    }

    /// <summary>
    /// #95 Phase 2 Slice C clear-after-attempt seam. Empties the four staged sets
    /// (<see cref="PendingMachinePlcAppends"/> / <see cref="PendingMachinePlcUpdates"/> /
    /// <see cref="PendingSettingAppends"/> / <see cref="PendingSettingUpdates"/>) — called by the aggregate
    /// repository after EVERY save attempt (the ratified Slice B consumed-by-the-attempt contract): after a
    /// durable commit so a subsequent save cannot double-apply the same staged changes, and after a FAILED
    /// attempt so a later save cannot double-apply a stale batch — the caller must re-load and re-stage to
    /// retry. The loaded folds (<see cref="LoadedMachinePlcs"/> / <see cref="LoadedSettings"/>) are
    /// intentionally left untouched: they reflect the load-time snapshot and are refreshed by the next
    /// <c>LoadAsync</c>, not by a save.
    /// </summary>
    public void ClearStagedMachineChanges()
    {
        this.pendingMachinePlcAppends.Clear();
        this.pendingMachinePlcUpdates.Clear();
        this.pendingSettingAppends.Clear();
        this.pendingSettingUpdates.Clear();
    }
}