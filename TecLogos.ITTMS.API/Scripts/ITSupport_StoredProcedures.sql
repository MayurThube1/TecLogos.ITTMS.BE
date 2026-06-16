-- =============================================================================
-- IT Support Engineer SQL Scripts
-- Database: TeclogosITTMS
-- Run this script to manually create the Ticket_Appointments table.
-- (Note: The repository also checks and auto-creates this table at startup).
-- =============================================================================

USE TeclogosITTMS;
GO

-- =============================================================================
-- 1. Create Ticket_Appointments Table
--    Stores scheduled appointments and site visits for tickets.
-- =============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Ticket_Appointments]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Ticket_Appointments] (
        [ID] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        [TicketID] UNIQUEIDENTIFIER NOT NULL,
        [AppointmentDate] DATETIME2 NOT NULL,
        [DurationMinutes] INT NOT NULL DEFAULT 60,
        [AppointmentType] VARCHAR(50) NOT NULL, -- 'SiteVisit' | 'RemoteSupport'
        [Status] VARCHAR(50) NOT NULL DEFAULT 'Scheduled', -- 'Scheduled' | 'Completed' | 'Cancelled'
        [Remarks] VARCHAR(500) NULL,
        [ScheduledByID] UNIQUEIDENTIFIER NOT NULL,
        
        -- Audit Fields
        [Version] INT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [Created] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedByID] UNIQUEIDENTIFIER NOT NULL,
        [Modified] DATETIME2 NULL,
        [ModifiedByID] UNIQUEIDENTIFIER NULL,
        [Deleted] DATETIME2 NULL,
        [DeletedByID] UNIQUEIDENTIFIER NULL,

        CONSTRAINT [FK_Appointments_Ticket] FOREIGN KEY ([TicketID]) REFERENCES [Ticket_Request] ([Id]),
        CONSTRAINT [FK_Appointments_ScheduledBy] FOREIGN KEY ([ScheduledByID]) REFERENCES [Employees] ([ID])
    );

    PRINT 'Ticket_Appointments table created successfully.';
END
ELSE
BEGIN
    PRINT 'Ticket_Appointments table already exists.';
END
GO
