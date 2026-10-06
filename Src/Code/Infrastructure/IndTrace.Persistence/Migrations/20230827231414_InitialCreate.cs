// <copyright file="20230827231414_InitialCreate.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace IndTrace.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Config.DatabaseLog",
                columns: table => new
                {
                    DatabaseLogID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    DatabaseUser = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Event = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Schema = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Object = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    TSQL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    XmlEvent = table.Column<string>(type: "xml", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatabaseLog_DatabaseLogID", x => x.DatabaseLogID)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "ConfigApps",
                columns: table => new
                {
                    AppId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConfigAppId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    PlcId = table.Column<int>(type: "int", nullable: false),
                    Pc = table.Column<int>(type: "int", nullable: false),
                    Client = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Factory = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Line = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Project = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK.IndTraceData.ConfigApps.AppId", x => x.AppId);
                });

            migrationBuilder.CreateTable(
                name: "ConfigDbs",
                columns: table => new
                {
                    SystemInformationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DatabaseVersion = table.Column<string>(name: "Database Version", type: "nvarchar(80)", maxLength: 80, nullable: false),
                    VersionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "CycleStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK.IndTraceData.CycleStatus.RecipeId", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Defects",
                columns: table => new
                {
                    DefectId = table.Column<int>(type: "int", nullable: false),
                    DefectTypeId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: false),
                    ShortName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK.IndTraceData.Defects.DefectId", x => x.DefectId);
                });

            migrationBuilder.CreateTable(
                name: "EpDaqHealth",
                columns: table => new
                {
                    RegisterEpDaqHealtId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    FechaHoraRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK.IndTraceData.EpDaqHealth.RegisterEpDaqHealtId", x => x.RegisterEpDaqHealtId);
                });

            migrationBuilder.CreateTable(
                name: "FlowStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KPI.OEE",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Availability = table.Column<decimal>(type: "decimal(12,4)", nullable: false),
                    Performance = table.Column<decimal>(type: "decimal(12,4)", nullable: false),
                    Quality = table.Column<decimal>(type: "decimal(12,4)", nullable: false),
                    OEE = table.Column<decimal>(type: "decimal(12,4)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KPI.OEE", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "MachineType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MasterLabel",
                columns: table => new
                {
                    MasterLabelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MasterLabelCode = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: false),
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    OrderID = table.Column<int>(type: "int", nullable: false),
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    LeaderId = table.Column<int>(type: "int", nullable: false),
                    ProgrammerID = table.Column<int>(type: "int", nullable: false),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    ToolingId = table.Column<int>(type: "int", nullable: false),
                    TimeStamp = table.Column<int>(type: "int", nullable: false),
                    OrderSize = table.Column<int>(type: "int", nullable: false),
                    OrderTime = table.Column<int>(type: "int", nullable: false),
                    OrderStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrderEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResultsID = table.Column<int>(type: "int", nullable: false),
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "PartStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PerformanceSpecs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "Plcs",
                columns: table => new
                {
                    PlcId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    PlcType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    PlcBrand = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Options = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: false),
                    CommLibrary = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    BrandOwner = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK.IndTraceData.Plcs.PlcId", x => x.PlcId);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    IsActive = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    CustomerPartNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    AliasPartNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: false),
                    RuleId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK.IndTraceData.Products.ProductId", x => x.ProductId);
                });

            migrationBuilder.CreateTable(
                name: "Recipe",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK.IndTraceData.Recipes.RecipeId", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ResultValidation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResultValidation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rules",
                columns: table => new
                {
                    RuleID = table.Column<int>(type: "int", nullable: false),
                    RuleJson = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rules", x => x.RuleID);
                });

            migrationBuilder.CreateTable(
                name: "Stoppages",
                columns: table => new
                {
                    StoppageId = table.Column<int>(type: "int", nullable: false),
                    StoppageTypeId = table.Column<int>(type: "int", nullable: false),
                    StoppageName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: false),
                    Description2 = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: false),
                    ShortName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    MinValue = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    MaxValue = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    ApplyForProduction = table.Column<bool>(type: "bit", nullable: true),
                    ItemProperty = table.Column<int>(type: "int", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK.IndTraceData.Stoppages.StoppageId", x => x.StoppageId);
                });

            migrationBuilder.CreateTable(
                name: "TagsGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagsGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Toolings",
                columns: table => new
                {
                    ToolId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK.IndTraceData.Tooling.ToolId", x => x.ToolId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "VariablesGroups",
                columns: table => new
                {
                    VariableGroupId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VariableGroupName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK.IndTraceData.VariablesGroups.VariableGroupId", x => x.VariableGroupId);
                });

            migrationBuilder.CreateTable(
                name: "WorkFlowType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkFlowType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Variables",
                columns: table => new
                {
                    VariableId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    PlcId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: false),
                    Alias = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    NetType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Length = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<int>(type: "int", nullable: false),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    VariableGroupId = table.Column<int>(type: "int", nullable: false),
                    VariableSpecId = table.Column<int>(type: "int", nullable: false),
                    TagStatus = table.Column<int>(type: "int", nullable: false),
                    NativeType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    NativeAddress = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK.IndTraceData.Variables.EntitieId", x => x.VariableId);
                    table.ForeignKey(
                        name: "FK.IndTraceData.Variables.VariableGroupId",
                        column: x => x.VariableGroupId,
                        principalTable: "VariablesGroups",
                        principalColumn: "VariableGroupId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BarCodes",
                columns: table => new
                {
                    BarCodeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    PartStatus = table.Column<int>(type: "int", nullable: false),
                    FlowStatus = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK.IndTraceData.BarCodes.BarCodeId", x => x.BarCodeId);
                    table.ForeignKey(
                        name: "FK.IndTraceData.BarCodes.FlowStatus",
                        column: x => x.FlowStatus,
                        principalTable: "FlowStatus",
                        principalColumn: "RecipeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK.IndTraceData.BarCodes.PartStatus",
                        column: x => x.PartStatus,
                        principalTable: "PartStatus",
                        principalColumn: "RecipeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK.IndTraceData.BarCodes.Products",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cycles",
                columns: table => new
                {
                    CycleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    BarCodeId = table.Column<int>(type: "int", nullable: false),
                    CycleStatus = table.Column<int>(type: "int", nullable: false),
                    PartStatus = table.Column<int>(type: "int", nullable: false),
                    CycleTime = table.Column<int>(type: "int", nullable: false),
                    TaktTime = table.Column<int>(type: "int", nullable: false),
                    StartedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    FinishedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK.IndTraceData.Cycles.CycleId", x => x.CycleId);
                    table.ForeignKey(
                        name: "FK.IndTraceData.Cycles.BarCodes",
                        column: x => x.BarCodeId,
                        principalTable: "BarCodes",
                        principalColumn: "BarCodeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK.IndTraceData.Cycles.CycleStatus",
                        column: x => x.CycleStatus,
                        principalTable: "CycleStatus",
                        principalColumn: "RecipeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK.IndTraceData.Cycles.PartStatus",
                        column: x => x.PartStatus,
                        principalTable: "PartStatus",
                        principalColumn: "RecipeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Registers",
                columns: table => new
                {
                    RegisterId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    VariableID = table.Column<int>(type: "int", nullable: false),
                    CycleId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    DataType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    StatusValueId = table.Column<int>(type: "int", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK.IndTraceData.Registers.RegisterId", x => x.RegisterId);
                    table.ForeignKey(
                        name: "FK.IndTraceData.Registers.Cycles",
                        column: x => x.CycleId,
                        principalTable: "Cycles",
                        principalColumn: "CycleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK.IndTraceData.Registers.Variables",
                        column: x => x.VariableID,
                        principalTable: "Variables",
                        principalColumn: "EntitieId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DefectsRegister",
                columns: table => new
                {
                    DefectRegisterId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BarCodeId = table.Column<int>(type: "int", nullable: false),
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    DefectId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PartsQuantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK.IndTraceData.DefectsRegister.DefectRegisterId", x => x.DefectRegisterId);
                    table.ForeignKey(
                        name: "FK.IndTraceData.DefectsRegister.BarCodeId",
                        column: x => x.BarCodeId,
                        principalTable: "BarCodes",
                        principalColumn: "BarCodeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK.IndTraceData.DefectsRegister.DefectId",
                        column: x => x.DefectId,
                        principalTable: "Defects",
                        principalColumn: "DefectId",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateTable(
                name: "MachinePlcs",
                columns: table => new
                {
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    PlcId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachinePlcs", x => new { x.MachineId, x.PlcId });
                    table.ForeignKey(
                        name: "FK_MachinePlcs_Plcs_PlcId",
                        column: x => x.PlcId,
                        principalTable: "Plcs",
                        principalColumn: "PlcId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Machines",
                columns: table => new
                {
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Description1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    MachineType = table.Column<int>(type: "int", nullable: false),
                    WorkFlowType = table.Column<int>(type: "int", nullable: false),
                    EnableAppTraceability = table.Column<int>(type: "int", nullable: false, defaultValueSql: "((1))"),
                    EnableBypassTraceability = table.Column<int>(type: "int", nullable: false, defaultValueSql: "((0))"),
                    RuleId = table.Column<int>(type: "int", nullable: false),
                    WorkFlowId = table.Column<int>(type: "int", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK.IndTraceData.Machines.MachineId", x => x.MachineId);
                    table.ForeignKey(
                        name: "FK.IndTraceData.Machines.MachineType",
                        column: x => x.MachineType,
                        principalTable: "MachineType",
                        principalColumn: "RecipeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK.IndTraceData.Machines.WorkFlowType",
                        column: x => x.WorkFlowType,
                        principalTable: "WorkFlowType",
                        principalColumn: "RecipeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MachineStatus",
                columns: table => new
                {
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    StatusMachine = table.Column<int>(type: "int", nullable: false),
                    BreakDownTime = table.Column<decimal>(type: "Decimal(18,4)", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "Datetime2(7)", nullable: false),
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK.IndTraceData.MachineStatus.MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "MachineId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductSpecs",
                columns: table => new
                {
                    ProductSpecId = table.Column<int>(type: "int", nullable: false),
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    ToolId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    RecipeType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    RecipeId = table.Column<int>(type: "int", nullable: false),
                    PerformanceSpecsName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    PerformanceSpecId = table.Column<int>(type: "int", nullable: false),
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK.IndTraceData.ProductSpecs.MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "MachineId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK.IndTraceData.ProductSpecs.ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK.IndTraceData.ProductSpecs.RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipe",
                        principalColumn: "RecipeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK.IndTraceData.ProductSpecs.ToolId",
                        column: x => x.ToolId,
                        principalTable: "Toolings",
                        principalColumn: "ToolId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RegisterStoppages",
                columns: table => new
                {
                    StoppageRegisterId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionOrderId = table.Column<int>(type: "int", nullable: false),
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    StoppageId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    StoppedTime = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    StartedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())"),
                    FinishedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RegistedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK.IndTraceData.StoppagesRegister.StoppageRegisterId", x => x.StoppageRegisterId);
                    table.ForeignKey(
                        name: "FK.IndTraceData.RegisterStoppages.MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "MachineId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK.IndTraceData.RegisterStoppages.StoppageId",
                        column: x => x.StoppageId,
                        principalTable: "Stoppages",
                        principalColumn: "StoppageId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    SettingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    Config = table.Column<string>(type: "nchar(4000)", fixedLength: true, maxLength: 4000, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK.IndTraceData.Settings.SettingId", x => x.SettingId);
                    table.ForeignKey(
                        name: "FK.IndTraceData.Settings.Machines",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "MachineId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StatusConfigurations",
                columns: table => new
                {
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "Datetime2", nullable: false),
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK.IndTraceData.StatusConfigurations.MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "MachineId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StatusConnections",
                columns: table => new
                {
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK.IndTraceData.StatusConnections.MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "MachineId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkFlows",
                columns: table => new
                {
                    WorkFlowId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    NextMachineId = table.Column<int>(type: "int", nullable: false),
                    LastMachineId = table.Column<int>(type: "int", nullable: false),
                    RuleId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK.IndTraceData.WorkFlows.WorkFlowId", x => x.WorkFlowId);
                    table.ForeignKey(
                        name: "FK.IndTraceData.WorkFlows.Machines.LastMachineId",
                        column: x => x.LastMachineId,
                        principalTable: "Machines",
                        principalColumn: "MachineId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK.IndTraceData.WorkFlows.Machines.NextMachineId",
                        column: x => x.NextMachineId,
                        principalTable: "Machines",
                        principalColumn: "MachineId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IDX.IndTraceData.BarCodes.BarCodeId",
                table: "BarCodes",
                column: "BarCodeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IDX.IndTraceData.BarCodes.Label",
                table: "BarCodes",
                column: "Label",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BarCodes_FlowStatus",
                table: "BarCodes",
                column: "FlowStatus");

            migrationBuilder.CreateIndex(
                name: "IX_BarCodes_MachineId",
                table: "BarCodes",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_BarCodes_PartStatus",
                table: "BarCodes",
                column: "PartStatus");

            migrationBuilder.CreateIndex(
                name: "IX_BarCodes_ProductId",
                table: "BarCodes",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IDX.IndTraceData.ConfigApps.AppId",
                table: "ConfigApps",
                column: "AppId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IDX.IndTraceData.Cycles.BarCodeId",
                table: "Cycles",
                column: "BarCodeId");

            migrationBuilder.CreateIndex(
                name: "IDX.IndTraceData.Cycles.CycleId",
                table: "Cycles",
                column: "CycleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cycles_CycleStatus",
                table: "Cycles",
                column: "CycleStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Cycles_MachineId",
                table: "Cycles",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_Cycles_PartStatus",
                table: "Cycles",
                column: "PartStatus");

            migrationBuilder.CreateIndex(
                name: "IDX.IndTraceData.Defects.DefectId",
                table: "Defects",
                column: "DefectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IDX.IndTraceData.Defects.Name",
                table: "Defects",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DefectsRegister_BarCodeId",
                table: "DefectsRegister",
                column: "BarCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_DefectsRegister_DefectId",
                table: "DefectsRegister",
                column: "DefectId");

            migrationBuilder.CreateIndex(
                name: "IX_DefectsRegister_MachineId",
                table: "DefectsRegister",
                column: "MachineId");

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

            migrationBuilder.CreateIndex(
                name: "IX_MachinePlcs_PlcId",
                table: "MachinePlcs",
                column: "PlcId");

            migrationBuilder.CreateIndex(
                name: "IDX.IndTraceData.Machines.MachineId",
                table: "Machines",
                column: "MachineId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Machines_MachineType",
                table: "Machines",
                column: "MachineType");

            migrationBuilder.CreateIndex(
                name: "IX_Machines_WorkFlowId",
                table: "Machines",
                column: "WorkFlowId");

            migrationBuilder.CreateIndex(
                name: "IX_Machines_WorkFlowType",
                table: "Machines",
                column: "WorkFlowType");

            migrationBuilder.CreateIndex(
                name: "IX_MachineStatus_MachineId",
                table: "MachineStatus",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IDX.IndTraceData.Orders.OrderId",
                table: "Orders",
                column: "OrderID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IDX.IndTraceData.Plcs.PlcId",
                table: "Plcs",
                column: "PlcId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IDX.IndTraceData.Products.ProductId",
                table: "Products",
                column: "ProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductSpecs_MachineId",
                table: "ProductSpecs",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSpecs_ProductId",
                table: "ProductSpecs",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSpecs_RecipeId",
                table: "ProductSpecs",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSpecs_ToolId",
                table: "ProductSpecs",
                column: "ToolId");

            migrationBuilder.CreateIndex(
                name: "IDX.IndTraceData.Registers.RegisterId",
                table: "Registers",
                column: "RegisterId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Registers_CycleId",
                table: "Registers",
                column: "CycleId");

            migrationBuilder.CreateIndex(
                name: "IX_Registers_VariableID",
                table: "Registers",
                column: "VariableID");

            migrationBuilder.CreateIndex(
                name: "IX_RegisterStoppages_MachineId",
                table: "RegisterStoppages",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_RegisterStoppages_StoppageId",
                table: "RegisterStoppages",
                column: "StoppageId");

            migrationBuilder.CreateIndex(
                name: "IDX.IndTraceData.Settings.SettingId",
                table: "Settings",
                column: "SettingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Settings_MachineId",
                table: "Settings",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_StatusConfigurations_MachineId",
                table: "StatusConfigurations",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_StatusConnections_MachineId",
                table: "StatusConnections",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IDX.IndTraceData.Variables.EntitieId",
                table: "Variables",
                column: "EntitieId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Variables_VariableGroupId",
                table: "Variables",
                column: "VariableGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkFlows_LastMachineId",
                table: "WorkFlows",
                column: "LastMachineId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkFlows_NextMachineId",
                table: "WorkFlows",
                column: "NextMachineId");

            migrationBuilder.AddForeignKey(
                name: "FK.IndTraceData.BarCodes.Machines",
                table: "BarCodes",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "MachineId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK.IndTraceData.Cycles.Machines",
                table: "Cycles",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "MachineId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK.IndTraceData.DefectsRegister.MachineId",
                table: "DefectsRegister",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "MachineId",
                onDelete: ReferentialAction.Cascade);

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

            migrationBuilder.AddForeignKey(
                name: "FK_MachinePlcs_Machines_MachineId",
                table: "MachinePlcs",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "MachineId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Machines_WorkFlows_WorkFlowId",
                table: "Machines",
                column: "WorkFlowId",
                principalTable: "WorkFlows",
                principalColumn: "WorkFlowId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK.IndTraceData.WorkFlows.Machines.LastMachineId",
                table: "WorkFlows");

            migrationBuilder.DropForeignKey(
                name: "FK.IndTraceData.WorkFlows.Machines.NextMachineId",
                table: "WorkFlows");

            migrationBuilder.DropTable(
                name: "Config.DatabaseLog");

            migrationBuilder.DropTable(
                name: "ConfigApps");

            migrationBuilder.DropTable(
                name: "ConfigDbs");

            migrationBuilder.DropTable(
                name: "DefectsRegister");

            migrationBuilder.DropTable(
                name: "Edges");

            migrationBuilder.DropTable(
                name: "EpDaqHealth");

            migrationBuilder.DropTable(
                name: "KPI.OEE");

            migrationBuilder.DropTable(
                name: "MachinePlcs");

            migrationBuilder.DropTable(
                name: "MachineStatus");

            migrationBuilder.DropTable(
                name: "MasterLabel");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "PerformanceSpecs");

            migrationBuilder.DropTable(
                name: "ProductSpecs");

            migrationBuilder.DropTable(
                name: "Registers");

            migrationBuilder.DropTable(
                name: "RegisterStoppages");

            migrationBuilder.DropTable(
                name: "ResultValidation");

            migrationBuilder.DropTable(
                name: "Rules");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "StatusConfigurations");

            migrationBuilder.DropTable(
                name: "StatusConnections");

            migrationBuilder.DropTable(
                name: "TagsGroups");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Defects");

            migrationBuilder.DropTable(
                name: "Plcs");

            migrationBuilder.DropTable(
                name: "Recipe");

            migrationBuilder.DropTable(
                name: "Toolings");

            migrationBuilder.DropTable(
                name: "Cycles");

            migrationBuilder.DropTable(
                name: "Variables");

            migrationBuilder.DropTable(
                name: "Stoppages");

            migrationBuilder.DropTable(
                name: "BarCodes");

            migrationBuilder.DropTable(
                name: "CycleStatus");

            migrationBuilder.DropTable(
                name: "VariablesGroups");

            migrationBuilder.DropTable(
                name: "FlowStatus");

            migrationBuilder.DropTable(
                name: "PartStatus");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Machines");

            migrationBuilder.DropTable(
                name: "MachineType");

            migrationBuilder.DropTable(
                name: "WorkFlowType");

            migrationBuilder.DropTable(
                name: "WorkFlows");
        }
    }
}