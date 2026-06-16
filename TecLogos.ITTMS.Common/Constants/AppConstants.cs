using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace TecLogos.ITTMS.Common.Constants
{
    public static class AppConstants
    {
        public static class Roles
        {
            public static string TeamLead { get; set; } = "Team Lead";
            public static string Administrator { get; set; } = "Administrator";
        }

        public static class CommentTypes
        {
            public static string WorkNote { get; set; } = "WorkNote";
            public static string Resolution { get; set; } = "Resolution";
            public static string Escalation { get; set; } = "Escalation";
            public static string Diagnosis { get; set; } = "Diagnosis";
        }

        public static class SlaStatus
        {
            public static string Resolved { get; set; } = "Resolved";
            public static string Closed { get; set; } = "Closed";
            public static string Escalated { get; set; } = "Escalated";
        }

        public static class AuditDefaults
        {
            public static int Version { get; set; } = 1;
            public static bool IsActive { get; set; } = true;
            public static bool IsDeleted { get; set; } = false;
        }

        public static void LoadFromConfiguration(IConfiguration configuration)
        {
            var section = configuration.GetSection("AppConstants");
            if (!section.Exists()) return;

            var rolesSection = section.GetSection("Roles");
            if (rolesSection.Exists())
            {
                Roles.TeamLead = rolesSection["TeamLead"] ?? Roles.TeamLead;
                Roles.Administrator = rolesSection["Administrator"] ?? Roles.Administrator;
            }

            var commentTypesSection = section.GetSection("CommentTypes");
            if (commentTypesSection.Exists())
            {
                CommentTypes.WorkNote = commentTypesSection["WorkNote"] ?? CommentTypes.WorkNote;
                CommentTypes.Resolution = commentTypesSection["Resolution"] ?? CommentTypes.Resolution;
                CommentTypes.Escalation = commentTypesSection["Escalation"] ?? CommentTypes.Escalation;
                CommentTypes.Diagnosis = commentTypesSection["Diagnosis"] ?? CommentTypes.Diagnosis;
            }

            var slaStatusSection = section.GetSection("SlaStatus");
            if (slaStatusSection.Exists())
            {
                SlaStatus.Resolved = slaStatusSection["Resolved"] ?? SlaStatus.Resolved;
                SlaStatus.Closed = slaStatusSection["Closed"] ?? SlaStatus.Closed;
                SlaStatus.Escalated = slaStatusSection["Escalated"] ?? SlaStatus.Escalated;
            }

            var auditDefaultsSection = section.GetSection("AuditDefaults");
            if (auditDefaultsSection.Exists())
            {
                if (int.TryParse(auditDefaultsSection["Version"], out int version))
                {
                    AuditDefaults.Version = version;
                }
                if (bool.TryParse(auditDefaultsSection["IsActive"], out bool isActive))
                {
                    AuditDefaults.IsActive = isActive;
                }
                if (bool.TryParse(auditDefaultsSection["IsDeleted"], out bool isDeleted))
                {
                    AuditDefaults.IsDeleted = isDeleted;
                }
            }
        }
    }
}
