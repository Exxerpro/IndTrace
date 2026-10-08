-- Prepares a freshly seeded demo database for the walkthrough videos and screenshots.
-- Idempotent: safe to run again. Run it with:  source docs/site-media/env.sh && demo_sql < docs/site-media/demo-db/prepare-demo.sql
--
-- The test fixtures are built for unit tests, not for running a line, so a seeded database needs these steps:
--   1. PLCs 100-900, one per station, enabled and bound to their machine.
--   2. The tags the simulated controller requires per PLC: exactly 4 event tags (group 1) and a reference tag
--      (group 256). The fixtures carry only register tags (group 128).
--   3. No boundary-test registers near int.MaxValue (they overflow the identity on the next insert).
--   4. No WorkFlows rows with a 0 endpoint (route authoring refuses to save while they exist).
--   5. The demo product L100003: route 100 -> 500, a label rule at the first station, and a recipe whose
--      minimum cycle time does not reject the simulated cycles.
--   6. Fictitious customer names, also stripped from product text, so no brand appears on screen.
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

-- 1. PLCs -------------------------------------------------------------------------------------------------
SET IDENTITY_INSERT Plcs ON;
DECLARE @plc int = 200;
WHILE @plc <= 900
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Plcs WHERE PlcId = @plc)
        INSERT INTO Plcs (PlcId, MachineId, Enabled, Name, IpAddress, PlcType, PlcBrand, Options, CommLibrary, BrandOwner)
        SELECT @plc, @plc, 1, Name, CONCAT('192.168.0.', @plc / 10), PlcType, PlcBrand, Options, CommLibrary, BrandOwner
        FROM Plcs WHERE PlcId = 100;
    SET @plc += 100;
END;
SET IDENTITY_INSERT Plcs OFF;
UPDATE Plcs SET Enabled = 1, MachineId = PlcId WHERE PlcId BETWEEN 100 AND 900;

-- 2. Event and reference tags ------------------------------------------------------------------------------
DECLARE @tags TABLE (Name nvarchar(50), NetType nvarchar(50), Length int, GroupId int);
INSERT INTO @tags VALUES
    (N'Command', N'System.Int16', 1, 1),
    (N'CommandFeedback', N'System.Int16', 1, 1),
    (N'HeartBeat', N'System.Int16', 1, 1),
    (N'PlcReady', N'System.Int16', 1, 1),
    (N'PartNumberReference', N'System.String', 30, 256);

INSERT INTO Variables (MachineId, PlcId, Name, Description, Alias, Address, NetType, Length, IsActive, Direction,
                       VariableGroupId, CreatedBy, ModifiedBy, CreatedOn, ModifiedOn)
SELECT p.PlcId, p.PlcId, t.Name, t.Name, CONCAT('DB1.', t.Name), CONCAT('DB1.', t.Name), t.NetType, t.Length, 1, 1,
       t.GroupId, N'Demo', N'Demo', SYSDATETIME(), SYSDATETIME()
FROM Plcs p CROSS JOIN @tags t
WHERE p.PlcId BETWEEN 100 AND 900
  AND NOT EXISTS (SELECT 1 FROM Variables v WHERE v.PlcId = p.PlcId AND v.Name = t.Name AND v.VariableGroupId = t.GroupId);

-- 3. Boundary-test registers -------------------------------------------------------------------------------
IF EXISTS (SELECT 1 FROM Registers WHERE RegisterId >= 2000000000)
BEGIN
    DELETE FROM Registers WHERE RegisterId >= 2000000000;
    DECLARE @maxRegister int = ISNULL((SELECT MAX(RegisterId) FROM Registers), 1);
    DBCC CHECKIDENT ('Registers', RESEED, @maxRegister) WITH NO_INFOMSGS;
END;

-- 4. Zero-endpoint WorkFlows -------------------------------------------------------------------------------
DELETE FROM WorkFlows WHERE LastMachineId = 0 OR NextMachineId = 0;

-- 5. Demo product L100003 ----------------------------------------------------------------------------------
DECLARE @product int = (SELECT ProductId FROM Products WHERE PartNumber = N'L100003');
IF @product IS NULL OR (SELECT COUNT(*) FROM Products WHERE PartNumber = N'L100003') <> 1
    THROW 50001, 'Expected exactly one product with part number L100003.', 1;

DELETE FROM WorkFlows WHERE ProductId = @product;
INSERT INTO WorkFlows (ProductId, LastMachineId, NextMachineId, RuleId, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
VALUES (@product, 100, 200, 0, N'Demo', SYSDATETIME(), N'Demo', SYSDATETIME()),
       (@product, 200, 300, 0, N'Demo', SYSDATETIME(), N'Demo', SYSDATETIME()),
       (@product, 300, 400, 0, N'Demo', SYSDATETIME(), N'Demo', SYSDATETIME()),
       (@product, 400, 500, 0, N'Demo', SYSDATETIME(), N'Demo', SYSDATETIME());

-- Roles: 3 = Initial|Serial, 2 = Serial, 34 = Serial|Final.
DELETE FROM RoutingNodes WHERE ProductId = @product;
INSERT INTO RoutingNodes (ProductId, MachineId, Role, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
VALUES (@product, 100, 3, N'Demo', SYSDATETIME(), N'Demo', SYSDATETIME()),
       (@product, 200, 2, N'Demo', SYSDATETIME(), N'Demo', SYSDATETIME()),
       (@product, 300, 2, N'Demo', SYSDATETIME(), N'Demo', SYSDATETIME()),
       (@product, 400, 2, N'Demo', SYSDATETIME(), N'Demo', SYSDATETIME()),
       (@product, 500, 34, N'Demo', SYSDATETIME(), N'Demo', SYSDATETIME());

-- The barcode label rule, cloned from the fixture's first rule.
IF NOT EXISTS (SELECT 1 FROM Rules WHERE MachineId = 100 AND ProductId = @product)
    INSERT INTO Rules (MachineId, ProductId, IsActive, Name, Description, RuleJson, Version, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
    SELECT TOP (1) 100, @product, 1, N'WS100_L100003', N'Demo label rule', RuleJson, Version, N'Demo', SYSDATETIME(), N'Demo', SYSDATETIME()
    FROM Rules ORDER BY RuleId;

-- Cycle times are whole seconds and the minimum is exclusive, so a short simulated cycle needs a minimum of 0.
UPDATE Recipes SET CycleTimeMinimum = 0, CycleTimeMaximum = 600 WHERE ProductId = @product;

-- 6. Fictitious customer names -----------------------------------------------------------------------------
DECLARE @names TABLE (Position int PRIMARY KEY, Name nvarchar(100));
INSERT INTO @names VALUES
    (1, N'Apex Lighting'), (2, N'Oemx'), (3, N'Orion Components'), (4, N'Northgate Auto'), (5, N'Solstice Mobility'),
    (6, N'Harbor Drive Systems'), (7, N'Granite Axle'), (8, N'Bluepeak Vehicles'), (9, N'Cobalt Trucks'),
    (10, N'Summit Electric'), (11, N'Redwood Coachworks'), (12, N'Lumen Optics'), (13, N'Ironvale Motors'),
    (14, N'Tidewater Auto'), (15, N'Vantage EV'), (16, N'Kestrel Sports Cars'), (17, N'Ashford Automotive'),
    (18, N'Highland Offroad'), (19, N'Nordic Safety Cars'), (20, N'Pacific Drive'), (21, N'Crescent Luxury'),
    (22, N'Aurora Premium'), (23, N'Keystone Motors'), (24, N'Heritage Coaches'), (25, N'Liberty Trucks'),
    (26, N'Ridgeline 4x4'), (27, N'Trailhead Pickups'), (28, N'Union Motor Works'), (29, N'Prairie Truck Co'),
    (30, N'Milano Classica'), (31, N'Torino Small Cars'), (32, N'Lyon Automobiles'), (33, N'Atlantique Motors');

-- Customers already carrying a fictitious name were renamed by an earlier run.
DECLARE @renames TABLE (CustomerId int PRIMARY KEY, OldName nvarchar(200), NewName nvarchar(100));
INSERT INTO @renames (CustomerId, OldName, NewName)
SELECT c.CustomerId, c.Name, n.Name
FROM (SELECT CustomerId, Name, ROW_NUMBER() OVER (ORDER BY CustomerId) AS Position FROM Customers) c
JOIN @names n ON n.Position = c.Position
WHERE c.Name NOT IN (SELECT Name FROM @names);

DECLARE @old nvarchar(200), @customer int;
DECLARE renames CURSOR LOCAL FAST_FORWARD FOR SELECT CustomerId, OldName FROM @renames;
OPEN renames;
FETCH NEXT FROM renames INTO @customer, @old;
WHILE @@FETCH_STATUS = 0
BEGIN
    -- Product text such as "Tail lamp for <Brand> vehicles" or "<Brand> door module" loses the brand.
    UPDATE Products SET
        ProductName = LTRIM(REPLACE(REPLACE(REPLACE(REPLACE(ProductName, N' for ' + @old + N' trucks', N''),
            N' for ' + @old + N' vehicles', N''), N'for ' + @old + N' ', N''), @old + N' ', N'')),
        Description = LTRIM(REPLACE(REPLACE(REPLACE(REPLACE(Description, N' for ' + @old + N' trucks', N''),
            N' for ' + @old + N' vehicles', N''), N'for ' + @old + N' ', N''), @old + N' ', N'')),
        CustomerPartNumber = LTRIM(REPLACE(REPLACE(REPLACE(REPLACE(CustomerPartNumber, N' for ' + @old + N' trucks', N''),
            N' for ' + @old + N' vehicles', N''), N'for ' + @old + N' ', N''), @old + N' ', N'')),
        AliasPartNumber = LTRIM(REPLACE(REPLACE(REPLACE(REPLACE(AliasPartNumber, N' for ' + @old + N' trucks', N''),
            N' for ' + @old + N' vehicles', N''), N'for ' + @old + N' ', N''), @old + N' ', N''))
    WHERE ProductName LIKE N'%' + @old + N'%' OR Description LIKE N'%' + @old + N'%'
       OR CustomerPartNumber LIKE N'%' + @old + N'%' OR AliasPartNumber LIKE N'%' + @old + N'%';
    FETCH NEXT FROM renames INTO @customer, @old;
END;
CLOSE renames;
DEALLOCATE renames;

UPDATE c SET Name = r.NewName FROM Customers c JOIN @renames r ON r.CustomerId = c.CustomerId;
UPDATE p SET CustomerName = c.Name FROM Products p JOIN Customers c ON c.CustomerId = p.CustomerId;

-- Any former customer name still left in product text is reported, not silently published.
SELECT r.OldName AS StillMentioned, COUNT(*) AS Products
FROM @renames r
JOIN Products p ON p.ProductName LIKE N'%' + r.OldName + N'%' OR p.Description LIKE N'%' + r.OldName + N'%'
               OR p.CustomerPartNumber LIKE N'%' + r.OldName + N'%' OR p.AliasPartNumber LIKE N'%' + r.OldName + N'%'
               OR p.CustomerName LIKE N'%' + r.OldName + N'%'
GROUP BY r.OldName;

COMMIT TRANSACTION;

SELECT PlcId, SUM(CASE WHEN VariableGroupId = 1 AND IsActive = 1 THEN 1 ELSE 0 END) AS EventTags,
       SUM(CASE WHEN VariableGroupId = 256 AND IsActive = 1 THEN 1 ELSE 0 END) AS ReferenceTags,
       SUM(CASE WHEN VariableGroupId = 128 AND IsActive = 1 THEN 1 ELSE 0 END) AS RegisterTags
FROM Variables WHERE PlcId BETWEEN 100 AND 900 GROUP BY PlcId ORDER BY PlcId;
PRINT 'prepare-demo: done';
