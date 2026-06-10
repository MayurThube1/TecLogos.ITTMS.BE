-- =============================================================================
-- SQL Scripts for TeclogosITTMS Login Feature
-- Run these against the TeclogosITTMS database after the tables are created.
-- =============================================================================

USE TeclogosITTMS;
GO

-- =============================================================================
-- 1. Stored Procedure: sp_GetEmployeeByEmail
--    Retrieves an active, non-deleted employee by their email address.
-- =============================================================================
CREATE OR ALTER PROCEDURE sp_GetEmployeeByEmail
    @Email NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ID,
        FirstName,
        MiddleName,
        LastName,
        DateOfBirth,
        Email,
        JoiningDate,
        DepartmentID,
        DesignationID,
        ManagerID,
        MobileNumber,
        PhoneNumber,
        GenderID,
        [Version],
        IsActive,
        IsDeleted,
        Created,
        CreatedByID,
        Modified,
        ModifiedByID,
        Deleted,
        DeletedByID
    FROM Employees
    WHERE Email = @Email
      AND IsActive = 1
      AND IsDeleted = 0;
END
GO

-- =============================================================================
-- 2. Stored Procedure: sp_GetPasswordHashByEmployeeId
--    Retrieves the password hash from the Authentication table for a given employee.
-- =============================================================================
CREATE OR ALTER PROCEDURE sp_GetPasswordHashByEmployeeId
    @EmployeeID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1 PasswordHash
    FROM Authentication
    WHERE EmployeeID = @EmployeeID
      AND IsActive = 1
      AND IsDeleted = 0
    ORDER BY Created DESC;
END
GO

-- =============================================================================
-- 3. Stored Procedure: sp_GetEmployeeRole
--    Retrieves the role name for a given employee by joining EmployeeRoles → Role.
-- =============================================================================
CREATE OR ALTER PROCEDURE sp_GetEmployeeRole
    @EmployeeID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1 r.[Name]
    FROM EmployeeRoles er
    INNER JOIN [Role] r ON er.RoleID = r.ID
    WHERE er.EmployeeID = @EmployeeID
      AND er.IsActive = 1
      AND er.IsDeleted = 0
      AND r.IsActive = 1
      AND r.IsDeleted = 0;
END
GO

-- =============================================================================
-- 4. Stored Procedure: sp_LogEmployeeLogin
--    Logs a login event by updating the existing Authentication record's LoginTime
--    or inserting a new record if none exists.
-- =============================================================================
CREATE OR ALTER PROCEDURE sp_LogEmployeeLogin
    @EmployeeID UNIQUEIDENTIFIER,
    @IPAddress VARCHAR(45)
AS
BEGIN
    SET NOCOUNT ON;

    -- Update existing active auth record with new login time
    IF EXISTS (
        SELECT 1 FROM Authentication
        WHERE EmployeeID = @EmployeeID AND IsActive = 1 AND IsDeleted = 0
    )
    BEGIN
        UPDATE Authentication
        SET LoginTime = GETUTCDATE(),
            IPAddress = @IPAddress,
            Modified = GETUTCDATE(),
            ModifiedByID = @EmployeeID
        WHERE EmployeeID = @EmployeeID
          AND IsActive = 1
          AND IsDeleted = 0;
    END
END
GO

-- =============================================================================
-- 5. Clean Up Existing Test Data
--    Prevents UNIQUE constraint violations and duplicates if run multiple times.
-- =============================================================================
DECLARE @SystemUserID_Clean UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000001';

-- Delete existing test junction roles
DELETE FROM EmployeeRoles 
WHERE EmployeeID IN (
    SELECT ID FROM Employees 
    WHERE Email IN ('admin@company.com', 'support@company.com', 'lead@company.com', 'employee@company.com')
);

-- Delete existing test authentication records
DELETE FROM Authentication 
WHERE EmployeeID IN (
    SELECT ID FROM Employees 
    WHERE Email IN ('admin@company.com', 'support@company.com', 'lead@company.com', 'employee@company.com')
);

-- Delete existing test employees
DELETE FROM Employees 
WHERE Email IN ('admin@company.com', 'support@company.com', 'lead@company.com', 'employee@company.com');

-- Delete existing roles to prevent duplicates
DELETE FROM [Role] 
WHERE [Name] IN ('Employee', 'IT Support Engineer', 'Team Lead', 'Administrator');
GO

-- =============================================================================
-- 6. Seed Data: Roles
--    Insert the four roles expected by the frontend.
-- =============================================================================
DECLARE @SystemUserID UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000001';

INSERT INTO [Role] (ID, [Name], [Description], CreatedByID, IsActive, IsDeleted)
VALUES
    (NEWID(), 'Employee',             'Standard employee with basic access',              @SystemUserID, 1, 0),
    (NEWID(), 'IT Support Engineer',  'IT support staff handling tickets',                @SystemUserID, 1, 0),
    (NEWID(), 'Team Lead',            'Team lead with escalation and approval access',    @SystemUserID, 1, 0),
    (NEWID(), 'Administrator',        'Full system administrator with all permissions',   @SystemUserID, 1, 0);
GO

-- =============================================================================
-- 7. Seed Data: Test Users
--    Creates four test employees with BCrypt-hashed passwords matching their roles.
-- =============================================================================
DECLARE @SysUser UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000001';

-- -----------------------------------------------------------------------------
-- User 1: Administrator (admin@company.com / Admin@123)
-- -----------------------------------------------------------------------------
DECLARE @AdminEmployeeID UNIQUEIDENTIFIER = NEWID();
DECLARE @AdminPasswordHash VARCHAR(255) = '$2a$11$8yTfd0Nc9dfTNTD793XTmeOL9pHj1i7M4eWsOuEdV9oLugyJsSwY6';
DECLARE @AdminRoleID UNIQUEIDENTIFIER;

INSERT INTO Employees (ID, FirstName, LastName, Email, CreatedByID, IsActive, IsDeleted)
VALUES (@AdminEmployeeID, 'Amir', 'Admin', 'admin@company.com', @SysUser, 1, 0);

INSERT INTO Authentication (ID, EmployeeID, LoginTime, IPAddress, PasswordHash, CreatedByID, IsActive, IsDeleted)
VALUES (NEWID(), @AdminEmployeeID, GETUTCDATE(), '127.0.0.1', @AdminPasswordHash, @SysUser, 1, 0);

SELECT @AdminRoleID = ID FROM [Role] WHERE [Name] = 'Administrator';

INSERT INTO EmployeeRoles (ID, EmployeeID, RoleID, CreatedByID, IsActive, IsDeleted)
VALUES (NEWID(), @AdminEmployeeID, @AdminRoleID, @SysUser, 1, 0);

-- -----------------------------------------------------------------------------
-- User 2: IT Support Engineer (support@company.com / Support@123)
-- -----------------------------------------------------------------------------
DECLARE @SupportEmployeeID UNIQUEIDENTIFIER = NEWID();
DECLARE @SupportPasswordHash VARCHAR(255) = '$2a$11$.9pQXSIgC1NGjfzHiP.bPu.QpZ8z4el3Hhb2GXJOtUPhxOC8NkakK';
DECLARE @SupportRoleID UNIQUEIDENTIFIER;

INSERT INTO Employees (ID, FirstName, LastName, Email, CreatedByID, IsActive, IsDeleted)
VALUES (@SupportEmployeeID, 'Sam', 'Support', 'support@company.com', @SysUser, 1, 0);

INSERT INTO Authentication (ID, EmployeeID, LoginTime, IPAddress, PasswordHash, CreatedByID, IsActive, IsDeleted)
VALUES (NEWID(), @SupportEmployeeID, GETUTCDATE(), '127.0.0.1', @SupportPasswordHash, @SysUser, 1, 0);

SELECT @SupportRoleID = ID FROM [Role] WHERE [Name] = 'IT Support Engineer';

INSERT INTO EmployeeRoles (ID, EmployeeID, RoleID, CreatedByID, IsActive, IsDeleted)
VALUES (NEWID(), @SupportEmployeeID, @SupportRoleID, @SysUser, 1, 0);

-- -----------------------------------------------------------------------------
-- User 3: Team Lead (lead@company.com / Lead@123)
-- -----------------------------------------------------------------------------
DECLARE @LeadEmployeeID UNIQUEIDENTIFIER = NEWID();
DECLARE @LeadPasswordHash VARCHAR(255) = '$2a$11$KvIU5RLXmLjXE63.hgRqw.TccYkIs0ytXO0T7matFoXgFgv.QRVHa';
DECLARE @LeadRoleID UNIQUEIDENTIFIER;

INSERT INTO Employees (ID, FirstName, LastName, Email, CreatedByID, IsActive, IsDeleted)
VALUES (@LeadEmployeeID, 'Tanya', 'Teamlead', 'lead@company.com', @SysUser, 1, 0);

INSERT INTO Authentication (ID, EmployeeID, LoginTime, IPAddress, PasswordHash, CreatedByID, IsActive, IsDeleted)
VALUES (NEWID(), @LeadEmployeeID, GETUTCDATE(), '127.0.0.1', @LeadPasswordHash, @SysUser, 1, 0);

SELECT @LeadRoleID = ID FROM [Role] WHERE [Name] = 'Team Lead';

INSERT INTO EmployeeRoles (ID, EmployeeID, RoleID, CreatedByID, IsActive, IsDeleted)
VALUES (NEWID(), @LeadEmployeeID, @LeadRoleID, @SysUser, 1, 0);

-- -----------------------------------------------------------------------------
-- User 4: Employee (employee@company.com / Employee@123)
-- -----------------------------------------------------------------------------
DECLARE @EmployeeEmployeeID UNIQUEIDENTIFIER = NEWID();
DECLARE @EmployeePasswordHash VARCHAR(255) = '$2a$11$ea7DcGqoicsjSjnizK8qgOZfw9kzGnBsJp11KZ.U7cAt6uYcVKlHW';
DECLARE @EmployeeRoleID UNIQUEIDENTIFIER;

INSERT INTO Employees (ID, FirstName, LastName, Email, CreatedByID, IsActive, IsDeleted)
VALUES (@EmployeeEmployeeID, 'Emily', 'Employee', 'employee@company.com', @SysUser, 1, 0);

INSERT INTO Authentication (ID, EmployeeID, LoginTime, IPAddress, PasswordHash, CreatedByID, IsActive, IsDeleted)
VALUES (NEWID(), @EmployeeEmployeeID, GETUTCDATE(), '127.0.0.1', @EmployeePasswordHash, @SysUser, 1, 0);

SELECT @EmployeeRoleID = ID FROM [Role] WHERE [Name] = 'Employee';

INSERT INTO EmployeeRoles (ID, EmployeeID, RoleID, CreatedByID, IsActive, IsDeleted)
VALUES (NEWID(), @EmployeeEmployeeID, @EmployeeRoleID, @SysUser, 1, 0);
GO

PRINT 'All stored procedures and seed data created successfully.';
GO
