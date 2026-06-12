using System;
using System.Collections.Generic;
using System.Text;

namespace TecLogos.ITTMS.Common.Constants
{
    public static class AppConstants
    {
        public static class Roles
        {
            public const string TeamLead = "Team Lead";
            public const string Administrator = "Administrator";
        }

        public static class CommentTypes
        {
            public const string WorkNote = "WorkNote";
            public const string Resolution = "Resolution";
        }

        public static class SlaStatus
        {
            public const string Resolved = "Resolved";
            public const string Closed = "Closed";
        }

        public static class AuditDefaults
        {
            public const int Version = 1;
            public const bool IsActive = true;
            public const bool IsDeleted = false;
        }
    }
}
