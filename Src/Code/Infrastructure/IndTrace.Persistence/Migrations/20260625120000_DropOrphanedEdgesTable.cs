// <copyright file="20260625120000_DropOrphanedEdgesTable.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IndTrace.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DropOrphanedEdgesTable : Migration
    {
        // C2 Chunk E (Migration C, REVERSIBLE): the legacy "Edges" graph table was orphaned when its C#
        // entity + DbSet were removed in Story 1.3; Chunk E cut every routing consumer over to the
        // "RoutingNodes" + clean "WorkFlows" tables, so "Edges" is now fully dead in the database. Up()
        // drops the orphaned "Edges" table outright (DropTable also removes its indexes and FKs). Down()
        // recreates it (empty) byte-faithfully — original columns, PK, the four indexes, and the three
        // foreign keys exactly as authored in 20230827231414_InitialCreate — so the migration is fully
        // reversible. No data migration — the table carries no live routing state once consumers are cut over.
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Edges");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Edges",
                columns: table => new
                {
                    EdgeId = table.Column<int>(type: "int", nullable: false),
                    FromMachineId = table.Column<int>(type: "int", nullable: false),
                    ToMachineId = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    WorkFlowId = table.Column<int>(type: "int", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK.IndTraceData.Edges.EdgeId", x => x.EdgeId);
                });

            migrationBuilder.CreateIndex(
                name: "IDX.IndTraceData.Edges.EdgeId",
                table: "Edges",
                column: "EdgeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Edges_FromMachineId",
                table: "Edges",
                column: "FromMachineId");

            migrationBuilder.CreateIndex(
                name: "IX_Edges_ToMachineId",
                table: "Edges",
                column: "ToMachineId");

            migrationBuilder.CreateIndex(
                name: "IX_Edges_WorkFlowId",
                table: "Edges",
                column: "WorkFlowId");

            migrationBuilder.AddForeignKey(
                name: "FK_Edges_Machines_FromMachineId",
                table: "Edges",
                column: "FromMachineId",
                principalTable: "Machines",
                principalColumn: "MachineId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Edges_Machines_ToMachineId",
                table: "Edges",
                column: "ToMachineId",
                principalTable: "Machines",
                principalColumn: "MachineId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Edges_WorkFlows_WorkFlowId",
                table: "Edges",
                column: "WorkFlowId",
                principalTable: "WorkFlows",
                principalColumn: "WorkFlowId");
        }
    }
}
