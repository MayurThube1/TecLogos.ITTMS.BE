using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using TecLogos.ITTMS.DAL.DBHelper;
using TecLogos.ITTMS.DAL.Interfaces;
using TecLogos.ITTMS.Models.DTOs;
using TecLogos.ITTMS.Models.DTOs.Dashboard;

namespace TecLogos.ITTMS.DAL.Repositories
{
    /// <summary>
    /// Dapper-based repository implementation for IT Support Engineer operations.
    /// Updated to match the user's controller and execute direct SQL.
    /// </summary>
    public class ITSupportEngineerRepository : IITSupportEngineerRepository
    {
        private readonly DBConnection _dbConnection;

        public ITSupportEngineerRepository(DBConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        /// <inheritdoc />
        public async Task EnsureTablesCreatedAsync()
        {
            using var connection = _dbConnection.GetConnection();
            const string createAppointmentsTableSql = @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Ticket_Appointments]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[Ticket_Appointments] (
                        [ID] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                        [TicketID] UNIQUEIDENTIFIER NOT NULL,
                        [AppointmentDate] DATETIME2 NOT NULL,
                        [DurationMinutes] INT NOT NULL DEFAULT 60,
                        [AppointmentType] VARCHAR(50) NOT NULL DEFAULT 'SiteVisit',
                        [Status] VARCHAR(50) NOT NULL DEFAULT 'Scheduled',
                        [Remarks] VARCHAR(500) NULL,
                        [ScheduledByID] UNIQUEIDENTIFIER NOT NULL,
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
                END";

            await connection.ExecuteAsync(createAppointmentsTableSql);
        }

        /// <inheritdoc />
        public async Task<PagedResultDTO<AssignedTicketDTO>> GetAssignedTicketsAsync(Guid engineerId, string? status, int pageNumber, int pageSize)
        {
            using var connection = _dbConnection.GetConnection();

            const string countSql = @"
                SELECT COUNT(*)
                FROM Ticket_Request t
                WHERE t.IsActive = 1 AND t.IsDeleted = 0
                  AND t.AssignedTo = @EngineerID
                  AND (@Status IS NULL OR t.Status = @Status)";

            const string querySql = @"
                SELECT
                    t.Id AS ID,
                    t.Number,
                    t.Subject,
                    t.Category,
                    t.SubCategory,
                    t.Priority,
                    t.Status,
                    t.AssetType,
                    t.Created,
                    sla.ResolutionDueDate,
                    ISNULL(sla.IsResolutionBreached, 0) AS IsSLABreached,
                    t.EmployeeName,
                    t.EmailId
                FROM Ticket_Request t
                LEFT JOIN Ticket_SLA_Tracking sla ON sla.TicketId = t.Id AND sla.IsActive = 1 AND sla.IsDeleted = 0
                WHERE t.IsActive = 1 AND t.IsDeleted = 0
                  AND t.AssignedTo = @EngineerID
                  AND (@Status IS NULL OR t.Status = @Status)
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
                FETCH NEXT @PageSize ROWS ONLY";

            var parameters = new { EngineerID = engineerId, Status = status, PageNumber = pageNumber, PageSize = pageSize };

            var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);
            var items = (await connection.QueryAsync<AssignedTicketDTO>(querySql, parameters)).ToList();

            return new PagedResultDTO<AssignedTicketDTO>
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = items
            };
        }

        /// <inheritdoc />
        public async Task<bool> UpdateTicketStatusAsync(Guid ticketId, string status, Guid updatedById)
        {
            using var connection = _dbConnection.GetConnection();
            if (connection.State != ConnectionState.Open) await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                // 1. Update status
                const string updateTicketSql = @"
                    UPDATE Ticket_Request
                    SET Status = @Status,
                        Modified = GETUTCDATE(),
                        ModifiedByID = @UpdatedByID
                    WHERE Id = @TicketID AND IsActive = 1 AND IsDeleted = 0";

                var rows = await connection.ExecuteAsync(updateTicketSql, new { Status = status, UpdatedByID = updatedById, TicketID = ticketId }, transaction);
                if (rows == 0)
                {
                    transaction.Rollback();
                    return false;
                }

                // 2. SLA tracking updates
                if (status == "In Progress")
                {
                    const string updateSlaFirstResponseSql = @"
                        UPDATE Ticket_SLA_Tracking
                        SET FirstResponseDate = GETUTCDATE(),
                            ResponseStatus = 'Responded'
                        WHERE TicketId = @TicketID 
                          AND FirstResponseDate IS NULL 
                          AND IsActive = 1 AND IsDeleted = 0";
                    await connection.ExecuteAsync(updateSlaFirstResponseSql, new { TicketID = ticketId }, transaction);
                }
                else if (status == "Resolved" || status == "Closed")
                {
                    const string updateSlaResolvedSql = @"
                        UPDATE Ticket_SLA_Tracking
                        SET ResolvedDate = GETUTCDATE(),
                            ResolutionStatus = @Status
                        WHERE TicketId = @TicketID 
                          AND IsActive = 1 AND IsDeleted = 0";
                    await connection.ExecuteAsync(updateSlaResolvedSql, new { TicketID = ticketId, Status = status }, transaction);
                }


                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> AddWorkNoteAsync(Guid ticketId, string noteText, Guid commentedById)
        {
            using var connection = _dbConnection.GetConnection();

            const string insertCommentSql = @"
                INSERT INTO Ticket_Comments (
                    ID, TicketID, CommentType, CommentText, CommentedByID,
                    Version, IsActive, IsDeleted, Created, CreatedByID
                )
                VALUES (
                    NEWID(), @TicketID, 'WorkNote', @CommentText, @CommentedByID,
                    1, 1, 0, GETUTCDATE(), @CommentedByID
                )";

            var rows = await connection.ExecuteAsync(insertCommentSql, new { TicketID = ticketId, CommentText = noteText, CommentedByID = commentedById });
            return rows > 0;
        }

        /// <inheritdoc />
        public async Task<bool> AddResolutionAsync(Guid ticketId, string resolutionText, Guid commentedById)
        {
            using var connection = _dbConnection.GetConnection();
            if (connection.State != ConnectionState.Open) await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                const string insertCommentSql = @"
                    INSERT INTO Ticket_Comments (
                        ID, TicketID, CommentType, CommentText, CommentedByID,
                        Version, IsActive, IsDeleted, Created, CreatedByID
                    )
                    VALUES (
                        NEWID(), @TicketID, 'Resolution', @CommentText, @CommentedByID,
                        1, 1, 0, GETUTCDATE(), @CommentedByID
                    )";

                var rows = await connection.ExecuteAsync(insertCommentSql, new { TicketID = ticketId, CommentText = resolutionText, CommentedByID = commentedById }, transaction);
                if (rows == 0)
                {
                    transaction.Rollback();
                    return false;
                }

                const string updateTicketSql = @"
                    UPDATE Ticket_Request
                    SET Status = 'Resolved',
                        Modified = GETUTCDATE(),
                        ModifiedByID = @UpdatedByID
                    WHERE Id = @TicketID AND IsActive = 1 AND IsDeleted = 0";
                await connection.ExecuteAsync(updateTicketSql, new { TicketID = ticketId, UpdatedByID = commentedById }, transaction);

                const string updateSlaResolvedSql = @"
                    UPDATE Ticket_SLA_Tracking
                    SET ResolvedDate = GETUTCDATE(),
                        ResolutionStatus = 'Resolved'
                    WHERE TicketId = @TicketID 
                      AND IsActive = 1 AND IsDeleted = 0";
                await connection.ExecuteAsync(updateSlaResolvedSql, new { TicketID = ticketId }, transaction);

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> EscalateTicketAsync(Guid ticketId, string reason, Guid escalatedById)
        {
            using var connection = _dbConnection.GetConnection();
            if (connection.State != ConnectionState.Open) await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                const string updateTicketSql = @"
                    UPDATE Ticket_Request
                    SET Priority = 'Critical',
                        Modified = GETUTCDATE(),
                        ModifiedByID = @EscalatedByID
                    WHERE Id = @TicketID AND IsActive = 1 AND IsDeleted = 0";

                var rows = await connection.ExecuteAsync(updateTicketSql, new { TicketID = ticketId, EscalatedByID = escalatedById }, transaction);
                if (rows == 0)
                {
                    transaction.Rollback();
                    return false;
                }

                const string insertCommentSql = @"
                    INSERT INTO Ticket_Comments (
                        ID, TicketID, CommentType, CommentText, CommentedByID,
                        Version, IsActive, IsDeleted, Created, CreatedByID
                    )
                    VALUES (
                        NEWID(), @TicketID, 'CustomerVisible', @Remarks, @CommentedByID,
                        1, 1, 0, GETUTCDATE(), @CommentedByID
                    )";

                var text = $"Ticket has been escalated to Critical priority. Reason: {reason}";
                await connection.ExecuteAsync(insertCommentSql, new { TicketID = ticketId, Remarks = text, CommentedByID = escalatedById }, transaction);

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> CloseTicketAsync(Guid ticketId, Guid closedById)
        {
            using var connection = _dbConnection.GetConnection();
            if (connection.State != ConnectionState.Open) await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                const string updateTicketSql = @"
                    UPDATE Ticket_Request
                    SET Status = 'Closed',
                        Modified = GETUTCDATE(),
                        ModifiedByID = @ClosedByID
                    WHERE Id = @TicketID AND IsActive = 1 AND IsDeleted = 0";

                var rows = await connection.ExecuteAsync(updateTicketSql, new { TicketID = ticketId, ClosedByID = closedById }, transaction);
                if (rows == 0)
                {
                    transaction.Rollback();
                    return false;
                }

                const string updateSlaResolvedSql = @"
                    UPDATE Ticket_SLA_Tracking
                    SET ResolvedDate = GETUTCDATE(),
                        ResolutionStatus = 'Closed'
                    WHERE TicketId = @TicketID 
                      AND IsActive = 1 AND IsDeleted = 0";
                await connection.ExecuteAsync(updateSlaResolvedSql, new { TicketID = ticketId }, transaction);

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> ScheduleAppointmentAsync(Guid ticketId, DateTime appointmentTime, string description, Guid engineerId)
        {
            using var connection = _dbConnection.GetConnection();

            const string insertSql = @"
                INSERT INTO Ticket_Appointments (
                    ID, TicketID, AppointmentDate, DurationMinutes, AppointmentType, Status, Remarks, ScheduledByID,
                    Version, IsActive, IsDeleted, Created, CreatedByID
                )
                VALUES (
                    @ID, @TicketID, @AppointmentDate, 60, 'SiteVisit', 'Scheduled', @Remarks, @ScheduledByID,
                    1, 1, 0, GETUTCDATE(), @ScheduledByID
                )";

            var parameters = new
            {
                ID = Guid.NewGuid(),
                TicketID = ticketId,
                AppointmentDate = appointmentTime,
                Remarks = description,
                ScheduledByID = engineerId
            };

            var rows = await connection.ExecuteAsync(insertSql, parameters);
            return rows > 0;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AppointmentDetailsDTO>> GetTicketAppointmentsAsync(Guid ticketId)
        {
            using var connection = _dbConnection.GetConnection();

            const string querySql = @"
                SELECT
                    a.ID,
                    a.TicketID,
                    t.Number AS TicketNumber,
                    a.AppointmentDate,
                    a.DurationMinutes,
                    a.AppointmentType,
                    a.Status,
                    a.Remarks,
                    a.ScheduledByID,
                    CONCAT(e.FirstName, ' ', e.LastName) AS ScheduledByName,
                    a.Created
                FROM Ticket_Appointments a
                INNER JOIN Ticket_Request t ON t.Id = a.TicketID
                INNER JOIN Employees e ON e.ID = a.ScheduledByID
                WHERE a.TicketID = @TicketID AND a.IsActive = 1 AND a.IsDeleted = 0
                ORDER BY a.AppointmentDate ASC";

            return await connection.QueryAsync<AppointmentDetailsDTO>(querySql, new { TicketID = ticketId });
        }

        /// <inheritdoc />
        public async Task<bool> UpdateAppointmentStatusAsync(Guid appointmentId, string status, Guid modifiedById)
        {
            using var connection = _dbConnection.GetConnection();

            const string updateSql = @"
                UPDATE Ticket_Appointments
                SET Status = @Status,
                    Modified = GETUTCDATE(),
                    ModifiedByID = @ModifiedByID
                WHERE ID = @AppointmentID AND IsActive = 1 AND IsDeleted = 0";

            var rows = await connection.ExecuteAsync(updateSql, new { Status = status, ModifiedByID = modifiedById, AppointmentID = appointmentId });
            return rows > 0;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AssetLookupDTO>> GetAvailableAssetsAsync()
        {
            using var connection = _dbConnection.GetConnection();

            const string querySql = @"
                SELECT
                    AssetId,
                    AssetCode,
                    AssetName,
                    AssetType
                FROM Asset_Master
                WHERE AssetStatus = 'Available' AND IsActive = 1 AND IsDeleted = 0
                ORDER BY AssetName";

            return await connection.QueryAsync<AssetLookupDTO>(querySql);
        }

        /// <inheritdoc />
        public async Task<bool> AllocateAssetAsync(Guid assetId, Guid employeeId, string? remarks, Guid allocatedById)
        {
            using var connection = _dbConnection.GetConnection();
            if (connection.State != ConnectionState.Open) await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                const string insertAllocationSql = @"
                    INSERT INTO Asset_Allocation (
                        Id, AssetId, EmployeeId, AllocatedDate, AllocationStatus, Remarks,
                        Version, IsActive, IsDeleted, Created, CreatedByID
                    )
                    VALUES (
                        NEWID(), @AssetID, @EmployeeID, GETUTCDATE(), 'Active', @Remarks,
                        1, 1, 0, GETUTCDATE(), @AllocatedByID
                    )";

                var rows = await connection.ExecuteAsync(insertAllocationSql, new { AssetID = assetId, EmployeeID = employeeId, Remarks = remarks, AllocatedByID = allocatedById }, transaction);
                if (rows == 0)
                {
                    transaction.Rollback();
                    return false;
                }

                const string updateAssetSql = @"
                    UPDATE Asset_Master
                    SET AssetStatus = 'Assigned',
                        AssignedToEmployeeId = @EmployeeID,
                        Modified = GETUTCDATE(),
                        ModifiedByID = @AllocatedByID
                    WHERE AssetId = @AssetID AND IsActive = 1 AND IsDeleted = 0";
                await connection.ExecuteAsync(updateAssetSql, new { EmployeeID = employeeId, AllocatedByID = allocatedById, AssetID = assetId }, transaction);

                const string insertHistorySql = @"
                    INSERT INTO Asset_History (
                        HistoryId, AssetId, ActionType, OldStatus, NewStatus, ActionBy, ActionDate, Remarks,
                        Version, IsActive, IsDeleted, Created, CreatedByID
                    )
                    VALUES (
                        NEWID(), @AssetID, 'Allocation', 'Available', 'Assigned', @AllocatedByID, GETUTCDATE(), @Remarks,
                        1, 1, 0, GETUTCDATE(), @AllocatedByID
                    )";
                await connection.ExecuteAsync(insertHistorySql, new { AssetID = assetId, AllocatedByID = allocatedById, Remarks = remarks }, transaction);

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> ReturnAssetAsync(Guid assetId, string? remarks, Guid returnedById)
        {
            using var connection = _dbConnection.GetConnection();
            if (connection.State != ConnectionState.Open) await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                const string updateAllocationSql = @"
                    UPDATE Asset_Allocation
                    SET ReturnedDate = GETUTCDATE(),
                        AllocationStatus = 'Returned',
                        Remarks = CASE WHEN Remarks IS NULL THEN @Remarks ELSE CONCAT(Remarks, ' | Returned: ', @Remarks) END,
                        Modified = GETUTCDATE(),
                        ModifiedByID = @ReturnedByID
                    WHERE AssetId = @AssetID AND AllocationStatus = 'Active' AND IsActive = 1 AND IsDeleted = 0";

                var rows = await connection.ExecuteAsync(updateAllocationSql, new { Remarks = remarks, ReturnedByID = returnedById, AssetID = assetId }, transaction);
                if (rows == 0)
                {
                    transaction.Rollback();
                    return false;
                }

                const string updateAssetSql = @"
                    UPDATE Asset_Master
                    SET AssetStatus = 'Available',
                        AssignedToEmployeeId = NULL,
                        Modified = GETUTCDATE(),
                        ModifiedByID = @ReturnedByID
                    WHERE AssetId = @AssetID AND IsActive = 1 AND IsDeleted = 0";
                await connection.ExecuteAsync(updateAssetSql, new { ReturnedByID = returnedById, AssetID = assetId }, transaction);

                const string insertHistorySql = @"
                    INSERT INTO Asset_History (
                        HistoryId, AssetId, ActionType, OldStatus, NewStatus, ActionBy, ActionDate, Remarks,
                        Version, IsActive, IsDeleted, Created, CreatedByID
                    )
                    VALUES (
                        NEWID(), @AssetID, 'Return', 'Assigned', 'Available', @ReturnedByID, GETUTCDATE(), @Remarks,
                        1, 1, 0, GETUTCDATE(), @ReturnedByID
                    )";
                await connection.ExecuteAsync(insertHistorySql, new { AssetID = assetId, ReturnedByID = returnedById, Remarks = remarks }, transaction);

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<SlaComplianceDTO> GetSlaComplianceAsync(Guid engineerId)
        {
            using var connection = _dbConnection.GetConnection();

            const string querySql = @"
                SELECT
                    COUNT(t.Id) AS TotalTicketsAssigned,
                    SUM(CASE WHEN t.Status IN ('Resolved', 'Closed') THEN 1 ELSE 0 END) AS ResolvedTicketsCount,
                    SUM(CASE WHEN sla.IsResolutionBreached = 1 OR sla.IsResponseBreached = 1 THEN 1 ELSE 0 END) AS BreachedTicketsCount
                FROM Ticket_Request t
                LEFT JOIN Ticket_SLA_Tracking sla ON sla.TicketId = t.Id AND sla.IsActive = 1 AND sla.IsDeleted = 0
                WHERE t.IsActive = 1 AND t.IsDeleted = 0
                  AND t.AssignedTo = @EngineerID";

            var compliance = await connection.QueryFirstOrDefaultAsync<SlaComplianceDTO>(querySql, new { EngineerID = engineerId });

            if (compliance == null)
            {
                return new SlaComplianceDTO
                {
                    TotalTicketsAssigned = 0,
                    ResolvedTicketsCount = 0,
                    BreachedTicketsCount = 0,
                    ComplianceRatePercentage = 100.0
                };
            }

            compliance.ComplianceRatePercentage = compliance.TotalTicketsAssigned > 0
                ? Math.Round(((double)(compliance.TotalTicketsAssigned - compliance.BreachedTicketsCount) / compliance.TotalTicketsAssigned) * 100.0, 2)
                : 100.0;

            return compliance;
        }
    }
}
