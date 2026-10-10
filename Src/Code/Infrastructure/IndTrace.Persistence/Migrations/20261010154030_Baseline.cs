// <copyright file="20261010154030_Baseline.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IndTrace.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Baseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Config.DatabaseLog",
                columns: table => new
                {
                    DatabaseLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DatabaseUser = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Event = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Schema = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Object = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Tsql = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    XmlEvent = table.Column<string>(type: "xml", maxLength: 8000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatabaseLog_DatabaseLogID", x => x.DatabaseLogId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "ConfigApps",
                columns: table => new
                {
                    AppId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConfigAppId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    Machine = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    PlcId = table.Column<int>(type: "int", nullable: false),
                    Pc = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Client = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Factory = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Line = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Project = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK.IndTraceData.ConfigApps.AppId", x => x.AppId);
                });

            migrationBuilder.CreateTable(
                name: "ConfigDbs",
                columns: table => new
                {
                    SystemInformationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DatabaseVersion = table.Column<string>(name: "Database Version", type: "nvarchar(80)", maxLength: 80, nullable: false),
                    VersionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigDbs", x => x.SystemInformationId);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    CustomerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK.IndTraceData.Customer.CustomerId", x => x.CustomerId);
                });

            migrationBuilder.CreateTable(
                name: "CycleStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK.IndTraceData.CycleStatus.id", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Defects",
                columns: table => new
                {
                    DefectId = table.Column<int>(type: "int", nullable: false),
                    DefectTypeId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: false),
                    ShortName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK.IndTraceData.Defects.DefectId", x => x.DefectId);
                });

            migrationBuilder.CreateTable(
                name: "DistinctRegisters",
                columns: table => new
                {
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    VariableId = table.Column<int>(type: "int", nullable: false),
                    MachineId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistinctRegisters", x => new { x.Name, x.VariableId, x.MachineId });
                });

            migrationBuilder.CreateTable(
                name: "FlowStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowStatus", x => x.Id);
                });

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

            migrationBuilder.CreateTable(
                name: "GatewayTask",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK.IndTraceData.GatewayTask.RecipeId", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lines",
                columns: table => new
                {
                    LineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK.IndTraceData.Line.LineId", x => x.LineId);
                });

            migrationBuilder.CreateTable(
                name: "MachineType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false)
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
                    Description = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterLabel", x => x.MasterLabelId);
                });

            migrationBuilder.CreateTable(
                name: "OeeRegisters",
                columns: table => new
                {
                    OeeRegisterId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    PlcId = table.Column<int>(type: "int", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    ApplicationFlag = table.Column<int>(type: "int", nullable: false),
                    EventCounter = table.Column<int>(type: "int", nullable: false),
                    CurrentTime = table.Column<int>(type: "int", nullable: false),
                    RunningTime = table.Column<int>(type: "int", nullable: false),
                    StoppedTime = table.Column<int>(type: "int", nullable: false),
                    FaultedTime = table.Column<int>(type: "int", nullable: false),
                    StatusFaultReason = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    TotalProduction = table.Column<double>(type: "float(18)", precision: 18, scale: 6, nullable: false),
                    StandardCycleTime = table.Column<double>(type: "float(18)", precision: 18, scale: 6, nullable: false),
                    ActualCycleTime = table.Column<double>(type: "float(18)", precision: 18, scale: 6, nullable: false),
                    PlanedProductionTime = table.Column<double>(type: "float(18)", precision: 18, scale: 6, nullable: false),
                    RejectEventCounter = table.Column<int>(type: "int", nullable: false),
                    StatusReject = table.Column<int>(type: "int", nullable: false),
                    RejectQuantityUnits = table.Column<double>(type: "float", nullable: false),
                    ProductionOk = table.Column<double>(type: "float", nullable: false),
                    ProductionNoK = table.Column<double>(type: "float", nullable: false),
                    Oee = table.Column<double>(type: "float(18)", precision: 18, scale: 6, nullable: false),
                    Availability = table.Column<double>(type: "float(18)", precision: 18, scale: 6, nullable: false),
                    Performance = table.Column<double>(type: "float(18)", precision: 18, scale: 6, nullable: false),
                    Quality = table.Column<double>(type: "float(18)", precision: 18, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK.IndTraceData.OeeRegisters.OeeRegisterId", x => x.OeeRegisterId);
                });

            migrationBuilder.CreateTable(
                name: "PartStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PerformanceSpecs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformanceSpecs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Plcs",
                columns: table => new
                {
                    PlcId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    Enabled = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    PlcType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    PlcBrand = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Options = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: false),
                    CommLibrary = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    BrandOwner = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false)
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
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    LineId = table.Column<int>(type: "int", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK.IndTraceData.Products.ProductId", x => x.ProductId);
                });

            migrationBuilder.CreateTable(
                name: "Recipes",
                columns: table => new
                {
                    RecipeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    CycleTimeMinimum = table.Column<int>(type: "int", nullable: false),
                    CycleTimeMaximum = table.Column<int>(type: "int", nullable: false),
                    MaxCyclesOk = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    MaxCyclesNOk = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    Retry = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK.IndTraceData.Recipes.RecipeId", x => x.RecipeId);
                });

            migrationBuilder.CreateTable(
                name: "ResultValidation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResultValidation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Shifts",
                columns: table => new
                {
                    ShiftId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    StartBy = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    ShiftType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaxDuration = table.Column<TimeSpan>(type: "time", nullable: false),
                    MinDuration = table.Column<TimeSpan>(type: "time", nullable: false),
                    NormalDuration = table.Column<TimeSpan>(type: "time", nullable: false),
                    CyclesOk = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK.IndTraceData.Shifts.ShiftID", x => x.ShiftId);
                });

            migrationBuilder.CreateTable(
                name: "ShiftsCatalog",
                columns: table => new
                {
                    ShiftCatalogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlantId = table.Column<int>(type: "int", nullable: false),
                    ShiftName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StartBy = table.Column<TimeSpan>(type: "time", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftsCatalog", x => x.ShiftCatalogId);
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
                    MinValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    MaxValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    ApplyForProduction = table.Column<bool>(type: "bit", nullable: true),
                    ItemProperty = table.Column<int>(type: "int", nullable: false)
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
                    DisplayName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagsGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaskGatewayRequests",
                columns: table => new
                {
                    CommandId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    BarCodeId = table.Column<int>(type: "int", nullable: false),
                    CycleId = table.Column<int>(type: "int", nullable: false),
                    CycleStatus = table.Column<int>(type: "int", nullable: false),
                    PartStatus = table.Column<int>(type: "int", nullable: false),
                    FlowStatus = table.Column<int>(type: "int", nullable: false),
                    ResultValidation = table.Column<int>(type: "int", nullable: false),
                    GatewayTask = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK.IndTraceData.TaskGatewayRequests.CommandId", x => x.CommandId);
                });

            migrationBuilder.CreateTable(
                name: "Toolings",
                columns: table => new
                {
                    ToolId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK.IndTraceData.Toolings.ToolId", x => x.ToolId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
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
                    CreatedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK.IndTraceData.Variables.EntitieId", x => x.VariableId);
                });

            migrationBuilder.CreateTable(
                name: "VariablesGroups",
                columns: table => new
                {
                    VariableGroupId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VariableGroupName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false)
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
                    DisplayName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkFlowType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KpiOees",
                columns: table => new
                {
                    KpiOeeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OeeRegisterId = table.Column<int>(type: "int", nullable: false),
                    Oee = table.Column<decimal>(type: "decimal(12,4)", nullable: false),
                    Availability = table.Column<decimal>(type: "decimal(12,4)", nullable: false),
                    Performance = table.Column<decimal>(type: "decimal(12,4)", nullable: false),
                    Quality = table.Column<decimal>(type: "decimal(12,4)", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK.IndTraceData.KpiOee.KpiOeeId", x => x.KpiOeeId);
                    table.ForeignKey(
                        name: "FK_KpiOees_OeeRegisters_OeeRegisterId",
                        column: x => x.OeeRegisterId,
                        principalTable: "OeeRegisters",
                        principalColumn: "OeeRegisterId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskGatewayResponses",
                columns: table => new
                {
                    ResponseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommandId = table.Column<int>(type: "int", nullable: false),
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    BarCodeId = table.Column<int>(type: "int", nullable: false),
                    CycleId = table.Column<int>(type: "int", nullable: false),
                    CyclesOk = table.Column<int>(type: "int", nullable: false),
                    ShiftId = table.Column<int>(type: "int", nullable: false),
                    ResultValidation = table.Column<int>(type: "int", nullable: false),
                    PartNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Error = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: false),
                    LastMachineId = table.Column<int>(type: "int", nullable: false),
                    NextMachineId = table.Column<int>(type: "int", nullable: false),
                    CycleStatus = table.Column<int>(type: "int", nullable: false),
                    FlowStatus = table.Column<int>(type: "int", nullable: false),
                    PartStatus = table.Column<int>(type: "int", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK.IndTraceData.TaskGatewayResponses.ResponseId", x => x.ResponseId);
                    table.ForeignKey(
                        name: "FK.IndTraceData.TaskGatewayResponses.CommandId",
                        column: x => x.CommandId,
                        principalTable: "TaskGatewayRequests",
                        principalColumn: "CommandId",
                        onDelete: ReferentialAction.Cascade);
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
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK.IndTraceData.BarCodes.BarCodeId", x => x.BarCodeId);
                    table.ForeignKey(
                        name: "FK.IndTraceData.BarCodes.Products",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CycleCompletion",
                columns: table => new
                {
                    CycleCompletionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CycleId = table.Column<int>(type: "int", nullable: false),
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    CompletedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK.IndTraceData.CycleCompletion.CycleCompletionId", x => x.CycleCompletionId);
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
                    CyclesOk = table.Column<int>(type: "int", nullable: false),
                    PartStatus = table.Column<int>(type: "int", nullable: false),
                    CycleTime = table.Column<int>(type: "int", nullable: false),
                    TaktTime = table.Column<int>(type: "int", nullable: false),
                    StartedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    FinishedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "PerformanceDatas",
                columns: table => new
                {
                    PerformanceDataId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    PlcId = table.Column<int>(type: "int", nullable: false),
                    BarCodeId = table.Column<int>(type: "int", nullable: false),
                    CycleId = table.Column<int>(type: "int", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    ApplicationFlag = table.Column<int>(type: "int", nullable: false),
                    EventCounter = table.Column<int>(type: "int", nullable: false),
                    CurrentTime = table.Column<int>(type: "int", nullable: false),
                    RunningTime = table.Column<int>(type: "int", nullable: false),
                    StoppedTime = table.Column<int>(type: "int", nullable: false),
                    FaultedTime = table.Column<int>(type: "int", nullable: false),
                    StatusFaultReason = table.Column<int>(type: "int", nullable: false),
                    TotalProduction = table.Column<double>(type: "float(18)", precision: 18, scale: 6, nullable: false),
                    ProductionOk = table.Column<double>(type: "float(18)", precision: 18, scale: 6, nullable: false),
                    ProductionNoK = table.Column<double>(type: "float(18)", precision: 18, scale: 6, nullable: false),
                    StatusFaultReject = table.Column<int>(type: "int", nullable: false),
                    RejectEventCounter = table.Column<int>(type: "int", nullable: false),
                    StatusReject = table.Column<int>(type: "int", nullable: false),
                    RejectQuantityUnits = table.Column<double>(type: "float", nullable: false),
                    StandardCycleTime = table.Column<double>(type: "float", nullable: false),
                    ActualCycleTime = table.Column<double>(type: "float", nullable: false),
                    PlanedProductionTime = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK.IndTraceData.PerformanceDatas.PerformanceDataId", x => x.PerformanceDataId);
                    table.ForeignKey(
                        name: "FK.IndTraceData.PerformanceDatas.Cycles",
                        column: x => x.CycleId,
                        principalTable: "Cycles",
                        principalColumn: "CycleId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Registers",
                columns: table => new
                {
                    RegisterId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: false),
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    VariableId = table.Column<int>(type: "int", nullable: false),
                    CycleId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    DataType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    StatusValueId = table.Column<int>(type: "int", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "datetime2(7)", nullable: false)
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
                        column: x => x.VariableId,
                        principalTable: "Variables",
                        principalColumn: "VariableId",
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
                    PartsQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
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
                name: "MachinePlcs",
                columns: table => new
                {
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    PlcId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
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
                name: "MachineStatus",
                columns: table => new
                {
                    StatusMachineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    BreakDownTime = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineStatus", x => x.StatusMachineId);
                });

            migrationBuilder.CreateTable(
                name: "Machines",
                columns: table => new
                {
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    MachineType = table.Column<int>(type: "int", nullable: false),
                    WorkFlowType = table.Column<int>(type: "int", nullable: false),
                    EnableAppTraceability = table.Column<int>(type: "int", nullable: false, defaultValueSql: "((1))"),
                    EnableBypassTraceability = table.Column<int>(type: "int", nullable: false, defaultValueSql: "((0))"),
                    Retry = table.Column<int>(type: "int", nullable: false, defaultValueSql: "((1))"),
                    RuleId = table.Column<int>(type: "int", nullable: false),
                    WorkFlowId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK.IndTraceData.Machines.MachineId", x => x.MachineId);
                });

            migrationBuilder.CreateTable(
                name: "ProductSpecs",
                columns: table => new
                {
                    ProductSpecId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    ToolId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    RecipeType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    RecipeId = table.Column<int>(type: "int", nullable: false),
                    PerformanceSpecsName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    PerformanceSpecId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductSpecs", x => x.ProductSpecId);
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
                        principalTable: "Recipes",
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
                    StoppedTime = table.Column<decimal>(type: "decimal(10,4)", precision: 10, scale: 4, nullable: false),
                    StartedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())"),
                    FinishedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RegistedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                name: "RoutingNodes",
                columns: table => new
                {
                    RoutingNodeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "Rules",
                columns: table => new
                {
                    RuleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RuleJson = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rules", x => x.RuleId);
                    table.ForeignKey(
                        name: "FK.IndTraceData.Rules.Machines",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "MachineId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK.IndTraceData.Rules.Products",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    SettingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    Config = table.Column<string>(type: "nchar(4000)", fixedLength: true, maxLength: 4000, nullable: false)
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
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                    CreatedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
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
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
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
                name: "IDX.IndTraceData.BarCodes.CreatedOn",
                table: "BarCodes",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IDX.IndTraceData.BarCodes.Label",
                table: "BarCodes",
                column: "Label",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BarCodes_MachineId",
                table: "BarCodes",
                column: "MachineId");

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
                name: "UX.IndTraceData.CycleCompletion.CycleId",
                table: "CycleCompletion",
                column: "CycleId",
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
                name: "IX_Cycles_MachineId",
                table: "Cycles",
                column: "MachineId");

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

            migrationBuilder.CreateIndex(
                name: "IDX.IndTraceData.KpiOee.KpiOeeId",
                table: "KpiOees",
                column: "KpiOeeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KpiOees_OeeRegisterId",
                table: "KpiOees",
                column: "OeeRegisterId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MachinePlcs_PlcId",
                table: "MachinePlcs",
                column: "PlcId");

            migrationBuilder.CreateIndex(
                name: "IX_MachineStatus_MachineId",
                table: "MachineStatus",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IDX.IndTraceData.Machines.MachineId",
                table: "Machines",
                column: "MachineId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Machines_WorkFlowId",
                table: "Machines",
                column: "WorkFlowId");

            migrationBuilder.CreateIndex(
                name: "IDX.IndTraceData.OeeRegisters.RegisterId",
                table: "OeeRegisters",
                column: "OeeRegisterId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OeeRegisters_Name_MachineId",
                table: "OeeRegisters",
                columns: new[] { "PlcId", "MachineId" });

            migrationBuilder.CreateIndex(
                name: "IX_OeeRegisters_TimeStamp",
                table: "OeeRegisters",
                column: "TimeStamp");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceDatas_CycleId",
                table: "PerformanceDatas",
                column: "CycleId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceDatas_MachineId_PlcId",
                table: "PerformanceDatas",
                columns: new[] { "MachineId", "PlcId" });

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceDatas_TimeStamp",
                table: "PerformanceDatas",
                column: "TimeStamp");

            migrationBuilder.CreateIndex(
                name: "IDX.IndTraceData.Plcs.PlcId",
                table: "Plcs",
                column: "PlcId",
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
                name: "IDX.IndTraceData.Customer.CustomerId",
                table: "Products",
                column: "CustomerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IDX.IndTraceData.Products.ProductId",
                table: "Products",
                column: "ProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegisterStoppages_MachineId",
                table: "RegisterStoppages",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_RegisterStoppages_StoppageId",
                table: "RegisterStoppages",
                column: "StoppageId");

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
                name: "IX_Registers_Name_MachineId",
                table: "Registers",
                columns: new[] { "Name", "MachineId" });

            migrationBuilder.CreateIndex(
                name: "IX_Registers_TimeStamp",
                table: "Registers",
                column: "TimeStamp");

            migrationBuilder.CreateIndex(
                name: "IX_Registers_VariableID",
                table: "Registers",
                column: "VariableId");

            migrationBuilder.CreateIndex(
                name: "IX_RoutingNodes_MachineId",
                table: "RoutingNodes",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "UX.IndTraceData.RoutingNodes.ProductId.MachineId",
                table: "RoutingNodes",
                columns: new[] { "ProductId", "MachineId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rules_MachineId",
                table: "Rules",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_Rules_ProductId",
                table: "Rules",
                column: "ProductId");

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
                name: "UX.IndTraceData.Shifts.MachineId_StartBy",
                table: "Shifts",
                columns: new[] { "MachineId", "StartBy" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StatusConfigurations_MachineId",
                table: "StatusConfigurations",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_StatusConnections_MachineId",
                table: "StatusConnections",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IDX.IndTraceData.TaskGatewayRequests.BarCodeId",
                table: "TaskGatewayRequests",
                column: "BarCodeId");

            migrationBuilder.CreateIndex(
                name: "IDX.IndTraceData.TaskGatewayRequests.CycleId",
                table: "TaskGatewayRequests",
                column: "CycleId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskGatewayResponses_CommandId",
                table: "TaskGatewayResponses",
                column: "CommandId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IDX.IndTraceData.Variables.EntitieId",
                table: "Variables",
                column: "VariableId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_MachinePlcNameAddressVariableGroup",
                table: "Variables",
                columns: new[] { "MachineId", "PlcId", "Name", "Address", "VariableGroupId" },
                unique: true);

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
                name: "FK.IndTraceData.CycleCompletion.Cycles",
                table: "CycleCompletion",
                column: "CycleId",
                principalTable: "Cycles",
                principalColumn: "CycleId",
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
                name: "FK_MachinePlcs_Machines_MachineId",
                table: "MachinePlcs",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "MachineId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK.IndTraceData.MachineStatus.MachineId",
                table: "MachineStatus",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "MachineId",
                onDelete: ReferentialAction.Restrict);

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
                name: "Customers");

            migrationBuilder.DropTable(
                name: "CycleCompletion");

            migrationBuilder.DropTable(
                name: "CycleStatus");

            migrationBuilder.DropTable(
                name: "DefectsRegister");

            migrationBuilder.DropTable(
                name: "DistinctRegisters");

            migrationBuilder.DropTable(
                name: "FlowStatus");

            migrationBuilder.DropTable(
                name: "FlowTransitionLog");

            migrationBuilder.DropTable(
                name: "GatewayTask");

            migrationBuilder.DropTable(
                name: "KpiOees");

            migrationBuilder.DropTable(
                name: "Lines");

            migrationBuilder.DropTable(
                name: "MachinePlcs");

            migrationBuilder.DropTable(
                name: "MachineStatus");

            migrationBuilder.DropTable(
                name: "MachineType");

            migrationBuilder.DropTable(
                name: "MasterLabel");

            migrationBuilder.DropTable(
                name: "PartStatus");

            migrationBuilder.DropTable(
                name: "PerformanceDatas");

            migrationBuilder.DropTable(
                name: "PerformanceSpecs");

            migrationBuilder.DropTable(
                name: "ProductSpecs");

            migrationBuilder.DropTable(
                name: "RegisterStoppages");

            migrationBuilder.DropTable(
                name: "Registers");

            migrationBuilder.DropTable(
                name: "ResultValidation");

            migrationBuilder.DropTable(
                name: "RoutingNodes");

            migrationBuilder.DropTable(
                name: "Rules");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "Shifts");

            migrationBuilder.DropTable(
                name: "ShiftsCatalog");

            migrationBuilder.DropTable(
                name: "StatusConfigurations");

            migrationBuilder.DropTable(
                name: "StatusConnections");

            migrationBuilder.DropTable(
                name: "TagsGroups");

            migrationBuilder.DropTable(
                name: "TaskGatewayResponses");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "VariablesGroups");

            migrationBuilder.DropTable(
                name: "WorkFlowType");

            migrationBuilder.DropTable(
                name: "Defects");

            migrationBuilder.DropTable(
                name: "OeeRegisters");

            migrationBuilder.DropTable(
                name: "Plcs");

            migrationBuilder.DropTable(
                name: "Recipes");

            migrationBuilder.DropTable(
                name: "Toolings");

            migrationBuilder.DropTable(
                name: "Stoppages");

            migrationBuilder.DropTable(
                name: "Cycles");

            migrationBuilder.DropTable(
                name: "Variables");

            migrationBuilder.DropTable(
                name: "TaskGatewayRequests");

            migrationBuilder.DropTable(
                name: "BarCodes");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Machines");

            migrationBuilder.DropTable(
                name: "WorkFlows");
        }
    }
}
