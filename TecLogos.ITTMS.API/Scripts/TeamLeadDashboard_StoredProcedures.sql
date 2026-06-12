USE TeclogosITTMS;
GO

-- =============================================================================
-- 1. sp_GetTeamLeadDashboardSummary
--    Returns top-level statistics cards for the Team Lead dashboard.
-- =============================================================================
CREATE OR ALTER PROCEDURE sp_GetTeamLeadDashboardSummary
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        COUNT(t.Id)                                                           AS TotalTickets,
        SUM(CASE WHEN t.Status IN ('Open','In Progress') THEN 1 ELSE 0 END)   AS OpenTickets,
        SUM(CASE WHEN t.Status = 'Resolved' AND CAST(t.Modified AS DATE) = CAST(GETUTCDATE() AS DATE) THEN 1 ELSE 0 END) AS ResolvedToday,
        SUM(CASE WHEN t.Status = 'Closed' THEN 1 ELSE 0 END)                  AS ClosedTickets,
        SUM(CASE WHEN sla.IsResolutionBreached = 1 THEN 1 ELSE 0 END)       AS SLABreachedCount,
        SUM(CASE WHEN t.Priority = 'Critical'
                 AND t.Status IN ('Open','In Progress') THEN 1 ELSE 0 END)  AS CriticalOpen
    FROM Ticket_Request t
    LEFT JOIN Ticket_SLA_Tracking sla
           ON sla.TicketId = t.Id AND sla.IsActive = 1 AND sla.IsDeleted = 0
    WHERE t.IsActive = 1 AND t.IsDeleted = 0;
END
GO

-- =============================================================================
-- 2. sp_GetAllTickets
--    Returns all tickets with filters and pagination for Team Lead view.
--    Returns 2 result sets: TotalCount (set 1) and paged rows (set 2).
-- =============================================================================
CREATE OR ALTER PROCEDURE sp_GetAllTickets
    @Status      VARCHAR(50) = NULL,
    @Priority    VARCHAR(50) = NULL,
    @AssignedTo  UNIQUEIDENTIFIER = NULL,
    @PageNumber  INT = 1,
    @PageSize    INT = 15
AS
BEGIN
    SET NOCOUNT ON;

    -- Result set 1: TotalCount
    SELECT COUNT(*) AS TotalCount
    FROM Ticket_Request t
    WHERE t.IsActive = 1 AND t.IsDeleted = 0
      AND (@Status IS NULL OR t.Status = @Status)
      AND (@Priority IS NULL OR t.Priority = @Priority)
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
        ISNULL(sla.IsResolutionBreached, 0) AS IsSLABreached,
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
-- 3. sp_AssignTicket
--    Assigns a support ticket to an IT Support Engineer.
-- =============================================================================
CREATE OR ALTER PROCEDURE sp_AssignTicket
    @TicketID       UNIQUEIDENTIFIER,
    @AssignedToID   UNIQUEIDENTIFIER,
    @AssignedByID   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    -- Update Ticket Request assignment and change Status to 'In Progress' if 'Open'
    UPDATE Ticket_Request
    SET AssignedTo = @AssignedToID,
        Status     = CASE WHEN Status = 'Open' THEN 'In Progress' ELSE Status END,
        Modified   = GETUTCDATE(),
        ModifiedByID = @AssignedByID
    WHERE Id = @TicketID
      AND IsActive = 1
      AND IsDeleted = 0;

    -- Log assignment in Ticket_Assignment_Log
    INSERT INTO Ticket_Assignment_Log (
        ID, TicketID, AssignedToID, AssignedByID, AssignedAt, 
        [Version], IsActive, IsDeleted, Created, CreatedByID
    )
    VALUES (
        NEWID(), @TicketID, @AssignedToID, @AssignedByID, GETUTCDATE(),
        1, 1, 0, GETUTCDATE(), @AssignedByID
    );

    -- Insert system comment notifying that the ticket has been assigned
    DECLARE @EngineerName NVARCHAR(200);
    SELECT @EngineerName = CONCAT(FirstName, ' ', LastName) 
    FROM Employees 
    WHERE ID = @AssignedToID;

    INSERT INTO Ticket_Comments (
        ID, TicketID, CommentType, CommentText, CommentedByID,
        [Version], IsActive, IsDeleted, Created, CreatedByID
    )
    VALUES (
        NEWID(), @TicketID, 'CustomerVisible', CONCAT('Ticket has been assigned to support engineer: ', ISNULL(@EngineerName, 'Unassigned')), @AssignedByID,
        1, 1, 0, GETUTCDATE(), @AssignedByID
    );
END
GO

-- =============================================================================
-- 4. sp_EscalateTicket
--    Escalates the ticket's priority to Critical.
-- =============================================================================
CREATE OR ALTER PROCEDURE sp_EscalateTicket
    @TicketID       UNIQUEIDENTIFIER,
    @EscalatedByID  UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    -- Update Priority to Critical in Ticket_Request
    UPDATE Ticket_Request
    SET Priority = 'Critical',
        Modified = GETUTCDATE(),
        ModifiedByID = @EscalatedByID
    WHERE Id = @TicketID
      AND IsActive = 1
      AND IsDeleted = 0;

    -- Log system comment about escalation
    INSERT INTO Ticket_Comments (
        ID, TicketID, CommentType, CommentText, CommentedByID,
        [Version], IsActive, IsDeleted, Created, CreatedByID
    )
    VALUES (
        NEWID(), @TicketID, 'CustomerVisible', 'Ticket has been escalated to Critical priority.', @EscalatedByID,
        1, 1, 0, GETUTCDATE(), @EscalatedByID
    );
END
GO

-- =============================================================================
-- 5. sp_GetEngineerWorkload
--    Returns support engineer ticket workload numbers.
-- =============================================================================
CREATE OR ALTER PROCEDURE sp_GetEngineerWorkload
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        e.ID AS EngineerID,
        CONCAT(e.FirstName, ' ', e.LastName) AS EngineerName,
        SUM(CASE WHEN t.Status = 'Open' THEN 1 ELSE 0 END) AS OpenCount,
        SUM(CASE WHEN t.Status = 'In Progress' THEN 1 ELSE 0 END) AS InProgressCount,
        SUM(CASE WHEN t.Status = 'Resolved' THEN 1 ELSE 0 END) AS ResolvedCount,
        SUM(CASE WHEN t.Status = 'Closed' THEN 1 ELSE 0 END) AS ClosedCount,
        COUNT(t.Id) AS TotalAssignedCount,
        SUM(CASE WHEN sla.IsResolutionBreached = 1 THEN 1 ELSE 0 END) AS SlaBreachedCount
    FROM Employees e
    INNER JOIN EmployeeRoles er ON er.EmployeeID = e.ID AND er.IsActive = 1 AND er.IsDeleted = 0
    INNER JOIN [Role] r ON r.ID = er.RoleID AND r.[Name] = 'IT Support Engineer' AND r.IsActive = 1 AND r.IsDeleted = 0
    LEFT JOIN Ticket_Request t ON t.AssignedTo = e.ID AND t.IsActive = 1 AND t.IsDeleted = 0
    LEFT JOIN Ticket_SLA_Tracking sla ON sla.TicketId = t.Id AND sla.IsActive = 1 AND sla.IsDeleted = 0
    WHERE e.IsActive = 1 AND e.IsDeleted = 0
    GROUP BY e.ID, e.FirstName, e.LastName;
END
GO

-- =============================================================================
-- 6. sp_GetSlaOverview
--    Returns active tickets with SLA tracking details.
-- =============================================================================
CREATE OR ALTER PROCEDURE sp_GetSlaOverview
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        t.Id AS TicketId,
        t.Number AS TicketNumber,
        t.Subject,
        t.Priority,
        t.Status,
        CONCAT(e.FirstName, ' ', e.LastName) AS AssignedToName,
        sla.ResponseDueDate,
        sla.ResolutionDueDate,
        sla.IsResponseBreached,
        sla.IsResolutionBreached,
        CASE 
            WHEN t.Status IN ('Resolved', 'Closed') THEN 0
            ELSE DATEDIFF(MINUTE, GETUTCDATE(), sla.ResolutionDueDate)
        END AS MinutesRemaining
    FROM Ticket_Request t
    LEFT JOIN Employees e ON e.ID = t.AssignedTo AND e.IsActive = 1 AND e.IsDeleted = 0
    LEFT JOIN Ticket_SLA_Tracking sla ON sla.TicketId = t.Id AND sla.IsActive = 1 AND sla.IsDeleted = 0
    WHERE t.IsActive = 1 AND t.IsDeleted = 0
      AND (sla.IsResolutionBreached = 1 OR sla.IsResponseBreached = 1 OR (t.Status NOT IN ('Resolved', 'Closed') AND sla.ResolutionDueDate IS NOT NULL))
    ORDER BY sla.IsResolutionBreached DESC, sla.ResolutionDueDate ASC;
END
GO
