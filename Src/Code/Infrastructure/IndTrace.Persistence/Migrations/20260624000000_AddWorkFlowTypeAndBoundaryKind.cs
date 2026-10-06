// <copyright file="20260624000000_AddWorkFlowTypeAndBoundaryKind.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IndTrace.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkFlowTypeAndBoundaryKind : Migration
    {
        // Story 3.1 (PURE ADDITIVE): Up() adds ONLY the two new, additive columns to the existing
        // "WorkFlows" table — "WorkFlowType" (a plain int holding a WorkFlowType bitmask value, mapped
        // in C# to WorkFlow.WorkFlowTypeValue to avoid shadowing the enum type; NOT an FK) and
        // "BoundaryKind" (an int boundary marker for the Story-3.2 reversible migration). Both are
        // non-null with defaultValue 0 so existing rows backfill to 0 (= WorkFlowType.None / boundary
        // None). No data migration, no behavior change, no FK changes. Down() drops both columns —
        // fully reversible.
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WorkFlowType",
                table: "WorkFlows",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BoundaryKind",
                table: "WorkFlows",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkFlowType",
                table: "WorkFlows");

            migrationBuilder.DropColumn(
                name: "BoundaryKind",
                table: "WorkFlows");
        }
    }
}
