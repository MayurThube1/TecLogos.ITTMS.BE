using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using TecLogos.ITTMS.Common.Constants;
using TecLogos.ITTMS.Common.Enums;
using TecLogos.ITTMS.DAL.DBHelper;
using TecLogos.ITTMS.DAL.Interfaces;
using TecLogos.ITTMS.Models.DTOs;
using TecLogos.ITTMS.Models.DTOs.Dashboard;

namespace TecLogos.ITTMS.DAL.Repositories
{
    public class ITSupportEngineerRepository : IITSupportEngineerRepository
    {
        private readonly DBConnection _dbConnection;

        public ITSupportEngineerRepository(DBConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<PagedResultDTO<AssignedTicketDTO>> GetAssignedTicketsAsync(Guid engineerId, string? status, int pageNumber, int pageSize)
        {
            using var connection = _dbConnection.GetConnection();
            int offset = (pageNumber - 1) * pageSize;

            var parameters = new
            {
                EngineerId = engineerId,
                Status = string.IsNullOrWhiteSpace(status) ? null : status,
                Offset = offset,
                PageSize = pageSize,
                IsActive = AppConstants.AuditDefaults.IsActive,
                IsDeleted = AppConstants.AuditDefaults.IsDeleted
            };

            string countQuery = @"
                SELECT COUNT(*) 
                FROM Ticket_Request t
                WHERE t.AssignedTo = @EngineerId
                  AND t.IsActive = @IsActive
                  AND t.IsDeleted = @IsDeleted
                  AND (@Status IS NULL OR t.Status = @Status)";

            string dataQuery = @"
                SELECT 
                    t.Id AS ID,
                    t.Number,
                    t.RequestType,
                    t.Subject,
                    t.Category,
                    t.SubCategory,
                    t.Priority,
                    t.Status,
                    t.AssetType,
                    t.Created,
                    sla.ResolutionDueDate,
                    COALESCE(sla.IsResolutionBreached, 0) AS IsSLABreached,
                    t.EmployeeName,
                    t.EmailId
                FROM Ticket_Request t
                LEFT JOIN Ticket_SLA_Tracking sla 
                       ON sla.TicketId = t.Id 
                      AND sla.IsActive = @IsActive 
                      AND sla.IsDeleted = @IsDeleted
                WHERE t.AssignedTo = @EngineerId
                  AND t.IsActive = @IsActive
                  AND t.IsDeleted = @IsDeleted
                  AND (@Status IS NULL OR t.Status = @Status)
                ORDER BY t.Created DESC
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

            int totalCount = await connection.ExecuteScalarAsync<int>(countQuery, parameters);
            var items = (await connection.QueryAsync<AssignedTicketDTO>(dataQuery, parameters)).ToList();

            return new PagedResultDTO<AssignedTicketDTO>
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = items
            };
        }

        public async Task<bool> UpdateTicketStatusAsync(Guid ticketId, string status, string? remarks, Guid engineerId)
        {
            using var connection = _dbConnection.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                string updateQuery = @"
                    UPDATE Ticket_Request
                    SET Status = @Status,
                        Modified = @UtcNow,
                        ModifiedByID = @EngineerId
                    WHERE Id = @TicketId
                      AND IsActive = @IsActive
                      AND IsDeleted = @IsDeleted";

                int affected = await connection.ExecuteAsync(updateQuery, new
                {
                    Status = status,
                    TicketId = ticketId,
                    EngineerId = engineerId,
                    UtcNow = DateTime.UtcNow,
                    IsActive = AppConstants.AuditDefaults.IsActive,
                    IsDeleted = AppConstants.AuditDefaults.IsDeleted
                }, transaction);

                if (affected == 0)
                {
                    transaction.Rollback();
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(remarks))
                {
                    string commentQuery = @"
                        INSERT INTO Ticket_Comments (ID, TicketID, CommentType, CommentText, CommentedByID, Version, IsActive, IsDeleted, Created, CreatedByID)
                        VALUES (@CommentId, @TicketId, @CommentType, @Remarks, @EngineerId, @Version, @IsActive, @IsDeleted, @UtcNow, @EngineerId)";

                    await connection.ExecuteAsync(commentQuery, new
                    {
                        CommentId = Guid.NewGuid(),
                        TicketId = ticketId,
                        CommentType = AppConstants.CommentTypes.WorkNote,
                        Remarks = remarks,
                        EngineerId = engineerId,
                        Version = AppConstants.AuditDefaults.Version,
                        IsActive = AppConstants.AuditDefaults.IsActive,
                        IsDeleted = AppConstants.AuditDefaults.IsDeleted,
                        UtcNow = DateTime.UtcNow
                    }, transaction);
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

        public async Task<bool> AddWorkNoteAsync(Guid ticketId, string noteText, Guid engineerId)
        {
            using var connection = _dbConnection.GetConnection();
            string commentQuery = @"
                INSERT INTO Ticket_Comments (ID, TicketID, CommentType, CommentText, CommentedByID, Version, IsActive, IsDeleted, Created, CreatedByID)
                VALUES (@CommentId, @TicketId, @CommentType, @NoteText, @EngineerId, @Version, @IsActive, @IsDeleted, @UtcNow, @EngineerId)";

            int affected = await connection.ExecuteAsync(commentQuery, new
            {
                CommentId = Guid.NewGuid(),
                TicketId = ticketId,
                CommentType = AppConstants.CommentTypes.WorkNote,
                NoteText = noteText,
                EngineerId = engineerId,
                Version = AppConstants.AuditDefaults.Version,
                IsActive = AppConstants.AuditDefaults.IsActive,
                IsDeleted = AppConstants.AuditDefaults.IsDeleted,
                UtcNow = DateTime.UtcNow
            });

            return affected > 0;
        }

        public async Task<bool> AddResolutionAsync(Guid ticketId, string resolutionText, Guid engineerId)
        {
            using var connection = _dbConnection.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Update Ticket Status to Resolved
                string updateTicketQuery = @"
                    UPDATE Ticket_Request
                    SET Status = @Status,
                        Modified = @UtcNow,
                        ModifiedByID = @EngineerId
                    WHERE Id = @TicketId
                      AND IsActive = @IsActive
                      AND IsDeleted = @IsDeleted";

                int affected = await connection.ExecuteAsync(updateTicketQuery, new
                {
                    Status = TicketStatus.Resolved,
                    TicketId = ticketId,
                    EngineerId = engineerId,
                    UtcNow = DateTime.UtcNow,
                    IsActive = AppConstants.AuditDefaults.IsActive,
                    IsDeleted = AppConstants.AuditDefaults.IsDeleted
                }, transaction);

                if (affected == 0)
                {
                    transaction.Rollback();
                    return false;
                }

                // Add Resolution Comment
                string commentQuery = @"
                    INSERT INTO Ticket_Comments (ID, TicketID, CommentType, CommentText, CommentedByID, Version, IsActive, IsDeleted, Created, CreatedByID)
                    VALUES (@CommentId, @TicketId, @CommentType, @ResolutionText, @EngineerId, @Version, @IsActive, @IsDeleted, @UtcNow, @EngineerId)";

                await connection.ExecuteAsync(commentQuery, new
                {
                    CommentId = Guid.NewGuid(),
                    TicketId = ticketId,
                    CommentType = AppConstants.CommentTypes.Resolution,
                    ResolutionText = resolutionText,
                    EngineerId = engineerId,
                    Version = AppConstants.AuditDefaults.Version,
                    IsActive = AppConstants.AuditDefaults.IsActive,
                    IsDeleted = AppConstants.AuditDefaults.IsDeleted,
                    UtcNow = DateTime.UtcNow
                }, transaction);

                // Update Ticket SLA Tracking
                string selectSlaQuery = @"
                    SELECT ResolutionDueDate 
                    FROM Ticket_SLA_Tracking 
                    WHERE TicketId = @TicketId 
                      AND IsActive = @IsActive 
                      AND IsDeleted = @IsDeleted";

                var resolutionDueDate = await connection.QueryFirstOrDefaultAsync<DateTime?>(selectSlaQuery, new
                {
                    TicketId = ticketId,
                    IsActive = AppConstants.AuditDefaults.IsActive,
                    IsDeleted = AppConstants.AuditDefaults.IsDeleted
                }, transaction);
                bool isBreached = resolutionDueDate.HasValue && DateTime.UtcNow > resolutionDueDate.Value;

                string updateSlaQuery = @"
                    UPDATE Ticket_SLA_Tracking
                    SET ResolvedDate = @UtcNow,
                        ResolutionStatus = @Status,
                        IsResolutionBreached = @IsBreached,
                        Modified = @UtcNow,
                        ModifiedByID = @EngineerId
                    WHERE TicketId = @TicketId
                      AND IsActive = @IsActive
                      AND IsDeleted = @IsDeleted";

                await connection.ExecuteAsync(updateSlaQuery, new
                {
                    IsBreached = isBreached ? 1 : 0,
                    TicketId = ticketId,
                    EngineerId = engineerId,
                    Status = AppConstants.SlaStatus.Resolved,
                    UtcNow = DateTime.UtcNow,
                    IsActive = AppConstants.AuditDefaults.IsActive,
                    IsDeleted = AppConstants.AuditDefaults.IsDeleted
                }, transaction);

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> EscalateTicketAsync(Guid ticketId, string reason, Guid engineerId)
        {
            using var connection = _dbConnection.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Find current engineer's manager
                string selectManagerQuery = @"
                    SELECT ManagerID 
                    FROM Employees 
                    WHERE ID = @EngineerId 
                      AND IsActive = @IsActive 
                      AND IsDeleted = @IsDeleted";

                Guid? escalateToId = await connection.QueryFirstOrDefaultAsync<Guid?>(selectManagerQuery, new
                {
                    EngineerId = engineerId,
                    IsActive = AppConstants.AuditDefaults.IsActive,
                    IsDeleted = AppConstants.AuditDefaults.IsDeleted
                }, transaction);

                // If no manager is assigned, find a Team Lead or Administrator
                if (!escalateToId.HasValue)
                {
                    string selectFallbackQuery = @"
                        SELECT TOP 1 e.ID
                        FROM Employees e
                        INNER JOIN EmployeeRoles er ON e.ID = er.EmployeeID
                        INNER JOIN [Role] r ON er.RoleID = r.ID
                        WHERE r.[Name] IN (@RoleTeamLead, @RoleAdmin)
                          AND e.IsActive = @IsActive
                          AND e.IsDeleted = @IsDeleted
                          AND er.IsActive = @IsActive
                          AND er.IsDeleted = @IsDeleted
                          AND r.IsActive = @IsActive
                          AND r.IsDeleted = @IsDeleted";

                    escalateToId = await connection.QueryFirstOrDefaultAsync<Guid?>(selectFallbackQuery, new
                    {
                        RoleTeamLead = AppConstants.Roles.TeamLead,
                        RoleAdmin = AppConstants.Roles.Administrator,
                        IsActive = AppConstants.AuditDefaults.IsActive,
                        IsDeleted = AppConstants.AuditDefaults.IsDeleted
                    }, transaction);
                }

                if (!escalateToId.HasValue)
                {
                    // No one to escalate to, rollback and return false
                    transaction.Rollback();
                    return false;
                }

                // Update Ticket assignment and status to Escalated
                string updateQuery = @"
                    UPDATE Ticket_Request
                    SET AssignedTo = @EscalateToId,
                        Status = @Status,
                        Modified = @UtcNow,
                        ModifiedByID = @EngineerId
                    WHERE Id = @TicketId
                      AND IsActive = @IsActive
                      AND IsDeleted = @IsDeleted";

                int affected = await connection.ExecuteAsync(updateQuery, new
                {
                    EscalateToId = escalateToId,
                    Status = TicketStatus.Escalated,
                    UtcNow = DateTime.UtcNow,
                    TicketId = ticketId,
                    EngineerId = engineerId,
                    IsActive = AppConstants.AuditDefaults.IsActive,
                    IsDeleted = AppConstants.AuditDefaults.IsDeleted
                }, transaction);

                if (affected == 0)
                {
                    transaction.Rollback();
                    return false;
                }

                // Add WorkNote for escalation
                string commentQuery = @"
                    INSERT INTO Ticket_Comments (ID, TicketID, CommentType, CommentText, CommentedByID, Version, IsActive, IsDeleted, Created, CreatedByID)
                    VALUES (@CommentId, @TicketId, @CommentType, @Reason, @EngineerId, @Version, @IsActive, @IsDeleted, @UtcNow, @EngineerId)";

                await connection.ExecuteAsync(commentQuery, new
                {
                    CommentId = Guid.NewGuid(),
                    TicketId = ticketId,
                    CommentType = AppConstants.CommentTypes.WorkNote,
                    Reason = $"[Escalated] Reason: {reason}",
                    EngineerId = engineerId,
                    Version = AppConstants.AuditDefaults.Version,
                    IsActive = AppConstants.AuditDefaults.IsActive,
                    IsDeleted = AppConstants.AuditDefaults.IsDeleted,
                    UtcNow = DateTime.UtcNow
                }, transaction);

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> CloseTicketAsync(Guid ticketId, Guid engineerId)
        {
            using var connection = _dbConnection.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                string updateTicketQuery = @"
                    UPDATE Ticket_Request
                    SET Status = @Status,
                        Modified = @UtcNow,
                        ModifiedByID = @EngineerId
                    WHERE Id = @TicketId
                      AND IsActive = @IsActive
                      AND IsDeleted = @IsDeleted";

                int affected = await connection.ExecuteAsync(updateTicketQuery, new
                {
                    Status = TicketStatus.Closed,
                    UtcNow = DateTime.UtcNow,
                    TicketId = ticketId,
                    EngineerId = engineerId,
                    IsActive = AppConstants.AuditDefaults.IsActive,
                    IsDeleted = AppConstants.AuditDefaults.IsDeleted
                }, transaction);

                if (affected == 0)
                {
                    transaction.Rollback();
                    return false;
                }

                string updateSlaQuery = @"
                    UPDATE Ticket_SLA_Tracking
                    SET ResolutionStatus = @Status,
                        Modified = @UtcNow,
                        ModifiedByID = @EngineerId
                    WHERE TicketId = @TicketId
                      AND IsActive = @IsActive
                      AND IsDeleted = @IsDeleted";

                await connection.ExecuteAsync(updateSlaQuery, new
                {
                    TicketId = ticketId,
                    EngineerId = engineerId,
                    Status = AppConstants.SlaStatus.Closed,
                    UtcNow = DateTime.UtcNow,
                    IsActive = AppConstants.AuditDefaults.IsActive,
                    IsDeleted = AppConstants.AuditDefaults.IsDeleted
                }, transaction);

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> ScheduleAppointmentAsync(Guid ticketId, DateTime appointmentTime, string description, Guid engineerId)
        {
            using var connection = _dbConnection.GetConnection();
            string commentQuery = @"
                INSERT INTO Ticket_Comments (ID, TicketID, CommentType, CommentText, CommentedByID, Version, IsActive, IsDeleted, Created, CreatedByID)
                VALUES (@CommentId, @TicketId, @CommentType, @NoteText, @EngineerId, @Version, @IsActive, @IsDeleted, @UtcNow, @EngineerId)";

            string formattedText = $"[Appointment Scheduled] Site visit/Appointment scheduled for: {appointmentTime:yyyy-MM-dd HH:mm UTC}. Details: {description}";

            int affected = await connection.ExecuteAsync(commentQuery, new
            {
                CommentId = Guid.NewGuid(),
                TicketId = ticketId,
                CommentType = AppConstants.CommentTypes.WorkNote,
                NoteText = formattedText,
                EngineerId = engineerId,
                Version = AppConstants.AuditDefaults.Version,
                IsActive = AppConstants.AuditDefaults.IsActive,
                IsDeleted = AppConstants.AuditDefaults.IsDeleted,
                UtcNow = DateTime.UtcNow
            });

            return affected > 0;
        }

        public async Task<bool> AllocateAssetAsync(Guid assetId, Guid employeeId, string? remarks, Guid engineerId)
        {
            using var connection = _dbConnection.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Update Asset Master status
                string updateAssetQuery = @"
                    UPDATE Asset_Master
                    SET AssignedToEmployeeId = @EmployeeId,
                        AssetStatus = @Status,
                        Modified = @UtcNow,
                        ModifiedByID = @EngineerId
                    WHERE AssetId = @AssetId
                      AND IsActive = @IsActive
                      AND IsDeleted = @IsDeleted";

                int affected = await connection.ExecuteAsync(updateAssetQuery, new
                {
                    EmployeeId = employeeId,
                    Status = AssetStatus.Allocated,
                    UtcNow = DateTime.UtcNow,
                    AssetId = assetId,
                    EngineerId = engineerId,
                    IsActive = AppConstants.AuditDefaults.IsActive,
                    IsDeleted = AppConstants.AuditDefaults.IsDeleted
                }, transaction);

                if (affected == 0)
                {
                    transaction.Rollback();
                    return false;
                }

                // Close any existing active allocation for this asset
                string closeAllocationsQuery = @"
                    UPDATE Asset_Allocation
                    SET ReturnedDate = @UtcNow,
                        AllocationStatus = @StatusReturned,
                        Modified = @UtcNow,
                        ModifiedByID = @EngineerId
                    WHERE AssetId = @AssetId
                      AND AllocationStatus = @StatusAllocated
                      AND ReturnedDate IS NULL
                      AND IsActive = @IsActive
                      AND IsDeleted = @IsDeleted";

                await connection.ExecuteAsync(closeAllocationsQuery, new
                {
                    AssetId = assetId,
                    EngineerId = engineerId,
                    StatusReturned = AssetStatus.Returned,
                    StatusAllocated = AssetStatus.Allocated,
                    UtcNow = DateTime.UtcNow,
                    IsActive = AppConstants.AuditDefaults.IsActive,
                    IsDeleted = AppConstants.AuditDefaults.IsDeleted
                }, transaction);

                // Insert new allocation record
                string insertAllocationQuery = @"
                    INSERT INTO Asset_Allocation (Id, AssetId, EmployeeId, AllocatedDate, AllocationStatus, Remarks, Version, IsActive, IsDeleted, Created, CreatedByID)
                    VALUES (@Id, @AssetId, @EmployeeId, @UtcNow, @Status, @Remarks, @Version, @IsActive, @IsDeleted, @UtcNow, @EngineerId)";

                await connection.ExecuteAsync(insertAllocationQuery, new
                {
                    Id = Guid.NewGuid(),
                    AssetId = assetId,
                    EmployeeId = employeeId,
                    Status = AssetStatus.Allocated,
                    Remarks = remarks,
                    EngineerId = engineerId,
                    Version = AppConstants.AuditDefaults.Version,
                    IsActive = AppConstants.AuditDefaults.IsActive,
                    IsDeleted = AppConstants.AuditDefaults.IsDeleted,
                    UtcNow = DateTime.UtcNow
                }, transaction);

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> ReturnAssetAsync(Guid assetId, string? remarks, Guid engineerId)
        {
            using var connection = _dbConnection.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Close active allocation
                string closeAllocationQuery = @"
                    UPDATE Asset_Allocation
                    SET ReturnedDate = @UtcNow,
                        AllocationStatus = @StatusReturned,
                        Remarks = COALESCE(@Remarks, Remarks),
                        Modified = @UtcNow,
                        ModifiedByID = @EngineerId
                    WHERE AssetId = @AssetId
                      AND AllocationStatus = @StatusAllocated
                      AND ReturnedDate IS NULL
                      AND IsActive = @IsActive
                      AND IsDeleted = @IsDeleted";

                int affectedAllocation = await connection.ExecuteAsync(closeAllocationQuery, new
                {
                    AssetId = assetId,
                    Remarks = remarks,
                    EngineerId = engineerId,
                    StatusReturned = AssetStatus.Returned,
                    StatusAllocated = AssetStatus.Allocated,
                    UtcNow = DateTime.UtcNow,
                    IsActive = AppConstants.AuditDefaults.IsActive,
                    IsDeleted = AppConstants.AuditDefaults.IsDeleted
                }, transaction);

                // Update Asset Master to make it Available
                string updateAssetQuery = @"
                    UPDATE Asset_Master
                    SET AssignedToEmployeeId = NULL,
                        AssetStatus = @Status,
                        Modified = @UtcNow,
                        ModifiedByID = @EngineerId
                    WHERE AssetId = @AssetId
                      AND IsActive = @IsActive
                      AND IsDeleted = @IsDeleted";

                int affectedAsset = await connection.ExecuteAsync(updateAssetQuery, new
                {
                    AssetId = assetId,
                    Status = AssetStatus.Available,
                    UtcNow = DateTime.UtcNow,
                    EngineerId = engineerId,
                    IsActive = AppConstants.AuditDefaults.IsActive,
                    IsDeleted = AppConstants.AuditDefaults.IsDeleted
                }, transaction);

                if (affectedAsset == 0)
                {
                    transaction.Rollback();
                    return false;
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

        public async Task<SlaComplianceDTO> GetSlaComplianceAsync(Guid engineerId)
        {
            using var connection = _dbConnection.GetConnection();

            string query = @"
                SELECT 
                    COUNT(*) AS TotalTicketsAssigned,
                    SUM(CASE WHEN t.Status = @StatusResolved THEN 1 ELSE 0 END) AS ResolvedTicketsCount,
                    SUM(CASE WHEN sla.IsResolutionBreached = 1 THEN 1 ELSE 0 END) AS BreachedTicketsCount
                FROM Ticket_Request t
                LEFT JOIN Ticket_SLA_Tracking sla 
                       ON sla.TicketId = t.Id 
                      AND sla.IsActive = @IsActive 
                      AND sla.IsDeleted = @IsDeleted
                WHERE t.AssignedTo = @EngineerId
                  AND t.IsActive = @IsActive
                  AND t.IsDeleted = @IsDeleted";

            var stats = await connection.QueryFirstOrDefaultAsync<SlaComplianceQueryResult>(query, new
            {
                EngineerId = engineerId,
                StatusResolved = TicketStatus.Resolved,
                IsActive = AppConstants.AuditDefaults.IsActive,
                IsDeleted = AppConstants.AuditDefaults.IsDeleted
            });

            int total = stats?.TotalTicketsAssigned ?? 0;
            int resolved = stats?.ResolvedTicketsCount ?? 0;
            int breached = stats?.BreachedTicketsCount ?? 0;
            double rate = total > 0 ? ((double)(total - breached) / total) * 100 : 100.0;

            return new SlaComplianceDTO
            {
                TotalTicketsAssigned = total,
                ResolvedTicketsCount = resolved,
                BreachedTicketsCount = breached,
                ComplianceRatePercentage = Math.Round(rate, 2)
            };
        }

        private class SlaComplianceQueryResult
        {
            public int TotalTicketsAssigned { get; set; }
            public int ResolvedTicketsCount { get; set; }
            public int BreachedTicketsCount { get; set; }
        }
    }
}
