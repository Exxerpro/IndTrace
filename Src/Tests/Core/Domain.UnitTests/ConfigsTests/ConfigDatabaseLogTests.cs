// <copyright file="ConfigDatabaseLogTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.ConfigsTests;

/// <summary>
/// Unit tests for ConfigDatabaseLog - Database configuration log entity for audit and monitoring.
/// Tests property validation, audit logging scenarios, database event tracking, and XML handling.
/// </summary>
public class ConfigDatabaseLogTests
{
    /// <summary>
    /// Executes ConfigDatabaseLog_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully operation.
    /// </summary>
    [Fact]
    public void ConfigDatabaseLog_WithAllRequiredProperties_ShouldCreateInstanceSuccessfully()
    {
        // Arrange & Act
        var instance = new ConfigDatabaseLog();

        // Assert
        instance.ShouldNotBeNull();
        instance.ShouldBeAssignableTo<IEntityRoot>();
        instance.DatabaseLogId.ShouldBe(0);
        instance.PostTime.ShouldBe(default(DateTime));
        instance.DatabaseUser.ShouldBe(string.Empty);
        instance.Event.ShouldBe(string.Empty);
        instance.Schema.ShouldBe(string.Empty);
        instance.Object.ShouldBe(string.Empty);
        instance.Tsql.ShouldBe(string.Empty);
        instance.XmlEvent.ShouldBe(string.Empty);
    }
    /// <summary>
    /// Executes ConfigDatabaseLog_Constructor_WhenCreated_ShouldImplementIEntityRoot operation.
    /// </summary>

    [Fact]
    public void ConfigDatabaseLog_Constructor_WhenCreated_ShouldImplementIEntityRoot()
    {
        // Arrange & Act
        var dbLog = new ConfigDatabaseLog();

        // Assert
        dbLog.ShouldBeAssignableTo<IEntityRoot>();
    }
    /// <summary>
    /// Executes DatabaseLogId_WhenSetToValidValues_ShouldReturnCorrectValue operation.
    /// </summary>
    /// <param name="logId">The logId.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(999)]
    [InlineData(0)]
    public void DatabaseLogId_WhenSetToValidValues_ShouldReturnCorrectValue(int logId)
    {
        // Arrange
        var dbLog = new ConfigDatabaseLog();

        // Act
        dbLog.DatabaseLogId = logId;

        // Assert
        dbLog.DatabaseLogId.ShouldBe(logId);
    }
    /// <summary>
    /// Executes PostTime_WhenSetToValidValues_ShouldReturnCorrectValue operation.
    /// </summary>
    /// <param name="dateTimeString">The dateTimeString.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData("2025-06-15 08:00:00")]
    [InlineData("2025-06-15 16:30:00")]
    [InlineData("2025-12-31 23:59:59")]
    public void PostTime_WhenSetToValidValues_ShouldReturnCorrectValue(string dateTimeString)
    {
        // Arrange
        var dbLog = new ConfigDatabaseLog();
        var postTime = DateTime.Parse(dateTimeString);

        // Act
        dbLog.PostTime = postTime;

        // Assert
        dbLog.PostTime.ShouldBe(postTime);
    }
    /// <summary>
    /// Executes DatabaseUser_WhenSetToValidValues_ShouldReturnCorrectValue operation.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData("sa")]
    [InlineData("manufacturinguser")]
    [InlineData("qc_inspector")]
    [InlineData("app_service")]
    [InlineData("IndTrace_App")]
    public void DatabaseUser_WhenSetToValidValues_ShouldReturnCorrectValue(string user)
    {
        // Arrange
        var dbLog = new ConfigDatabaseLog();

        // Act
        dbLog.DatabaseUser = user;

        // Assert
        dbLog.DatabaseUser.ShouldBe(user);
    }
    /// <summary>
    /// Executes Event_WhenSetToValidValues_ShouldReturnCorrectValue operation.
    /// </summary>
    /// <param name="eventType">The eventType.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData("INSERT")]
    [InlineData("UPDATE")]
    [InlineData("DELETE")]
    [InlineData("SELECT")]
    [InlineData("ALTER_TABLE")]
    [InlineData("CREATE_INDEX")]
    public void Event_WhenSetToValidValues_ShouldReturnCorrectValue(string eventType)
    {
        // Arrange
        var dbLog = new ConfigDatabaseLog();

        // Act
        dbLog.Event = eventType;

        // Assert
        dbLog.Event.ShouldBe(eventType);
    }
    /// <summary>
    /// Executes Schema_WhenSetToValidValues_ShouldReturnCorrectValue operation.
    /// </summary>
    /// <param name="schema">The schema.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData("dbo")]
    [InlineData("IndTrace")]
    [InlineData("Config")]
    [InlineData("Manufacturing")]
    [InlineData("Audit")]
    public void Schema_WhenSetToValidValues_ShouldReturnCorrectValue(string schema)
    {
        // Arrange
        var dbLog = new ConfigDatabaseLog();

        // Act
        dbLog.Schema = schema;

        // Assert
        dbLog.Schema.ShouldBe(schema);
    }
    /// <summary>
    /// Executes Object_WhenSetToValidValues_ShouldReturnCorrectValue operation.
    /// </summary>
    /// <param name="objectName">The objectName.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData("Machines")]
    [InlineData("BarCodes")]
    [InlineData("ConfigApps")]
    [InlineData("IX_Machines_Name")]
    [InlineData("sp_GetProductionData")]
    public void Object_WhenSetToValidValues_ShouldReturnCorrectValue(string objectName)
    {
        // Arrange
        var dbLog = new ConfigDatabaseLog();

        // Act
        dbLog.Object = objectName;

        // Assert
        dbLog.Object.ShouldBe(objectName);
    }
    /// <summary>
    /// Executes Tsql_WhenSetToValidValues_ShouldReturnCorrectValue operation.
    /// </summary>
    /// <param name="tsqlStatement">The tsqlStatement.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData("INSERT INTO Machines (Name) VALUES ('Machine001')")]
    [InlineData("UPDATE BarCodes SET Status = 'Processed' WHERE UserId = 123")]
    [InlineData("DELETE FROM Cycles WHERE CreatedOn < '2024-01-01'")]
    [InlineData("SELECT * FROM ConfigApps WHERE Client = 'Ford'")]
    public void Tsql_WhenSetToValidValues_ShouldReturnCorrectValue(string tsqlStatement)
    {
        // Arrange
        var dbLog = new ConfigDatabaseLog();

        // Act
        dbLog.Tsql = tsqlStatement;

        // Assert
        dbLog.Tsql.ShouldBe(tsqlStatement);
    }
    /// <summary>
    /// Executes XmlEvent_WhenSetToValidValues_ShouldReturnCorrectValue operation.
    /// </summary>
    /// <param name="xmlEvent">The xmlEvent.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData("<event><action>INSERT</action><table>Machines</table></event>")]
    [InlineData("<audit><user>system</user><timestamp>2025-06-15T08:00:00</timestamp></audit>")]
    [InlineData("<manufacturing><line>A1</line><product>F150</product></manufacturing>")]
    public void XmlEvent_WhenSetToValidValues_ShouldReturnCorrectValue(string xmlEvent)
    {
        // Arrange
        var dbLog = new ConfigDatabaseLog();

        // Act
        dbLog.XmlEvent = xmlEvent;

        // Assert
        dbLog.XmlEvent.ShouldBe(xmlEvent);
    }
    /// <summary>
    /// Executes ConfigDatabaseLog_Properties_WithManufacturingAuditScenario_ShouldConfigureCorrectly operation.
    /// </summary>

    [Fact]
    public void ConfigDatabaseLog_Properties_WithManufacturingAuditScenario_ShouldConfigureCorrectly()
    {
        // Arrange
        var dbLog = new ConfigDatabaseLog();
        var eventTime = new DateTime(2025, 6, 15, 10, 30, 0);

        // Act - Configure for manufacturing audit log
        dbLog.DatabaseLogId = 1001;
        dbLog.PostTime = eventTime;
        dbLog.DatabaseUser = "IndTrace_Manufacturing";
        dbLog.Event = "INSERT";
        dbLog.Schema = "dbo";
        dbLog.Object = "Cycles";
        dbLog.Tsql = "INSERT INTO Cycles (MachineId, PartNumber, Status) VALUES (100, 'F150-DOOR-001', 'Started')";
        dbLog.XmlEvent = "<event><action>INSERT</action><table>Cycles</table><machine>100</machine><part>F150-DOOR-001</part></event>";

        // Assert
        dbLog.DatabaseLogId.ShouldBe(1001);
        dbLog.PostTime.ShouldBe(eventTime);
        dbLog.DatabaseUser.ShouldBe("IndTrace_Manufacturing");
        dbLog.Event.ShouldBe("INSERT");
        dbLog.Schema.ShouldBe("dbo");
        dbLog.Object.ShouldBe("Cycles");
        dbLog.Tsql.ShouldBe("INSERT INTO Cycles (MachineId, PartNumber, Status) VALUES (100, 'F150-DOOR-001', 'Started')");
        dbLog.XmlEvent.ShouldBe("<event><action>INSERT</action><table>Cycles</table><machine>100</machine><part>F150-DOOR-001</part></event>");
    }
    /// <summary>
    /// Executes ConfigDatabaseLog_Properties_WithQualityControlAuditScenario_ShouldConfigureCorrectly operation.
    /// </summary>

    [Fact]
    public void ConfigDatabaseLog_Properties_WithQualityControlAuditScenario_ShouldConfigureCorrectly()
    {
        // Arrange
        var dbLog = new ConfigDatabaseLog();
        var eventTime = new DateTime(2025, 6, 15, 14, 15, 30);

        // Act - Configure for quality control audit
        dbLog.DatabaseLogId = 2002;
        dbLog.PostTime = eventTime;
        dbLog.DatabaseUser = "QC_Inspector";
        dbLog.Event = "UPDATE";
        dbLog.Schema = "Quality";
        dbLog.Object = "DefectRegisters";
        dbLog.Tsql = "UPDATE DefectRegisters SET Status = 'Resolved' WHERE DefectId = 456";
        dbLog.XmlEvent = "<qc_event><inspector>QC_Inspector</inspector><defect_id>456</defect_id><resolution>Resolved</resolution></qc_event>";

        // Assert
        dbLog.DatabaseLogId.ShouldBe(2002);
        dbLog.PostTime.ShouldBe(eventTime);
        dbLog.DatabaseUser.ShouldBe("QC_Inspector");
        dbLog.Event.ShouldBe("UPDATE");
        dbLog.Schema.ShouldBe("Quality");
        dbLog.Object.ShouldBe("DefectRegisters");
        dbLog.Tsql.ShouldBe("UPDATE DefectRegisters SET Status = 'Resolved' WHERE DefectId = 456");
        dbLog.XmlEvent.ShouldBe("<qc_event><inspector>QC_Inspector</inspector><defect_id>456</defect_id><resolution>Resolved</resolution></qc_event>");
    }
    /// <summary>
    /// Executes DatabaseUser_WithEdgeCaseValues_ShouldStoreCorrectly operation.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("VeryLongUserNameThatExceedsTypicalDatabaseUsernameLengthLimitsForManufacturingSystemsAndQualityControlApplications")]
    public void DatabaseUser_WithEdgeCaseValues_ShouldStoreCorrectly(string? value)
    {
        // Arrange
        var dbLog = new ConfigDatabaseLog();

        // Act
        dbLog.DatabaseUser = value!;

        // Assert
        dbLog.DatabaseUser.ShouldBe(value);
    }
    /// <summary>
    /// Executes Tsql_WithEdgeCaseValues_ShouldStoreCorrectly operation.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("SELECT * FROM VeryLongTableNameWithManyColumnsAndComplexJoinsForManufacturingDataAnalysisAndReporting")]
    public void Tsql_WithEdgeCaseValues_ShouldStoreCorrectly(string? value)
    {
        // Arrange
        var dbLog = new ConfigDatabaseLog();

        // Act
        dbLog.Tsql = value!;

        // Assert
        dbLog.Tsql.ShouldBe(value);
    }
    /// <summary>
    /// Executes XmlEvent_WithEdgeCaseValues_ShouldStoreCorrectly operation.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="description">The description.</param>

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("<root></root>")]
    [InlineData("<complex><data><manufacturing><line>A1</line><machines><machine id='100'><name>Press001</name></machine></machines></manufacturing></data></complex>")]
    public void XmlEvent_WithEdgeCaseValues_ShouldStoreCorrectly(string? value)
    {
        // Arrange
        var dbLog = new ConfigDatabaseLog();

        // Act
        dbLog.XmlEvent = value!;

        // Assert
        dbLog.XmlEvent.ShouldBe(value);
    }
}
