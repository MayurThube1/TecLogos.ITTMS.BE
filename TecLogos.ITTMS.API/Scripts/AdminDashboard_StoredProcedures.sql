-- =============================================================================
-- Admin Dashboard Stored Procedures
-- Database: TeclogosITTMS
-- Run after CreateTable.sql, ForeignKey.sql, StoredProcedures.sql,
--           Sp_EmployeeDash.sql, and Sp_TeamleadDash.sql
-- =============================================================================

USE TeclogosITTMS;
GO

-- =============================================================================
-- 1. sp_GetAdminDashboardSummary
--    Returns top-level statistics cards for the Admin Dashboard:
--      - TotalEmployees       : active employees in the system
--      - OpenTickets          : tickets with Status IN ('Open', 'In Progress')
--      - ResolvedTickets      : tickets with Status IN ('Resolved', 'Closed')
--      - TotalAssets          : active assets in Asset_Master
--      - PendingAssetRequests : asset requests awaiting approval
--      - SLABreachedCount     : open/in-progress tickets with SLA breach
-- =============================================================================
CREATE OR ALTER PROCEDURE sp_GetAdminDashboardSummary
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        -- Active employee count
        (SELECT COUNT(*)
         FROM   Employees
         WHERE  IsActive = 1 AND IsDeleted = 0)                              AS TotalEmployees,

        -- Open / In-Progress tickets
        (SELECT COUNT(*)
         FROM   Ticket_Request
         WHERE  IsActive = 1 AND IsDeleted = 0
           AND  Status IN ('Open','In Progress'))                             AS OpenTickets,

        -- Resolved / Closed tickets
        (SELECT COUNT(*)
         FROM   Ticket_Request
         WHERE  IsActive = 1 AND IsDeleted = 0
           AND  Status IN ('Resolved','Closed'))                              AS ResolvedTickets,

        -- Total active assets
        (SELECT COUNT(*)
         FROM   Asset_Master
         WHERE  IsActive = 1 AND IsDeleted = 0)                              AS TotalAssets,

        -- Pending asset requests (ManagerApprovalStatus = 'Pending')
        (SELECT COUNT(*)
         FROM   Asset_Request
         WHERE  IsActive = 1 AND IsDeleted = 0
           AND  ManagerApprovalStatus = 'Pending')                            AS PendingAssetRequests,

        -- SLA-breached open tickets
        (SELECT COUNT(*)
         FROM   Ticket_Request t
         INNER JOIN Ticket_SLA_Tracking sla
                ON sla.TicketId = t.Id AND sla.IsActive = 1 AND sla.IsDeleted = 0
         WHERE  t.IsActive = 1 AND t.IsDeleted = 0
           AND  t.Status IN ('Open','In Progress')
           AND  sla.IsResolutionBreached = 1)                                 AS SLABreachedCount;
END
GO

-- =============================================================================
-- 2. sp_GetAllEmployees
--    Returns paged employee list with optional department, status, and search
--    filters. Returns 2 result sets: TotalCount (set 1) and paged rows (set 2).
-- =============================================================================
CREATE OR ALTER PROCEDURE sp_GetAllEmployees
    @Department  NVARCHAR(100) = NULL,
    @IsActive    BIT           = NULL,
    @Search      NVARCHAR(200) = NULL,
    @PageNumber  INT           = 1,
    @PageSize    INT           = 15
AS
BEGIN
    SET NOCOUNT ON;

    -- Result set 1: TotalCount
    SELECT COUNT(*) AS TotalCount
    FROM Employees e
    LEFT JOIN Departments d ON d.ID = e.DepartmentID AND d.IsActive = 1 AND d.IsDeleted = 0
    LEFT JOIN EmployeeRoles er ON er.EmployeeID = e.ID AND er.IsActive = 1 AND er.IsDeleted = 0
    LEFT JOIN [Role] r ON r.ID = er.RoleID AND r.IsActive = 1 AND r.IsDeleted = 0
    WHERE e.IsDeleted = 0
      AND (@Department IS NULL OR d.DepartmentName = @Department)
      AND (@IsActive IS NULL OR e.IsActive = @IsActive)
      AND (@Search IS NULL OR (
          e.FirstName LIKE '%' + @Search + '%'
          OR e.LastName LIKE '%' + @Search + '%'
          OR e.Email LIKE '%' + @Search + '%'
      ));

    -- Result set 2: Paged rows
    SELECT
        e.ID                                              AS Id,
        CONCAT(
            ISNULL(e.FirstName, ''),
            CASE WHEN e.MiddleName IS NOT NULL
                      AND LEN(LTRIM(RTRIM(e.MiddleName))) > 0
                 THEN ' ' + e.MiddleName ELSE '' END,
            ' ',
            ISNULL(e.LastName, '')
        )                                                  AS FullName,
        e.Email,
        d.DepartmentName                                   AS Department,
        des.DesignationName                                AS Designation,
        r.[Name]                                           AS [Role],
        e.IsActive,
        e.JoiningDate
    FROM Employees e
    LEFT JOIN Departments d ON d.ID = e.DepartmentID AND d.IsActive = 1 AND d.IsDeleted = 0
    LEFT JOIN Designations des ON des.ID = e.DesignationID AND des.IsActive = 1 AND des.IsDeleted = 0
    LEFT JOIN EmployeeRoles er ON er.EmployeeID = e.ID AND er.IsActive = 1 AND er.IsDeleted = 0
    LEFT JOIN [Role] r ON r.ID = er.RoleID AND r.IsActive = 1 AND r.IsDeleted = 0
    WHERE e.IsDeleted = 0
      AND (@Department IS NULL OR d.DepartmentName = @Department)
      AND (@IsActive IS NULL OR e.IsActive = @IsActive)
      AND (@Search IS NULL OR (
          e.FirstName LIKE '%' + @Search + '%'
          OR e.LastName LIKE '%' + @Search + '%'
          OR e.Email LIKE '%' + @Search + '%'
      ))
    ORDER BY e.Created DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- =============================================================================
-- 3. sp_GetEmployeeByIdAdmin
--    Returns detailed employee info for Admin view, including aggregated
--    ticket and asset counts.
-- =============================================================================
CREATE OR ALTER PROCEDURE sp_GetEmployeeByIdAdmin
    @EmployeeID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        e.ID                                              AS Id,
        CONCAT(
            ISNULL(e.FirstName, ''),
            CASE WHEN e.MiddleName IS NOT NULL
                      AND LEN(LTRIM(RTRIM(e.MiddleName))) > 0
                 THEN ' ' + e.MiddleName ELSE '' END,
            ' ',
            ISNULL(e.LastName, '')
        )                                                  AS FullName,
        e.Email,
        e.MobileNumber,
        d.DepartmentName                                   AS Department,
        des.DesignationName                                AS Designation,
        r.[Name]                                           AS [Role],
        CONCAT(
            ISNULL(m.FirstName, ''),
            ' ',
            ISNULL(m.LastName, '')
        )                                                  AS ManagerName,
        e.IsActive,
        e.JoiningDate,

        -- Aggregate counts
        (SELECT COUNT(*)
         FROM   Ticket_Request
         WHERE  EmployeeId = e.ID AND IsActive = 1 AND IsDeleted = 0)       AS TotalTicketsRaised,

        (SELECT COUNT(*)
         FROM   Ticket_Request
         WHERE  EmployeeId = e.ID AND IsActive = 1 AND IsDeleted = 0
           AND  Status IN ('Open','In Progress'))                             AS OpenTickets,

        (SELECT COUNT(*)
         FROM   Asset_Allocation
         WHERE  EmployeeId = e.ID AND AllocationStatus = 'Active'
           AND  IsActive = 1 AND IsDeleted = 0)                              AS AssetsAllocated

    FROM Employees e
    LEFT JOIN Departments d ON d.ID = e.DepartmentID AND d.IsActive = 1 AND d.IsDeleted = 0
    LEFT JOIN Designations des ON des.ID = e.DesignationID AND des.IsActive = 1 AND des.IsDeleted = 0
    LEFT JOIN EmployeeRoles er ON er.EmployeeID = e.ID AND er.IsActive = 1 AND er.IsDeleted = 0
    LEFT JOIN [Role] r ON r.ID = er.RoleID AND r.IsActive = 1 AND r.IsDeleted = 0
    LEFT JOIN Employees m ON m.ID = e.ManagerID
    WHERE e.ID = @EmployeeID AND e.IsDeleted = 0;
END
GO

-- =============================================================================
-- 4. sp_ToggleEmployeeStatus
--    Toggles an employee's IsActive flag (activate/deactivate).
-- =============================================================================
CREATE OR ALTER PROCEDURE sp_ToggleEmployeeStatus
    @EmployeeID  UNIQUEIDENTIFIER,
    @UpdatedByID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Employees
    SET IsActive    = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END,
        Modified    = GETUTCDATE(),
        ModifiedByID = @UpdatedByID
    WHERE ID = @EmployeeID AND IsDeleted = 0;

    SELECT @@ROWCOUNT AS Result;
END
GO

-- =============================================================================
-- 5. sp_GetAllTicketsAdmin
--    Returns all tickets with filters and pagination for Admin view.
--    Adds Department filter compared to Team Lead's sp_GetAllTickets.
--    Returns 2 result sets: TotalCount (set 1) and paged rows (set 2).
-- =============================================================================
CREATE OR ALTER PROCEDURE sp_GetAllTicketsAdmin
    @Status      VARCHAR(50)          = NULL,
    @Priority    VARCHAR(50)          = NULL,
    @Department  NVARCHAR(100)        = NULL,
    @AssignedTo  UNIQUEIDENTIFIER     = NULL,
    @PageNumber  INT                  = 1,
    @PageSize    INT                  = 15
AS
BEGIN
    SET NOCOUNT ON;

    -- Result set 1: TotalCount
    SELECT COUNT(*) AS TotalCount
    FROM Ticket_Request t
    WHERE t.IsActive = 1 AND t.IsDeleted = 0
      AND (@Status IS NULL OR t.Status = @Status)
      AND (@Priority IS NULL OR t.Priority = @Priority)
      AND (@Department IS NULL OR t.Department = @Department)
      AND (@AssignedTo IS NULL OR t.AssignedTo = @AssignedTo);

    -- Result set 2: Paged rows
    SELECT
        t.Id,
        t.Number,
        t.Category,
        t.SubCategory,
        t.Priority,
        t.Subject,
        t.Status,
        t.Created       AS RaisedOn,
        t.Modified      AS LastUpdated,
        t.EmployeeName,
        t.Department,
        CONCAT(e.FirstName, ' ', e.LastName) AS AssignedToName,
        ISNULL(sla.IsResolutionBreached, 0)  AS IsSLABreached,
        sla.ResolutionDueDate
    FROM Ticket_Request t
    LEFT JOIN Employees e
           ON e.ID        = t.AssignedTo
          AND e.IsActive  = 1
          AND e.IsDeleted = 0
    LEFT JOIN Ticket_SLA_Tracking sla
           ON sla.TicketId  = t.Id
          AND sla.IsActive  = 1
          AND sla.IsDeleted = 0
    WHERE t.IsActive = 1 AND t.IsDeleted = 0
      AND (@Status IS NULL OR t.Status = @Status)
      AND (@Priority IS NULL OR t.Priority = @Priority)
      AND (@Department IS NULL OR t.Department = @Department)
      AND (@AssignedTo IS NULL OR t.AssignedTo = @AssignedTo)
    ORDER BY
        CASE t.Priority
            WHEN 'Critical' THEN 1
            WHEN 'High' THEN 2
            WHEN 'Medium' THEN 3
            WHEN 'Low' THEN 4
            ELSE 5
        END ASC,
        t.Created DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- =============================================================================
-- 6. sp_GetAdminTicketDetail
--    Returns full detail for a single ticket including comments and attachments.
--    Admin has unrestricted access (no EmployeeID ownership check).
--    Returns 3 result sets: ticket header, comments, attachments.
-- =============================================================================
CREATE OR ALTER PROCEDURE sp_GetAdminTicketDetail
    @TicketID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    -- Result set 1: Ticket header
    SELECT
        t.Id,
        t.Number,
        t.RequestType,
        t.Category,
        t.SubCategory,
        t.Priority,
        t.Subject,
        t.Description,
        t.Status,
        t.Department,
        t.Location,
        t.ContactNumber,
        t.EmailId,
        t.Created,
        t.Modified          AS LastUpdated,

        -- Assigned engineer
        CONCAT(
            ISNULL(eng.FirstName, ''),
            CASE WHEN eng.MiddleName IS NOT NULL
                      AND LEN(LTRIM(RTRIM(eng.MiddleName))) > 0
                 THEN ' ' + eng.MiddleName ELSE '' END,
            ' ',
            ISNULL(eng.LastName, '')
        )                   AS AssignedToName,
        eng.Email           AS AssignedToEmail,

        -- SLA info
        sla.ResolutionDueDate,
        sla.ResponseDueDate,
        sla.ResolutionStatus,
        ISNULL(sla.IsResolutionBreached, 0) AS IsSLABreached,

        -- Feedback
        CASE WHEN fb.ID IS NOT NULL THEN 1 ELSE 0 END AS FeedbackSubmitted,
        fb.Rating           AS FeedbackRating,
        fb.Comments         AS FeedbackComments

    FROM Ticket_Request t
    LEFT JOIN Employees eng
           ON eng.ID = t.AssignedTo AND eng.IsActive = 1 AND eng.IsDeleted = 0
    LEFT JOIN Ticket_SLA_Tracking sla
           ON sla.TicketId = t.Id AND sla.IsActive = 1 AND sla.IsDeleted = 0
    LEFT JOIN Ticket_Feedback fb
           ON fb.TicketID = t.Id AND fb.IsActive = 1 AND fb.IsDeleted = 0
    WHERE t.Id = @TicketID AND t.IsActive = 1 AND t.IsDeleted = 0;

    -- Result set 2: Comments (public-facing only; WorkNotes excluded)
    SELECT
        c.ID,
        c.CommentType,
        c.CommentText,
        c.Created           AS CommentedAt,
        CONCAT(
            ISNULL(ce.FirstName, ''),
            ' ',
            ISNULL(ce.LastName, '')
        )                   AS CommentedByName,
        CASE WHEN c.CommentType = 'WorkNote' THEN 1 ELSE 0 END AS IsInternal
    FROM  Ticket_Comments c
    JOIN  Employees ce
           ON ce.ID        = c.CommentedByID
          AND ce.IsActive  = 1
          AND ce.IsDeleted = 0
    WHERE c.TicketID  = @TicketID
      AND c.CommentType <> 'WorkNote'
      AND c.IsActive  = 1
      AND c.IsDeleted = 0
    ORDER BY c.Created ASC;

    -- Result set 3: Attachments
    SELECT
        a.ID,
        a.FileName,
        a.FileSize          AS FileSizeBytes,
        a.Created           AS UploadedAt
    FROM  Ticket_Attachments a
    WHERE a.TicketID  = @TicketID
      AND a.IsActive  = 1
      AND a.IsDeleted = 0
    ORDER BY a.Created ASC;
END
GO

-- =============================================================================
-- 7. sp_UpdateTicketStatusAdmin
--    Admin override: changes a ticket's status. Optionally adds a remark
--    as a system comment.
-- =============================================================================
CREATE OR ALTER PROCEDURE sp_UpdateTicketStatusAdmin
    @TicketID    UNIQUEIDENTIFIER,
    @Status      VARCHAR(50),
    @UpdatedByID UNIQUEIDENTIFIER,
    @Remarks     NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Ticket_Request
    SET Status       = @Status,
        Modified     = GETUTCDATE(),
        ModifiedByID = @UpdatedByID
    WHERE Id = @TicketID AND IsActive = 1 AND IsDeleted = 0;

    -- Add remark as a system comment if provided
    IF @Remarks IS NOT NULL AND LEN(@Remarks) > 0
    BEGIN
        INSERT INTO Ticket_Comments (
            ID, TicketID, CommentType, CommentText, CommentedByID,
            [Version], IsActive, IsDeleted, Created, CreatedByID
        )
        VALUES (
            NEWID(), @TicketID, 'CustomerVisible',
            CONCAT('Status changed to ', @Status, '. Remarks: ', @Remarks),
            @UpdatedByID,
            1, 1, 0, GETUTCDATE(), @UpdatedByID
        );
    END

    SELECT @@ROWCOUNT AS Result;
END
GO

-- =============================================================================
-- 8. sp_ReassignTicketAdmin
--    Reassigns a ticket to a different support engineer. Logs assignment
--    in Ticket_Assignment_Log and inserts a system comment.
-- =============================================================================
CREATE OR ALTER PROCEDURE sp_ReassignTicketAdmin
    @TicketID       UNIQUEIDENTIFIER,
    @AssignedToID   UNIQUEIDENTIFIER,
    @ReassignedByID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    -- Update ticket assignment
    UPDATE Ticket_Request
    SET AssignedTo   = @AssignedToID,
        Status       = CASE WHEN Status = 'Open' THEN 'In Progress' ELSE Status END,
        Modified     = GETUTCDATE(),
        ModifiedByID = @ReassignedByID
    WHERE Id = @TicketID AND IsActive = 1 AND IsDeleted = 0;

    -- Log reassignment
    INSERT INTO Ticket_Assignment_Log (
        ID, TicketID, AssignedToID, AssignedByID, AssignedAt,
        [Version], IsActive, IsDeleted, Created, CreatedByID
    )
    VALUES (
        NEWID(), @TicketID, @AssignedToID, @ReassignedByID, GETUTCDATE(),
        1, 1, 0, GETUTCDATE(), @ReassignedByID
    );

    -- Insert system comment
    DECLARE @EngineerName NVARCHAR(200);
    SELECT @EngineerName = CONCAT(FirstName, ' ', LastName)
    FROM Employees
    WHERE ID = @AssignedToID;

    INSERT INTO Ticket_Comments (
        ID, TicketID, CommentType, CommentText, CommentedByID,
        [Version], IsActive, IsDeleted, Created, CreatedByID
    )
    VALUES (
        NEWID(), @TicketID, 'CustomerVisible',
        CONCAT('Ticket has been reassigned to support engineer: ', ISNULL(@EngineerName, 'Unassigned')),
        @ReassignedByID,
        1, 1, 0, GETUTCDATE(), @ReassignedByID
    );

    SELECT @@ROWCOUNT AS Result;
END
GO

-- =============================================================================
-- 9. sp_GetAllAssets
--    Returns paged asset list with optional type, status, and search filters.
--    Returns 2 result sets: TotalCount (set 1) and paged rows (set 2).
-- =============================================================================
CREATE OR ALTER PROCEDURE sp_GetAllAssets
    @AssetType  VARCHAR(50)   = NULL,
    @Status     VARCHAR(50)   = NULL,
    @Search     NVARCHAR(200) = NULL,
    @PageNumber INT           = 1,
    @PageSize   INT           = 15
AS
BEGIN
    SET NOCOUNT ON;

    -- Result set 1: TotalCount
    SELECT COUNT(*) AS TotalCount
    FROM Asset_Master am
    LEFT JOIN Asset_Allocation aa
           ON aa.AssetId = am.AssetId AND aa.AllocationStatus = 'Active'
          AND aa.IsActive = 1 AND aa.IsDeleted = 0
    WHERE am.IsActive = 1 AND am.IsDeleted = 0
      AND (@AssetType IS NULL OR am.AssetType = @AssetType)
      AND (@Status IS NULL OR am.AssetStatus = @Status)
      AND (@Search IS NULL OR (
          am.AssetCode LIKE '%' + @Search + '%'
          OR am.AssetName LIKE '%' + @Search + '%'
          OR am.SerialNumber LIKE '%' + @Search + '%'
      ));

    -- Result set 2: Paged rows
    SELECT
        am.AssetId                                         AS Id,
        am.AssetCode                                       AS AssetTag,
        am.AssetName,
        am.AssetType,
        am.SerialNumber,
        am.AssetStatus                                     AS Status,
        CONCAT(ae.FirstName, ' ', ae.LastName)             AS AllocatedTo,
        aa.AllocatedDate,
        am.WarrantyEndDate                                 AS WarrantyExpiryDate
    FROM Asset_Master am
    LEFT JOIN Asset_Allocation aa
           ON aa.AssetId = am.AssetId AND aa.AllocationStatus = 'Active'
          AND aa.IsActive = 1 AND aa.IsDeleted = 0
    LEFT JOIN Employees ae
           ON ae.ID = aa.EmployeeId AND ae.IsActive = 1 AND ae.IsDeleted = 0
    WHERE am.IsActive = 1 AND am.IsDeleted = 0
      AND (@AssetType IS NULL OR am.AssetType = @AssetType)
      AND (@Status IS NULL OR am.AssetStatus = @Status)
      AND (@Search IS NULL OR (
          am.AssetCode LIKE '%' + @Search + '%'
          OR am.AssetName LIKE '%' + @Search + '%'
          OR am.SerialNumber LIKE '%' + @Search + '%'
      ))
    ORDER BY am.Created DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- =============================================================================
-- 10. sp_GetAssetDetail
--     Returns detailed information for a specific asset including allocation.
-- =============================================================================
CREATE OR ALTER PROCEDURE sp_GetAssetDetail
    @AssetID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        am.AssetId                                         AS Id,
        am.AssetCode                                       AS AssetTag,
        am.AssetName,
        am.AssetType,
        am.SerialNumber,
        am.Brand                                           AS Manufacturer,
        am.ModelNo                                         AS Model,
        am.AssetStatus                                     AS Status,
        am.PurchaseDate,
        am.PurchaseCost,
        am.WarrantyEndDate                                 AS WarrantyExpiryDate,
        am.Location,
        CONCAT(ae.FirstName, ' ', ae.LastName)             AS AllocatedTo,
        aa.AllocatedDate
    FROM Asset_Master am
    LEFT JOIN Asset_Allocation aa
           ON aa.AssetId = am.AssetId AND aa.AllocationStatus = 'Active'
          AND aa.IsActive = 1 AND aa.IsDeleted = 0
    LEFT JOIN Employees ae
           ON ae.ID = aa.EmployeeId AND ae.IsActive = 1 AND ae.IsDeleted = 0
    WHERE am.AssetId = @AssetID AND am.IsActive = 1 AND am.IsDeleted = 0;
END
GO

-- =============================================================================
-- 11. sp_GetAssetRequests
--     Returns paged asset request list with optional status filter.
--     Returns 2 result sets: TotalCount (set 1) and paged rows (set 2).
-- =============================================================================
CREATE OR ALTER PROCEDURE sp_GetAssetRequests
    @Status     VARCHAR(50) = NULL,
    @PageNumber INT         = 1,
    @PageSize   INT         = 15
AS
BEGIN
    SET NOCOUNT ON;

    -- Result set 1: TotalCount
    SELECT COUNT(*) AS TotalCount
    FROM Asset_Request ar
    WHERE ar.IsActive = 1 AND ar.IsDeleted = 0
      AND (@Status IS NULL OR ar.ManagerApprovalStatus = @Status);

    -- Result set 2: Paged rows
    SELECT
        ar.AssetRequestId                                  AS Id,
        CONCAT(e.FirstName, ' ', e.LastName)               AS EmployeeName,
        d.DepartmentName                                   AS Department,
        ar.AssetType,
        ar.RequestReason                                   AS Reason,
        ar.ManagerApprovalStatus                           AS Status,
        ar.Created                                         AS RequestedOn,
        CONCAT(rev.FirstName, ' ', rev.LastName)           AS ReviewedBy,
        ar.ApprovedDate                                    AS ReviewedOn
    FROM Asset_Request ar
    LEFT JOIN Employees e
           ON e.ID = ar.EmployeeId AND e.IsActive = 1 AND e.IsDeleted = 0
    LEFT JOIN Departments d
           ON d.ID = e.DepartmentID AND d.IsActive = 1 AND d.IsDeleted = 0
    LEFT JOIN Employees rev
           ON rev.ID = ar.ApprovedBy AND rev.IsActive = 1 AND rev.IsDeleted = 0
    WHERE ar.IsActive = 1 AND ar.IsDeleted = 0
      AND (@Status IS NULL OR ar.ManagerApprovalStatus = @Status)
    ORDER BY ar.Created DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- =============================================================================
-- 12. sp_UpdateAssetRequestStatus
--     Admin approves or rejects an asset request.
-- =============================================================================
CREATE OR ALTER PROCEDURE sp_UpdateAssetRequestStatus
    @RequestID    UNIQUEIDENTIFIER,
    @Status       VARCHAR(50),          -- 'Approved' | 'Rejected'
    @ReviewedByID UNIQUEIDENTIFIER,
    @Remarks      NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Asset_Request
    SET ManagerApprovalStatus = @Status,
        ApprovedBy    = @ReviewedByID,
        ApprovedDate  = GETUTCDATE(),
        Modified      = GETUTCDATE(),
        ModifiedByID  = @ReviewedByID
    WHERE AssetRequestId = @RequestID AND IsActive = 1 AND IsDeleted = 0;

    SELECT @@ROWCOUNT AS Result;
END
GO

-- =============================================================================
-- 13. sp_GetDepartmentList
--     Returns active departments for filter dropdowns.
-- =============================================================================
CREATE OR ALTER PROCEDURE sp_GetDepartmentList
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ID              AS Id,
        DepartmentName  AS [Name]
    FROM Departments
    WHERE IsActive = 1 AND IsDeleted = 0
    ORDER BY DepartmentName;
END
GO

-- =============================================================================
-- 14. sp_GetRoleList
--     Returns active roles for filter dropdowns.
-- =============================================================================
CREATE OR ALTER PROCEDURE sp_GetRoleList
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ID    AS Id,
        [Name]
    FROM [Role]
    WHERE IsActive = 1 AND IsDeleted = 0
    ORDER BY [Name];
END
GO

-- =============================================================================
-- 15. sp_GetAllEngineersAdmin
--     Returns all active IT Support Engineers for ticket reassignment dropdown.
-- =============================================================================
CREATE OR ALTER PROCEDURE sp_GetAllEngineersAdmin
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        e.ID,
        CONCAT(e.FirstName, ' ', e.LastName) AS FullName
    FROM Employees e
    INNER JOIN EmployeeRoles er ON er.EmployeeID = e.ID AND er.IsActive = 1 AND er.IsDeleted = 0
    INNER JOIN [Role] r ON r.ID = er.RoleID AND r.[Name] = 'IT Support Engineer' AND r.IsActive = 1 AND r.IsDeleted = 0
    WHERE e.IsActive = 1 AND e.IsDeleted = 0
    ORDER BY e.FirstName, e.LastName;
END
GO
