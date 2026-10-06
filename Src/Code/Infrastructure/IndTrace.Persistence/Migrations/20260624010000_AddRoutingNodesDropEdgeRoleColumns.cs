// <copyright file="20260624010000_AddRoutingNodesDropEdgeRoleColumns.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IndTrace.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRoutingNodesDropEdgeRoleColumns : Migration
    {
        // C2 Chunk A (REVERSIBLE): the routing redesign moves a node's composable routing role off the
        // edge (WorkFlow) row and onto a first-class node table keyed by (ProductId, MachineId). Up()
        // creates the new "RoutingNodes" table (with its Machine FK and the unique (ProductId, MachineId)
        // index) and then drops the two Story-3.1 columns the redesign superseded — "WorkFlowType" and
        // "BoundaryKind" — from the existing "WorkFlows" table. No data migration, no consumer changes.
        // Down() re-adds those two WorkFlow columns (non-null, defaultValue 0) and drops the RoutingNodes
        // table — fully reversible.
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoutingNodes",
                columns: table => new
                {
                    RoutingNodeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK.IndTraceData.RoutingNodes.RoutingNodeId", x => x.RoutingNodeId);
                    table.ForeignKey(
                        name: "FK.IndTraceData.RoutingNodes.Machines.MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "MachineId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "UX.IndTraceData.RoutingNodes.ProductId.MachineId",
                table: "RoutingNodes",
                columns: new[] { "ProductId", "MachineId" },
                unique: true);

            migrationBuilder.DropColumn(
                name: "WorkFlowType",
                table: "WorkFlows");

            migrationBuilder.DropColumn(
                name: "BoundaryKind",
                table: "WorkFlows");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropTable(
                name: "RoutingNodes");
        }
    }
}
