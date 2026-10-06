CREATE OR ALTER TRIGGER trg_Products_ValidateCustomerMention
ON [dbo].[Products]
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted i
        INNER JOIN [dbo].[Customers] c
            ON (
                LOWER(i.ProductName) LIKE '%' + LOWER(c.Name) + '%'
                OR LOWER(i.Description) LIKE '%' + LOWER(c.Name) + '%'
            )
            AND i.CustomerId <> c.CustomerId
    )
    BEGIN
        RAISERROR('Product mentions a customer name but CustomerId does not match.', 16, 1);
        ROLLBACK;
        RETURN;
    END
END;
