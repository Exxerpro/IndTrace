-- Removes what a previous recording created, so the videos can be recorded again:
-- the product TL-2040 added by define-routing.js, with its route, recipe and rules.
SET NOCOUNT ON;
DECLARE @product int = (SELECT ProductId FROM Products WHERE PartNumber = N'TL-2040');
IF @product IS NOT NULL
BEGIN
    DELETE FROM WorkFlows WHERE ProductId = @product;
    DELETE FROM RoutingNodes WHERE ProductId = @product;
    DELETE FROM Recipes WHERE ProductId = @product;
    DELETE FROM Rules WHERE ProductId = @product;
    DELETE FROM Products WHERE ProductId = @product;
END;
PRINT 'reset-media: done';
