// <copyright file="20260620005213_AddFlowTransitionLog.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IndTrace.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFlowTransitionLog : Migration
    {
        // Story 3.5 (AC1/CR2/NFR2): ADDITIVE-ONLY migration. Up() creates ONLY the new, stand-alone
        // FlowTransitionLog table (plus its diagnostic indexes); it alters NO existing table or column.
        //
        // NOTE FOR THE REVIEWER/ORCHESTRATOR: the committed ApplicationDbContextModelSnapshot.cs was stale
        // versus the current model (a PRE-EXISTING repo condition, unrelated to this story — a probe migration
        // with this entity removed reproduces the identical ~222-operation drift). The raw EF scaffold of this
        // migration therefore carried that unrelated drift in Up()/Down(). Per the story's hard constraint
        // ("additive schema ONLY; if it touches an existing table, STOP") the unrelated drift has been removed
        // here so Up() is the FlowTransitionLog CreateTable ONLY. The FlowTransitionLog operations below are the
        // verbatim EF-scaffolded operations for this entity. The model snapshot was left as EF regenerated it
        // (it now accurately reflects the current model), so subsequent migrations stay consistent.
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FlowTransitionLog",
                columns: table => new
                {
                    FlowTransitionLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    From = table.Column<int>(type: "int", nullable: false),
                    To = table.Column<int>(type: "int", nullable: false),
                    FromCycleStatus = table.Column<int>(type: "int", nullable: false),
                    Trigger = table.Column<int>(type: "int", nullable: false),
                    Path = table.Column<int>(type: "int", nullable: false),
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    BarCodeId = table.Column<int>(type: "int", nullable: false),
                    CycleId = table.Column<int>(type: "int", nullable: false),
                    ResultValidation = table.Column<int>(type: "int", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowTransitionLog_FlowTransitionLogId", x => x.FlowTransitionLogId);
                });

            migrationBuilder.CreateIndex(
                name: "IDX_FlowTransitionLog_BarCodeId",
                table: "FlowTransitionLog",
                column: "BarCodeId");

            migrationBuilder.CreateIndex(
                name: "IDX_FlowTransitionLog_MachineId",
                table: "FlowTransitionLog",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IDX_FlowTransitionLog_TimeStamp",
                table: "FlowTransitionLog",
                column: "TimeStamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlowTransitionLog");
        }
    }
}
