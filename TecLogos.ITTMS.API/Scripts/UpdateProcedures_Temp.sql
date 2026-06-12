USE TeclogosITTMS;
GO

-- =============================================================================
-- 1. sp_GetEmployeeTicketDetail (Updated Aliases)
-- =============================================================================
CREATE OR ALTER PROCEDURE sp_GetEmployeeTicketDetail
    @TicketID   UNIQUEIDENTIFIER,
    @EmployeeID UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    -- Guard: only return a ticket the employee raised
    IF NOT EXISTS (
        SELECT 1
        FROM   Ticket_Request
        WHERE  Id         = @TicketID
          AND  EmployeeId = @EmployeeID
          AND  IsActive   = 1
          AND  IsDeleted  = 0
    )
    BEGIN
        SELECT NULL AS Id;
        RETURN;
    END

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

        -- Feedback already submitted?
        CASE WHEN fb.ID IS NOT NULL THEN 1 ELSE 0 END AS FeedbackSubmitted,
        fb.Rating           AS FeedbackRating

    FROM Ticket_Request t
    LEFT JOIN Employees e
           ON e.ID = t.EmployeeId AND e.IsActive = 1 AND e.IsDeleted = 0
    LEFT JOIN Employees eng
           ON eng.ID = t.AssignedTo AND eng.IsActive = 1 AND eng.IsDeleted = 0
    LEFT JOIN Ticket_SLA_Tracking sla
           ON sla.TicketId = t.Id AND sla.IsActive = 1 AND sla.IsDeleted = 0
    LEFT JOIN Ticket_Feedback fb
           ON fb.TicketID = t.Id AND fb.SubmittedByID = @EmployeeID
          AND fb.IsActive = 1 AND fb.IsDeleted = 0
    WHERE t.Id = @TicketID;

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
-- 2. sp_GetTeamLeadTicketDetail (Updated Aliases)
-- =============================================================================
CREATE OR ALTER PROCEDURE sp_GetTeamLeadTicketDetail
    @TicketID   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    -- Guard: check if the ticket exists
    IF NOT EXISTS (
        SELECT 1
        FROM   Ticket_Request
        WHERE  Id         = @TicketID
          AND  IsActive   = 1
          AND  IsDeleted  = 0
    )
    BEGIN
        SELECT NULL AS Id;
        RETURN;
    END

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

        -- Feedback submitted by the ticket creator
        CASE WHEN fb.ID IS NOT NULL THEN 1 ELSE 0 END AS FeedbackSubmitted,
        fb.Rating           AS FeedbackRating

    FROM Ticket_Request t
    LEFT JOIN Employees e
           ON e.ID = t.EmployeeId AND e.IsActive = 1 AND e.IsDeleted = 0
    LEFT JOIN Employees eng
           ON eng.ID = t.AssignedTo AND eng.IsActive = 1 AND eng.IsDeleted = 0
    LEFT JOIN Ticket_SLA_Tracking sla
           ON sla.TicketId = t.Id AND sla.IsActive = 1 AND sla.IsDeleted = 0
    LEFT JOIN Ticket_Feedback fb
           ON fb.TicketID = t.Id AND fb.SubmittedByID = t.EmployeeId
          AND fb.IsActive = 1 AND fb.IsDeleted = 0
    WHERE t.Id = @TicketID;

    -- Result set 2: Comments (Team Lead / Admin can see all comments, including internal work notes)
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
-- 3. sp_GetAdminTicketDetail (Updated Aliases)
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
