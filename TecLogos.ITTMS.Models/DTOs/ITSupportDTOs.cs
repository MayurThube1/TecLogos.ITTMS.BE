using System;
using System.ComponentModel.DataAnnotations;

namespace TecLogos.ITTMS.Models.DTOs
{
    public class UpdateTicketStatusRequestDTO
    {
        [Required(ErrorMessage = "TicketID is required.")]
        public Guid TicketId { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters.")]
        public string Status { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Remarks cannot exceed 1000 characters.")]
        public string? Remarks { get; set; }
    }

    public class AddWorkNoteRequestDTO
    {
        [Required(ErrorMessage = "TicketID is required.")]
        public Guid TicketId { get; set; }

        [Required(ErrorMessage = "NoteText is required.")]
        public string NoteText { get; set; } = string.Empty;
    }

    public class AddResolutionRequestDTO
    {
        [Required(ErrorMessage = "TicketID is required.")]
        public Guid TicketId { get; set; }

        [Required(ErrorMessage = "ResolutionText is required.")]
        public string ResolutionText { get; set; } = string.Empty;
    }

    public class EscalateTicketRequestDTO
    {
        [Required(ErrorMessage = "TicketID is required.")]
        public Guid TicketId { get; set; }

        [Required(ErrorMessage = "Reason is required.")]
        [StringLength(1000, ErrorMessage = "Reason cannot exceed 1000 characters.")]
        public string Reason { get; set; } = string.Empty;
    }

    public class CloseTicketRequestDTO
    {
        [Required(ErrorMessage = "TicketID is required.")]
        public Guid TicketId { get; set; }

        [StringLength(1000, ErrorMessage = "Remarks cannot exceed 1000 characters.")]
        public string? Remarks { get; set; }
    }

    public class ScheduleAppointmentRequestDTO
    {
        [Required(ErrorMessage = "TicketID is required.")]
        public Guid TicketId { get; set; }

        [Required(ErrorMessage = "AppointmentTime is required.")]
        public DateTime AppointmentTime { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string Description { get; set; } = string.Empty;
    }

    public class AllocateAssetRequestDTO
    {
        [Required(ErrorMessage = "AssetID is required.")]
        public Guid AssetId { get; set; }

        [Required(ErrorMessage = "EmployeeID is required.")]
        public Guid EmployeeId { get; set; }

        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters.")]
        public string? Remarks { get; set; }
    }

    public class ReturnAssetRequestDTO
    {
        [Required(ErrorMessage = "AssetID is required.")]
        public Guid AssetId { get; set; }

        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters.")]
        public string? Remarks { get; set; }
    }

    public class SlaComplianceDTO
    {
        public int TotalTicketsAssigned { get; set; }
        public int ResolvedTicketsCount { get; set; }
        public int BreachedTicketsCount { get; set; }
        public double ComplianceRatePercentage { get; set; }
    }

    public class AssignedTicketDTO
    {
        public Guid ID { get; set; }
        public string Number { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string SubCategory { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? AssetType { get; set; }
        public DateTime Created { get; set; }
        public DateTime? ResolutionDueDate { get; set; }
        public bool IsSLABreached { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmailId { get; set; }
    }

    /// <summary>
    /// Unified request DTO for executing write actions under a single endpoint.
    /// </summary>
    public class ITSupportActionDTO
    {
        [Required(ErrorMessage = "Action type is required.")]
        [RegularExpression("^(UpdateStatus|AddWorkNote|AddResolution|Escalate|Close|ScheduleAppointment|AllocateAsset|ReturnAsset)$", 
            ErrorMessage = "Action must be UpdateStatus, AddWorkNote, AddResolution, Escalate, Close, ScheduleAppointment, AllocateAsset, or ReturnAsset.")]
        public string Action { get; set; } = string.Empty;

        public UpdateTicketStatusRequestDTO? UpdateStatus { get; set; }
        public AddWorkNoteRequestDTO? AddWorkNote { get; set; }
        public AddResolutionRequestDTO? AddResolution { get; set; }
        public EscalateTicketRequestDTO? Escalate { get; set; }
        public CloseTicketRequestDTO? Close { get; set; }
        public ScheduleAppointmentRequestDTO? ScheduleAppointment { get; set; }
        public AllocateAssetRequestDTO? AllocateAsset { get; set; }
        public ReturnAssetRequestDTO? ReturnAsset { get; set; }
    }
}
